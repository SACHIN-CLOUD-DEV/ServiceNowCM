using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;

namespace ServiceNowCM.Application.Services
{
    public class IntegrationService
    {
        private readonly IIntegrationRepository _integrationRepository;
        private readonly IServiceNowConnectionRepository _serviceNowConnectionRepository;
        private readonly ICredentialStore _credentialStore;
        private readonly IServiceNowClient _serviceNowClient;
        private readonly IContentManagerConnectionRepository _contentManagerConnectionRepository;
        private readonly IContentManagerClient _contentManagerClient;

        public IntegrationService(
            IIntegrationRepository integrationRepository,
            IServiceNowConnectionRepository serviceNowConnectionRepository,
            ICredentialStore credentialStore,
            IServiceNowClient serviceNowClient,
            IContentManagerConnectionRepository contentManagerConnectionRepository,
            IContentManagerClient contentManagerClient)
        {
            _integrationRepository = integrationRepository;
            _serviceNowConnectionRepository = serviceNowConnectionRepository;
            _credentialStore = credentialStore;
            _serviceNowClient = serviceNowClient;
            _contentManagerConnectionRepository = contentManagerConnectionRepository;
            _contentManagerClient = contentManagerClient;
        }

        // ---------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------

        public async Task<IntegrationConfiguration> CreateAsync(CreateIntegrationRequest request)
        {
            var connection =
                await _serviceNowConnectionRepository.GetByIdAsync(request.ServiceNowConnectionId);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{request.ServiceNowConnectionId}' was not found.");
            }

