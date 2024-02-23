
using Application.Core.CRM.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.CRM
{
	[Route("api/[controller]")]
	[ApiController]
	public class CustomerController : ControllerBase
	{
		private readonly IMediator _mediator;
		public CustomerController(IMediator mediator)
		{
			_mediator = mediator;
		}
		[HttpPost("create")]
		public async Task<IActionResult> create(CreateCustomerCommand customer)
		{
			var result = await _mediator.Send(customer);
			return Ok(result);
		}
	}
}

