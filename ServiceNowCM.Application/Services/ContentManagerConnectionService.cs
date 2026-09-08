using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.Services
{
    public class ContentManagerConnectionService
    {
        private readonly IContentManagerConnectionRepository _repository;
        private readonly ICredentialStore _credentialStore;
        private readonly IContentManagerClient _contentManagerClient;

        public ContentManagerConnectionService(
            IContentManagerConnectionRepository repository,
            ICredentialStore credentialStore,
            IContentManagerClient contentManagerClient)
        {
            _repository = repository;
            _credentialStore = credentialStore;
            _contentManagerClient = contentManagerClient;
        }

        public async Task<ContentManagerConnection> CreateAsync(
            CreateContentManagerConnectionRequest request)
        {
            var exists =
                await _repository.ExistsByNameAsync(request.Name);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"A Content Manager connection named '{request.Name}' already exists.");
            }

            string? credentialReference = null;

            if (request.AuthenticationType ==
                ContentManagerAuthenticationType.ExplicitCredentials)
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    throw new ArgumentException(
                        "Username is required when using explicit credentials.");
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new ArgumentException(
                        "Password is required when using explicit credentials.");
                }

                credentialReference =
                    $"cm-{Guid.NewGuid():N}";

                await _credentialStore.StoreAsync(
                    credentialReference,
                    request.Password);
            }

            var connection =
                new ContentManagerConnection(
                    request.Name,
                    request.WorkgroupServerName,
                    request.WorkgroupServerPort,
                    request.DatasetId,
                    request.AuthenticationType,
                    request.Username,
                    credentialReference);

            await _repository.AddAsync(connection);

            return connection;
        }

        public async Task<ContentManagerConnectionTestResult> TestConnectionAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            var connection =
                await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"Content Manager connection with ID '{id}' was not found.");
            }

            string? password = null;

            if (connection.AuthenticationType ==
                ContentManagerAuthenticationType.ExplicitCredentials)
            {
                if (string.IsNullOrWhiteSpace(
                    connection.CredentialReference))
                {
                    throw new InvalidOperationException(
                        "The Content Manager connection does not have a credential reference.");
                }

                password =
                    await _credentialStore.GetAsync(
                        connection.CredentialReference);

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "The configured Content Manager credential could not be retrieved.");
                }
            }

            return await _contentManagerClient.TestConnectionAsync(
                connection,
                password,
                cancellationToken);
        }

        public async Task<IReadOnlyList<ContentManagerRecordTypeDto>>
            GetRecordTypesAsync(
                long id,
        CancellationToken cancellationToken = default)
        {
            var connection =
                await _repository.GetByIdAsync(id);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"Content Manager connection with ID '{id}' was not found.");
            }

            string? password = null;

            if (connection.AuthenticationType ==
                ContentManagerAuthenticationType.ExplicitCredentials)
            {
                if (string.IsNullOrWhiteSpace(
                    connection.CredentialReference))
                {
                    throw new InvalidOperationException(
                        "The Content Manager connection does not have a credential reference.");
                }

                password =
                    await _credentialStore.GetAsync(
                        connection.CredentialReference);

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "The configured Content Manager credential could not be retrieved.");
                }
            }

            return await _contentManagerClient.GetRecordTypesAsync(
                connection,
                password,
                cancellationToken);
        }

        public async Task<IReadOnlyList<ContentManagerFieldDto>> GetFieldsAsync(
            int connectionId,
            long recordTypeUri,
            CancellationToken cancellationToken = default)
        {
            var connection = await _repository.GetByIdAsync(connectionId);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"Content Manager connection with ID {connectionId} was not found.");
            }

            string? password = null;

            if (connection.AuthenticationType ==
                ContentManagerAuthenticationType.ExplicitCredentials)
            {
                if (string.IsNullOrWhiteSpace(connection.CredentialReference))
                {
                    throw new InvalidOperationException(
                        "Credential reference is missing.");
                }

                password =
                    await _credentialStore.GetAsync(
                        connection.CredentialReference);
            }

            return await _contentManagerClient.GetFieldsAsync(
                connection,
                password,
                recordTypeUri,
                cancellationToken);
        }

        public async Task<ContentManagerConnection?> GetByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<ContentManagerConnection>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
