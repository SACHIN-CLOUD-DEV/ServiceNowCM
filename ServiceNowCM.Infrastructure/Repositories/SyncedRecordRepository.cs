using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Infrastructure.Persistence;

namespace ServiceNowCM.Infrastructure.Repositories
{
    public class SyncedRecordRepository : ISyncedRecordRepository
    {
        private readonly ServiceNowCMDbContext _dbContext;

        public SyncedRecordRepository(
            ServiceNowCMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SyncedRecord?> GetBySourceAsync(
            long integrationId,
            string sourceSysId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.SyncedRecords
                .FirstOrDefaultAsync(
                    x =>
                        x.IntegrationId == integrationId &&
                        x.SourceSysId == sourceSysId,
                    cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            long integrationId,
            string sourceSysId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.SyncedRecords
                .AnyAsync(
                    x =>
                        x.IntegrationId == integrationId &&
                        x.SourceSysId == sourceSysId,
                    cancellationToken);
        }

        public async Task AddAsync(
            SyncedRecord syncedRecord,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.SyncedRecords
                .AddAsync(
                    syncedRecord,
                    cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        public async Task UpdateAsync(
            SyncedRecord syncedRecord,
            CancellationToken cancellationToken = default)
        {
            _dbContext.SyncedRecords.Update(
                syncedRecord);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<HashSet<string>> GetExistingSourceIdsAsync(
            long integrationId,
            IEnumerable<string> sourceSysIds,
            CancellationToken cancellationToken = default)
        {
            var ids =
                sourceSysIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            if (ids.Count == 0)
            {
                return new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
            }

            var existingIds =
                await _dbContext.SyncedRecords
                    .Where(x =>
                        x.IntegrationId == integrationId &&
                        ids.Contains(x.SourceSysId))
                    .Select(x => x.SourceSysId)
                    .ToListAsync(cancellationToken);

            return existingIds.ToHashSet(
                StringComparer.OrdinalIgnoreCase);
        }
    }
}