# ?? Sistema Completo de Ventas con Cobranza Multi-Forma de Pago

## ?? Objetivo

Implementar un sistema robusto de ventas que incluya:

1. **Folio personalizado** automático (VTA-001, VTA-002, etc.)
2. **Múltiples formas de pago** (Efectivo, Tarjeta, Transferencia, etc.)
3. **Integración con terminales bancarias** (guardar referencias, autorizaciones, etc.)
4. **Descuento automático de inventario** al completar la venta
5. **Kardex completo** de todos los movimientos
6. **Validaciones** de stock antes de vender

---

## ?? Estructura de Tablas

### **1. Sale (Venta)**

```sql
CREATE TABLE [Sales] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Code] NVARCHAR(20) NOT NULL UNIQUE,              -- VTA-001, VTA-002
    [SaleDate] DATETIME2 NOT NULL,
    [CustomerId] INT NULL,
    [CustomerName] NVARCHAR(200) NULL,
    [WarehouseId] INT NOT NULL,
    [UserId] INT NOT NULL,                             -- Vendedor/Cajero
    [PriceListId] INT NULL,
    
    -- Montos
    [SubTotal] DECIMAL(18,2) NOT NULL,
    [DiscountAmount] DECIMAL(18,2) DEFAULT 0,
    [DiscountPercentage] DECIMAL(6,4) DEFAULT 0,
    [TaxAmount] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    
    -- Control de pago
    [AmountPaid] DECIMAL(18,2) NOT NULL,
    [ChangeAmount] DECIMAL(18,2) DEFAULT 0,
    [IsPaid] BIT NOT NULL DEFAULT 0,
    
    -- Estado
    [Status] NVARCHAR(50) NOT NULL,                   -- Draft, Completed, Cancelled, Invoiced
    [IsPostedToInventory] BIT NOT NULL DEFAULT 0,
    [PostedToInventoryDate] DATETIME2 NULL,
    
    -- Facturación
    [RequiresInvoice] BIT DEFAULT 0,
    [InvoiceId] INT NULL,
    [InvoiceUuid] NVARCHAR(50) NULL,
    
    -- Metadatos
    [Notes] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedByUserId] INT NULL,
    [UpdatedAt] DATETIME2 NULL,
    [CancelledAt] DATETIME2 NULL,
    [CancelledByUserId] INT NULL,
    [CancellationReason] NVARCHAR(500) NULL,
    
    CONSTRAINT [FK_Sales_Customer] FOREIGN KEY ([CustomerId]) 
        REFERENCES [Customer]([ID]) ON DELETE SET NULL,
    CONSTRAINT [FK_Sales_Warehouse] FOREIGN KEY ([WarehouseId]) 
        REFERENCES [Warehouses]([Id]) ON DELETE RESTRICT,
    CONSTRAINT [FK_Sales_User] FOREIGN KEY ([UserId]) 
        REFERENCES [Users]([Id]) ON DELETE RESTRICT,
    CONSTRAINT [FK_Sales_CreatedBy] FOREIGN KEY ([CreatedByUserId]) 
        REFERENCES [Users]([Id]) ON DELETE RESTRICT
);

CREATE INDEX IX_Sales_Code ON [Sales]([Code]);
CREATE INDEX IX_Sales_SaleDate ON [Sales]([SaleDate]);
CREATE INDEX IX_Sales_CustomerId ON [Sales]([CustomerId]);
CREATE INDEX IX_Sales_Status ON [Sales]([Status]);
CREATE INDEX IX_Sales_WarehouseId ON [Sales]([WarehouseId]);
```

---

### **2. SaleDetail (Detalle de Venta)**

