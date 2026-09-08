using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Interfaces;

public interface ISyncFailureRepository
{
    Task AddAsync(
        SyncFailure syncFailure,
        CancellationToken cancellationToken = default);

    Task<SyncFailure?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncFailure>> GetByJobIdAsync(
        long syncJobId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncFailure>> GetUnresolvedByIntegrationIdAsync(
        long integrationId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SyncFailure syncFailure,
        CancellationToken cancellationToken = default);
}
