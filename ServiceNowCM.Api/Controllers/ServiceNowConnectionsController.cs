using Microsoft.AspNetCore.Mvc;
using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Services;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/servicenow/connections")]
    public class ServiceNowConnectionsController : ControllerBase
    {
        private readonly ServiceNowConnectionService _service;

        public ServiceNowConnectionsController(ServiceNowConnectionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceNowConnectionRequest request)
        {
            var connection = await _service.CreateAsync(request);

            var response = new ServiceNowConnectionResponse
            {
                Id = connection.Id,
                Name = connection.Name,
                InstanceUrl = connection.InstanceUrl,
                AuthenticationType = connection.AuthenticationType,
                ClientId = connection.ClientId,
                Username = connection.Username,
                IsActive = connection.IsActive,
                CreatedAtUtc = connection.CreatedAtUtc,
                ModifiedAtUtc = connection.ModifiedAtUtc
            };

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var connections = await _service.GetAllAsync();

            var response = connections.Select(connection => new ServiceNowConnectionResponse
            {
                Id = connection.Id,
                Name = connection.Name,
                InstanceUrl = connection.InstanceUrl,
                AuthenticationType = connection.AuthenticationType,
                ClientId = connection.ClientId,
                Username = connection.Username,
                IsActive = connection.IsActive,
                CreatedAtUtc = connection.CreatedAtUtc,
                ModifiedAtUtc = connection.ModifiedAtUtc
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var connection = await _service.GetByIdAsync(id);

            if (connection == null)
            {
                return NotFound();
            }

            var response = new ServiceNowConnectionResponse
            {
                Id = connection.Id,
                Name = connection.Name,
                InstanceUrl = connection.InstanceUrl,
                AuthenticationType = connection.AuthenticationType,
                ClientId = connection.ClientId,
                Username = connection.Username,
                IsActive = connection.IsActive,
                CreatedAtUtc = connection.CreatedAtUtc,
                ModifiedAtUtc = connection.ModifiedAtUtc
            };

            return Ok(response);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, UpdateServiceNowConnectionRequest request)
        {
            var connection = await _service.UpdateAsync(id, request);

            var response = new ServiceNowConnectionResponse
            {
                Id = connection.Id,
                Name = connection.Name,
                InstanceUrl = connection.InstanceUrl,
                AuthenticationType = connection.AuthenticationType,
                ClientId = connection.ClientId,
                Username = connection.Username,
                IsActive = connection.IsActive,
                CreatedAtUtc = connection.CreatedAtUtc,
                ModifiedAtUtc = connection.ModifiedAtUtc
            };

            return Ok(response);
        }

        [HttpPost("{id:long}/enable")]
        public async Task<IActionResult> Enable(long id)
        {
            var connection =
                await _service.EnableAsync(id);

            var response = ToResponse(connection);

            return Ok(response);
        }

        [HttpPost("{id:long}/disable")]
        public async Task<IActionResult> Disable(long id)
        {
            var connection =
                await _service.DisableAsync(id);

            var response = ToResponse(connection);

            return Ok(response);
        }

        private static ServiceNowConnectionResponse ToResponse(ServiceNowConnection connection)
        {
            return new ServiceNowConnectionResponse
            {
                Id = connection.Id,
                Name = connection.Name,
                InstanceUrl = connection.InstanceUrl,
                AuthenticationType = connection.AuthenticationType,
                ClientId = connection.ClientId,
                Username = connection.Username,
                IsActive = connection.IsActive,
                CreatedAtUtc = connection.CreatedAtUtc,
                ModifiedAtUtc = connection.ModifiedAtUtc
            };
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);

            return NoContent();
        }

        [HttpPost("{id:long}/test")]
        public async Task<IActionResult> TestConnection(long id, CancellationToken cancellationToken)
        {
            var result = await _service.TestConnectionAsync(id, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:long}/tables")]
        public async Task<IActionResult> GetTables(long id, CancellationToken cancellationToken)
        {
            var tables =
                await _service.GetTablesAsync(
                    id,
                    cancellationToken);

            return Ok(tables);
        }

        [HttpGet("{id:long}/tables/{tableName}/fields")]
        public async Task<IActionResult> GetFields(long id, string tableName, CancellationToken cancellationToken)
        {
            var fields =
                await _service.GetFieldsAsync(
                    id,
                    tableName,
                    cancellationToken);

            return Ok(fields);
        }

        [HttpPost("{id:long}/preview")]
        public async Task<IActionResult> Preview(long id,ServiceNowQueryOptions options,CancellationToken cancellationToken)
        {
            var result =
                await _service.PreviewAsync(
                    id,
                    options,
                    cancellationToken);

            return Ok(result);
        }


    }


}
