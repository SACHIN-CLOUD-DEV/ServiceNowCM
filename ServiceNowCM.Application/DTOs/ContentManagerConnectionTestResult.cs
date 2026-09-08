namespace ServiceNowCM.Application.DTOs
{
    public class ContentManagerConnectionTestResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public long DurationMs { get; set; }
    }
}