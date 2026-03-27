# Flujo Completo: Complementos de Pago con Timbrado

## 📋 Resumen
La tabla `Payments` ahora funciona **exactamente igual** que `Invoices`: guarda toda la información necesaria para timbrar el complemento de pago Y almacena el resultado del timbrado (UUID, XML, sellos, etc.).

---

## 🗄️ Estructura de Datos

### Tabla: `Payments`
Esta tabla tiene **TODO** lo necesario para generar y guardar el complemento de pago:

#### ✅ Datos para GENERAR el complemento:
```sql
-- Emisor
CompanyId              -- int (FK a Companies con RFC, nombre, domicilio)

-- Receptor
CustomerId             -- int (FK a Customers con RFC, nombre, domicilio fiscal)
CustomerName           -- nvarchar(200)

-- Datos del Pago
PaymentNumber          -- nvarchar(20) - Folio interno: PAG-2026-001
PaymentDate            -- datetime2 - Fecha del pago
TotalAmount            -- decimal(18,2) - Monto total pagado
Currency               -- nvarchar(3) - MXN
ExchangeRate           -- decimal(18,6) - Tipo de cambio
PaymentFormSAT         -- nvarchar(2) - c_FormaPago (03=Transferencia, 01=Efectivo, etc.)
BankOrigin             -- nvarchar(100) - Banco del cliente (opcional)
BankAccountOrigin      -- nvarchar(20) - Cuenta origen (opcional)
BankDestination        -- nvarchar(100) - Banco destino (opcional)
BankAccountDestination -- nvarchar(20) - Cuenta destino (opcional)
Reference              -- nvarchar(50) - Número de referencia (SPEI, cheque, etc.)
```

#### ✅ Datos del TIMBRADO (se guardan después de timbrar):
```sql
-- Datos del PAC/SAT después del timbrado
Uuid                   -- nvarchar(50) - UUID del complemento timbrado
TimbradoAt             -- datetime2 - Fecha y hora del timbrado
XmlCfdi                -- nvarchar(MAX) - XML completo timbrado
CadenaOriginalSat      -- nvarchar(MAX) - Cadena original del SAT
SelloCfdi              -- nvarchar(MAX) - Sello digital del CFDI
SelloSat               -- nvarchar(MAX) - Sello digital del SAT
NoCertificadoCfdi      -- nvarchar(20) - Número de certificado del CFDI
NoCertificadoSat       -- nvarchar(20) - Número de certificado del SAT
QrCode                 -- nvarchar(MAX) - Código QR en base64
```

#### ✅ Control y Auditoría:
```sql
Status                 -- nvarchar(20) - Draft, Applied, Complemented, Cancelled
AppliedToInvoices      -- int - Cuántas facturas se pagaron
ComplementsGenerated   -- int - Cuántos complementos se generaron
ComplementsWithError   -- int - Cuántos fallaron
CreatedAt              -- datetime2
AppliedAt              -- datetime2 (cuando se aplicó)
CompletedAt            -- datetime2 (cuando se completó el timbrado)
CancelledAt            -- datetime2 (si se canceló)
CancellationReason     -- nvarchar(500)
Notes                  -- nvarchar(1000)
```

---

## 🔄 Flujo Completo del Proceso

### **Paso 1: Crear Batch de Pagos** 
`POST /api/payments/batches`

```json
{
  "companyId": 1,
  "paymentDate": "2026-03-27",
  "paymentFormSAT": "03",
  "bankDestination": "BBVA Bancomer",
  "accountDestination": "0123456789",
  "description": "Pagos del 27 de marzo",
  "payments": [
    {
      "customerId": 5,
      "reference": "SPEI123456",
      "invoices": [
        { "invoicePPDId": 10, "amountToPay": 5000 },
        { "invoicePPDId": 11, "amountToPay": 3000 }
      ]
    },
    {
      "customerId": 8,
      "reference": "SPEI789012",
      "invoices": [
        { "invoicePPDId": 15, "amountToPay": 10000 }
      ]
    }
  ]
}
```

**Se crean registros en:**
- ✅ `PaymentBatches` (1 registro): El lote con el folio BTCP-COMP001-270326-001
- ✅ `Payments` (2 registros): Un pago por cada cliente
- ✅ `PaymentApplications` (3 registros): Cada factura aplicada con sus impuestos calculados

