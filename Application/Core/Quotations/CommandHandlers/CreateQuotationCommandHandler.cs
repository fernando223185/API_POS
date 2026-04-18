using Application.Abstractions.Catalogue;
using Application.Abstractions.Common;
using Application.Abstractions.Config;
using Application.Abstractions.CRM;
using Application.Abstractions.Quotations;
using Application.Core.Quotations.Commands;
using Application.DTOs.Quotations;
using Domain.Entities;
using MediatR;

namespace Application.Core.Quotations.CommandHandlers
{
    public class CreateQuotationCommandHandler : IRequestHandler<CreateQuotationCommand, QuotationResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreateQuotationCommandHandler(
            IQuotationRepository quotationRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IWarehouseRepository warehouseRepository,
            ICodeGeneratorService codeGenerator)
        {
            _quotationRepository = quotationRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _warehouseRepository = warehouseRepository;
            _codeGenerator = codeGenerator;
        }

        public async Task<QuotationResponseDto> Handle(CreateQuotationCommand request, CancellationToken cancellationToken)
        {
            if (!request.Data.Details.Any())
                throw new InvalidOperationException("Debe agregar al menos un producto a la cotización");

            if (request.Data.CustomerId == 0) request.Data.CustomerId = null;
            if (request.Data.PriceListId == 0) request.Data.PriceListId = null;

            var warehouse = await _warehouseRepository.GetByIdAsync(request.Data.WarehouseId)
                ?? throw new KeyNotFoundException($"Almacén con ID {request.Data.WarehouseId} no encontrado");

            Customer? customer = null;
            if (request.Data.CustomerId.HasValue)
            {
                customer = await _customerRepository.GetByIdAsync(request.Data.CustomerId.Value)
                    ?? throw new KeyNotFoundException($"Cliente con ID {request.Data.CustomerId} no encontrado");
            }

            var code = await _codeGenerator.GenerateNextCodeAsync("COT", "Quotations");

            decimal subTotal = 0;
            decimal totalTax = 0;
            decimal totalDiscount = 0;
            var details = new List<QuotationDetail>();

            foreach (var detailDto in request.Data.Details)
            {
                var product = await _productRepository.GetByIdAsync(detailDto.ProductId)
                    ?? throw new KeyNotFoundException($"Producto con ID {detailDto.ProductId} no encontrado");

                if (!product.IsActive)
                    throw new InvalidOperationException($"El producto '{product.name}' está inactivo");

                var lineSubTotal = detailDto.Quantity * detailDto.UnitPrice;
                var lineDiscount = lineSubTotal * (detailDto.DiscountPercentage / 100);
                var lineAfterDiscount = lineSubTotal - lineDiscount;
                var lineTax = lineAfterDiscount * product.TaxRate;
                var lineTotal = lineAfterDiscount + lineTax;

                details.Add(new QuotationDetail
                {
                    ProductId = product.ID,
                    ProductCode = product.code,
                    ProductName = product.name,
                    Quantity = detailDto.Quantity,
                    UnitPrice = detailDto.UnitPrice,
                    DiscountPercentage = detailDto.DiscountPercentage,
                    DiscountAmount = lineDiscount,
                    TaxPercentage = product.TaxRate,
                    TaxAmount = lineTax,
                    SubTotal = lineAfterDiscount,
                    Total = lineTotal,
                    Notes = detailDto.Notes
                });

                subTotal += lineAfterDiscount;
                totalTax += lineTax;
                totalDiscount += lineDiscount;
            }

            var globalDiscount = subTotal * (request.Data.DiscountPercentage / 100);
            var finalSubTotal = subTotal - globalDiscount;
            var finalTotalDiscount = totalDiscount + globalDiscount;

            if (request.Data.DiscountPercentage > 0)
            {
                totalTax = 0;
                foreach (var d in details)
                {
                    var proportion = d.SubTotal / subTotal;
                    var newSubTotal = finalSubTotal * proportion;
                    var newTax = newSubTotal * d.TaxPercentage;
                    d.SubTotal = newSubTotal;
                    d.TaxAmount = newTax;
                    d.Total = newSubTotal + newTax;
                    totalTax += newTax;
                }
            }

            var finalTotal = finalSubTotal + totalTax;

            var quotation = new Quotation
            {
                Code = code,
                QuotationDate = DateTime.UtcNow,
                ValidUntil = request.Data.ValidUntil,
                CustomerId = request.Data.CustomerId,
                CustomerName = customer != null ? $"{customer.Name} {customer.LastName}" : "Público General",
                WarehouseId = request.Data.WarehouseId,
                BranchId = warehouse.BranchId,
                CompanyId = warehouse.Branch?.CompanyId,
                UserId = request.UserId,
                PriceListId = request.Data.PriceListId,
                SubTotal = finalSubTotal,
                DiscountAmount = finalTotalDiscount,
                DiscountPercentage = request.Data.DiscountPercentage,
                TaxAmount = totalTax,
                Total = finalTotal,
                Status = "Draft",
                RequiresInvoice = request.Data.RequiresInvoice,
                Notes = request.Data.Notes,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                Details = details
            };

            await _quotationRepository.CreateAsync(quotation);

            var saved = await _quotationRepository.GetByIdAsync(quotation.Id)
                ?? throw new InvalidOperationException("Error al obtener la cotización creada");

            return QuotationMapper.ToResponseDto(saved);
        }
    }
}
