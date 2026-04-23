using Application.Abstractions.Common;
using Application.Abstractions.Shipping;
using Application.Core.ShippingCarrier.Commands;
using Application.Core.ShippingCarrier.Queries;
using Application.DTOs.ShippingCarrier;
using MediatR;

namespace Application.Core.ShippingCarrier.CommandHandlers
{
    // ─────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────
    public class CreateShippingCarrierCommandHandler : IRequestHandler<CreateShippingCarrierCommand, ShippingCarrierDto>
    {
        private readonly IShippingCarrierRepository _repo;
        private readonly ICodeGeneratorService _codeGen;

        public CreateShippingCarrierCommandHandler(IShippingCarrierRepository repo, ICodeGeneratorService codeGen)
        {
            _repo = repo;
            _codeGen = codeGen;
        }

        public async Task<ShippingCarrierDto> Handle(CreateShippingCarrierCommand request, CancellationToken cancellationToken)
        {
            var d = request.Data;

            string code;
            if (!string.IsNullOrWhiteSpace(d.CustomCode))
            {
                // Código manual: validar unicidad
                code = d.CustomCode.Trim().ToUpper();
                if (await _repo.CodeExistsAsync(code))
                    throw new InvalidOperationException($"Ya existe una paquetería con el código '{code}'");
            }
            else
            {
                // Código autogenerado: prefijo configurable + 5 dígitos → FEDEX-00001
                var prefix = string.IsNullOrWhiteSpace(d.CodePrefix) ? "PKT" : d.CodePrefix.Trim().ToUpper();
                code = await _codeGen.GenerateNextCodeAsync(prefix, "ShippingCarriers", length: 5);
            }

            var entity = new Domain.Entities.ShippingCarrier
            {
                CompanyId          = d.CompanyId,
                Code               = code,
                Name               = d.Name.Trim(),
                Description        = d.Description,
                ContactName        = d.ContactName,
                Phone              = d.Phone,
                PhoneAlt           = d.PhoneAlt,
                Email              = d.Email,
                Website            = d.Website,
                Address            = d.Address,
                City               = d.City,
                State              = d.State,
                ZipCode            = d.ZipCode,
                Country            = d.Country ?? "México",
                AccountNumber      = d.AccountNumber,
                AccountRepName     = d.AccountRepName,
                AccountRepPhone    = d.AccountRepPhone,
                AccountRepEmail    = d.AccountRepEmail,
                ServiceTypes       = d.ServiceTypes,
                EstimatedDeliveryDays = d.EstimatedDeliveryDays,
                Coverage           = d.Coverage ?? "nacional",
                BasePrice          = d.BasePrice,
                PricePerKg         = d.PricePerKg,
                ExpressPricePerKg  = d.ExpressPricePerKg,
                MaxWeightKg        = d.MaxWeightKg,
                TrackingUrl        = d.TrackingUrl,
                ApiKey             = d.ApiKey,
                ApiSecret          = d.ApiSecret,
                ApiEndpoint        = d.ApiEndpoint,
                LogoUrl            = d.LogoUrl,
                Notes              = d.Notes,
                IsActive           = true,
                CreatedAt          = DateTime.UtcNow,
            };

            var created = await _repo.CreateAsync(entity);
            return ShippingCarrierMapper.MapToDto(created);
        }
    }

    // ─────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────
    public class UpdateShippingCarrierCommandHandler : IRequestHandler<UpdateShippingCarrierCommand, ShippingCarrierDto>
    {
        private readonly IShippingCarrierRepository _repo;

        public UpdateShippingCarrierCommandHandler(IShippingCarrierRepository repo) => _repo = repo;

