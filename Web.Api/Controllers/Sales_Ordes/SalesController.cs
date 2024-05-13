using Application.Core.Sales_orders.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Controllers.CRM;

namespace Web.Api.Controllers.Sales_Ordes
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SalesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateSaleCommand sale)
        {
            var result = await _mediator.Send(sale);
            if (result != null)
            {
                return Ok(new { message = "Data created successfully.", error = 0, result });
            }
            return BadRequest(new { message = "Failed to created data.", error = 1 });

        }
    }
}
