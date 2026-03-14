using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Company
{
    /// <summary>
    /// DTO para crear una nueva empresa
    /// </summary>
    public class CreateCompanyDto
    {
        [Required(ErrorMessage = "La razón social es requerida")]
        [StringLength(300, ErrorMessage = "La razón social no puede exceder 300 caracteres")]
        public string LegalName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(200, ErrorMessage = "El nombre comercial no puede exceder 200 caracteres")]
        public string TradeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RFC es requerido")]
        [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres")]
        [RegularExpression(@"^[A-ZÑ&]{3,4}\d{6}[A-V1-9][A-Z0-9][0-9A]$", ErrorMessage = "RFC inválido")]
        public string TaxId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El régimen fiscal es requerido")]
        [StringLength(10)]
        public string SatTaxRegime { get; set; } = "601";

        [Required(ErrorMessage = "El código postal fiscal es requerido")]
        [StringLength(10)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Código postal inválido")]
        public string FiscalZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección fiscal es requerida")]
        [StringLength(500)]
        public string FiscalAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(10)]
        public string? InvoiceSeries { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El folio inicial debe ser mayor a 0")]
        public int InvoiceStartingFolio { get; set; } = 1;

        [Required]
        [StringLength(3)]
        [RegularExpression(@"^(MXN|USD|EUR)$", ErrorMessage = "Moneda inválida (MXN, USD, EUR)")]
        public string DefaultCurrency { get; set; } = "MXN";

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? Slogan { get; set; }

        public bool IsMainCompany { get; set; } = false;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una empresa existente
    /// </summary>
    public class UpdateCompanyDto
    {
        [Required(ErrorMessage = "La razón social es requerida")]
        [StringLength(300)]
        public string LegalName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(200)]
        public string TradeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El régimen fiscal es requerido")]
        [StringLength(10)]
        public string SatTaxRegime { get; set; } = "601";

        [Required(ErrorMessage = "El código postal fiscal es requerido")]
        [StringLength(10)]
        public string FiscalZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección fiscal es requerida")]
        [StringLength(500)]
        public string FiscalAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(10)]
        public string? InvoiceSeries { get; set; }

        [Required]
        [StringLength(3)]
        public string DefaultCurrency { get; set; } = "MXN";

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? Slogan { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para operaciones de empresa
    /// </summary>
    public class CompanyResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string TradeName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string SatTaxRegime { get; set; } = string.Empty;
        public string FiscalZipCode { get; set; } = string.Empty;
        public string FiscalAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? InvoiceSeries { get; set; }
        public int InvoiceStartingFolio { get; set; }
        public int InvoiceCurrentFolio { get; set; }
        public string DefaultCurrency { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Slogan { get; set; }
        public bool IsActive { get; set; }
        public bool IsMainCompany { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? UpdatedByUserName { get; set; }
    }

    /// <summary>
    /// DTO paginado de empresas
    /// </summary>
    public class CompanyListResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<CompanyResponseDto> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// DTO para configuración fiscal avanzada
    /// </summary>
    public class UpdateCompanyFiscalConfigDto
    {
        [StringLength(500)]
        public string? SatCertificatePath { get; set; }

        [StringLength(500)]
        public string? SatKeyPath { get; set; }

        [StringLength(500)]
        public string? SatKeyPassword { get; set; }

        [Range(1, int.MaxValue)]
        public int? InvoiceCurrentFolio { get; set; }
    }
}
