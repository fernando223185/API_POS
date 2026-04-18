using Application.Abstractions.Common;
using Application.Abstractions.Quotations;
using Application.Abstractions.Sales;
using Application.Core.Quotations.Commands;
using Application.DTOs.Quotations;
using Application.DTOs.Sales;
using Application.Core.Sales;
using Domain.Entities;
using MediatR;

namespace Application.Core.Quotations.CommandHandlers
{
    /// <summary>
    /// Convierte una cotización en una Sale al escanear el QR.
    /// La venta queda en estado Draft lista para procesar el pago (POS) o entregar (Delivery).
    /// </summary>
    public class ConvertQuotationToSaleCommandHandler : IRequestHandler<ConvertQuotationToSaleCommand, ConvertQuotationResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public ConvertQuotationToSaleCommandHandler(
            IQuotationRepository quotationRepository,
            ISaleRepository saleRepository,
            ICodeGeneratorService codeGenerator)
        {
            _quotationRepository = quotationRepository;
            _saleRepository = saleRepository;
            _codeGenerator = codeGenerator;
        }

        public async Task<ConvertQuotationResponseDto> Handle(ConvertQuotationToSaleCommand request, CancellationToken cancellationToken)
        {
            var quotation = await _quotationRepository.GetByIdAsync(request.QuotationId)
                ?? throw new KeyNotFoundException($"Cotización con ID {request.QuotationId} no encontrada");

            if (quotation.Status == "Converted")
                throw new InvalidOperationException($"Esta cotización ya fue convertida en la venta {quotation.Sale?.Code}");

            if (quotation.Status == "Cancelled")
                throw new InvalidOperationException("No se puede convertir una cotización cancelada");

            if (quotation.ValidUntil.HasValue && quotation.ValidUntil.Value < DateTime.UtcNow)
                throw new InvalidOperationException("Esta cotización ha vencido y no puede convertirse en venta");

            var saleType = string.IsNullOrWhiteSpace(request.Data.SaleType) ? "POS" : request.Data.SaleType;

            var codePrefix = saleType == "Delivery" ? "ORA" : "SLA";
            var code = await _codeGenerator.GenerateNextCodeAsync(codePrefix, "Sales", "Code", 5);

            // Crear la Sale con los mismos productos y precios de la cotización
            var sale = new Sale
            {
                Code = code,
                SaleDate = DateTime.UtcNow,
                CustomerId = quotation.CustomerId,
                CustomerName = quotation.CustomerName,
                WarehouseId = quotation.WarehouseId,
                BranchId = quotation.BranchId,
                CompanyId = quotation.CompanyId,
                UserId = request.UserId,
                PriceListId = quotation.PriceListId,
                SubTotal = quotation.SubTotal,
                DiscountAmount = quotation.DiscountAmount,
                DiscountPercentage = quotation.DiscountPercentage,
                TaxAmount = quotation.TaxAmount,
                Total = quotation.Total,
                Status = "Draft",
                RequiresInvoice = quotation.RequiresInvoice,
                SaleType = saleType,
                DeliveryAddress = request.Data.DeliveryAddress,
                ScheduledDeliveryDate = request.Data.ScheduledDeliveryDate,
                Notes = request.Data.Notes ?? quotation.Notes,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                Details = quotation.Details.Select(d => new SaleDetail
                {
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    DiscountPercentage = d.DiscountPercentage,
                    DiscountAmount = d.DiscountAmount,
                    TaxPercentage = d.TaxPercentage,
                    TaxAmount = d.TaxAmount,
                    SubTotal = d.SubTotal,
                    Total = d.Total,
                    Notes = d.Notes
                }).ToList()
            };

            await _saleRepository.CreateAsync(sale);

            // Marcar cotización como convertida
            quotation.Status = "Converted";
            quotation.SaleId = sale.Id;
            quotation.ConvertedAt = DateTime.UtcNow;
            quotation.UpdatedAt = DateTime.UtcNow;
            await _quotationRepository.UpdateAsync(quotation);

            var updatedQuotation = await _quotationRepository.GetByIdAsync(quotation.Id)!
                ?? throw new InvalidOperationException("Error al obtener la cotización actualizada");

            return new ConvertQuotationResponseDto
            {
                Message = "Cotización convertida en venta exitosamente",
                Error = 0,
                Data = new ConvertQuotationDataDto
                {
                    Quotation = QuotationMapper.ToResponseDto(updatedQuotation),
                    SaleId = sale.Id,
                    SaleCode = sale.Code
                }
            };
        }
    }
}
