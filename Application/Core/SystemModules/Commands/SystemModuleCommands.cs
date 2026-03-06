using Application.DTOs.SystemModules;
using MediatR;

namespace Application.Core.SystemModules.Commands
{
    // ===================================================================
    // COMMANDS PARA MÓDULOS
    // ===================================================================

    /// <summary>
    /// Command para crear un nuevo módulo
    /// </summary>
    public class CreateModuleCommand : IRequest<ModuleCommandResponseDto>
    {
        public CreateModuleDto ModuleData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateModuleCommand(CreateModuleDto moduleData, int createdByUserId)
        {
            ModuleData = moduleData;
            CreatedByUserId = createdByUserId;
        }
    }

    /// <summary>
    /// Command para actualizar un módulo existente
    /// </summary>
    public class UpdateModuleCommand : IRequest<ModuleCommandResponseDto>
    {
        public int ModuleId { get; set; }
        public UpdateModuleDto ModuleData { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdateModuleCommand(int moduleId, UpdateModuleDto moduleData, int updatedByUserId)
        {
            ModuleId = moduleId;
            ModuleData = moduleData;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Command para eliminar un módulo (soft delete)
    /// </summary>
    public class DeleteModuleCommand : IRequest<DeleteResponseDto>
    {
        public int ModuleId { get; set; }
        public int DeletedByUserId { get; set; }

        public DeleteModuleCommand(int moduleId, int deletedByUserId)
        {
            ModuleId = moduleId;
            DeletedByUserId = deletedByUserId;
        }
    }

    // ===================================================================
    // COMMANDS PARA SUBMÓDULOS
    // ===================================================================

    /// <summary>
    /// Command para crear un nuevo submódulo
    /// </summary>
    public class CreateSubmoduleCommand : IRequest<SubmoduleCommandResponseDto>
    {
        public CreateSubmoduleDto SubmoduleData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateSubmoduleCommand(CreateSubmoduleDto submoduleData, int createdByUserId)
        {
            SubmoduleData = submoduleData;
            CreatedByUserId = createdByUserId;
        }
    }

    /// <summary>
    /// Command para actualizar un submódulo existente
    /// </summary>
    public class UpdateSubmoduleCommand : IRequest<SubmoduleCommandResponseDto>
    {
        public int SubmoduleId { get; set; }
        public UpdateSubmoduleDto SubmoduleData { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdateSubmoduleCommand(int submoduleId, UpdateSubmoduleDto submoduleData, int updatedByUserId)
        {
            SubmoduleId = submoduleId;
            SubmoduleData = submoduleData;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Command para eliminar un submódulo (soft delete)
    /// </summary>
    public class DeleteSubmoduleCommand : IRequest<DeleteResponseDto>
    {
        public int SubmoduleId { get; set; }
        public int DeletedByUserId { get; set; }

        public DeleteSubmoduleCommand(int submoduleId, int deletedByUserId)
        {
            SubmoduleId = submoduleId;
            DeletedByUserId = deletedByUserId;
        }
    }
}
