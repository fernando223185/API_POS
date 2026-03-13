# ? Sistema de Ventas con Cobranza - Implementación Completa

## ?? **ESTADO: IMPLEMENTADO**

Se ha implementado un sistema completo de ventas con las siguientes características:

---

## ?? **Archivos Creados/Modificados**

### **1. Entidades del Dominio**
- ? `Domain/Entities/Sale.cs` - Venta principal
- ? `Domain/Entities/SaleDetail.cs` - Detalles de productos
- ? `Domain/Entities/SalePayment.cs` - Pagos múltiples formas
- ? `Domain/Entities/InventoryMovement.cs` - Actualizado con SaleId

### **2. Migración de Base de Datos**
- ? `Infrastructure/Migrations/20260310000000_CreateSalesAndPaymentsTables.cs`
  - Tabla `Sales` con foliado automático
  - Tabla `SaleDetails` para productos vendidos
  - Tabla `SalePayments` para múltiples formas de pago
  - Campo `SaleId` en `InventoryMovements`

### **3. DTOs**
- ? `Application/DTOs/Sales/SaleDtos.cs` (completo)
  - `CreateSaleRequestDto`
  - `CreateSaleDetailDto`
  - `ProcessSalePaymentsRequestDto`
  - `CreateSalePaymentDto`
  - `SaleResponseDto`
  - `SaleDetailResponseDto`
  - `SalePaymentResponseDto`
  - `ProcessSalePaymentsResponseDto`
  - `SalesPagedResponseDto`
  - `SalesStatisticsDto`

### **4. Repositorios**
- ? `Application/Abstractions/Sales/ISaleRepository.cs`
- ? `Infrastructure/Repositories/SaleRepository.cs`
  - CreateAsync
  - GetByIdAsync
  - GetByCodeAsync
  - UpdateAsync
  - GetPagedAsync
  - GetStatisticsAsync
  - ExistsByCodeAsync

### **5. Commands**
- ? `Application/Core/Sales/Commands/SaleCommands.cs`
  - `CreateSaleCommand` - Crear venta (Draft)
  - `ProcessSalePaymentsCommand` - Procesar pagos y completar
  - `CancelSaleCommand` - Cancelar venta

### **6. Command Handlers**
- ? `Application/Core/Sales/CommandHandlers/CreateSaleCommandHandler.cs`
  - Validación de productos y cliente
  - Generación de código automático (VTA-000001, etc.)
  - Cálculo de totales con descuentos e impuestos
  - Creación en estado Draft

- ? `Application/Core/Sales/CommandHandlers/ProcessSalePaymentsCommandHandler.cs` **? CRÍTICO**
  - Validación de stock disponible
  - Registro de múltiples formas de pago
  - **Descuento automático de inventario**
  - **Creación de movimientos en Kardex**
  - Cálculo de cambio
  - Actualización de stock en tiempo real

- ? `Application/Core/Sales/CommandHandlers/CancelSaleCommandHandler.cs`
  - Cancelación con razón
  - Validaciones de estado

### **7. Queries**
- ? `Application/Core/Sales/Queries/SaleQueries.cs`
  - `GetSaleByIdQuery`
  - `GetSalesPagedQuery`
  - `GetSalesStatisticsQuery`

### **8. Query Handlers**
- ? `Application/Core/Sales/QueryHandlers/SaleQueryHandlers.cs`
  - `GetSaleByIdQueryHandler`
  - `GetSalesPagedQueryHandler`
  - `GetSalesStatisticsQueryHandler`

### **9. Controller**
- ? `Web.Api/Controllers/Sales/SalesController.cs`
  - `POST /api/sales` - Crear venta
  - `POST /api/sales/{id}/payments` - Procesar pagos
  - `GET /api/sales` - Listar ventas
  - `GET /api/sales/{id}` - Obtener venta
  - `GET /api/sales/statistics` - Estadísticas
  - `PUT /api/sales/{id}/cancel` - Cancelar
  - `GET /api/sales/{id}/payments` - Obtener pagos

### **10. Configuración**
- ? `Infrastructure/Persistence/POSDbContext.cs` - Configuración EF Core
- ? `Web.Api/Program.cs` - Registro de `ISaleRepository`

---

## ?? **Funcionalidades Implementadas**

### **1. Folio Automático**
```csharp
var code = await _codeGenerator.GenerateNextCodeAsync("VTA", "Sales");
// Resultado: VTA-000001, VTA-000002, etc.
```

### **2. Múltiples Formas de Pago**
- ? Efectivo (Cash)
- ? Tarjeta de Crédito/Débito (CreditCard, DebitCard)
- ? Transferencia Bancaria (Transfer)
- ? Cheque (Check)
- ? N formas de pago por venta

