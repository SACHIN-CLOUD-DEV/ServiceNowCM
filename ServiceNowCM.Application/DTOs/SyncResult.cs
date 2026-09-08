namespace ServiceNowCM.Application.DTOs
{
    public class SyncResult
    {
        public long IntegrationId { get; set; }

        public string IntegrationName { get; set; } = string.Empty;

        public int TotalFetched { get; set; }

        public int TotalProcessed { get; set; }

        public int PagesProcessed { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TotalSucceeded { get; set; }

        public int TotalFailed { get; set; }

        public long ServiceNowFetchMilliseconds { get; set; }

        public long ContentManagerProcessingMilliseconds { get; set; }

        public long TotalDurationMilliseconds { get; set; }

        public double RecordsPerSecond { get; set; }

        public int TotalSkipped { get; set; }
    }
}