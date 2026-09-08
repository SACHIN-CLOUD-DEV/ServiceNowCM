using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Infrastructure.Persistence;

namespace ServiceNowCM.Infrastructure.Repositories
{
    public class ServiceNowConnectionRepository: IServiceNowConnectionRepository
    {
        private readonly ServiceNowCMDbContext _dbContext;

        public ServiceNowConnectionRepository(ServiceNowCMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ServiceNowConnection connection)
        {
            await _dbContext.ServiceNowConnections.AddAsync(connection);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<ServiceNowConnection?> GetByIdAsync(long id)
        {
            return await _dbContext.ServiceNowConnections.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<ServiceNowConnection>> GetAllAsync()
        {
            return await _dbContext.ServiceNowConnections.AsNoTracking().ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _dbContext.ServiceNowConnections.AnyAsync(x => x.Name == name);
        }

        public async Task UpdateAsync(ServiceNowConnection connection)
        {
            _dbContext.ServiceNowConnections.Update(connection);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(ServiceNowConnection connection)
        {
            _dbContext.ServiceNowConnections.Remove(connection);

            await _dbContext.SaveChangesAsync();
        }
    }
}