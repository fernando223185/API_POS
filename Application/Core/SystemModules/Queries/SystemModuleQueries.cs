using Application.DTOs.SystemModules;
using MediatR;

namespace Application.Core.SystemModules.Queries
{
    // ===================================================================
    // QUERIES PARA MÓDULOS
    // ===================================================================

    /// <summary>
    /// Query para obtener todos los módulos con sus submódulos
    /// </summary>
    public class GetAllModulesQuery : IRequest<ModulesListResponseDto>
    {
        public bool IncludeInactive { get; set; }

        public GetAllModulesQuery(bool includeInactive = false)
        {
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query para obtener un módulo por ID con sus submódulos
    /// </summary>
    public class GetModuleByIdQuery : IRequest<ModuleResponseDto?>
    {
        public int ModuleId { get; set; }

        public GetModuleByIdQuery(int moduleId)
        {
            ModuleId = moduleId;
        }
    }

    // ===================================================================
    // QUERIES PARA SUBMÓDULOS
    // ===================================================================

    /// <summary>
    /// Query para obtener todos los submódulos de un módulo
    /// </summary>
    public class GetSubmodulesByModuleQuery : IRequest<SubmodulesListResponseDto>
    {
        public int ModuleId { get; set; }
        public bool IncludeInactive { get; set; }

        public GetSubmodulesByModuleQuery(int moduleId, bool includeInactive = false)
        {
            ModuleId = moduleId;
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query para obtener un submódulo por ID
    /// </summary>
    public class GetSubmoduleByIdQuery : IRequest<SubmoduleResponseDto?>
    {
        public int SubmoduleId { get; set; }

        public GetSubmoduleByIdQuery(int submoduleId)
        {
            SubmoduleId = submoduleId;
        }
    }
}
