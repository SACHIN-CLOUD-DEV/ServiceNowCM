using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.DTOs
{
    public class ServiceNowConnectionResponse
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string InstanceUrl { get; set; } = string.Empty;

        public AuthenticationType AuthenticationType { get; set; }

        public string? ClientId { get; set; }

        public string? Username { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ModifiedAtUtc { get; set; }
    }
}
