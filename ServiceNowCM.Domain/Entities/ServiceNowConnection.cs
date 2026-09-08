using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Domain.Entities
{
    public class ServiceNowConnection
    {
        public long Id { get; private set; }

        public string Name { get; private set; } = null!;

        public string InstanceUrl { get; private set; } = null!;

        public bool IsActive { get; private set; }

        public AuthenticationType AuthenticationType { get; private set; }

        public string? ClientId { get; private set; }

        public string? ClientSecret { get; private set; }

        public string? Username { get; private set; }

        public string? Password { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? ModifiedAtUtc { get; private set; }

        public string? CredentialReference { get; private set; }



        private ServiceNowConnection()
        {
            // EF Core only
        }


        public ServiceNowConnection(string name, string instanceUrl, AuthenticationType authenticationType, string? clientID, string? clientSecret, string? username, string? password)
        {

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Connection name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(instanceUrl))
            {
                throw new ArgumentException("Instance URL is required.", nameof(instanceUrl));
            }

            if (authenticationType == AuthenticationType.OAuth2)
            {
                if (string.IsNullOrWhiteSpace(clientID))
                {
                    throw new ArgumentException("Client ID is required for OAuth2 authentication.", nameof(clientID));
                }
                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new ArgumentException("Client Secret is required for OAuth2 authentication.", nameof(clientSecret));
                }
            }
            else if (authenticationType == AuthenticationType.Basic)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Username is required for Basic authentication.", nameof(username));
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Password is required for Basic authentication.", nameof(password));
                }
            }


            Name = name;
            InstanceUrl = instanceUrl;
            AuthenticationType = authenticationType;

            ClientId = clientID;
            ClientSecret = clientSecret;
            Username = username;
            Password = password;

            IsActive = true;

            CreatedAtUtc = DateTime.UtcNow;
        }

        public void SetCredentialReference(string credentialReference)
        {
            if (string.IsNullOrWhiteSpace(credentialReference))
            {
                throw new ArgumentException(
                    "Credential reference cannot be empty.",
                    nameof(credentialReference));
            }

            CredentialReference = credentialReference;
            ModifiedAtUtc = DateTime.UtcNow;
        }

        public void Enable()
        {
            IsActive = true;
            ModifiedAtUtc = DateTime.UtcNow;
        }

        public void Disable()
        {
            IsActive = false;
            ModifiedAtUtc = DateTime.UtcNow;
        }

        public void Update(
    string name,
    string instanceUrl,
    AuthenticationType authenticationType,
    string? clientId,
    string? username)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Connection name is required.",
                    nameof(name));
            }

            if (string.IsNullOrWhiteSpace(instanceUrl))
            {
                throw new ArgumentException(
                    "Instance URL is required.",
                    nameof(instanceUrl));
            }

            Name = name;
            InstanceUrl = instanceUrl;
            AuthenticationType = authenticationType;
            ClientId = clientId;
            Username = username;

            ModifiedAtUtc = DateTime.UtcNow;
        }
    }
}
