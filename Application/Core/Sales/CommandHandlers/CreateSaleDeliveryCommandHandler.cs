using Application.Abstractions.Catalogue;
using Application.Abstractions.Common;
using Application.Abstractions.Config;
using Application.Abstractions.CRM;
using Application.Abstractions.Inventory;
using Application.Abstractions.Sales;
using Application.Core.Sales.Commands;
using Application.DTOs.Sales;
using Domain.Entities;
using MediatR;

namespace Application.Core.Sales.CommandHandlers
{
    /// <summary>
    /// Handler para crear una venta de tipo Delivery.
    /// Igual que POS pero fuerza SaleType = "Delivery" y requiere dirección de entrega.
    /// La venta queda en Draft — el pago se registra al confirmar la entrega.
    /// </summary>
    public class CreateSaleDeliveryCommandHandler : IRequestHandler<CreateSaleDeliveryCommand, SaleResponseDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreateSaleDeliveryCommandHandler(
            ISaleRepository saleRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IWarehouseRepository warehouseRepository,
            ICodeGeneratorService codeGenerator)
        {
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _warehouseRepository = warehouseRepository;
            _codeGenerator = codeGenerator;
        }

        public async Task<SaleResponseDto> Handle(CreateSaleDeliveryCommand request, CancellationToken cancellationToken)
        {
            if (!request.SaleData.Details.Any())
                throw new InvalidOperationException("Debe agregar al menos un producto a la venta");

            if (request.SaleData.CustomerId == 0) request.SaleData.CustomerId = null;
            if (request.SaleData.PriceListId == 0) request.SaleData.PriceListId = null;

            var warehouse = await _warehouseRepository.GetByIdAsync(request.SaleData.WarehouseId)
                ?? throw new KeyNotFoundException($"Almacén con ID {request.SaleData.WarehouseId} no encontrado");

            Customer? customer = null;
            if (request.SaleData.CustomerId.HasValue)
            {
                customer = await _customerRepository.GetByIdAsync(request.SaleData.CustomerId.Value)
                    ?? throw new KeyNotFoundException($"Cliente con ID {request.SaleData.CustomerId} no encontrado");
            }

            var code = await _codeGenerator.GenerateNextCodeAsync("ORA", "Sales", "Code", 5);

            decimal subTotal = 0;
            decimal totalTax = 0;
            decimal totalDiscount = 0;
            var details = new List<SaleDetail>();

            foreach (var detailDto in request.SaleData.Details)
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

                details.Add(new SaleDetail
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
                    UnitCost = product.BaseCost,
                    TotalCost = detailDto.Quantity * product.BaseCost,
                    Notes = detailDto.Notes,
                    SerialNumber = detailDto.SerialNumber,
                    LotNumber = detailDto.LotNumber
                });

                subTotal += lineAfterDiscount;
                totalTax += lineTax;
                totalDiscount += lineDiscount;
            }

            var globalDiscountAmount = subTotal * (request.SaleData.DiscountPercentage / 100);
            var finalSubTotal = subTotal - globalDiscountAmount;
            var finalTotalDiscount = totalDiscount + globalDiscountAmount;

            if (request.SaleData.DiscountPercentage > 0)
            {
                totalTax = 0;
                foreach (var detail in details)
                {
                    var proportion = detail.SubTotal / subTotal;
                    var newSubTotal = finalSubTotal * proportion;
                    var newTax = newSubTotal * detail.TaxPercentage;
                    detail.SubTotal = newSubTotal;
                    detail.TaxAmount = newTax;
                    detail.Total = newSubTotal + newTax;
                    totalTax += newTax;
                }
            }

            var finalTotal = finalSubTotal + totalTax;

            var sale = new Sale
            {
                Code = code,
                SaleDate = DateTime.UtcNow,
                CustomerId = request.SaleData.CustomerId,
                CustomerName = customer != null ? $"{customer.Name} {customer.LastName}" : "Público General",
                WarehouseId = request.SaleData.WarehouseId,
                BranchId = warehouse.BranchId,
                CompanyId = warehouse.Branch?.CompanyId,
                UserId = request.UserId,
                PriceListId = request.SaleData.PriceListId,
                SubTotal = subTotal,
                DiscountAmount = finalTotalDiscount,
                DiscountPercentage = request.SaleData.DiscountPercentage,
                TaxAmount = totalTax,
                Total = finalTotal,
                Status = "Draft",
                RequiresInvoice = request.SaleData.RequiresInvoice,
                SaleType = "Delivery",
                DeliveryAddress = request.SaleData.DeliveryAddress,
                ScheduledDeliveryDate = request.SaleData.ScheduledDeliveryDate,
                Notes = request.SaleData.Notes,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                Details = details
            };

            await _saleRepository.CreateAsync(sale);

            var saved = await _saleRepository.GetByIdAsync(sale.Id)
                ?? throw new InvalidOperationException("Error al obtener la venta creada");

            return SaleMapper.ToResponseDto(saved);
        }
    }
}
