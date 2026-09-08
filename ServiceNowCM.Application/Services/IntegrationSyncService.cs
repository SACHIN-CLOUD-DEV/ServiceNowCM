using Microsoft.Extensions.Logging;
using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;
using System.Diagnostics;
using System.Text.Json;

namespace ServiceNowCM.Application.Services
{
    public class IntegrationSyncService
    {
        private readonly IIntegrationRepository _integrationRepository;
        private readonly IServiceNowConnectionRepository _serviceNowConnectionRepository;
        private readonly ICredentialStore _credentialStore;
        private readonly IServiceNowClient _serviceNowClient;
        private readonly IContentManagerConnectionRepository _contentManagerConnectionRepository;
        private readonly IContentManagerClient _contentManagerClient;
        private readonly ISyncedRecordRepository _syncedRecordRepository;
        private readonly ISyncJobRepository _syncJobRepository;
        private readonly ISyncFailureRepository _syncFailureRepository;
        private readonly ILogger<IntegrationSyncService> _logger;

        public IntegrationSyncService(
            IIntegrationRepository integrationRepository,
            IServiceNowConnectionRepository serviceNowConnectionRepository,
            IContentManagerConnectionRepository contentManagerConnectionRepository,
            IServiceNowClient serviceNowClient,
            IContentManagerClient contentManagerClient,
            ICredentialStore credentialStore,
            ISyncedRecordRepository syncedRecordRepository,
            ISyncJobRepository syncJobRepository,
            ISyncFailureRepository syncFailureRepository,
            ILogger<IntegrationSyncService> logger)
        {
            _integrationRepository = integrationRepository;
            _serviceNowConnectionRepository = serviceNowConnectionRepository;
            _contentManagerConnectionRepository = contentManagerConnectionRepository;
            _serviceNowClient = serviceNowClient;
            _contentManagerClient = contentManagerClient;
            _credentialStore = credentialStore;
            _syncedRecordRepository = syncedRecordRepository;
            _syncJobRepository = syncJobRepository;
            _syncFailureRepository = syncFailureRepository;
            _logger = logger;
        }

        // =============================================================
        // NEW SYNCHRONIZATION
        // =============================================================
        public Task<SyncResult> SyncAsync(
            long integrationId,
            CancellationToken cancellationToken = default)
        {
            return ExecuteSyncAsync(
                integrationId,
                resume: false,
                cancellationToken);
        }

        // =============================================================
        // RESUME PREVIOUS FAILED / INCOMPLETE SYNCHRONIZATION
        // =============================================================
        public Task<SyncResult> ResumeAsync(
            long integrationId,
            CancellationToken cancellationToken = default)
        {
            return ExecuteSyncAsync(
                integrationId,
                resume: true,
                cancellationToken);
        }

