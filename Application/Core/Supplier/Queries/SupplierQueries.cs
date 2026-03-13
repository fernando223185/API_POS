using Application.DTOs.Supplier;
using MediatR;

namespace Application.Core.Supplier.Queries
{
    /// <summary>
    /// Query para obtener todos los proveedores
    /// </summary>
    public class GetAllSuppliersQuery : IRequest<SuppliersListResponseDto>
    {
        public bool IncludeInactive { get; set; }

        public GetAllSuppliersQuery(bool includeInactive = false)
        {
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query para obtener proveedores paginados
    /// </summary>
    public class GetSuppliersPagedQuery : IRequest<SuppliersPagedResponseDto>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool IncludeInactive { get; set; }
        public string? SearchTerm { get; set; }

        public GetSuppliersPagedQuery(
            int pageNumber,
            int pageSize,
            bool includeInactive = false,
            string? searchTerm = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            IncludeInactive = includeInactive;
            SearchTerm = searchTerm;
        }
    }

    /// <summary>
    /// Query para obtener proveedor por ID
    /// </summary>
    public class GetSupplierByIdQuery : IRequest<SupplierResponseDto?>
    {
        public int SupplierId { get; set; }

        public GetSupplierByIdQuery(int supplierId)
        {
            SupplierId = supplierId;
        }
    }

    /// <summary>
    /// Query para obtener proveedor por código
    /// </summary>
    public class GetSupplierByCodeQuery : IRequest<SupplierResponseDto?>
    {
        public string Code { get; set; }

        public GetSupplierByCodeQuery(string code)
        {
            Code = code;
        }
    }
}
