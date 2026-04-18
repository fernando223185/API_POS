using Application.Abstractions.Quotations;
using Application.Core.Quotations.Commands;
using Application.DTOs.Quotations;
using MediatR;

namespace Application.Core.Quotations.CommandHandlers
{
    public class CancelQuotationCommandHandler : IRequestHandler<CancelQuotationCommand, QuotationResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;

        public CancelQuotationCommandHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationResponseDto> Handle(CancelQuotationCommand request, CancellationToken cancellationToken)
        {
            var quotation = await _quotationRepository.GetByIdAsync(request.QuotationId)
                ?? throw new KeyNotFoundException($"Cotización con ID {request.QuotationId} no encontrada");

            if (quotation.Status == "Converted")
                throw new InvalidOperationException("No se puede cancelar una cotización que ya fue convertida en venta");

            if (quotation.Status == "Cancelled")
                throw new InvalidOperationException("La cotización ya está cancelada");

            quotation.Status = "Cancelled";
            quotation.CancelledAt = DateTime.UtcNow;
            quotation.CancelledByUserId = request.UserId;
            quotation.CancellationReason = request.Reason;
            quotation.UpdatedAt = DateTime.UtcNow;

            await _quotationRepository.UpdateAsync(quotation);

            var updated = await _quotationRepository.GetByIdAsync(quotation.Id)
                ?? throw new InvalidOperationException("Error al obtener la cotización actualizada");

            return QuotationMapper.ToResponseDto(updated);
        }
    }
}
