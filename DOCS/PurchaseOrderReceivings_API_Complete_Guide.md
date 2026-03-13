# ?? API de Recepciones de Mercancía - Guía Completa

## ?? BASE URL
```
http://localhost:7254/api/PurchaseOrderReceivings
```

---

## ?? FLUJO DE NEGOCIO

```
1. CREAR ORDEN DE COMPRA ? Estado: Pending
2. APROBAR ORDEN ? Estado: Approved
3. MARCAR EN TRÁNSITO ? Estado: InTransit
4. CREAR RECEPCIÓN ?? ESTE MÓDULO
   ??> Estado OC: PartiallyReceived/Received
5. APLICAR A INVENTARIO ? CRÍTICO
   ??> Crea movimientos de inventario
   ??> Actualiza stock en almacén
```

---

## ?? ENDPOINTS DISPONIBLES

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/` | Obtener todas las recepciones |
| GET | `/paged` | Recepciones paginadas con filtros |
| GET | `/{id}` | Obtener recepción por ID |
| GET | `/purchase-order/{poId}` | Recepciones de una OC específica |
| GET | `/pending-to-post` | Recepciones pendientes de aplicar |
| POST | `/` | Crear nueva recepción |
| POST | `/{id}/post-to-inventory` | **Aplicar al inventario** ? |

---

## ?? CONSULTAS

### 1. Obtener todas las recepciones

**Endpoint:**
```http
GET /api/PurchaseOrderReceivings
```

**Query Parameters:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `includePosted` | boolean | true | Incluir recepciones ya aplicadas |

**Respuesta (200):**
```json
{
  "message": "Recepciones obtenidas exitosamente",
  "error": 0,
  "receivings": [
    {
      "id": 1,
      "code": "REC-001",
      "purchaseOrderId": 1,
      "purchaseOrderCode": "OC-001",
      "supplierName": "Proveedor ABC S.A.",
      "warehouseName": "Almacén Principal",
      "receivingDate": "2026-03-15",
      "status": "Received",
      "isPostedToInventory": false,
      "totalQuantityReceived": 145,
      "totalQuantityAccepted": 140,
      "totalQuantityRejected": 5
    }
  ],
  "totalReceivings": 10,
  "pendingToPost": 3,
  "posted": 7
}
```

---

### 2. Obtener recepciones paginadas

**Endpoint:**
```http
GET /api/PurchaseOrderReceivings/paged
```

**Query Parameters:**
| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Número de página |
| `pageSize` | int | 10 | Tamaño de página (máx 100) |
| `searchTerm` | string | null | Buscar en código, OC, proveedor |
| `purchaseOrderId` | int? | null | Filtrar por OC |
| `warehouseId` | int? | null | Filtrar por almacén |
| `status` | string? | null | Filtrar por estado |
| `onlyPendingToPost` | boolean? | null | Solo pendientes de aplicar |

**Ejemplos:**
```http
# Página 1 con 20 registros
GET /api/PurchaseOrderReceivings/paged?pageNumber=1&pageSize=20

# Solo pendientes de aplicar
GET /api/PurchaseOrderReceivings/paged?onlyPendingToPost=true

