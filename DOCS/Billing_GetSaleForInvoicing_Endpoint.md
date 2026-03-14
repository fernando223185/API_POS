# ?? Endpoint para Obtener Venta Individual para Facturación

## ? Implementación Completa

Se ha creado un endpoint completo para obtener toda la información de una venta específica necesaria para el proceso de facturación CFDI.

---

## ?? Nuevo Endpoint

### **GET /api/billing/sale/{saleId}**

Obtiene una venta individual con toda la información necesaria para facturar:
- ? Información de la empresa emisora (RFC, régimen fiscal, certificados)
- ? Información del cliente receptor (RFC, régimen fiscal, uso de CFDI)
- ? Detalles de productos (con claves SAT)
- ? Formas de pago utilizadas
- ? Sucursal y almacén
- ? Montos y descuentos

---

## ?? Request

```http
GET /api/billing/sale/{saleId}
Authorization: Bearer {token}
```

### Parámetros

| Parámetro | Tipo | Ubicación | Requerido | Descripción |
|-----------|------|-----------|-----------|-------------|
| saleId | int | path | Sí | ID de la venta a obtener |

---

## ?? Response

### Estructura del Response

```json
{
  "message": "Venta obtenida exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "VTA-000001",
    "saleDate": "2026-03-13T10:30:00Z",
    
    // ?? Empresa Emisora (Completa)
    "company": {
      "id": 1,
      "legalName": "XOCHILT CASAS CHAVEZ",
      "rfc": "CACX7605101P8",
      "fiscalRegime": "612",
      "postalCode": "06000",
      "email": "elbuena22@gmail.com",
      "serie": "XO",
      "nextFolio": 1
    },
    
    // ?? Sucursal
    "branch": {
      "id": 1,
      "name": "Sucursal Centro",
      "address": "Calle Principal #123",
      "phone": "555-1234"
    },
    
    // ?? Cliente Receptor (Completo)
    "customer": {
      "id": 10,
      "name": "Juan Pérez",
      "rfc": "XAXX010101000",
      "fiscalRegime": "612",
      "postalCode": "00000",
      "email": "cliente@example.com",
      "address": "Calle Cliente #456",
      "cfdiUse": "G03"
    },
    
    // ?? Montos
    "subTotal": 151.00,
    "discountAmount": 0.00,
    "discountPercentage": 0.0000,
    "taxAmount": 24.16,
    "total": 175.16,
    
    // ?? Estado
    "isPaid": true,
    "requiresInvoice": true,
    "status": "Completed",
    "invoiceUuid": null,
    
    // ?? Detalles de Productos
    "details": [
      {
        "id": 1,
        "productId": 123,
        "productCode": "PROD-001",
        "productName": "Producto Ejemplo",
        "quantity": 2.0,
        "unitPrice": 75.50,
        "discountAmount": 0.00,
        "taxAmount": 12.08,
        "subTotal": 75.50,
        "total": 87.58,
        "satProductKey": "01010101",
        "satUnitKey": "H87",
        "notes": null
      }
    ],
    
    // ?? Formas de Pago
    "payments": [
      {
        "id": 1,
        "paymentMethod": "Cash",
        "amount": 175.16,
        "paymentDate": "2026-03-13T10:35:00Z",
        "cardNumber": null,
        "transactionReference": null,
        "bankName": null
      }
    ],
    
    "notes": null,
    "createdAt": "2026-03-13T10:30:00Z"
  }
}
```

---

## ? Validaciones

El endpoint valida automáticamente:

1. **Venta existe**: Error 404 si no se encuentra
2. **Estado Completada**: Error 400 si no está completada
3. **Está pagada**: Error 400 si no está pagada
4. **Tiene empresa asignada**: Error 400 si no tiene CompanyId

### Respuestas de Error

#### 404 - Venta no encontrada
```json
{
  "message": "Venta con ID 999 no encontrada",
  "error": 1
}
```

#### 400 - Venta no completada
```json
{
  "message": "La venta debe estar completada para facturar. Estado actual: Draft",
  "error": 1
}
```

#### 400 - Venta no pagada
```json
{
  "message": "La venta debe estar pagada para facturar",
  "error": 1
}
```

#### 400 - Sin empresa asignada
```json
{
  "message": "La venta debe tener una empresa emisora asignada",
  "error": 1
}
```

---

## ?? Permisos Requeridos

