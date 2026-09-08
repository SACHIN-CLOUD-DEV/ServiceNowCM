using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.Services
{
    public class ServiceNowConnectionService
    {

        private readonly IServiceNowConnectionRepository _repository;

        private readonly ICredentialStore _credentialStore;

        private readonly IServiceNowClient _serviceNowClient;

        public ServiceNowConnectionService(
    IServiceNowConnectionRepository repository,
    ICredentialStore credentialStore,
    IServiceNowClient serviceNowClient)
        {
            _repository = repository;
            _credentialStore = credentialStore;
            _serviceNowClient = serviceNowClient;
        }

        public async Task<ServiceNowConnection> CreateAsync(CreateServiceNowConnectionRequest request)
        {
            var exists = await _repository.ExistsByNameAsync(request.Name);

            string secret;

            if (exists)
            {
                throw new InvalidOperationException(
                    $"A ServiceNow connection named '{request.Name}' already exists.");
            }
            var connection = new ServiceNowConnection(
      request.Name,
      request.InstanceUrl,
      request.AuthenticationType,
      request.ClientId,
      request.ClientSecret,
      request.Username,
      request.Password);

            if (request.AuthenticationType == AuthenticationType.OAuth2)
            {
                secret = request.ClientSecret!;
            }
            else
            {
                secret = request.Password!;
            }

            var credentialKey = $"servicenow-{Guid.NewGuid():N}";

            var credentialReference = await _credentialStore.StoreAsync(credentialKey, secret);

            connection.SetCredentialReference(credentialReference);

            await _repository.AddAsync(connection);

            return connection;
        }

        public async Task<ServiceNowConnection> UpdateAsync(long id, UpdateServiceNowConnectionRequest request)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            var nameExists = await _repository.ExistsByNameAsync(request.Name);

            if (nameExists && !connection.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"A ServiceNow connection named '{request.Name}' already exists.");
            }

            connection.Update(
                request.Name,
                request.InstanceUrl,
                request.AuthenticationType,
                request.ClientId,
                request.Username);

            string? newSecret = null;

            if (request.AuthenticationType == AuthenticationType.OAuth2)
            {
                newSecret = request.ClientSecret;
            }
            else if (request.AuthenticationType == AuthenticationType.Basic)
            {
                newSecret = request.Password;
            }

            if (!string.IsNullOrWhiteSpace(newSecret))
            {
                var credentialKey = $"servicenow-{Guid.NewGuid():N}";

                var credentialReference = await _credentialStore.StoreAsync(credentialKey, newSecret);

                connection.SetCredentialReference(credentialReference);
            }

            await _repository.UpdateAsync(connection);

            return connection;
        }

        public async Task<ServiceNowConnection> EnableAsync(long id)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            connection.Enable();

            await _repository.UpdateAsync(connection);

            return connection;
        }

        public async Task<ServiceNowConnection> DisableAsync(long id)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            connection.Disable();

            await _repository.UpdateAsync(connection);

            return connection;
        }

        public async Task<ServiceNowConnection?> GetByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<ServiceNowConnection>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            if (!string.IsNullOrWhiteSpace(connection.CredentialReference))
            {
                await _credentialStore.DeleteAsync(connection.CredentialReference);
            }

            await _repository.DeleteAsync(connection);
        }

        public async Task<ConnectionTestResult> TestConnectionAsync(long id, CancellationToken cancellationToken = default)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            if (string.IsNullOrWhiteSpace(connection.CredentialReference))
            {
                throw new InvalidOperationException(
                    "No credential is configured for this ServiceNow connection.");
            }

            var secret = await _credentialStore.GetAsync(connection.CredentialReference);

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "The configured ServiceNow credential could not be retrieved.");
            }

            return await _serviceNowClient.TestConnectionAsync(
                connection,
                secret,
                cancellationToken);
        }

        public async Task<IReadOnlyList<ServiceNowTableInfo>> GetTablesAsync(long id, CancellationToken cancellationToken = default)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            if (string.IsNullOrWhiteSpace(connection.CredentialReference))
            {
                throw new InvalidOperationException(
                    "No credential is configured for this ServiceNow connection.");
            }

            var secret = await _credentialStore.GetAsync(connection.CredentialReference);

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "The configured ServiceNow credential could not be retrieved.");
            }

            return await _serviceNowClient.GetTablesAsync(
                connection,
                secret,
                cancellationToken);
        }

        public async Task<IReadOnlyList<ServiceNowFieldInfo>> GetFieldsAsync(
            long id,
            string tableName,
            CancellationToken cancellationToken = default)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            if (string.IsNullOrWhiteSpace(connection.CredentialReference))
            {
                throw new InvalidOperationException(
                    "No credential is configured for this ServiceNow connection.");
            }

            var secret = await _credentialStore.GetAsync(connection.CredentialReference);

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "The configured ServiceNow credential could not be retrieved.");
            }

            return await _serviceNowClient.GetFieldsAsync(
                connection,
                secret,
                tableName,
                cancellationToken);
        }

        public async Task<ServiceNowPageResult> PreviewAsync(
            long id,
            ServiceNowQueryOptions options,
            CancellationToken cancellationToken = default)
        {
            var connection = await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{id}' was not found.");
            }

            if (string.IsNullOrWhiteSpace(connection.CredentialReference))
            {
                throw new InvalidOperationException(
                    "No credential is configured for this ServiceNow connection.");
            }

            var secret =await _credentialStore.GetAsync(connection.CredentialReference);

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "The configured ServiceNow credential could not be retrieved.");
            }

            return await _serviceNowClient.FetchPageAsync(
                connection,
                secret,
                options,
                cancellationToken);
        }

    }
}
