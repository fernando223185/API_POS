using Application.DTOs.Branch;
using MediatR;

namespace Application.Core.Branch.Queries
{
    /// <summary>
    /// Query para obtener todas las sucursales
    /// </summary>
    public class GetAllBranchesQuery : IRequest<BranchesListResponseDto>
    {
        public bool IncludeInactive { get; set; }

        public GetAllBranchesQuery(bool includeInactive = false)
        {
            IncludeInactive = includeInactive;
        }
    }

    /// <summary>
    /// Query para obtener sucursales paginadas
    /// </summary>
    public class GetBranchesPagedQuery : IRequest<BranchesPagedResponseDto>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool IncludeInactive { get; set; }
        public string? SearchTerm { get; set; }

        public GetBranchesPagedQuery(int pageNumber, int pageSize, bool includeInactive = false, string? searchTerm = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            IncludeInactive = includeInactive;
            SearchTerm = searchTerm;
        }
    }

    /// <summary>
    /// Query para obtener una sucursal por ID
    /// </summary>
    public class GetBranchByIdQuery : IRequest<BranchResponseDto?>
    {
        public int BranchId { get; set; }

        public GetBranchByIdQuery(int branchId)
        {
            BranchId = branchId;
        }
    }

    /// <summary>
    /// Query para obtener una sucursal por código
    /// </summary>
    public class GetBranchByCodeQuery : IRequest<BranchResponseDto?>
    {
        public string Code { get; set; }

        public GetBranchByCodeQuery(string code)
        {
            Code = code;
        }
    }
}
