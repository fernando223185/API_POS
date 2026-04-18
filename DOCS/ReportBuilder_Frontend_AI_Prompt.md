# Prompt para AI Frontend — Sistema de Report Builder

> Usa este prompt directamente en tu herramienta de AI para frontend (Cursor, Copilot Chat, Claude, etc.)

---

## PROMPT

Necesito que implementes un **Report Builder** completo en React + Electron (TypeScript) para una app POS. El backend ya está construido. A continuación te doy todos los detalles del API, modelos y UX esperada.

---

## BASE URL

```
https://<tu-dominio>/api
```

---

## ENDPOINTS DISPONIBLES

### 1. Listar plantillas
```
GET /api/reports/templates
GET /api/reports/templates?type=Sales
GET /api/reports/templates?type=Sales&companyId=1
```
- Sin parámetros → devuelve **todas** las plantillas de todos los tipos
- Con `type` → filtra por tipo de reporte
- Con `companyId` → filtra por empresa (incluye globales `companyId=null`)

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Reporte de Venta Estándar",
      "reportType": "Sales",
      "description": "Plantilla por defecto para comprobantes de venta POS",
      "isDefault": true,
      "createdAt": "2026-04-18T00:00:00Z"
    }
  ],
  "total": 6,
  "error": 0
}
```

---

### 2. Obtener plantilla por ID (con secciones completas)
```
GET /api/reports/templates/{id}
```
**Response:**
```json
{
  "data": {
    "id": 1,
    "name": "Reporte de Venta Estándar",
    "reportType": "Sales",
    "description": "Plantilla por defecto",
    "isDefault": true,
    "isActive": true,
    "companyId": null,
    "createdByUserId": null,
    "createdByName": null,
    "createdAt": "2026-04-18T00:00:00Z",
    "updatedAt": null,
    "sections": [
      {
        "type": "Header",
        "title": "Información de la Venta",
        "order": 1,
        "showTitle": true,
        "fields": [
          { "field": "saleCode",     "label": "N° de Venta", "bold": true,  "fontSize": 10, "align": "left",  "format": "text",     "inline": false },
          { "field": "saleDate",     "label": "Fecha",       "bold": false, "fontSize": 9,  "align": "left",  "format": "datetime", "inline": true  },
          { "field": "customerName", "label": "Cliente",     "bold": false, "fontSize": 9,  "align": "left",  "format": "text",     "inline": false }
        ],
        "columns": []
      },
      {
        "type": "Table",
        "title": "Detalle de Productos",
        "order": 2,
        "showTitle": true,
        "fields": [],
        "columns": [
          { "field": "productCode", "label": "Código",      "width": 60,  "align": "left",   "format": "text",     "bold": false },
          { "field": "productName", "label": "Descripción", "width": 0,   "align": "left",   "format": "text",     "bold": false },
          { "field": "quantity",    "label": "Cant.",        "width": 45,  "align": "center", "format": "number",   "bold": false },
          { "field": "unitPrice",   "label": "P.Unit",       "width": 65,  "align": "right",  "format": "currency", "bold": false },
          { "field": "lineTotal",   "label": "Total",        "width": 70,  "align": "right",  "format": "currency", "bold": true  }
        ]
      },
      {
        "type": "Summary",
        "title": "Totales",
        "order": 3,
        "showTitle": true,
        "fields": [
          { "field": "totalAmount",    "label": "TOTAL",         "bold": true,  "fontSize": 11, "align": "right", "format": "currency", "inline": false },
          { "field": "paymentMethods", "label": "Forma de pago", "bold": false, "fontSize": 9,  "align": "left",  "format": "text",     "inline": false }
        ],
        "columns": []
      }
    ]
  },
  "error": 0
}
```

---

### 3. Obtener datos para preview en tiempo real ⭐
```
GET /api/reports/templates/{id}/preview-data
```
Este endpoint es la **clave del preview**. Devuelve el mismo `sections[]` de la plantilla más datos de ejemplo listos para renderizar, sin necesidad de conocer qué campos existen por tipo de reporte.

**Response:**
```json
{
  "data": {
    "templateName": "Reporte de Venta Estándar",
    "reportType": "Sales",
    "sections": [ ...mismo array sections[] que el GET por ID... ],
    "mockDataRow": {
      "saleCode":       "VTA-00042",
      "saleDate":       "18/04/2026 10:35",
      "customerName":   "María García López",
      "totalAmount":    "$1,377.50",
      "paymentMethods": "Efectivo"
    },
    "mockTableRows": [
      { "productCode": "PROD-001", "productName": "Laptop Lenovo 15\"", "quantity": "1", "unitPrice": "$950.00", "lineTotal": "$1,046.90" },
      { "productCode": "PROD-017", "productName": "Mouse inalámbrico",  "quantity": "2", "unitPrice": "$175.00", "lineTotal": "$406.00"   }
    ]
  },
  "error": 0
}
```

---

### 4. Obtener catálogo de campos disponibles (para el editor)
```
GET /api/reports/fields/{reportType}
```
`reportType`: `Sales` | `Delivery` | `Quotation` | `Purchase` | `Inventory` | `CashierShift` | `Invoice`

**Response:**
```json
{
  "data": {
    "reportType": "Sales",
    "availableSectionTypes": ["Header", "Table", "Summary", "Footer"],
    "fields": [
      { "key": "saleCode",      "label": "Código de venta",   "defaultFormat": "text",     "applicableSections": ["Header","Summary","Footer"] },
      { "key": "customerName",  "label": "Nombre del cliente","defaultFormat": "text",     "applicableSections": ["Header","Summary","Footer"] },
      { "key": "totalAmount",   "label": "Total",             "defaultFormat": "currency", "applicableSections": ["Header","Summary","Footer"] },
      { "key": "productName",   "label": "Descripción",       "defaultFormat": "text",     "applicableSections": ["Table"] },
      { "key": "quantity",      "label": "Cantidad",          "defaultFormat": "number",   "applicableSections": ["Table"] }
    ]
  },
  "error": 0
}
```

---

### 5. Crear plantilla
```
POST /api/reports/templates
Authorization: Bearer {token}
```
**Body:**
```json
{
  "name": "Mi Plantilla",
  "reportType": "Sales",
  "description": "Descripción opcional",
  "isDefault": false,
  "sections": [ ...array de secciones... ]
}
```

---

### 6. Actualizar plantilla
```
PUT /api/reports/templates/{id}
Authorization: Bearer {token}
```
**Body:** igual que crear (sin `reportType`, no se puede cambiar el tipo).

---

### 7. Eliminar plantilla
```
DELETE /api/reports/templates/{id}
Authorization: Bearer {token}
```
> La plantilla marcada como `isDefault` no se puede eliminar.

---

### 8. Establecer como plantilla por defecto
```
POST /api/reports/templates/{id}/set-default
Authorization: Bearer {token}
```

---

### 9. Generar HTML de vista previa ⭐ (preview exacto en iframe)
```
GET /api/reports/templates/{id}/preview-html
```
**Sin token requerido.** Devuelve `text/html; charset=utf-8` — una página HTML autocontenida con CSS inline que replica exactamente el layout visual del PDF real (mismos colores, proporciones, tabla, totales, timbrado).

Usarlo directamente en un `<iframe>` — sin librerías extra, sin conversión de bytes:
```typescript
const previewUrl = `${BASE_URL}/reports/templates/${id}/preview-html`;
// <iframe src={previewUrl} style={{ width: '100%', height: '700px', border: 'none' }} />
```

> **Para facturas CFDI**: el HTML usa el layout fijo SAT (emisor/FACTURA en dos columnas, receptor, tabla de conceptos, QR, sección de timbrado) — idéntico al PDF oficial.
> **Para otros tipos**: el HTML replica el motor de plantillas genérico (header/summary/table/footer con los campos configurados).

---

### 10. Generar PDF real (con documento real)
```
POST /api/reports/generate
Authorization: Bearer {token}
```
**Body:**
```json
{
  "templateId": 1,
  "reportType": "Sales",
  "documentIds": [42],
  "companyId": 1
}
```
**Response:** `application/pdf` binario. Descargarlo como blob y guardarlo con `<a download>` o abrirlo con `window.open()`.

---

## MODELOS TYPESCRIPT

```typescript
type SectionType  = 'Header' | 'Table' | 'Summary' | 'Footer';
type TextAlign    = 'left' | 'center' | 'right';
type FieldFormat  = 'text' | 'number' | 'currency' | 'percentage' | 'date' | 'datetime';
type ReportType   = 'Sales' | 'Delivery' | 'Quotation' | 'Purchase' | 'Inventory' | 'CashierShift' | 'Invoice';

