namespace ServiceNowCM.Domain.Entities
{
    public class IntegrationField
    {
        public long Id { get; private set; }

        public long IntegrationConfigurationId { get; private set; }

        public IntegrationConfiguration IntegrationConfiguration { get; private set; } = null!;

        public string SourceFieldName { get; private set; } = string.Empty;

        public string? SourceFieldLabel { get; private set; }

        public string? SourceFieldDataType { get; private set; }

        public int DisplayOrder { get; private set; }

        public string? TargetFieldName { get; private set; }

        public string? TargetFieldType { get; private set; }

        public long? TargetFieldUri { get; private set; }

        private IntegrationField()
        {
        }

        public IntegrationField(
            string sourceFieldName,
            string? sourceFieldLabel,
            string? sourceFieldDataType,
            int displayOrder)
        {
            if (string.IsNullOrWhiteSpace(sourceFieldName))
            {
                throw new ArgumentException(
                    "Source field name is required.",
                    nameof(sourceFieldName));
            }

            SourceFieldName = sourceFieldName;
            SourceFieldLabel = sourceFieldLabel;
            SourceFieldDataType = sourceFieldDataType;
            DisplayOrder = displayOrder;
        }

        public void SetTarget(
            string? targetFieldName,
            string? targetFieldType,
            long? targetFieldUri)
        {
            TargetFieldName = targetFieldName;
            TargetFieldType = targetFieldType;
            TargetFieldUri = targetFieldUri;
        }
    }
}