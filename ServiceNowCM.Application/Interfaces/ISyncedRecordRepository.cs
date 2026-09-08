using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Interfaces
{
    public interface ISyncedRecordRepository
    {
        Task<SyncedRecord?> GetBySourceAsync(
            long integrationId,
            string sourceSysId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            long integrationId,
            string sourceSysId,
            CancellationToken cancellationToken = default);

        Task<HashSet<string>> GetExistingSourceIdsAsync(
            long integrationId,
            IEnumerable<string> sourceSysIds,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            SyncedRecord syncedRecord,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            SyncedRecord syncedRecord,
            CancellationToken cancellationToken = default);

    }
}
