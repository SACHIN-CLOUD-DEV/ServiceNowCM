namespace ServiceNowCM.Application.DTOs
{
    public class ConnectionTestResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? StatusCode { get; set; }

        public long DurationMilliseconds { get; set; }
    }
}