
using Application.Core.CRM.CommandHandlers;
using Application.Core.CRM.Commands;
using Application.Core.CRM.Queries;

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

		[HttpPut("update")]
		public async Task<IActionResult> update(UpdateCustomerCommand customer)
		{
			var result = await _mediator.Send(customer);
			return Ok(result);
		}
		[HttpPost("List")]
		public async Task<IActionResult> Get([FromBody] GetCustomerByPageQuery query)
		{
			var customers = await _mediator.Send(query);
			if(customers == null) 
			{
				return NotFound();
			}

			return Ok(customers);
		}

		[HttpGet("find_by_id/{id}")]
		public async Task<IActionResult> GetyById(int id)
		{
			var query = new GetCustomerByIdQuery { ID = id };
			var customer = await _mediator.Send(query);

			if (customer == null)
			{
				return NotFound();
			}

			return Ok(customer);
		}
	}
}

