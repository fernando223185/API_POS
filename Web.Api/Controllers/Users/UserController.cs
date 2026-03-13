using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Core.Users.Commands;
using Microsoft.AspNetCore.Authorization;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crear un nuevo usuario en el sistema
        /// </summary>
        [HttpPost("create")]
        [RequireAuthentication]
        public async Task<IActionResult> create(CreateUserCommand command)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Datos de entrada inválidos",
                        error = 1,
                        errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Obtener información del usuario actual del token
                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                var currentUserName = HttpContext.Items["UserName"] as string ?? "System";

                Console.WriteLine($"👤 Creando usuario: {command.Name} por {currentUserName}");

                // Crear usuario
                var user = await _mediator.Send(command);

                Console.WriteLine($"✅ Usuario creado exitosamente: {user.Code} - {user.Name} (ID: {user.Id})");

                // Retornar respuesta estructurada
                return Ok(new
                {
                    message = "Usuario creado exitosamente",
                    error = 0,
                    data = new
                    {
                        id = user.Id,
                        code = user.Code,
                        name = user.Name,
                        email = user.Email,
                        phone = user.Phone,
                        roleId = user.RoleId,
                        active = user.Active,
                        defaultWarehouseId = user.DefaultWarehouseId,
                        canSellFromMultipleWarehouses = user.CanSellFromMultipleWarehouses,
                        createdAt = user.CreatedAt
                    },
                    createdBy = currentUserName
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Error de validación: {ex.Message}");
                return BadRequest(new
                {
                    message = ex.Message,
                    error = 1
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear usuario: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error al crear usuario",
                    error = 2,
                    details = ex.Message
                });
            }
        }
    }
}

