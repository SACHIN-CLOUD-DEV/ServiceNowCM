namespace ServiceNowCM.Domain.Entities
{
    public class IntegrationConfiguration
    {
        public long Id { get; private set; }

        public string Name { get; private set; } = null!;

        public long ServiceNowConnectionId { get; private set; }

        public string TableName { get; private set; } = null!;

        public string? EncodedQuery { get; private set; }

        public int PageSize { get; private set; }

        public int ProcessingBatchSize { get; private set; }

        public bool DisplayValues { get; private set; }

        public bool ExcludeReferenceLinks { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? ModifiedAtUtc { get; private set; }

        public long? ContentManagerConnectionId { get; private set; }

        public long? ContentManagerRecordTypeUri { get; private set; }

        public string? ContentManagerRecordTypeName { get; private set; }

        public ICollection<IntegrationField> Fields { get; private set; } = new List<IntegrationField>();


        private IntegrationConfiguration()
        {
        }

        public IntegrationConfiguration(
            string name,
            long serviceNowConnectionId,
            string tableName,
            string? encodedQuery,
            int pageSize,
            int processingBatchSize,
            bool displayValues = true,
            bool excludeReferenceLinks = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Integration name is required.",
                    nameof(name));

            if (serviceNowConnectionId <= 0)
                throw new ArgumentException(
                    "ServiceNow connection is required.",
                    nameof(serviceNowConnectionId));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException(
                    "ServiceNow table name is required.",
                    nameof(tableName));

            if (pageSize <= 0 || pageSize > 1000)
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    "Page size must be between 1 and 1000.");

            if (processingBatchSize <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(processingBatchSize),
                    "Processing batch size must be greater than zero.");

            Name = name.Trim();
            ServiceNowConnectionId = serviceNowConnectionId;
            TableName = tableName.Trim();
            EncodedQuery = encodedQuery;
            PageSize = pageSize;
            ProcessingBatchSize = processingBatchSize;
            DisplayValues = displayValues;
            ExcludeReferenceLinks = excludeReferenceLinks;

            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void AddField(
            string sourceFieldName,
            string? sourceFieldLabel,
            string? sourceDataType,
            int displayOrder)
        {
            if (Fields.Any(x =>
                x.SourceFieldName.Equals(
                    sourceFieldName,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"Field '{sourceFieldName}' is already configured for this integration.");
            }

            var field = new IntegrationField(

                sourceFieldName,
                sourceFieldLabel,
                sourceDataType,
                displayOrder);

            Fields.Add(field);

            ModifiedAtUtc = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string tableName,
            string? encodedQuery,
            int pageSize,
            int processingBatchSize,
            bool displayValues,
            bool excludeReferenceLinks)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Integration name is required.",
                    nameof(name));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException(
                    "ServiceNow table name is required.",
                    nameof(tableName));

            if (pageSize <= 0 || pageSize > 1000)
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    "Page size must be between 1 and 1000.");

            if (processingBatchSize <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(processingBatchSize),
                    "Processing batch size must be greater than zero.");

            Name = name.Trim();
            TableName = tableName.Trim();
            EncodedQuery = encodedQuery;
            PageSize = pageSize;
            ProcessingBatchSize = processingBatchSize;
            DisplayValues = displayValues;
            ExcludeReferenceLinks = excludeReferenceLinks;

            ModifiedAtUtc = DateTime.UtcNow;
        }

        public void ReplaceFields(IEnumerable<IntegrationField> fields)
        {
            Fields.Clear();

            foreach (var field in fields)
            {
                Fields.Add(field);
            }

            ModifiedAtUtc = DateTime.UtcNow;
        }

        public void AddField(IntegrationField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            Fields.Add(field);
        }

        public void SetContentManagerTarget(
            long contentManagerConnectionId,
            long recordTypeUri,
            string recordTypeName)
        {
            if (contentManagerConnectionId <= 0)
            {
                throw new ArgumentException(
                    "Content Manager connection ID must be greater than zero.",
                    nameof(contentManagerConnectionId));
            }

            if (recordTypeUri <= 0)
            {
                throw new ArgumentException(
                    "Record Type URI must be greater than zero.",
                    nameof(recordTypeUri));
            }

            if (string.IsNullOrWhiteSpace(recordTypeName))
            {
                throw new ArgumentException(
                    "Record Type name is required.",
                    nameof(recordTypeName));
            }

            ContentManagerConnectionId = contentManagerConnectionId;
            ContentManagerRecordTypeUri = recordTypeUri;
            ContentManagerRecordTypeName = recordTypeName;
        }
    }
}
