using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Domain.Entities;
using System.Text.Json;

namespace ServiceNowCM.Application.Interfaces
{
    public interface IContentManagerClient
    {
        Task<ContentManagerConnectionTestResult> TestConnectionAsync(
            ContentManagerConnection connection,
            string? password,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ContentManagerRecordTypeDto>> GetRecordTypesAsync(
            ContentManagerConnection connection,
            string? password,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ContentManagerFieldDto>> GetFieldsAsync(
            ContentManagerConnection connection,
            string? password,
            long recordTypeUri,
            CancellationToken cancellationToken = default);

        Task<ContentManagerCreateRecordResult> CreateRecordAsync(
            ContentManagerConnection connection,
            string? password,
            long recordTypeUri,
            IReadOnlyDictionary<string, JsonElement> sourceRecord,
            IEnumerable<IntegrationField> mappings,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ContentManagerRecordCreateResult>> CreateRecordsAsync(
            ContentManagerConnection connection,
            string? password,
            long recordTypeUri,
            IEnumerable<IReadOnlyDictionary<string, JsonElement>> sourceRecords,
            IEnumerable<IntegrationField> mappings,
            CancellationToken cancellationToken = default);

        Task<IContentManagerProcessingSession> CreateProcessingSessionAsync(
            ContentManagerConnection connection,
            string? password,
            long recordTypeUri,
            IEnumerable<IntegrationField> mappings,
            CancellationToken cancellationToken = default);
    }
}
