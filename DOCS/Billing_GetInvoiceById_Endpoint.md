# Endpoint: Obtener Factura por ID

## 📋 Resumen

Endpoint para obtener una factura específica con todos sus detalles, incluyendo información del comprobante, emisor, receptor, montos, conceptos CFDI y datos de timbrado.

---

## 🔗 Endpoint

```
GET /api/billing/invoices/{id}
```

### Parámetros de Ruta
- **id** (int, requerido): ID de la factura a consultar

### Autenticación
✅ Requiere autenticación  
🔒 Permiso requerido: `CFDI - View`

---

## 📤 Respuesta Exitosa

### Status Code: `200 OK`

```json
{
  "message": "Factura obtenida exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    
    // Referencia a venta origen
    "saleId": 123,
    "saleCode": "VTA-2024-0123",
    
    // Información del comprobante
    "serie": "A",
    "folio": "001",
    "invoiceDate": "2024-03-19T10:30:00Z",
    "formaPago": "03",
    "metodoPago": "PUE",
    "condicionesDePago": "Contado",
    "tipoDeComprobante": "I",
    "lugarExpedicion": "64000",
    
    // Emisor
    "companyId": 1,
    "emisorRfc": "AAA010101AAA",
    "emisorNombre": "MI EMPRESA SA DE CV",
    "emisorRegimenFiscal": "601",
    
    // Receptor
    "customerId": 45,
    "receptorRfc": "XAXX010101000",
    "receptorNombre": "PUBLICO EN GENERAL",
    "receptorDomicilioFiscal": "64000",
    "receptorRegimenFiscal": "616",
    "receptorUsoCfdi": "G03",
    
    // Montos
    "subTotal": 1000.00,
    "discountAmount": 0.00,
    "taxAmount": 160.00,
    "total": 1160.00,
    "moneda": "MXN",
    "tipoCambio": 1.000000,
    
    // Estado
    "status": "Timbrada",
    "uuid": "12345678-1234-1234-1234-123456789012",
    "timbradoAt": "2024-03-19T10:35:00Z",
    
    // Información de timbrado (solo si status = "Timbrada")
    "xmlCfdi": "<xml>...</xml>",
    "cadenaOriginalSat": "||...",
    "selloCfdi": "ABC123...",
    "selloSat": "XYZ789...",
    "noCertificadoCfdi": "20001000000300022323",
    "noCertificadoSat": "20001000000300022815",
    "qrCode": "data:image/png;base64,...",
    
    // Cancelación (solo si status = "Cancelada")
    "cancelledAt": null,
    "cancellationReason": null,
    "cancelledByUserId": null,
    "cancelledByUserName": null,
    
    // Detalles (conceptos CFDI)
    "details": [
      {
        "id": 1,
        "productId": 10,
        "claveProdServ": "01010101",
        "noIdentificacion": "PROD-001",
        "cantidad": 2.000000,
        "claveUnidad": "H87",
        "unidad": "Pieza",
        "descripcion": "Producto ejemplo",
        "valorUnitario": 500.000000,
        "importe": 1000.000000,
        "descuento": 0.000000,
        "objetoImp": "02",
        
        // Impuestos - Traslados
        "tieneTraslados": true,
        "trasladoBase": 1000.00,
        "trasladoImpuesto": "002",
        "trasladoTipoFactor": "Tasa",
        "trasladoTasaOCuota": 0.160000,
        "trasladoImporte": 160.00,
        
        // Impuestos - Retenciones
        "tieneRetenciones": false,
        "retencionBase": null,
        "retencionImpuesto": null,
        "retencionTipoFactor": null,
        "retencionTasaOCuota": null,
        "retencionImporte": null,
        
        "notes": null
      }
    ],
    
    // Auditoría
    "notes": "Factura generada desde venta",
    "createdAt": "2024-03-19T10:30:00Z",
    "createdByUserId": 1,
    "createdByUserName": "Juan Pérez",
    "updatedAt": "2024-03-19T10:35:00Z"
  }
}
```

---

## ❌ Respuestas de Error

### 404 Not Found - Factura no encontrada
```json
{
  "message": "Factura con ID 999 no encontrada",
  "error": 1
}
```

### 401 Unauthorized - Sin autenticación
```json
{
  "message": "Usuario no autenticado",
  "error": 1
}
```

### 403 Forbidden - Sin permisos
```json
{
  "message": "No tiene permisos para acceder a este módulo",
  "error": 1
}
```

### 500 Internal Server Error
```json
{
  "message": "Error al obtener factura",
  "error": 2,
  "details": "Detalles del error..."
}
```

---

## 📊 Estados de Factura

| Estado | Descripción |
|--------|-------------|
| `Borrador` | Factura creada pero no timbrada |
| `Timbrada` | Factura timbrada con el PAC (tiene UUID y XML) |
| `Cancelada` | Factura cancelada ante el SAT |

