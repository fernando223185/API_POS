# ?? Sistema de Ventas con Cobranza - Estado de Implementación

## ? Completado

### **1. Diseño y Documentación**
- ? Documento de diseño completo (`DOCS/Sales_System_Complete_Design.md`)
- ? Definición de tablas y relaciones
- ? Flujo de venta documentado

### **2. Migración de Base de Datos**
- ? Migración creada (`20260310000000_CreateSalesAndPaymentsTables.cs`)
- ? Tabla `Sales` con foliado automático
- ? Tabla `SaleDetails` para productos
- ? Tabla `SalePayments` para múltiples formas de pago
- ? Campo `SaleId` agregado a `InventoryMovements`

### **3. Entidades del Dominio**
- ? `Sale.cs` - Entidad principal de venta
- ? `SaleDetail.cs` - Detalle de productos
- ? `SalePayment.cs` - Formas de pago
- ? `InventoryMovement.cs` - Actualizado con SaleId

### **4. Configuración de EF Core**
- ? DbSets agregados en `POSDbContext`
- ? Configuración de relaciones en `OnModelCreating`
- ? Índices y restricciones configuradas

---

## ?? Pendiente de Implementar

### **5. DTOs (Application/DTOs/Sales/)**

```csharp
// SaleDtos.cs
public class CreateSaleRequestDto
{
    public int? CustomerId { get; set; }
    public int WarehouseId { get; set; }
    public int? PriceListId { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool RequiresInvoice { get; set; }
    public string? Notes { get; set; }
    public List<CreateSaleDetailDto> Details { get; set; } = new();
}

public class CreateSaleDetailDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? Notes { get; set; }
}

public class ProcessSalePaymentsRequestDto
{
    public List<CreateSalePaymentDto> Payments { get; set; } = new();
}

public class CreateSalePaymentDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    
    // Tarjeta
    public string? CardNumber { get; set; }
    public string? CardType { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? TransactionReference { get; set; }
    public string? TerminalId { get; set; }
    public string? BankName { get; set; }
    
    // Transfer/Check
    public string? TransferReference { get; set; }
    public string? CheckNumber { get; set; }
    public string? CheckBank { get; set; }
    public string? Notes { get; set; }
}

public class SaleResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    
    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }
    public bool IsPaid { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public bool IsPostedToInventory { get; set; }
    
    public List<SaleDetailResponseDto> Details { get; set; } = new();
    public List<SalePaymentResponseDto> Payments { get; set; } = new();
}
```

### **6. Repositorios (Infrastructure/Repositories/)**

```csharp
// ISaleRepository.cs
public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale sale);
    Task<Sale?> GetByIdAsync(int id);
    Task<Sale?> GetByCodeAsync(string code);
    Task<Sale> UpdateAsync(Sale sale);
    Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPagedAsync(
        int page, int pageSize, 
        int? warehouseId = null, 
        int? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? status = null
    );
}

// SaleRepository.cs
public class SaleRepository : ISaleRepository
{
    private readonly POSDbContext _context;
    
    public async Task<Sale> CreateAsync(Sale sale)
    {
        _context.SalesNew.Add(sale);
        await _context.SaveChangesAsync();
        return sale;
    }
    
    public async Task<Sale?> GetByIdAsync(int id)
    {
        return await _context.SalesNew
            .Include(s => s.Details)
                .ThenInclude(d => d.Product)
            .Include(s => s.Payments)
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
    
    // ... otros métodos
}
```

### **7. Commands (Application/Core/Sales/Commands/)**

```csharp
// CreateSaleCommand.cs
public record CreateSaleCommand(
    CreateSaleRequestDto SaleData,
    int UserId
) : IRequest<SaleResponseDto>;

// ProcessSalePaymentsCommand.cs
public record ProcessSalePaymentsCommand(
    int SaleId,
    ProcessSalePaymentsRequestDto PaymentsData,
    int UserId
) : IRequest<ProcessSalePaymentsResponseDto>;

// CancelSaleCommand.cs
public record CancelSaleCommand(
    int SaleId,
    string Reason,
    int UserId
) : IRequest<SaleResponseDto>;
```

### **8. Command Handlers (Application/Core/Sales/CommandHandlers/)**

