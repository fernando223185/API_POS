using Application.Abstractions.Messaging;
using Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Core.Users.Commands
{
    public class CreateUserCommand : ICommand<User>
    {
        [Required]
        [StringLength(100)]
        public string Code { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }  

        [StringLength(100)]
        public string LastName { get; set; }  

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } 

        [StringLength(20)]
        public string Phone { get; set; } 

        [Required]
        public int RoleId { get; set; } 

        [Required]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; }

        [Required]
        public int StatusId { get; set; } = 1; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    }
}
