using Application.DTOs.Quotations;
using MediatR;

namespace Application.Core.Quotations.Commands
{
    public record CreateQuotationCommand(
        CreateQuotationRequestDto Data,
        int UserId
    ) : IRequest<QuotationResponseDto>;

    public record CancelQuotationCommand(
        int QuotationId,
        string Reason,
        int UserId
    ) : IRequest<QuotationResponseDto>;

    /// <summary>
    /// Convierte la cotización en una Sale al escanear el QR.
    /// Puede identificarse por Id o por Code (desde el QR).
    /// </summary>
    public record ConvertQuotationToSaleCommand(
        int QuotationId,
        ConvertQuotationToSaleDto Data,
        int UserId
    ) : IRequest<ConvertQuotationResponseDto>;
}
