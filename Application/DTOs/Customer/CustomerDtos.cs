using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Customer
{
    public class CreateCustomerRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // Información fiscal básica
        [Required]
        [StringLength(20)]
        public string TaxId { get; set; } = string.Empty; // RFC

        // Dirección detallada (campos originales)
        [Required]
        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Commentary { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }
        
        [Required]
        public int StateId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InteriorNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ExteriorNumber { get; set; } = string.Empty;

        [Required]
        public int StatusId { get; set; }

        // Información fiscal para CFDI
        [StringLength(200)]
        public string? CompanyName { get; set; } // Razón social (si es empresa, se usa para generar código)

        [StringLength(10)]
        public string? SatTaxRegime { get; set; } // Régimen fiscal SAT (601, 603, etc.)

        [StringLength(10)]
        public string? SatCfdiUse { get; set; } = "G03"; // Uso del CFDI por defecto

        // Lista de precios y configuraciones comerciales
        public int? PriceListId { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; } = 0;

        [Range(0, 365)]
        public int PaymentTermsDays { get; set; } = 0; 

        public bool IsActive { get; set; } = true;
    }

    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Commentary { get; set; } = string.Empty;
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string InteriorNumber { get; set; } = string.Empty;
        public string ExteriorNumber { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }

        // ? CAMPOS ERP AVANZADOS
        public string? CompanyName { get; set; }
        public string? SatTaxRegime { get; set; }
        public string? SatCfdiUse { get; set; }
        public int? PriceListId { get; set; }
        public string? PriceListName { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal CreditLimit { get; set; }
        public int PaymentTermsDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }

    // ? NUEVO: DTO específico para la tabla de clientes (optimizado para frontend)
    public class CustomerTableDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        
        // Cliente (Name + LastName)
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{Name} {LastName}".Trim();
        
        // Contacto (Phone + Email)
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Empresa (CompanyName o "Persona Física")
        public string? CompanyName { get; set; }
        public string DisplayCompany => string.IsNullOrEmpty(CompanyName) ? "Persona Física" : CompanyName;
        
        // Información fiscal
        public string TaxId { get; set; } = string.Empty;
        public string? SatTaxRegime { get; set; }
        
        // Fecha registro
        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedAt => CreatedAt.ToString("dd/MM/yyyy");
        
        // Estado
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Activo" : "Inactivo";
        public string StatusColor => IsActive ? "green" : "red";
        
        // Lista de precios
        public int? PriceListId { get; set; }
        public string? PriceListName { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal CreditLimit { get; set; }
        
        // Información adicional útil
        public string Address { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public int PaymentTermsDays { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Para acciones y auditoría
        public string? CreatedByName { get; set; }
        public int StatusId { get; set; }
    }

    // ? NUEVO: DTO para respuesta paginada
    public class PagedCustomersResponseDto
    {
        public List<CustomerTableDto> Customers { get; set; } = new();
        public PaginationInfoDto Pagination { get; set; } = new();
        public FilterInfoDto Filters { get; set; } = new();
    }

    // ? NUEVO: DTO para información de paginación
    public class PaginationInfoDto
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => ((CurrentPage - 1) * PageSize) + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }

    // ? NUEVO: DTO para información de filtros aplicados
    public class FilterInfoDto
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public bool? IsActive { get; set; }
        public int? StatusId { get; set; }
        public int? PriceListId { get; set; }
        public int ActiveFiltersCount { get; set; }
    }
}