using Application.Abstractions.Config;
using Application.DTOs.SystemModules;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SystemModuleRepository : ISystemModuleRepository
    {
        private readonly POSDbContext _context;

        public SystemModuleRepository(POSDbContext context)
        {
            _context = context;
        }

        // ===================================================================
        // MÓDULOS - QUERIES
        // ===================================================================

        public async Task<ModulesListResponseDto> GetAllModulesAsync(bool includeInactive = false)
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

            return new ModulesListResponseDto
            {
                Message = "Módulos obtenidos exitosamente",
                Error = 0,
                Modules = modules,
                TotalModules = modules.Count,
                TotalSubmodules = modules.Sum(m => m.Submodules.Count)
            };
        }

        public async Task<ModuleResponseDto?> GetModuleByIdAsync(int moduleId)
        {
            var module = await _context.SystemModules
                .Include(m => m.Submodules)
                .Where(m => m.Id == moduleId)
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

            return module;
        }

        // ===================================================================
        // MÓDULOS - COMMANDS
        // ===================================================================

        public async Task<SystemModule> CreateModuleAsync(SystemModule module)
        {
            _context.SystemModules.Add(module);
            await _context.SaveChangesAsync();
            return module;
        }

        public async Task<SystemModule?> UpdateModuleAsync(int moduleId, SystemModule module)
        {
            var existing = await _context.SystemModules.FindAsync(moduleId);
            if (existing == null) return null;

            existing.Name = module.Name;
            existing.Description = module.Description;
            existing.Path = module.Path;
            existing.Icon = module.Icon;
            existing.Order = module.Order;
            existing.IsActive = module.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteModuleAsync(int moduleId)
        {
            var module = await _context.SystemModules
                .Include(m => m.Submodules)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null) return false;

            // Soft delete
            module.IsActive = false;
            module.UpdatedAt = DateTime.UtcNow;

            // También desactivar submódulos
            foreach (var submodule in module.Submodules)
            {
                submodule.IsActive = false;
                submodule.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ModuleExistsAsync(int moduleId)
        {
            return await _context.SystemModules.AnyAsync(m => m.Id == moduleId);
        }

        // ===================================================================
        // SUBMÓDULOS - QUERIES
        // ===================================================================

        public async Task<SubmodulesListResponseDto> GetSubmodulesByModuleAsync(int moduleId, bool includeInactive = false)
        {
            var module = await _context.SystemModules
                .Where(m => m.Id == moduleId)
                .Select(m => new { m.Id, m.Name })
                .FirstOrDefaultAsync();

            if (module == null)
            {
                return new SubmodulesListResponseDto
                {
                    Message = "Módulo no encontrado",
                    Error = 1,
                    Submodules = new List<SubmoduleResponseDto>()
                };
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

            return new SubmodulesListResponseDto
            {
                Message = "Submódulos obtenidos exitosamente",
                Error = 0,
                ModuleId = moduleId,
                ModuleName = module.Name,
                Submodules = submodules,
                TotalSubmodules = submodules.Count
            };
        }

        public async Task<SubmoduleResponseDto?> GetSubmoduleByIdAsync(int submoduleId)
        {
            var submodule = await _context.SystemSubmodules
                .Where(s => s.Id == submoduleId)
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

            return submodule;
        }

        // ===================================================================
        // SUBMÓDULOS - COMMANDS
        // ===================================================================

        public async Task<SystemSubmodule> CreateSubmoduleAsync(SystemSubmodule submodule)
        {
            _context.SystemSubmodules.Add(submodule);
            await _context.SaveChangesAsync();
            return submodule;
        }

        public async Task<SystemSubmodule?> UpdateSubmoduleAsync(int submoduleId, SystemSubmodule submodule)
        {
            var existing = await _context.SystemSubmodules.FindAsync(submoduleId);
            if (existing == null) return null;

            existing.Name = submodule.Name;
            existing.Description = submodule.Description;
            existing.Path = submodule.Path;
            existing.Icon = submodule.Icon;
            existing.Order = submodule.Order;
            existing.Color = submodule.Color;
            existing.IsActive = submodule.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteSubmoduleAsync(int submoduleId)
        {
            var submodule = await _context.SystemSubmodules.FindAsync(submoduleId);
            if (submodule == null) return false;

            // Soft delete
            submodule.IsActive = false;
            submodule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmoduleExistsAsync(int submoduleId)
        {
            return await _context.SystemSubmodules.AnyAsync(s => s.Id == submoduleId);
        }
    }
}
