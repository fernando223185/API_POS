# ? **Sistema de Ventas con Cobranza - COMPLETADO**

## ?? **ESTADO: IMPLEMENTADO Y FUNCIONANDO**

---

## ?? **Resumen Ejecutivo**

Se ha implementado exitosamente un **sistema completo de ventas con cobranza multi-forma de pago** que incluye:

1. ? **Folio automático** (VTA-000001, VTA-000002...)
2. ? **Múltiples formas de pago** por venta
3. ? **Control automático de inventario** (descuenta stock al completar venta)
4. ? **Kardex completo** (movimientos en `InventoryMovements`)
5. ? **Arquitectura CQRS** con MediatR
6. ? **Validaciones robustas** (stock, pagos, estados)

---

## ??? **Base de Datos Actualizada**

### **Tablas Creadas:**

#### **1. Sales (Ventas)**
```sql
- Id (PK, IDENTITY)
- Code (UNIQUE) - Código único de venta
- SaleDate - Fecha de venta
- CustomerId (FK nullable) - Cliente
- CustomerName - Nombre del cliente (denormalizado)
- WarehouseId (FK) - Almacén
- UserId (FK) - Usuario vendedor
- PriceListId (FK nullable) - Lista de precios
- SubTotal, DiscountAmount, DiscountPercentage
- TaxAmount, Total
- AmountPaid, ChangeAmount, IsPaid
- Status - Draft/Completed/Cancelled
- IsPostedToInventory, PostedToInventoryDate
- RequiresInvoice, InvoiceId, InvoiceUuid
- Notes, CreatedAt, UpdatedAt, CancelledAt
```

#### **2. SaleDetails (Detalles de Venta)**
```sql
- Id (PK, IDENTITY)
- SaleId (FK) - Venta relacionada
- ProductId (FK) - Producto
- ProductCode, ProductName (denormalizados)
- Quantity, UnitPrice
- DiscountPercentage, DiscountAmount
- TaxPercentage, TaxAmount
- SubTotal, Total
- UnitCost, TotalCost (para cálculo de utilidad)
- SerialNumber, LotNumber, Notes
```

#### **3. SalePayments (Pagos de Venta)**
```sql
- Id (PK, IDENTITY)
- SaleId (FK) - Venta relacionada
- PaymentMethod - Cash/CreditCard/DebitCard/Transfer/Check
- Amount, PaymentDate
- CardNumber (4 dígitos), CardType, AuthorizationCode
- TransactionReference, TerminalId, BankName
- TransferReference, CheckNumber, CheckBank
- Status, Notes, CreatedAt
```

#### **4. InventoryMovements (Actualizado)**
```sql
+ SaleId (FK nullable) - Nuevo campo agregado
```

### **Script SQL Ejecutado:**
?? `Infrastructure/Scripts/CreateSalesPaymentsTables.sql`

### **Migración Registrada:**
?? `20260311200504_AddSalesPaymentsSystem`

---

## ?? **Archivos Implementados**

### **Entidades del Dominio**
- ? `Domain/Entities/Sale.cs`
- ? `Domain/Entities/SaleDetail.cs`
- ? `Domain/Entities/SalePayment.cs`
- ? `Domain/Entities/InventoryMovement.cs` (actualizado con `SaleId`)

### **DTOs**
- ? `Application/DTOs/Sales/SaleDtos.cs`
  - CreateSaleRequestDto, CreateSaleDetailDto
  - ProcessSalePaymentsRequestDto, CreateSalePaymentDto
  - SaleResponseDto, SaleDetailResponseDto, SalePaymentResponseDto
  - SalesPagedResponseDto, SalesStatisticsDto

### **Repositorios**
- ? `Application/Abstractions/Sales/ISaleRepository.cs`
- ? `Infrastructure/Repositories/SaleRepository.cs`

### **Commands & Handlers**
- ? `Application/Core/Sales/Commands/SaleCommands.cs`
- ? `Application/Core/Sales/CommandHandlers/CreateSaleCommandHandler.cs`
- ? `Application/Core/Sales/CommandHandlers/ProcessSalePaymentsCommandHandler.cs` ?
- ? `Application/Core/Sales/CommandHandlers/CancelSaleCommandHandler.cs`

### **Queries & Handlers**
- ? `Application/Core/Sales/Queries/SaleQueries.cs`
- ? `Application/Core/Sales/QueryHandlers/SaleQueryHandlers.cs`

### **Controller**
- ? `Web.Api/Controllers/Sales/SalesController.cs`

### **Configuración**
- ? `Infrastructure/Persistence/POSDbContext.cs` (configuración EF Core)
- ? `Web.Api/Program.cs` (registro de servicios)

---

## ?? **Endpoints Disponibles**

```http
POST   /api/sales                      - Crear venta (Draft)
POST   /api/sales/{id}/payments        - Procesar pagos y completar venta ?
GET    /api/sales                      - Listar ventas (paginado + filtros)
GET    /api/sales/{id}                 - Obtener venta por ID
GET    /api/sales/statistics           - Estadísticas de ventas
PUT    /api/sales/{id}/cancel          - Cancelar venta
GET    /api/sales/{id}/payments        - Obtener pagos de una venta
```

---

## ?? **Flujo Completo de una Venta**

