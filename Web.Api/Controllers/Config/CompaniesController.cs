using Application.Core.Company.Commands;
using Application.Core.Company.Queries;
using Application.Abstractions.Config;
using Application.Abstractions.Storage;
using Application.DTOs.Company;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// Controlador para gestión de empresas
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICompanyRepository _companyRepository;
        private readonly IS3StorageService _s3StorageService;
        private readonly IConfiguration _configuration;
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        private static readonly string[] AllowedImageContentTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"];
        private const long MaxLogoFileSize = 5 * 1024 * 1024;
        private const string CompanyLogosFolder = "companies/logos";

        public CompaniesController(IMediator mediator, ICompanyRepository companyRepository, IS3StorageService s3StorageService, IConfiguration configuration)
        {
            _mediator = mediator;
            _companyRepository = companyRepository;
            _s3StorageService = s3StorageService;
            _configuration = configuration;
        }

        /// <summary>
        /// Crear nueva empresa
        /// </summary>
        [HttpPost]
        [RequirePermission("Configuracion", "Create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCompany([FromForm] CreateCompanyDto request, [FromForm] IFormFile? logoFile)
        {
            string? savedLogoUrl = null;
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Creando empresa - Usuario: {userName}, Razón Social: {request.LegalName}");

                if (logoFile is not null)
                {
                    var logoValidation = ValidateLogoFile(logoFile);
                    if (logoValidation is not null)
                        return logoValidation;

                    savedLogoUrl = await SaveCompanyLogoAsync(logoFile, request.TradeName, userId);
                    request.LogoUrl = savedLogoUrl;
                }

                var command = new CreateCompanyCommand(request, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Empresa creada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                await DeleteCompanyLogoIfManagedAsync(savedLogoUrl, null);
                Console.WriteLine($"? Validation error: {ex.Message}");
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                await DeleteCompanyLogoIfManagedAsync(savedLogoUrl, null);
                Console.WriteLine($"? Error creating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar empresa existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("Configuracion", "Edit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCompany(int id, [FromForm] UpdateCompanyDto request, [FromForm] IFormFile? logoFile)
        {
            string? savedLogoUrl = null;
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Actualizando empresa {id} - Usuario: {userName}");

                var existingCompany = await _companyRepository.GetByIdAsync(id);
                if (existingCompany == null)
                {
                    return NotFound(new { message = $"Empresa con ID {id} no encontrada", error = 1 });
                }

                var previousLogoUrl = existingCompany.LogoUrl;

                if (logoFile is not null)
                {
                    var logoValidation = ValidateLogoFile(logoFile);
                    if (logoValidation is not null)
                        return logoValidation;

                    savedLogoUrl = await SaveCompanyLogoAsync(logoFile, request.TradeName, userId);
                    request.LogoUrl = savedLogoUrl;
                }
                else if (request.LogoUrl is null)
                {
                    request.LogoUrl = previousLogoUrl;
                }

                var command = new UpdateCompanyCommand(id, request, userId);
                var result = await _mediator.Send(command);

                if (logoFile is not null)
                    await DeleteCompanyLogoIfManagedAsync(previousLogoUrl, request.LogoUrl);
                else if (string.IsNullOrWhiteSpace(request.LogoUrl))
                    await DeleteCompanyLogoIfManagedAsync(previousLogoUrl, request.LogoUrl);

                return Ok(new
                {
                    message = "Empresa actualizada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                await DeleteCompanyLogoIfManagedAsync(savedLogoUrl, null);
                Console.WriteLine($"? Not found: {ex.Message}");
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                await DeleteCompanyLogoIfManagedAsync(savedLogoUrl, null);
                Console.WriteLine($"? Error updating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        private IActionResult? ValidateLogoFile(IFormFile file)
        {
            if (file.Length == 0)
            {
                return BadRequest(new { message = "El archivo de logo está vacío", error = 1 });
            }

            if (file.Length > MaxLogoFileSize)
            {
                return BadRequest(new
                {
                    message = "El logo es demasiado grande. Tamaño máximo: 5MB",
                    error = 1,
                    fileSize = file.Length,
                    maxSize = MaxLogoFileSize
                });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message = "Formato de logo no válido. Solo se permiten: JPG, JPEG, PNG, GIF, WebP",
                    error = 1,
                    allowedFormats = AllowedImageExtensions
                });
            }

            if (!string.IsNullOrWhiteSpace(file.ContentType)
                && !AllowedImageContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(new
                {
                    message = "Tipo de archivo no permitido para el logo",
                    error = 1,
                    contentType = file.ContentType
                });
            }

            return null;
        }

        private async Task<string> SaveCompanyLogoAsync(IFormFile file, string tradeName, int userId)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeTradeName = BuildSlug(string.IsNullOrWhiteSpace(tradeName) ? $"company-{userId}" : tradeName);
            var fileName = $"{safeTradeName}-{Guid.NewGuid():N}{extension}";
            var companiesBucket = GetCompaniesBucketName();

            await using var stream = file.OpenReadStream();
            var key = await _s3StorageService.UploadImageAsync(
                stream,
                fileName,
                file.ContentType,
                CompanyLogosFolder,
                companiesBucket);

            return _s3StorageService.GetPublicUrl(key, companiesBucket);
        }

        private async Task DeleteCompanyLogoIfManagedAsync(string? oldLogoUrl, string? newLogoUrl)
        {
            if (string.IsNullOrWhiteSpace(oldLogoUrl))
                return;

            if (string.Equals(oldLogoUrl, newLogoUrl, StringComparison.OrdinalIgnoreCase))
                return;

            var companiesBucket = GetCompaniesBucketName();
            var publicPrefix = _s3StorageService.GetPublicUrl(string.Empty, companiesBucket);
            if (!oldLogoUrl.StartsWith(publicPrefix, StringComparison.OrdinalIgnoreCase))
                return;

            var oldKey = oldLogoUrl.Replace(publicPrefix, string.Empty, StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(oldKey))
                return;

            await _s3StorageService.DeleteImageAsync(oldKey, companiesBucket);
        }

        private string GetCompaniesBucketName() =>
            _configuration["AWS:S3:CompaniesBucketName"] ?? "expanda-companies";

        private static string BuildSlug(string value)
        {
            var chars = value
                .Trim()
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray();

            var slug = new string(chars);
            while (slug.Contains("--", StringComparison.Ordinal))
                slug = slug.Replace("--", "-", StringComparison.Ordinal);

            return slug.Trim('-');
        }

        /// <summary>
        /// Obtener empresa por ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("Configuracion", "View")]
        public async Task<IActionResult> GetCompany(int id)
        {
            try
            {
                var query = new GetCompanyByIdQuery(id);
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Empresa obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener todas las empresas activas
        /// </summary>
        [HttpGet("active")]
        [RequirePermission("Configuracion", "View")]
        public async Task<IActionResult> GetActiveCompanies()
        {
            try
            {
                var query = new GetAllActiveCompaniesQuery();
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    message = "Empresas activas obtenidas exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting active companies: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresas activas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener empresas paginadas con filtros
        /// </summary>
        [HttpGet]
        [RequirePermission("Configuracion", "View")]
        public async Task<IActionResult> GetCompanies(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                Console.WriteLine($"?? Getting companies - Page: {page}, PageSize: {pageSize}, Active: {isActive}, Search: {searchTerm}");

                var query = new GetCompaniesPagedQuery(page, pageSize, isActive, searchTerm);
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting companies: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresas",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener empresa principal/matriz
        /// </summary>
        [HttpGet("main")]
        [RequirePermission("Configuracion", "View")]
        public async Task<IActionResult> GetMainCompany()
        {
            try
            {
                var query = new GetMainCompanyQuery();
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "No hay empresa principal configurada",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Empresa principal obtenida exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error getting main company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener empresa principal",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Desactivar empresa (baja lógica)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [RequirePermission("Configuracion", "Edit")]
        public async Task<IActionResult> DeactivateCompany(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Desactivando empresa {id} - Usuario: {userName}");

                var command = new DeactivateCompanyCommand(id, userId);
                await _mediator.Send(command);

                return Ok(new
                {
                    message = "Empresa desactivada exitosamente",
                    error = 0
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error deactivating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al desactivar empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Reactivar empresa
        /// </summary>
        [HttpPut("{id}/reactivate")]
        [RequirePermission("Configuracion", "Edit")]
        public async Task<IActionResult> ReactivateCompany(int id)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Reactivando empresa {id} - Usuario: {userName}");

                var command = new ReactivateCompanyCommand(id, userId);
                await _mediator.Send(command);

                return Ok(new
                {
                    message = "Empresa reactivada exitosamente",
                    error = 0
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error reactivating company: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al reactivar empresa",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar configuración fiscal (certificados SAT, folios)
        /// </summary>
        [HttpPut("{id}/fiscal-config")]
        [RequirePermission("Configuracion", "Edit")]
        public async Task<IActionResult> UpdateFiscalConfig(int id, [FromBody] UpdateCompanyFiscalConfigDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int? ?? 0;
                var userName = HttpContext.Items["UserName"] as string ?? "Unknown";

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado", error = 1 });
                }

                Console.WriteLine($"?? Actualizando configuración fiscal empresa {id} - Usuario: {userName}");

                var command = new UpdateCompanyFiscalConfigCommand(id, request, userId);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Configuración fiscal actualizada exitosamente",
                    error = 0,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error updating fiscal config: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar configuración fiscal",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}