# Recepciones de OC-001
GET /api/PurchaseOrderReceivings/paged?purchaseOrderId=1
```

---

### 3. Obtener recepción por ID

**Endpoint:**
```http
GET /api/PurchaseOrderReceivings/{id}
```

**Respuesta (200):**
```json
{
  "message": "Recepción obtenida exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "REC-001",
    "purchaseOrderCode": "OC-001",
    "supplierName": "Proveedor ABC",
    "warehouseName": "Almacén Principal",
    "receivingDate": "2026-03-15",
    "supplierInvoiceNumber": "FACT-12345",
    "carrierName": "DHL",
    "trackingNumber": "DHL987654321",
    "status": "Received",
    "isPostedToInventory": false,
    "details": [
      {
        "id": 1,
        "productCode": "PROD-001",
        "productName": "Monitor LG 24 pulgadas",
        "quantityOrdered": 100,
        "quantityReceived": 95,
        "quantityAccepted": 90,
        "quantityRejected": 5,
        "quantityPending": 10,
        "storageLocation": "A1-B2",
        "lotNumber": "LOTE-2026-001"
      }
    ]
  }
}
```

---

### 4. Obtener recepciones de una OC

**Endpoint:**
```http
GET /api/PurchaseOrderReceivings/purchase-order/{purchaseOrderId}
```

**Ejemplo:**
```http
GET /api/PurchaseOrderReceivings/purchase-order/1
```

---

### 5. Obtener recepciones pendientes de aplicar

**Endpoint:**
```http
GET /api/PurchaseOrderReceivings/pending-to-post
```

**Uso:** Para mostrar un dashboard de recepciones que necesitan ser aplicadas al inventario.

---

## ? CREAR RECEPCIÓN

### Endpoint:
```http
POST /api/PurchaseOrderReceivings
```

### Body:
```json
{
  "purchaseOrderId": 1,
  "receivingDate": "2026-03-15",
  "supplierInvoiceNumber": "FACT-12345",
  "carrierName": "DHL",
  "trackingNumber": "DHL987654321",
  "notes": "Recepción parcial - 5 monitores dañados",
  "details": [
    {
      "purchaseOrderDetailId": 1,
      "productId": 1,
      "quantityReceived": 95,
      "quantityAccepted": 90,
      "quantityRejected": 5,
      "storageLocation": "A1-B2",
      "lotNumber": "LOTE-2026-001",
      "expirationDate": null,
      "notes": "5 unidades con pantalla rota"
    },
    {
      "purchaseOrderDetailId": 2,
      "productId": 2,
      "quantityReceived": 50,
      "quantityAccepted": 50,
      "quantityRejected": 0,
      "storageLocation": "A1-C3",
      "lotNumber": "LOTE-2026-002",
      "expirationDate": null,
      "notes": null
    }
  ]
}
```

### Validaciones:
- ? OC debe estar en estado: Approved, InTransit o PartiallyReceived
- ? `quantityReceived` ? `quantityOrdered` (no recibir más de lo ordenado)
- ? `quantityAccepted + quantityRejected = quantityReceived`

### Respuesta Exitosa (200):
```json
{
  "message": "Recepción creada exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "REC-001",  // ? GENERADO AUTOMÁTICAMENTE
    "purchaseOrderCode": "OC-001",
    "status": "Received",
    "isPostedToInventory": false,  // ?? AÚN NO APLICADO
    "totalQuantityReceived": 145,
    "totalQuantityAccepted": 140,
    "totalQuantityRejected": 5
  }
}
```

### Qué sucede automáticamente:
1. ? Genera código (REC-001, REC-002, etc.)
2. ? Actualiza `QuantityReceived` en la OC
3. ? Actualiza estado de OC:
   - Todo recibido ? `Received`
   - Parcial ? `PartiallyReceived`

---

## ? APLICAR A INVENTARIO (CRÍTICO)

### Endpoint:
```http
POST /api/PurchaseOrderReceivings/{id}/post-to-inventory
```

### Descripción:
**Este es el paso MÁS IMPORTANTE**. Aplicar una recepción al inventario:
1. Crea movimientos de inventario (`InventoryMovements`)
2. Actualiza stock en almacén (`ProductStock`)
3. Marca la recepción como aplicada

### Ejemplo:
```http
POST /api/PurchaseOrderReceivings/1/post-to-inventory
Authorization: Bearer {token}
```

### Respuesta Exitosa (200):
```json
{
  "message": "Recepción aplicada a inventario exitosamente",
  "error": 0,
  "data": {
    "receivingId": 1,
    "receivingCode": "REC-001",
    "totalMovementsCreated": 2,
    "movements": [
      {
        "movementCode": "MOV-001",
        "productCode": "PROD-001",
        "productName": "Monitor LG 24 pulgadas",
        "warehouseCode": "ALM-001",
        "quantity": 90,
        "stockBefore": 10,
        "stockAfter": 100  // ? STOCK ACTUALIZADO
      },
      {
        "movementCode": "MOV-002",
        "productCode": "PROD-002",
        "productName": "Teclado Logitech",
        "warehouseCode": "ALM-001",
        "quantity": 50,
        "stockBefore": 5,
        "stockAfter": 55  // ? STOCK ACTUALIZADO
      }
    ]
  }
}
```

### Validaciones:
- ? Recepción NO debe estar aplicada (`IsPostedToInventory = false`)
- ? Estado debe ser `Received` o `QualityCheck`
- ?? **ACCIÓN IRREVERSIBLE** - No se puede deshacer

### Qué sucede en el backend:
```csharp
// Por cada producto ACEPTADO:
1. Genera código movimiento (MOV-001, MOV-002, ...)
2. Crea InventoryMovement:
   - Tipo: IN (Entrada)
   - Cantidad: quantityAccepted
   - Stock antes/después