        public async Task<ShippingCarrierDto> Handle(UpdateShippingCarrierCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Paquetería {request.Id} no encontrada");

            var d = request.Data;
            entity.Name               = d.Name.Trim();
            entity.Description        = d.Description;
            if (!string.IsNullOrWhiteSpace(d.Code))
            {
                var newCode = d.Code.Trim().ToUpper();
                if (newCode != entity.Code && await _repo.CodeExistsAsync(newCode, entity.Id))
                    throw new InvalidOperationException($"Ya existe una paquetería con el código '{newCode}'");
                entity.Code = newCode;
            }
            entity.ContactName        = d.ContactName;
            entity.Phone              = d.Phone;
            entity.PhoneAlt           = d.PhoneAlt;
            entity.Email              = d.Email;
            entity.Website            = d.Website;
            entity.Address            = d.Address;
            entity.City               = d.City;
            entity.State              = d.State;
            entity.ZipCode            = d.ZipCode;
            entity.Country            = d.Country;
            entity.AccountNumber      = d.AccountNumber;
            entity.AccountRepName     = d.AccountRepName;
            entity.AccountRepPhone    = d.AccountRepPhone;
            entity.AccountRepEmail    = d.AccountRepEmail;
            entity.ServiceTypes       = d.ServiceTypes;
            entity.EstimatedDeliveryDays = d.EstimatedDeliveryDays;
            entity.Coverage           = d.Coverage;
            entity.BasePrice          = d.BasePrice;
            entity.PricePerKg         = d.PricePerKg;
            entity.ExpressPricePerKg  = d.ExpressPricePerKg;
            entity.MaxWeightKg        = d.MaxWeightKg;
            entity.TrackingUrl        = d.TrackingUrl;
            entity.ApiKey             = d.ApiKey;
            entity.ApiSecret          = d.ApiSecret;
            entity.ApiEndpoint        = d.ApiEndpoint;
            entity.LogoUrl            = d.LogoUrl;
            entity.IsActive           = d.IsActive;
            entity.Notes              = d.Notes;
            entity.UpdatedAt          = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(entity);
            return ShippingCarrierMapper.MapToDto(updated);
        }
    }

    // ─────────────────────────────────────────────
    // DELETE
    // ─────────────────────────────────────────────
    public class DeleteShippingCarrierCommandHandler : IRequestHandler<DeleteShippingCarrierCommand, bool>
    {
        private readonly IShippingCarrierRepository _repo;

        public DeleteShippingCarrierCommandHandler(IShippingCarrierRepository repo) => _repo = repo;

        public async Task<bool> Handle(DeleteShippingCarrierCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Paquetería {request.Id} no encontrada");

            return await _repo.DeleteAsync(entity.Id);
        }
    }

    // ─────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────
    public class GetAllShippingCarriersQueryHandler : IRequestHandler<GetAllShippingCarriersQuery, List<ShippingCarrierDto>>
    {
        private readonly IShippingCarrierRepository _repo;

        public GetAllShippingCarriersQueryHandler(IShippingCarrierRepository repo) => _repo = repo;

        public async Task<List<ShippingCarrierDto>> Handle(GetAllShippingCarriersQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetAllAsync(request.CompanyId, request.IncludeInactive);
            return list.Select(ShippingCarrierMapper.MapToDto).ToList();
        }
    }

    public class GetShippingCarrierByIdQueryHandler : IRequestHandler<GetShippingCarrierByIdQuery, ShippingCarrierDto?>
    {
        private readonly IShippingCarrierRepository _repo;

        public GetShippingCarrierByIdQueryHandler(IShippingCarrierRepository repo) => _repo = repo;

        public async Task<ShippingCarrierDto?> Handle(GetShippingCarrierByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            return entity is null ? null : ShippingCarrierMapper.MapToDto(entity);
        }
    }

    // ─────────────────────────────────────────────
    // MAPPER
    // ─────────────────────────────────────────────
    internal static class ShippingCarrierMapper
    {
        internal static ShippingCarrierDto MapToDto(Domain.Entities.ShippingCarrier c) => new()
        {
            Id                    = c.Id,
            CompanyId             = c.CompanyId,
            Code                  = c.Code,
            Name                  = c.Name,
            Description           = c.Description,
            ContactName           = c.ContactName,
            Phone                 = c.Phone,
            PhoneAlt              = c.PhoneAlt,
            Email                 = c.Email,
            Website               = c.Website,
            Address               = c.Address,
            City                  = c.City,
            State                 = c.State,
            ZipCode               = c.ZipCode,
            Country               = c.Country,
            AccountNumber         = c.AccountNumber,
            AccountRepName        = c.AccountRepName,
            AccountRepPhone       = c.AccountRepPhone,
            AccountRepEmail       = c.AccountRepEmail,
            ServiceTypes          = c.ServiceTypes,
            EstimatedDeliveryDays = c.EstimatedDeliveryDays,
            Coverage              = c.Coverage,
            BasePrice             = c.BasePrice,
            PricePerKg            = c.PricePerKg,
            ExpressPricePerKg     = c.ExpressPricePerKg,
            MaxWeightKg           = c.MaxWeightKg,
            TrackingUrl           = c.TrackingUrl,
            ApiEndpoint           = c.ApiEndpoint,
            LogoUrl               = c.LogoUrl,
            IsActive              = c.IsActive,
            Notes                 = c.Notes,
            CreatedAt             = c.CreatedAt,
            UpdatedAt             = c.UpdatedAt,
        };
    }
}
