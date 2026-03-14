using Application.Abstractions.Config;
using Application.Core.Branch.Queries;
using Application.DTOs.Branch;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Core.Branch.QueryHandlers
{
    /// <summary>
    /// Handler para obtener todas las sucursales
    /// </summary>
    public class GetAllBranchesQueryHandler : IRequestHandler<GetAllBranchesQuery, BranchesListResponseDto>
    {
        private readonly IBranchRepository _branchRepository;

        public GetAllBranchesQueryHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<BranchesListResponseDto> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
        {
            var branches = await _branchRepository.GetAllAsync(request.IncludeInactive);

            var branchDtos = branches.Select(b => new BranchResponseDto
            {
                Id = b.Id,
                Code = b.Code,
                CompanyId = b.CompanyId,
                CompanyName = b.Company?.LegalName,
                Name = b.Name,
                Description = b.Description,
                Address = b.Address,
                City = b.City,
                State = b.State,
                ZipCode = b.ZipCode,
                Country = b.Country,
                Phone = b.Phone,
                Email = b.Email,
                ManagerName = b.ManagerName,
                IsMainBranch = b.IsMainBranch,
                IsActive = b.IsActive,
                OpeningDate = b.OpeningDate,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                CreatedByUserName = b.CreatedBy?.Name,
                UpdatedByUserName = b.UpdatedBy?.Name
            }).ToList();

            var totalBranches = await _branchRepository.GetTotalCountAsync();
            var activeBranches = await _branchRepository.GetActiveCountAsync();

            return new BranchesListResponseDto
            {
                Message = "Sucursales obtenidas exitosamente",
                Error = 0,
                Branches = branchDtos,
                TotalBranches = totalBranches,
                ActiveBranches = activeBranches,
                InactiveBranches = totalBranches - activeBranches
            };
        }
    }

    /// <summary>
    /// Handler para obtener sucursales paginadas
    /// </summary>
    public class GetBranchesPagedQueryHandler : IRequestHandler<GetBranchesPagedQuery, BranchesPagedResponseDto>
    {
        private readonly IBranchRepository _branchRepository;

        public GetBranchesPagedQueryHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<BranchesPagedResponseDto> Handle(GetBranchesPagedQuery request, CancellationToken cancellationToken)
        {
            var (branches, totalRecords) = await _branchRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.IncludeInactive,
                request.SearchTerm
            );

            var branchDtos = branches.Select(b => new BranchResponseDto
            {
                Id = b.Id,
                Code = b.Code,
                CompanyId = b.CompanyId,
                CompanyName = b.Company?.LegalName,
                Name = b.Name,
                Description = b.Description,
                Address = b.Address,
                City = b.City,
                State = b.State,
                ZipCode = b.ZipCode,
                Country = b.Country,
                Phone = b.Phone,
                Email = b.Email,
                ManagerName = b.ManagerName,
                IsMainBranch = b.IsMainBranch,
                IsActive = b.IsActive,
                OpeningDate = b.OpeningDate,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                CreatedByUserName = b.CreatedBy?.Name,
                UpdatedByUserName = b.UpdatedBy?.Name
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new BranchesPagedResponseDto
            {
                Message = "Sucursales obtenidas exitosamente",
                Error = 0,
                Data = branchDtos,
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
    /// Handler para obtener sucursal por ID
    /// </summary>
    public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchResponseDto?>
    {
        private readonly IBranchRepository _branchRepository;

        public GetBranchByIdQueryHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<BranchResponseDto?> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
            {
                return null;
            }

            return new BranchResponseDto
            {
                Id = branch.Id,
                Code = branch.Code,
                CompanyId = branch.CompanyId,
                CompanyName = branch.Company?.LegalName,
                Name = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                City = branch.City,
                State = branch.State,
                ZipCode = branch.ZipCode,
                Country = branch.Country,
                Phone = branch.Phone,
                Email = branch.Email,
                ManagerName = branch.ManagerName,
                IsMainBranch = branch.IsMainBranch,
                IsActive = branch.IsActive,
                OpeningDate = branch.OpeningDate,
                CreatedAt = branch.CreatedAt,
                UpdatedAt = branch.UpdatedAt,
                CreatedByUserName = branch.CreatedBy?.Name,
                UpdatedByUserName = branch.UpdatedBy?.Name
            };
        }
    }

    /// <summary>
    /// Handler para obtener sucursal por código
    /// </summary>
    public class GetBranchByCodeQueryHandler : IRequestHandler<GetBranchByCodeQuery, BranchResponseDto?>
    {
        private readonly IBranchRepository _branchRepository;

        public GetBranchByCodeQueryHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<BranchResponseDto?> Handle(GetBranchByCodeQuery request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByCodeAsync(request.Code);
            if (branch == null)
            {
                return null;
            }

            return new BranchResponseDto
            {
                Id = branch.Id,
                Code = branch.Code,
                CompanyId = branch.CompanyId,
                CompanyName = branch.Company?.LegalName,
                Name = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                City = branch.City,
                State = branch.State,
                ZipCode = branch.ZipCode,
                Country = branch.Country,
                Phone = branch.Phone,
                Email = branch.Email,
                ManagerName = branch.ManagerName,
                IsMainBranch = branch.IsMainBranch,
                IsActive = branch.IsActive,
                OpeningDate = branch.OpeningDate,
                CreatedAt = branch.CreatedAt,
                UpdatedAt = branch.UpdatedAt,
                CreatedByUserName = branch.CreatedBy?.Name,
                UpdatedByUserName = branch.UpdatedBy?.Name
            };
        }
    }
}
