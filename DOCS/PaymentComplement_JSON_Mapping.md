# Mapeo de Datos para Complemento de Pago (JSON de Timbrado)

## Resumen
Este documento explica cómo los datos guardados en las tablas `InvoicesPPD` y `PaymentApplications` se mapean a los campos necesarios para generar el JSON de timbrado de un Complemento de Pago (CFDI tipo "P").

---

## Estructura del JSON de Timbrado

### 1. Datos del CFDI Principal

```json
{
  "Version": "4.0",
  "Serie": "00",                    // → Generar desde PaymentApplication.ComplementSerie
  "Folio": "0000179826",            // → Generar desde PaymentApplication.ComplementFolio
  "Fecha": "2024-04-29T00:00:00",   // → Payment.PaymentDate
  "SubTotal": 0,                     // → Siempre 0 para complemento de pago
  "Moneda": "XXX",                   // → Siempre "XXX" para complemento de pago
  "Total": 0,                        // → Siempre 0 para complemento de pago
  "TipoDeComprobante": "P",          // → Siempre "P" (Pago)
  "Exportacion": "01",               // → "01" (No aplica)
  "LugarExpedicion": "75700"         // → Company.ZipCode
}
```

**Fuentes de datos:**
- `Payment.PaymentDate` → Fecha del pago
- `Company` → Datos del emisor (lugar de expedición)
- Serie/Folio del complemento se generan automáticamente

---

### 2. Datos del Emisor

```json
"Emisor": {
  "Rfc": "EKU9003173C9",              // → Company.RFC
  "Nombre": "ESCUELA KEMPER URGATE",  // → Company.Name
  "RegimenFiscal": 601                 // → Company.TaxRegime
}
```

**Fuentes de datos:**
- Tabla `Company` (la empresa que emite el complemento)

---

### 3. Datos del Receptor

```json
"Receptor": {
  "Rfc": "XAXX010101000",                   // → InvoicePPD.CustomerRFC
  "Nombre": "PUBLICO GENERAL",              // → InvoicePPD.CustomerName
  "DomicilioFiscalReceptor": "75700",       // → InvoicePPD.CustomerZipCode ✅ NUEVO
  "RegimenFiscalReceptor": "616",           // → InvoicePPD.CustomerTaxRegime ✅ NUEVO
  "UsoCFDI": "CP01"                         // → InvoicePPD.CustomerCfdiUse ✅ NUEVO (CP01 = Complemento de pago)
}
```

**Fuentes de datos:**
- Tabla `InvoicesPPD`:
  - `CustomerRFC` (existía)
  - `CustomerName` (existía)
  - `CustomerZipCode` ← **NUEVO CAMPO**
  - `CustomerTaxRegime` ← **NUEVO CAMPO**
  - `CustomerCfdiUse` ← **NUEVO CAMPO**

---

### 4. Conceptos (Concepto cero para complemento de pago)

```json
"Conceptos": [
  {
    "ClaveProdServ": "84111506",    // → Código SAT fijo para servicios de pago
    "Cantidad": 1,
    "ClaveUnidad": "ACT",           // → Código SAT fijo para "Actividad"
    "Descripcion": "Pago",
    "ValorUnitario": 0,
    "Importe": 0,
    "ObjetoImp": "01"               // → "01" (No objeto de impuesto en concepto)
  }
]
```

**Fuentes de datos:**
- Valores fijos según la normativa SAT para complementos de pago

---

### 5. Complemento de Pago 2.0

#### 5.1 Totales del Complemento

```json
"Complemento": {
  "Any": [
    {
      "Pago20:Pagos": {
        "Version": "2.0",
        "Totales": {
          "TotalTrasladosBaseIVA16": "5843.11",     // → SUMA de PaymentApplication.TaxBase (todas las aplicaciones)
          "TotalTrasladosImpuestoIVA16": "934.90",  // → SUMA de PaymentApplication.TaxAmount (todas las aplicaciones)
          "MontoTotalPagos": "6778.00"              // → Payment.TotalAmount
        }
      }
    }
  ]
}
```

**Cálculo:**
```csharp
// Suma de todas las PaymentApplications de este Payment
var totals = new {
    TotalTrasladosBaseIVA16 = paymentApplications.Sum(pa => pa.TaxBase),
    TotalTrasladosImpuestoIVA16 = paymentApplications.Sum(pa => pa.TaxAmount),
    MontoTotalPagos = payment.TotalAmount
};
```

---

#### 5.2 Pago

```json
"Pago": [
  {
    "FechaPago": "2022-09-09T17:33:38",   // → Payment.PaymentDate
    "FormaDePagoP": "01",                  // → Payment.PaymentFormSAT (01=Efectivo, 03=Transferencia, etc.)
    "MonedaP": "MXN",                      // → Payment.Currency
    "TipoCambioP": "1",                    // → Payment.ExchangeRate
    "Monto": "6778.00"                     // → Payment.TotalAmount
  }
]
```

