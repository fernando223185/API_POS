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
        /// Endpoint de login con soporte completo para CORS
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            try
            {
                // ✅ Agregar headers de CORS explícitos
                AddCorsHeaders();

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
                            RoleName = user.Role?.Name ?? "Usuario"
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
                Console.WriteLine($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        /// <summary>
        /// Manejo de preflight OPTIONS para CORS
        /// </summary>
        [HttpOptions("login")]
        public IActionResult PreflightLogin()
        {
            AddCorsHeaders();
            return Ok();
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromHeader] string authorization)
        {
            try
            {
                AddCorsHeaders();

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
                Console.WriteLine($"Token validation error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpOptions("validate-token")]
        public IActionResult PreflightValidateToken()
        {
            AddCorsHeaders();
            return Ok();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromHeader] string authorization)
        {
            try
            {
                AddCorsHeaders();

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

                // Aquí podrías obtener información actualizada del usuario desde la base de datos
                // Por ahora retornamos un nuevo token básico
                
                return Ok(new { 
                    message = "Token refresh endpoint - implement as needed", 
                    error = 0,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token refresh error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        [HttpOptions("refresh-token")]
        public IActionResult PreflightRefreshToken()
        {
            AddCorsHeaders();
            return Ok();
        }

        /// <summary>
        /// Agregar headers de CORS manualmente para compatibilidad máxima
        /// ✅ ACTUALIZADO: Incluye soporte para IPs de red local
        /// </summary>
        private void AddCorsHeaders()
        {
            var origin = Request.Headers["Origin"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(origin))
            {
                var uri = new Uri(origin);
                
                // Verificar si el origin es localhost
                var isLocalhost = uri.Host == "localhost" || 
                                 uri.Host == "127.0.0.1" || 
                                 uri.Host == "::1";
                
                // ✅ Verificar si el origin es una IP de red privada
                var isPrivateNetwork = uri.Host.StartsWith("192.168.") ||
                                      uri.Host.StartsWith("10.") ||
                                      (uri.Host.StartsWith("172.") && 
                                       int.TryParse(uri.Host.Split('.')[1], out var secondOctet) && 
                                       secondOctet >= 16 && secondOctet <= 31);
                
                // ✅ Permitir localhost Y redes privadas
                if (isLocalhost || isPrivateNetwork)
                {
                    Response.Headers.Add("Access-Control-Allow-Origin", origin);
                    Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                    Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    Response.Headers.Add("Access-Control-Allow-Headers", 
                        "Origin, X-Requested-With, Content-Type, Accept, Authorization, Cache-Control, Pragma");
                    Response.Headers.Add("Access-Control-Max-Age", "86400");
                    
                    var networkType = isLocalhost ? "localhost" : "private network";
                    Console.WriteLine($"🌐 CORS headers added for {networkType} origin: {origin}");
                }
                else
                {
                    Console.WriteLine($"🚫 CORS blocked for non-local origin: {origin}");
                }
            }
        }
    }
}