// Campo para secciones Header / Summary / Footer
interface ReportSectionField {
  field:   string;       // key del catálogo, ej: "saleCode"
  label:   string;       // texto que se muestra como etiqueta
  bold:    boolean;
  fontSize: number;      // 8–14
  align:   TextAlign;
  format:  FieldFormat;
  inline:  boolean;      // true = misma fila que el campo anterior (grid 2 columnas)
}

// Columna para secciones Table
interface ReportTableColumn {
  field:  string;
  label:  string;
  width:  number;        // ancho en puntos PDF (0 = relativo/auto)
  align:  TextAlign;
  format: FieldFormat;
  bold:   boolean;
}

// Sección dentro de la plantilla
interface ReportSectionDefinition {
  type:      SectionType;
  title:     string;
  order:     number;
  showTitle: boolean;
  fields:    ReportSectionField[];   // Header / Summary / Footer
  columns:   ReportTableColumn[];    // Table
}

// Plantilla completa (usada en editor y guardado)
interface ReportTemplate {
  id?:            number;
  name:           string;
  reportType:     ReportType;
  description?:   string;
  isDefault:      boolean;
  isActive?:      boolean;
  companyId?:     number | null;
  createdByName?: string | null;
  createdAt?:     string;
  updatedAt?:     string | null;
  sections:       ReportSectionDefinition[];
}

