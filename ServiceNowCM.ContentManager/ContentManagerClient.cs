using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;
using System.Diagnostics;
using System.Text.Json;
using TRIM.SDK;

namespace ServiceNowCM.ContentManager.Services
{
    public class ContentManagerClient : IContentManagerClient
    {
        public Task<ContentManagerConnectionTestResult> TestConnectionAsync(
            ContentManagerConnection connection,
            string? password,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var db =
                    CreateDatabase(
                        connection,
                        password);

                db.Connect();

                stopwatch.Stop();

                return Task.FromResult(
                    new ContentManagerConnectionTestResult
                    {
                        Success = true,
                        Message =
                            "Successfully connected to Content Manager.",
                        DurationMs =
                            stopwatch.ElapsedMilliseconds
                    });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                return Task.FromResult(
                    new ContentManagerConnectionTestResult
                    {
                        Success = false,
                        Message = ex.Message,
                        DurationMs =
                            stopwatch.ElapsedMilliseconds
                    });
            }
        }

        public Task<IReadOnlyList<ContentManagerRecordTypeDto>>
            GetRecordTypesAsync(
                ContentManagerConnection connection,
                string? password,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var db =
                CreateDatabase(
                    connection,
                    password);

            db.Connect();

            var search =
                new TrimMainObjectSearch(
                    db,
                    BaseObjectTypes.RecordType);

            search.SelectAll();

            var results =
                new List<ContentManagerRecordTypeDto>();

            foreach (RecordType recordType in search)
            {
                cancellationToken.ThrowIfCancellationRequested();

                results.Add(
                    new ContentManagerRecordTypeDto
                    {
                        Uri = recordType.Uri,
                        Name = recordType.Name
                    });
            }

            return Task.FromResult<
                IReadOnlyList<ContentManagerRecordTypeDto>>(
                    results
                        .OrderBy(x => x.Name)
                        .ToList());
        }

        public Task<IReadOnlyList<ContentManagerFieldDto>>
            GetFieldsAsync(
                ContentManagerConnection connection,
                string? password,
                long recordTypeUri,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var db =
                CreateDatabase(
                    connection,
                    password);

            db.Connect();

            var recordType =
                new RecordType(
                    db,
                    recordTypeUri);

            var results =
                new List<ContentManagerFieldDto>();

            FieldDefinitionList fields =
                recordType.UserFields;

            foreach (FieldDefinition field in fields)
            {
                cancellationToken.ThrowIfCancellationRequested();

                results.Add(
                    new ContentManagerFieldDto
                    {
                        Name = field.Name,
                        Label = field.Name,
                        FieldType = "AdditionalField",
                        DataType = field.Format.ToString(),
                        Uri = field.Uri
                    });
            }

            return Task.FromResult<
                IReadOnlyList<ContentManagerFieldDto>>(
                    results
                        .OrderBy(x => x.Label)
                        .ToList());
        }

        public Task<ContentManagerCreateRecordResult>
            CreateRecordAsync(
                ContentManagerConnection connection,
                string? password,
                long recordTypeUri,
                IReadOnlyDictionary<string, JsonElement> sourceRecord,
                IEnumerable<IntegrationField> mappings,
                CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // -----------------------------------------------------
                // 1. Create and connect CM Database
                // -----------------------------------------------------
                var db =
                    CreateDatabase(
                        connection,
                        password);

                db.Connect();

                // -----------------------------------------------------
                // 2. Load Record Type
                // -----------------------------------------------------
                var recordType =
                    new RecordType(
                        db,
                        recordTypeUri);

                // -----------------------------------------------------
                // 3. Create CM Record
                // -----------------------------------------------------
                var record =
                    new Record(
                        db,
                        recordType);

                // -----------------------------------------------------
                // 4. Process configured mappings
                // -----------------------------------------------------
                foreach (var mapping in
                         mappings.OrderBy(x => x.DisplayOrder))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(
                            mapping.TargetFieldName) ||
                        string.IsNullOrWhiteSpace(
                            mapping.TargetFieldType))
                    {
                        continue;
                    }

                    if (!sourceRecord.TryGetValue(
                            mapping.SourceFieldName,
                            out var sourceValue))
                    {
                        continue;
                    }

