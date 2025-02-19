using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Core.Users.Commands;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> create(CreateUserCommand user)
        {
            var result = await _mediator.Send(user);
            return Ok(result);
        }
	}
}

