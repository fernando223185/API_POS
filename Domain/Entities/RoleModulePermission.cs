using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Permisos de un ROL por módulo/submódulo del sistema
    /// Estructura idéntica a UserModulePermission pero para roles
    /// </summary>
    [Table("RoleModulePermissions")]
    public class RoleModulePermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID del rol
        /// </summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// ID del módulo del sistema (1-8)
        /// </summary>
        [Required]
        public int ModuleId { get; set; }

        /// <summary>
        /// Nombre del módulo (para facilitar consultas)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Path del módulo
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Path { get; set; }

        /// <summary>
        /// Icono del módulo
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Icon { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Si el rol tiene acceso a este módulo
        /// </summary>
        public bool HasAccess { get; set; }

        /// <summary>
        /// ID del submódulo (opcional, puede ser null para permisos a nivel módulo)
        /// </summary>
        public int? SubmoduleId { get; set; }

        /// <summary>
        /// Puede ver/leer
        /// </summary>
        public bool CanView { get; set; }

        /// <summary>
        /// Puede crear
        /// </summary>
        public bool CanCreate { get; set; }

        /// <summary>
        /// Puede editar
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Puede eliminar
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Fecha de creación del permiso
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuario que creó/asignó este permiso
        /// </summary>
        public int? CreatedByUserId { get; set; }

        // Relaciones
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
