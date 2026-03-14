# ?? Billing: Endpoint de Ventas Pendientes de Timbrar

## ? Implementación Completa

Se ha creado un endpoint completo en el módulo de **Billing** para obtener las ventas pendientes de timbrar (facturar) con filtros avanzados incluyendo el filtro por `RequiresInvoice`.

---

## ?? Funcionalidad

### Criterios de "Venta Pendiente de Timbrar"

Una venta se considera **pendiente de timbrar** cuando cumple:
1. ? `Status = "Completed"` - Venta completada
2. ? `IsPaid = true` - Venta pagada
3. ? `InvoiceUuid IS NULL` - No tiene UUID (no ha sido timbrada)

Adicionalmente, se puede filtrar por:
- ? `RequiresInvoice = true` - Solo ventas que requieren factura
- ? `RequiresInvoice = false` - Solo ventas que NO requieren factura
- ? Sin filtro - Todas las ventas pendientes (independiente de RequiresInvoice)

---

## ?? Endpoint

### **GET** `/api/billing/pending-sales`

Obtiene ventas completadas y pagadas que aún no han sido timbradas.

**Requiere permiso:** `Billing.ViewPending`

### Query Parameters

| Parámetro | Tipo | Requerido | Default | Descripción |
|-----------|------|-----------|---------|-------------|
| `page` | int | No | 1 | Número de página |
| `pageSize` | int | No | 20 | Cantidad de registros por página |
| `onlyRequiresInvoice` | bool? | No | null | **true**: Solo ventas que requieren factura<br>**false**: Solo ventas que NO requieren factura<br>**null**: Todas las ventas pendientes |
| `warehouseId` | int? | No | null | Filtrar por almacén |
| `branchId` | int? | No | null | Filtrar por sucursal |
| `companyId` | int? | No | null | Filtrar por empresa |
| `fromDate` | DateTime? | No | null | Fecha desde (SaleDate >= fromDate) |
| `toDate` | DateTime? | No | null | Fecha hasta (SaleDate <= toDate) |

---

## ?? Ejemplos de Uso

### 1. Todas las ventas pendientes de timbrar

```http
GET /api/billing/pending-sales?page=1&pageSize=20
Authorization: Bearer {token}
```

**Resultado:** Todas las ventas completadas, pagadas y sin timbrar (sin importar RequiresInvoice)

---

### 2. Solo ventas que REQUIEREN factura

```http
GET /api/billing/pending-sales?page=1&pageSize=20&onlyRequiresInvoice=true
Authorization: Bearer {token}
```

**Resultado:** Solo ventas con `RequiresInvoice = true`

---

### 3. Solo ventas que NO requieren factura

```http
GET /api/billing/pending-sales?page=1&pageSize=20&onlyRequiresInvoice=false
Authorization: Bearer {token}
```

**Resultado:** Solo ventas con `RequiresInvoice = false`

---

### 4. Ventas por empresa

```http
GET /api/billing/pending-sales?companyId=1&onlyRequiresInvoice=true
Authorization: Bearer {token}
```

**Resultado:** Ventas pendientes de la empresa con ID 1 que requieren factura

---

### 5. Ventas por rango de fechas

```http
GET /api/billing/pending-sales?fromDate=2026-03-01&toDate=2026-03-31&onlyRequiresInvoice=true
Authorization: Bearer {token}
```

**Resultado:** Ventas pendientes de marzo 2026 que requieren factura

---

### 6. Filtros combinados

```http
GET /api/billing/pending-sales?companyId=1&branchId=2&warehouseId=5&onlyRequiresInvoice=true&fromDate=2026-03-01
Authorization: Bearer {token}
```

**Resultado:** Ventas pendientes de empresa 1, sucursal 2, almacén 5, que requieren factura, desde marzo 2026

---

## ?? Response

### Estructura del Response

