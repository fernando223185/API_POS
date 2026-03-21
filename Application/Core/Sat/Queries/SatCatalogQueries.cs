using Application.DTOs.Sat;
using MediatR;

namespace Application.Core.Sat.Queries
{
    public class GetSatUsoCfdiQuery : IRequest<SatCatalogResponseDto<SatUsoCfdiDto>>
    {
        public bool? PersonaFisica { get; set; }
        public bool? PersonaMoral { get; set; }
    }

    public class GetSatRegimenFiscalQuery : IRequest<SatCatalogResponseDto<SatRegimenFiscalDto>>
    {
        public bool? PersonaFisica { get; set; }
        public bool? PersonaMoral { get; set; }
    }

    public class GetSatFormaPagoQuery : IRequest<SatCatalogResponseDto<SatFormaPagoDto>>
    {
    }

    public class GetSatMetodoPagoQuery : IRequest<SatCatalogResponseDto<SatMetodoPagoDto>>
    {
    }

    public class GetSatTipoComprobanteQuery : IRequest<SatCatalogResponseDto<SatTipoComprobanteDto>>
    {
    }

    public class GetSatUnidadMedidaQuery : IRequest<SatCatalogResponseDto<SatUnidadMedidaDto>>
    {
        public string? Search { get; set; }
    }

    public class GetSatProductoServicioQuery : IRequest<SatCatalogResponseDto<SatProductoServicioDto>>
    {
        public string? Search { get; set; }
        public int Limit { get; set; } = 50;
    }
}
