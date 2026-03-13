# ?? API de Órdenes de Compra - Guía Completa de Endpoints

## ?? BASE URL
```
http://localhost:7254/api/PurchaseOrders
```

---

## ?? ÍNDICE DE ENDPOINTS

1. [Consultas (GET)](#consultas)
   - [Obtener todas las órdenes](#1-obtener-todas-las-órdenes-de-compra)
   - [Obtener órdenes paginadas](#2-obtener-órdenes-paginadas-con-filtros)
   - [Obtener orden por ID](#3-obtener-orden-por-id)
   - [Obtener orden por código](#4-obtener-orden-por-código)
   - [Obtener órdenes pendientes de recibir](#5-obtener-órdenes-pendientes-de-recibir)
2. [Crear (POST)](#crear)
   - [Crear orden de compra](#6-crear-orden-de-compra)
3. [Cambios de Estado (PATCH)](#cambios-de-estado)
   - [Aprobar orden](#7-aprobar-orden)
   - [Marcar como en tránsito](#8-marcar-como-en-tránsito)
4. [Cancelar (DELETE)](#cancelar)
   - [Cancelar orden](#9-cancelar-orden)

---

## ?? CONSULTAS

### 1. Obtener todas las órdenes de compra

**Endpoint:**
```http
GET /api/PurchaseOrders
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Query Parameters:**
| Parámetro | Tipo | Requerido | Default | Descripción |
|-----------|------|-----------|---------|-------------|
| `includeInactive` | boolean | No | false | Incluir órdenes inactivas/canceladas |

**Ejemplo:**
```http
GET /api/PurchaseOrders?includeInactive=false
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200):**
```json
{
  "message": "Órdenes de compra obtenidas exitosamente",
  "error": 0,
  "purchaseOrders": [
    {
      "id": 1,
      "code": "OC-001",
      "supplierId": 1,
      "supplierCode": "PROV-001",
      "supplierName": "Proveedor ABC S.A.",
      "warehouseId": 1,
      "warehouseCode": "ALM-001",
      "warehouseName": "Almacén Principal",
      "orderDate": "2026-03-10T00:00:00",
      "expectedDeliveryDate": "2026-03-15T00:00:00",
      "status": "Pending",
      "subTotal": 11000.00,
      "tax": 1760.00,
      "total": 12760.00,
      "notes": "Orden urgente",
      "supplierReference": "REF-12345",
      "paymentTerms": "30 días",
      "deliveryTerms": "LAB",
      "isActive": true,
      "createdAt": "2026-03-10T10:30:00",
      "updatedAt": null,
      "createdByUserName": "Administrador",
      "updatedByUserName": null,
      "details": [
        {
          "id": 1,
          "productId": 1,
          "productCode": "PROD-001",
          "productName": "Monitor LG 24 pulgadas",
          "quantityOrdered": 100,
          "quantityReceived": 0,
          "quantityPending": 100,
          "unitPrice": 50.00,
          "discount": 0.00,
          "taxPercentage": 16.00,
          "subTotal": 5000.00,
          "taxAmount": 800.00,
          "total": 5800.00,
          "notes": null
        },
        {
          "id": 2,
          "productId": 2,
          "productCode": "PROD-002",
          "productName": "Teclado Logitech",
          "quantityOrdered": 50,
          "quantityReceived": 0,
          "quantityPending": 50,
          "unitPrice": 120.00,
          "discount": 0.00,
          "taxPercentage": 16.00,
          "subTotal": 6000.00,
          "taxAmount": 960.00,
          "total": 6960.00,
          "notes": null
        }
      ],
      "totalItems": 2,
      "totalQuantityOrdered": 150,
      "totalQuantityReceived": 0,
      "completionPercentage": 0.00
    }
  ],
  "totalOrders": 1,
  "pendingOrders": 1,
  "approvedOrders": 0,
  "receivedOrders": 0
}
```

---

### 2. Obtener órdenes paginadas con filtros

**Endpoint:**
```http
GET /api/PurchaseOrders/paged
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Query Parameters:**
| Parámetro | Tipo | Requerido | Default | Descripción |
|-----------|------|-----------|---------|-------------|
| `pageNumber` | int | No | 1 | Número de página |
| `pageSize` | int | No | 10 | Tamaño de página (máx 100) |
| `includeInactive` | boolean | No | false | Incluir inactivas |
| `searchTerm` | string | No | null | Buscar en código, proveedor, almacén, referencia |
| `status` | string | No | null | Filtrar por estado (Pending, Approved, etc.) |
| `supplierId` | int | No | null | Filtrar por proveedor |
| `warehouseId` | int | No | null | Filtrar por almacén |

**Ejemplos:**

```http
# Página 1 con 20 registros
GET /api/PurchaseOrders/paged?pageNumber=1&pageSize=20

# Buscar órdenes que contengan "ABC"
GET /api/PurchaseOrders/paged?searchTerm=ABC

# Filtrar por estado
GET /api/PurchaseOrders/paged?status=Approved

# Filtrar por proveedor
GET /api/PurchaseOrders/paged?supplierId=1

# Combinación de filtros
GET /api/PurchaseOrders/paged?pageNumber=1&pageSize=10&status=Pending&supplierId=1
```

**Respuesta Exitosa (200):**
```json
{
  "message": "Órdenes de compra obtenidas exitosamente",
  "error": 0,
  "data": [
    {
      "id": 1,
      "code": "OC-001",
      "supplierId": 1,
      "supplierCode": "PROV-001",
      "supplierName": "Proveedor ABC S.A.",
      "warehouseId": 1,
      "warehouseCode": "ALM-001",
      "warehouseName": "Almacén Principal",
      "orderDate": "2026-03-10T00:00:00",
      "expectedDeliveryDate": "2026-03-15T00:00:00",
      "status": "Pending",
      "subTotal": 11000.00,
      "tax": 1760.00,
      "total": 12760.00,
      "notes": null,
      "details": [...],
      "totalItems": 2,
      "totalQuantityOrdered": 150,
      "totalQuantityReceived": 0,
      "completionPercentage": 0.00
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalRecords": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### 3. Obtener orden por ID

**Endpoint:**
```http
GET /api/PurchaseOrders/{id}
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Path Parameters:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `id` | int | Sí | ID de la orden de compra |

**Ejemplo:**
```http
GET /api/PurchaseOrders/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200):**
```json
{
  "message": "Orden de compra obtenida exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "OC-001",
    "supplierId": 1,
    "supplierCode": "PROV-001",
    "supplierName": "Proveedor ABC S.A.",
    "warehouseId": 1,
    "warehouseCode": "ALM-001",
    "warehouseName": "Almacén Principal",
    "orderDate": "2026-03-10T00:00:00",
    "expectedDeliveryDate": "2026-03-15T00:00:00",
    "status": "Pending",
    "subTotal": 11000.00,
    "tax": 1760.00,
    "total": 12760.00,
    "notes": "Orden urgente",
    "supplierReference": "REF-12345",
    "paymentTerms": "30 días",
    "deliveryTerms": "LAB",
    "isActive": true,
    "createdAt": "2026-03-10T10:30:00",
    "updatedAt": null,
    "createdByUserName": "Administrador",
    "updatedByUserName": null,
    "details": [...],
    "totalItems": 2,
    "totalQuantityOrdered": 150,
    "totalQuantityReceived": 0,
    "completionPercentage": 0.00
  }
}
```

**Respuesta Error - No Encontrada (404):**
```json
{
  "message": "Orden de compra no encontrada",
  "error": 1
}
```

---

### 4. Obtener orden por código

**Endpoint:**
```http
GET /api/PurchaseOrders/code/{code}
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Path Parameters:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `code` | string | Sí | Código de la orden (ej: OC-001) |

**Ejemplo:**
```http
GET /api/PurchaseOrders/code/OC-001
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta:** Igual que el endpoint por ID

---

### 5. Obtener órdenes pendientes de recibir

**Endpoint:**
```http
GET /api/PurchaseOrders/pending-to-receive
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Descripción:** Retorna órdenes con estado `Approved`, `InTransit` o `PartiallyReceived`

**Ejemplo:**
```http
GET /api/PurchaseOrders/pending-to-receive
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200):**
```json
{
  "message": "Órdenes pendientes de recibir obtenidas exitosamente",
  "error": 0,
  "purchaseOrders": [
    {
      "id": 1,
      "code": "OC-001",
      "status": "Approved",
      ...
    }
  ],
  "totalOrders": 1,
  "pendingOrders": 0,
  "approvedOrders": 0,
  "receivedOrders": 0
}
```

---

## ? CREAR

### 6. Crear orden de compra

**Endpoint:**
```http
POST /api/PurchaseOrders
```

**Headers:**
```http
Authorization: Bearer {token}
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "supplierId": 1,
  "warehouseId": 1,
  "orderDate": "2026-03-10",
  "expectedDeliveryDate": "2026-03-15",
  "notes": "Orden urgente",
  "supplierReference": "REF-12345",
  "paymentTerms": "30 días",
  "deliveryTerms": "LAB",
  "details": [
    {
      "productId": 1,
      "quantityOrdered": 100,
      "unitPrice": 50.00,
      "discount": 0.00,
      "taxPercentage": 16.00,
      "notes": null
    },
    {
      "productId": 2,
      "quantityOrdered": 50,
      "unitPrice": 120.00,
      "discount": 0.00,
      "taxPercentage": 16.00,
      "notes": null
    }
  ]
}
```

**Campos del Body:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `supplierId` | int | ? Sí | ID del proveedor |
| `warehouseId` | int | ? Sí | ID del almacén destino |
| `orderDate` | string (date) | No | Fecha de la orden (default: hoy) |
| `expectedDeliveryDate` | string (date) | No | Fecha esperada de entrega |
| `notes` | string | No | Observaciones |
| `supplierReference` | string | No | Referencia del proveedor |
| `paymentTerms` | string | No | Términos de pago (ej: "30 días") |
| `deliveryTerms` | string | No | Condiciones de entrega (ej: "LAB", "FOB") |
| `details` | array | ? Sí | Detalle de productos |

**Campos de `details[]`:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `productId` | int | ? Sí | ID del producto |
| `quantityOrdered` | decimal | ? Sí | Cantidad a ordenar (> 0) |
| `unitPrice` | decimal | ? Sí | Precio unitario (? 0) |
| `discount` | decimal | No | Descuento por línea (default: 0) |
| `taxPercentage` | decimal | No | % de IVA (default: 16) |
| `notes` | string | No | Notas del producto |

**Validaciones automáticas:**
- ? Proveedor debe existir y estar activo
- ? Almacén debe existir, estar activo y permitir recepción
- ? Productos deben existir y estar activos
- ? Cantidad > 0
- ? Precio ? 0
- ? Código se genera automáticamente (OC-001, OC-002, etc.)
- ? SubTotal, Tax y Total se calculan automáticamente

**Respuesta Exitosa (200):**
```json
{
  "message": "Orden de compra creada exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "OC-001",  // ? GENERADO AUTOMÁTICAMENTE
    "supplierId": 1,
    "supplierCode": "PROV-001",
    "supplierName": "Proveedor ABC S.A.",
    "warehouseId": 1,
    "warehouseCode": "ALM-001",
    "warehouseName": "Almacén Principal",
    "orderDate": "2026-03-10T00:00:00",
    "expectedDeliveryDate": "2026-03-15T00:00:00",
    "status": "Pending",  // ? ESTADO INICIAL
    "subTotal": 11000.00,  // ? CALCULADO
    "tax": 1760.00,  // ? CALCULADO
    "total": 12760.00,  // ? CALCULADO
    "notes": "Orden urgente",
    "supplierReference": "REF-12345",
    "paymentTerms": "30 días",
    "deliveryTerms": "LAB",
    "isActive": true,
    "createdAt": "2026-03-10T10:30:00",
    "updatedAt": null,
    "createdByUserName": "Administrador",
    "updatedByUserName": null,
    "details": [
      {
        "id": 1,
        "productId": 1,
        "productCode": "PROD-001",
        "productName": "Monitor LG 24 pulgadas",
        "quantityOrdered": 100,
        "quantityReceived": 0,  // ? INICIA EN 0
        "quantityPending": 100,  // ? IGUAL A ORDERED
        "unitPrice": 50.00,
        "discount": 0.00,
        "taxPercentage": 16.00,
        "subTotal": 5000.00,  // ? CALCULADO
        "taxAmount": 800.00,  // ? CALCULADO
        "total": 5800.00,  // ? CALCULADO
        "notes": null
      },
      {
        "id": 2,
        "productId": 2,
        "productCode": "PROD-002",
        "productName": "Teclado Logitech",
        "quantityOrdered": 50,
        "quantityReceived": 0,
        "quantityPending": 50,
        "unitPrice": 120.00,
        "discount": 0.00,
        "taxPercentage": 16.00,
        "subTotal": 6000.00,
        "taxAmount": 960.00,
        "total": 6960.00,
        "notes": null
      }
    ],
    "totalItems": 2,  // ? CANTIDAD DE LÍNEAS
    "totalQuantityOrdered": 150,  // ? SUMA DE CANTIDADES
    "totalQuantityReceived": 0,
    "completionPercentage": 0.00
  }
}
```

**Errores Posibles:**

**404 - Proveedor no encontrado:**
```json
{
  "message": "Proveedor con ID 99 no encontrado o inactivo",
  "error": 1
}
```

**404 - Almacén no encontrado:**
```json
{
  "message": "Almacén con ID 99 no encontrado, inactivo o no permite recepción",
  "error": 1
}
```

**404 - Producto no encontrado:**
```json
{
  "message": "Producto con ID 99 no encontrado o inactivo",
  "error": 1
}
```

**400 - Cantidad inválida:**
```json
{
  "message": "La cantidad debe ser mayor a cero",
  "error": 1
}
```

**400 - Precio inválido:**
```json
{
  "message": "El precio unitario no puede ser negativo",
  "error": 1
}
```

---

## ?? CAMBIOS DE ESTADO

### 7. Aprobar orden

**Endpoint:**
```http
PATCH /api/PurchaseOrders/{id}/approve
```

**Headers:**
```http
Authorization: Bearer {token}
Content-Type: application/json
```

**Path Parameters:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `id` | int | Sí | ID de la orden a aprobar |

**Body (JSON) - OPCIONAL:**
```json
{
  "notes": "Aprobado por gerencia"
}
```

**Ejemplo:**
```http
PATCH /api/PurchaseOrders/1/approve
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "notes": "Aprobado con urgencia"
}
```

**Validaciones:**
- ? Orden debe existir
- ? Estado actual debe ser `Pending`

**Respuesta Exitosa (200):**
```json
{
  "message": "Orden de compra aprobada exitosamente",
  "error": 0,
  "purchaseOrderId": 1
}
```

**Errores:**

**404 - Orden no encontrada:**
```json
{
  "message": "Orden de compra no encontrada",
  "error": 1
}
```

**400 - Estado inválido:**
```json
{
  "message": "Solo se pueden aprobar órdenes en estado Pending. Estado actual: Approved",
  "error": 1
}
```

---

### 8. Marcar como en tránsito

**Endpoint:**
```http
PATCH /api/PurchaseOrders/{id}/in-transit
```

**Headers:**
```http
Authorization: Bearer {token}
Content-Type: application/json
```

**Body (JSON) - OPCIONAL:**
```json
{
  "notes": "Envío DHL - Tracking: DHL123456789"
}
```

**Ejemplo:**
```http
PATCH /api/PurchaseOrders/1/in-transit
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "notes": "En tránsito vía DHL, tracking: DHL123456"
}
```

**Validaciones:**
- ? Orden debe existir
- ? Estado actual debe ser `Approved`

**Respuesta Exitosa (200):**
```json
{
  "message": "Orden marcada como en tránsito exitosamente",
  "error": 0,
  "purchaseOrderId": 1
}
```

**Errores:**

**400 - Estado inválido:**
```json
{
  "message": "Solo se pueden marcar como en tránsito órdenes aprobadas. Estado actual: Pending",
  "error": 1
}
```

---

## ??? CANCELAR

### 9. Cancelar orden

**Endpoint:**
```http
DELETE /api/PurchaseOrders/{id}
```

**Headers:**
```http
Authorization: Bearer {token}
Content-Type: application/json
```

**Body (JSON) - OPCIONAL:**
```json
{
  "notes": "Cancelado por duplicado"
}
```

**Ejemplo:**
```http
DELETE /api/PurchaseOrders/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "notes": "Cancelado por el proveedor"
}
```

**Validaciones:**
- ? Orden debe existir
- ? **NO se puede cancelar** si tiene recepciones (`Received` o `PartiallyReceived`)

**Respuesta Exitosa (200):**
```json
{
  "message": "Orden de compra cancelada exitosamente",
  "error": 0,
  "purchaseOrderId": 1
}
```

**Errores:**

**400 - No se puede cancelar:**
```json
{
  "message": "No se puede cancelar una orden que ya tiene recepciones",
  "error": 1
}
```

---

## ?? Estados de una Orden de Compra

| Estado | Descripción | Puede avanzar a |
|--------|-------------|-----------------|
| `Pending` | Creada, pendiente de aprobación | `Approved`, `Cancelled` |
| `Approved` | Aprobada, lista para envío | `InTransit`, `Cancelled` |
| `InTransit` | En tránsito hacia el almacén | `PartiallyReceived`, `Received` |
| `PartiallyReceived` | Recibida parcialmente | `Received` |
| `Received` | Completamente recibida | - (estado final) |
| `Cancelled` | Cancelada | - (estado final) |

---

## ?? Ejemplos de Uso Completo

### Ejemplo 1: Crear orden simple con 1 producto

```http
POST /api/PurchaseOrders
Authorization: Bearer {token}
Content-Type: application/json

{
  "supplierId": 1,
  "warehouseId": 1,
  "orderDate": "2026-03-10",
  "expectedDeliveryDate": "2026-03-15",
  "paymentTerms": "15 días",
  "details": [
    {
      "productId": 1,
      "quantityOrdered": 50,
      "unitPrice": 100.00
    }
  ]
}
```

**Resultado:** OC-001 creada por $5,800 (50 × $100 + IVA)

---

### Ejemplo 2: Crear orden con múltiples productos y descuentos

```http
POST /api/PurchaseOrders
Authorization: Bearer {token}
Content-Type: application/json

{
  "supplierId": 1,
  "warehouseId": 1,
  "orderDate": "2026-03-10",
  "expectedDeliveryDate": "2026-03-15",
  "notes": "Compra trimestral",
  "supplierReference": "REF-Q1-2026",
  "paymentTerms": "30 días",
  "details": [
    {
      "productId": 1,
      "quantityOrdered": 100,
      "unitPrice": 50.00,
      "discount": 500.00,
      "notes": "Descuento por volumen"
    },
    {
      "productId": 2,
      "quantityOrdered": 50,
      "unitPrice": 120.00,
      "discount": 0.00
    },
    {
      "productId": 3,
      "quantityOrdered": 25,
      "unitPrice": 200.00,
      "discount": 250.00
    }
  ]
}
```

**Cálculos:**
- Línea 1: (100 × $50 - $500) × 1.16 = $5,220
- Línea 2: (50 × $120) × 1.16 = $6,960
- Línea 3: (25 × $200 - $250) × 1.16 = $5,510
- **Total: $17,690**

---

### Ejemplo 3: Flujo completo (Crear ? Aprobar ? En Tránsito)

**Paso 1: Crear**
```http
POST /api/PurchaseOrders
{
  "supplierId": 1,
  "warehouseId": 1,
  "details": [...]
}
```
? Respuesta: `OC-001` con `status: "Pending"`

**Paso 2: Aprobar**
```http
PATCH /api/PurchaseOrders/1/approve
{
  "notes": "Aprobado por gerencia"
}
```
? `status` cambia a `"Approved"`

**Paso 3: Marcar en tránsito**
```http
PATCH /api/PurchaseOrders/1/in-transit
{
  "notes": "Tracking: DHL123456"
}
```
? `status` cambia a `"InTransit"`

**Paso 4: Recibir (próximo módulo)**
```http
POST /api/PurchaseOrderReceivings
{
  "purchaseOrderId": 1,
  ...
}
```
? `status` cambia a `"PartiallyReceived"` o `"Received"`

---

## ?? Autenticación

Todos los endpoints requieren autenticación JWT:

1. **Login:**
```http
POST /api/Login
Content-Type: application/json

{
  "userCode": "ADMIN001",
  "password": "admin123"
}
```

2. **Usar token en headers:**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? Códigos de Error

| Código | Descripción |
|--------|-------------|
| `0` | ? Operación exitosa |
| `1` | ? Recurso no encontrado o validación fallida |
| `2` | ? Error interno del servidor |

---

## ?? Notas Importantes

1. **Código automático:**
   - ? Se genera automáticamente (OC-001, OC-002, etc.)
   - ? Nunca se repite (thread-safe)
   - ? Secuencial sin importar eliminaciones

2. **Cálculos automáticos:**
   - ? SubTotal por línea = (Cantidad × Precio) - Descuento
   - ? Tax por línea = SubTotal × (TaxPercentage / 100)
   - ? Total por línea = SubTotal + Tax
   - ? Total orden = Suma de totales de líneas

3. **Estados:**
   - ? Estado inicial siempre es `Pending`
   - ? No se puede regresar de estado
   - ? `Received` y `Cancelled` son estados finales

4. **Validaciones:**
   - ? Proveedor, almacén y productos deben existir y estar activos
   - ? Almacén debe permitir recepción (`AllowsReceiving = true`)
   - ? Cantidades > 0
   - ? Precios ? 0

---

## ?? Próximos Módulos

1. **Recepciones** (`PurchaseOrderReceivings`)
   - Crear recepción contra OC
   - Control de calidad
   - Aplicar a inventario

2. **Movimientos de Inventario** (`InventoryMovements`)
   - Kardex
   - Consultas de stock

3. **Stock de Productos** (`ProductStock`)
   - Stock por almacén
   - Productos bajo mínimo

---

?? **Documentado por:** GitHub Copilot  
?? **Fecha:** 10 de Marzo de 2026  
? **Versión:** 1.0.0 - Módulo de Órdenes de Compra
