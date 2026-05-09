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
        private readonly IUserRepository _userRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreateSaleCommandHandler(
            ISaleRepository saleRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IWarehouseRepository warehouseRepository,
            IUserRepository userRepository,
            ICodeGeneratorService codeGenerator)
        {
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _warehouseRepository = warehouseRepository;
            _userRepository = userRepository;
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

            // 3. Obtener el almacen principal de la sucursal del usuario
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new KeyNotFoundException("Usuario con ID " + request.UserId + " no encontrado");

            if (!user.Active)
            {
                throw new InvalidOperationException("El usuario no esta activo");
            }

            if (!user.BranchId.HasValue)
            {
                throw new InvalidOperationException("El usuario no tiene una sucursal asignada");
            }

            var warehouse = await _warehouseRepository.GetMainByBranchIdAsync(user.BranchId.Value)
                ?? throw new InvalidOperationException("La sucursal del usuario no tiene un almacen principal activo configurado");

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
            var code = await _codeGenerator.GenerateNextCodeAsync("SLA", "Sales", "Code", 5);

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
                WarehouseId = warehouse.Id,
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
                SaleType = "POS",
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
            var saleWithRelations = await _saleRepository.GetByIdAsync(sale.Id);
            if (saleWithRelations == null)
                throw new InvalidOperationException("Error al obtener la venta creada");

            return SaleMapper.ToResponseDto(saleWithRelations);
        }
    }
}