**Estado inicial de Payment:**
```json
{
  "id": 100,
  "paymentNumber": "PAG-2026-001",
  "customerId": 5,
  "companyId": 1,
  "paymentDate": "2026-03-27",
  "totalAmount": 8000,
  "paymentFormSAT": "03",
  "reference": "SPEI123456",
  "status": "Draft",
  "uuid": null,           // ← AÚN NO TIMBRADO
  "timbradoAt": null,
  "xmlCfdi": null
}
```

---

### **Paso 2: Timbrar el Complemento de Pago**
`POST /api/payments/{paymentId}/stamp`

Este endpoint debe:

1. **Obtener datos del Payment:**
   - CompanyId → buscar Company (RFC emisor, certificados, etc.)
   - CustomerId → buscar Customer (RFC receptor, nombre, domicilio)
   - PaymentDate, TotalAmount, PaymentFormSAT, Reference

2. **Obtener las aplicaciones de pago:**
   - Query a `PaymentApplications` WHERE PaymentId = {paymentId}
   - Cada registro tiene: UUID de factura, montos, impuestos calculados

3. **Generar XML del complemento:**
```xml
<cfdi:Comprobante 
    Version="4.0" 
    TipoDeComprobante="P"
    Fecha="2026-03-27T10:30:00"
    LugarExpedicion="64000">
    
    <cfdi:Emisor Rfc="EMP123456ABC" Nombre="Mi Empresa SA" />
    <cfdi:Receptor Rfc="CLI987654XYZ" Nombre="Cliente Ejemplo" UsoCFDI="CP01" />
    
    <cfdi:Conceptos>
        <cfdi:Concepto 
            ClaveProdServ="84111506"
            Cantidad="1"
            ClaveUnidad="ACT"
            Descripcion="Pago"
            ValorUnitario="0"
            Importe="0" />
    </cfdi:Conceptos>
    
    <cfdi:Complemento>
        <pago20:Pagos Version="2.0">
            <pago20:Totales 
                TotalRetencionesIVA="0"
                TotalRetencionesISR="0"
                TotalTrasladosBaseIVA16="6896.55"
                TotalTrasladosImpuestoIVA16="1103.45"
                MontoTotalPagos="8000.00" />
                
            <pago20:Pago 
                FechaPago="2026-03-27T10:30:00"
                FormaDePagoP="03"
                MonedaP="MXN"
                TipoCambioP="1"
                Monto="8000.00"
                NumOperacion="SPEI123456">
                
                <!-- Factura 1 -->
                <pago20:DoctoRelacionado 
                    IdDocumento="A1B2C3D4-..." 
                    Serie="A" Folio="1001"
                    MonedaDR="MXN"
                    EquivalenciaDR="1"
                    NumParcialidad="1"
                    ImpSaldoAnt="10000.00"
                    ImpPagado="5000.00"
                    ImpSaldoInsoluto="5000.00"
                    ObjetoImpDR="02">
                    
                    <pago20:ImpuestosDR>
                        <pago20:TrasladosDR>
                            <pago20:TrasladoDR 
                                BaseDR="4310.34"
                                ImpuestoDR="002"
                                TipoFactorDR="Tasa"
                                TasaOCuotaDR="0.160000"
                                ImporteDR="689.66" />
                        </pago20:TrasladosDR>
                    </pago20:ImpuestosDR>
                </pago20:DoctoRelacionado>
                
                <!-- Factura 2 -->
                <pago20:DoctoRelacionado ...>
                    ...
                </pago20:DoctoRelacionado>
            </pago20:Pago>
        </pago20:Pagos>
    </cfdi:Complemento>
</cfdi:Comprobante>
```

4. **Enviar al PAC para timbrar:**
   - POST a API del PAC (SW Sapien, Finkok, etc.)
   - Recibir XML timbrado con UUID, sellos, certificados