// Resumen de plantilla (en listado)
interface ReportTemplateSummary {
  id:          number;
  name:        string;
  reportType:  ReportType;
  description?: string;
  isDefault:   boolean;
  createdAt:   string;
}

// Respuesta del endpoint preview-data
interface ReportPreviewData {
  templateName:  string;
  reportType:    ReportType;
  sections:      ReportSectionDefinition[];
  mockDataRow:   Record<string, string>;       // campo → valor formateado
  mockTableRows: Record<string, string>[];     // array de filas
}

// Campo del catálogo
interface FieldDefinition {
  key:                 string;
  label:               string;
  defaultFormat:       FieldFormat;
  applicableSections:  SectionType[];
  description?:        string;
}
```

---

## ARQUITECTURA DE PANTALLAS

### Pantalla 1 — Lista de plantillas (`ReportTemplatesScreen`)
- Carga `GET /api/reports/templates` al montar
- Agrupa las plantillas por `reportType` (Sales, Delivery, etc.)
- Card por plantilla: nombre, badge "Por defecto", fecha de creación
- Acciones por card: Editar / Eliminar / Establecer como default
- No se puede eliminar la plantilla `isDefault`
- Botón flotante "+ Nueva plantilla"

---

### Pantalla 2 — Editor de plantilla (`ReportTemplateEditorScreen`)

**Paso 1 — Configuración básica:**
- Input: Nombre de la plantilla
- Picker: Tipo de reporte (`ReportType`)
  - Al cambiar el tipo → llama `GET /api/reports/fields/{type}` para cargar el catálogo
  - Tipos disponibles: `Sales`, `Delivery`, `Quotation`, `Purchase`, `Inventory`, `CashierShift`, `Invoice`
- Textarea: Descripción (opcional)
- Botón "Siguiente"

**Paso 2 — Constructor de secciones:**
- Lista vertical de secciones (drag & drop para reordenar)
- Botón "+ Agregar sección" → Modal/Dropdown con opciones: Header / Table / Summary / Footer
- Click en sección → abre editor de esa sección
- Botón "Siguiente" → va al Paso 3

**Paso 2b — Editor de sección (Modal o panel lateral):**

Para `Header`, `Summary`, `Footer`:
- Input: Título de la sección
- Toggle: Mostrar título (`showTitle`)
- Lista de campos actuales con controles:
  - Picker: Campo del catálogo (solo los de `applicableSections` que incluya el tipo de sección)
  - Input: Label personalizado
  - Toggle: Negrita
  - `<input type="range">` o `<input type="number">`: FontSize (8–14)
  - Segmented: Alineación (left / center / right)
  - Picker: Formato (text / number / currency / percentage / date / datetime)
  - Toggle: Inline (mostrar junto al campo anterior)
- Botón "+ Agregar campo"

Para `Table`:
- Input: Título de la sección
- Toggle: Mostrar título
- Lista de columnas con controles:
  - Picker: Campo (solo `applicableSections` incluye `"Table"`)
  - Input: Label
  - Stepper: Width en puntos (0 = auto)
  - Toggle: Negrita
  - Segmented: Alineación
  - Picker: Formato
- Botón "+ Agregar columna"

**Paso 3 — Guardar:**
- Toggle: "Establecer como plantilla por defecto"
- Botón "Guardar" → `POST` si es nueva, `PUT` si es edición

---

### Pantalla 3 — Preview HTML en iframe (`ReportPreviewScreen`)

> El backend genera un HTML con el layout **idéntico** al PDF real — mismos colores, estructura, tabla de conceptos, totales y sección de timbrado. Sin librerías externas: solo un `<iframe>`.

**Cómo funciona:**

- `GET /api/reports/templates/{id}/preview-html` devuelve una página HTML autocontenida (`text/html`).
- El frontend la carga en un `<iframe>` — cero conversión de bytes, sin dependencias extra.
- Para **plantillas guardadas**: carga `preview-html` directamente con el ID.
- Para **plantillas nuevas** (sin ID): guarda temporalmente con `POST`, carga el preview, luego el usuario confirma o cancela.

**Componente `HtmlPreviewPanel`:**

```tsx
import { useState } from 'react';

