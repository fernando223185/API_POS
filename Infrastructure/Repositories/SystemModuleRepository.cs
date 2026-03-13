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

        public async Task<ModulesListResponseDto> GetAllModulesAsync(bool includeInactive = false, bool includeSubmodules = true)
        {
            var query = _context.Modules
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(m => m.IsActive);
            }

            if (includeSubmodules)
            {
                query = query.Include(m => m.Submodules.Where(s => includeInactive || s.IsActive));
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

        public async Task<ModuleResponseDto?> GetModuleByIdAsync(int id)
        {
            var module = await _context.Modules
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
                    Submodules = m.Submodules
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
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return module;
        }

        // ===================================================================
        // MÓDULOS - COMMANDS
        // ===================================================================

        public async Task<SystemModule> CreateModuleAsync(SystemModule module)
        {
            _context.Modules.Add(module);
            await _context.SaveChangesAsync();
            return module;
        }

        public async Task<bool> UpdateModuleAsync(int moduleId, SystemModule updatedModule)
        {
            var existing = await _context.Modules.FindAsync(moduleId);
            if (existing == null)
                return false;

            existing.Name = updatedModule.Name;
            existing.Description = updatedModule.Description;
            existing.Path = updatedModule.Path;
            existing.Icon = updatedModule.Icon;
            existing.Order = updatedModule.Order;
            existing.IsActive = updatedModule.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteModuleAsync(int moduleId)
        {
            var module = await _context.Modules
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
            return await _context.Modules.AnyAsync(m => m.Id == moduleId);
        }

        // ===================================================================
        // SUBMÓDULOS - QUERIES
        // ===================================================================

        public async Task<SubmodulesListResponseDto> GetSubmodulesByModuleAsync(int moduleId, bool includeInactive = false)
        {
            var module = await _context.Modules
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

            var query = _context.Submodules
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
            var submodule = await _context.Submodules
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
            _context.Submodules.Add(submodule);
            await _context.SaveChangesAsync();
            return submodule;
        }

        public async Task<bool> UpdateSubmoduleAsync(int submoduleId, SystemSubmodule updatedSubmodule)
        {
            var existing = await _context.Submodules.FindAsync(submoduleId);
            if (existing == null)
                return false;

            existing.Name = updatedSubmodule.Name;
            existing.Description = updatedSubmodule.Description;
            existing.Path = updatedSubmodule.Path;
            existing.Icon = updatedSubmodule.Icon;
            existing.Order = updatedSubmodule.Order;
            existing.Color = updatedSubmodule.Color;
            existing.IsActive = updatedSubmodule.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSubmoduleAsync(int submoduleId)
        {
            var submodule = await _context.Submodules.FindAsync(submoduleId);
            if (submodule == null)
                return false;

            // Soft delete
            submodule.IsActive = false;
            submodule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmoduleExistsAsync(int submoduleId)
        {
            return await _context.Submodules.AnyAsync(s => s.Id == submoduleId);
        }
    }
}
