using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.DTOs
{
    public class CreateContentManagerConnectionRequest
    {
        public string Name { get; set; } = string.Empty;

        public string WorkgroupServerName { get; set; } = string.Empty;

        public int WorkgroupServerPort { get; set; } = 1137;

        public string DatasetId { get; set; } = string.Empty;

        public ContentManagerAuthenticationType AuthenticationType
        {
            get;
            set;
        }

        public string? Username { get; set; }

        public string? Password { get; set; }
    }
}