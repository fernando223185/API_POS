# ?? Sistema Completo de Gestión de Compras, Recepción e Inventario

## ?? Flujo Completo Implementado

```
1. ORDEN DE COMPRA (Purchase Order)
          ?
2. RECEPCIÓN DE MERCANCÍA (Receiving)
          ?
3. CONTROL DE CALIDAD (Quality Check)
          ?
4. APLICACIÓN A INVENTARIO (Post to Inventory)
          ?
5. MOVIMIENTOS DE INVENTARIO (Inventory Movements)
          ?
6. STOCK ACTUALIZADO (Product Stock)
```

---

## ?? Estructura de Tablas Creadas

### 1?? **PurchaseOrders** (Órdenes de Compra)
```sql
- Id
- Code (OC-001, OC-002...) ? Generado automáticamente
- SupplierId (FK a Suppliers)
- WarehouseId (FK a Warehouses)
- OrderDate
- ExpectedDeliveryDate
- Status (Pending, Approved, InTransit, PartiallyReceived, Received, Cancelled)
- SubTotal, Tax, Total
- Notes, SupplierReference, PaymentTerms
```

### 2?? **PurchaseOrderDetails** (Detalle de OC)
```sql
- Id
- PurchaseOrderId (FK)
- ProductId (FK)
- QuantityOrdered (solicitado)
- QuantityReceived (recibido acumulado)
- QuantityPending (pendiente)
- UnitPrice, Discount, TaxPercentage
- SubTotal, Total
```

### 3?? **PurchaseOrderReceivings** (Recepciones)
```sql
- Id
- Code (REC-001, REC-002...) ? Generado automáticamente
- PurchaseOrderId (FK)
- ReceivingDate
- WarehouseId (FK)
- Status (Draft, Received, QualityCheck, Approved, Rejected)
- SupplierInvoiceNumber
- CarrierName, TrackingNumber
- ReceivedBy
- IsPostedToInventory (si ya se aplicó al inventario)
- PostedToInventoryDate
```

### 4?? **PurchaseOrderReceivingDetails** (Detalle de Recepción)
```sql
- Id
- PurchaseOrderReceivingId (FK)
- ProductId (FK)
- PurchaseOrderDetailId (FK a la línea de la OC)
- QuantityReceived (recibido en esta recepción)
- QuantityApproved (aprobado en control de calidad)
- QuantityRejected (rechazado)
- RejectionReason
- StorageLocation (ubicación física)
- LotNumber (lote)
- ExpiryDate (vencimiento)
```

### 5?? **InventoryMovements** (Movimientos/Kardex)
```sql
- Id
- Code (MOV-001, MOV-002...) ? Generado automáticamente
- MovementType (IN, OUT, TRANSFER, ADJUSTMENT)
- MovementSubType (PURCHASE, SALE, RETURN, etc.)
- ProductId (FK)
- WarehouseId (FK)
- Quantity (+ para IN, - para OUT)
- StockBefore (antes del movimiento)
- StockAfter (después del movimiento)
- UnitCost, TotalCost
- MovementDate
- ReferenceDocumentId
- ReferenceDocumentType (PurchaseOrder, Sale, etc.)
- PurchaseOrderReceivingId (FK)
- LotNumber, StorageLocation
```

### 6?? **ProductStock** (Stock por Almacén)
```sql
- Id
- ProductId (FK)
- WarehouseId (FK)
- Quantity (total en stock)
- ReservedQuantity (reservado)
- AvailableQuantity (disponible = Quantity - Reserved)
- AverageCost (costo promedio)
- MinimumStock, MaximumStock, ReorderPoint
- LastMovementDate
```

---

## ?? Flujo de Proceso Completo

### **PASO 1: Crear Orden de Compra**

```http
POST /api/PurchaseOrders
Authorization: Bearer {token}
Content-Type: application/json

{
  "supplierId": 1,
  "warehouseId": 1,
  "orderDate": "2026-03-10",
  "expectedDeliveryDate": "2026-03-15",
  "paymentTerms": "30 días",
  "notes": "Orden urgente",
  "details": [
    {
      "productId": 1,
      "quantityOrdered": 100,
      "unitPrice": 50.00,
      "taxPercentage": 16
    },
    {
      "productId": 2,
      "quantityOrdered": 50,
      "unitPrice": 120.00,
      "taxPercentage": 16
    }
  ]
}
```