                    var value =
                        GetJsonValueAsString(
                            sourceValue);

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    // -------------------------------------------------
                    // Built-in CM property
                    // -------------------------------------------------
                    if (string.Equals(
                            mapping.TargetFieldType,
                            "Property",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        ApplyBuiltInProperty(
                            record,
                            mapping,
                            value);

                        continue;
                    }

                    // -------------------------------------------------
                    // CM Additional Field
                    // -------------------------------------------------
                    if (string.Equals(
                            mapping.TargetFieldType,
                            "AdditionalField",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        ApplyAdditionalField(
                            db,
                            record,
                            mapping,
                            value);

                        continue;
                    }

                    throw new InvalidOperationException(
                        $"Unsupported Content Manager target field type " +
                        $"'{mapping.TargetFieldType}' for ServiceNow field " +
                        $"'{mapping.SourceFieldName}'.");
                }

                // -----------------------------------------------------
                // 5. Save CM Record
                // -----------------------------------------------------
                record.Save();

                return Task.FromResult(
                    new ContentManagerCreateRecordResult
                    {
                        Success = true,
                        RecordUri = record.Uri,
                        RecordNumber = record.Number,
                        Message =
                            "Content Manager record created successfully."
                    });
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new ContentManagerCreateRecordResult
                    {
                        Success = false,
                        Message = ex.Message
                    });
            }
        }

        // =============================================================
        // PROCESSING SESSION
        // =============================================================

        public Task<IContentManagerProcessingSession>
            CreateProcessingSessionAsync(
                ContentManagerConnection connection,
                string? password,
                long recordTypeUri,
                IEnumerable<IntegrationField> mappings,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (connection == null)
            {
                throw new ArgumentNullException(
                    nameof(connection));
            }

            if (recordTypeUri <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(recordTypeUri),
                    "Content Manager Record Type URI must be greater than zero.");
            }

            if (mappings == null)
            {
                throw new ArgumentNullException(
                    nameof(mappings));
            }

            // ---------------------------------------------------------
            // IMPORTANT THREAD-SAFETY CHANGE
            //
            // DO NOT create:
            //
            // Database
            // RecordType
            // FieldDefinition
            //
            // here.
            //
            // TRIM SDK Database objects are thread-affine.
            //
            // IntegrationSyncService contains async operations and the
            // continuation may execute on another thread.
            //
            // Therefore the processing session stores only normal .NET
            // configuration and creates the CM Database inside each
            // ProcessRecordsAsync() execution.
            // ---------------------------------------------------------

            var mappingList =
                mappings
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

            IContentManagerProcessingSession session =
                new Processing.ContentManagerProcessingSession(
                    connection,
                    password,
                    recordTypeUri,
                    mappingList);

            return Task.FromResult(
                session);
        }

        // =============================================================
        // BATCH RECORD CREATION
        // =============================================================

        public Task<IReadOnlyList<ContentManagerRecordCreateResult>>
            CreateRecordsAsync(
                ContentManagerConnection connection,
                string? password,
                long recordTypeUri,
                IEnumerable<IReadOnlyDictionary<string, JsonElement>>
                    sourceRecords,
                IEnumerable<IntegrationField> mappings,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (connection == null)
            {
                throw new ArgumentNullException(
                    nameof(connection));
            }

            if (sourceRecords == null)
            {
                throw new ArgumentNullException(
                    nameof(sourceRecords));
            }

            if (mappings == null)
            {
                throw new ArgumentNullException(
                    nameof(mappings));
            }

            // ---------------------------------------------------------
            // Create a lightweight session.
            //
            // No TRIM SDK Database is created here.
            // ---------------------------------------------------------
            IContentManagerProcessingSession session =
                new Processing.ContentManagerProcessingSession(
                    connection,
                    password,
                    recordTypeUri,
                    mappings);

            // ---------------------------------------------------------
            // ProcessRecordsAsync internally performs all CM SDK work
            // synchronously on the same thread.
            // ---------------------------------------------------------
            return session.ProcessRecordsAsync(
                sourceRecords,
                cancellationToken);
        }

        // =============================================================
        // BUILT-IN PROPERTY MAPPING
        // =============================================================

        private static void ApplyBuiltInProperty(
            Record record,
            IntegrationField mapping,
            string value)
        {
            if (string.Equals(
                    mapping.TargetFieldName,
                    "Title",
                    StringComparison.OrdinalIgnoreCase))
            {
                record.Title = value;

                return;
            }

            throw new InvalidOperationException(
                $"Unsupported Content Manager built-in property " +
                $"'{mapping.TargetFieldName}'.");
        }

        // =============================================================
        // ADDITIONAL FIELD MAPPING
        //
        // Used by the single-record CreateRecordAsync method.
        // =============================================================

        private static void ApplyAdditionalField(
            Database db,
            Record record,
            IntegrationField mapping,
            string value)
        {
            if (!mapping.TargetFieldUri.HasValue)
            {
                throw new InvalidOperationException(
                    $"Content Manager Additional Field URI is missing " +
                    $"for '{mapping.TargetFieldName}'.");
            }

            var fieldDefinition =
                new FieldDefinition(
                    db,
                    mapping.TargetFieldUri.Value);

            var propertyOrFieldValue =
                new PropertyOrFieldValue(
                    fieldDefinition);

            propertyOrFieldValue.SetValueFromString(
                value);

            var userFieldValue =
                propertyOrFieldValue.GetCurrentValue();

            record.SetFieldValue(
                fieldDefinition,
                userFieldValue);
        }

        // =============================================================
        // JSON VALUE CONVERSION
        // =============================================================

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

        // =============================================================
        // DATABASE CREATION
        // =============================================================

        private Database CreateDatabase(
            ContentManagerConnection connection,
            string? password)
        {
            var db =
                new Database
                {
                    WorkgroupServerName =
                        connection.WorkgroupServerName,

                    WorkgroupServerPort =
                        connection.WorkgroupServerPort,

                    Id =
                        connection.DatasetId
                };

            if (connection.AuthenticationType ==
                ContentManagerAuthenticationType.IntegratedWindows)
            {
                db.AuthenticationMethod =
                    ClientAuthenticationMechanism.IntegratedWindows;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(
                        connection.Username))
                {
                    throw new InvalidOperationException(
                        "Username is required for explicit authentication.");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "Password could not be retrieved.");
                }

                db.SetAuthenticationCredentials(
                    connection.Username,
                    password);
            }

            return db;
        }
    }
}