---

## 🔍 Información Incluida

El endpoint incluye:

### 1. **Datos del Comprobante**
   - Serie y folio
   - Fecha de emisión
   - Forma y método de pago SAT
   - Tipo de comprobante
   - Lugar de expedición

### 2. **Datos del Emisor**
   - RFC, nombre y régimen fiscal
   - Referencia a la empresa

### 3. **Datos del Receptor**
   - RFC, nombre, domicilio fiscal
   - Régimen fiscal y uso del CFDI
   - Referencia al cliente (si aplica)

### 4. **Montos**
   - Subtotal, descuentos, impuestos y total
   - Moneda y tipo de cambio

### 5. **Conceptos (Detalles)**
   - Claves SAT (producto/servicio y unidad)
   - Cantidades, precios y descripciones
   - Impuestos trasladados y retenidos por concepto

### 6. **Información de Timbrado** (solo si está timbrada)
   - UUID del SAT
   - XML completo del CFDI
   - Sellos digitales
   - Código QR
   - Números de certificado

### 7. **Información de Cancelación** (solo si está cancelada)
   - Fecha y motivo de cancelación
   - Usuario que canceló

### 8. **Auditoría**
   - Fechas de creación y actualización
   - Usuario creador

---

## 💡 Casos de Uso

1. **Visualizar factura completa**: Mostrar todos los datos en pantalla de detalle
2. **Descargar XML**: Obtener el XML para envío por correo o descarga
3. **Imprimir factura**: Usar los datos para generar PDF
4. **Auditoría**: Revisar quién, cuándo y cómo se creó la factura
5. **Re-timbrado**: Verificar datos antes de intentar timbrar
6. **Verificación SAT**: Confirmar UUID y sellos digitales

---

## 🔄 Relación con Otros Endpoints

| Endpoint | Propósito |
|----------|-----------|
| `GET /api/billing/invoices` | Listar facturas con paginación |
| `POST /api/billing/invoices` | Crear nueva factura |
| `POST /api/billing/timbrar` | Timbrar CFDI desde venta |
| `POST /api/billing/invoices/{id}/stamp` | Timbrar factura borrador |
| `GET /api/billing/sale/{saleId}` | Obtener venta para facturación |

---

## 📝 Ejemplo de Uso (JavaScript/Axios)

```javascript
// Obtener factura por ID
async function getInvoice(invoiceId) {
  try {
    const response = await axios.get(
      `http://localhost:5000/api/billing/invoices/${invoiceId}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    if (response.data.error === 0) {
      const invoice = response.data.data;
      console.log(`Factura ${invoice.serie}-${invoice.folio}`);
      console.log(`Estado: ${invoice.status}`);
      console.log(`Total: $${invoice.total} ${invoice.moneda}`);
      console.log(`Conceptos: ${invoice.details.length}`);
      
      return invoice;
    }
  } catch (error) {
    if (error.response?.status === 404) {
      console.error('Factura no encontrada');
    } else if (error.response?.status === 403) {
      console.error('Sin permisos para ver facturas');
    } else {
      console.error('Error al obtener factura:', error.message);
    }
    throw error;
  }
}

// Uso
const invoice = await getInvoice(1);

// Descargar XML si está timbrada
if (invoice.status === 'Timbrada' && invoice.xmlCfdi) {
  const blob = new Blob([invoice.xmlCfdi], { type: 'text/xml' });
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `${invoice.serie}-${invoice.folio}-${invoice.uuid}.xml`;
  link.click();
}

// Mostrar código QR si existe
if (invoice.qrCode) {
  const img = document.createElement('img');
  img.src = invoice.qrCode;
  document.getElementById('qr-container').appendChild(img);
}
```

---

## 🔒 Notas de Seguridad

1. ✅ El endpoint valida permisos CFDI - View
2. ✅ Los datos sensibles (sellos, certificados) solo se incluyen si el usuario tiene acceso
3. ✅ El XML completo solo se retorna para facturas timbradas
4. ✅ Se valida que la factura exista antes de retornar datos

---

## ✅ Implementación Completada

- ✅ Query: `GetInvoiceByIdQuery`
- ✅ Query Handler: `GetInvoiceByIdQueryHandler`
- ✅ Endpoint: `GET /api/billing/invoices/{id}`
- ✅ DTOs: `InvoiceDto`, `InvoiceDetailDto`, `InvoiceResponseDto`
- ✅ Repositorio: `IInvoiceRepository.GetByIdAsync()`
- ✅ Permisos: `CFDI - View`
- ✅ Manejo de errores: 404, 401, 403, 500

---

**Fecha de creación**: 19 de marzo de 2026  
**Versión**: 1.0  
**Autor**: Sistema de Facturación EasyPOS
