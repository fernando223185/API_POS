# Prompt — Sistema de Plantillas de Reportes (Electron + React)

## Contexto General

Eres el frontend de un sistema POS (Point of Sale) construido con **Electron + React**. El backend es una API REST en .NET con Clean Architecture.

Se implementó un **sistema completo de plantillas de reportes** que te permite:
1. Almacenar plantillas HTML personalizadas en la base de datos
2. Editar visualmente esas plantillas desde el frontend
3. Guardar los cambios de vuelta a la API
4. Generar PDFs en el backend usando Playwright/Chromium (el HTML se convierte a PDF en el servidor)

Tu responsabilidad es **controlar toda la UI de gestión y uso de estas plantillas**.

---

## Motor de Plantillas

### Antes (legacy — aún funciona)
Las plantillas usaban secciones JSON procesadas por QuestPDF. Si el campo `engine` del response es `"sections"`, es una plantilla legacy.

### Ahora (nuevo — preferido)
Las plantillas almacenan **HTML completo con sintaxis Liquid** en el campo `htmlTemplate`. El backend renderiza el HTML con los datos reales y lo convierte a PDF con Playwright (Chromium headless).

Si `engine === "html"`, la plantilla usa el nuevo motor.

### Sintaxis Liquid disponible
```liquid
{{ companyName }}         → variable del header/datos del documento
{{ customerName }}
{{ now }}                 → fecha/hora actual de generación

{% for item in items %}   → loop de líneas del documento
  {{ item.productName }}
  {{ item.quantity }}
  {{ item.unitPrice }}
  {{ item.total }}
{% endfor %}
```

---

## Tipos de Reporte disponibles

| Valor exacto   | Descripción             |
|----------------|-------------------------|
| `Sales`        | Ventas                  |
| `Quotation`    | Cotizaciones            |
| `Purchase`     | Compras                 |
| `CashierShift` | Turno de caja           |
| `Inventory`    | Inventario / Kardex     |
| `Delivery`     | Entregas / Despacho     |
| `Invoice`      | Factura electrónica CFDI|

---

## Regla de Negocio Crítica — Plantilla Activa

**Por cada tipo de reporte solo puede existir UNA plantilla activa al mismo tiempo.**

- `isDefault: true` + `isActive: true` → Esta ES la plantilla que se usa para generar PDFs
- `isDefault: false` + `isActive: false` → Esta es una plantilla inactiva/desactivada

Cuando el usuario marca una plantilla como activa (`set-default`), el backend **automáticamente desactiva todas las demás** del mismo tipo.

> Nunca muestres ambas flags por separado al usuario. Trátalo como un único estado: **"Activa"** / **"Inactiva"**.

---

## Endpoints disponibles

### Base URL
```
/api/reports
```

---

### 1. Obtener plantilla activa por tipo ⭐ (NUEVO)
```
GET /api/reports/templates/active/{type}
GET /api/reports/templates/active/{type}?companyId=1
```
Devuelve la **única** plantilla con `isDefault=true` e `isActive=true` para ese tipo.

**Uso principal:** Al momento de generar un PDF, primero llama este endpoint para saber qué plantilla está activa.

**Response:**
```json
{
  "data": {
    "id": 3,
    "name": "Factura Principal",
    "reportType": "Invoice",
    "description": "Plantilla CFDI v4",
    "isDefault": true,
    "isActive": true,
    "htmlTemplate": "<!DOCTYPE html>...",
    "engine": "html",
    "companyId": null,
    "createdByUserId": 1,
    "createdByName": "Admin",
    "createdAt": "2026-04-22T16:47:00Z",
    "updatedAt": null,
    "sections": []
  },
  "error": 0
}
```
**404** si no hay ninguna activa para ese tipo.

---

### 2. Listar plantillas
```
GET /api/reports/templates
GET /api/reports/templates?type=Invoice
GET /api/reports/templates?type=Invoice&companyId=1
```
**Response:**
```json
{
  "data": [
    { "id": 1, "name": "...", "reportType": "Invoice", "isDefault": true, "createdAt": "..." },
    { "id": 2, "name": "...", "reportType": "Invoice", "isDefault": false, "createdAt": "..." }
  ],
  "total": 2,
  "error": 0
}
```

---

### 3. Obtener plantilla por ID (con HTML completo)
```
GET /api/reports/templates/{id}
```
Igual al response de `/active/{type}` arriba. Usar cuando necesitas cargar el HTML para editar.

---