        // =============================================================
        // COMMON SYNCHRONIZATION ENGINE
        // =============================================================
        private async Task<SyncResult> ExecuteSyncAsync(
            long integrationId,
            bool resume,
            CancellationToken cancellationToken = default)
        {
            var totalStopwatch = Stopwatch.StartNew();

            // ---------------------------------------------------------
            // 1. Load Integration
            // ---------------------------------------------------------
            var integration =
                await _integrationRepository.GetByIdAsync(
                    integrationId);

            if (integration == null)
            {
                throw new KeyNotFoundException(
                    $"Integration with ID '{integrationId}' was not found.");
            }

            // ---------------------------------------------------------
            // 2. Validate integration
            // ---------------------------------------------------------
            if (!integration.IsActive)
            {
                throw new InvalidOperationException(
                    $"Integration '{integration.Name}' is not active.");
            }

            if (!integration.ContentManagerConnectionId.HasValue)
            {
                throw new InvalidOperationException(
                    "Content Manager connection is not configured for this integration.");
            }

            if (!integration.ContentManagerRecordTypeUri.HasValue)
            {
                throw new InvalidOperationException(
                    "Content Manager Record Type is not configured for this integration.");
            }

            if (integration.PageSize <= 0)
            {
                throw new InvalidOperationException(
                    "PageSize must be greater than zero.");
            }

            var batchSize =
                integration.ProcessingBatchSize;

            if (batchSize <= 0)
            {
                throw new InvalidOperationException(
                    "ProcessingBatchSize must be greater than zero.");
            }

            // ---------------------------------------------------------
            // 3. Determine NEW or RESUME execution
            // ---------------------------------------------------------
            var startingOffset = 0;
            var startingPage = 0;

            SyncJob syncJob;

            if (resume)
            {
                var previousJob =
                    await _syncJobRepository
                        .GetLatestIncompleteAsync(
                            integration.Id,
                            cancellationToken);

                if (previousJob == null)
                {
                    throw new InvalidOperationException(
                        $"No incomplete synchronization job was found " +
                        $"for integration '{integration.Name}'.");
                }

                startingOffset =
                    previousJob.LastCompletedOffset;

                startingPage =
                    previousJob.LastCompletedPage;

                if (startingOffset < 0 ||
                    startingPage < 0)
                {
                    throw new InvalidOperationException(
                        "The previous synchronization job contains an invalid checkpoint.");
                }

                previousJob.MarkInterrupted();

                await _syncJobRepository.UpdateAsync(
                    previousJob,
                    cancellationToken);

                syncJob =
                    new SyncJob(
                        integration.Id);

                syncJob.AdvanceCheckpoint(
                    completedOffset: startingOffset,
                    completedPage: startingPage);

                await _syncJobRepository.AddAsync(
                    syncJob,
                    cancellationToken);

                _logger.LogInformation(
                    "Sync job {SyncJobId} created to resume integration " +
                    "{IntegrationId} from previous job {PreviousJobId}. " +
                    "StartingOffset: {StartingOffset}, StartingPage: {StartingPage}",
                    syncJob.Id,
                    integration.Id,
                    previousJob.Id,
                    startingOffset,
                    startingPage);
            }
            else
            {
                syncJob =
                    new SyncJob(
                        integration.Id);

                await _syncJobRepository.AddAsync(
                    syncJob,
                    cancellationToken);

                _logger.LogInformation(
                    "Sync job {SyncJobId} started as a NEW synchronization " +
                    "for integration {IntegrationId} - {IntegrationName}",
                    syncJob.Id,
                    integration.Id,
                    integration.Name);
            }

            try
            {
                // -----------------------------------------------------
                // 4. Load ServiceNow connection
                // -----------------------------------------------------
                var serviceNowConnection =
                    await _serviceNowConnectionRepository
                        .GetByIdAsync(
                            integration.ServiceNowConnectionId);

                if (serviceNowConnection == null)
                {
                    throw new KeyNotFoundException(
                        $"ServiceNow connection with ID " +
                        $"'{integration.ServiceNowConnectionId}' was not found.");
                }

                if (string.IsNullOrWhiteSpace(
                        serviceNowConnection.CredentialReference))
                {
                    throw new InvalidOperationException(
                        "The ServiceNow connection does not have a credential reference.");
                }

                var serviceNowSecret =
                    await _credentialStore.GetAsync(
                        serviceNowConnection.CredentialReference);

                if (string.IsNullOrWhiteSpace(
                        serviceNowSecret))
                {
                    throw new InvalidOperationException(
                        "The configured ServiceNow credential could not be retrieved.");
                }

                // -----------------------------------------------------
                // 5. Load Content Manager connection
                // -----------------------------------------------------
                var cmConnection =
                    await _contentManagerConnectionRepository
                        .GetByIdAsync(
                            integration.ContentManagerConnectionId.Value);

                if (cmConnection == null)
                {
                    throw new KeyNotFoundException(
                        $"Content Manager connection with ID " +
                        $"'{integration.ContentManagerConnectionId.Value}' was not found.");
                }

                // -----------------------------------------------------
                // 6. Load CM credential if ExplicitCredentials
                // -----------------------------------------------------
                string? cmPassword = null;

                if (cmConnection.AuthenticationType ==
                    ContentManagerAuthenticationType.ExplicitCredentials)
                {
                    if (string.IsNullOrWhiteSpace(
                            cmConnection.CredentialReference))
                    {
                        throw new InvalidOperationException(
                            "The Content Manager connection does not have a credential reference.");
                    }

                    cmPassword =
                        await _credentialStore.GetAsync(
                            cmConnection.CredentialReference);

                    if (string.IsNullOrWhiteSpace(
                            cmPassword))
                    {
                        throw new InvalidOperationException(
                            "The configured Content Manager credential could not be retrieved.");
                    }
                }

                // -----------------------------------------------------
                // 7. Initialise SyncResult
                // -----------------------------------------------------
                var result =
                    new SyncResult
                    {
                        IntegrationId =
                            integration.Id,

                        IntegrationName =
                            integration.Name
                    };

                // -----------------------------------------------------
                // 8. Prepare ServiceNow fields
                // -----------------------------------------------------
                var requestedFields =
                    integration.Fields
                        .OrderBy(
                            x => x.DisplayOrder)
                        .Select(
                            x => x.SourceFieldName)
                        .Where(
                            x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase)
                        .ToList();

                if (!requestedFields.Contains(
                        "sys_id",
                        StringComparer.OrdinalIgnoreCase))
                {
                    requestedFields.Add(
                        "sys_id");
                }

                if (!requestedFields.Contains(
                        "number",
                        StringComparer.OrdinalIgnoreCase))
                {
                    requestedFields.Add(
                        "number");
                }

                // -----------------------------------------------------
                // 9. Multi-page state
                // -----------------------------------------------------
                var offset =
                    startingOffset;

                var pageNumber =
                    startingPage;

                var hasMore = true;

                long totalServiceNowFetchMilliseconds = 0;
                long totalCmProcessingMilliseconds = 0;

                IContentManagerProcessingSession?
                    cmSession = null;

                _logger.LogInformation(
                    "SyncJob {SyncJobId}: Synchronization execution starting. " +
                    "Resume: {Resume}, StartingOffset: {StartingOffset}, " +
                    "StartingPage: {StartingPage}, PageSize: {PageSize}, " +
                    "BatchSize: {BatchSize}",
                    syncJob.Id,
                    resume,
                    startingOffset,
                    startingPage,
                    integration.PageSize,
                    batchSize);

                // =====================================================
                // 10. MULTI-PAGE LOOP
                // =====================================================
                while (hasMore)
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    var nextPageNumber =
                        pageNumber + 1;

                    _logger.LogInformation(
                        "SyncJob {SyncJobId}: Fetching ServiceNow page " +
                        "{PageNumber} at offset {Offset}",
                        syncJob.Id,
                        nextPageNumber,
                        offset);

                    // -------------------------------------------------
                    // 11. Build ServiceNow query
                    // -------------------------------------------------
                    var options =
                        new ServiceNowQueryOptions
                        {
                            TableName =
                                integration.TableName,

                            Query =
                                integration.EncodedQuery,

                            PageSize =
                                integration.PageSize,

                            Offset =
                                offset,

                            DisplayValues =
                                integration.DisplayValues,

                            ExcludeReferenceLinks =
                                integration.ExcludeReferenceLinks,

                            Fields =
                                requestedFields
                        };

                    // -------------------------------------------------
                    // 12. Fetch ServiceNow page
                    // -------------------------------------------------
                    var serviceNowStopwatch =
                        Stopwatch.StartNew();

                    var page =
                        await _serviceNowClient
                            .FetchPageAsync(
                                serviceNowConnection,
                                serviceNowSecret,
                                options,
                                cancellationToken);

                    serviceNowStopwatch.Stop();

                    totalServiceNowFetchMilliseconds +=
                        serviceNowStopwatch.ElapsedMilliseconds;

                    if (page.ReturnedCount == 0 ||
                        page.Records.Count == 0)
                    {
                        hasMore = false;

                        break;
                    }

                    pageNumber++;

                    result.PagesProcessed++;

                    result.TotalFetched +=
                        page.ReturnedCount;

                    var failuresBeforePage =
                        result.TotalFailed;

                    // =================================================
                    // 13. PROCESS CURRENT PAGE IN BATCHES
                    // =================================================
                    for (var batchOffset = 0;
                         batchOffset < page.Records.Count;
                         batchOffset += batchSize)
                    {
                        cancellationToken
                            .ThrowIfCancellationRequested();

                        var batchRecords =
                            page.Records
                                .Skip(batchOffset)
                                .Take(batchSize)
                                .Select(
                                    x =>
                                        (IReadOnlyDictionary<
                                            string,
                                            JsonElement>)x)
                                .ToList();

                        if (batchRecords.Count == 0)
                        {
                            continue;
                        }

                        // ---------------------------------------------
                        // 14. Collect sys_ids
                        // ---------------------------------------------
                        var sourceIds =
                            new List<string>();

                        foreach (var sourceRecord in batchRecords)
                        {
                            cancellationToken
                                .ThrowIfCancellationRequested();

                            if (!sourceRecord.TryGetValue(
                                    "sys_id",
                                    out var sysIdElement))
                            {
                                result.TotalFailed++;

                                _logger.LogWarning(
                                    "SyncJob {SyncJobId}: ServiceNow record " +
                                    "does not contain sys_id.",
                                    syncJob.Id);

                                continue;
                            }

                            var sourceSysId =
                                GetJsonValueAsString(
                                    sysIdElement);

                            if (string.IsNullOrWhiteSpace(
                                    sourceSysId))
                            {
                                result.TotalFailed++;

                                _logger.LogWarning(
                                    "SyncJob {SyncJobId}: ServiceNow record " +
                                    "contains an empty sys_id.",
                                    syncJob.Id);

                                continue;
                            }

                            sourceIds.Add(
                                sourceSysId);
                        }

                        // ---------------------------------------------
                        // 15. Batch duplicate lookup
                        // ---------------------------------------------
                        var existingSourceIds =
                            await _syncedRecordRepository
                                .GetExistingSourceIdsAsync(
                                    integration.Id,
                                    sourceIds,
                                    cancellationToken);

                        // ---------------------------------------------
                        // 16. Build records not yet synced
                        // ---------------------------------------------
                        var newRecords =
                            new List<
                                IReadOnlyDictionary<
                                    string,
                                    JsonElement>>();

                        foreach (var sourceRecord in batchRecords)
                        {
                            cancellationToken
                                .ThrowIfCancellationRequested();

                            if (!sourceRecord.TryGetValue(
                                    "sys_id",
                                    out var sysIdElement))
                            {
                                continue;
                            }

                            var sourceSysId =
                                GetJsonValueAsString(
                                    sysIdElement);

                            if (string.IsNullOrWhiteSpace(
                                    sourceSysId))
                            {
                                continue;
                            }

                            if (existingSourceIds.Contains(
                                    sourceSysId))
                            {
                                result.TotalSkipped++;

                                continue;
                            }

                            newRecords.Add(
                                sourceRecord);
                        }

                        // ---------------------------------------------
                        // 17. Process only new records in CM
                        // ---------------------------------------------
                        if (newRecords.Count > 0)
                        {
                            if (cmSession == null)
                            {
                                cmSession =
                                    await _contentManagerClient
                                        .CreateProcessingSessionAsync(
                                            cmConnection,
                                            cmPassword,
                                            integration
                                                .ContentManagerRecordTypeUri
                                                .Value,
                                            integration.Fields,
                                            cancellationToken);
                            }

                            var cmStopwatch =
                                Stopwatch.StartNew();

                            var createResults =
                                await cmSession
                                    .ProcessRecordsAsync(
                                        newRecords,
                                        cancellationToken);

                            cmStopwatch.Stop();

                            totalCmProcessingMilliseconds +=
                                cmStopwatch.ElapsedMilliseconds;

                            // =========================================
                            // 18. Process CM results
                            // =========================================
                            foreach (var createResult in createResults)
                            {
                                cancellationToken
                                    .ThrowIfCancellationRequested();

                                result.TotalProcessed++;

                                // -------------------------------------
                                // Find original ServiceNow record
                                // -------------------------------------
                                var sourceRecord =
                                    newRecords
                                        .FirstOrDefault(
                                            x =>
                                            {
                                                if (!x.TryGetValue(
                                                        "sys_id",
                                                        out var sourceIdElement))
                                                {
                                                    return false;
                                                }

                                                var currentSourceSysId =
                                                    GetJsonValueAsString(
                                                        sourceIdElement);

                                                return string.Equals(
                                                    currentSourceSysId,
                                                    createResult.SourceSysId,
                                                    StringComparison
                                                        .OrdinalIgnoreCase);
                                            });

                                string?
                                    sourceRecordNumber = null;

                                if (sourceRecord != null &&
                                    sourceRecord.TryGetValue(
                                        "number",
                                        out var numberElement))
                                {
                                    sourceRecordNumber =
                                        GetJsonValueAsString(
                                            numberElement);
                                }

                                // -------------------------------------
                                // CONTENT MANAGER CREATION FAILURE
                                // -------------------------------------
                                if (!createResult.Success)
                                {
                                    result.TotalFailed++;

                                    await RecordFailureAsync(
                                        syncJobId:
                                            syncJob.Id,

                                        integrationId:
                                            integration.Id,

                                        sourceSysId:
                                            createResult.SourceSysId,

                                        sourceRecordNumber:
                                            sourceRecordNumber,

                                        errorCategory:
                                            "ContentManager",

                                        errorMessage:
                                            string.IsNullOrWhiteSpace(
                                                createResult.Message)
                                                ? "Content Manager record creation failed."
                                                : createResult.Message,

                                        attemptNumber:
                                            1,

                                        contentManagerRecordUri:
                                            createResult.RecordUri,

                                        cancellationToken:
                                            cancellationToken);

                                    _logger.LogWarning(
                                        "CM record creation failed. " +
                                        "SyncJobId: {SyncJobId}, " +
                                        "IntegrationId: {IntegrationId}, " +
                                        "SourceSysId: {SourceSysId}, " +
                                        "SourceRecordNumber: {SourceRecordNumber}, " +
                                        "Error: {Error}",
                                        syncJob.Id,
                                        integration.Id,
                                        createResult.SourceSysId,
                                        sourceRecordNumber,
                                        createResult.Message);

                                    continue;
                                }

                                // -------------------------------------
                                // CM succeeded but SourceSysId missing
                                // -------------------------------------
                                if (string.IsNullOrWhiteSpace(
                                        createResult.SourceSysId))
                                {
                                    result.TotalFailed++;

                                    _logger.LogWarning(
                                        "CM record was created but SourceSysId " +
                                        "was not returned. SyncJobId: {SyncJobId}",
                                        syncJob.Id);

                                    continue;
                                }

                                // -------------------------------------
                                // CM succeeded but RecordUri missing
                                // -------------------------------------
                                if (!createResult.RecordUri.HasValue)
                                {
                                    result.TotalFailed++;

                                    _logger.LogWarning(
                                        "CM result does not contain RecordUri. " +
                                        "SyncJobId: {SyncJobId}, " +
                                        "SourceSysId: {SourceSysId}",
                                        syncJob.Id,
                                        createResult.SourceSysId);

                                    continue;
                                }

                                // -------------------------------------
                                // 19. Save ServiceNow -> CM identity
                                // -------------------------------------
                                try
                                {
                                    var syncedRecord =
                                        new SyncedRecord(
                                            integration.Id,
                                            createResult.SourceSysId,
                                            sourceRecordNumber,
                                            createResult.RecordUri.Value,
                                            createResult.RecordNumber);

                                    await _syncedRecordRepository
                                        .AddAsync(
                                            syncedRecord,
                                            cancellationToken);

                                    result.TotalSucceeded++;
                                }
                                catch (Exception ex)
                                {
                                    result.TotalFailed++;

                                    _logger.LogError(
                                        ex,
                                        "Sync tracking failed. " +
                                        "SyncJobId: {SyncJobId}, " +
                                        "SourceSysId: {SourceSysId}, " +
                                        "CM URI: {RecordUri}",
                                        syncJob.Id,
                                        createResult.SourceSysId,
                                        createResult.RecordUri);
                                }
                            }
                        }

                        // ---------------------------------------------
                        // 20. Update counters after every batch
                        // ---------------------------------------------
                        syncJob.UpdateCounters(
                            fetched:
                                result.TotalFetched,

                            processed:
                                result.TotalProcessed,

                            succeeded:
                                result.TotalSucceeded,

                            skipped:
                                result.TotalSkipped,

                            failed:
                                result.TotalFailed);

                        await _syncJobRepository
                            .UpdateAsync(
                                syncJob,
                                cancellationToken);
                    }

                    // =================================================
                    // 21. PAGE COMPLETION CHECK
                    // =================================================
                    var pageFailures =
                        result.TotalFailed -
                        failuresBeforePage;

                    if (pageFailures > 0)
                    {
                        syncJob.UpdateCounters(
                            fetched:
                                result.TotalFetched,

                            processed:
                                result.TotalProcessed,

                            succeeded:
                                result.TotalSucceeded,

                            skipped:
                                result.TotalSkipped,

                            failed:
                                result.TotalFailed);

                        await _syncJobRepository
                            .UpdateAsync(
                                syncJob,
                                cancellationToken);

                        throw new InvalidOperationException(
                            $"ServiceNow page {pageNumber} completed with " +
                            $"{pageFailures} failed record(s). " +
                            $"Checkpoint was not advanced.");
                    }

                    // -------------------------------------------------
                    // 22. Entire page completed safely
                    // -------------------------------------------------
                    offset +=
                        page.ReturnedCount;

                    syncJob.UpdateCounters(
                        fetched:
                            result.TotalFetched,

                        processed:
                            result.TotalProcessed,

                        succeeded:
                            result.TotalSucceeded,

                        skipped:
                            result.TotalSkipped,

                        failed:
                            result.TotalFailed);

                    syncJob.AdvanceCheckpoint(
                        completedOffset:
                            offset,

                        completedPage:
                            pageNumber);

                    await _syncJobRepository
                        .UpdateAsync(
                            syncJob,
                            cancellationToken);

                    _logger.LogInformation(
                        "SyncJob {SyncJobId}: Completed page {PageNumber}. " +
                        "CheckpointOffset: {Offset}. " +
                        "Fetched: {Fetched}, Processed: {Processed}, " +
                        "Succeeded: {Succeeded}, Skipped: {Skipped}, " +
                        "Failed: {Failed}",
                        syncJob.Id,
                        pageNumber,
                        offset,
                        result.TotalFetched,
                        result.TotalProcessed,
                        result.TotalSucceeded,
                        result.TotalSkipped,
                        result.TotalFailed);

                    // -------------------------------------------------
                    // 23. Is another page required?
                    // -------------------------------------------------
                    hasMore =
                        page.ReturnedCount ==
                        integration.PageSize;
                }

                // -----------------------------------------------------
                // 24. Store timings
                // -----------------------------------------------------
                result.ServiceNowFetchMilliseconds =
                    totalServiceNowFetchMilliseconds;

                result.ContentManagerProcessingMilliseconds =
                    totalCmProcessingMilliseconds;

                totalStopwatch.Stop();

                result.TotalDurationMilliseconds =
                    totalStopwatch.ElapsedMilliseconds;

                // -----------------------------------------------------
                // 25. Throughput
                // -----------------------------------------------------
                if (result.ContentManagerProcessingMilliseconds > 0 &&
                    result.TotalProcessed > 0)
                {
                    result.RecordsPerSecond =
                        Math.Round(
                            result.TotalProcessed /
                            (result
                                .ContentManagerProcessingMilliseconds /
                             1000.0),
                            2);
                }
                else
                {
                    result.RecordsPerSecond = 0;
                }

                // -----------------------------------------------------
                // 26. Final result
                // -----------------------------------------------------
                result.Success =
                    result.TotalFailed == 0;

                if (result.TotalFetched == 0)
                {
                    result.Message =
                        resume
                            ? $"Resume completed. No additional ServiceNow " +
                              $"records were found after offset {startingOffset}."
                            : "No ServiceNow records matched the configured query.";
                }
                else
                {
                    result.Message =
                        $"{(resume ? "Resume" : "Synchronization")} completed. " +
                        $"Pages: {result.PagesProcessed}, " +
                        $"Fetched: {result.TotalFetched}, " +
                        $"Processed: {result.TotalProcessed}, " +
                        $"Succeeded: {result.TotalSucceeded}, " +
                        $"Skipped: {result.TotalSkipped}, " +
                        $"Failed: {result.TotalFailed}, " +
                        $"SN Fetch: {result.ServiceNowFetchMilliseconds} ms, " +
                        $"CM Processing: " +
                        $"{result.ContentManagerProcessingMilliseconds} ms, " +
                        $"Total: {result.TotalDurationMilliseconds} ms, " +
                        $"Throughput: {result.RecordsPerSecond} records/sec.";
                }

                // -----------------------------------------------------
                // 27. Store final counters
                // -----------------------------------------------------
                syncJob.UpdateCounters(
                    fetched:
                        result.TotalFetched,

                    processed:
                        result.TotalProcessed,

                    succeeded:
                        result.TotalSucceeded,

                    skipped:
                        result.TotalSkipped,

                    failed:
                        result.TotalFailed);

                // -----------------------------------------------------
                // 28. Complete / fail SyncJob
                // -----------------------------------------------------
                if (result.TotalFailed == 0)
                {
                    syncJob.MarkCompleted();

                    _logger.LogInformation(
                        "Sync job {SyncJobId} completed successfully. " +
                        "Resume: {Resume}, Pages: {Pages}, " +
                        "Fetched: {Fetched}, Processed: {Processed}, " +
                        "Succeeded: {Succeeded}, Skipped: {Skipped}",
                        syncJob.Id,
                        resume,
                        result.PagesProcessed,
                        result.TotalFetched,
                        result.TotalProcessed,
                        result.TotalSucceeded,
                        result.TotalSkipped);
                }
                else
                {
                    syncJob.MarkFailed(
                        $"{result.TotalFailed} record(s) failed during synchronization.");

                    _logger.LogWarning(
                        "Sync job {SyncJobId} completed with " +
                        "{Failed} failed record(s).",
                        syncJob.Id,
                        result.TotalFailed);
                }

                await _syncJobRepository
                    .UpdateAsync(
                        syncJob,
                        cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();

                try
                {
                    syncJob.MarkFailed(
                        ex.Message);

                    await _syncJobRepository
                        .UpdateAsync(
                            syncJob,
                            CancellationToken.None);
                }
                catch (Exception jobUpdateException)
                {
                    _logger.LogError(
                        jobUpdateException,
                        "Unable to persist failure status for " +
                        "SyncJob {SyncJobId}",
                        syncJob.Id);
                }

                _logger.LogError(
                    ex,
                    "Sync job {SyncJobId} failed for integration " +
                    "{IntegrationId} - {IntegrationName}. Resume: {Resume}",
                    syncJob.Id,
                    integration.Id,
                    integration.Name,
                    resume);

                throw;
            }
        }