### **Paso 1: Crear Venta (Draft)**
```http
POST /api/sales
Authorization: Bearer {token}
Content-Type: application/json

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

**? Resultado:**
- Se crea venta con código `VTA-000001`
- Estado: `Draft`
- Total calculado automáticamente
- NO descuenta inventario aún

### **Paso 2: Procesar Pagos y Completar Venta**
```http
POST /api/sales/1/payments
Authorization: Bearer {token}
Content-Type: application/json

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
      "terminalId": "TERM001"
    }
  ]
}
```

**? Resultado:**
- Se registran los pagos en `SalePayments`
- Se valida que total pagado >= total venta
- Se calcula el cambio
- **Se descuenta inventario automáticamente** ?
- Se crean movimientos en `InventoryMovements` tipo `OUT/SALE`
- Se actualiza `ProductStock`
- Estado cambia a: `Completed`
- `IsPostedToInventory = true`

---

## ?? **Características Clave**

### **1. Folio Automático Secuencial**
```csharp
var code = await _codeGenerator.GenerateNextCodeAsync("VTA", "Sales");
// Genera: VTA-000001, VTA-000002, VTA-000003...
```

### **2. Múltiples Formas de Pago**
- ? Efectivo (Cash)
- ? Tarjeta de Crédito (CreditCard)
- ? Tarjeta de Débito (DebitCard)
- ? Transferencia Bancaria (Transfer)
- ? Cheque (Check)
- ? N formas de pago por venta

### **3. Integración con Terminales Bancarias**
Guarda información completa de transacciones:
- Últimos 4 dígitos de tarjeta
- Tipo de tarjeta (Visa, Mastercard, etc.)
- Código de autorización
- Referencia de transacción
- ID de terminal
- Banco

### **4. Descuento Automático de Inventario**
Al completar la venta:
1. Valida stock disponible
2. Genera código de movimiento automático
3. Crea registro en `InventoryMovements` (tipo `OUT/SALE`)
4. Actualiza `ProductStock` (descuenta cantidad)
5. Registra `StockBefore` y `StockAfter`
6. Vincula movimiento con venta (`SaleId`)

### **5. Cálculo Automático de Totales**
- Descuentos por línea
- Descuento global sobre la venta
- Impuestos (IVA)
- Redistribución proporcional de impuestos
- Cambio a devolver

### **6. Validaciones Robustas**
- ? Stock suficiente antes de vender
- ? Total pagado >= Total venta
- ? Almacén existe y está activo
- ? Productos existen y están activos
- ? Usuario autenticado
- ? Estado correcto para cada operación

### **7. Denormalización para Auditoría**
Se guardan copias de:
- Nombre del cliente
- Código y nombre del producto
- Costos unitarios y totales
- Para mantener historial aunque se modifiquen datos maestros

---

## ?? **Para AWS/Producción**

### **Aplicar en AWS:**

```bash
# 1. Conectarse al servidor AWS
ssh -i tu-key.pem ec2-user@tu-servidor-aws

# 2. Subir el script SQL
scp -i tu-key.pem Infrastructure/Scripts/CreateSalesPaymentsTables.sql ec2-user@servidor:/home/ec2-user/

# 3. Ejecutar el script
sqlcmd -S localhost -d ERP -U sa -P "TuPassword" \
  -i CreateSalesPaymentsTables.sql

# 4. Actualizar código de la API
cd /var/www/erpapi
git pull origin main
dotnet publish -c Release -o /var/www/erpapi/publish

# 5. Reiniciar servicio
sudo systemctl restart erpapi
```

### **Script SQL Portable:**
El archivo `Infrastructure/Scripts/CreateSalesPaymentsTables.sql` es completamente portable y puede ejecutarse en cualquier servidor SQL Server sin modificaciones.

---

## ? **Verificación**

### **Verificar tablas creadas:**
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Sales', 'SaleDetails', 'SalePayments')
ORDER BY TABLE_NAME;
```

### **Verificar migración registrada:**
```sql
SELECT * FROM [__EFMigrationsHistory]
WHERE MigrationId = '20260311200504_AddSalesPaymentsSystem';
```

### **Probar el sistema:**
1. Crear venta: `POST /api/sales`
2. Procesar pagos: `POST /api/sales/1/payments`
3. Ver kardex: Verificar `InventoryMovements` con `SaleId` no nulo
4. Ver stock: Verificar `ProductStock` actualizado

---

## ?? **Documentación Adicional**

- `DOCS/Sales_System_Complete_Design.md` - Diseño completo del sistema
- `DOCS/Sales_Implementation_Status.md` - Estado de implementación
- `Infrastructure/Scripts/CreateSalesPaymentsTables.sql` - Script SQL ejecutado

---

## ?? **Beneficios del Sistema**

1. ? Control preciso de inventario en tiempo real
2. ? Kardex completo con auditoría
3. ? Múltiples formas de pago por venta
4. ? Integración con terminales bancarias
5. ? Cálculo automático de totales, impuestos y descuentos
6. ? Arquitectura escalable y mantenible (CQRS)
7. ? Preparado para facturación electrónica (CFDI)
8. ? Historial completo denormalizado para auditoría
9. ? Portable a AWS sin modificaciones
10. ? Compatible con Entity Framework migrations

---

**?? SISTEMA COMPLETAMENTE FUNCIONAL Y LISTO PARA PRODUCCIÓN** ??