### 4. Preview HTML en vivo
```
GET /api/reports/templates/{id}/preview-html
```
Devuelve el HTML ya renderizado con **datos de ejemplo (mock)**. Úsalo para mostrar un `<webview>` o `<iframe>` dentro del editor de plantillas.

- Content-Type: `text/html; charset=utf-8`
- No requiere body

---

### 5. Preview PDF
```
GET /api/reports/templates/{id}/preview-pdf
```
Devuelve el PDF generado con datos mock. Útil para mostrar un preview del PDF antes de guardar cambios.

- Content-Type: `application/pdf`

---

### 6. Crear plantilla
```
POST /api/reports/templates
Content-Type: application/json
```
**Body:**
```json
{
  "name": "Mi plantilla personalizada",
  "reportType": "Invoice",
  "description": "Descripción opcional",
  "isDefault": false,
  "htmlTemplate": "<!DOCTYPE html><html>...</html>",
  "sections": []
}
```
> Si envías `htmlTemplate`, el campo `sections` se ignora. Solo envía uno de los dos.

---

### 7. Actualizar plantilla (guardar cambios del editor)
```
PUT /api/reports/templates/{id}
Content-Type: application/json
```
**Body igual al POST.**

> Este es el endpoint que usas cuando el usuario edita el HTML y hace "Guardar".

---

### 8. Eliminar plantilla
```
DELETE /api/reports/templates/{id}
```
> No se puede eliminar la plantilla que está activa (`isDefault=true`). El backend devuelve `400`.

---

### 9. Activar una plantilla (marcar como predeterminada)
```
POST /api/reports/templates/{id}/set-default
```
Sin body. El backend desactiva todas las demás del mismo tipo automáticamente.

**Response:**
```json
{ "message": "Plantilla marcada como predeterminada", "error": 0 }
```

---

### 10. Generar PDF real
```
POST /api/reports/generate
Content-Type: application/json
```
**Body:**
```json
{
  "templateId": 3,
  "reportType": "Invoice",
  "documentIds": [101, 102],
  "fromDate": null,
  "toDate": null,
  "warehouseId": null,
  "companyId": 1
}
```
- `templateId` es opcional. Si se omite, el backend usa la plantilla activa del tipo indicado.
- Devuelve `application/pdf` directamente.

---

### 11. Catálogo de campos disponibles por tipo
```
GET /api/reports/fields/{type}
```
Devuelve todos los campos que puedes usar como variables Liquid en la plantilla HTML.

**Response:**
```json
{
  "data": {
    "reportType": "Invoice",
    "fields": [
      { "key": "companyName", "label": "Nombre empresa", "defaultFormat": "text", "applicableSections": ["Header"] },
      { "key": "customerName", "label": "Cliente", "defaultFormat": "text", "applicableSections": ["Header"] },
      { "key": "total", "label": "Total", "defaultFormat": "currency", "applicableSections": ["Summary"] }
    ],
    "availableSectionTypes": ["Header", "Table", "Summary", "Footer"]
  },
  "error": 0
}
```
Úsalo en el editor de plantillas para mostrar un **panel de variables** que el usuario puede insertar con click.

---

## Flujo recomendado en UI

### Gestión de plantillas (pantalla de administración)
1. Carga la lista: `GET /api/reports/templates?type=Invoice`
2. Muestra cada plantilla con badge **"Activa"** si `isDefault === true`
3. Botón **"Editar"** → carga `GET /api/reports/templates/{id}` → abre editor con el HTML
4. En el editor: panel derecho con `GET /api/reports/fields/{type}` para insertar variables
5. Botón **"Preview"** dentro del editor → muestra `GET /api/reports/templates/{id}/preview-html` en un `<webview>` o iframe
6. Botón **"Guardar"** → `PUT /api/reports/templates/{id}` con el HTML actualizado
7. Botón **"Activar"** → `POST /api/reports/templates/{id}/set-default` → refresca lista
8. Botón **"Nueva plantilla"** → `POST /api/reports/templates`

### Generación de PDF (desde cualquier módulo)
```js
// Opción A: usar plantilla activa automáticamente
const pdf = await api.post('/api/reports/generate', {
  reportType: 'Invoice',
  documentIds: [saleId]
});

// Opción B: usar plantilla específica
const pdf = await api.post('/api/reports/generate', {
  templateId: 3,
  reportType: 'Invoice',
  documentIds: [saleId]
});
```
El response es un `Blob` de tipo `application/pdf`. Ábrelo con Electron:
```js
const buffer = await response.arrayBuffer();
const blob = new Blob([buffer], { type: 'application/pdf' });
const url = URL.createObjectURL(blob);
window.open(url); // o usar shell.openPath con archivo temporal
```

