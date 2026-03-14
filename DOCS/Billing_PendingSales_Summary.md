# ? RESUMEN: Endpoint de Ventas Pendientes de Timbrar

## ?? Implementación Completa

Se ha creado un **endpoint completo** en el módulo de Billing para obtener ventas pendientes de timbrar con filtro por `RequiresInvoice` y filtros avanzados.

---

## ?? Endpoint Creado

### **GET** `/api/billing/pending-sales`

**Descripción:** Obtiene ventas completadas, pagadas y sin timbrar

**Permiso:** `Billing.ViewPending`

---

## ??? Filtros Disponibles

| Parámetro | Tipo | Función |
|-----------|------|---------|
| `onlyRequiresInvoice` | bool? | **true**: Solo ventas que requieren factura<br>**false**: Solo ventas que NO requieren<br>**null**: Todas |
| `companyId` | int? | Filtrar por empresa |
| `branchId` | int? | Filtrar por sucursal |
| `warehouseId` | int? | Filtrar por almacén |
| `fromDate` | DateTime? | Fecha desde |
| `toDate` | DateTime? | Fecha hasta |
| `page` | int | Número de página (default: 1) |
| `pageSize` | int | Tamaño de página (default: 20) |

---

## ?? Ejemplos de Uso

### Todas las ventas pendientes
```http
GET /api/billing/pending-sales
```

### Solo ventas que requieren factura
```http
GET /api/billing/pending-sales?onlyRequiresInvoice=true
```

### Solo ventas que NO requieren factura
```http
GET /api/billing/pending-sales?onlyRequiresInvoice=false
```

### Filtros combinados
```http
GET /api/billing/pending-sales?companyId=1&branchId=2&onlyRequiresInvoice=true&fromDate=2026-03-01
```

---

## ?? Response Ejemplo

```json
{
  "message": "Ventas pendientes de timbrar obtenidas exitosamente",
  "error": 0,
  "data": [
    {
      "id": 123,
      "code": "VTA-000123",
      "saleDate": "2026-03-10T14:30:00Z",
      "customerName": "Juan Pérez",
      "customerRfc": "PEGJ850101ABC",
      "customerEmail": "juan@email.com",
      "warehouseName": "Almacén Principal",
      "branchName": "Sucursal Centro",
      "companyName": "Mi Empresa SA de CV",
      "total": 9860.00,
      "requiresInvoice": true,
      "daysPending": 3
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalRecords": 45,
  "totalPages": 3,
  "summary": {
    "totalSales": 45,
    "salesRequiresInvoice": 38,
    "salesNotRequiresInvoice": 7,
    "totalAmount": 125750.00,
    "averageAmount": 2794.44,
    "averageDaysPending": 5
  }
}
```

---

## ??? Arquitectura

### Archivos Creados

| Archivo | Propósito |
|---------|-----------|
| `Application/Core/Billing/Queries/BillingQueries.cs` | Query CQRS |
| `Application/Core/Billing/QueryHandlers/BillingQueryHandlers.cs` | Handler MediatR |
| `Application/DTOs/Billing/BillingDtos.cs` | DTOs de response |

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `Application/Abstractions/Sales/ISaleRepository.cs` | Agregado `GetPendingInvoiceSalesAsync` |
| `Infrastructure/Repositories/SaleRepository.cs` | Implementación del método |
| `Web.Api/Controllers/Billing/BillingController.cs` | Nuevo endpoint |

---

## ? Características

### Filtros Inteligentes
- ? `onlyRequiresInvoice` con 3 estados (true/false/null)
- ? Filtros por empresa, sucursal, almacén
- ? Filtros por rango de fechas
- ? Paginación completa

### Datos Completos
- ? Información del cliente (nombre, RFC, email)
- ? Ubicación completa (warehouse ? branch ? company)
- ? Montos (subtotal, tax, total)
- ? Días pendientes calculados automáticamente

### Resumen Estadístico
- ? Total de ventas pendientes
- ? Cantidad con/sin RequiresInvoice
- ? Monto total pendiente
- ? Promedio de monto y días

---

## ?? Criterios de "Pendiente de Timbrar"

Una venta se considera pendiente cuando:
```
Status = "Completed"
AND IsPaid = true
AND InvoiceUuid IS NULL
```

---

## ?? Seguridad

**Permiso requerido:** `Billing.ViewPending`

---

## ?? Performance

- ? Índices en BranchId y CompanyId
- ? Query optimizado con JOINs eficientes
- ? AsNoTracking para lectura rápida
- ? Paginación a nivel de BD

---

## ?? Casos de Uso

### 1. Dashboard de Facturación
```http
GET /api/billing/pending-sales?page=1&pageSize=10
```
Muestra las últimas 10 ventas pendientes con resumen.

### 2. Facturación Masiva
```http
GET /api/billing/pending-sales?onlyRequiresInvoice=true&pageSize=100
```
Obtiene 100 ventas que requieren factura.

### 3. Seguimiento por Sucursal
```http
GET /api/billing/pending-sales?branchId=2&onlyRequiresInvoice=true
```
Ventas pendientes de una sucursal específica.

### 4. Reporte Mensual
```http
GET /api/billing/pending-sales?companyId=1&fromDate=2026-03-01&toDate=2026-03-31
```
Todas las ventas pendientes del mes por empresa.

---

## ? Checklist

- [x] ? Query CQRS creado
- [x] ? QueryHandler implementado
- [x] ? DTOs definidos
- [x] ? Repository interface actualizado
- [x] ? Repository implementado
- [x] ? Controller endpoint creado
- [x] ? Permisos configurados
- [x] ? Build exitoso
- [x] ? Documentación completa

---

## ?? Estado Final

```
???????????????????????????????????????
?   BILLING: PENDING SALES            ?
?   Estado: ? PRODUCCIÓN             ?
???????????????????????????????????????
?  ? Endpoint: /api/billing/pending-sales ?
?  ? Filtro RequiresInvoice: Sí      ?
?  ? Filtros avanzados: Sí           ?
?  ? Paginación: Sí                  ?
?  ? Resumen: Sí                     ?
?  ? CQRS + MediatR: Sí              ?
?  ? Build: EXITOSO                  ?
???????????????????????????????????????
```

**Fecha:** 2026-03-13  
**Estado:** ? **COMPLETADO**

---

## ?? Documentación

**Documentación completa:** `DOCS/Billing_PendingSales_Endpoint.md`

---

? **¡Endpoint listo para usar!**
