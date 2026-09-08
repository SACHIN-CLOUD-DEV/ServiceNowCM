using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.DTOs
{
    public class UpdateServiceNowConnectionRequest
    {
        public string Name { get; set; } = string.Empty;

        public string InstanceUrl { get; set; } = string.Empty;

        public AuthenticationType AuthenticationType { get; set; }

        public string? ClientId { get; set; }

        public string? ClientSecret { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }
    }
}