        // =============================================================
        // RECORD LEVEL FAILURE LOGGING
        // =============================================================
        private async Task RecordFailureAsync(
            long syncJobId,
            long integrationId,
            string? sourceSysId,
            string? sourceRecordNumber,
            string errorCategory,
            string errorMessage,
            int attemptNumber = 1,
            long? contentManagerRecordUri = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var failure =
                    new SyncFailure(
                        syncJobId,
                        integrationId,
                        sourceSysId,
                        sourceRecordNumber,
                        errorCategory,
                        errorMessage,
                        attemptNumber,
                        contentManagerRecordUri);

                await _syncFailureRepository
                    .AddAsync(
                        failure,
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to persist SyncFailure. " +
                    "JobId: {SyncJobId}, " +
                    "IntegrationId: {IntegrationId}, " +
                    "SourceSysId: {SourceSysId}",
                    syncJobId,
                    integrationId,
                    sourceSysId);
            }
        }

        private static string? GetJsonValueAsString(
            JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String =>
                    element.GetString(),

                JsonValueKind.Number =>
                    element.ToString(),

                JsonValueKind.True =>
                    "true",

                JsonValueKind.False =>
                    "false",

                JsonValueKind.Null =>
                    null,

                JsonValueKind.Undefined =>
                    null,

                _ =>
                    element.ToString()
            };
        }
    }
}