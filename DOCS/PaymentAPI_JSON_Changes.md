# Cambios en JSON de Endpoints de Pagos - AccountsReceivable

## 📋 Resumen de Cambios
Se movieron los campos de **Complemento de Pago (Timbrado SAT)** desde `PaymentApplicationDto` hacia `PaymentDto`. 

**Razón:** Un pago genera **UN SOLO complemento** con múltiples aplicaciones, no un complemento por aplicación.

---

## 🔄 Endpoints Afectados

### 1. **POST** `/api/accounts-receivable/payments`
**Retorna:** `PaymentDto` (response modificado)

### 2. **GET** `/api/accounts-receivable/payments/{id}`
**Retorna:** `PaymentDto` (response modificado)

### 3. **GET** `/api/accounts-receivable/batches`
**Retorna:** `PaymentBatchPagedResultDto` → contiene lista de `PaymentDto` (modificado)

### 4. **POST** `/api/accounts-receivable/batches`
**Retorna:** `PaymentBatchDto` → contiene lista de `PaymentDto` (modificado)

### 5. **GET** `/api/accounts-receivable/batches/{id}`
**Retorna:** `PaymentBatchDto` → contiene lista de `PaymentDto` (modificado)

### 6. **GET** `/api/accounts-receivable/customers/{customerId}/statement`
**Retorna:** `CustomerStatementDto` → contiene lista de `PaymentDto` (modificado)

---

## ✅ PaymentDto - NUEVOS Campos Agregados

```json
{
  "id": 1,
  "paymentNumber": "PAY-2026-0001",
  "customerId": 5,
  "customerName": "Cliente SA de CV",
  "companyId": 1,
  
  // ⭐ NUEVOS CAMPOS - Datos de Complemento de Pago
  "complementSerie": "COMP",
  "complementFolio": "123",
  "uuid": "12345678-ABCD-1234-ABCD-123456789ABC",
  "timbradoAt": "2026-03-27T10:30:00Z",
  "xmlCfdi": "<cfdi:Comprobante...>",
  "cadenaOriginalSat": "||1.0|UUID|...",
  "selloCfdi": "ABC123...",
  "selloSat": "XYZ789...",
  "noCertificadoCfdi": "30001000000500003456",
  "noCertificadoSat": "30001000000400002345",
  "qrCode": "https://verificacfdi.facturaelectronica.sat.gob.mx/...",
  "xmlPath": "/cfdi/complements/2026/03/COMP-123.xml",
  "pdfPath": "/cfdi/complements/2026/03/COMP-123.pdf",
  "emailSent": false,
  "complementError": null,
  // FIN NUEVOS CAMPOS ⭐
  
  "totalAmount": 15000.00,
  "currency": "MXN",
  "paymentFormSAT": "03",
  "status": "Applied",
  "appliedToInvoices": 3,
  "complementsGenerated": 1,
  "complementsWithError": 0,
  "applications": [
    // Array de PaymentApplicationDto (ver abajo)
  ]
}
```

---

## ❌ PaymentApplicationDto - Campos ELIMINADOS

**Campos que ya NO están en PaymentApplicationDto:**

```diff
- "complementUuid": null,              // ❌ ELIMINADO
- "complementStatus": null,            // ❌ ELIMINADO
- "complementTimbradoAt": null,        // ❌ ELIMINADO
- "xmlPath": null,                     // ❌ ELIMINADO
- "pdfPath": null,                     // ❌ ELIMINADO
- "complementSerie": null,             // ❌ ELIMINADO
- "complementFolio": null,             // ❌ ELIMINADO
- "xmlCfdi": null,                     // ❌ ELIMINADO
- "cadenaOriginalSat": null,           // ❌ ELIMINADO
- "selloCfdi": null,                   // ❌ ELIMINADO
- "selloSat": null,                    // ❌ ELIMINADO
- "noCertificadoCfdi": null,           // ❌ ELIMINADO
- "noCertificadoSat": null,            // ❌ ELIMINADO
- "qrCode": null,                      // ❌ ELIMINADO
- "emailSent": false,                  // ❌ ELIMINADO
- "complementError": null              // ❌ ELIMINADO
```

