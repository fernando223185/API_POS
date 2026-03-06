using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Definición de módulos del sistema (configurables en BD)
    /// </summary>
    [Table("Modules")]
    public class SystemModule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // ID manual (1-8)
        public int Id { get; set; }

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

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relación con submódulos
        public ICollection<SystemSubmodule> Submodules { get; set; } = new List<SystemSubmodule>();
    }
}
