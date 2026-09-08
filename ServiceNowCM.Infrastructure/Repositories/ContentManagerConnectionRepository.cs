using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Infrastructure.Persistence;

namespace ServiceNowCM.Infrastructure.Repositories
{
    public class ContentManagerConnectionRepository: IContentManagerConnectionRepository
    {
        private readonly ServiceNowCMDbContext _dbContext;

        public ContentManagerConnectionRepository(ServiceNowCMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ContentManagerConnection connection)
        {
            await _dbContext.ContentManagerConnections.AddAsync(connection);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<ContentManagerConnection?> GetByIdAsync(long id)
        {
            return await _dbContext.ContentManagerConnections.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<ContentManagerConnection>>GetAllAsync()
        {
            return await _dbContext.ContentManagerConnections
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _dbContext.ContentManagerConnections.AnyAsync(x => x.Name == name);
        }

        public async Task UpdateAsync(ContentManagerConnection connection)
        {
            _dbContext.ContentManagerConnections.Update(connection);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            ContentManagerConnection connection)
        {
            _dbContext.ContentManagerConnections.Remove(connection);

            await _dbContext.SaveChangesAsync();
        }
    }
}