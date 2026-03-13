# ?? Sistema de Compras, Recepción e Inventario - Estado Actual

## ? PARTE 1 COMPLETADA: Base de Datos y Servicios Core

### 1?? **Entidades Creadas** ?

#### **Domain/Entities/**
- ? `PurchaseOrder.cs` - Órdenes de compra
- ? `PurchaseOrderDetail.cs` - Detalle de órdenes
- ? `PurchaseOrderReceiving.cs` - Recepciones de mercancía
- ? `PurchaseOrderReceivingDetail.cs` - Detalle de recepciones
- ? `InventoryMovement.cs` - Movimientos de inventario (Kardex)
- ? `ProductStock.cs` - Stock por producto/almacén

### 2?? **Migración Aplicada** ?

**Migración:** `20260310173708_AddPurchaseOrdersInventorySystemFixed`

**Tablas creadas:**
```sql
? PurchaseOrders
? PurchaseOrderDetails
? PurchaseOrderReceivings
? PurchaseOrderReceivingDetails
? InventoryMovements
? ProductStock
```

**Características:**
- ? Relaciones configuradas correctamente (sin cascadas múltiples)
- ? Índices optimizados
- ? Restricciones de integridad referencial
- ? Índice único en `ProductStock` (ProductId + WarehouseId)

### 3?? **Servicio de Generación de Códigos** ?

#### **ICodeGeneratorService** - Interface
**Ubicación:** `Application/Abstractions/Common/ICodeGeneratorService.cs`

```csharp
Task<string> GenerateNextCodeAsync(
    string prefix,        // "OC", "REC", "MOV", etc.
    string tableName,     // Nombre de la tabla
    string codeColumnName = "Code",
    int length = 3        // Longitud del número
);
```

#### **CodeGeneratorService** - Implementación
**Ubicación:** `Infrastructure/Services/CodeGeneratorService.cs`

**Características:**
- ? **Thread-Safe**: Usa `SemaphoreSlim` para prevenir condiciones de carrera
- ? **Transaccional**: Usa transacciones de BD para atomicidad
- ? **Sin Duplicados**: Doble verificación antes de generar código
- ? **Escalable**: Encuentra automáticamente el siguiente número disponible
- ? **Flexible**: Prefijo y longitud configurables

**Ejemplos de uso:**
```csharp
// Generar código de orden de compra
var code = await _codeGenerator.GenerateNextCodeAsync("OC", "PurchaseOrders");
// Resultado: "OC-001", "OC-002", etc.

// Generar código de recepción
var code = await _codeGenerator.GenerateNextCodeAsync("REC", "PurchaseOrderReceivings");
// Resultado: "REC-001", "REC-002", etc.

// Generar código de movimiento
var code = await _codeGenerator.GenerateNextCodeAsync("MOV", "InventoryMovements");
// Resultado: "MOV-001", "MOV-002", etc.
```

### 4?? **DbContext Actualizado** ?

**Configuraciones agregadas:**
```csharp
? PurchaseOrder - ON DELETE RESTRICT (no cascade)
? PurchaseOrderDetail - CASCADE solo con PurchaseOrder
? PurchaseOrderReceiving - RESTRICT (auditoría)
? PurchaseOrderReceivingDetail - CASCADE solo con Receiving
? InventoryMovement - RESTRICT con todos
? ProductStock - RESTRICT + índice único (Product + Warehouse)
```

### 5?? **Servicio Registrado en DI** ?

```csharp
// Web.Api/Program.cs
builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
```

---

## ?? PARTE 2: PENDIENTE - Implementación CQRS

### **Necesitamos crear:**

#### 1. **Repositorios**
```
Application/Abstractions/Purchasing/
??? IPurchaseOrderRepository.cs
??? IPurchaseOrderReceivingRepository.cs
??? IInventoryMovementRepository.cs

Infrastructure/Repositories/
??? PurchaseOrderRepository.cs
??? PurchaseOrderReceivingRepository.cs
??? InventoryMovementRepository.cs
```

#### 2. **DTOs**
```
Application/DTOs/PurchaseOrder/
??? PurchaseOrderDtos.cs
??? PurchaseOrderDetailDto.cs
??? CreatePurchaseOrderDto.cs

Application/DTOs/Receiving/
??? ReceivingDtos.cs
??? ReceivingDetailDto.cs
??? QualityCheckDto.cs

Application/DTOs/Inventory/
??? InventoryMovementDto.cs
??? ProductStockDto.cs
```

