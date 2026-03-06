namespace Application.DTOs.SystemModules
{
    // ===================================================================
    // DTOs PARA COMMANDS (Crear/Actualizar/Eliminar)
    // ===================================================================

    /// <summary>
    /// DTO para crear o actualizar un módulo
    /// </summary>
    public class CreateModuleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO para actualizar un módulo
    /// </summary>
    public class UpdateModuleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO para crear un submódulo
    /// </summary>
    public class CreateSubmoduleDto
    {
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO para actualizar un submódulo
    /// </summary>
    public class UpdateSubmoduleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
    }

    // ===================================================================
    // DTOs DE RESPUESTA
    // ===================================================================

    /// <summary>
    /// DTO de respuesta al crear/actualizar módulo
    /// </summary>
    public class ModuleCommandResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public ModuleResponseDto Data { get; set; }
    }

    /// <summary>
    /// DTO de respuesta al crear/actualizar submódulo
    /// </summary>
    public class SubmoduleCommandResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public SubmoduleResponseDto Data { get; set; }
    }

    /// <summary>
    /// DTO de respuesta al eliminar
    /// </summary>
    public class DeleteResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public int DeletedId { get; set; }
    }
}
