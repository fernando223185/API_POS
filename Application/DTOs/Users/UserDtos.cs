using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Users
{
    /// <summary>
    /// DTO de respuesta de usuario
    /// </summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ✅ Empresa y Sucursal
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }

        // ✅ Control de almacén
        public int? DefaultWarehouseId { get; set; }
        public string? DefaultWarehouseCode { get; set; }
        public string? DefaultWarehouseName { get; set; }
        public bool CanSellFromMultipleWarehouses { get; set; }
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

    /// <summary>
    /// DTO para crear un nuevo usuario
    /// </summary>
    public class CreateUserDto
    {
        [Required(ErrorMessage = "El c�digo es requerido")]
        [StringLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El password es requerido")]
        [MinLength(6, ErrorMessage = "El password debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inv�lido")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "El RoleId es requerido")]
        public int RoleId { get; set; }

        // ? NUEVO: Control de almac�n
        /// <summary>
        /// Almac�n asignado por defecto (NULL si no tiene asignado)
        /// </summary>
        public int? DefaultWarehouseId { get; set; }

        /// <summary>
        /// Permite vender desde m�ltiples almacenes (false por defecto)
        /// </summary>
        public bool CanSellFromMultipleWarehouses { get; set; } = false;
    }

    /// <summary>
    /// DTO para actualizar un usuario existente
    /// </summary>
    public class UpdateUserDto
    {
        [StringLength(255)]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public int? RoleId { get; set; }

        public bool? Active { get; set; }

        /// <summary>
        /// Nueva contraseña (opcional, solo si se desea cambiar)
        /// </summary>
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? NewPassword { get; set; }

        /// <summary>
        /// ID de la empresa asignada
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// ID de la sucursal asignada
        /// </summary>
        public int? BranchId { get; set; }

        // ✅ Control de almacén
        public int? DefaultWarehouseId { get; set; }
        
        public bool? CanSellFromMultipleWarehouses { get; set; }
    }
}
