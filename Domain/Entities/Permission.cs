using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Permission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }  // "Create", "Read", "Update", "Delete"

        [Required]
        [StringLength(100)]
        public string Resource { get; set; }  // "Customer", "Product", "Sale", "User"

        [StringLength(255)]
        public string Description { get; set; }  // "Crear clientes"

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public Module Module { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}