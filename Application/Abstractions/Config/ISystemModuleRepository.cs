using Application.DTOs.SystemModules;
using Domain.Entities;

namespace Application.Abstractions.Config
{
    public interface ISystemModuleRepository
    {
        // ===================================================================
        // MÓDULOS - QUERIES
        // ===================================================================

        /// <summary>
        /// Obtener todos los módulos con sus submódulos
        /// </summary>
        Task<ModulesListResponseDto> GetAllModulesAsync(bool includeInactive = false, bool includeSubmodules = true);

        /// <summary>
        /// Obtener un módulo por ID con sus submódulos
        /// </summary>
        Task<ModuleResponseDto?> GetModuleByIdAsync(int moduleId);

        // ===================================================================
        // MÓDULOS - COMMANDS
        // ===================================================================

        /// <summary>
        /// Crear un nuevo módulo
        /// </summary>
        Task<SystemModule> CreateModuleAsync(SystemModule module);

        /// <summary>
        /// Actualizar un módulo existente
        /// </summary>
        Task<bool> UpdateModuleAsync(int moduleId, SystemModule module);

        /// <summary>
        /// Eliminar un módulo (soft delete)
        /// </summary>
        Task<bool> DeleteModuleAsync(int moduleId);

        /// <summary>
        /// Verificar si un módulo existe
        /// </summary>
        Task<bool> ModuleExistsAsync(int moduleId);

        // ===================================================================
        // SUBMÓDULOS - QUERIES
        // ===================================================================

        /// <summary>
        /// Obtener todos los submódulos de un módulo
        /// </summary>
        Task<SubmodulesListResponseDto> GetSubmodulesByModuleAsync(int moduleId, bool includeInactive = false);

        /// <summary>
        /// Obtener un submódulo por ID
        /// </summary>
        Task<SubmoduleResponseDto?> GetSubmoduleByIdAsync(int submoduleId);

        // ===================================================================
        /// SUBMÓDULOS - COMMANDS
        // ===================================================================

        /// <summary>
        /// Crear un nuevo submódulo
        /// </summary>
        Task<SystemSubmodule> CreateSubmoduleAsync(SystemSubmodule submodule);

        /// <summary>
        /// Actualizar un submódulo existente
        /// </summary>
        Task<bool> UpdateSubmoduleAsync(int submoduleId, SystemSubmodule submodule);

        /// <summary>
        /// Eliminar un submódulo (soft delete)
        /// </summary>
        Task<bool> DeleteSubmoduleAsync(int submoduleId);

        /// <summary>
        /// Verificar si un submódulo existe
        /// </summary>
        Task<bool> SubmoduleExistsAsync(int submoduleId);
    }
}
