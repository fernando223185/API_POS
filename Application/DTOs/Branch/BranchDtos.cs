using System;

namespace Application.DTOs.Branch
{
    /// <summary>
    /// DTO para crear una nueva sucursal
    /// </summary>
    public class CreateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "México";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public bool IsMainBranch { get; set; } = false;
        public DateTime? OpeningDate { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una sucursal existente
    /// </summary>
    public class UpdateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "México";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public bool IsMainBranch { get; set; }
        public DateTime? OpeningDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO de respuesta para sucursal
    /// </summary>
    public class BranchResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ManagerName { get; set; }
        public bool IsMainBranch { get; set; }
        public bool IsActive { get; set; }
        public DateTime? OpeningDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? UpdatedByUserName { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para lista de sucursales
    /// </summary>
    public class BranchesListResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<BranchResponseDto> Branches { get; set; } = new();
        public int TotalBranches { get; set; }
        public int ActiveBranches { get; set; }
        public int InactiveBranches { get; set; }
    }

    /// <summary>
    /// DTO para respuesta paginada de sucursales
    /// </summary>
    public class BranchesPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<BranchResponseDto> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
