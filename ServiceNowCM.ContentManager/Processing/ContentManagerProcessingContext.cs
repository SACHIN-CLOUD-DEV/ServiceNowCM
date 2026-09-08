using ServiceNowCM.Domain.Entities;
using TRIM.SDK;

namespace ServiceNowCM.ContentManager.Processing
{
    internal sealed class ContentManagerProcessingContext
    {
        public Database Database { get; }

        public RecordType RecordType { get; }

        public IReadOnlyDictionary<long, FieldDefinition>
            AdditionalFields
        { get; }

        public ContentManagerProcessingContext(
            Database database,
            RecordType recordType,
            IReadOnlyDictionary<long, FieldDefinition> additionalFields)
        {
            Database = database
                ?? throw new ArgumentNullException(nameof(database));

            RecordType = recordType
                ?? throw new ArgumentNullException(nameof(recordType));

            AdditionalFields = additionalFields
                ?? throw new ArgumentNullException(nameof(additionalFields));
        }
    }
}