interface HtmlPreviewPanelProps {
  templateId: number;
  visible: boolean;
}

function HtmlPreviewPanel({ templateId, visible }: HtmlPreviewPanelProps) {
  const [loading, setLoading] = useState(true);
  const previewUrl = `${BASE_URL}/reports/templates/${templateId}/preview-html`;

  if (!visible) return null;

  return (
    <div style={{ flex: 1, background: '#d0d0d0', display: 'flex', flexDirection: 'column' }}>
      {loading && (
        <div style={{ textAlign: 'center', padding: 16 }}>Cargando preview...</div>
      )}
      <iframe
        src={previewUrl}
        style={{
          width: '100%',
          height: '700px',
          border: 'none',
          display: loading ? 'none' : 'block',
        }}
        onLoad={() => setLoading(false)}
        onError={() => console.warn('Preview error')}
      />
    </div>
  );
}
```

**Flujo de UX recomendado:**

1. Editor abierto → preview React básico en tiempo real (sin llamadas al backend)
2. Botón "Ver preview exacto" → abre modal con `HtmlPreviewPanel`
3. Modal con el HTML renderizado en iframe + botón "Cerrar" para volver a editar

**Nota sobre facturas CFDI**: Para tipo `Invoice`, el HTML usa el layout oficial SAT (idéntico al PDF timbrado) — no el motor genérico de secciones.

---

**Componente `ReportPreview`** *(fallback ligero durante edición — antes de guardar la plantilla)*:

```tsx
// Preview simplificado en React � solo mientras se edita antes de tener ID.
// Una vez guardada la plantilla, usa GET /preview-html para el preview exacto.
function ReportPreview({ sections, mockDataRow, mockTableRows, templateName }: ReportPreviewProps) {
  const ordered = [...sections].sort((a, b) => a.order - b.order);
  return (
    <div className="preview-container" style={{ padding: 16, background: '#fff', minWidth: 600 }}>
      <h2 style={{ textAlign: 'center' }}>{templateName}</h2>
      {ordered.map((section, idx) =>
        section.type === 'Table'
          ? <PreviewTableSection key={idx} section={section} rows={mockTableRows} />
          : <PreviewKeyValueSection key={idx} section={section} data={mockDataRow} />
      )}
    </div>
  );
}
```

---

## MOCK DATA LOCAL (para plantillas nuevas)

Cuando se crea una plantilla nueva (sin ID), usa este objeto para el preview sin llamar al backend:

```typescript
const MOCK_DATA: Record<string, { row: Record<string, string>; tableRows: Record<string, string>[] }> = {
  Sales: {
    row: {
      saleCode: 'VTA-00042', saleDate: '18/04/2026 10:35', saleType: 'Punto de venta',
      customerName: 'María García López', customerTaxId: 'GALM800101ABC',
      sellerName: 'Carlos Ramírez', branchName: 'Sucursal Centro',
      totalSubtotal: '$1,250.00', totalDiscount: '$62.50', totalTax: '$190.00',
      totalAmount: '$1,377.50', amountPaid: '$1,400.00', changeAmount: '$22.50',
      paymentMethods: 'Efectivo',
    },
    tableRows: [
      { productCode: 'PROD-001', productName: 'Laptop Lenovo IdeaPad 15"', quantity: '1', unitPrice: '$950.00', discountPercentage: '5%', taxAmount: '$144.40', lineTotal: '$1,046.90' },
      { productCode: 'PROD-017', productName: 'Mouse inalámbrico Logitech',  quantity: '2', unitPrice: '$175.00', discountPercentage: '0%', taxAmount: '$56.00',  lineTotal: '$406.00'   },
    ],
  },
  Delivery: {
    row: {
      saleCode: 'VTA-00042', saleDate: '18/04/2026 10:35', saleType: 'Entrega a domicilio',
      customerName: 'María García López', deliveryAddress: 'Av. Juárez 123, Col. Centro, Gdl.',
      scheduledDate: '20/04/2026', deliveredAt: '20/04/2026 14:20',
      sellerName: 'Carlos Ramírez', branchName: 'Sucursal Centro',
      totalAmount: '$1,377.50', paymentMethods: 'Efectivo',
    },
    tableRows: [
      { productCode: 'PROD-001', productName: 'Laptop Lenovo IdeaPad 15"', quantity: '1', unitPrice: '$950.00', lineTotal: '$1,046.90' },
      { productCode: 'PROD-017', productName: 'Mouse inalámbrico Logitech',  quantity: '2', unitPrice: '$175.00', lineTotal: '$406.00'   },
    ],
  },
  Quotation: {
    row: {
      quotationCode: 'COT-00015', quotationDate: '18/04/2026 09:00', validUntil: '25/04/2026',
      status: 'Pendiente', customerName: 'Empresa Constructora ABC', customerTaxId: 'ECA900101XYZ',
      sellerName: 'Laura Sánchez', branchName: 'Sucursal Norte',
      totalSubtotal: '$4,500.00', totalDiscount: '$225.00', totalTax: '$675.00', totalAmount: '$4,950.00',
    },
    tableRows: [
      { productCode: 'MAT-201', productName: 'Cemento Portland 50kg',       quantity: '100', unitPrice: '$18.00', lineTotal: '$1,983.60' },
      { productCode: 'MAT-312', productName: 'Varilla de acero 3/8" × 12m', quantity: '50',  unitPrice: '$58.00', lineTotal: '$3,167.60' },
    ],
  },
  Purchase: {
    row: {
      poCode: 'OC-00008', poDate: '18/04/2026 08:00', status: 'Enviada',
      supplierName: 'Distribuidora Nacional S.A.', supplierTaxId: 'DNA850601MNO',
      warehouseName: 'Almacén Central', notes: 'Entrega en 3-5 días hábiles',
      totalAmount: '$8,750.00',
    },
    tableRows: [
      { productCode: 'COMP-10', productName: 'Pantalla LED 24" Full HD', quantityOrdered: '10', quantityReceived: '0', unitCost: '$420.00', lineTotal: '$4,200.00' },
      { productCode: 'COMP-22', productName: 'Disco SSD 500GB',           quantityOrdered: '15', quantityReceived: '0', unitCost: '$302.00', lineTotal: '$4,530.00' },
    ],
  },
  Inventory: {
    row: {
      reportDate: '18/04/2026 12:00', warehouseName: 'Almacén Central',
      companyName: 'Empresa Demo S.A. de C.V.',
      totalProducts: '248', belowMinimum: '12', totalStockValue: '$384,200.00',
    },
    tableRows: [
      { productCode: 'PROD-001', productName: 'Laptop Lenovo IdeaPad 15"', category: 'Electrónica', currentStock: '8',  minimumStock: '5',  unitCost: '$950.00', totalValue: '$7,600.00' },
      { productCode: 'PROD-017', productName: 'Mouse inalámbrico Logitech', category: 'Periféricos', currentStock: '3',  minimumStock: '10', unitCost: '$175.00', totalValue: '$525.00'   },
      { productCode: 'PROD-043', productName: 'Teclado USB compacto',       category: 'Periféricos', currentStock: '22', minimumStock: '10', unitCost: '$125.00', totalValue: '$2,750.00' },
    ],
  },
  CashierShift: {
    row: {
      shiftCode: 'TRN-20260418-01', cashierName: 'Ana Martínez',
      openedAt: '18/04/2026 08:00', closedAt: '18/04/2026 16:00',
      warehouseName: 'Caja 1 - Planta Baja',
      openingCash: '$500.00', closingCash: '$2,830.00', difference: '$0.00',
      cashTotal: '$1,450.00', cardTotal: '$780.00', transferTotal: '$100.00',
      totalSales: '$2,330.00', salesCount: '18',
    },
    tableRows: [
      { saleCode: 'VTA-00040', saleTime: '08:45', customerName: 'Público general', paymentMethod: 'Efectivo', saleTotal: '$320.00'    },
      { saleCode: 'VTA-00041', saleTime: '09:12', customerName: 'Roberto Flores',  paymentMethod: 'Tarjeta',  saleTotal: '$780.00'    },
      { saleCode: 'VTA-00042', saleTime: '10:35', customerName: 'María García',    paymentMethod: 'Efectivo', saleTotal: '$1,377.50'  },
    ],
  },
  Invoice: {
    row: {
      invoiceFolio: '1001', invoiceSerie: 'A', invoiceDate: '18/04/2026 10:35',
      invoiceStatus: 'Timbrada', uuid: 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890',
      timbradoAt: '18/04/2026 10:36', tipoDeComprobante: 'I - Ingreso',
      metodoPago: 'PUE - Pago en una sola exhibición', formaPago: '01 - Efectivo',
      condicionesDePago: 'Contado', moneda: 'MXN', lugarExpedicion: '44100',
      emisorRfc: 'EDE901231XX9', emisorNombre: 'Empresa Demo S.A. de C.V.',
      emisorRegimenFiscal: '601 - General de Ley Personas Morales',
      receptorRfc: 'GALM800101ABC', receptorNombre: 'María García López',
      receptorDomicilioFiscal: '44200',
      receptorRegimenFiscal: '612 - Personas Físicas con Actividades Empresariales',
      receptorUsoCfdi: 'G03 - Gastos en general', saleCode: 'VTA-00042',
      subTotal: '$1,250.00', discountAmount: '$62.50', taxAmount: '$190.00', total: '$1,377.50',
    },
    tableRows: [
      { claveProdServ: '43211503', noIdentificacion: 'PROD-001', descripcion: 'Laptop Lenovo IdeaPad 15"', cantidad: '1', claveUnidad: 'H87', unidad: 'Pieza', valorUnitario: '$950.00', descuento: '$47.50', importe: '$902.50', trasladoTasa: '16%', trasladoImporte: '$144.40' },
      { claveProdServ: '43211706', noIdentificacion: 'PROD-017', descripcion: 'Mouse inalámbrico Logitech', cantidad: '2', claveUnidad: 'H87', unidad: 'Pieza', valorUnitario: '$175.00', descuento: '$0.00', importe: '$350.00', trasladoTasa: '16%', trasladoImporte: '$56.00' },
      { claveProdServ: '43211706', noIdentificacion: 'PROD-043', descripcion: 'Teclado USB compacto', cantidad: '1', claveUnidad: 'H87', unidad: 'Pieza', valorUnitario: '$125.00', descuento: '$0.00', importe: '$125.00', trasladoTasa: '16%', trasladoImporte: '$20.00' },
    ],
  },
};
```

---

## FLUJO DE ESTADOS DEL EDITOR

```typescript
// Estado global del editor
interface EditorState {
  // Meta
  templateId:  number | null;     // null = nueva plantilla
  name:        string;
  reportType:  ReportType | null;
  description: string;
  isDefault:   boolean;

