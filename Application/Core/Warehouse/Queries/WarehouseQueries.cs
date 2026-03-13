using Application.DTOs.Warehouse;
using MediatR;

namespace Application.Core.Warehouse.Queries
{
    /// <summary>
    /// Query para obtener todos los almacenes
    /// </summary>
    public class GetAllWarehousesQuery : IRequest<WarehousesListResponseDto>
    {
        public bool IncludeInactive { get; set; }

        public GetAllWarehousesQuery(bool includeInactive = false)
        {
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query para obtener almacenes paginados
    /// </summary>
    public class GetWarehousesPagedQuery : IRequest<WarehousesPagedResponseDto>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool IncludeInactive { get; set; }
        public string? SearchTerm { get; set; }
        public int? BranchId { get; set; }

        public GetWarehousesPagedQuery(int pageNumber, int pageSize, bool includeInactive = false, string? searchTerm = null, int? branchId = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            IncludeInactive = includeInactive;
            SearchTerm = searchTerm;
            BranchId = branchId;
        }
    }

    /// <summary>
    /// Query para obtener un almacén por ID
    /// </summary>
    public class GetWarehouseByIdQuery : IRequest<WarehouseResponseDto?>
    {
        public int WarehouseId { get; set; }

        public GetWarehouseByIdQuery(int warehouseId)
        {
            WarehouseId = warehouseId;
        }
    }

    /// <summary>
    /// Query para obtener un almacén por código
    /// </summary>
    public class GetWarehouseByCodeQuery : IRequest<WarehouseResponseDto?>
    {
        public string Code { get; set; }

        public GetWarehouseByCodeQuery(string code)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Query para obtener almacenes por sucursal
    /// </summary>
    public class GetWarehousesByBranchQuery : IRequest<WarehousesByBranchResponseDto>
    {
        public int BranchId { get; set; }
        public bool IncludeInactive { get; set; }

        public GetWarehousesByBranchQuery(int branchId, bool includeInactive = false)
        {
            BranchId = branchId;
            IncludeInactive = includeInactive;
        }
    }
}