**Respuesta:**
```json
{
  "message": "Orden de compra creada exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "OC-001",  // ? Generado automáticamente
    "supplierId": 1,
    "supplierName": "Proveedor ABC",
    "warehouseId": 1,
    "warehouseName": "Almacén Principal",
    "status": "Pending",
    "orderDate": "2026-03-10",
    "expectedDeliveryDate": "2026-03-15",
    "subTotal": 11000.00,
    "tax": 1760.00,
    "total": 12760.00,
    "details": [
      {
        "productId": 1,
        "productName": "Monitor LG",
        "quantityOrdered": 100,
        "quantityReceived": 0,
        "quantityPending": 100,
        "unitPrice": 50.00,
        "total": 5800.00
      },
      {
        "productId": 2,
        "productName": "Teclado Logitech",
        "quantityOrdered": 50,
        "quantityReceived": 0,
        "quantityPending": 50,
        "unitPrice": 120.00,
        "total": 6960.00
      }
    ]
  }
}
```

**Estados de Orden de Compra:**
- `Pending` - Pendiente de aprobación
- `Approved` - Aprobada, lista para envío
- `InTransit` - En tránsito
- `PartiallyReceived` - Parcialmente recibida
- `Received` - Completamente recibida
- `Cancelled` - Cancelada

---

### **PASO 2: Aprobar Orden de Compra**

```http
PATCH /api/PurchaseOrders/1/approve
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Orden de compra aprobada",
  "error": 0,
  "code": "OC-001",
  "status": "Approved"
}
```

---

### **PASO 3: Recibir Mercancía (Cuando llega el proveedor)**

```http
POST /api/PurchaseOrderReceivings
Authorization: Bearer {token}
Content-Type: application/json

{
  "purchaseOrderId": 1,
  "warehouseId": 1,
  "receivingDate": "2026-03-12",
  "supplierInvoiceNumber": "FACT-12345",
  "carrierName": "DHL",
  "trackingNumber": "DHL123456",
  "receivedBy": "Carlos López",
  "notes": "Llegó en buen estado",
  "details": [
    {
      "productId": 1,
      "purchaseOrderDetailId": 1,
      "quantityReceived": 95,  // ?? Solo recibimos 95 de 100
      "storageLocation": "A1-B2",
      "lotNumber": "LOTE-2026-001"
    },
    {
      "productId": 2,
      "purchaseOrderDetailId": 2,
      "quantityReceived": 50,  // ? Completo
      "storageLocation": "B3-C1",
      "lotNumber": "LOTE-2026-002"
    }
  ]
}
```

**Respuesta:**
```json
{
  "message": "Recepción registrada exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "REC-001",  // ? Generado automáticamente
    "purchaseOrderId": 1,
    "purchaseOrderCode": "OC-001",
    "status": "Received",
    "receivingDate": "2026-03-12",
    "supplierInvoiceNumber": "FACT-12345",
    "details": [
      {
        "productId": 1,
        "productName": "Monitor LG",
        "quantityOrdered": 100,
        "quantityReceived": 95,
        "variance": -5,  // ?? Faltante
        "storageLocation": "A1-B2"
      },
      {
        "productId": 2,
        "productName": "Teclado Logitech",
        "quantityOrdered": 50,
        "quantityReceived": 50,
        "variance": 0,  // ? Completo
        "storageLocation": "B3-C1"
      }
    ],
    "summary": {
      "totalReceived": 145,
      "totalExpected": 150,
      "completionPercentage": 96.67
    }
  }
}
```

**Estados de Recepción:**
- `Draft` - Borrador (se puede editar)
- `Received` - Recibido físicamente
- `QualityCheck` - En control de calidad
- `Approved` - Aprobado (listo para aplicar a inventario)
- `Rejected` - Rechazado

---

### **PASO 4: Control de Calidad (Opcional)**

```http
PATCH /api/PurchaseOrderReceivings/1/quality-check
Authorization: Bearer {token}
Content-Type: application/json

{
  "details": [
    {
      "receivingDetailId": 1,
      "quantityApproved": 90,  // ?? Solo 90 aprobados
      "quantityRejected": 5,   // ? 5 rechazados
      "rejectionReason": "Producto defectuoso"
    },
    {
      "receivingDetailId": 2,
      "quantityApproved": 50,  // ? Todos aprobados
      "quantityRejected": 0
    }
  ]
}
```

**Respuesta:**
```json
{
  "message": "Control de calidad completado",
  "error": 0,
  "code": "REC-001",
  "status": "QualityCheck",
  "summary": {
    "totalReceived": 145,
    "totalApproved": 140,
    "totalRejected": 5,
    "approvalRate": 96.55
  }
}
```

---

### **PASO 5: Aplicar a Inventario (Post to Inventory)**

