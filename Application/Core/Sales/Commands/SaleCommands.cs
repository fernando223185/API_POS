using Application.DTOs.Sales;
using MediatR;

namespace Application.Core.Sales.Commands
{
    /// <summary>
    /// Command para crear una venta (estado Draft)
    /// </summary>
    public record CreateSaleCommand(
        CreateSaleRequestDto SaleData,
        int UserId
    ) : IRequest<SaleResponseDto>;

    /// <summary>
    /// Command para procesar pagos y completar la venta
    /// </summary>
    public record ProcessSalePaymentsCommand(
        int SaleId,
        ProcessSalePaymentsRequestDto PaymentsData,
        int UserId
    ) : IRequest<ProcessSalePaymentsResponseDto>;

    /// <summary>
    /// Command para cancelar una venta
    /// </summary>
    public record CancelSaleCommand(
        int SaleId,
        string Reason,
        int UserId
    ) : IRequest<SaleResponseDto>;
}