**Estructura actual de PaymentApplicationDto (después de cambios):**

```json
{
  "id": 101,
  "paymentId": 1,
  "invoiceId": 45,
  "folioUUID": "87654321-DCBA-4321-DCBA-987654321CBA",
  "serieAndFolio": "FAC-2026-0045",
  "paymentType": "PPD",
  "partialityNumber": 1,
  "previousBalance": 20000.00,
  "amountApplied": 5000.00,
  "newBalance": 15000.00,
  "documentCurrency": "MXN",
  "documentExchangeRate": 1.0,
  "taxObject": "02",
  "taxBase": 4310.34,
  "taxCode": "002",
  "taxFactorType": "Tasa",
  "taxRate": 0.160000,
  "taxAmount": 689.66,
  "createdAt": "2026-03-27T10:00:00Z",
  "appliedAt": "2026-03-27T10:30:00Z"
}
```

---

## 🔍 Ejemplo Completo - PaymentDto con Applications

```json
{
  "id": 1,
  "paymentNumber": "PAY-2026-0001",
  "customerId": 5,
  "customerName": "ACME Corp SA de CV",
  "companyId": 1,
  "batchId": null,
  "batchNumber": null,
  "isBatchPayment": false,
  "paymentDate": "2026-03-27T00:00:00Z",
  "totalAmount": 15000.00,
  "currency": "MXN",
  "paymentFormSAT": "03",
  "reference": "TRANSFER-123456",
  
  "emisorRfc": "AAA010101AAA",
  "emisorNombre": "Mi Empresa SA de CV",
  "emisorRegimenFiscal": "601",
  "lugarExpedicion": "64000",
  
  "receptorRfc": "BBB020202BBB",
  "receptorNombre": "ACME Corp SA de CV",
  "receptorDomicilioFiscal": "64120",
  "receptorRegimenFiscal": "601",
  "receptorUsoCfdi": "CP01",
  
  "status": "Applied",
  "appliedToInvoices": 3,
  "complementsGenerated": 1,
  "complementsWithError": 0,
  
  // ⭐ DATOS DEL COMPLEMENTO (NUEVO)
  "complementSerie": "COMP",
  "complementFolio": "123",
  "uuid": "12345678-ABCD-1234-ABCD-123456789ABC",
  "timbradoAt": "2026-03-27T10:30:00Z",
  "xmlCfdi": "<?xml version=\"1.0\" encoding=\"UTF-8\"?><cfdi:Comprobante...",
  "cadenaOriginalSat": "||1.0|UUID|2026-03-27T10:30:00|...",
  "selloCfdi": "ABC123DEF456...",
  "selloSat": "XYZ789UVW012...",
  "noCertificadoCfdi": "30001000000500003456",
  "noCertificadoSat": "30001000000400002345",
  "qrCode": "https://verificacfdi.facturaelectronica.sat.gob.mx/?id=...",
  "xmlPath": "/cfdi/complements/2026/03/COMP-123.xml",
  "pdfPath": "/cfdi/complements/2026/03/COMP-123.pdf",
  "emailSent": false,
  "complementError": null,
  
  "notes": "Pago vía transferencia bancaria",
  "createdAt": "2026-03-27T10:00:00Z",
  "appliedAt": "2026-03-27T10:15:00Z",
  "completedAt": "2026-03-27T10:30:00Z",
  
  "applications": [
    {
      "id": 101,
      "paymentId": 1,
      "invoiceId": 45,
      "folioUUID": "87654321-DCBA-4321-DCBA-987654321CBA",
      "serieAndFolio": "FAC-2026-0045",
      "paymentType": "PPD",
      "partialityNumber": 1,
      "previousBalance": 20000.00,
      "amountApplied": 5000.00,
      "newBalance": 15000.00,
      "documentCurrency": "MXN",
      "documentExchangeRate": 1.0,
      "taxObject": "02",
      "taxBase": 4310.34,
      "taxCode": "002",
      "taxFactorType": "Tasa",
      "taxRate": 0.160000,
      "taxAmount": 689.66,
      "createdAt": "2026-03-27T10:00:00Z",
      "appliedAt": "2026-03-27T10:30:00Z"
    },
    {
      "id": 102,
      "paymentId": 1,
      "invoiceId": 46,
      "folioUUID": "11111111-2222-3333-4444-555555555555",
      "serieAndFolio": "FAC-2026-0046",
      "paymentType": "PPD",
      "partialityNumber": 2,
      "previousBalance": 8000.00,
      "amountApplied": 8000.00,
      "newBalance": 0.00,
      "documentCurrency": "MXN",
      "documentExchangeRate": 1.0,
      "taxObject": "02",
      "taxBase": 6896.55,
      "taxCode": "002",
      "taxFactorType": "Tasa",
      "taxRate": 0.160000,
      "taxAmount": 1103.45,
      "createdAt": "2026-03-27T10:00:00Z",
      "appliedAt": "2026-03-27T10:30:00Z"
    },
    {
      "id": 103,
      "paymentId": 1,
      "invoiceId": 47,
      "folioUUID": "66666666-7777-8888-9999-000000000000",
      "serieAndFolio": "FAC-2026-0047",
      "paymentType": "PPD",
      "partialityNumber": 1,
      "previousBalance": 5000.00,
      "amountApplied": 2000.00,
      "newBalance": 3000.00,
      "documentCurrency": "MXN",
      "documentExchangeRate": 1.0,
      "taxObject": "02",
      "taxBase": 1724.14,
      "taxCode": "002",
      "taxFactorType": "Tasa",
      "taxRate": 0.160000,
      "taxAmount": 275.86,
      "createdAt": "2026-03-27T10:00:00Z",
      "appliedAt": "2026-03-27T10:30:00Z"
    }
  ]
}
```

