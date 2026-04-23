using Application.Core.Backup;
using Application.DTOs.Backup;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// Gestión de respaldos de base de datos. Solo Super Admin.
    /// </summary>
    [Route("api/backups")]
    [ApiController]
    public class BackupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackupsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// GET /api/backups — Lista todos los respaldos
        /// </summary>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _mediator.Send(new GetAllBackupsQuery());
                return Ok(new { data = result, total = result.Count, error = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// GET /api/backups/{id} — Detalle de un respaldo
        /// </summary>
        [HttpGet("{id:int}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetBackupByIdQuery(id));
                if (result is null)
                    return NotFound(new { message = $"Backup {id} no encontrado", error = 1 });
                return Ok(new { data = result, error = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// POST /api/backups — Crea un nuevo respaldo y lo sube a S3
        /// </summary>
        [HttpPost]
        [RequireAuthentication]
        public async Task<IActionResult> Create([FromBody] CreateBackupDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value;
                int.TryParse(userIdClaim, out var userId);

                var result = await _mediator.Send(new CreateBackupCommand(dto.Notes, userId > 0 ? userId : null));
                return StatusCode(201, new { data = result, error = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// GET /api/backups/{id}/download — URL pre-firmada S3 (expira en 5 min)
        /// </summary>
        [HttpGet("{id:int}/download")]
        [RequireAuthentication]
        public async Task<IActionResult> GetDownloadUrl(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetBackupDownloadUrlQuery(id));
                return Ok(new { data = result, error = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }

        /// <summary>
        /// DELETE /api/backups/{id} — Elimina el registro y el archivo de S3
        /// </summary>
        [HttpDelete("{id:int}")]
        [RequireAuthentication]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mediator.Send(new DeleteBackupCommand(id));
                return Ok(new { message = "Backup eliminado correctamente", error = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, error = 2 });
            }
        }
    }
}