```csharp
[RequirePermission("CFDI", "Ventas")]
```

El usuario debe tener acceso al módulo **CFDI** y al submódulo **Ventas**.

---

## ?? Casos de Uso

### 1. Obtener venta para mostrar en formulario de facturación

```javascript
// Frontend: Obtener venta seleccionada
const saleId = 1; // VTA-000001

const response = await fetch(`/api/billing/sale/${saleId}`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const { data } = await response.json();

// Usar los datos en el formulario
setCompanyData(data.company);
setCustomerData(data.customer);
setProducts(data.details);
setPayments(data.payments);
```

### 2. Validar datos antes de timbrar

```javascript
// Verificar que tenga RFC válido
if (data.customer.rfc === 'XAXX010101000') {
  // Cliente es público en general
  alert('El cliente es público en general. Usa la empresa emisora como receptor.');
}

// Verificar que tenga uso de CFDI
if (!data.customer.cfdiUse) {
  // Usar G03 por defecto
  data.customer.cfdiUse = 'G03';
}
```

### 3. Generar XML del CFDI

```javascript
// Mapear a estructura de CFDI
const cfdi = {
  Emisor: {
    Rfc: data.company.rfc,
    Nombre: data.company.legalName,
    RegimenFiscal: data.company.fiscalRegime
  },
  Receptor: {
    Rfc: data.customer.rfc,
    Nombre: data.customer.name,
    UsoCFDI: data.customer.cfdiUse,
    DomicilioFiscalReceptor: data.customer.postalCode
  },
  Conceptos: data.details.map(d => ({
    ClaveProdServ: d.satProductKey,
    ClaveUnidad: d.satUnitKey,
    Descripcion: d.productName,
    Cantidad: d.quantity,
    ValorUnitario: d.unitPrice,
    Importe: d.subTotal,
    Descuento: d.discountAmount
  })),
  Total: data.total,
  SubTotal: data.subTotal
};

// Enviar al PAC para timbrado
```

---

## ??? Arquitectura de la Implementación

### 1. **Query** (CQRS)
```csharp
// Application/Core/Billing/Queries/BillingQueries.cs
public record GetSaleForInvoicingQuery(int SaleId) 
    : IRequest<SaleForInvoicingDto>;
```

### 2. **Handler**
```csharp
// Application/Core/Billing/QueryHandlers/BillingQueryHandlers.cs
public class GetSaleForInvoicingQueryHandler 
    : IRequestHandler<GetSaleForInvoicingQuery, SaleForInvoicingDto>
{
    private readonly ISaleRepository _saleRepository;
    
    public async Task<SaleForInvoicingDto> Handle(
        GetSaleForInvoicingQuery request,
        CancellationToken cancellationToken)
    {
        // Obtener venta con todas las relaciones
        var sale = await _saleRepository.GetSaleForInvoicingAsync(request.SaleId);
        
        // Validaciones
        // Mapeo a DTO
        
        return result;
    }
}
```

### 3. **Repository**
```csharp
// Infrastructure/Repositories/SaleRepository.cs
public async Task<Sale?> GetSaleForInvoicingAsync(int saleId)
{
    return await _context.SalesNew
        .Include(s => s.Company)           // Empresa emisora
        .Include(s => s.Branch)            // Sucursal
        .Include(s => s.Customer)          // Cliente receptor
        .Include(s => s.Details)
            .ThenInclude(d => d.Product)   // Productos (claves SAT)
        .Include(s => s.Payments)          // Formas de pago
        .Include(s => s.Warehouse)
            .ThenInclude(w => w.Branch)
                .ThenInclude(b => b!.Company)
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Id == saleId);
}
```

### 4. **Controller**
```csharp
// Web.Api/Controllers/Billing/BillingController.cs
[HttpGet("sale/{saleId}")]
[RequirePermission("CFDI", "Ventas")]
public async Task<IActionResult> GetSaleForInvoicing(int saleId)
{
    var query = new GetSaleForInvoicingQuery(saleId);
    var result = await _mediator.Send(query);
    
    return Ok(new {
        message = "Venta obtenida exitosamente",
        error = 0,
        data = result
    });
}
```

---

## ?? DTOs Creados

### `SaleForInvoicingDto`
DTO principal con toda la información de la venta

