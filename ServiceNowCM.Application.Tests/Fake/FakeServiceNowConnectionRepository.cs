using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Tests.Fakes
{
    public class FakeServiceNowConnectionRepository
        : IServiceNowConnectionRepository
    {
        private readonly List<ServiceNowConnection> _connections = new();

        public Task AddAsync(ServiceNowConnection connection)
        {
            _connections.Add(connection);

            return Task.CompletedTask;
        }

        public Task<ServiceNowConnection?> GetByIdAsync(long id)
        {
            var connection = _connections.FirstOrDefault(x => x.Id == id);

            return Task.FromResult(connection);
        }

        public Task<IReadOnlyList<ServiceNowConnection>> GetAllAsync()
        {
            IReadOnlyList<ServiceNowConnection> result = _connections;

            return Task.FromResult(result);
        }

        public Task<bool> ExistsByNameAsync(string name)
        {
            var exists = _connections.Any(
                x => x.Name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(exists);
        }

        public Task UpdateAsync(ServiceNowConnection connection)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ServiceNowConnection connection)
        {
            _connections.Remove(connection);

            return Task.CompletedTask;
        }
    }
}