```sql
CREATE TABLE [SaleDetails] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [SaleId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [ProductCode] NVARCHAR(50) NOT NULL,              -- Denormalizado para auditoría
    [ProductName] NVARCHAR(200) NOT NULL,             -- Denormalizado para auditoría
    
    -- Cantidades
    [Quantity] DECIMAL(18,4) NOT NULL,
    [UnitPrice] DECIMAL(18,4) NOT NULL,
    [DiscountPercentage] DECIMAL(6,4) DEFAULT 0,
    [DiscountAmount] DECIMAL(18,2) DEFAULT 0,
    [TaxPercentage] DECIMAL(6,4) DEFAULT 0.16,
    [TaxAmount] DECIMAL(18,2) NOT NULL,
    
    -- Montos
    [SubTotal] DECIMAL(18,2) NOT NULL,
    [Total] DECIMAL(18,2) NOT NULL,
    
    -- Costos (para cálculo de utilidad)
    [UnitCost] DECIMAL(18,4) NULL,
    [TotalCost] DECIMAL(18,2) NULL,
    
    -- Información adicional
    [Notes] NVARCHAR(500) NULL,
    [SerialNumber] NVARCHAR(100) NULL,               -- Si el producto tiene serie
    [LotNumber] NVARCHAR(100) NULL,                  -- Si el producto tiene lote
    
    CONSTRAINT [FK_SaleDetails_Sale] FOREIGN KEY ([SaleId]) 
        REFERENCES [Sales]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SaleDetails_Product] FOREIGN KEY ([ProductId]) 
        REFERENCES [Products]([ID]) ON DELETE RESTRICT
);

CREATE INDEX IX_SaleDetails_SaleId ON [SaleDetails]([SaleId]);
CREATE INDEX IX_SaleDetails_ProductId ON [SaleDetails]([ProductId]);
```

---

### **3. SalePayment (Formas de Pago)**

**? CLAVE: Permite N formas de pago por venta**

```sql
CREATE TABLE [SalePayments] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [SaleId] INT NOT NULL,
    [PaymentMethod] NVARCHAR(50) NOT NULL,            -- Cash, CreditCard, DebitCard, Transfer, Check
    [Amount] DECIMAL(18,2) NOT NULL,
    [PaymentDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Información de tarjeta/terminal
    [CardNumber] NVARCHAR(4) NULL,                    -- Últimos 4 dígitos
    [CardType] NVARCHAR(20) NULL,                     -- Visa, Mastercard, Amex
    [AuthorizationCode] NVARCHAR(50) NULL,
    [TransactionReference] NVARCHAR(100) NULL,        -- Referencia de terminal
    [TerminalId] NVARCHAR(50) NULL,
    [BankName] NVARCHAR(100) NULL,
    
    -- Transferencias/Cheques
    [TransferReference] NVARCHAR(100) NULL,
    [CheckNumber] NVARCHAR(50) NULL,
    [CheckBank] NVARCHAR(100) NULL,
    
    -- Control
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Completed', -- Pending, Completed, Cancelled
    [Notes] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_SalePayments_Sale] FOREIGN KEY ([SaleId]) 
        REFERENCES [Sales]([Id]) ON DELETE CASCADE
);

CREATE INDEX IX_SalePayments_SaleId ON [SalePayments]([SaleId]);
CREATE INDEX IX_SalePayments_PaymentMethod ON [SalePayments]([PaymentMethod]);
CREATE INDEX IX_SalePayments_PaymentDate ON [SalePayments]([PaymentDate]);
```

---

## ?? Flujo de Venta

### **Paso 1: Crear Venta (Draft)**

```csharp
POST /api/sales

{
  "customerId": 5,
  "warehouseId": 1,
  "priceListId": 1,
  "discountPercentage": 5.0,
  "requiresInvoice": true,
  "details": [
    {
      "productId": 10,
      "quantity": 2,
      "unitPrice": 1500.00,
      "discountPercentage": 0
    },
    {
      "productId": 15,
      "quantity": 1,
      "unitPrice": 500.00,
      "discountPercentage": 10
    }
  ]
}
```

**Respuesta:**
```json
{
  "message": "Venta creada exitosamente",
  "error": 0,
  "data": {
    "id": 123,
    "code": "VTA-000123",
    "saleDate": "2026-03-10T15:30:00",
    "status": "Draft",
    "subTotal": 3350.00,
    "discountAmount": 167.50,
    "taxAmount": 508.80,
    "total": 3691.30,
    "amountPaid": 0.00,
    "isPaid": false
  }
}
```

---

### **Paso 2: Procesar Pagos (Múltiples Formas)**

