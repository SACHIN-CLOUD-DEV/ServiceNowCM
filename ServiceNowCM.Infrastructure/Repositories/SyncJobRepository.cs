using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Infrastructure.Persistence;

namespace ServiceNowCM.Infrastructure.Repositories;

public class SyncJobRepository : ISyncJobRepository
{
    private readonly ServiceNowCMDbContext _dbContext;

    public SyncJobRepository(ServiceNowCMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SyncJob syncJob,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SyncJobs.AddAsync(
            syncJob,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<SyncJob?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncJobs
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<SyncJob?> GetLatestIncompleteAsync(
    long integrationId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncJobs
            .Where(x =>
                x.IntegrationId == integrationId &&
                (x.Status == "Failed" ||
                 x.Status == "Running" ||
                 x.Status == "Interrupted"))
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        SyncJob syncJob,
        CancellationToken cancellationToken = default)
    {
        _dbContext.SyncJobs.Update(syncJob);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}