5. **GUARDAR resultado en `Payments`:**
```csharp
payment.Uuid = responseFromPac.Uuid; // "A1B2C3D4-E5F6-..."
payment.TimbradoAt = DateTime.UtcNow;
payment.XmlCfdi = responseFromPac.XmlCompleto;
payment.CadenaOriginalSat = responseFromPac.CadenaOriginal;
payment.SelloCfdi = responseFromPac.SelloCfdi;
payment.SelloSat = responseFromPac.SelloSat;
payment.NoCertificadoCfdi = responseFromPac.NumeroCertificado;
payment.NoCertificadoSat = responseFromPac.NumeroCertificadoSat;
payment.QrCode = GenerateQRCode(payment.Uuid, emisorRfc, receptorRfc, totalAmount);
payment.Status = "Complemented";
payment.CompletedAt = DateTime.UtcNow;

await _paymentRepository.UpdateAsync(payment);
```

---

### **Paso 3: Obtener el Complemento Timbrado**
`GET /api/payments/{paymentId}`

**Respuesta:**
```json
{
  "id": 100,
  "paymentNumber": "PAG-2026-001",
  "customerId": 5,
  "customerName": "Cliente Ejemplo SA",
  "paymentDate": "2026-03-27",
  "totalAmount": 8000,
  "paymentFormSAT": "03",
  "reference": "SPEI123456",
  "status": "Complemented",
  
  // DATOS DEL TIMBRADO ✅
  "uuid": "A1B2C3D4-E5F6-1234-5678-ABCDEF123456",
  "timbradoAt": "2026-03-27T11:00:00Z",
  "xmlCfdi": "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<cfdi:Comprobante...",
  "cadenaOriginalSat": "||4.0|P|A1B2C3D4...",
  "selloCfdi": "ABC123...",
  "selloSat": "XYZ789...",
  "noCertificadoCfdi": "00001000000123456789",
  "noCertificadoSat": "00001000000987654321",
  "qrCode": "iVBORw0KGgoAAAANSUhEUgAA...",
  
  "appliedToInvoices": 2,
  "applications": [
    {
      "invoicePPDId": 10,
      "folioUUID": "INVOICE-UUID-1",
      "serieAndFolio": "A-1001",
      "previousBalance": 10000,
      "amountApplied": 5000,
      "newBalance": 5000,
      "taxBase": 4310.34,
      "taxAmount": 689.66
    },
    {
      "invoicePPDId": 11,
      "folioUUID": "INVOICE-UUID-2",
      "serieAndFolio": "A-1002",
      "previousBalance": 5000,
      "amountApplied": 3000,
      "newBalance": 2000,
      "taxBase": 2586.21,
      "taxAmount": 413.79
    }
  ]
}
```

---

## 📊 Comparación con Invoices

| Característica | **Invoices** | **Payments** |
|---|---|---|
| RFC Emisor | ✅ Company.RFC | ✅ Company.RFC |
| RFC Receptor | ✅ Customer.RFC | ✅ Customer.RFC |
| Guarda UUID | ✅ Invoice.Uuid | ✅ Payment.Uuid |
| Guarda XML | ✅ Invoice.XmlCfdi | ✅ Payment.XmlCfdi |
| Guarda Sellos | ✅ Invoice.SelloCfdi/SelloSat | ✅ Payment.SelloCfdi/SelloSat |
| Guarda QR | ✅ Invoice.QrCode | ✅ Payment.QrCode |
| Fecha Timbrado | ✅ Invoice.TimbradoAt | ✅ Payment.TimbradoAt |
| Tipo Comprobante | "I" (Ingreso) | **"P" (Pago)** |
| Conceptos | ✅ InvoiceDetails | ❌ Solo "Pago" |
| Documentos Relacionados | ❌ | ✅ PaymentApplications |

---

## ✅ Ventajas de Este Diseño

1. **Todo en un solo lugar**: No necesitas buscar en múltiples tablas
2. **Misma lógica que Invoices**: El código de timbrado es casi idéntico
3. **Cancelación fácil**: Ya tienes CancelledAt y CancellationReason
4. **Auditoría completa**: Sabes cuándo se creó, aplicó y timbró
5. **Descarga directa**: El XML está en la BD, no en archivos externos

---

## 🎯 Siguiente Paso

Implementar el endpoint:
```csharp
POST /api/payments/{paymentId}/stamp
```

Este endpoint:
1. Lee el Payment y sus PaymentApplications
2. Construye el XML del complemento de pago
3. Envía al PAC
4. Guarda UUID, XML, sellos en el mismo Payment
5. Actualiza Status = "Complemented"

¿Quieres que te ayude a implementar el endpoint de timbrado? 🚀