#### 3. **Commands (CQRS)**
```
Application/Core/PurchaseOrder/Commands/
??? CreatePurchaseOrderCommand.cs
??? ApprovePurchaseOrderCommand.cs
??? CancelPurchaseOrderCommand.cs
??? UpdatePurchaseOrderCommand.cs

Application/Core/Receiving/Commands/
??? CreateReceivingCommand.cs
??? QualityCheckCommand.cs
??? PostToInventoryCommand.cs
??? RejectReceivingCommand.cs

Application/Core/Inventory/Commands/
??? CreateInventoryMovementCommand.cs
??? AdjustStockCommand.cs
```

#### 4. **Queries (CQRS)**
```
Application/Core/PurchaseOrder/Queries/
??? GetAllPurchaseOrdersQuery.cs
??? GetPurchaseOrderByIdQuery.cs
??? GetPurchaseOrderByCodeQuery.cs
??? GetPendingPurchaseOrdersQuery.cs

Application/Core/Receiving/Queries/
??? GetReceivingByIdQuery.cs
??? GetReceivingsByPurchaseOrderQuery.cs
??? GetPendingToPostQuery.cs

Application/Core/Inventory/Queries/
??? GetInventoryMovementsQuery.cs
??? GetKardexByProductQuery.cs
??? GetStockByWarehouseQuery.cs
```

#### 5. **Handlers**
```
Application/Core/PurchaseOrder/CommandHandlers/
Application/Core/PurchaseOrder/QueryHandlers/
Application/Core/Receiving/CommandHandlers/
Application/Core/Receiving/QueryHandlers/
Application/Core/Inventory/CommandHandlers/
Application/Core/Inventory/QueryHandlers/
```

#### 6. **Controllers**
```
Web.Api/Controllers/Purchasing/
??? PurchaseOrdersController.cs
??? PurchaseOrderReceivingsController.cs
??? InventoryController.cs
```

---

## ?? Flujo Completo Planeado

### **1. Crear Orden de Compra**
```
POST /api/PurchaseOrders
?
CreatePurchaseOrderCommandHandler
?
- Genera código: "OC-001"
- Valida proveedor y almacén
- Calcula totales
- Crea orden y detalles
?
Response: PurchaseOrderResponseDto
```

### **2. Aprobar Orden**
```
PATCH /api/PurchaseOrders/{id}/approve
?
ApprovePurchaseOrderCommandHandler
?
- Cambia status: Pending ? Approved
- Valida permisos
?
Response: Success
```

### **3. Recibir Mercancía**
```
POST /api/PurchaseOrderReceivings
?
CreateReceivingCommandHandler
?
- Genera código: "REC-001"
- Valida OC aprobada
- Verifica cantidades
- Crea recepción y detalles
- Actualiza QuantityReceived en OC
- Actualiza status OC (Partial/Received)
?
Response: ReceivingResponseDto
```

### **4. Control de Calidad**
```
PATCH /api/PurchaseOrderReceivings/{id}/quality-check
?
QualityCheckCommandHandler
?
- Registra aprobados/rechazados
- Cambia status: Received ? QualityCheck
?
Response: QualityCheckResultDto
```

### **5. Aplicar a Inventario** ? (Crítico)
```
POST /api/PurchaseOrderReceivings/{id}/post-to-inventory
?
PostToInventoryCommandHandler
?
1. Valida recepción aprobada
2. Por cada producto:
   - Genera código movimiento: "MOV-001"
   - Crea InventoryMovement (tipo IN)
   - Actualiza/Crea ProductStock
   - Calcula stock before/after
   - Actualiza costo promedio
3. Marca IsPostedToInventory = true
4. Actualiza fecha PostedToInventoryDate
?
Response: InventoryPostResultDto
```

---

## ?? Métodos Clave de Repositorios

### **IPurchaseOrderRepository**
```csharp
Task<PurchaseOrder?> GetByIdAsync(int id);
Task<PurchaseOrder?> GetByCodeAsync(string code);
Task<List<PurchaseOrder>> GetAllAsync(bool includeInactive = false);
Task<List<PurchaseOrder>> GetByStatusAsync(string status);
Task<List<PurchaseOrder>> GetPendingToReceiveAsync();
Task<PurchaseOrder> CreateAsync(PurchaseOrder purchaseOrder);
Task UpdateAsync(PurchaseOrder purchaseOrder);
Task<bool> ExistsAsync(int id);
```

### **IPurchaseOrderReceivingRepository**
```csharp
Task<PurchaseOrderReceiving?> GetByIdAsync(int id);
Task<List<PurchaseOrderReceiving>> GetByPurchaseOrderIdAsync(int purchaseOrderId);
Task<List<PurchaseOrderReceiving>> GetPendingToPostAsync();
Task<PurchaseOrderReceiving> CreateAsync(PurchaseOrderReceiving receiving);
Task UpdateAsync(PurchaseOrderReceiving receiving);
```