### **3. Integración con Terminales Bancarias**
```csharp
{
  "paymentMethod": "CreditCard",
  "amount": 1500.00,
  "cardNumber": "4152",                    // Últimos 4 dígitos
  "cardType": "Visa",
  "authorizationCode": "AUTH987654",
  "transactionReference": "TXN123456789",
  "terminalId": "TERM001",
  "bankName": "BBVA"
}
```

### **4. Descuento Automático de Inventario**
Por cada producto vendido:
1. Valida stock disponible
2. Genera movimiento de tipo `OUT` subtipo `SALE`
3. Descuenta cantidad del `ProductStock`
4. Registra en `InventoryMovement` (Kardex)
5. Actualiza `LastMovementDate`

### **5. Cálculo Automático de Totales**
- Descuentos por línea
- Descuento global
- Impuestos (IVA)
- Redistribución proporcional de impuestos
- Cálculo de cambio

### **6. Validaciones Críticas**
- ? Stock suficiente antes de vender
- ? Total pagado >= Total venta
- ? Almacén activo
- ? Productos activos
- ? Usuario autenticado
- ? Estado correcto para cada operación

---

## ?? **Ejemplo de Flujo Completo**

### **Paso 1: Crear Venta**
```http
POST /api/sales

{
  "customerId": 5,
  "warehouseId": 1,
  "discountPercentage": 5,
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
    "status": "Draft",
    "total": 3691.30,
    "isPaid": false
  }
}
```

### **Paso 2: Procesar Pagos**
```http
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
      "authorizationCode": "AUTH123456"
    }
  ]
}
```

**Respuesta:**
```json
{
  "message": "Venta completada exitosamente",
  "error": 0,
  "data": {
    "saleId": 123,
    "saleCode": "VTA-000123",
    "status": "Completed",
    "amountPaid": 3691.30,
    "changeAmount": 0.00,
    "totalPayments": 2,
    "totalMovements": 2,
    "inventoryMovements": [
      {
        "movementCode": "MOV-001234",
        "productCode": "PROD-010",
        "quantity": -2.00,
        "stockBefore": 50.00,
        "stockAfter": 48.00
      },
      {
        "movementCode": "MOV-001235",
        "productCode": "PROD-015",
        "quantity": -1.00,
        "stockBefore": 100.00,
        "stockAfter": 99.00
      }
    ]
  }
}
```

---

## ?? **PENDIENTE PARA COMPLETAR**

### **Acción Requerida:**

1. **Eliminar código viejo conflictivo:**
   - `Application/Core/Sales_orders/` (carpeta completa - código antiguo)
   - `Application/Abstractions/Sales_Orders/ISalesRepository.cs`
   - `Infrastructure/Repositories/SalesRepository.cs` (el viejo, no `SaleRepository.cs`)

2. **Ejecutar migración:**
   ```bash
   dotnet ef migrations add CreateSalesAndPaymentsTables --project Infrastructure --startup-project Web.Api
   dotnet ef database update --project Infrastructure --startup-project Web.Api
   ```

3. **Compilar:**
   ```bash
   dotnet build
   ```

4. **Probar en Postman:**
   - POST `/api/sales` - Crear venta
   - POST `/api/sales/{id}/payments` - Completar venta

---

## ?? **Beneficios del Sistema**

1. ? **Foliado automático** secuencial
2. ? **Múltiples formas de pago** en una sola venta
3. ? **Integración con terminales bancarias** (guardar referencias)
4. ? **Kardex preciso** con movimientos detallados
5. ? **Stock en tiempo real** actualizado automáticamente
6. ? **Auditoría completa** de todas las transacciones
7. ? **Validaciones robustas** antes de procesar
8. ? **Cálculo automático** de totales, impuestos y cambio
9. ? **Denormalización** de datos para auditoría
10. ? **Arquitectura CQRS** escalable y mantenible

---

## ?? **Documentación Adicional**

- `DOCS/Sales_System_Complete_Design.md` - Diseño completo del sistema
- `DOCS/Sales_Implementation_Status.md` - Estado y código pendiente

---

## ? **Checklist de Implementación**

- [x] Diseño del sistema
- [x] Entidades del dominio (Sale, SaleDetail, SalePayment)
- [x] Migración de base de datos
- [x] DTOs completos
- [x] Repositorios (ISaleRepository, SaleRepository)
- [x] Commands (Create, ProcessPayments, Cancel)
- [x] Handlers (CreateSale, ProcessPayments, Cancel)
- [x] Queries (GetById, GetPaged, GetStatistics)
- [x] Query Handlers
- [x] Controller completo
- [x] Configuración EF Core
- [x] Registro de servicios
- [ ] Eliminar código viejo (pendiente)
- [ ] Ejecutar migración (pendiente)
- [ ] Testing en Postman (pendiente)

---

**?? Sistema listo para usar una vez se elimine el código viejo y se ejecute la migración!**
