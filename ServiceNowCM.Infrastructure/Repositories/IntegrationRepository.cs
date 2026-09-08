using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Infrastructure.Persistence;

namespace ServiceNowCM.Infrastructure.Repositories
{
    public class IntegrationRepository : IIntegrationRepository
    {
        private readonly ServiceNowCMDbContext _dbContext;

        public IntegrationRepository(ServiceNowCMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(IntegrationConfiguration integration)
        {
            await _dbContext.IntegrationConfigurations.AddAsync(integration);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IntegrationConfiguration?> GetByIdAsync(long id)
        {
            return await _dbContext.IntegrationConfigurations
                .Include(x => x.Fields)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<IntegrationConfiguration>> GetAllAsync()
        {
            return await _dbContext.IntegrationConfigurations
                .Include(x => x.Fields)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(
            string name)
        {
            return await _dbContext.IntegrationConfigurations.AnyAsync(x => x.Name == name);
        }

        public async Task UpdateAsync(
            IntegrationConfiguration integration)
        {
            _dbContext.IntegrationConfigurations.Update(integration);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            IntegrationConfiguration integration)
        {
            _dbContext.IntegrationConfigurations.Remove(integration);

            await _dbContext.SaveChangesAsync();
        }
    }
}