**Fuentes de datos:**
- Tabla `Payment`:
  - `PaymentDate`
  - `PaymentFormSAT` ← Forma de pago
  - `Currency`
  - `ExchangeRate`
  - `TotalAmount`

---

#### 5.3 Documentos Relacionados (por cada factura pagada)

```json
"DoctoRelacionado": [
  {
    "IdDocumento": "b7c8d2bf-cb4e-4f84-af89-c68b6731206a",  // → PaymentApplication.FolioUUID
    "Serie": "FA",                                           // → InvoicePPD.Serie
    "Folio": "N0000216349",                                  // → InvoicePPD.Folio
    "MonedaDR": "MXN",                                       // → PaymentApplication.DocumentCurrency ✅ NUEVO
    "EquivalenciaDR": "1",                                   // → PaymentApplication.DocumentExchangeRate ✅ NUEVO
    "NumParcialidad": "2",                                   // → PaymentApplication.PartialityNumber
    "ImpSaldoAnt": "6777.41",                                // → PaymentApplication.PreviousBalance
    "ImpPagado": "6777.41",                                  // → PaymentApplication.AmountApplied
    "ImpSaldoInsoluto": "0.00",                              // → PaymentApplication.NewBalance
    "ObjetoImpDR": "02"                                      // → PaymentApplication.TaxObject ✅ NUEVO (02=Sí objeto)
  }
]
```

**Fuentes de datos:**
- Tabla `PaymentApplication`:
  - `FolioUUID` (existía)
  - `PartialityNumber` (existía)
  - `PreviousBalance` (existía)
  - `AmountApplied` (existía)
  - `NewBalance` (existía)
  - `DocumentCurrency` ← **NUEVO CAMPO**
  - `DocumentExchangeRate` ← **NUEVO CAMPO**
  - `TaxObject` ← **NUEVO CAMPO**
- Tabla `InvoicePPD`:
  - `Serie`
  - `Folio`

---

#### 5.4 ImpuestosDR (impuestos del documento relacionado)

```json
"ImpuestosDR": {
  "TrasladosDR": [
    {
      "BaseDR": "5842.600000",           // → PaymentApplication.TaxBase ✅ NUEVO (calculado proporcionalmente)
      "ImpuestoDR": "002",               // → PaymentApplication.TaxCode ✅ NUEVO (002=IVA)
      "TipoFactorDR": "Tasa",            // → PaymentApplication.TaxFactorType ✅ NUEVO
      "TasaOCuotaDR": "0.160000",        // → PaymentApplication.TaxRate ✅ NUEVO (0.16 = 16%)
      "ImporteDR": "934.816000"          // → PaymentApplication.TaxAmount ✅ NUEVO (calculado proporcionalmente)
    }
  ]
}
```

**Cómo se calculan los impuestos (proporcionalmente):**

```csharp
// Ejemplo: Factura original de $10,000 (Subtotal: $8,620.69, IVA 16%: $1,379.31)
// Si se paga $5,000 (50% de la factura):

decimal proportionPaid = AmountApplied / OriginalInvoiceAmount;  // 5000 / 10000 = 0.5
decimal taxBase = Subtotal * proportionPaid;                     // 8620.69 * 0.5 = 4310.345
decimal taxAmount = TaxAmount * proportionPaid;                  // 1379.31 * 0.5 = 689.655
```

**Fuentes de datos:**
- Tabla `PaymentApplication`:
  - `TaxBase` ← **NUEVO CAMPO** (calculado automáticamente)
  - `TaxCode` ← **NUEVO CAMPO** (002 = IVA)
  - `TaxFactorType` ← **NUEVO CAMPO** (Tasa)
  - `TaxRate` ← **NUEVO CAMPO** (0.160000 para IVA 16%)
  - `TaxAmount` ← **NUEVO CAMPO** (calculado automáticamente)
- Tabla `InvoicePPD`:
  - `Subtotal` ← **NUEVO CAMPO** (subtotal de la factura original)
  - `TaxAmount` ← **NUEVO CAMPO** (impuestos de la factura original)

---

#### 5.5 ImpuestosP (totales de impuestos del pago)

```json
"ImpuestosP": {
  "TrasladosP": [
    {
      "BaseP": "5843.110000",            // → SUMA de PaymentApplication.TaxBase (este pago)
      "ImpuestoP": "002",                // → "002" (IVA)
      "TipoFactorP": "Tasa",             // → "Tasa"
      "TasaOCuotaP": "0.160000",         // → 0.160000 (16%)
      "ImporteP": "934.897600"           // → SUMA de PaymentApplication.TaxAmount (este pago)
    }
  ]
}
```

