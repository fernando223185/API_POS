using Application.Core.Product.Commands;
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
        [HttpPost("update")]
        public async Task<IActionResult> update(UpdateProductCommand product)
        {
            var query = new UpdateProductCommand
            {
                ID = product.ID,
                name = product.name,
                description = product.description,
                code = product.code,
                barcode = product.barcode,
                category = product.category,
                branch = product.branch,
                price = product.price,
            };

            var result = _mediator.Send(query);
            return Ok(result);
        }
    }
}
