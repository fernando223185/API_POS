using Application.Core.Sat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers.Sat
{
    [ApiController]
    [Route("api/sat")]
    [Authorize]
    public class SatCatalogController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SatCatalogController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener catálogo de Uso del CFDI
        /// </summary>
        /// <param name="personaFisica">Filtrar por aplicable a persona física</param>
        /// <param name="personaMoral">Filtrar por aplicable a persona moral</param>
        [HttpGet("uso-cfdi")]
        public async Task<IActionResult> GetUsoCfdi([FromQuery] bool? personaFisica = null, [FromQuery] bool? personaMoral = null)
        {
            var query = new GetSatUsoCfdiQuery
            {
                PersonaFisica = personaFisica,
                PersonaMoral = personaMoral
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener catálogo de Régimen Fiscal
        /// </summary>
        /// <param name="personaFisica">Filtrar por aplicable a persona física</param>
        /// <param name="personaMoral">Filtrar por aplicable a persona moral</param>
        [HttpGet("regimen-fiscal")]
        public async Task<IActionResult> GetRegimenFiscal([FromQuery] bool? personaFisica = null, [FromQuery] bool? personaMoral = null)
        {
            var query = new GetSatRegimenFiscalQuery
            {
                PersonaFisica = personaFisica,
                PersonaMoral = personaMoral
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener catálogo de Forma de Pago
        /// </summary>
        [HttpGet("forma-pago")]
        public async Task<IActionResult> GetFormaPago()
        {
            var query = new GetSatFormaPagoQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener catálogo de Método de Pago
        /// </summary>
        [HttpGet("metodo-pago")]
        public async Task<IActionResult> GetMetodoPago()
        {
            var query = new GetSatMetodoPagoQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener catálogo de Tipo de Comprobante
        /// </summary>
        [HttpGet("tipo-comprobante")]
        public async Task<IActionResult> GetTipoComprobante()
        {
            var query = new GetSatTipoComprobanteQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener catálogo de Unidad de Medida
        /// </summary>
        /// <param name="search">Búsqueda por código, nombre o descripción</param>
        [HttpGet("unidad-medida")]
        public async Task<IActionResult> GetUnidadMedida([FromQuery] string? search = null)
        {
            var query = new GetSatUnidadMedidaQuery { Search = search };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener catálogo de Producto/Servicio
        /// </summary>
        /// <param name="search">Búsqueda por código o descripción</param>
        /// <param name="limit">Límite de resultados (default: 50)</param>
        [HttpGet("producto-servicio")]
        public async Task<IActionResult> GetProductoServicio([FromQuery] string? search = null, [FromQuery] int limit = 50)
        {
            var query = new GetSatProductoServicioQuery
            {
                Search = search,
                Limit = limit
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
