using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Interfaces
{
    public interface IServiceNowClient
    {
        Task<ConnectionTestResult> TestConnectionAsync(
            ServiceNowConnection connection,
            string secret,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ServiceNowTableInfo>> GetTablesAsync(
            ServiceNowConnection connection,
            string secret,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ServiceNowFieldInfo>> GetFieldsAsync(
            ServiceNowConnection connection,
            string secret,
            string tableName,
            CancellationToken cancellationToken = default);

        Task<ServiceNowPageResult> FetchPageAsync(
            ServiceNowConnection connection,
            string secret,
            ServiceNowQueryOptions options,
            CancellationToken cancellationToken = default);
    }
}