  // Secciones (estado mutable mientras edita)
  sections:    ReportSectionDefinition[];

  // Datos para el preview (cargados al abrir plantilla existente o al elegir tipo)
  previewMockRow:   Record<string, string>;
  previewMockTable: Record<string, string>[];

  // Catálogo de campos del tipo seleccionado
  fieldCatalog: FieldDefinition[];
}
```

**Reglas de estado:**
- Al seleccionar `reportType` en Paso 1 → llama `GET /api/reports/fields/{type}` y carga `fieldCatalog`; carga `MOCK_DATA[type]` en `previewMockRow/Table`
- Al abrir plantilla existente → llama `GET /api/reports/templates/{id}/preview-data` y setea `sections` + `previewMockRow/Table` desde la respuesta
- Cada cambio en `sections` → `ReportPreview` re-renderiza automáticamente (live preview sin llamadas al backend)
- Al guardar → si `templateId` es null → `POST`, si no → `PUT`

---

## GENERACIÓN DEL PDF

```typescript
// Descarga el PDF directamente en el navegador / Electron
const generateAndDownloadPdf = async (
  templateId: number,
  documentId: number,
  reportType: ReportType,
  fileName = `reporte_${documentId}.pdf`,
) => {
  const response = await fetch(`${BASE_URL}/reports/generate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify({
      templateId,
      reportType,
      documentIds: [documentId],
      companyId: currentCompanyId,
    }),
  });

  if (!response.ok) throw new Error('Error al generar el PDF');

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);

  // Opción 1: descarga automática
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  a.click();
  URL.revokeObjectURL(url);

  // Opción 2: abrir en pestaña nueva (para previsualizar)
  // window.open(url, '_blank');
};
```

---

## VALIDACIONES

- Nombre de plantilla: no puede estar vacío
- Tipo de reporte: requerido
- Al menos una sección
- Cada sección debe tener al menos 1 campo (para Header/Summary/Footer) o 1 columna (para Table)
- No se puede eliminar una plantilla que sea `isDefault`
- Usar `section.fields ?? []` y `section.columns ?? []` siempre (nunca asumir que existen)

---

## LIBRERÍAS RECOMENDADAS

- `@dnd-kit/sortable` — drag & drop para reordenar secciones
- `<a download>` + `URL.createObjectURL()` — descargar PDF (nativo del navegador)
- `<iframe>` nativo — preview HTML del reporte (`GET /preview-html`) ⭐ sin librerías extra
- `@radix-ui/react-dialog` o `shadcn/ui Dialog` — modal/panel lateral para editor de sección
- `<select>` HTML nativo o `react-select` — pickers de tipo/campo/formato

