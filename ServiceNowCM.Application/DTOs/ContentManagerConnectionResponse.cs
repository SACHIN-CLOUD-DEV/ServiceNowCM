using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.DTOs
{
    public class ContentManagerConnectionResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string WorkgroupServerName { get; set; } = string.Empty;

        public int WorkgroupServerPort { get; set; }

        public string DatasetId { get; set; } = string.Empty;

        public ContentManagerAuthenticationType AuthenticationType
        {
            get;
            set;
        }

        public string? Username { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ModifiedAtUtc { get; set; }
    }
}