using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Infrastructure.Persistence;

namespace ServiceNowCM.Infrastructure.Repositories;

public class SyncFailureRepository : ISyncFailureRepository
{
    private readonly ServiceNowCMDbContext _dbContext;

    public SyncFailureRepository(
        ServiceNowCMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SyncFailure syncFailure,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SyncFailures.AddAsync(
            syncFailure,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<SyncFailure?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncFailures
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SyncFailure>> GetByJobIdAsync(
        long syncJobId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncFailures
            .AsNoTracking()
            .Where(x => x.SyncJobId == syncJobId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SyncFailure>>
        GetUnresolvedByIntegrationIdAsync(
            long integrationId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncFailures
            .AsNoTracking()
            .Where(x =>
                x.IntegrationId == integrationId &&
                !x.IsResolved)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        SyncFailure syncFailure,
        CancellationToken cancellationToken = default)
    {
        _dbContext.SyncFailures.Update(
            syncFailure);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}