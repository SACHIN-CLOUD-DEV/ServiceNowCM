using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Application.Tests.Fakes
{
    public class FakeServiceNowClient : IServiceNowClient
    {
        public Task<ConnectionTestResult> TestConnectionAsync(
            ServiceNowConnection connection,
            string secret,
            CancellationToken cancellationToken = default)
        {
            var result = new ConnectionTestResult
            {
                Success = true,
                Message = "Fake ServiceNow connection successful.",
                StatusCode = 200,
                DurationMilliseconds = 10
            };

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ServiceNowTableInfo>> GetTablesAsync(
            ServiceNowConnection connection,
            string secret,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ServiceNowTableInfo> tables =
                new List<ServiceNowTableInfo>
                {
            new ServiceNowTableInfo
            {
                Name = "incident",
                Label = "Incident"
            },
            new ServiceNowTableInfo
            {
                Name = "sc_request",
                Label = "Request"
            }
                };

            return Task.FromResult(tables);
        }

        public Task<IReadOnlyList<ServiceNowFieldInfo>> GetFieldsAsync(
            ServiceNowConnection connection,
            string secret,
            string tableName,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ServiceNowFieldInfo> fields =
                new List<ServiceNowFieldInfo>
                {
            new ServiceNowFieldInfo
            {
                Name = "sys_id",
                Label = "Sys ID",
                DataType = "string",
                IsReference = false
            },
            new ServiceNowFieldInfo
            {
                Name = "number",
                Label = "Number",
                DataType = "string",
                IsReference = false
            },
            new ServiceNowFieldInfo
            {
                Name = "short_description",
                Label = "Short Description",
                DataType = "string",
                IsReference = false
            }
                };

            return Task.FromResult(fields);
        }

        public Task<ServiceNowPageResult> FetchPageAsync(
            ServiceNowConnection connection,
            string secret,
            ServiceNowQueryOptions options,
            CancellationToken cancellationToken = default)
        {
            var result = new ServiceNowPageResult
            {
                Offset = options.Offset,
                PageSize = options.PageSize,
                ReturnedCount = 0,
                HasMore = false
            };

            return Task.FromResult(result);
        }
    }
}