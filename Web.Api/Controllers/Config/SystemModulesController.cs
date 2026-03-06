using Application.DTOs.SystemModules;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Authorization;
using Domain.Entities;

namespace Web.Api.Controllers.Config
{
    /// <summary>
    /// CRUD completo para gestionar módulos y submódulos del sistema
    /// </summary>
    [Route("api/Modules")]
    [ApiController]
    public class AppModulesController : ControllerBase
    {
        private readonly POSDbContext _context;

        public AppModulesController(POSDbContext context)
        {
            _context = context;
        }

        #region ?? MÓDULOS - CRUD

        /// <summary>
        /// ?? Obtener todos los módulos con sus submódulos
        /// </summary>
        [HttpGet]
        [RequireAuthentication]
        public async Task<IActionResult> GetAllModules([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = _context.SystemModules
                    .Include(m => m.Submodules.Where(s => includeInactive || s.IsActive))
                    .AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(m => m.IsActive);
                }

                var modules = await query
                    .OrderBy(m => m.Order)
                    .Select(m => new ModuleResponseDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Path = m.Path,
                        Icon = m.Icon,
                        Order = m.Order,
                        IsActive = m.IsActive,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        Submodules = m.Submodules.OrderBy(s => s.Order).Select(s => new SubmoduleResponseDto
                        {
                            Id = s.Id,
                            ModuleId = s.ModuleId,
                            Name = s.Name,
                            Description = s.Description,
                            Path = s.Path,
                            Icon = s.Icon,
                            Order = s.Order,
                            Color = s.Color,
                            IsActive = s.IsActive,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ModulesListResponseDto
                {
                    Message = "Módulos obtenidos exitosamente",
                    Error = 0,
                    Modules = modules,
                    TotalModules = modules.Count,
                    TotalSubmodules = modules.Sum(m => m.Submodules.Count)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener módulos: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener módulos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un módulo por ID con sus submódulos
        /// </summary>
        [HttpGet("{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetModuleById(int id)
        {
            try
            {
                var module = await _context.SystemModules
                    .Include(m => m.Submodules)
                    .Where(m => m.Id == id)
                    .Select(m => new ModuleResponseDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Path = m.Path,
                        Icon = m.Icon,
                        Order = m.Order,
                        IsActive = m.IsActive,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        Submodules = m.Submodules.OrderBy(s => s.Order).Select(s => new SubmoduleResponseDto
                        {
                            Id = s.Id,
                            ModuleId = s.ModuleId,
                            Name = s.Name,
                            Description = s.Description,
                            Path = s.Path,
                            Icon = s.Icon,
                            Order = s.Order,
                            Color = s.Color,
                            IsActive = s.IsActive,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (module == null)
                {
                    return NotFound(new
                    {
                        message = "Módulo no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Módulo obtenido exitosamente",
                    error = 0,
                    data = module
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ? Crear un nuevo módulo
        /// </summary>
        [HttpPost]
        [RequirePermission("Configuration", "ManagePermissions")]
        public async Task<IActionResult> CreateModule([FromBody] CreateUpdateModuleDto dto)
        {
            try
            {
                // Verificar si ya existe un módulo con ese ID
                var exists = await _context.SystemModules.AnyAsync(m => m.Id == dto.Id);
                if (exists)
                {
                    return BadRequest(new
                    {
                        message = $"Ya existe un módulo con el ID {dto.Id}",
                        error = 1
                    });
                }

                var module = new SystemModule
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Path = dto.Path,
                    Icon = dto.Icon,
                    Order = dto.Order,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SystemModules.Add(module);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Módulo creado: {module.Name} (ID: {module.Id})");

                return Ok(new
                {
                    message = "Módulo creado exitosamente",
                    error = 0,
                    data = new ModuleResponseDto
                    {
                        Id = module.Id,
                        Name = module.Name,
                        Description = module.Description,
                        Path = module.Path,
                        Icon = module.Icon,
                        Order = module.Order,
                        IsActive = module.IsActive,
                        CreatedAt = module.CreatedAt,
                        Submodules = new List<SubmoduleResponseDto>()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Actualizar un módulo existente
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("Configuration", "ManagePermissions")]
        public async Task<IActionResult> UpdateModule(int id, [FromBody] CreateUpdateModuleDto dto)
        {
            try
            {
                var module = await _context.SystemModules.FindAsync(id);
                if (module == null)
                {
                    return NotFound(new
                    {
                        message = "Módulo no encontrado",
                        error = 1
                    });
                }

                module.Name = dto.Name;
                module.Description = dto.Description;
                module.Path = dto.Path;
                module.Icon = dto.Icon;
                module.Order = dto.Order;
                module.IsActive = dto.IsActive;
                module.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Console.WriteLine($"? Módulo actualizado: {module.Name} (ID: {module.Id})");

                return Ok(new
                {
                    message = "Módulo actualizado exitosamente",
                    error = 0,
                    data = new ModuleResponseDto
                    {
                        Id = module.Id,
                        Name = module.Name,
                        Description = module.Description,
                        Path = module.Path,
                        Icon = module.Icon,
                        Order = module.Order,
                        IsActive = module.IsActive,
                        CreatedAt = module.CreatedAt,
                        UpdatedAt = module.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar un módulo (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("Configuration", "ManagePermissions")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            try
            {
                var module = await _context.SystemModules
                    .Include(m => m.Submodules)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (module == null)
                {
                    return NotFound(new
                    {
                        message = "Módulo no encontrado",
                        error = 1
                    });
                }

                // Soft delete: marcar como inactivo
                module.IsActive = false;
                module.UpdatedAt = DateTime.UtcNow;

                // También desactivar todos los submódulos
                foreach (var submodule in module.Submodules)
                {
                    submodule.IsActive = false;
                    submodule.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"??? Módulo eliminado (soft): {module.Name} (ID: {module.Id})");

                return Ok(new
                {
                    message = "Módulo eliminado exitosamente",
                    error = 0,
                    moduleId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar módulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar módulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion

        #region ?? SUBMÓDULOS - CRUD

        /// <summary>
        /// ?? Obtener todos los submódulos de un módulo
        /// </summary>
        [HttpGet("{moduleId}/submodules")]
        [RequireAuthentication]
        public async Task<IActionResult> GetSubmodulesByModule(int moduleId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var module = await _context.SystemModules
                    .Where(m => m.Id == moduleId)
                    .Select(m => new { m.Id, m.Name })
                    .FirstOrDefaultAsync();

                if (module == null)
                {
                    return NotFound(new
                    {
                        message = "Módulo no encontrado",
                        error = 1
                    });
                }

                var query = _context.SystemSubmodules
                    .Where(s => s.ModuleId == moduleId);

                if (!includeInactive)
                {
                    query = query.Where(s => s.IsActive);
                }

                var submodules = await query
                    .OrderBy(s => s.Order)
                    .Select(s => new SubmoduleResponseDto
                    {
                        Id = s.Id,
                        ModuleId = s.ModuleId,
                        Name = s.Name,
                        Description = s.Description,
                        Path = s.Path,
                        Icon = s.Icon,
                        Order = s.Order,
                        Color = s.Color,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new SubmodulesListResponseDto
                {
                    Message = "Submódulos obtenidos exitosamente",
                    Error = 0,
                    ModuleId = moduleId,
                    ModuleName = module.Name,
                    Submodules = submodules,
                    TotalSubmodules = submodules.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener submódulos: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener submódulos",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Obtener un submódulo por ID
        /// </summary>
        [HttpGet("submodules/{id}")]
        [RequireAuthentication]
        public async Task<IActionResult> GetSubmoduleById(int id)
        {
            try
            {
                var submodule = await _context.SystemSubmodules
                    .Where(s => s.Id == id)
                    .Select(s => new SubmoduleResponseDto
                    {
                        Id = s.Id,
                        ModuleId = s.ModuleId,
                        Name = s.Name,
                        Description = s.Description,
                        Path = s.Path,
                        Icon = s.Icon,
                        Order = s.Order,
                        Color = s.Color,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (submodule == null)
                {
                    return NotFound(new
                    {
                        message = "Submódulo no encontrado",
                        error = 1
                    });
                }

                return Ok(new
                {
                    message = "Submódulo obtenido exitosamente",
                    error = 0,
                    data = submodule
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al obtener submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al obtener submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ? Crear un nuevo submódulo
        /// </summary>
        [HttpPost("submodules")]
        [RequirePermission("Configuration", "ManagePermissions")]
        public async Task<IActionResult> CreateSubmodule([FromBody] CreateUpdateSubmoduleDto dto)
        {
            try
            {
                // Verificar que el módulo padre existe
                var moduleExists = await _context.SystemModules.AnyAsync(m => m.Id == dto.ModuleId);
                if (!moduleExists)
                {
                    return BadRequest(new
                    {
                        message = $"El módulo padre con ID {dto.ModuleId} no existe",
                        error = 1
                    });
                }

                // Verificar si ya existe un submódulo con ese ID
                var exists = await _context.SystemSubmodules.AnyAsync(s => s.Id == dto.Id);
                if (exists)
                {
                    return BadRequest(new
                    {
                        message = $"Ya existe un submódulo con el ID {dto.Id}",
                        error = 1
                    });
                }

                var submodule = new SystemSubmodule
                {
                    Id = dto.Id,
                    ModuleId = dto.ModuleId,
                    Name = dto.Name,
                    Description = dto.Description,
                    Path = dto.Path,
                    Icon = dto.Icon,
                    Order = dto.Order,
                    Color = dto.Color,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SystemSubmodules.Add(submodule);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Submódulo creado: {submodule.Name} (ID: {submodule.Id})");

                return Ok(new
                {
                    message = "Submódulo creado exitosamente",
                    error = 0,
                    data = new SubmoduleResponseDto
                    {
                        Id = submodule.Id,
                        ModuleId = submodule.ModuleId,
                        Name = submodule.Name,
                        Description = submodule.Description,
                        Path = submodule.Path,
                        Icon = submodule.Icon,
                        Order = submodule.Order,
                        Color = submodule.Color,
                        IsActive = submodule.IsActive,
                        CreatedAt = submodule.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ?? Actualizar un submódulo existente
        /// </summary>
        [HttpPut("submodules/{id}")]
        [RequirePermission("Configuration", "ManagePermissions")]
        public async Task<IActionResult> UpdateSubmodule(int id, [FromBody] CreateUpdateSubmoduleDto dto)
        {
            try
            {
                var submodule = await _context.SystemSubmodules.FindAsync(id);
                if (submodule == null)
                {
                    return NotFound(new
                    {
                        message = "Submódulo no encontrado",
                        error = 1
                    });
                }

                submodule.Name = dto.Name;
                submodule.Description = dto.Description;
                submodule.Path = dto.Path;
                submodule.Icon = dto.Icon;
                submodule.Order = dto.Order;
                submodule.Color = dto.Color;
                submodule.IsActive = dto.IsActive;
                submodule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Console.WriteLine($"? Submódulo actualizado: {submodule.Name} (ID: {submodule.Id})");

                return Ok(new
                {
                    message = "Submódulo actualizado exitosamente",
                    error = 0,
                    data = new SubmoduleResponseDto
                    {
                        Id = submodule.Id,
                        ModuleId = submodule.ModuleId,
                        Name = submodule.Name,
                        Description = submodule.Description,
                        Path = submodule.Path,
                        Icon = submodule.Icon,
                        Order = submodule.Order,
                        Color = submodule.Color,
                        IsActive = submodule.IsActive,
                        CreatedAt = submodule.CreatedAt,
                        UpdatedAt = submodule.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al actualizar submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// ??? Eliminar un submódulo (soft delete)
        /// </summary>
        [HttpDelete("submodules/{id}")]
        [RequirePermission("Configuration", "ManagePermissions")]
        public async Task<IActionResult> DeleteSubmodule(int id)
        {
            try
            {
                var submodule = await _context.SystemSubmodules.FindAsync(id);
                if (submodule == null)
                {
                    return NotFound(new
                    {
                        message = "Submódulo no encontrado",
                        error = 1
                    });
                }

                // Soft delete: marcar como inactivo
                submodule.IsActive = false;
                submodule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Console.WriteLine($"??? Submódulo eliminado (soft): {submodule.Name} (ID: {submodule.Id})");

                return Ok(new
                {
                    message = "Submódulo eliminado exitosamente",
                    error = 0,
                    submoduleId = id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar submódulo: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al eliminar submódulo",
                    error = 2,
                    details = ex.Message
                });
            }
        }

        #endregion
    }
}
