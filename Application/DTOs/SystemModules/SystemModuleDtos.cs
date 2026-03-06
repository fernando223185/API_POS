namespace Application.DTOs.SystemModules
{
    /// <summary>
    /// DTO para crear/actualizar un módulo
    /// </summary>
    public class CreateUpdateModuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO para crear/actualizar un submódulo
    /// </summary>
    public class CreateUpdateSubmoduleDto
    {
        public int Id { get; set; }
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
    /// DTO de respuesta de módulo
    /// </summary>
    public class ModuleResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<SubmoduleResponseDto> Submodules { get; set; } = new();
    }

    /// <summary>
    /// DTO de respuesta de submódulo
    /// </summary>
    public class SubmoduleResponseDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO de listado de módulos
    /// </summary>
    public class ModulesListResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public List<ModuleResponseDto> Modules { get; set; } = new();
        public int TotalModules { get; set; }
        public int TotalSubmodules { get; set; }
    }

    /// <summary>
    /// DTO de listado de submódulos
    /// </summary>
    public class SubmodulesListResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public List<SubmoduleResponseDto> Submodules { get; set; } = new();
        public int TotalSubmodules { get; set; }
    }
}
