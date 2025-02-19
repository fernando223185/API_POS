using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Code { get; set; }  // Código único del usuario

        [Required]
        [StringLength(255)]
        public string Name { get; set; }  
        [Required]
        public byte[] PasswordHash { get; set; }  // Contraseña encriptada

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } 

        [StringLength(20)]
        public string Phone { get; set; }  // Teléfono

        [Required]
        public int RoleId { get; set; }  // Relación con la tabla Roles

        [ForeignKey("RoleId")]
        public Role Role { get; set; }  // Propiedad de navegación

        [Required]
        public bool Active { get; set; } = true;  // Estado del usuario (activo/inactivo)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Fecha de creación
        public DateTime? UpdatedAt { get; set; }  // Fecha de actualización
        public DateTime? DeletedAt { get; set; }  
    }
}
