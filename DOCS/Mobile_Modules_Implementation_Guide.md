# Guía de Implementación: Módulos Móviles
## Ventas Delivery + Cotizaciones (QR → Venta)

> **Base URL:** `https://<tu-servidor>/api`  
> **Autenticación:** Bearer Token JWT en header `Authorization: Bearer {token}`  
> **Todos los requests/responses son JSON (`Content-Type: application/json`)**

---

## ÍNDICE
1. [Módulo: Ventas Delivery](#modulo-ventas-delivery)
2. [Módulo: Cotizaciones](#modulo-cotizaciones)
3. [Flujo QR: Cotización → Venta](#flujo-qr)
4. [Catálogo de Formas de Pago](#formas-de-pago)
5. [Estados y Códigos de Error](#estados)

---

## 1. MÓDULO: VENTAS DELIVERY {#modulo-ventas-delivery}

Una **venta Delivery** es una venta que se crea con dirección de entrega y fecha estimada.  
El inventario y el pago se procesan al confirmar la entrega (`PUT /deliver`).

### Estados posibles de una venta Delivery
```
Draft → Completed
Draft → Cancelled
```
- `Draft`: Creada, pendiente de entrega y pago
- `Completed`: Entregada y pagada (inventario ya descontado)
- `Cancelled`: Cancelada

---

### 1.1 Crear venta Delivery

```
POST /api/sales/delivery
Authorization: Bearer {token}
```

**Request body:**
```json
{
  "customerId": 5,               // opcional (null = Público General)
  "warehouseId": 1,              // REQUERIDO
  "priceListId": null,           // opcional
  "discountPercentage": 0,       // 0-100
  "requiresInvoice": false,
  "deliveryAddress": "Calle 5 de Mayo #123, Col. Centro",  // opcional
  "scheduledDeliveryDate": "2026-04-20T14:00:00Z",         // opcional
  "notes": "Llamar antes de llegar",
  "details": [
    {
      "productId": 12,
      "quantity": 2,
      "unitPrice": 150.00,
      "discountPercentage": 0,
      "notes": null
    },
    {
      "productId": 8,
      "quantity": 1,
      "unitPrice": 80.00,
      "discountPercentage": 10,
      "notes": null
    }
  ]
}
```

**Respuesta exitosa (200):**
```json
{
  "message": "Venta delivery creada exitosamente",
  "error": 0,
  "data": {
    "id": 42,
    "code": "VTA-000042",
    "saleDate": "2026-04-17T19:00:00Z",
    "saleType": "Delivery",
    "status": "Draft",
    "customerId": 5,
    "customerName": "Juan Pérez",
    "warehouseId": 1,
    "warehouseName": "Almacén Principal",
    "userId": 3,
    "userName": "vendedor01",
    "deliveryAddress": "Calle 5 de Mayo #123, Col. Centro",
    "scheduledDeliveryDate": "2026-04-20T14:00:00Z",
    "deliveredAt": null,
    "subTotal": 352.59,
    "discountAmount": 8.62,
    "discountPercentage": 0,
    "taxAmount": 54.62,
    "total": 398.62,
    "amountPaid": 0,
    "isPaid": false,
    "requiresInvoice": false,
    "notes": "Llamar antes de llegar",
    "createdAt": "2026-04-17T19:00:00Z",
    "details": [
      {
        "id": 88,
        "productId": 12,
        "productCode": "PRD-000012",
        "productName": "Producto A",
        "quantity": 2,
        "unitPrice": 150.00,
        "discountPercentage": 0,
        "discountAmount": 0,
        "taxPercentage": 0.16,
        "taxAmount": 48.00,
        "subTotal": 300.00,
        "total": 348.00,
        "notes": null
      }
    ],
    "payments": []
  }
}
```

**Errores posibles:**
| HTTP | `error` | `message` |
|------|---------|-----------|
| 404 | 1 | "Almacén con ID X no encontrado" |
| 404 | 1 | "Producto con ID X no encontrado" |
| 400 | 1 | "El producto 'X' está inactivo" |
| 400 | 1 | "Stock insuficiente para..." |
| 401 | 1 | "Usuario no autenticado" |

---

### 1.2 Confirmar entrega y registrar pago

```
PUT /api/sales/{id}/deliver
Authorization: Bearer {token}
```

Cuando el repartidor entrega el pedido: descuenta inventario, registra el pago y marca la venta como `Completed`.

**Request body:**
```json
{
  "deliveryAddress": "Calle 5 de Mayo #123, Col. Centro",  // opcional (actualiza la dirección real)
  "payments": [
    {
      "paymentMethod": "Efectivo",
      "amount": 400.00
    }
  ],
  "notes": "Entregado en portería"
}
```

**Respuesta exitosa (200):**
```json
{
  "message": "Entrega confirmada y pagos registrados",
  "error": 0,
  "data": {
    "id": 42,
    "code": "VTA-000042",
    "saleType": "Delivery",
    "status": "Completed",
    "deliveredAt": "2026-04-20T14:30:00Z",
    "amountPaid": 400.00,
    "changeAmount": 1.38,
    "isPaid": true,
    "total": 398.62,
    "payments": [
      {
        "id": 15,
        "paymentMethod": "Efectivo",
        "amount": 400.00,
        "paymentDate": "2026-04-20T14:30:00Z",
        "status": "Completed"
      }
    ]
  }
}
```

**Errores posibles:**
| HTTP | mensaje |
|------|---------|
| 404 | Venta no encontrada |
| 400 | "Esta venta no es de tipo Delivery" |
| 400 | "La venta ya fue completada" |
| 400 | "Stock insuficiente para..." |

---

### 1.3 Listar ventas Delivery pendientes

Para mostrar la lista de pedidos pendientes de entrega, usar el endpoint general de ventas con filtro:

```
GET /api/sales?saleType=Delivery&status=Draft&page=1&pageSize=20
Authorization: Bearer {token}
```

**Query params disponibles:**
| Param | Tipo | Descripción |
|-------|------|-------------|
| `page` | int | Número de página (default: 1) |
| `pageSize` | int | Registros por página (default: 20) |
| `warehouseId` | int | Filtrar por almacén |
| `customerId` | int | Filtrar por cliente |
| `status` | string | `Draft`, `Completed`, `Cancelled` |
| `saleType` | string | `POS`, `Delivery` |
| `fromDate` | datetime | Fecha inicio |
| `toDate` | datetime | Fecha fin |

**Respuesta (200):**
```json
{
  "error": 0,
  "data": [
    {
      "id": 42,
      "code": "VTA-000042",
      "saleDate": "2026-04-17T19:00:00Z",
      "saleType": "Delivery",
      "status": "Draft",
      "customerName": "Juan Pérez",
      "warehouseName": "Almacén Principal",
      "deliveryAddress": "Calle 5 de Mayo #123",
      "scheduledDeliveryDate": "2026-04-20T14:00:00Z",
      "deliveredAt": null,
      "total": 398.62,
      "isPaid": false,
      "totalItems": 2,
      "userName": "vendedor01"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalRecords": 5,
  "totalPages": 1
}
```

---

### 1.4 Ver detalle de una venta

```
GET /api/sales/{id}
Authorization: Bearer {token}
```

Devuelve el objeto `SaleResponseDto` completo (misma estructura que la respuesta de creación).

---

## 2. MÓDULO: COTIZACIONES {#modulo-cotizaciones}

Una **cotización** no mueve inventario ni genera pago.  
Se genera un **código único** (ej. `COT-000001`) que se incluye en un QR.  
Al escanear el QR, el cliente o vendedor convierte la cotización en una venta real.

### Estados posibles
```
Draft → Converted (al escanear QR y confirmar)
Draft → Cancelled
```
| Estado | Descripción |
|--------|-------------|
| `Draft` | Recién creada |
| `Sent` | Enviada al cliente (actualización manual de status) |
| `Accepted` | Cliente la aceptó |
| `Rejected` | Cliente la rechazó |
| `Expired` | Venció sin convertirse |
| `Converted` | Se convirtió en venta |
| `Cancelled` | Cancelada manualmente |

---

### 2.1 Crear cotización

```
POST /api/quotations
Authorization: Bearer {token}
```

**Request body:**
```json
{
  "customerId": 5,               // opcional
  "warehouseId": 1,              // REQUERIDO
  "priceListId": null,           // opcional
  "discountPercentage": 0,       // descuento global 0-100
  "requiresInvoice": false,
  "validUntil": "2026-04-30T23:59:59Z",  // fecha de vencimiento, opcional
  "notes": "Cotización válida por 2 semanas",
  "details": [
    {
      "productId": 12,
      "quantity": 3,
      "unitPrice": 150.00,
      "discountPercentage": 0,
      "notes": null
    }
  ]
}
```

**Respuesta exitosa (200):**
```json
{
  "message": "Cotización creada exitosamente",
  "error": 0,
  "data": {
    "id": 7,
    "code": "COT-000007",
    "quotationDate": "2026-04-17T19:00:00Z",
    "validUntil": "2026-04-30T23:59:59Z",
    "isExpired": false,
    "status": "Draft",
    "customerId": 5,
    "customerName": "Juan Pérez",
    "warehouseId": 1,
    "warehouseName": "Almacén Principal",
    "userId": 3,
    "userName": "vendedor01",
    "subTotal": 466.38,
    "discountAmount": 0,
    "discountPercentage": 0,
    "taxAmount": 72.00,
    "total": 522.00,
    "requiresInvoice": false,
    "saleId": null,
    "saleCode": null,
    "convertedAt": null,
    "notes": "Cotización válida por 2 semanas",
    "createdAt": "2026-04-17T19:00:00Z",
    "totalItems": 1,
    "totalQuantity": 3,
    "details": [
      {
        "id": 10,
        "productId": 12,
        "productCode": "PRD-000012",
        "productName": "Producto A",
        "quantity": 3,
        "unitPrice": 150.00,
        "discountPercentage": 0,
        "discountAmount": 0,
        "taxPercentage": 0.16,
        "taxAmount": 72.00,
        "subTotal": 450.00,
        "total": 522.00,
        "notes": null
      }
    ]
  }
}
```

> **Generar QR:** El código `COT-000007` (campo `data.code`) es el valor que debes codificar en el QR.

---

### 2.2 Listar cotizaciones

```
GET /api/quotations?page=1&pageSize=20&status=Draft
Authorization: Bearer {token}
```

**Query params:**
| Param | Tipo | Descripción |
|-------|------|-------------|
| `page` | int | Página (default: 1) |
| `pageSize` | int | Por página (default: 20) |
| `warehouseId` | int | Filtrar por almacén |
| `customerId` | int | Filtrar por cliente |
| `userId` | int | Filtrar por vendedor |
| `status` | string | `Draft`, `Converted`, `Cancelled`, etc. |
| `fromDate` | datetime | Fecha inicio |
| `toDate` | datetime | Fecha fin |

**Respuesta (200):**
```json
{
  "error": 0,
  "data": {
    "items": [
      {
        "id": 7,
        "code": "COT-000007",
        "quotationDate": "2026-04-17T19:00:00Z",
        "validUntil": "2026-04-30T23:59:59Z",
        "customerName": "Juan Pérez",
        "warehouseName": "Almacén Principal",
        "total": 522.00,
        "status": "Draft",
        "requiresInvoice": false,
        "totalItems": 1,
        "userName": "vendedor01",
        "saleId": null,
        "saleCode": null
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

---

### 2.3 Ver cotización por ID

```
GET /api/quotations/{id}
Authorization: Bearer {token}
```

Devuelve el objeto `QuotationResponseDto` completo (misma estructura que la respuesta de creación).

---

### 2.4 Buscar cotización por código (resultado de escanear QR)

```
GET /api/quotations/by-code/{code}
Authorization: Bearer {token}
```

Ejemplo: `GET /api/quotations/by-code/COT-000007`

Devuelve el mismo `QuotationResponseDto` completo.  
Usar este endpoint **inmediatamente después de escanear el QR** para mostrar el resumen antes de confirmar la conversión.

**Error si no existe:**
```json
{
  "message": "Cotización con código 'COT-000007' no encontrada",
  "error": 1
}
```

---

### 2.5 Cancelar cotización

```
DELETE /api/quotations/{id}?reason=El cliente ya no lo necesita
Authorization: Bearer {token}
```

**Respuesta (200):**
```json
{
  "message": "Cotización cancelada",
  "error": 0,
  "data": { ... }  // QuotationResponseDto con status "Cancelled"
}
```

---

## 3. FLUJO QR: COTIZACIÓN → VENTA {#flujo-qr}

### Paso a paso

```
1. Vendedor crea cotización  →  POST /api/quotations
2. App muestra QR con el código (ej: "COT-000007")
3. Cliente / cajero escanea el QR
4. App consulta la cotización  →  GET /api/quotations/by-code/COT-000007
5. App muestra resumen (productos, total, cliente)
6. Usuario elige tipo de venta: POS o Delivery
7. App convierte en venta  →  POST /api/quotations/{id}/convert
8. Se crea una Sale en estado "Draft"
9. Si es POS:  procesar pago inmediatamente  →  POST /api/sales/{saleId}/payments
   Si es Delivery: la venta queda pendiente hasta confirmar entrega  →  PUT /api/sales/{saleId}/deliver
```

---

### 3.1 Convertir cotización en venta

```
POST /api/quotations/{id}/convert
Authorization: Bearer {token}
```

**Request body para venta POS (pago en caja):**
```json
{
  "saleType": "POS",
  "deliveryAddress": null,
  "scheduledDeliveryDate": null,
  "notes": null
}
```

**Request body para venta Delivery:**
```json
{
  "saleType": "Delivery",
  "deliveryAddress": "Av. Insurgentes 400, CDMX",
  "scheduledDeliveryDate": "2026-04-21T10:00:00Z",
  "notes": "Entregar en recepción"
}
```

**Respuesta exitosa (200):**
```json
{
  "message": "Cotización convertida en venta exitosamente",
  "error": 0,
  "data": {
    "quotation": {
      "id": 7,
      "code": "COT-000007",
      "status": "Converted",
      "convertedAt": "2026-04-17T20:00:00Z",
      "saleId": 43,
      "saleCode": "VTA-000043"
    },
    "saleId": 43,
    "saleCode": "VTA-000043"
  }
}
```

> Después de esta llamada, navegar al flujo de pago (`POST /api/sales/43/payments`) si es POS,  
> o al módulo de entregas pendientes si es Delivery.

**Errores posibles:**
| HTTP | mensaje |
|------|---------|
| 404 | "Cotización con ID X no encontrada" |
| 400 | "Esta cotización ya fue convertida en la venta VTA-000043" |
| 400 | "No se puede convertir una cotización cancelada" |
| 400 | "Esta cotización ha vencido y no puede convertirse en venta" |

---

### 3.2 Procesar pago de la venta generada (si saleType = POS)

Después de convertir, procesar el pago:

```
POST /api/sales/{saleId}/payments
Authorization: Bearer {token}
```

**Request body:**
```json
{
  "payments": [
    {
      "paymentMethod": "Efectivo",
      "amount": 530.00
    }
  ]
}
```

---

## 4. CATÁLOGO DE FORMAS DE PAGO {#formas-de-pago}

Usar exactamente estos valores en el campo `paymentMethod`:

| Valor | Descripción |
|-------|-------------|
| `Efectivo` | Pago en efectivo |
| `Tarjeta de Crédito` | Tarjeta de crédito |
| `Tarjeta de Débito` | Tarjeta de débito |
| `Transferencia` | Transferencia bancaria |
| `Cheque` | Cheque bancario |
| `Vales` | Vales de despensa / regalo |

**Campos adicionales según forma de pago:**

Para `Tarjeta de Crédito` / `Tarjeta de Débito`:
```json
{
  "paymentMethod": "Tarjeta de Crédito",
  "amount": 500.00,
  "cardNumber": "4321",          // últimos 4 dígitos
  "cardType": "Visa",
  "authorizationCode": "ABC123",
  "terminalId": "POS-001",
  "bankName": "BBVA"
}
```

Para `Transferencia`:
```json
{
  "paymentMethod": "Transferencia",
  "amount": 500.00,
  "transferReference": "REF-2026041700123"
}
```

---

## 5. ESTADOS Y CÓDIGOS DE ERROR {#estados}

### Estructura general de respuestas

**Éxito:**
```json
{
  "message": "...",
  "error": 0,
  "data": { ... }
}
```

**Error:**
```json
{
  "message": "Descripción del error",
  "error": 1
}
```

### Códigos HTTP

| Código | Significado |
|--------|-------------|
| 200 | OK |
| 400 | Bad Request — datos inválidos o regla de negocio |
| 401 | Unauthorized — token expirado o ausente |
| 403 | Forbidden — sin permiso para la operación |
| 404 | Not Found — recurso no encontrado |
| 500 | Server Error — error interno |

### Permisos requeridos por endpoint

| Endpoint | Módulo | Permiso |
|----------|--------|---------|
| `POST /sales/delivery` | Ventas | Create |
| `PUT /sales/{id}/deliver` | Ventas | Edit |
| `GET /sales` | Ventas | View |
| `POST /quotations` | Cotizaciones | Create |
| `GET /quotations` | Cotizaciones | View |
| `GET /quotations/{id}` | Cotizaciones | View |
| `GET /quotations/by-code/{code}` | Cotizaciones | View |
| `POST /quotations/{id}/convert` | Ventas | Create |
| `DELETE /quotations/{id}` | Cotizaciones | Delete |

---

## PANTALLAS SUGERIDAS

### Módulo Ventas Delivery

```
[Lista de Deliveries Pendientes]
  → Filtros: fecha, almacén, estado
  → Card por pedido: cliente, dirección, total, fecha estimada
  → Botón "Confirmar Entrega" → abre formulario de pago
  → Formulario de pago → formas de pago + dirección real entrega
  → Resultado: ticket / comprobante

[Nueva Venta Delivery]
  → Seleccionar cliente (opcional)
  → Seleccionar almacén
  → Buscador de productos + cantidad + precio
  → Dirección de entrega (opcional)
  → Fecha estimada (optional)
  → Notas
  → Botón "Crear Pedido"
```

### Módulo Cotizaciones

```
[Lista de Cotizaciones]
  → Filtros: estado, fecha, cliente
  → Card por cotización: código, cliente, total, vencimiento, estado
  → Botón "Ver QR" → muestra QR con el código COT-XXXXXX
  → Botón "Cancelar"

[Nueva Cotización]
  → Mismo formulario que venta pero sin pagos
  → Campo "Válida hasta" (fecha de vencimiento)
  → Al crear → mostrar QR automáticamente

[Escanear QR / Convertir a Venta]
  → Abrir cámara para escanear QR
  → Leer código COT-XXXXXX
  → Llamar GET /quotations/by-code/{code}
  → Mostrar resumen: cliente, productos, total
  → Botón "Venta POS" → convierte y va al flujo de pago
  → Botón "Venta Delivery" → pide dirección + fecha → convierte
```