```csharp
POST /api/sales/123/payments

{
  "payments": [
    {
      "paymentMethod": "Cash",
      "amount": 2000.00
    },
    {
      "paymentMethod": "CreditCard",
      "amount": 1691.30,
      "cardNumber": "4152",
      "cardType": "Visa",
      "authorizationCode": "AUTH123456",
      "transactionReference": "TXN789012",
      "terminalId": "TERM001",
      "bankName": "BBVA"
    }
  ]
}
```

**Validación:**
- `Total de Pagos >= Total de Venta`
- Stock disponible para todos los productos
- Almacén activo

**Respuesta:**
```json
{
  "message": "Venta completada exitosamente",
  "error": 0,
  "data": {
    "id": 123,
    "code": "VTA-000123",
    "status": "Completed",
    "isPostedToInventory": true,
    "isPaid": true,
    "amountPaid": 3691.30,
    "changeAmount": 0.00,
    "payments": [
      {
        "id": 1,
        "paymentMethod": "Cash",
        "amount": 2000.00
      },
      {
        "id": 2,
        "paymentMethod": "CreditCard",
        "amount": 1691.30,
        "authorizationCode": "AUTH123456"
      }
    ],
    "inventoryMovements": [
      {
        "movementCode": "MOV-001234",
        "productCode": "PROD-010",
        "productName": "Laptop",
        "quantity": -2.00,
        "stockBefore": 50.00,
        "stockAfter": 48.00
      },
      {
        "movementCode": "MOV-001235",
        "productCode": "PROD-015",
        "productName": "Mouse",
        "quantity": -1.00,
        "stockBefore": 100.00,
        "stockAfter": 99.00
      }
    ]
  }
}
```

---

## ?? Acciones Automáticas al Completar Venta

### **1. Generar Folio Automático**

```csharp
var code = await _codeGenerator.GenerateNextCodeAsync("VTA", "Sales");
// Resultado: VTA-000001, VTA-000002, etc.
```

---

### **2. Validar Stock**

```csharp
foreach (var detail in sale.Details)
{
    var stock = await _inventoryService.GetStockAsync(
        detail.ProductId, 
        sale.WarehouseId
    );
    
    if (stock == null || stock.Quantity < detail.Quantity)
    {
        throw new InvalidOperationException(
            $"Stock insuficiente para {detail.ProductName}. " +
            $"Disponible: {stock?.Quantity ?? 0}, Solicitado: {detail.Quantity}"
        );
    }
}
```

---

### **3. Descontar Inventario**

```csharp
foreach (var detail in sale.Details)
{
    // Generar código de movimiento
    var movementCode = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");
    
    // Obtener stock actual
    var currentStock = await _inventoryService.GetStockAsync(detail.ProductId, sale.WarehouseId);
    
    decimal stockBefore = currentStock.Quantity;
    decimal stockAfter = stockBefore - detail.Quantity;
    
    // Crear movimiento de salida
    var movement = new InventoryMovement
    {
        Code = movementCode,
        ProductId = detail.ProductId,
        WarehouseId = sale.WarehouseId,
        MovementType = "OUT",
        MovementSubType = "SALE",
        Quantity = -detail.Quantity,  // ? Negativo para salida
        StockBefore = stockBefore,
        StockAfter = stockAfter,
        UnitCost = detail.UnitCost,
        TotalCost = detail.TotalCost,
        ReferenceDocumentType = "Sale",
        ReferenceDocumentId = sale.Id,
        ReferenceDocumentCode = sale.Code,
        MovementDate = DateTime.UtcNow,
        Notes = $"Venta - Cliente: {sale.CustomerName}",
        CreatedByUserId = sale.CreatedByUserId,
        CreatedAt = DateTime.UtcNow
    };
    
    await _inventoryService.CreateMovementAsync(movement);
    
    // Actualizar stock
    currentStock.Quantity -= detail.Quantity;
    currentStock.LastMovementDate = DateTime.UtcNow;
    await _inventoryService.UpdateStockAsync(currentStock);
}
```

---

### **4. Registrar Pagos**