### `CompanyForInvoicingDto`
Información fiscal de la empresa emisora:
- LegalName, TaxId (RFC), SatTaxRegime
- FiscalZipCode, Email
- InvoiceSeries, InvoiceCurrentFolio

### `CustomerForInvoicingDto`
Información fiscal del cliente receptor:
- Name, TaxId (RFC), SatTaxRegime
- ZipCode, Email, Address
- SatCfdiUse

### `SaleDetailForInvoicingDto`
Detalles de productos:
- ProductCode, ProductName
- Quantity, UnitPrice, SubTotal, Total
- SatProductKey, SatUnitKey (claves SAT)

### `PaymentMethodForInvoicingDto`
Formas de pago utilizadas:
- PaymentMethod, Amount
- CardNumber, TransactionReference, BankName

---

## ?? Notas Importantes

### 1. **Mapeo de Propiedades Corregido**

Se corrigieron los nombres de las propiedades de las entidades:

| DTO Property | Entity Property | Descripción |
|--------------|-----------------|-------------|
| `Rfc` | `TaxId` | RFC de Company y Customer |
| `FiscalRegime` | `SatTaxRegime` | Régimen fiscal SAT |
| `PostalCode` | `FiscalZipCode` | Código postal fiscal (Company) |
| `PostalCode` | `ZipCode` | Código postal (Customer) |
| `CfdiUse` | `SatCfdiUse` | Uso del CFDI |
| `Serie` | `InvoiceSeries` | Serie de facturas |
| `NextFolio` | `InvoiceCurrentFolio` | Folio actual |

### 2. **Claves SAT Pendientes**

Las claves SAT de productos (`SatProductKey`, `SatUnitKey`) están hardcodeadas:
- `SatProductKey`: "01010101" (pendiente agregar a Product)
- `SatUnitKey`: "H87" (pendiente agregar a Product)

**TODO**: Agregar estos campos a la entidad `Product`

### 3. **Valores por Defecto**

Para ventas a público general:
- RFC: "XAXX010101000"
- Código Postal: "00000"
- Uso CFDI: "G03" (Gastos en general)
- Régimen Fiscal: "612" (Personas físicas con actividades empresariales)

---

## ?? Testing

### Probar en Postman

```bash
GET http://localhost:7254/api/billing/sale/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Ejemplo de cURL

```bash
curl -X GET "http://localhost:7254/api/billing/sale/1" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json"
```

---

## ? Checklist de Implementación

- [x] Query CQRS creado
- [x] Handler implementado con validaciones
- [x] DTOs completos para facturación
- [x] Método en repositorio con todas las relaciones
- [x] Endpoint en controller con manejo de errores
- [x] Permisos configurados
- [x] Validaciones de negocio
- [x] Compilación exitosa
- [x] Documentación completa

---

## ?? Integración con Frontend

El frontend puede usar esta información para:

1. **Mostrar datos de la empresa emisora** en el formulario de facturación
2. **Pre-llenar datos del cliente receptor**
3. **Listar productos a facturar** con sus claves SAT
4. **Mostrar formas de pago utilizadas**
5. **Validar datos antes de enviar al PAC**
6. **Generar XML del CFDI**

---

## ?? Flujo Completo de Facturación

```
1. Usuario selecciona venta pendiente
   ?
2. Frontend llama a GET /api/billing/sale/{id}
   ?
3. Backend valida y obtiene toda la información
   ?
4. Frontend muestra formulario pre-llenado
   ?
5. Usuario revisa/completa datos fiscales
   ?
6. Frontend genera XML del CFDI
   ?
7. Frontend envía XML al PAC para timbrado
   ?
8. Backend actualiza InvoiceUuid con UUID del SAT
```

---

## ?? Referencias

- **Endpoint de ventas pendientes**: `GET /api/billing/pending-sales`
- **Entidades relacionadas**: `Sale`, `Company`, `Customer`, `Product`
- **Documento relacionado**: `DOCS/Billing_PendingSales_Summary.md`

---

## ?? Próximos Pasos

1. ? Endpoint para timbrar factura (POST /api/billing/invoice/stamp)
2. ? Agregar claves SAT a entidad Product
3. ? Integración con PAC (Proveedor Autorizado de Certificación)
4. ? Generar PDF de factura
5. ? Enviar factura por email

---

**Fecha de creación**: 13 de Marzo de 2026  
**Autor**: Sistema ERP POS  
**Versión**: 1.0
