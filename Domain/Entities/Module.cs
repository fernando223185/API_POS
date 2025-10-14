using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Module
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }  // "CRM", "Sales", "Products", "Users"

        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}