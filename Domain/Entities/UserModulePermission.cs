using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Permisos personalizados por usuario para módulos y submódulos
    /// Permite control granular de acceso: Ver, Crear, Editar, Eliminar
    /// </summary>
    public class UserModulePermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        /// <summary>
        /// ID del módulo (1=Inicio, 2=Ventas, 3=Productos, etc.)
        /// </summary>
        [Required]
        public int ModuleId { get; set; }

        /// <summary>
        /// ID del submódulo (21=Nueva Venta, 31=Catálogo, etc.)
        /// NULL si es permiso a nivel de módulo principal
        /// </summary>
        public int? SubmoduleId { get; set; }

        /// <summary>
        /// Nombre del módulo/submódulo para referencia
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Ruta del módulo/submódulo
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Path { get; set; }

        /// <summary>
        /// Icono FontAwesome
        /// </summary>
        [StringLength(50)]
        public string Icon { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Si tiene acceso al módulo/submódulo
        /// </summary>
        [Required]
        public bool HasAccess { get; set; } = false;

        // ?? PERMISOS GRANULARES
        /// <summary>
        /// Permiso para VER/CONSULTAR recursos
        /// </summary>
        public bool CanView { get; set; } = false;

        /// <summary>
        /// Permiso para CREAR nuevos recursos
        /// </summary>
        public bool CanCreate { get; set; } = false;

        /// <summary>
        /// Permiso para EDITAR recursos existentes
        /// </summary>
        public bool CanEdit { get; set; } = false;

        /// <summary>
        /// Permiso para ELIMINAR recursos
        /// </summary>
        public bool CanDelete { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuario que asignó estos permisos
        /// </summary>
        public int? CreatedByUserId { get; set; }
    }
}