---

## 📊 Comparación Antes vs Después

### ❌ ANTES (INCORRECTO)
```
Payment
  - uuid: null ❌
  - Applications[0]
      - complementUuid: "123-ABC" ❌
      - xmlPath: "/path1.xml" ❌
  - Applications[1]
      - complementUuid: "456-DEF" ❌
      - xmlPath: "/path2.xml" ❌
```
**Problema:** Generaba 1 complemento por aplicación (incorrecto según SAT)

### ✅ DESPUÉS (CORRECTO)
```
Payment
  - uuid: "123-ABC" ✅
  - xmlPath: "/complement.xml" ✅
  - Applications[0]
      - amountApplied: 5000
      - partialityNumber: 1
  - Applications[1]
      - amountApplied: 8000
      - partialityNumber: 2
```
**Correcto:** 1 pago = 1 complemento con múltiples DoctoRelacionado

---

## 🎯 Migración Frontend - Checklist

- [ ] Actualizar interfaces TypeScript/modelos de `PaymentDto`
- [ ] Agregar 16 nuevos campos de complemento a nivel Payment
- [ ] Remover 16 campos de complemento de `PaymentApplicationDto`
- [ ] Actualizar componentes que muestren datos de timbrado:
  - Mostrar UUID en nivel de Payment (no Application)
  - Descargar XML/PDF desde Payment (no Application)
  - Mostrar QR code desde Payment
- [ ] Actualizar lógica de generación de complementos:
  - Botón "Timbrar" a nivel de Payment (no Application)
  - Estado de timbrado (`uuid != null`) a nivel Payment
- [ ] Actualizar filtros/búsquedas que usen campos de complemento
- [ ] Actualizar tablas/grids que muestren estado de complementos

---

## 🚀 Testing Recomendado

1. **Test de estructura JSON:**
   ```bash
   GET /api/accounts-receivable/payments/1
   ```
   Verificar que `uuid`, `xmlPath`, `complementFolio` estén en raíz del objeto Payment

2. **Test de aplicaciones:**
   Verificar que `applications[]` NO contenga campos de complemento

3. **Test de batches:**
   ```bash
   GET /api/accounts-receivable/batches/1
   ```
   Verificar que cada Payment en el array tenga los campos de complemento

---

## 📞 Contacto
Si tu IA frontend necesita ejemplos adicionales o tiene dudas sobre la estructura, revisar este documento.

**Fecha de cambio:** 27 de Marzo 2026  
**Migración aplicada:** `SimplifyPaymentComplementModel` (20260327183944)
