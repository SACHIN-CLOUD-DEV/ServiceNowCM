namespace ServiceNowCM.Domain.Entities
{
    public class SyncedRecord
    {
        public long Id { get; private set; }

        public long IntegrationId { get; private set; }

        public string SourceSysId { get; private set; } = string.Empty;

        public string? SourceRecordNumber { get; private set; }

        public long ContentManagerRecordUri { get; private set; }

        public string? ContentManagerRecordNumber { get; private set; }

        public DateTime FirstSyncedAtUtc { get; private set; }

        public DateTime LastSyncedAtUtc { get; private set; }

        private SyncedRecord()
        {
        }

        public SyncedRecord(
            long integrationId,
            string sourceSysId,
            string? sourceRecordNumber,
            long contentManagerRecordUri,
            string? contentManagerRecordNumber)
        {
            if (integrationId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(integrationId));
            }

            if (string.IsNullOrWhiteSpace(sourceSysId))
            {
                throw new ArgumentException(
                    "Source sys_id is required.",
                    nameof(sourceSysId));
            }

            if (contentManagerRecordUri <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contentManagerRecordUri));
            }

            IntegrationId = integrationId;

            SourceSysId = sourceSysId.Trim();

            SourceRecordNumber =
                string.IsNullOrWhiteSpace(sourceRecordNumber)
                    ? null
                    : sourceRecordNumber.Trim();

            ContentManagerRecordUri = contentManagerRecordUri;

            ContentManagerRecordNumber =string.IsNullOrWhiteSpace(contentManagerRecordNumber)? null : contentManagerRecordNumber.Trim();

            FirstSyncedAtUtc = DateTime.UtcNow;
            LastSyncedAtUtc = DateTime.UtcNow;
        }

        public void MarkSynced(
            long contentManagerRecordUri,
            string? contentManagerRecordNumber)
        {
            if (contentManagerRecordUri <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contentManagerRecordUri));
            }

            ContentManagerRecordUri =
                contentManagerRecordUri;

            ContentManagerRecordNumber =
                string.IsNullOrWhiteSpace(contentManagerRecordNumber)
                    ? null
                    : contentManagerRecordNumber.Trim();

            LastSyncedAtUtc = DateTime.UtcNow;
        }
    }
}