```csharp
decimal totalPaid = 0;

foreach (var paymentDto in request.Payments)
{
    var payment = new SalePayment
    {
        SaleId = sale.Id,
        PaymentMethod = paymentDto.PaymentMethod,
        Amount = paymentDto.Amount,
        PaymentDate = DateTime.UtcNow,
        
        // Datos de tarjeta/terminal
        CardNumber = paymentDto.CardNumber,
        CardType = paymentDto.CardType,
        AuthorizationCode = paymentDto.AuthorizationCode,
        TransactionReference = paymentDto.TransactionReference,
        TerminalId = paymentDto.TerminalId,
        BankName = paymentDto.BankName,
        
        // Transferencias/Cheques
        TransferReference = paymentDto.TransferReference,
        CheckNumber = paymentDto.CheckNumber,
        CheckBank = paymentDto.CheckBank,
        
        Status = "Completed",
        CreatedAt = DateTime.UtcNow
    };
    
    await _salePaymentRepository.CreateAsync(payment);
    totalPaid += payment.Amount;
}

// Validar que se cubra el total
if (totalPaid < sale.Total)
{
    throw new InvalidOperationException(
        $"El total pagado ({totalPaid:C2}) es menor al total de la venta ({sale.Total:C2})"
    );
}

// Calcular cambio
sale.AmountPaid = totalPaid;
sale.ChangeAmount = totalPaid - sale.Total;
sale.IsPaid = true;
```

---

## ?? Ejemplos de Uso

### **Ejemplo 1: Venta con Efectivo**

```json
POST /api/sales/123/payments

{
  "payments": [
    {
      "paymentMethod": "Cash",
      "amount": 4000.00
    }
  ]
}
```

**Resultado:**
- Total: $3,691.30
- Pagado: $4,000.00
- Cambio: $308.70

---

### **Ejemplo 2: Venta con 3 Formas de Pago**

```json
{
  "payments": [
    {
      "paymentMethod": "Cash",
      "amount": 1000.00
    },
    {
      "paymentMethod": "CreditCard",
      "amount": 1500.00,
      "cardNumber": "4152",
      "authorizationCode": "AUTH123"
    },
    {
      "paymentMethod": "Transfer",
      "amount": 1191.30,
      "transferReference": "TRF789456"
    }
  ]
}
```

**Resultado:**
- Total de Pagos: $3,691.30 ?
- Cambio: $0.00

---

### **Ejemplo 3: Terminal Bancaria (Datos Completos)**

```json
{
  "paymentMethod": "CreditCard",
  "amount": 3691.30,
  "cardNumber": "4152",                    // Últimos 4 dígitos
  "cardType": "Visa",
  "authorizationCode": "AUTH987654",
  "transactionReference": "TXN123456789",
  "terminalId": "TERM001",
  "bankName": "BBVA",
  "notes": "Transacción aprobada"
}
```

---

## ?? Kardex Completo

Cada venta genera:

1. **Movimiento de Inventario** por cada producto
   - Tipo: `OUT`
   - Subtipo: `SALE`
   - Referencia: Código de venta (VTA-000123)
   
2. **Actualización de Stock** en tiempo real

3. **Registro de Pagos** con toda la información bancaria

---

## ?? Validaciones Críticas

### **Antes de Completar la Venta:**

? **Stock suficiente** en el almacén  
? **Total pagado >= Total venta**  
? **Almacén activo**  
? **Productos activos**  
? **Usuario con permisos**  
? **Validar métodos de pago**  

---

## ?? Beneficios del Sistema

1. **Foliado automático** secuencial
2. **Múltiples formas de pago** en una sola venta
3. **Integración completa con terminales** bancarias
4. **Kardex preciso** con stock actualizado en tiempo real
5. **Auditoría completa** de todas las transacciones
6. **Referencias bancarias** guardadas para conciliación
7. **Cambio calculado** automáticamente
8. **Cancelaciones** con razón y auditoría

---

## ?? Próximos Pasos

1. ? Crear migración con las tablas
2. ? Crear entidades en `Domain/Entities`
3. ? Implementar DTOs
4. ? Crear repositorios
5. ? Implementar Commands/Queries con CQRS
6. ? Crear controller `/api/sales`
7. ? Integrar con InventoryService
8. ? Documentar endpoints en Postman

**¿Procedo con la implementación completa?** ??
