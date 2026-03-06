namespace Application.DTOs.Roles
{
    /// <summary>
    /// DTO para crear un nuevo rol
    /// </summary>
    public class CreateRoleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO para actualizar un rol existente
    /// </summary>
    public class UpdateRoleDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO de respuesta detallada de rol
    /// </summary>
    public class RoleDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPermissions { get; set; }
        public List<RoleUserDto> Users { get; set; } = new();
        public List<RolePermissionDto> Permissions { get; set; } = new();
    }

    /// <summary>
    /// DTO de usuario dentro de un rol
    /// </summary>
    public class RoleUserDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
    }

    /// <summary>
    /// DTO de permiso dentro de un rol
    /// </summary>
    public class RolePermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Resource { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO para asignar permisos a un rol
    /// </summary>
    public class AssignRolePermissionsDto
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    /// <summary>
    /// DTO de respuesta simple de rol
    /// </summary>
    public class RoleResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPermissions { get; set; }
    }

    /// <summary>
    /// DTO de listado de roles
    /// </summary>
    public class RolesListResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public List<RoleResponseDto> Roles { get; set; } = new();
        public int TotalRoles { get; set; }
    }

    // ===================================================================
    // ? NUEVOS DTOs PARA SISTEMA UNIFICADO (Módulos/Submódulos)
    // RENOMBRADOS para evitar conflicto con UserPermissions DTOs
    // ===================================================================

    /// <summary>
    /// DTO para guardar permisos de rol por módulos/submódulos (UNIFICADO)
    /// Misma estructura que SaveUserPermissionsRequestDto pero para roles
    /// </summary>
    public class SaveRoleModulePermissionsDto
    {
        public int RoleId { get; set; }
        public List<RoleModulePermissionItemDto> Modules { get; set; } = new();
    }

    /// <summary>
    /// DTO de módulo con permisos para ROL
    /// </summary>
    public class RoleModulePermissionItemDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool HasAccess { get; set; }
        public List<RoleSubmodulePermissionItemDto> Submodules { get; set; } = new();
    }

    /// <summary>
    /// DTO de submódulo con permisos granulares para ROL
    /// </summary>
    public class RoleSubmodulePermissionItemDto
    {
        public int SubmoduleId { get; set; }
        public string SubmoduleName { get; set; }
        public bool HasAccess { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// DTO de respuesta de permisos de rol por módulos
    /// </summary>
    public class RoleModulePermissionsResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<RoleModulePermissionItemDto> Modules { get; set; } = new();
    }
}