```csharp
// CreateSaleCommandHandler.cs
public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleResponseDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICodeGeneratorService _codeGenerator;
    
    public async Task<SaleResponseDto> Handle(
        CreateSaleCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Generar código automático
        var code = await _codeGenerator.GenerateNextCodeAsync("VTA", "Sales");
        
        // 2. Calcular totales
        decimal subTotal = 0;
        decimal totalTax = 0;
        var details = new List<SaleDetail>();
        
        foreach (var detailDto in request.SaleData.Details)
        {
            var product = await _productRepository.GetByIdAsync(detailDto.ProductId);
            
            var quantity = detailDto.Quantity;
            var unitPrice = detailDto.UnitPrice;
            var discountPct = detailDto.DiscountPercentage;
            
            var lineSubTotal = quantity * unitPrice;
            var discountAmount = lineSubTotal * (discountPct / 100);
            var lineAfterDiscount = lineSubTotal - discountAmount;
            var taxAmount = lineAfterDiscount * (product.TaxRate);
            var lineTotal = lineAfterDiscount + taxAmount;
            
            var detail = new SaleDetail
            {
                ProductId = product.ID,
                ProductCode = product.code,
                ProductName = product.name,
                Quantity = quantity,
                UnitPrice = unitPrice,
                DiscountPercentage = discountPct,
                DiscountAmount = discountAmount,
                TaxPercentage = product.TaxRate,
                TaxAmount = taxAmount,
                SubTotal = lineAfterDiscount,
                Total = lineTotal,
                UnitCost = product.BaseCost,
                TotalCost = quantity * product.BaseCost
            };
            
            details.Add(detail);
            subTotal += lineAfterDiscount;
            totalTax += taxAmount;
        }
        
        // 3. Aplicar descuento global
        var globalDiscount = subTotal * (request.SaleData.DiscountPercentage / 100);
        var finalSubTotal = subTotal - globalDiscount;
        var finalTotal = finalSubTotal + totalTax;
        
        // 4. Crear venta
        var sale = new Sale
        {
            Code = code,
            SaleDate = DateTime.UtcNow,
            CustomerId = request.SaleData.CustomerId,
            CustomerName = customer?.Name,
            WarehouseId = request.SaleData.WarehouseId,
            UserId = request.UserId,
            PriceListId = request.SaleData.PriceListId,
            SubTotal = subTotal,
            DiscountAmount = globalDiscount,
            DiscountPercentage = request.SaleData.DiscountPercentage,
            TaxAmount = totalTax,
            Total = finalTotal,
            Status = "Draft",
            RequiresInvoice = request.SaleData.RequiresInvoice,
            Notes = request.SaleData.Notes,
            CreatedByUserId = request.UserId,
            Details = details
        };
        
        await _saleRepository.CreateAsync(sale);
        
        return MapToResponse(sale);
    }
}

// ProcessSalePaymentsCommandHandler.cs
public class ProcessSalePaymentsCommandHandler : IRequestHandler<ProcessSalePaymentsCommand, ProcessSalePaymentsResponseDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IInventoryService _inventoryService;
    private readonly ICodeGeneratorService _codeGenerator;
    
    public async Task<ProcessSalePaymentsResponseDto> Handle(
        ProcessSalePaymentsCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Obtener venta
        var sale = await _saleRepository.GetByIdAsync(request.SaleId);
        
        // 2. Validar estado
        if (sale.Status != "Draft")
            throw new InvalidOperationException("Solo se pueden procesar pagos de ventas en estado Draft");
        
        // 3. Validar stock
        foreach (var detail in sale.Details)
        {
            var stock = await _inventoryService.GetStockAsync(detail.ProductId, sale.WarehouseId);
            
            if (stock == null || stock.Quantity < detail.Quantity)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente para {detail.ProductName}. " +
                    $"Disponible: {stock?.Quantity ?? 0}, Solicitado: {detail.Quantity}"
                );
            }
        }
        
        // 4. Procesar pagos
        decimal totalPaid = 0;
        foreach (var paymentDto in request.PaymentsData.Payments)
        {
            var payment = new SalePayment
            {
                SaleId = sale.Id,
                PaymentMethod = paymentDto.PaymentMethod,
                Amount = paymentDto.Amount,
                CardNumber = paymentDto.CardNumber,
                CardType = paymentDto.CardType,
                AuthorizationCode = paymentDto.AuthorizationCode,
                TransactionReference = paymentDto.TransactionReference,
                TerminalId = paymentDto.TerminalId,
                BankName = paymentDto.BankName,
                TransferReference = paymentDto.TransferReference,
                CheckNumber = paymentDto.CheckNumber,
                CheckBank = paymentDto.CheckBank,
                Status = "Completed"
            };
            
            sale.Payments.Add(payment);
            totalPaid += payment.Amount;
        }
        
        // 5. Validar monto
        if (totalPaid < sale.Total)
        {
            throw new InvalidOperationException(
                $"El total pagado ({totalPaid:C2}) es menor al total de la venta ({sale.Total:C2})"
            );
        }
        
        // 6. Actualizar venta
        sale.AmountPaid = totalPaid;
        sale.ChangeAmount = totalPaid - sale.Total;
        sale.IsPaid = true;
        sale.Status = "Completed";
        
        // 7. Descontar inventario
        var movements = new List<InventoryMovementSummaryDto>();
        
        foreach (var detail in sale.Details)
        {
            var movementCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");
            var currentStock = await _inventoryService.GetStockAsync(detail.ProductId, sale.WarehouseId);
            
            decimal stockBefore = currentStock.Quantity;
            decimal stockAfter = stockBefore - detail.Quantity;
            
            var movement = new InventoryMovement
            {
                Code = movementCode,
                ProductId = detail.ProductId,
                WarehouseId = sale.WarehouseId,
                MovementType = "OUT",
                MovementSubType = "SALE",
                Quantity = -detail.Quantity,
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                UnitCost = detail.UnitCost,
                TotalCost = detail.TotalCost,
                ReferenceDocumentType = "Sale",
                ReferenceDocumentId = sale.Id,
                ReferenceDocumentCode = sale.Code,
                SaleId = sale.Id,
                MovementDate = DateTime.UtcNow,
                Notes = $"Venta - Cliente: {sale.CustomerName}",
                CreatedByUserId = request.UserId
            };
            
            await _inventoryService.CreateMovementAsync(movement);
            
            currentStock.Quantity -= detail.Quantity;
            currentStock.LastMovementDate = DateTime.UtcNow;
            await _inventoryService.UpdateStockAsync(currentStock);
            
            movements.Add(new InventoryMovementSummaryDto
            {
                MovementCode = movementCode,
                ProductCode = detail.ProductCode,
                ProductName = detail.ProductName,
                Quantity = -detail.Quantity,
                StockBefore = stockBefore,
                StockAfter = stockAfter
            });
        }
        
        // 8. Marcar como aplicado a inventario
        sale.IsPostedToInventory = true;
        sale.PostedToInventoryDate = DateTime.UtcNow;
        
        await _saleRepository.UpdateAsync(sale);
        
        return new ProcessSalePaymentsResponseDto
        {
            Message = "Venta completada exitosamente",
            Error = 0,
            Data = new ProcessSalePaymentsDataDto
            {
                SaleId = sale.Id,
                SaleCode = sale.Code,
                Status = sale.Status,
                AmountPaid = sale.AmountPaid,
                ChangeAmount = sale.ChangeAmount,
                TotalMovements = movements.Count,
                Payments = sale.Payments.Select(p => new SalePaymentResponseDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    AuthorizationCode = p.AuthorizationCode
                }).ToList(),
                InventoryMovements = movements
            }
        };
    }
}
```

