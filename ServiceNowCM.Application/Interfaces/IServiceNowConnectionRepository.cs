using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Interfaces
{
    public interface IServiceNowConnectionRepository
    {
        Task AddAsync(ServiceNowConnection connection);

        Task<ServiceNowConnection?> GetByIdAsync(long id);

        Task<IReadOnlyList<ServiceNowConnection>> GetAllAsync();

        Task<bool> ExistsByNameAsync(string name);

        Task UpdateAsync(ServiceNowConnection connection);

        Task DeleteAsync(ServiceNowConnection connection);
    }
}
