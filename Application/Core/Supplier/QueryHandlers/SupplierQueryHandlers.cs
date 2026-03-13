using Application.Abstractions.Purchasing;
using Application.Core.Supplier.Queries;
using Application.DTOs.Supplier;
using MediatR;

namespace Application.Core.Supplier.QueryHandlers
{
    /// <summary>
    /// Helper para mapear Supplier a DTO
    /// </summary>
    internal static class SupplierMapper
    {
        internal static async Task<SupplierResponseDto> MapToResponseDtoAsync(
            Domain.Entities.Supplier supplier,
            ISupplierRepository repository)
        {
            var totalOrders = await repository.GetPurchaseOrderCountAsync(supplier.Id);
            var totalPurchased = await repository.GetTotalPurchasedAsync(supplier.Id);

            return new SupplierResponseDto
            {
                Id = supplier.Id,
                Code = supplier.Code,
                Name = supplier.Name,
                TaxId = supplier.TaxId,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                ZipCode = supplier.ZipCode,
                Country = supplier.Country,
                PaymentTermsDays = supplier.PaymentTermsDays,
                CreditLimit = supplier.CreditLimit,
                DefaultDiscountPercentage = supplier.DefaultDiscountPercentage,
                IsActive = supplier.IsActive,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt,
                TotalPurchaseOrders = totalOrders,
                TotalPurchased = totalPurchased
            };
        }
    }

    /// <summary>
    /// Handler para obtener todos los proveedores
    /// </summary>
    public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, SuppliersListResponseDto>
    {
        private readonly ISupplierRepository _supplierRepository;

        public GetAllSuppliersQueryHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<SuppliersListResponseDto> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken)
        {
            var suppliers = await _supplierRepository.GetAllAsync(request.IncludeInactive);

            var supplierDtos = new List<SupplierResponseDto>();
            foreach (var supplier in suppliers)
            {
                var dto = await SupplierMapper.MapToResponseDtoAsync(supplier, _supplierRepository);
                supplierDtos.Add(dto);
            }

            var totalSuppliers = await _supplierRepository.GetTotalCountAsync();
            var activeSuppliers = await _supplierRepository.GetActiveCountAsync();

            return new SuppliersListResponseDto
            {
                Message = "Proveedores obtenidos exitosamente",
                Error = 0,
                Suppliers = supplierDtos,
                TotalSuppliers = totalSuppliers,
                ActiveSuppliers = activeSuppliers,
                InactiveSuppliers = totalSuppliers - activeSuppliers
            };
        }
    }

    /// <summary>
    /// Handler para obtener proveedores paginados
    /// </summary>
    public class GetSuppliersPagedQueryHandler : IRequestHandler<GetSuppliersPagedQuery, SuppliersPagedResponseDto>
    {
        private readonly ISupplierRepository _supplierRepository;

        public GetSuppliersPagedQueryHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<SuppliersPagedResponseDto> Handle(GetSuppliersPagedQuery request, CancellationToken cancellationToken)
        {
            var (suppliers, totalRecords) = await _supplierRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.IncludeInactive,
                request.SearchTerm
            );

            var supplierDtos = new List<SupplierResponseDto>();
            foreach (var supplier in suppliers)
            {
                var dto = await SupplierMapper.MapToResponseDtoAsync(supplier, _supplierRepository);
                supplierDtos.Add(dto);
            }

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new SuppliersPagedResponseDto
            {
                Message = "Proveedores obtenidos exitosamente",
                Error = 0,
                Data = supplierDtos,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };
        }
    }

    /// <summary>
    /// Handler para obtener proveedor por ID
    /// </summary>
    public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierResponseDto?>
    {
        private readonly ISupplierRepository _supplierRepository;

        public GetSupplierByIdQueryHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<SupplierResponseDto?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);
            if (supplier == null)
            {
                return null;
            }

            return await SupplierMapper.MapToResponseDtoAsync(supplier, _supplierRepository);
        }
    }

    /// <summary>
    /// Handler para obtener proveedor por código
    /// </summary>
    public class GetSupplierByCodeQueryHandler : IRequestHandler<GetSupplierByCodeQuery, SupplierResponseDto?>
    {
        private readonly ISupplierRepository _supplierRepository;

        public GetSupplierByCodeQueryHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<SupplierResponseDto?> Handle(GetSupplierByCodeQuery request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.GetByCodeAsync(request.Code);
            if (supplier == null)
            {
                return null;
            }

            return await SupplierMapper.MapToResponseDtoAsync(supplier, _supplierRepository);
        }
    }
}
