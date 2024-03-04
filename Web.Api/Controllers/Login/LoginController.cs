using MediatR;
using Application.Core.Login.Commands;


using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.LoginController
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
	{
		private readonly IMediator _mediator;
		public LoginController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            try
            {
                var user = await _mediator.Send(command);
                if (user != null)
                {
                    return Ok(new { message = "Login successful", error = 0 , user });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid credentials", error = 1 });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}