### **9. Controller (Web.Api/Controllers/Sales/)**

```csharp
[Route("api/sales")]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    [RequirePermission("Sales", "Create")]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequestDto request)
    {
        var userId = HttpContext.Items["UserId"] as int? ?? 0;
        var command = new CreateSaleCommand(request, userId);
        var result = await _mediator.Send(command);
        
        return Ok(new
        {
            message = "Venta creada exitosamente",
            error = 0,
            data = result
        });
    }
    
    [HttpPost("{saleId}/payments")]
    [RequirePermission("Sales", "Complete")]
    public async Task<IActionResult> ProcessPayments(
        int saleId, 
        [FromBody] ProcessSalePaymentsRequestDto request)
    {
        var userId = HttpContext.Items["UserId"] as int? ?? 0;
        var command = new ProcessSalePaymentsCommand(saleId, request, userId);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    [RequirePermission("Sales", "View")]
    public async Task<IActionResult> GetSale(int id)
    {
        var query = new GetSaleByIdQuery(id);
        var result = await _mediator.Send(query);
        
        return Ok(new { message = "Venta obtenida", error = 0, data = result });
    }
    
    [HttpGet]
    [RequirePermission("Sales", "View")]
    public async Task<IActionResult> GetSales(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? warehouseId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null)
    {
        var query = new GetSalesPagedQuery(page, pageSize, warehouseId, fromDate, toDate, status);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
    
    [HttpPut("{id}/cancel")]
    [RequirePermission("Sales", "Cancel")]
    public async Task<IActionResult> CancelSale(
        int id, 
        [FromBody] CancelSaleRequestDto request)
    {
        var userId = HttpContext.Items["UserId"] as int? ?? 0;
        var command = new CancelSaleCommand(id, request.Reason, userId);
        var result = await _mediator.Send(command);
        
        return Ok(new { message = "Venta cancelada", error = 0, data = result });
    }
}
```

---

## ?? Pasos para Completar

1. ? **Ejecutar migración:**
   ```bash
   dotnet ef migrations add CreateSalesAndPaymentsTables
   dotnet ef database update
   ```

2. ? **Crear DTOs** en `Application/DTOs/Sales/SaleDtos.cs`

3. ? **Crear Repositorios:**
   - `Application/Abstractions/Sales/ISaleRepository.cs`
   - `Infrastructure/Repositories/SaleRepository.cs`

4. ? **Crear Commands/Queries:**
   - `Application/Core/Sales/Commands/`
   - `Application/Core/Sales/Queries/`

5. ? **Crear Handlers:**
   - `Application/Core/Sales/CommandHandlers/`
   - `Application/Core/Sales/QueryHandlers/`

6. ? **Crear Controller:**
   - `Web.Api/Controllers/Sales/SalesController.cs`

7. ? **Registrar servicios** en `Program.cs`

8. ? **Testing** en Postman

9. ? **Documentación** completa

---

## ?? Endpoints Finales

```
POST   /api/sales                      - Crear venta (Draft)
POST   /api/sales/{id}/payments        - Procesar pagos y completar
GET    /api/sales                      - Listar ventas
GET    /api/sales/{id}                 - Obtener venta por ID
PUT    /api/sales/{id}/cancel          - Cancelar venta
GET    /api/sales/{id}/payments        - Obtener pagos de una venta
```

---

**¿Deseas que continúe con la implementación completa de DTOs, Repositorios y Handlers?** ??