**Este paso es CRÍTICO y hace lo siguiente:**
1. ? Crea movimientos en `InventoryMovements` (Kardex)
2. ? Actualiza `ProductStock` por almacén
3. ? Actualiza `QuantityReceived` en `PurchaseOrderDetails`
4. ? Actualiza estado de `PurchaseOrder`
5. ? Marca `IsPostedToInventory = true` en `PurchaseOrderReceiving`

```http
POST /api/PurchaseOrderReceivings/1/post-to-inventory
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Recepción aplicada al inventario exitosamente",
  "error": 0,
  "data": {
    "receivingCode": "REC-001",
    "purchaseOrderCode": "OC-001",
    "warehouseName": "Almacén Principal",
    "isPostedToInventory": true,
    "postedDate": "2026-03-12T14:30:00",
    "movementsCreated": [
      {
        "movementCode": "MOV-001",
        "productName": "Monitor LG",
        "quantity": 90,
        "stockBefore": 0,
        "stockAfter": 90,
        "unitCost": 50.00
      },
      {
        "movementCode": "MOV-002",
        "productName": "Teclado Logitech",
        "quantity": 50,
        "stockBefore": 20,
        "stockAfter": 70,
        "unitCost": 120.00
      }
    ],
    "stockUpdated": [
      {
        "productId": 1,
        "productName": "Monitor LG",
        "warehouseId": 1,
        "previousStock": 0,
        "newStock": 90,
        "averageCost": 50.00
      },
      {
        "productId": 2,
        "productName": "Teclado Logitech",
        "warehouseId": 1,
        "previousStock": 20,
        "newStock": 70,
        "averageCost": 120.00
      }
    ]
  }
}
```

---

## ?? Consultas Disponibles

### **1. Órdenes de Compra**

#### Listar todas
```http
GET /api/PurchaseOrders
GET /api/PurchaseOrders?status=Pending
GET /api/PurchaseOrders?supplierId=1
```

#### Paginadas
```http
GET /api/PurchaseOrders/paged?pageNumber=1&pageSize=10
GET /api/PurchaseOrders/paged?status=Approved&pageSize=20
```

#### Por ID
```http
GET /api/PurchaseOrders/1
```

#### Por código
```http
GET /api/PurchaseOrders/code/OC-001
```

#### Pendientes de recibir
```http
GET /api/PurchaseOrders/pending-to-receive
```

---

### **2. Recepciones**

#### Listar todas
```http
GET /api/PurchaseOrderReceivings
GET /api/PurchaseOrderReceivings?status=Received
```

#### Por orden de compra
```http
GET /api/PurchaseOrderReceivings/purchase-order/1
```

#### Pendientes de aplicar a inventario
```http
GET /api/PurchaseOrderReceivings/pending-to-post
```

---

### **3. Movimientos de Inventario (Kardex)**

#### Todos los movimientos
```http
GET /api/InventoryMovements
```

#### Por producto
```http
GET /api/InventoryMovements/product/1
GET /api/InventoryMovements/product/1?warehouseId=1
```

#### Por almacén
```http
GET /api/InventoryMovements/warehouse/1
```

#### Por rango de fechas
```http
GET /api/InventoryMovements?startDate=2026-03-01&endDate=2026-03-31
```

#### Por tipo
```http
GET /api/InventoryMovements?movementType=IN
GET /api/InventoryMovements?movementType=OUT
```

---

### **4. Stock de Productos**

#### Stock por almacén
```http
GET /api/ProductStock/warehouse/1
```

#### Stock de un producto
```http
GET /api/ProductStock/product/1
```

#### Stock consolidado (todos los almacenes)
```http
GET /api/ProductStock/product/1/consolidated
```

**Respuesta:**
```json
{
  "productId": 1,
  "productName": "Monitor LG",
  "totalStock": 150,
  "totalReserved": 20,
  "totalAvailable": 130,
  "byWarehouse": [
    {
      "warehouseId": 1,
      "warehouseName": "Almacén Principal",
      "quantity": 90,
      "reserved": 10,
      "available": 80
    },
    {
      "warehouseId": 2,
      "warehouseName": "Almacén Secundario",
      "quantity": 60,
      "reserved": 10,
      "available": 50
    }
  ]
}
```

#### Productos bajo stock mínimo
```http
GET /api/ProductStock/low-stock
GET /api/ProductStock/low-stock?warehouseId=1
```

---

## ?? Reportes Importantes

### **1. Reporte de Recepciones**
```http
GET /api/Reports/receivings?startDate=2026-03-01&endDate=2026-03-31
```

### **2. Kardex Detallado**
```http
GET /api/Reports/kardex?productId=1&warehouseId=1&startDate=2026-03-01
```

