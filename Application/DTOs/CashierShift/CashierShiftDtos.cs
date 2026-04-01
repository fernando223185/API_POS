using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CashierShift
{
    /// <summary>
    /// DTO para abrir un nuevo turno de cajero
    /// </summary>
    public class OpenShiftRequestDto
    {
        [Required(ErrorMessage = "La sucursal es requerida")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "El fondo inicial es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El fondo inicial debe ser mayor o igual a 0")]
        public decimal InitialCash { get; set; }

        [MaxLength(1000)]
        public string? OpeningNotes { get; set; }
    }

    /// <summary>
    /// DTO para cerrar un turno de cajero
    /// </summary>
    public class CloseShiftRequestDto
    {
        [Required(ErrorMessage = "El efectivo final es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El efectivo final debe ser mayor o igual a 0")]
        public decimal FinalCash { get; set; }

        [MaxLength(1000)]
        public string? ClosingNotes { get; set; }

        /// <summary>
        /// Desglose de billetes y monedas (opcional)
        /// </summary>
        public CashBreakdownDto? CashBreakdown { get; set; }
    }

    /// <summary>
    /// Desglose de efectivo por denominación
    /// </summary>
    public class CashBreakdownDto
    {
        public int Coins001 { get; set; }  // $0.10
        public int Coins005 { get; set; }  // $0.50
        public int Coins01 { get; set; }   // $1
        public int Coins02 { get; set; }   // $2
        public int Coins05 { get; set; }   // $5
        public int Coins10 { get; set; }   // $10
        public int Bills20 { get; set; }   // $20
        public int Bills50 { get; set; }   // $50
        public int Bills100 { get; set; }  // $100
        public int Bills200 { get; set; }  // $200
        public int Bills500 { get; set; }  // $500
        public int Bills1000 { get; set; } // $1000

        public decimal CalculateTotal()
        {
            return (Coins001 * 0.10m) +
                   (Coins005 * 0.50m) +
                   (Coins01 * 1m) +
                   (Coins02 * 2m) +
                   (Coins05 * 5m) +
                   (Coins10 * 10m) +
                   (Bills20 * 20m) +
                   (Bills50 * 50m) +
                   (Bills100 * 100m) +
                   (Bills200 * 200m) +
                   (Bills500 * 500m) +
                   (Bills1000 * 1000m);
        }
    }

    /// <summary>
    /// DTO  de respuesta de un turno
    /// </summary>
    public class CashierShiftResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;

        // Cajero
        public int CashierId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public string CashierCode { get; set; } = string.Empty;

        // Ubicación
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }

        // Fechas
        public DateTime OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Efectivo
        public decimal InitialCash { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal? FinalCash { get; set; }
        public decimal? Difference { get; set; }

        // Ventas
        public int TotalSales { get; set; }
        public int CancelledSales { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal CancelledSalesAmount { get; set; }

        // Desglose por forma de pago
        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal TransferSales { get; set; }
        public decimal OtherSales { get; set; }

        // Movimientos de efectivo
        public decimal CashWithdrawals { get; set; }
        public decimal CashDeposits { get; set; }

        // Notas
        public string? OpeningNotes { get; set; }
        public string? ClosingNotes { get; set; }
        public string? CancellationReason { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public int? ClosedByUserId { get; set; }
        public string? ClosedByName { get; set; }
        public int? CancelledByUserId { get; set; }
        public string? CancelledByName { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Duración del turno
        public TimeSpan? Duration => ClosingDate.HasValue 
            ? ClosingDate.Value - OpeningDate 
            : DateTime.UtcNow - OpeningDate;
    }

    /// <summary>
    /// Reporte detallado de un turno de caja
    /// </summary>
    public class ShiftReportDto
    {
        public CashierShiftResponseDto Shift { get; set; } = null!;
        public List<PaymentMethodSummaryDto> PaymentMethodSummary { get; set; } = new();
        public CashFlowSummaryDto CashFlow { get; set; } = null!;
        public List<ShiftSaleDto> Sales { get; set; } = new();
    }

    public class PaymentMethodSummaryDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class CashFlowSummaryDto
    {
        public decimal InitialCash { get; set; }
        public decimal CashSalesIn { get; set; }
        public decimal CashDepositsIn { get; set; }
        public decimal CashWithdrawalsOut { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal FinalCash { get; set; }
        public decimal Difference { get; set; }
        public string DifferenceStatus => Difference switch
        {
            > 0 => "Sobrante",
            < 0 => "Faltante",
            _ => "Exacto"
        };
    }

    public class ShiftSaleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string? CustomerName { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> PaymentMethods { get; set; } = new();
    }

    /// <summary>
    /// Respuesta paginada de turnos
    /// </summary>
    public class CashierShiftPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<CashierShiftResponseDto> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
    }
}
