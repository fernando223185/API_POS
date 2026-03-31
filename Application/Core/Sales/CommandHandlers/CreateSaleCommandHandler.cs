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
    /// Handler para crear una venta (estado Draft)
    /// </summary>
    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleResponseDto>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreateSaleCommandHandler(
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

        public async Task<SaleResponseDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que hay productos
            if (!request.SaleData.Details.Any())
            {
                throw new InvalidOperationException("Debe agregar al menos un producto a la venta");
            }

            // 2. Validar y normalizar IDs (convertir 0 a null)
            if (request.SaleData.PriceListId.HasValue && request.SaleData.PriceListId.Value == 0)
            {
                request.SaleData.PriceListId = null;
            }

            if (request.SaleData.CustomerId.HasValue && request.SaleData.CustomerId.Value == 0)
            {
                request.SaleData.CustomerId = null;
            }

            // 3. ? OBTENER WAREHOUSE CON BRANCH Y COMPANY
            var warehouse = await _warehouseRepository.GetByIdAsync(request.SaleData.WarehouseId);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"Almacén con ID {request.SaleData.WarehouseId} no encontrado");
            }

            // 4. Obtener cliente si existe
            Customer? customer = null;
            if (request.SaleData.CustomerId.HasValue)
            {
                customer = await _customerRepository.GetByIdAsync(request.SaleData.CustomerId.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Cliente con ID {request.SaleData.CustomerId} no encontrado");
                }
            }

            // 5. Generar c�digo autom�tico
            var code = await _codeGenerator.GenerateNextCodeAsync("VTA", "Sales");

            // 6. Calcular totales
            decimal subTotal = 0;
            decimal totalTax = 0;
            decimal totalDiscount = 0;
            var details = new List<SaleDetail>();

            foreach (var detailDto in request.SaleData.Details)
            {
                // Obtener producto
                var product = await _productRepository.GetByIdAsync(detailDto.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Producto con ID {detailDto.ProductId} no encontrado");
                }

                if (!product.IsActive)
                {
                    throw new InvalidOperationException($"El producto '{product.name}' está inactivo");
                }

                // Calcular montos del detalle
                var quantity = detailDto.Quantity;
                var unitPrice = detailDto.UnitPrice;
                var discountPct = detailDto.DiscountPercentage;

                var lineSubTotal = quantity * unitPrice;
                var lineDiscount = lineSubTotal * (discountPct / 100);
                var lineAfterDiscount = lineSubTotal - lineDiscount;
                var lineTax = lineAfterDiscount * product.TaxRate;
                var lineTotal = lineAfterDiscount + lineTax;

                var detail = new SaleDetail
                {
                    ProductId = product.ID,
                    ProductCode = product.code,
                    ProductName = product.name,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    DiscountPercentage = discountPct,
                    DiscountAmount = lineDiscount,
                    TaxPercentage = product.TaxRate,
                    TaxAmount = lineTax,
                    SubTotal = lineAfterDiscount,
                    Total = lineTotal,
                    UnitCost = product.BaseCost,
                    TotalCost = quantity * product.BaseCost,
                    Notes = detailDto.Notes,
                    SerialNumber = detailDto.SerialNumber,
                    LotNumber = detailDto.LotNumber
                };

                details.Add(detail);
                subTotal += lineAfterDiscount;
                totalTax += lineTax;
                totalDiscount += lineDiscount;
            }

            // 7. Aplicar descuento global (sobre el subtotal despu�s de descuentos individuales)
            var globalDiscountAmount = subTotal * (request.SaleData.DiscountPercentage / 100);
            var finalSubTotal = subTotal - globalDiscountAmount;
            var finalTotalDiscount = totalDiscount + globalDiscountAmount;

            // 8. Recalcular impuestos si hay descuento global
            if (request.SaleData.DiscountPercentage > 0)
            {
                // Redistribuir impuestos proporcionalmente
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

            // 9. ? CREAR VENTA CON BRANCHID Y COMPANYID AUTOM�TICOS
            var sale = new Sale
            {
                Code = code,
                SaleDate = DateTime.UtcNow,
                CustomerId = request.SaleData.CustomerId,
                CustomerName = customer != null ? $"{customer.Name} {customer.LastName}" : "Público General",
                WarehouseId = request.SaleData.WarehouseId,
                BranchId = warehouse.BranchId,                      // ? ASIGNADO AUTOM�TICAMENTE
                CompanyId = warehouse.Branch?.CompanyId,            // ? ASIGNADO AUTOM�TICAMENTE
                UserId = request.UserId,
                PriceListId = request.SaleData.PriceListId,
                SubTotal = subTotal, // Subtotal antes de descuento global
                DiscountAmount = finalTotalDiscount,
                DiscountPercentage = request.SaleData.DiscountPercentage,
                TaxAmount = totalTax,
                Total = finalTotal,
                Status = "Draft",
                RequiresInvoice = request.SaleData.RequiresInvoice,
                Notes = request.SaleData.Notes,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                Details = details
            };

            // 10. Guardar en base de datos
            await _saleRepository.CreateAsync(sale);

            Console.WriteLine($"? Venta {sale.Code} creada exitosamente en estado Draft");
            Console.WriteLine($"   ?? Warehouse: {warehouse.Name}");
            Console.WriteLine($"   ?? Branch: {warehouse.Branch?.Name}");
            Console.WriteLine($"   ?? Company: {warehouse.Branch?.Company?.LegalName}");

            // 11. Mapear a DTO de respuesta
            return await MapToResponseDto(sale);
        }

        private async Task<SaleResponseDto> MapToResponseDto(Sale sale)
        {
            // Recargar con todas las relaciones
            var saleWithRelations = await _saleRepository.GetByIdAsync(sale.Id);
            if (saleWithRelations == null)
            {
                throw new InvalidOperationException("Error al obtener la venta creada");
            }

            var totalCost = saleWithRelations.Details.Sum(d => d.TotalCost ?? 0);
            var grossProfit = saleWithRelations.Total - totalCost;
            var profitMargin = saleWithRelations.Total > 0 ? (grossProfit / saleWithRelations.Total) * 100 : 0;

            return new SaleResponseDto
            {
                Id = saleWithRelations.Id,
                Code = saleWithRelations.Code,
                SaleDate = saleWithRelations.SaleDate,
                CustomerId = saleWithRelations.CustomerId,
                CustomerName = saleWithRelations.CustomerName,
                WarehouseId = saleWithRelations.WarehouseId,
                WarehouseName = saleWithRelations.Warehouse.Name,
                BranchId = saleWithRelations.BranchId,                       // ? NUEVO
                BranchName = saleWithRelations.Branch?.Name,                 // ? ACTUALIZADO
                CompanyId = saleWithRelations.CompanyId,                     // ? NUEVO
                CompanyName = saleWithRelations.Company?.LegalName,          // ? NUEVO
                UserId = saleWithRelations.UserId,
                UserName = saleWithRelations.User.Name,
                PriceListId = saleWithRelations.PriceListId,
                PriceListName = saleWithRelations.PriceList?.Name,
                SubTotal = saleWithRelations.SubTotal,
                DiscountAmount = saleWithRelations.DiscountAmount,
                DiscountPercentage = saleWithRelations.DiscountPercentage,
                TaxAmount = saleWithRelations.TaxAmount,
                Total = saleWithRelations.Total,
                AmountPaid = saleWithRelations.AmountPaid,
                ChangeAmount = saleWithRelations.ChangeAmount,
                IsPaid = saleWithRelations.IsPaid,
                Status = saleWithRelations.Status,
                IsPostedToInventory = saleWithRelations.IsPostedToInventory,
                PostedToInventoryDate = saleWithRelations.PostedToInventoryDate,
                RequiresInvoice = saleWithRelations.RequiresInvoice,
                InvoiceUuid = saleWithRelations.InvoiceUuid,
                Notes = saleWithRelations.Notes,
                CreatedAt = saleWithRelations.CreatedAt,
                CreatedByName = saleWithRelations.CreatedBy?.Name,
                CancelledAt = saleWithRelations.CancelledAt,
                CancellationReason = saleWithRelations.CancellationReason,
                Details = saleWithRelations.Details.Select(d => new SaleDetailResponseDto
                {
                    Id = d.Id,
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
                    UnitCost = d.UnitCost,
                    TotalCost = d.TotalCost,
                    LineProfit = d.Total - (d.TotalCost ?? 0),
                    Notes = d.Notes,
                    SerialNumber = d.SerialNumber,
                    LotNumber = d.LotNumber
                }).ToList(),
                Payments = saleWithRelations.Payments.Select(p => new SalePaymentResponseDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    CardNumber = p.CardNumber,
                    CardType = p.CardType,
                    AuthorizationCode = p.AuthorizationCode,
                    TransactionReference = p.TransactionReference,
                    TerminalId = p.TerminalId,
                    BankName = p.BankName,
                    TransferReference = p.TransferReference,
                    CheckNumber = p.CheckNumber,
                    CheckBank = p.CheckBank,
                    Status = p.Status,
                    Notes = p.Notes
                }).ToList(),
                TotalItems = saleWithRelations.Details.Count,
                TotalQuantity = saleWithRelations.Details.Sum(d => d.Quantity),
                TotalCost = totalCost,
                GrossProfit = grossProfit,
                ProfitMarginPercentage = profitMargin
            };
        }
    }
}
