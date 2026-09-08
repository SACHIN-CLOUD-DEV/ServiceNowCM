using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Interfaces;

public interface ISyncJobRepository
{
    Task AddAsync(
        SyncJob syncJob,
        CancellationToken cancellationToken = default);

    Task<SyncJob?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<SyncJob?> GetLatestIncompleteAsync(
        long integrationId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SyncJob syncJob,
        CancellationToken cancellationToken = default);
}