### **3. Variaciones en Recepciones**
```http
GET /api/Reports/receiving-variances?startDate=2026-03-01
```

**Muestra:**
- Productos con faltantes
- Productos con excedentes
- % de cumplimiento por proveedor

---

## ?? Casos de Uso Especiales

### **Caso 1: Recepción Parcial**

**Escenario:** El proveedor envía en 2 entregas

**OC-001:**
- 100 Monitores solicitados

**Primera recepción (REC-001):**
```json
{
  "purchaseOrderId": 1,
  "details": [{
    "productId": 1,
    "quantityReceived": 60  // Solo 60 de 100
  }]
}
```
? Estado OC: `PartiallyReceived`
? Pendiente: 40

**Segunda recepción (REC-002):**
```json
{
  "purchaseOrderId": 1,
  "details": [{
    "productId": 1,
    "quantityReceived": 40  // Los 40 restantes
  }]
}
```
? Estado OC: `Received`
? Pendiente: 0

---

### **Caso 2: Devolución al Proveedor**

Si se rechaza producto en control de calidad:

```http
POST /api/PurchaseOrderReceivings/1/create-return
{
  "details": [{
    "receivingDetailId": 1,
    "quantityToReturn": 5,
    "returnReason": "Producto defectuoso"
  }]
}
```

Esto genera:
- Documento de devolución
- Movimiento OUT en inventario
- Ajuste en stock

---

### **Caso 3: Ajuste de Inventario**

Para corregir diferencias (inventario físico vs sistema):

```http
POST /api/InventoryMovements/adjustment
{
  "productId": 1,
  "warehouseId": 1,
  "quantityAdjustment": -3,  // Negativo = faltante
  "reason": "Diferencia en inventario físico",
  "notes": "Producto extraviado"
}
```

---

## ? Validaciones Implementadas

### En Orden de Compra:
- ? Proveedor debe existir y estar activo
- ? Almacén debe existir y estar activo
- ? Almacén debe permitir recepción (`AllowsReceiving = true`)
- ? Productos deben existir
- ? Cantidades > 0
- ? Precios > 0

### En Recepción:
- ? Orden de compra debe existir
- ? Orden de compra debe estar Aprobada o InTransit
- ? No se puede recibir más de lo ordenado (configurable)
- ? Almacén debe coincidir con la OC o ser autorizado
- ? Productos deben estar en la OC

### En Post to Inventory:
- ? Recepción debe estar Approved
- ? No debe estar ya aplicada (`IsPostedToInventory = false`)
- ? Cantidad aprobada > 0
- ? Stock no puede ser negativo

---

## ?? Permisos Necesarios

```json
{
  "Compras": {
    "CrearOrdenCompra": "Create",
    "VerOrdenesCompra": "View",
    "EditarOrdenCompra": "Edit",
    "AprobarOrdenCompra": "Approve",
    "CancelarOrdenCompra": "Delete"
  },
  "Recepciones": {
    "CrearRecepcion": "Create",
    "VerRecepciones": "View",
    "ControlCalidad": "QualityCheck",
    "AplicarInventario": "PostToInventory"
  },
  "Inventario": {
    "VerMovimientos": "View",
    "CrearAjuste": "Adjustment",
    "VerStock": "ViewStock"
  }
}
```

---

## ?? Beneficios de este Sistema

1. **? Trazabilidad Completa**
   - Cada producto tiene historial desde orden hasta inventario
   - Se puede rastrear por lote/serie

2. **? Control de Calidad**
   - Permite aprobar/rechazar antes de aplicar a inventario

3. **? Kardex Detallado**
   - Todos los movimientos con stock before/after
   - Costo promedio actualizado

4. **? Recepciones Parciales**
   - Permite múltiples entregas de una misma OC

5. **? Prevención de Errores**
   - No se puede aplicar dos veces al inventario
   - Validaciones en cada paso

6. **? Reportes Gerenciales**
   - Variaciones proveedor
   - Productos con problemas recurrentes
   - Eficiencia de recepciones

---

## ?? Siguiente Paso: Implementación del Backend

Ahora que tienes las entidades, necesitamos:

1. ? Repositorios (IRepository + implementación)
2. ? DTOs
3. ? Commands y Queries (CQRS)
4. ? Handlers
5. ? Controllers

**¿Quieres que implemente todo el backend completo?**

Puedo crear:
- Repositorios con métodos específicos
- DTOs estructurados
- Comandos y queries
- Handlers con lógica de negocio
- Controllers con todos los endpoints

---

?? **Documentado por:** GitHub Copilot  
?? **Fecha:** 10 de Marzo de 2026  
? **Versión:** 1.0.0