---

## Modelo de datos completo — `ReportTemplateResponseDto`

```ts
interface ReportTemplateResponse {
  id: number;
  name: string;
  reportType: 'Sales' | 'Quotation' | 'Purchase' | 'CashierShift' | 'Inventory' | 'Delivery' | 'Invoice';
  description: string | null;
  isDefault: boolean;       // true = esta es la activa
  isActive: boolean;        // true = esta es la activa (siempre igual a isDefault)
  htmlTemplate: string | null; // HTML Liquid completo
  engine: 'html' | 'sections'; // calculado: html si htmlTemplate != null
  sections: ReportSectionDefinition[]; // legacy, vacío si engine = "html"
  companyId: number | null;
  createdByUserId: number | null;
  createdByName: string | null;
  createdAt: string; // ISO 8601
  updatedAt: string | null;
}
```

---

## Endpoints Legacy que DEBES reemplazar

Existen endpoints de PDF dispersos en módulos individuales que fueron creados antes del sistema de plantillas. **Ya no debes llamarlos.** Todos generan PDFs con diseño hardcodeado (QuestPDF) que el usuario no puede personalizar.

### Tabla de migración

| Módulo | Endpoint ANTIGUO (no usar) | Reemplazo correcto |
|--------|----------------------------|--------------------|
| Ventas — ticket | `GET /api/sales/{id}/ticket/pdf` | `POST /api/reports/generate` con `reportType: "Sales"` |
| Ventas — factura visual | `GET /api/sales/{id}/invoice/pdf` | `POST /api/reports/generate` con `reportType: "Sales"` |
| Cotizaciones | `GET /api/quotations/{id}/pdf` | `POST /api/reports/generate` con `reportType: "Quotation"` |
| Órdenes de compra | `GET /api/purchase-orders/{id}/pdf` | `POST /api/reports/generate` con `reportType: "Purchase"` |
| Recepción de mercancía | `GET /api/purchase-order-receivings/{id}/pdf` | `POST /api/reports/generate` con `reportType: "Purchase"` |
| Turno de caja | `GET /api/cashier-shifts/{id}/export-pdf` | `POST /api/reports/generate` con `reportType: "CashierShift"` |
| Kardex / Inventario | `GET /api/inventory/kardex/.../pdf` | `POST /api/reports/generate` con `reportType: "Inventory"` |

### Llamada de reemplazo

```js
// ANTES (legacy — no usar)
const pdf = await api.get(`/api/quotations/${id}/pdf`, { responseType: 'blob' });

// AHORA (correcto)
const pdf = await api.post('/api/reports/generate', {
  reportType: 'Quotation',
  documentIds: [id]
}, { responseType: 'blob' });
```

### Excepciones — NO reemplazar

Estos dos endpoints generan **documentos fiscales CFDI timbrados** (con sello SAT, UUID, cadena original). Son documentos legales, no reportes visuales. Déjalos como están:

| Endpoint | Motivo |
|----------|--------|
| `GET /api/billing/invoices/{id}/pdf` | PDF del CFDI v4 timbrado (factura fiscal) |
| `GET /api/accounts-receivable/payments/{id}/pdf` | PDF del complemento de pago timbrado |

---

## Consideraciones de UX

- **Estado "Activa":** Muestra un indicador visual claro (badge verde, ícono de check, etc.). Solo puede haber una por tipo.
- **Editor HTML:** Usa un editor de código (Monaco Editor o CodeMirror) con syntax highlighting de HTML. Agrega un botón "Preview" que cargue el iframe/webview con el endpoint `preview-html`.
- **Variables Liquid:** En el editor, muestra un panel lateral con los campos del catálogo. Al hacer click en un campo, inserta `{{ nombreDelCampo }}` en la posición del cursor.
- **Confirmación al activar:** Antes de llamar `set-default`, muestra un mensaje: *"Al activar esta plantilla, la plantilla actual quedará desactivada. ¿Continuar?"*
- **No eliminar activa:** Si el usuario intenta eliminar la plantilla activa, muestra error antes de llamar al API (o maneja el 400 que regresa el backend).
- **Nuevo motor HTML:** Todas las plantillas sembradas por defecto ya usan `engine: "html"`. El motor legacy (`sections`) existe solo por compatibilidad con plantillas antiguas.