3. Actualiza/Crea ProductStock:
   - Si no existe ? crea registro
   - Si existe ? suma cantidad
4. Marca recepción como aplicada:
   - IsPostedToInventory = true
   - PostedToInventoryDate = NOW
```

---

## ?? ESTADOS DE RECEPCIÓN

| Estado | Descripción | Puede Aplicar |
|--------|-------------|---------------|
| `Received` | Recibida físicamente | ? Sí |
| `QualityCheck` | En control de calidad | ? Sí |
| `Applied` | Aplicada a inventario | ? No |
| `Rejected` | Rechazada | ? No |

---

## ?? VALIDACIONES IMPORTANTES

### Al crear recepción:
```javascript
// 1. Validar que no se reciba más de lo ordenado
if (quantityReceived > quantityOrdered) {
  throw "No se puede recibir más de lo ordenado";
}

// 2. Validar que aceptado + rechazado = recibido
if (quantityAccepted + quantityRejected !== quantityReceived) {
  throw "La suma de aceptados y rechazados debe ser igual a recibidos";
}

// 3. Validar estado de OC
if (!['Approved', 'InTransit', 'PartiallyReceived'].includes(orderStatus)) {
  throw "La OC debe estar aprobada o en tránsito";
}
```

### Al aplicar inventario:
```javascript
// 1. Validar que no esté aplicada
if (receiving.isPostedToInventory) {
  throw "Ya fue aplicada al inventario";
}

// 2. Confirmación del usuario
const confirmed = confirm("¿Está seguro? Esta acción NO se puede deshacer");
if (!confirmed) return;
```

---

## ?? FLUJO COMPLETO DE EJEMPLO

### 1. Crear OC
```http
POST /api/PurchaseOrders
{
  "supplierId": 1,
  "warehouseId": 1,
  "details": [
    { "productId": 1, "quantityOrdered": 100, "unitPrice": 50 }
  ]
}
```
**Resultado:** OC-001 | Estado: Pending

### 2. Aprobar OC
```http
PATCH /api/PurchaseOrders/1/approve
```
**Resultado:** OC-001 | Estado: Approved

### 3. Recibir Mercancía
```http
POST /api/PurchaseOrderReceivings
{
  "purchaseOrderId": 1,
  "receivingDate": "2026-03-15",
  "details": [
    {
      "purchaseOrderDetailId": 1,
      "productId": 1,
      "quantityReceived": 95,
      "quantityAccepted": 90,
      "quantityRejected": 5
    }
  ]
}
```
**Resultado:** 
- REC-001 creada
- OC-001 | Estado: PartiallyReceived
- Stock: SIN CAMBIOS (todavía no aplicado)

### 4. Aplicar a Inventario
```http
POST /api/PurchaseOrderReceivings/1/post-to-inventory
```
**Resultado:**
- MOV-001 creado
- Stock Monitor: 10 ? 100 (+90)
- REC-001 | IsPostedToInventory: true

---

## ?? NOTAS CRÍTICAS

1. **SOLO productos aceptados** entran al inventario
2. **Productos rechazados** NO afectan el stock
3. **Una vez aplicada**, NO se puede editar ni deshacer
4. **El código REC-XXX** se genera automáticamente
5. **Múltiples recepciones** pueden aplicarse a la misma OC (entregas parciales)

---

## ?? INTEGRACIÓN CON OTROS MÓDULOS

### Con Órdenes de Compra:
```http
# Obtener OCs disponibles para recibir
GET /api/PurchaseOrders/pending-to-receive
```

### Con Inventario:
```http
# Ver stock actualizado después de aplicar
GET /api/ProductStock?warehouseId=1
```

### Con Movimientos:
```http
# Ver kardex del producto
GET /api/InventoryMovements?productId=1
```

---

?? **Documentado por:** GitHub Copilot  
?? **Fecha:** 10 de Marzo de 2026  
? **Versión:** 1.0.0 - Módulo de Recepciones Completo  
?? **Estado:** LISTO PARA PRODUCCIÓN
