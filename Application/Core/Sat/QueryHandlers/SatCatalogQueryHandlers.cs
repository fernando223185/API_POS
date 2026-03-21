using Application.Abstractions.Sat;
using Application.Core.Sat.Queries;
using Application.DTOs.Sat;
using MediatR;

namespace Application.Core.Sat.QueryHandlers
{
    public class GetSatUsoCfdiQueryHandler : IRequestHandler<GetSatUsoCfdiQuery, SatCatalogResponseDto<SatUsoCfdiDto>>
    {
        private readonly ISatCatalogRepository _repository;

        public GetSatUsoCfdiQueryHandler(ISatCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<SatCatalogResponseDto<SatUsoCfdiDto>> Handle(GetSatUsoCfdiQuery request, CancellationToken cancellationToken)
        {
            var items = await _repository.GetUsoCfdiAsync(request.PersonaFisica, request.PersonaMoral);

            var dtos = items.Select(x => new SatUsoCfdiDto
            {
                Codigo = x.Codigo,
                Descripcion = x.Descripcion,
                AplicaPersonaFisica = x.AplicaPersonaFisica,
                AplicaPersonaMoral = x.AplicaPersonaMoral
            }).ToList();

            return new SatCatalogResponseDto<SatUsoCfdiDto>
            {
                Message = "Catálogo obtenido exitosamente",
                Error = 0,
                Data = dtos,
                Total = dtos.Count
            };
        }
    }

    public class GetSatRegimenFiscalQueryHandler : IRequestHandler<GetSatRegimenFiscalQuery, SatCatalogResponseDto<SatRegimenFiscalDto>>
    {
        private readonly ISatCatalogRepository _repository;

        public GetSatRegimenFiscalQueryHandler(ISatCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<SatCatalogResponseDto<SatRegimenFiscalDto>> Handle(GetSatRegimenFiscalQuery request, CancellationToken cancellationToken)
        {
            var items = await _repository.GetRegimenFiscalAsync(request.PersonaFisica, request.PersonaMoral);

            var dtos = items.Select(x => new SatRegimenFiscalDto
            {
                Codigo = x.Codigo,
                Descripcion = x.Descripcion,
                AplicaPersonaFisica = x.AplicaPersonaFisica,
                AplicaPersonaMoral = x.AplicaPersonaMoral
            }).ToList();

            return new SatCatalogResponseDto<SatRegimenFiscalDto>
            {
                Message = "Catálogo obtenido exitosamente",
                Error = 0,
                Data = dtos,
                Total = dtos.Count
            };
        }
    }

    public class GetSatFormaPagoQueryHandler : IRequestHandler<GetSatFormaPagoQuery, SatCatalogResponseDto<SatFormaPagoDto>>
    {
        private readonly ISatCatalogRepository _repository;

        public GetSatFormaPagoQueryHandler(ISatCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<SatCatalogResponseDto<SatFormaPagoDto>> Handle(GetSatFormaPagoQuery request, CancellationToken cancellationToken)
        {
            var items = await _repository.GetFormaPagoAsync();

            var dtos = items.Select(x => new SatFormaPagoDto
            {
                Codigo = x.Codigo,
                Descripcion = x.Descripcion,
                Bancarizado = x.Bancarizado
            }).ToList();

            return new SatCatalogResponseDto<SatFormaPagoDto>
            {
                Message = "Catálogo obtenido exitosamente",
                Error = 0,
                Data = dtos,
                Total = dtos.Count
            };
        }
    }

    public class GetSatMetodoPagoQueryHandler : IRequestHandler<GetSatMetodoPagoQuery, SatCatalogResponseDto<SatMetodoPagoDto>>
    {
        private readonly ISatCatalogRepository _repository;

        public GetSatMetodoPagoQueryHandler(ISatCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<SatCatalogResponseDto<SatMetodoPagoDto>> Handle(GetSatMetodoPagoQuery request, CancellationToken cancellationToken)
        {
            var items = await _repository.GetMetodoPagoAsync();

            var dtos = items.Select(x => new SatMetodoPagoDto
            {
                Codigo = x.Codigo,
                Descripcion = x.Descripcion
            }).ToList();

            return new SatCatalogResponseDto<SatMetodoPagoDto>
            {
                Message = "Catálogo obtenido exitosamente",
                Error = 0,
                Data = dtos,
                Total = dtos.Count
            };
        }
    }

    public class GetSatTipoComprobanteQueryHandler : IRequestHandler<GetSatTipoComprobanteQuery, SatCatalogResponseDto<SatTipoComprobanteDto>>
    {
        private readonly ISatCatalogRepository _repository;

        public GetSatTipoComprobanteQueryHandler(ISatCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<SatCatalogResponseDto<SatTipoComprobanteDto>> Handle(GetSatTipoComprobanteQuery request, CancellationToken cancellationToken)
        {
            var items = await _repository.GetTipoComprobanteAsync();

            var dtos = items.Select(x => new SatTipoComprobanteDto
            {
                Codigo = x.Codigo,
                Descripcion = x.Descripcion
            }).ToList();

            return new SatCatalogResponseDto<SatTipoComprobanteDto>
            {
                Message = "Catálogo obtenido exitosamente",
                Error = 0,
                Data = dtos,
                Total = dtos.Count
            };
        }
    }

    public class GetSatUnidadMedidaQueryHandler : IRequestHandler<GetSatUnidadMedidaQuery, SatCatalogResponseDto<SatUnidadMedidaDto>>
    {
        private readonly ISatCatalogRepository _repository;

        public GetSatUnidadMedidaQueryHandler(ISatCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<SatCatalogResponseDto<SatUnidadMedidaDto>> Handle(GetSatUnidadMedidaQuery request, CancellationToken cancellationToken)
        {
            var items = await _repository.GetUnidadMedidaAsync(request.Search);

            var dtos = items.Select(x => new SatUnidadMedidaDto
            {
                ClaveUnidad = x.ClaveUnidad,
                Nombre = x.Nombre,
                Simbolo = x.Simbolo,
                Descripcion = x.Descripcion
            }).ToList();

            return new SatCatalogResponseDto<SatUnidadMedidaDto>
            {
                Message = "Catálogo obtenido exitosamente",
                Error = 0,
                Data = dtos,
                Total = dtos.Count
            };
        }
    }

    public class GetSatProductoServicioQueryHandler : IRequestHandler<GetSatProductoServicioQuery, SatCatalogResponseDto<SatProductoServicioDto>>
    {
        private readonly ISatCatalogRepository _repository;

        public GetSatProductoServicioQueryHandler(ISatCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<SatCatalogResponseDto<SatProductoServicioDto>> Handle(GetSatProductoServicioQuery request, CancellationToken cancellationToken)
        {
            var items = await _repository.GetProductoServicioAsync(request.Search, request.Limit);

            var dtos = items.Select(x => new SatProductoServicioDto
            {
                ClaveProdServ = x.ClaveProdServ,
                Descripcion = x.Descripcion,
                IncluyeIva = x.IncluyeIva,
                IncluyeIeps = x.IncluyeIeps
            }).ToList();

            return new SatCatalogResponseDto<SatProductoServicioDto>
            {
                Message = "Catálogo obtenido exitosamente",
                Error = 0,
                Data = dtos,
                Total = dtos.Count
            };
        }
    }
}
