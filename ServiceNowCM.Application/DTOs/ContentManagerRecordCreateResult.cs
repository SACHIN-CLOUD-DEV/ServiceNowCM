namespace ServiceNowCM.Application.DTOs
{
    public class ContentManagerRecordCreateResult
    {
        public string? SourceSysId { get; set; }

        public bool Success { get; set; }

        public long? RecordUri { get; set; }

        public string? RecordNumber { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
