namespace ServiceNowCM.Domain.Entities;

public class SyncFailure
{
    public long Id { get; private set; }

    public long SyncJobId { get; private set; }

    public long IntegrationId { get; private set; }

    public string? SourceSysId { get; private set; }

    public string? SourceRecordNumber { get; private set; }

    public string ErrorCategory { get; private set; } = string.Empty;

    public string ErrorMessage { get; private set; } = string.Empty;

    public int AttemptNumber { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public long? ContentManagerRecordUri { get; private set; }

    public bool IsResolved { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    public SyncJob SyncJob { get; private set; } = null!;

    public IntegrationConfiguration Integration { get; private set; } = null!;

    // Required by EF Core
    private SyncFailure()
    {
    }

    public SyncFailure(
        long syncJobId,
        long integrationId,
        string? sourceSysId,
        string? sourceRecordNumber,
        string errorCategory,
        string errorMessage,
        int attemptNumber = 1,
        long? contentManagerRecordUri = null)
    {
        if (syncJobId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(syncJobId),
                "SyncJobId must be greater than zero.");
        }

        if (integrationId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(integrationId),
                "IntegrationId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(errorCategory))
        {
            throw new ArgumentException(
                "Error category is required.",
                nameof(errorCategory));
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "Error message is required.",
                nameof(errorMessage));
        }

        if (attemptNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attemptNumber),
                "Attempt number must be greater than zero.");
        }

        SyncJobId = syncJobId;
        IntegrationId = integrationId;

        SourceSysId =
            string.IsNullOrWhiteSpace(sourceSysId)
                ? null
                : sourceSysId.Trim();

        SourceRecordNumber =
            string.IsNullOrWhiteSpace(sourceRecordNumber)
                ? null
                : sourceRecordNumber.Trim();

        ErrorCategory = errorCategory.Trim();
        ErrorMessage = errorMessage.Trim();

        AttemptNumber = attemptNumber;

        ContentManagerRecordUri =
            contentManagerRecordUri;

        OccurredAtUtc = DateTime.UtcNow;

        IsResolved = false;
        ResolvedAtUtc = null;
    }

    public void MarkResolved()
    {
        if (IsResolved)
        {
            return;
        }

        IsResolved = true;
        ResolvedAtUtc = DateTime.UtcNow;
    }
}
