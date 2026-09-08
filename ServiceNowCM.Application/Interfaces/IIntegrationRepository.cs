using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Interfaces
{
    public interface IIntegrationRepository
    {
        Task AddAsync(IntegrationConfiguration integration);

        Task<IntegrationConfiguration?> GetByIdAsync(long id);

        Task<IReadOnlyList<IntegrationConfiguration>> GetAllAsync();

        Task<bool> ExistsByNameAsync(string name);

        Task UpdateAsync(IntegrationConfiguration integration);

        Task DeleteAsync(IntegrationConfiguration integration);
    }
}