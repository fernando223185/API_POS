using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Definición de submódulos del sistema (configurables en BD)
    /// </summary>
    [Table("Submodules")]
    public class SystemSubmodule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // ID manual (21, 22, 31, etc.)
        public int Id { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public SystemModule Module { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(200)]
        public string Path { get; set; }

        [Required]
        [StringLength(50)]
        public string Icon { get; set; }

        public int Order { get; set; }

        [StringLength(100)]
        public string Color { get; set; } // Para gradientes Tailwind

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
