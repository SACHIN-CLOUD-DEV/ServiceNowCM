using Microsoft.AspNetCore.Mvc;
using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Services;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/contentmanager/connections")]
    public class ContentManagerConnectionsController : ControllerBase
    {
        private readonly ContentManagerConnectionService _service;

        public ContentManagerConnectionsController(
            ContentManagerConnectionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateContentManagerConnectionRequest request)
        {
            var connection =
                await _service.CreateAsync(request);

            var response = ToResponse(connection);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var connection =
                await _service.GetByIdAsync(id);

            if (connection == null)
            {
                return NotFound();
            }

            return Ok(ToResponse(connection));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var connections =
                await _service.GetAllAsync();

            return Ok(
                connections
                    .Select(ToResponse)
                    .ToList());
        }

        [HttpPost("{id:long}/test")]
        public async Task<IActionResult> TestConnection(
            long id,
            CancellationToken cancellationToken)
        {
            var result =
                await _service.TestConnectionAsync(
                    id,
                    cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:long}/record-types")]
        public async Task<IActionResult> GetRecordTypes(
            long id,
            CancellationToken cancellationToken)
        {
            var recordTypes =
                await _service.GetRecordTypesAsync(
                    id,
                    cancellationToken);

            return Ok(recordTypes);
        }

        [HttpGet("{id:int}/record-types/{recordTypeUri:long}/fields")]
        public async Task<ActionResult<IReadOnlyList<ContentManagerFieldDto>>> GetFields(
            int id,
            long recordTypeUri,
            CancellationToken cancellationToken)
        {
            var fields =
                await _service.GetFieldsAsync(
                    id,
                    recordTypeUri,
                    cancellationToken);

            return Ok(fields);
        }

        private static ContentManagerConnectionResponse ToResponse(ContentManagerConnection connection)
        {
            return new ContentManagerConnectionResponse
            {
                Id = connection.Id,
                Name = connection.Name,
                WorkgroupServerName =
                    connection.WorkgroupServerName,
                WorkgroupServerPort =
                    connection.WorkgroupServerPort,
                DatasetId = connection.DatasetId,
                AuthenticationType =
                    connection.AuthenticationType,
                Username = connection.Username,
                IsActive = connection.IsActive,
                CreatedAtUtc = connection.CreatedAtUtc,
                ModifiedAtUtc = connection.ModifiedAtUtc
            };
        }

        
    }
}