            var exists =
                await _integrationRepository.ExistsByNameAsync(
                    request.Name);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"An integration named '{request.Name}' already exists.");
            }

            ValidateFields(request.Fields);

            var integration =
                new IntegrationConfiguration(
                    request.Name,
                    request.ServiceNowConnectionId,
                    request.TableName,
                    request.EncodedQuery,
                    request.PageSize,
                    request.ProcessingBatchSize,
                    request.DisplayValues,
                    request.ExcludeReferenceLinks);

            await ValidateContentManagerTargetAsync(
                request.ContentManagerConnectionId,
                request.ContentManagerRecordTypeUri,
                request.ContentManagerRecordTypeName,
                request.Fields);

            if (request.ContentManagerConnectionId.HasValue &&
            request.ContentManagerRecordTypeUri.HasValue &&
            !string.IsNullOrWhiteSpace(request.ContentManagerRecordTypeName))
            {
                integration.SetContentManagerTarget(
                    request.ContentManagerConnectionId.Value,
                    request.ContentManagerRecordTypeUri.Value,
                    request.ContentManagerRecordTypeName);
            }

            foreach (var fieldRequest in request.Fields)
            {
                var field =
                    CreateIntegrationField(fieldRequest);

                integration.AddField(field);
            }

            await _integrationRepository.AddAsync(integration);

            return integration;
        }

        // ---------------------------------------------------------
        // GET BY ID
        // ---------------------------------------------------------

        public async Task<IntegrationConfiguration?> GetByIdAsync(long id)
        {
            return await _integrationRepository.GetByIdAsync(id);
        }

        // ---------------------------------------------------------
        // GET ALL
        // ---------------------------------------------------------

        public async Task<IReadOnlyList<IntegrationConfiguration>> GetAllAsync()
        {
            return await _integrationRepository.GetAllAsync();
        }

        // ---------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------

        public async Task<IntegrationConfiguration> UpdateAsync(
            long id,
            UpdateIntegrationRequest request)
        {
            var integration =
                await _integrationRepository.GetByIdAsync(id);

            if (integration == null)
            {
                throw new KeyNotFoundException(
                    $"Integration with ID '{id}' was not found.");
            }

            var serviceNowConnection =
                await _serviceNowConnectionRepository.GetByIdAsync(
                    request.ServiceNowConnectionId);

            if (serviceNowConnection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{request.ServiceNowConnectionId}' was not found.");
            }

            var nameExists =
                await _integrationRepository.ExistsByNameAsync(
                    request.Name);

            if (nameExists &&
                !integration.Name.Equals(
                    request.Name,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"An integration named '{request.Name}' already exists.");
            }

            ValidateFields(request.Fields);

            await ValidateContentManagerTargetAsync(
                request.ContentManagerConnectionId,
                request.ContentManagerRecordTypeUri,
                request.ContentManagerRecordTypeName,
                request.Fields);

            integration.SetServiceNowConnection(
                request.ServiceNowConnectionId);

            integration.Update(
                request.Name,
                request.TableName,
                request.EncodedQuery,
                request.PageSize,
                request.ProcessingBatchSize,
                request.DisplayValues,
                request.ExcludeReferenceLinks);

            if (request.ContentManagerConnectionId.HasValue &&
                request.ContentManagerRecordTypeUri.HasValue &&
                !string.IsNullOrWhiteSpace(request.ContentManagerRecordTypeName))
            {
                integration.SetContentManagerTarget(
                    request.ContentManagerConnectionId.Value,
                    request.ContentManagerRecordTypeUri.Value,
                    request.ContentManagerRecordTypeName);
            }

            var fields =
                new List<IntegrationField>();

            foreach (var fieldRequest in request.Fields)
            {
                var field =
                    CreateIntegrationField(fieldRequest);

                fields.Add(field);
            }

            integration.ReplaceFields(fields);

            await _integrationRepository.UpdateAsync(integration);

            return integration;
        }

        // ---------------------------------------------------------
        // PREVIEW
        // ---------------------------------------------------------

        public async Task<ServiceNowPageResult> PreviewAsync(
            long integrationId,
            int offset = 0,
            CancellationToken cancellationToken = default)
        {
            var integration =
                await _integrationRepository.GetByIdAsync(
                    integrationId);

            if (integration == null)
            {
                throw new KeyNotFoundException(
                    $"Integration with ID '{integrationId}' was not found.");
            }

            var connection =
                await _serviceNowConnectionRepository.GetByIdAsync(
                    integration.ServiceNowConnectionId);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    $"ServiceNow connection with ID '{integration.ServiceNowConnectionId}' was not found.");
            }

            if (string.IsNullOrWhiteSpace(
                connection.CredentialReference))
            {
                throw new InvalidOperationException(
                    "The ServiceNow connection does not have a credential reference.");
            }

            var secret =
                await _credentialStore.GetAsync(
                    connection.CredentialReference);

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "The configured ServiceNow credential could not be retrieved.");
            }

            var options =
                new ServiceNowQueryOptions
                {
                    TableName = integration.TableName,
                    Query = integration.EncodedQuery,
                    PageSize = integration.PageSize,
                    Offset = offset,
                    DisplayValues = integration.DisplayValues,
                    ExcludeReferenceLinks =
                        integration.ExcludeReferenceLinks,

                    Fields = integration.Fields
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => x.SourceFieldName)
                        .ToList()
                };

            return await _serviceNowClient.FetchPageAsync(
                connection,
                secret,
                options,
                cancellationToken);
        }

        private async Task ValidateContentManagerTargetAsync(
            long? connectionId,
            long? recordTypeUri,
            string? recordTypeName,
            IReadOnlyCollection<IntegrationFieldRequest> fields,
            CancellationToken cancellationToken = default)
        {
            var nothingProvided =
                !connectionId.HasValue &&
                !recordTypeUri.HasValue &&
                string.IsNullOrWhiteSpace(recordTypeName);

            if (nothingProvided)
            {
                return;
            }

            if (!connectionId.HasValue ||
                !recordTypeUri.HasValue ||
                string.IsNullOrWhiteSpace(recordTypeName))
            {
                throw new ArgumentException(
                    "Content Manager connection, Record Type URI and Record Type name must all be provided together.");
            }

            var cmConnection =
                await _contentManagerConnectionRepository.GetByIdAsync(
                    connectionId.Value);

            if (cmConnection == null)
            {
                throw new KeyNotFoundException(
                    $"Content Manager connection with ID '{connectionId.Value}' was not found.");
            }

            string? password = null;

            if (cmConnection.AuthenticationType ==
                ContentManagerAuthenticationType.ExplicitCredentials)
            {
                if (string.IsNullOrWhiteSpace(cmConnection.CredentialReference))
                {
                    throw new InvalidOperationException(
                        "The Content Manager connection does not have a credential reference.");
                }

                password =
                    await _credentialStore.GetAsync(
                        cmConnection.CredentialReference);

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "The configured Content Manager credential could not be retrieved.");
                }
            }

            var recordTypes =
                await _contentManagerClient.GetRecordTypesAsync(
                    cmConnection,
                    password,
                    cancellationToken);

            var recordType =
                recordTypes.FirstOrDefault(
                    x => x.Uri == recordTypeUri.Value);

            if (recordType == null)
            {
                throw new ArgumentException(
                    $"Record Type URI '{recordTypeUri.Value}' was not found in the selected Content Manager connection.");
            }

            if (!recordType.Name.Equals(
                recordTypeName,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Record Type name '{recordTypeName}' does not match URI '{recordTypeUri.Value}'.");
            }

            // ---------------------------------------------------------
            // Validate configured CM Additional Field mappings
            // ---------------------------------------------------------
            var additionalFieldMappings =
                fields
                    .Where(
                        x => string.Equals(
                            x.TargetFieldType,
                            "AdditionalField",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (additionalFieldMappings.Count == 0)
            {
                return;
            }

            var availableFields =
                await _contentManagerClient.GetFieldsAsync(
                    cmConnection,
                    password,
                    recordTypeUri.Value,
                    cancellationToken);

            foreach (var fieldMapping in additionalFieldMappings)
            {
                if (!fieldMapping.TargetFieldUri.HasValue)
                {
                    throw new ArgumentException(
                        $"Content Manager Additional Field URI is required for " +
                        $"ServiceNow field '{fieldMapping.SourceFieldName}'.");
                }

                var targetFieldExists =
                    availableFields.Any(
                        x => x.Uri ==
                             fieldMapping.TargetFieldUri.Value);

                if (!targetFieldExists)
                {
                    throw new ArgumentException(
                        $"Content Manager Additional Field URI " +
                        $"'{fieldMapping.TargetFieldUri.Value}' configured for " +
                        $"ServiceNow field '{fieldMapping.SourceFieldName}' " +
                        $"was not found on Record Type " +
                        $"'{recordTypeName}' (URI {recordTypeUri.Value}).");
                }
            }
        }

        // ---------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------

        public async Task DeleteAsync(long id)
        {
            var integration =
                await _integrationRepository.GetByIdAsync(id);

            if (integration == null)
            {
                throw new KeyNotFoundException(
                    $"Integration with ID '{id}' was not found.");
            }

            await _integrationRepository.DeleteAsync(
                integration);
        }

        // ---------------------------------------------------------
        // PRIVATE HELPERS
        // ---------------------------------------------------------

        private static IntegrationField CreateIntegrationField(
            IntegrationFieldRequest fieldRequest)
        {
            var field =
                new IntegrationField(
                    fieldRequest.SourceFieldName,
                    fieldRequest.SourceFieldLabel,
                    fieldRequest.SourceFieldDataType,
                    fieldRequest.DisplayOrder);

            field.SetTarget(
                fieldRequest.TargetFieldName,
                fieldRequest.TargetFieldType,
                fieldRequest.TargetFieldUri);

            return field;
        }

        private static void ValidateFields(
            IReadOnlyCollection<IntegrationFieldRequest>? fields)
        {
            if (fields == null || fields.Count == 0)
            {
                throw new ArgumentException(
                    "At least one ServiceNow field must be selected.");
            }
        }
    }
}