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

        public string Description { get; set; }  // Descripción del rol

        public ICollection<User> Users { get; set; }  // Relación con usuarios
    }
}
