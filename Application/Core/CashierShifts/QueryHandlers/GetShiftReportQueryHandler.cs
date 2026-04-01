using Application.Abstractions.CashierShifts;
using Application.Abstractions.Sales;
using Application.Core.CashierShifts.Queries;
using Application.DTOs.CashierShift;
using MediatR;

namespace Application.Core.CashierShifts.QueryHandlers
{
    /// <summary>
    /// Handler para generar reporte detallado de un turno
    /// Incluye: datos del turno, resumen por método de pago, flujo de efectivo, y listado de ventas
    /// </summary>
    public class GetShiftReportQueryHandler : IRequestHandler<GetShiftReportQuery, ShiftReportDto>
    {
        private readonly ICashierShiftRepository _shiftRepository;
        private readonly ISaleRepository _saleRepository;

        public GetShiftReportQueryHandler(
            ICashierShiftRepository shiftRepository,
            ISaleRepository saleRepository)
        {
            _shiftRepository = shiftRepository;
            _saleRepository = saleRepository;
        }

        public async Task<ShiftReportDto> Handle(GetShiftReportQuery query, CancellationToken cancellationToken)
        {
            // 1. Obtener el turno
            var shift = await _shiftRepository.GetByIdAsync(query.ShiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con ID {query.ShiftId} no encontrado");
            }

            // 2. Obtener todas las ventas del turno
            var endDate = shift.ClosingDate ?? DateTime.UtcNow;
            var (sales, _) = await _saleRepository.GetPagedAsync(
                page: 1,
                pageSize: 10000,
                warehouseId: shift.WarehouseId,
                userId: shift.CashierId,
                fromDate: shift.OpeningDate,
                toDate: endDate,
                status: "Completed" // Solo ventas completadas para el reporte
            );

            var salesList = sales.ToList();

            // 3. Calcular resumen por método de pago
            var paymentSummaries = new List<PaymentMethodSummaryDto>();
            var totalSalesAmount = salesList.Sum(s => s.Total);

            // Agrupar por método de pago principal
            var cashAmount = shift.CashSales;
            var cardAmount = shift.CardSales;
            var transferAmount = shift.TransferSales;
            var otherAmount = shift.OtherSales;

            if (cashAmount > 0)
            {
                paymentSummaries.Add(new PaymentMethodSummaryDto
                {
                    PaymentMethod = "Efectivo",
                    Count = salesList.Count(s => s.Payments?.Any(p => p.PaymentMethod == "Efectivo") == true),
                    Amount = cashAmount,
                    Percentage = totalSalesAmount > 0 ? (cashAmount / totalSalesAmount) * 100 : 0
                });
            }

            if (cardAmount > 0)
            {
                paymentSummaries.Add(new PaymentMethodSummaryDto
                {
                    PaymentMethod = "Tarjeta",
                    Count = salesList.Count(s => s.Payments?.Any(p => p.PaymentMethod == "Tarjeta") == true),
                    Amount = cardAmount,
                    Percentage = totalSalesAmount > 0 ? (cardAmount / totalSalesAmount) * 100 : 0
                });
            }

            if (transferAmount > 0)
            {
                paymentSummaries.Add(new PaymentMethodSummaryDto
                {
                    PaymentMethod = "Transferencia",
                    Count = salesList.Count(s => s.Payments?.Any(p => p.PaymentMethod == "Transferencia") == true),
                    Amount = transferAmount,
                    Percentage = totalSalesAmount > 0 ? (transferAmount / totalSalesAmount) * 100 : 0
                });
            }

            if (otherAmount > 0)
            {
                paymentSummaries.Add(new PaymentMethodSummaryDto
                {
                    PaymentMethod = "Otros",
                    Count = salesList.Count(s => s.Payments?.Any(p => 
                        p.PaymentMethod != "Efectivo" && 
                        p.PaymentMethod != "Tarjeta" && 
                        p.PaymentMethod != "Transferencia") == true),
                    Amount = otherAmount,
                    Percentage = totalSalesAmount > 0 ? (otherAmount / totalSalesAmount) * 100 : 0
                });
            }

            // 4. Calcular flujo de efectivo
            var cashFlow = new CashFlowSummaryDto
            {
                InitialCash = shift.InitialCash,
                CashSalesIn = shift.CashSales,
                CashDepositsIn = shift.CashDeposits,
                CashWithdrawalsOut = shift.CashWithdrawals,
                ExpectedCash = shift.ExpectedCash,
                FinalCash = shift.FinalCash ?? 0m,
                Difference = shift.Difference ?? 0m
            };

            // 5. Mapear ventas
            var shiftSales = salesList.Select(s => new ShiftSaleDto
            {
                Id = s.Id,
                Code = s.Code,
                SaleDate = s.SaleDate,
                CustomerName = s.Customer != null ? $"{s.Customer.Name} {s.Customer.LastName}" : "Público General",
                Total = s.Total,
                Status = s.Status,
                PaymentMethods = s.Payments?.Select(p => p.PaymentMethod).Distinct().ToList() ?? new List<string>()
            }).ToList();

            // 6. Construir DTO del reporte
            var report = new ShiftReportDto
            {
                Shift = new CashierShiftResponseDto
                {
                    Id = shift.Id,
                    Code = shift.Code,
                    CashierId = shift.CashierId,
                    CashierName = shift.Cashier.Name,
                    CashierCode = shift.Cashier.Code,
                    WarehouseId = shift.WarehouseId,
                    WarehouseName = shift.Warehouse.Name,
                    BranchId = shift.BranchId,
                    BranchName = shift.Branch?.Name,
                    CompanyId = shift.CompanyId,
                    CompanyName = shift.Company?.LegalName,
                    OpeningDate = shift.OpeningDate,
                    ClosingDate = shift.ClosingDate,
                    Status = shift.Status,
                    InitialCash = shift.InitialCash,
                    ExpectedCash = shift.ExpectedCash,
                    FinalCash = shift.FinalCash,
                    Difference = shift.Difference,
                    TotalSales = shift.TotalSales,
                    CancelledSales = shift.CancelledSales,
                    TotalSalesAmount = shift.TotalSalesAmount,
                    CancelledSalesAmount = shift.CancelledSalesAmount,
                    CashSales = shift.CashSales,
                    CardSales = shift.CardSales,
                    TransferSales = shift.TransferSales,
                    OtherSales = shift.OtherSales,
                    CashWithdrawals = shift.CashWithdrawals,
                    CashDeposits = shift.CashDeposits,
                    OpeningNotes = shift.OpeningNotes,
                    ClosingNotes = shift.ClosingNotes,
                    CancellationReason = shift.CancellationReason,
                    CreatedAt = shift.CreatedAt,
                    ClosedByUserId = shift.ClosedByUserId,
                    ClosedByName = shift.ClosedBy?.Name,
                    CancelledByUserId = shift.CancelledByUserId,
                    CancelledByName = shift.CancelledBy?.Name,
                    CancelledAt = shift.CancelledAt
                },
                PaymentMethodSummary = paymentSummaries,
                CashFlow = cashFlow,
                Sales = shiftSales
            };

            return report;
        }

        /// <summary>
        /// Obtiene el método de pago dominante de una venta
        /// </summary>
        private static string GetPrimaryPaymentMethod(Domain.Entities.Sale sale)
        {
            if (sale.Payments == null || !sale.Payments.Any())
            {
                return "Sin pago";
            }

            // Si solo hay un pago, retornar ese método
            if (sale.Payments.Count == 1)
            {
                return sale.Payments.First().PaymentMethod;
            }

            // Si hay múltiples pagos, retornar el de mayor monto
            var primaryPayment = sale.Payments.OrderByDescending(p => p.Amount).First();
            return primaryPayment.PaymentMethod;
        }
    }
}