```json
{
  "message": "Ventas pendientes de timbrar obtenidas exitosamente",
  "error": 0,
  "data": [
    {
      "id": 123,
      "code": "VTA-000123",
      "saleDate": "2026-03-10T14:30:00Z",
      "customerId": 45,
      "customerName": "Juan Pérez García",
      "customerRfc": "PEGJ850101ABC",
      "customerEmail": "juan.perez@email.com",
      "warehouseId": 5,
      "warehouseName": "Almacén Principal",
      "branchId": 2,
      "branchName": "Sucursal Centro",
      "companyId": 1,
      "companyName": "Mi Empresa SA de CV",
      "subTotal": 8500.00,
      "taxAmount": 1360.00,
      "total": 9860.00,
      "requiresInvoice": true,
      "isPaid": true,
      "status": "Completed",
      "createdAt": "2026-03-10T14:35:00Z",
      "daysPending": 3,
      "notes": "Cliente requiere factura urgente"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalRecords": 45,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true,
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

### Campos del Response

#### **Data (Ventas)**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | int | ID de la venta |
| `code` | string | Código de la venta (VTA-000123) |
| `saleDate` | DateTime | Fecha de la venta |
| `customerId` | int? | ID del cliente |
| `customerName` | string? | Nombre completo del cliente |
| `customerRfc` | string? | RFC del cliente |
| `customerEmail` | string? | Email del cliente |
| `warehouseId` | int | ID del almacén |
| `warehouseName` | string? | Nombre del almacén |
| `branchId` | int? | ID de la sucursal |
| `branchName` | string? | Nombre de la sucursal |
| `companyId` | int? | ID de la empresa |
| `companyName` | string? | Razón social de la empresa |
| `subTotal` | decimal | Subtotal de la venta |
| `taxAmount` | decimal | Impuestos (IVA) |
| `total` | decimal | Total de la venta |
| `requiresInvoice` | bool | Si requiere factura |
| `isPaid` | bool | Si está pagada (siempre true) |
| `status` | string | Estado (siempre "Completed") |
| `createdAt` | DateTime | Fecha de creación |
| `daysPending` | int | Días desde la venta |
| `notes` | string? | Notas de la venta |

#### **Summary (Resumen)**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `totalSales` | int | Total de ventas pendientes |
| `salesRequiresInvoice` | int | Ventas que requieren factura |
| `salesNotRequiresInvoice` | int | Ventas que NO requieren factura |
| `totalAmount` | decimal | Monto total pendiente |
| `averageAmount` | decimal | Monto promedio por venta |
| `averageDaysPending` | int | Días promedio pendientes |

---

## ??? Arquitectura Implementada

### Archivos Creados/Modificados

#### 1. **Query** (CQRS Pattern)
**Archivo:** `Application/Core/Billing/Queries/BillingQueries.cs`

```csharp
public record GetPendingInvoiceSalesQuery(
    int Page,
    int PageSize,
    bool? OnlyRequiresInvoice = null,
    int? WarehouseId = null,
    int? BranchId = null,
    int? CompanyId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<PendingInvoiceSalesResponseDto>;
```

#### 2. **DTOs**
**Archivo:** `Application/DTOs/Billing/BillingDtos.cs`

- `PendingInvoiceSalesResponseDto` - Response paginado
- `PendingInvoiceSaleDto` - Datos de cada venta
- `PendingInvoiceSummaryDto` - Resumen estadístico

#### 3. **Query Handler**
**Archivo:** `Application/Core/Billing/QueryHandlers/BillingQueryHandlers.cs`

```csharp
public class GetPendingInvoiceSalesQueryHandler : 
    IRequestHandler<GetPendingInvoiceSalesQuery, PendingInvoiceSalesResponseDto>
{
    private readonly ISaleRepository _saleRepository;
    
    public async Task<PendingInvoiceSalesResponseDto> Handle(
        GetPendingInvoiceSalesQuery request, 
        CancellationToken cancellationToken)
    {
        // Lógica de negocio...
    }
}
```

#### 4. **Repository Interface**
**Archivo:** `Application/Abstractions/Sales/ISaleRepository.cs`

```csharp
Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPendingInvoiceSalesAsync(
    int page,
    int pageSize,
    bool? onlyRequiresInvoice = null,
    int? warehouseId = null,
    int? branchId = null,
    int? companyId = null,
    DateTime? fromDate = null,
    DateTime? toDate = null
);
```

#### 5. **Repository Implementation**
**Archivo:** `Infrastructure/Repositories/SaleRepository.cs`

```csharp
public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPendingInvoiceSalesAsync(
    int page, int pageSize, bool? onlyRequiresInvoice, ...)
{
    var query = _context.SalesNew
        .Include(s => s.Customer)
        .Include(s => s.Warehouse)
        .Include(s => s.Branch)
        .Include(s => s.Company)
        .AsQueryable();

    // Filtro principal
    query = query.Where(s => 
        s.Status == "Completed" && 
        s.IsPaid == true && 
        string.IsNullOrEmpty(s.InvoiceUuid));

    // Filtros adicionales...
    
    return (sales, totalCount);
}
```

#### 6. **Controller**
**Archivo:** `Web.Api/Controllers/Billing/BillingController.cs`

```csharp
[HttpGet("pending-sales")]
[RequirePermission("Billing", "ViewPending")]
public async Task<IActionResult> GetPendingInvoiceSales(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] bool? onlyRequiresInvoice = null,
    [FromQuery] int? warehouseId = null,
    [FromQuery] int? branchId = null,
    [FromQuery] int? companyId = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null)
{
    var query = new GetPendingInvoiceSalesQuery(
        page, pageSize, onlyRequiresInvoice, 
        warehouseId, branchId, companyId, 
        fromDate, toDate);

    var result = await _mediator.Send(query);
    return Ok(result);
}
```

---

## ?? Query SQL Generado

El endpoint genera un query optimizado similar a:

```sql
SELECT 
    s.Id,
    s.Code,
    s.SaleDate,
    s.CustomerId,
    s.CustomerName,
    c.TaxId as CustomerRfc,
    c.Email as CustomerEmail,
    s.WarehouseId,
    w.Name as WarehouseName,
    s.BranchId,
    b.Name as BranchName,
    s.CompanyId,
    comp.LegalName as CompanyName,
    s.SubTotal,
    s.TaxAmount,
    s.Total,
    s.RequiresInvoice,
    s.IsPaid,
    s.Status,
    s.CreatedAt,
    s.Notes
