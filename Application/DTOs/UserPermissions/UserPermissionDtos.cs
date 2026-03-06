namespace Application.DTOs.UserPermissions
{
    /// <summary>
    /// DTO para guardar permisos de usuario
    /// FORMATO UNIFICADO - Idéntico al de SaveRoleModulePermissionsDto
    /// </summary>
    public class SaveUserPermissionsRequestDto
    {
        public int UserId { get; set; }
        public List<UserModulePermissionItemDto> Modules { get; set; } = new();
    }

    /// <summary>
    /// DTO de un módulo con sus permisos para USUARIO
    /// FORMATO UNIFICADO - Idéntico al de RoleModulePermissionItemDto
    /// </summary>
    public class UserModulePermissionItemDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool HasAccess { get; set; }
        public List<UserSubmodulePermissionItemDto> Submodules { get; set; } = new();
    }

    /// <summary>
    /// DTO de un submódulo con sus permisos para USUARIO
    /// FORMATO UNIFICADO - Idéntico al de RoleSubmodulePermissionItemDto
    /// </summary>
    public class UserSubmodulePermissionItemDto
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
    /// DTO de respuesta al guardar permisos de usuario
    /// </summary>
    public class UserPermissionsResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public int UserId { get; set; }
        public int TotalPermissionsSaved { get; set; }
        public DateTime SavedAt { get; set; }
    }

    /// <summary>
    /// DTO para obtener permisos de usuario
    /// FORMATO UNIFICADO
    /// </summary>
    public class GetUserPermissionsResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<UserModulePermissionItemDto> Modules { get; set; } = new();
        public int TotalModules { get; set; }
        public int TotalSubmodules { get; set; }
    }

    /// <summary>
    /// DTO para verificar un permiso específico
    /// </summary>
    public class CheckUserPermissionRequestDto
    {
        public int UserId { get; set; }
        public int ModuleId { get; set; }
        public int? SubmoduleId { get; set; }
        public string Action { get; set; } // "view", "create", "edit", "delete"
    }

    /// <summary>
    /// DTO de respuesta al verificar permiso
    /// </summary>
    public class CheckUserPermissionResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public int UserId { get; set; }
        public int ModuleId { get; set; }
        public int? SubmoduleId { get; set; }
        public string Action { get; set; }
        public bool HasPermission { get; set; }
    }

    // ===================================================================
    // MANTENER PARA RETROCOMPATIBILIDAD (DEPRECATED)
    // ===================================================================

    /// <summary>
    /// [DEPRECATED] Usar UserModulePermissionItemDto
    /// </summary>
    [Obsolete("Use UserModulePermissionItemDto instead")]
    public class ModulePermissionDto
    {
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool HasAccess { get; set; }
        public PermissionActionsDto Permissions { get; set; }
        public List<SubmodulePermissionDto> Submodules { get; set; } = new();
    }

    /// <summary>
    /// [DEPRECATED] Usar UserSubmodulePermissionItemDto
    /// </summary>
    [Obsolete("Use UserSubmodulePermissionItemDto instead")]
    public class SubmodulePermissionDto
    {
        public int SubmoduleId { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool HasAccess { get; set; }
        public PermissionActionsDto Permissions { get; set; }
    }

    /// <summary>
    /// [DEPRECATED] Los permisos ahora están directamente en el submódulo
    /// </summary>
    [Obsolete("Permissions are now directly on the submodule")]
    public class PermissionActionsDto
    {
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
