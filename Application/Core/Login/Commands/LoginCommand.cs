using Application.Abstractions.Messaging;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Core.Login.Commands
{
    public class LoginCommand : ICommand<User>
    {
        [Required(ErrorMessage = "El código de usuario es requerido")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }
}