FROM Sales s
LEFT JOIN Customers c ON c.ID = s.CustomerId
LEFT JOIN Warehouses w ON w.Id = s.WarehouseId
LEFT JOIN Branches b ON b.Id = s.BranchId
LEFT JOIN Companies comp ON comp.Id = s.CompanyId
WHERE 
    s.Status = 'Completed'
    AND s.IsPaid = 1
    AND s.InvoiceUuid IS NULL
    AND (@OnlyRequiresInvoice IS NULL OR s.RequiresInvoice = @OnlyRequiresInvoice)
    AND (@CompanyId IS NULL OR s.CompanyId = @CompanyId)
    AND (@BranchId IS NULL OR s.BranchId = @BranchId)
    AND (@WarehouseId IS NULL OR s.WarehouseId = @WarehouseId)
    AND (@FromDate IS NULL OR s.SaleDate >= @FromDate)
    AND (@ToDate IS NULL OR s.SaleDate <= @ToDate)
ORDER BY s.SaleDate DESC
OFFSET (@Page - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;
```

---

## ?? Casos de Uso

### Caso 1: Facturación Masiva
```http
GET /api/billing/pending-sales?onlyRequiresInvoice=true&pageSize=100
```
Obtiene las primeras 100 ventas que requieren factura para procesamiento masivo.

### Caso 2: Seguimiento por Sucursal
```http
GET /api/billing/pending-sales?branchId=2&onlyRequiresInvoice=true
```
Monitorea las ventas pendientes de facturar en una sucursal específica.

### Caso 3: Dashboard de Facturación
```http
GET /api/billing/pending-sales?page=1&pageSize=10
```
Muestra las 10 ventas más recientes pendientes (con resumen estadístico).

### Caso 4: Reportes por Empresa
```http
GET /api/billing/pending-sales?companyId=1&fromDate=2026-03-01&toDate=2026-03-31
```
Genera reporte mensual de ventas pendientes por empresa.

### Caso 5: Ventas sin Factura
```http
GET /api/billing/pending-sales?onlyRequiresInvoice=false
```
Lista ventas que NO requieren factura (para otros procesos de cierre).

---

## ?? Beneficios

### 1. **Flexibilidad Total**
- ? Filtrar por `RequiresInvoice` (true, false o null)
- ? Filtros por empresa, sucursal, almacén
- ? Filtros por rango de fechas
- ? Paginación configurable

### 2. **Performance Optimizado**
- ? Índices en BranchId y CompanyId
- ? Query con JOINs eficientes
- ? AsNoTracking para lectura rápida
- ? Paginación a nivel de base de datos

### 3. **Información Completa**
- ? Datos del cliente (incluyendo RFC y Email)
- ? Ubicación completa (Warehouse ? Branch ? Company)
- ? Cálculo automático de días pendientes
- ? Resumen estadístico incluido

### 4. **CQRS + MediatR**
- ? Separación de responsabilidades
- ? Código testeable
- ? Fácil de mantener y extender

---

## ? Checklist de Implementación

- [x] ? Query creado (CQRS)
- [x] ? QueryHandler implementado
- [x] ? DTOs definidos
- [x] ? Método en ISaleRepository
- [x] ? Método en SaleRepository
- [x] ? Endpoint en BillingController
- [x] ? Permisos configurados (Billing.ViewPending)
- [x] ? Build exitoso
- [x] ? Documentación completa

---

## ?? Seguridad

**Permiso requerido:** `Billing.ViewPending`

Para acceder al endpoint, el usuario debe tener el permiso `ViewPending` en el módulo `Billing`.

---

## ?? Ejemplo Completo en Postman

### Request
```
GET https://api.tuempresa.com/api/billing/pending-sales?page=1&pageSize=20&onlyRequiresInvoice=true&companyId=1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response
```json
{
  "message": "Ventas pendientes de timbrar obtenidas exitosamente",
  "error": 0,
  "data": [
    {
      "id": 250,
      "code": "VTA-000250",
      "saleDate": "2026-03-13T10:30:00Z",
      "customerId": 78,
      "customerName": "María González Rodríguez",
      "customerRfc": "GORM920515XYZ",
      "customerEmail": "maria.gonzalez@email.com",
      "warehouseId": 3,
      "warehouseName": "Almacén Norte",
      "branchId": 2,
      "branchName": "Sucursal Centro",
      "companyId": 1,
      "companyName": "Mi Empresa SA de CV",
      "subTotal": 12500.00,
      "taxAmount": 2000.00,
      "total": 14500.00,
      "requiresInvoice": true,
      "isPaid": true,
      "status": "Completed",
      "createdAt": "2026-03-13T10:35:00Z",
      "daysPending": 0,
      "notes": "Cliente solicita factura a nombre de la empresa"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalRecords": 38,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "summary": {
    "totalSales": 38,
    "salesRequiresInvoice": 38,
    "salesNotRequiresInvoice": 0,
    "totalAmount": 456750.00,
    "averageAmount": 12019.74,
    "averageDaysPending": 2
  }
}
```

---

## ?? Resultado Final

? **Endpoint completamente funcional**  
? **Filtro por RequiresInvoice implementado**  
? **Filtros avanzados (empresa, sucursal, almacén, fechas)**  
? **Paginación incluida**  
? **Resumen estadístico**  
? **Arquitectura CQRS + MediatR**  
? **Build exitoso**  

**Fecha de implementación:** 2026-03-13  
**Estado:** ? **PRODUCCIÓN**
