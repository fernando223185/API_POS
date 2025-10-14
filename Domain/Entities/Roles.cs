using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }  // Nombre del rol (Admin, Usuario, etc.)

        [StringLength(255)]
        public string Description { get; set; }  // Descripción del rol

        [Required]
        public bool IsActive { get; set; } = true;

        public ICollection<User> Users { get; set; } = new List<User>();  // Relación con usuarios
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>(); // Relación con permisos
    }
}