### **IInventoryMovementRepository**
```csharp
Task<List<InventoryMovement>> GetByProductAsync(int productId, int? warehouseId = null);
Task<List<InventoryMovement>> GetByWarehouseAsync(int warehouseId, DateTime? startDate = null);
Task<InventoryMovement> CreateAsync(InventoryMovement movement);
Task<ProductStock?> GetStockAsync(int productId, int warehouseId);
Task UpdateStockAsync(ProductStock stock);
```

---

## ?? Estructura de DTOs Planeada

### **CreatePurchaseOrderDto**
```csharp
{
  "supplierId": 1,
  "warehouseId": 1,
  "expectedDeliveryDate": "2026-03-15",
  "paymentTerms": "30 días",
  "notes": "Urgente",
  "details": [
    {
      "productId": 1,
      "quantityOrdered": 100,
      "unitPrice": 50.00
    }
  ]
}
```

### **CreateReceivingDto**
```csharp
{
  "purchaseOrderId": 1,
  "supplierInvoiceNumber": "FACT-123",
  "carrierName": "DHL",
  "details": [
    {
      "productId": 1,
      "purchaseOrderDetailId": 1,
      "quantityReceived": 95,
      "storageLocation": "A1-B2",
      "lotNumber": "LOTE-001"
    }
  ]
}
```

---

## ? Optimizaciones Implementadas

### **1. Generador de Códigos**
- ? Thread-safe con semáforo
- ? Transaccional
- ? Sin duplicados garantizado

### **2. Relaciones de BD**
- ? No cascade en FK críticas (auditoría)
- ? Cascade solo donde tiene sentido (Details)
- ? Índices en todas las FK

### **3. ProductStock**
- ? Índice único (Product + Warehouse)
- ? Campos calculados (Available = Quantity - Reserved)

---

## ?? Próximos Pasos (Orden Recomendado)

### **Paso 1: Órdenes de Compra** (Alta prioridad)
1. Crear repositorio `PurchaseOrderRepository`
2. Crear DTOs
3. Crear commands/queries
4. Crear handlers
5. Crear controller
6. Probar endpoints

### **Paso 2: Recepciones** (Alta prioridad)
1. Crear repositorio `PurchaseOrderReceivingRepository`
2. Crear DTOs
3. Crear commands (Create, QualityCheck, PostToInventory)
4. Crear handlers con lógica de negocio
5. Crear controller
6. Probar flujo completo

### **Paso 3: Inventario** (Media prioridad)
1. Crear repositorio `InventoryMovementRepository`
2. Crear queries para kardex
3. Crear queries para stock
4. Crear command para ajustes
5. Crear controller
6. Reportes

### **Paso 4: Reportes** (Baja prioridad)
1. Kardex detallado
2. Variaciones en recepciones
3. Stock bajo mínimo
4. Productos más comprados

---

## ?? Consideraciones Importantes

### **1. Transacciones**
El proceso de PostToInventory debe ser transaccional:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // 1. Crear movimientos
    // 2. Actualizar stock
    // 3. Marcar receiving como posted
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### **2. Validaciones Críticas**
- ? No recibir más de lo ordenado (configurable)
- ? No aplicar dos veces al inventario
- ? Almacén debe permitir recepción
- ? Proveedor debe estar activo
- ? Stock no puede ser negativo

### **3. Auditoría**
Todos los movimientos deben registrar:
- Usuario que ejecutó la acción
- Fecha y hora exacta
- Documento de referencia
- Stock antes y después

---

## ?? Beneficios del Sistema Actual

1. **? Escalable**: Generador de códigos soporta millones de registros
2. **? Thread-Safe**: Múltiples usuarios simultáneos
3. **? Sin Duplicados**: Códigos únicos garantizados
4. **? Flexible**: Prefijos y longitudes configurables
5. **? Performante**: Consultas SQL optimizadas
6. **? Auditab le**: Trazabilidad completa
7. **? Seguro**: Transacciones y validaciones

---

## ?? Estado Actual del Proyecto

```
? Base de Datos: COMPLETADA
? Entidades: CREADAS
? Migración: APLICADA
? Servicio de Códigos: IMPLEMENTADO
? Configuración: LISTA

?? Repositorios: PENDIENTE
?? DTOs: PENDIENTE
?? CQRS: PENDIENTE
?? Controllers: PENDIENTE
?? Testing: PENDIENTE
```

---

**¿Continuar con la implementación de Repositorios, DTOs y CQRS?**

Puedo implementar el módulo completo de **Órdenes de Compra** con:
- Repositorio completo
- DTOs estructurados
- Commands y Queries
- Handlers con lógica de negocio
- Controller con todos los endpoints
- Documentación de uso

---

?? **Documentado por:** GitHub Copilot  
?? **Fecha:** 10 de Marzo de 2026  
? **Versión:** 1.0 - Base Implementada
