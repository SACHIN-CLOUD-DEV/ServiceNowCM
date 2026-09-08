using ServiceNowCM.Application.DTOs;
using System.Text.Json;

namespace ServiceNowCM.Application.Interfaces
{
    public interface IContentManagerProcessingSession
    {
        Task<IReadOnlyList<ContentManagerRecordCreateResult>> ProcessRecordsAsync(
            IEnumerable<IReadOnlyDictionary<string, JsonElement>> sourceRecords,
            CancellationToken cancellationToken = default);
    }
}