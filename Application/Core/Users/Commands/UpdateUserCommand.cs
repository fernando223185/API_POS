using Application.Abstractions.Messaging;
using System.ComponentModel.DataAnnotations;

namespace Application.Core.Users.Commands
{
    public class UpdateUserCommand : ICommand<bool>
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string? Code { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public int? RoleId { get; set; }

        /// <summary>
        /// Nueva contraseña (opcional, solo si se desea cambiar)
        /// </summary>
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string? NewPassword { get; set; }

        /// <summary>
        /// ID de la empresa asignada
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// ID de la sucursal asignada
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Almacén asignado por defecto
        /// </summary>
        public int? DefaultWarehouseId { get; set; }

        /// <summary>
        /// Permite vender desde múltiples almacenes
        /// </summary>
        public bool? CanSellFromMultipleWarehouses { get; set; }

        public bool? Active { get; set; }
    }
}
