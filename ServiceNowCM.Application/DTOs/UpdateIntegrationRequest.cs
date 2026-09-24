namespace ServiceNowCM.Application.DTOs
{
    public class UpdateIntegrationRequest
    {
        public string Name { get; set; } = string.Empty;

        public long ServiceNowConnectionId { get; set; }

        public string TableName { get; set; } = string.Empty;

        public string? EncodedQuery { get; set; }

        public int PageSize { get; set; } = 500;

        public int ProcessingBatchSize { get; set; } = 100;

        public bool DisplayValues { get; set; } = true;

        public bool ExcludeReferenceLinks { get; set; } = true;

        public long? ContentManagerConnectionId { get; set; }

        public long? ContentManagerRecordTypeUri { get; set; }

        public string? ContentManagerRecordTypeName { get; set; }

        public List<IntegrationFieldRequest> Fields { get; set; } = new();
    }
}
