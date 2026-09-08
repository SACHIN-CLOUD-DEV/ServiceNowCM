namespace ServiceNowCM.Domain.Entities;

public class SyncJob
{
    public long Id { get; private set; }

    public long IntegrationId { get; private set; }

    public string Status { get; private set; } = null!;

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public int LastCompletedOffset { get; private set; }

    public int LastCompletedPage { get; private set; }

    public int TotalFetched { get; private set; }

    public int TotalProcessed { get; private set; }

    public int TotalSucceeded { get; private set; }

    public int TotalSkipped { get; private set; }

    public int TotalFailed { get; private set; }

    public int RetryCount { get; private set; }

    public string? ErrorSummary { get; private set; }

    public IntegrationConfiguration Integration { get; private set; } = null!;

    private SyncJob()
    {
        // Required by EF Core
    }

    public SyncJob(long integrationId)
    {
        if (integrationId <= 0)
            throw new ArgumentException(
                "Integration ID must be greater than zero.",
                nameof(integrationId));

        IntegrationId = integrationId;

        Status = "Running";

        StartedAtUtc = DateTime.UtcNow;

        LastCompletedOffset = 0;
        LastCompletedPage = 0;

        TotalFetched = 0;
        TotalProcessed = 0;
        TotalSucceeded = 0;
        TotalSkipped = 0;
        TotalFailed = 0;
        RetryCount = 0;
    }

    public void UpdateCounters(
    int fetched,
    int processed,
    int succeeded,
    int skipped,
    int failed)
    {
        if (fetched < 0 ||
            processed < 0 ||
            succeeded < 0 ||
            skipped < 0 ||
            failed < 0)
        {
            throw new ArgumentException(
                "Sync counters cannot be negative.");
        }

        TotalFetched = fetched;
        TotalProcessed = processed;
        TotalSucceeded = succeeded;
        TotalSkipped = skipped;
        TotalFailed = failed;
    }

    public void AdvanceCheckpoint(
        int completedOffset,
        int completedPage)
    {
        if (completedOffset < 0)
        {
            throw new ArgumentException(
                "Completed offset cannot be negative.",
                nameof(completedOffset));
        }

        if (completedPage < 0)
        {
            throw new ArgumentException(
                "Completed page cannot be negative.",
                nameof(completedPage));
        }

        LastCompletedOffset = completedOffset;
        LastCompletedPage = completedPage;
    }

    public void MarkCompleted()
    {
        Status = "Completed";
        CompletedAtUtc = DateTime.UtcNow;
        ErrorSummary = null;
    }

    public void MarkFailed(string errorSummary)
    {
        if (string.IsNullOrWhiteSpace(errorSummary))
            throw new ArgumentException(
                "Error summary is required.",
                nameof(errorSummary));

        Status = "Failed";

        CompletedAtUtc = DateTime.UtcNow;

        ErrorSummary = errorSummary.Trim();
    }

    public void IncrementRetry()
    {
        RetryCount++;
    }

    public void MarkRunning()
    {
        Status = "Running";
        CompletedAtUtc = null;
        ErrorSummary = null;
    }

    public void MarkInterrupted()
    {
        Status = "Interrupted";
        CompletedAtUtc = DateTime.UtcNow;
        ErrorSummary = "Synchronization was interrupted and resumed by a subsequent job.";
    }
}