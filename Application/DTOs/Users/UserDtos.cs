namespace Application.DTOs.Users
{
    /// <summary>
    /// DTO de respuesta de usuario
    /// </summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO de respuesta de rol
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
    /// DTO de listado de usuarios
    /// </summary>
    public class UsersListResponseDto
    {
        public string Message { get; set; }
        public int Error { get; set; }
        public List<UserResponseDto> Users { get; set; } = new();
        public int TotalUsers { get; set; }
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

    /// <summary>
    /// DTO para filtros de consulta de usuarios
    /// </summary>
    public class UserFiltersDto
    {
        public string Search { get; set; }
        public int? RoleId { get; set; }
        public bool? Active { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
