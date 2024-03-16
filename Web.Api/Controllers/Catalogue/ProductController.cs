using Application.Core.Product.Commands;
using Application.Core.Product.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.Catalogue
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("create")]
        public async Task<IActionResult> create(CreateProductCommand product)
        {
            var result = await _mediator.Send(product);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> update(UpdateProductCommand product)
        {
            var result = await _mediator.Send(product);

            if(result != null) 
            {
                return Ok(new { message = "Data updated successfully.", error = 0, result });
            }
            return NotFound(new { message = "Failed to update data.", error = 1 });

        }

        [HttpPost("List")]
        public async Task<IActionResult> Get([FromBody] GetProductByPageQuery query)
        {
            var products = await _mediator.Send(query);
            if (products == null)
            {
                return NotFound();
            }

            return Ok(products);
        }

        [HttpGet("find_by_id/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetProductByIdQuery { ID = id };

            var products = await _mediator.Send(query);

            if (products == null)
            {
                return NotFound();
            }
            return Ok(products);
        }
    }
}
