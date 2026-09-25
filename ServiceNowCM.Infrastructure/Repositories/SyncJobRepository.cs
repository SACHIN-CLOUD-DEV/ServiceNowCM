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

    public async Task<SyncJob?> GetLatestResumableAsync(
        long integrationId,
        CancellationToken cancellationToken = default)
    {
        // Always inspect the latest execution first.
        // An older Failed job must not be resumed after a newer
        // execution has already completed or is currently running.
        var latestJob =
            await _dbContext.SyncJobs
                .Where(x =>
                    x.IntegrationId == integrationId)
                .OrderByDescending(x =>
                    x.StartedAtUtc)
                .ThenByDescending(x =>
                    x.Id)
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (latestJob == null)
        {
            return null;
        }

        // Current safe policy:
        // only the latest Failed job can be resumed.
        //
        // Running is excluded because we do not yet have heartbeat/lease
        // logic to determine whether it is active or abandoned.
        //
        // Interrupted is historical and must not be selected repeatedly.
        if (!string.Equals(
                latestJob.Status,
                "Failed",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return latestJob;
    }

    public async Task<bool> HasRunningJobAsync(
    long integrationId,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.SyncJobs
            .AnyAsync(
                x =>
                    x.IntegrationId == integrationId &&
                    x.Status == "Running",
                cancellationToken);
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
