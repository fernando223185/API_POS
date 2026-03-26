using MediatR;
using Application.Core.Login.Commands;
using Application.Abstractions.Security;
using Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.LoginController
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly IJwtTokenService _jwtTokenService;

		public LoginController(IMediator mediator, IJwtTokenService jwtTokenService)
		{
			_mediator = mediator;
			_jwtTokenService = jwtTokenService;
		}

        /// <summary>
        /// Endpoint de login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Password))
                {
                    return BadRequest(new { 
                        message = "Code and Password are required", 
                        error = 1,
                        errors = new {
                            Code = string.IsNullOrWhiteSpace(command.Code) ? new[] { "The Code field is required." } : null,
                            Password = string.IsNullOrWhiteSpace(command.Password) ? new[] { "The Password field is required." } : null
                        }
                    });
                }

                var user = await _mediator.Send(command);
                
                if (user != null)
                {
                    // Generar JWT token
                    var token = _jwtTokenService.GenerateToken(
                        user.Id,
                        user.Code,
                        user.Name,
                        user.Email,
                        user.RoleId,
                        user.Role?.Name ?? "Usuario"
                    );

                    var response = new LoginResponseDto
                    {
                        Message = "Login successful",
                        Error = 0,
                        Token = token,
                        TokenType = "Bearer",
                        ExpiresAt = DateTime.UtcNow.AddHours(8),
                        User = new UserInfoDto
                        {
                            Id = user.Id,
                            Code = user.Code,
                            Name = user.Name,
                            Email = user.Email,
                            Active = user.Active,
                            RoleId = user.RoleId,
                            RoleName = user.Role?.Name ?? "Usuario",
                            BranchId = user.BranchId,
                            CompanyId = user.CompanyId
                        }
                    };

                    return Ok(response);
                }
                else
                {
                    return Unauthorized(new { message = "Invalid credentials", error = 1 });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message, error = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Validar si un token JWT es válido
        /// </summary>
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromHeader] string authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return BadRequest(new { message = "Invalid authorization header", error = 1 });
                }

                var token = authorization.Substring("Bearer ".Length).Trim();
                
                if (_jwtTokenService.ValidateToken(token))
                {
                    var userId = await _jwtTokenService.GetUserIdFromTokenAsync(token);
                    
                    return Ok(new { 
                        message = "Token is valid", 
                        error = 0, 
                        valid = true,
                        userId = userId
                    });
                }
                else
                {
                    return Unauthorized(new { message = "Token is invalid or expired", error = 1, valid = false });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Token validation error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Refrescar token JWT (placeholder)
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromHeader] string authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                {
                    return BadRequest(new { message = "Invalid authorization header", error = 1 });
                }

                var token = authorization.Substring("Bearer ".Length).Trim();
                var userId = await _jwtTokenService.GetUserIdFromTokenAsync(token);
                
                if (userId == null)
                {
                    return Unauthorized(new { message = "Invalid token", error = 1 });
                }

                // TODO: Implementar lógica de refresh token completa
                
                return Ok(new { 
                    message = "Token refresh endpoint - implement as needed", 
                    error = 0,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Token refresh error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }
    }
}

