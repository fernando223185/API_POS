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

        // ✅ NUEVO: Control de almacén para ventas
        /// <summary>
        /// Almacén asignado por defecto (NULL si no tiene asignado)
        /// </summary>
        public int? DefaultWarehouseId { get; set; }

        /// <summary>
        /// Permite vender desde múltiples almacenes (false por defecto)
        /// </summary>
        public bool CanSellFromMultipleWarehouses { get; set; } = false;
    }
}
