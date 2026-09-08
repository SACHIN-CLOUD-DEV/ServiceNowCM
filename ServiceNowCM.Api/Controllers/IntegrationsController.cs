using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Services;
using ServiceNowCM.Domain.Entities;

namespace ServiceNowCM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/integrations")]
    public class IntegrationsController : ControllerBase
    {
        private readonly IntegrationService _service;
        private readonly IntegrationSyncService _syncService;
        private readonly ILogger<IntegrationsController> _logger;

        public IntegrationsController(
            IntegrationService integrationService,
            IntegrationSyncService integrationSyncService,
            ILogger<IntegrationsController> logger)
        {
            _service = integrationService;
            _syncService = integrationSyncService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateIntegrationRequest request)
        {
            var integration = await _service.CreateAsync(request);

            var response = ToResponse(integration);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var integration = await _service.GetByIdAsync(id);

            if (integration == null)
            {
                return NotFound();
            }

            return Ok(ToResponse(integration));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var integrations = await _service.GetAllAsync();

            var response = integrations
                .Select(ToResponse)
                .ToList();

            return Ok(response);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, UpdateIntegrationRequest request)
        {
            var integration =
                await _service.UpdateAsync(
                    id,
                    request);

            return Ok(ToResponse(integration));
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);

            return NoContent();
        }

        [HttpPost("{id:long}/preview")]
        public async Task<IActionResult> Preview(long id, [FromQuery] int offset = 0, CancellationToken cancellationToken = default)
        {
            var result =
                await _service.PreviewAsync(
                    id,
                    offset,
                    cancellationToken);

            return Ok(result);
        }

        [HttpPost("{id:long}/sync")]
        public async Task<IActionResult> Sync(
            long id,
            CancellationToken cancellationToken)
        {
            var result =
                await _syncService.SyncAsync(
                    id,
                    cancellationToken);

            return Ok(result);
        }

        [HttpPost("{id:long}/resume")]
        [ProducesResponseType(typeof(SyncResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SyncResult>> ResumeSync(
    long id,
    CancellationToken cancellationToken)
        {
            try
            {
                var result =
                    await _syncService.ResumeAsync(
                        id,
                        cancellationToken);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while resuming integration {IntegrationId}",
                    id);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "An unexpected error occurred while resuming the synchronization."
                    });
            }
        }

        private static IntegrationResponse ToResponse(IntegrationConfiguration integration)
        {
            return new IntegrationResponse
            {
                Id = integration.Id,
                Name = integration.Name,
                ServiceNowConnectionId =integration.ServiceNowConnectionId,
                TableName = integration.TableName,
                EncodedQuery = integration.EncodedQuery,
                PageSize = integration.PageSize,
                ProcessingBatchSize =integration.ProcessingBatchSize,
                DisplayValues =integration.DisplayValues,
                ExcludeReferenceLinks = integration.ExcludeReferenceLinks,
                IsActive = integration.IsActive,
                CreatedAtUtc = integration.CreatedAtUtc,
                ModifiedAtUtc = integration.ModifiedAtUtc,
                ContentManagerConnectionId = integration.ContentManagerConnectionId,
                ContentManagerRecordTypeUri = integration.ContentManagerRecordTypeUri,
                ContentManagerRecordTypeName = integration.ContentManagerRecordTypeName,

                Fields = integration.Fields
                .OrderBy(x => x.DisplayOrder)
                .Select(field => new IntegrationFieldResponse
                {
                    Id = field.Id,
                    SourceFieldName = field.SourceFieldName,
                    SourceFieldLabel = field.SourceFieldLabel,
                    SourceDataType = field.SourceFieldDataType,
                    DisplayOrder = field.DisplayOrder,

                    TargetFieldName = field.TargetFieldName,
                    TargetFieldType = field.TargetFieldType,
                    TargetFieldUri = field.TargetFieldUri
                })
                .ToList()
            };
        }
    }
}