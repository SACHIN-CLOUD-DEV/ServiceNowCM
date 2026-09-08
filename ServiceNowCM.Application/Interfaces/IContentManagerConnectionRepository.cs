using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Interfaces
{
    public interface IContentManagerConnectionRepository
    {
        Task AddAsync(ContentManagerConnection connection);

        Task<ContentManagerConnection?> GetByIdAsync(long id);

        Task<IReadOnlyList<ContentManagerConnection>> GetAllAsync();

        Task<bool> ExistsByNameAsync(string name);

        Task UpdateAsync(ContentManagerConnection connection);

        Task DeleteAsync(ContentManagerConnection connection);
    }
}
