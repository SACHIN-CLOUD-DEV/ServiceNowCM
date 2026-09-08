using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Domain.Entities
{
    public class ContentManagerConnection
    {
        public long Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string WorkgroupServerName { get; private set; } = string.Empty;

        public int WorkgroupServerPort { get; private set; }

        public string DatasetId { get; private set; } = string.Empty;

        public ContentManagerAuthenticationType AuthenticationType
        {
            get;
            private set;
        }

        public string? Username { get; private set; }

        public string? CredentialReference { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? ModifiedAtUtc { get; private set; }

        private ContentManagerConnection()
        {
            // Required by EF Core.
        }

        public ContentManagerConnection(
            string name,
            string workgroupServerName,
            int workgroupServerPort,
            string datasetId,
            ContentManagerAuthenticationType authenticationType,
            string? username = null,
            string? credentialReference = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Connection name is required.",nameof(name));

            if (string.IsNullOrWhiteSpace(workgroupServerName))
                throw new ArgumentException(
                    "Workgroup Server name is required.",
                    nameof(workgroupServerName));

            if (workgroupServerPort <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(workgroupServerPort),
                    "Workgroup Server port must be greater than zero.");

            if (string.IsNullOrWhiteSpace(datasetId))
                throw new ArgumentException(
                    "Dataset ID is required.",
                    nameof(datasetId));

            Name = name.Trim();

            WorkgroupServerName = workgroupServerName.Trim();

            WorkgroupServerPort = workgroupServerPort;

            DatasetId = datasetId.Trim();

            AuthenticationType = authenticationType;

            Username = username?.Trim();

            CredentialReference = credentialReference;

            IsActive = true;

            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}