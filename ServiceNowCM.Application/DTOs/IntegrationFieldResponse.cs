namespace ServiceNowCM.Application.DTOs
{
    public class IntegrationFieldResponse
    {
        public long Id { get; set; }

        public string SourceFieldName { get; set; } = string.Empty;

        public string? SourceFieldLabel { get; set; }

        public string? SourceDataType { get; set; }

        public int DisplayOrder { get; set; }

        public string? TargetFieldName { get; set; }

        public string? TargetFieldType { get; set; }

        public long? TargetFieldUri { get; set; }
    }
}