**Cálculo:**
```csharp
// Suma de todos los documentos relacionados en este pago
var impuestosP = new {
    BaseP = paymentApplications.Sum(pa => pa.TaxBase),
    ImporteP = paymentApplications.Sum(pa => pa.TaxAmount),
    ImpuestoP = "002",
    TipoFactorP = "Tasa",
    TasaOCuotaP = 0.160000M
};
```

---

## Ejemplo Completo de Flujo

### 1. Timbrar Factura PPD
```http
POST /api/accounts-receivable/invoices-ppd
{
  "InvoiceId": 12345,
  "CustomerId": 100,
  "CustomerName": "PUBLICO GENERAL",
  "CustomerRFC": "XAXX010101000",
  "CustomerZipCode": "75700",              ← NUEVO
  "CustomerTaxRegime": "616",              ← NUEVO
  "CustomerCfdiUse": "CP01",               ← NUEVO
  "CompanyId": 1,
  "FolioUUID": "b7c8d2bf-cb4e-4f84-af89-c68b6731206a",
  "Serie": "FA",
  "Folio": "N0000216349",
  "InvoiceDate": "2022-01-15",
  "CreditDays": 30,
  "Currency": "MXN",
  "ExchangeRate": 1.0,
  "TotalAmount": 10000.00,
  "Subtotal": 8620.69,                    ← NUEVO
  "TaxAmount": 1379.31                    ← NUEVO (IVA 16%)
}
```

### 2. Registrar Pago
```http
POST /api/accounts-receivable/payments
{
  "CustomerId": 100,
  "CompanyId": 1,
  "PaymentDate": "2022-02-15T10:30:00",
  "PaymentMethodSAT": "03",               // 03 = Transferencia electrónica
  "PaymentFormSAT": "01",                 // 01 = Efectivo
  "Currency": "MXN",
  "ExchangeRate": 1.0,
  "Reference": "TRANSFERENCIA 12345",
  "Invoices": [
    {
      "InvoicePPDId": 500,
      "AmountToPay": 5000.00              // Pago parcial (50% de la factura)
    }
  ]
}
```

### 3. Sistema Calcula Automáticamente:
```
PaymentApplication creada con:
- PreviousBalance: 10000.00
- AmountApplied: 5000.00
- NewBalance: 5000.00
- PartialityNumber: 1
- DocumentCurrency: "MXN"
- DocumentExchangeRate: 1.0
- TaxObject: "02"
- TaxBase: 4310.345 (8620.69 * 0.5)
- TaxCode: "002"
- TaxFactorType: "Tasa"
- TaxRate: 0.160000
- TaxAmount: 689.655 (1379.31 * 0.5)
```

### 4. Generar Complemento de Pago
```http
POST /api/accounts-receivable/payments/{id}/generate-complements
{
  "SendEmailsAutomatically": true
}
```

El sistema ahora tiene **TODOS** los datos necesarios para construir el JSON de timbrado completo.

---

## Campos Nuevos Agregados

### Tabla `InvoicesPPD`
- ✅ `CustomerZipCode` (nvarchar(10)) - Código postal del domicilio fiscal del receptor
- ✅ `CustomerTaxRegime` (nvarchar(10)) - Régimen fiscal SAT (601, 603, 616, etc.)
- ✅ `CustomerCfdiUse` (nvarchar(5)) - Uso del CFDI (CP01 para complemento de pago)
- ✅ `Subtotal` (decimal(18,2)) - Subtotal de la factura original (sin impuestos)
- ✅ `TaxAmount` (decimal(18,2)) - Total de impuestos de la factura original

### Tabla `PaymentApplications`
- ✅ `DocumentCurrency` (nvarchar(3)) - Moneda del documento relacionado (MonedaDR)
- ✅ `DocumentExchangeRate` (decimal(18,6)) - Tipo de cambio del documento (EquivalenciaDR)
- ✅ `TaxObject` (nvarchar(2)) - Objeto de impuesto (01=No, 02=Sí)
- ✅ `TaxBase` (decimal(18,6)) - Base del impuesto calculada proporcionalmente
- ✅ `TaxCode` (nvarchar(3)) - Código del impuesto SAT (002=IVA, 001=ISR)
- ✅ `TaxFactorType` (nvarchar(20)) - Tipo de factor (Tasa, Cuota, Exento)
- ✅ `TaxRate` (decimal(8,6)) - Tasa del impuesto (0.160000 para IVA 16%)
- ✅ `TaxAmount` (decimal(18,6)) - Importe del impuesto calculado proporcionalmente

---

## Conclusión

Con estos cambios, el sistema **guarda automáticamente toda la información necesaria** para generar el JSON de timbrado de un Complemento de Pago sin necesidad de consultar tablas adicionales o hacer cálculos en tiempo de timbrado.

Los impuestos se calculan **proporcionalmente** al momento de aplicar el pago, garantizando que los montos sean exactos según la normativa del SAT.
