namespace ServiceNowCM.Application.DTOs
{
    public class IntegrationResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public long ServiceNowConnectionId { get; set; }

        public string TableName { get; set; } = string.Empty;

        public string? EncodedQuery { get; set; }

        public int PageSize { get; set; }

        public int ProcessingBatchSize { get; set; }

        public bool DisplayValues { get; set; }

        public bool ExcludeReferenceLinks { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ModifiedAtUtc { get; set; }

        public long? ContentManagerConnectionId { get; set; }

        public long? ContentManagerRecordTypeUri { get; set; }

        public string? ContentManagerRecordTypeName { get; set; }

        public List<IntegrationFieldResponse> Fields { get; set; } = new();
    }
}