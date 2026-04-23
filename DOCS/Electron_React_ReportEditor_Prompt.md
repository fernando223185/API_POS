# Prompt — Editor de Plantillas de Reportes (Electron + React)

## Objetivo

Construir una pantalla completa de **edición de plantillas HTML** para reportes/documentos PDF del sistema POS. El usuario puede ver, editar y previsualizar en tiempo real el HTML de cada plantilla antes de guardar los cambios.

---

## Contexto del sistema

- Las plantillas se almacenan en la base de datos como **HTML completo con sintaxis Liquid** (`{{ variable }}`, `{% for item in items %}`)
- El backend renderiza el HTML con datos reales y lo convierte a PDF con Playwright (Chromium headless)
- Cada tipo de reporte tiene exactamente **una plantilla activa** (`isDefault: true`, `isActive: true`)
- Pueden existir múltiples plantillas por tipo, pero solo una está activa a la vez

### Tipos disponibles

| `reportType`   | Descripción              |
|----------------|--------------------------|
| `Sales`        | Ventas / Tickets         |
| `Quotation`    | Cotizaciones             |
| `Purchase`     | Órdenes de compra        |
| `CashierShift` | Turno de caja            |
| `Inventory`    | Inventario / Kardex      |
| `Delivery`     | Entregas / Despacho      |
| `Invoice`      | Factura CFDI 4.0         |
| `Payment`      | Complemento de Pago CFDI |

---

## Flujo de la pantalla

### 1. Pantalla principal — Lista de plantillas

```
┌─────────────────────────────────────────────────────────────┐
│  Plantillas de Reportes                    [+ Nueva Plantilla] │
│                                                              │
│  Filtro: [Todos los tipos ▼]  [Buscar por nombre...]        │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🟢 ACTIVA  │ Factura CFDI Principal  │ Invoice │ [Editar] │
│  │ ⚫ INACTIVA │ Factura v2 (prueba)     │ Invoice │ [Editar] │
│  │ 🟢 ACTIVA  │ Ticket de Venta         │ Sales   │ [Editar] │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

- **🟢 ACTIVA**: `isDefault === true` — esta plantilla genera los PDFs
- **⚫ INACTIVA**: `isDefault === false`
- Solo muestra UN estado (no dos badges separados)
- Clic en **Editar** abre el editor

---

### 2. Editor de plantilla (pantalla dividida)

```
┌──────────────────────────────────────────────────────────────────┐
│ ← Factura CFDI Principal  [Invoice]  🟢 ACTIVA    [Activar] [Guardar] │
├─────────────────────────┬──────────────────────────────────────────┤
│  EDITOR HTML            │  PREVIEW EN VIVO                        │
│                         │                                          │
│  <Variables>            │  ┌────────────────────────────────────┐ │
│  {{ emisorNombre }}  ←  │  │  [iframe / webview con resultado]  │ │
│  {{ receptorNombre }}   │  │                                    │ │
│  {{ uuid }}             │  │  Logo  Empresa Demo S.A. de C.V.  │ │
│  {{ total }}            │  │  RFC: EDE901231XX9                 │ │
│  ...                    │  │                                    │ │
│                         │  │  FACTURA A-00121                   │ │
│  [Monaco Editor]        │  │  ...                               │ │
│  <!DOCTYPE html>        │  └────────────────────────────────────┘ │
│  <html>                 │                                          │
│    ...                  │  [⟳ Actualizar preview]                  │
│  </html>                │                                          │
└─────────────────────────┴──────────────────────────────────────────┘
```

---

## Endpoints que debes usar

### Base URL: `/api/reports`

---

### Cargar lista de plantillas por tipo
```http
GET /api/reports/templates?type=Invoice
```
```json
{
  "data": [
    { "id": 1, "name": "Factura Principal", "reportType": "Invoice", "isDefault": true, "createdAt": "..." },
    { "id": 2, "name": "Factura v2",        "reportType": "Invoice", "isDefault": false, "createdAt": "..." }
  ],
  "total": 2,
  "error": 0
}
```

---

### Cargar el HTML completo de una plantilla para editar
```http
GET /api/reports/templates/{id}
```
```json
{
  "data": {
    "id": 1,
    "name": "Factura Principal",
    "reportType": "Invoice",
    "isDefault": true,
    "isActive": true,
    "htmlTemplate": "<!DOCTYPE html><html>...</html>",
    "engine": "html",
    "sections": []
  },
  "error": 0
}
```
Carga `data.htmlTemplate` en el editor de código.

---

### Live Preview ⭐ (CLAVE del editor)
```http
POST /api/reports/templates/live-preview
Content-Type: application/json
```
```json
{
  "reportType": "Invoice",
  "htmlTemplate": "<!DOCTYPE html><html>... lo que hay en el editor ...</html>"
}
```
**Response:** `text/html` — el HTML ya renderizado con **datos de ejemplo (mock)**.

Cargarlo en un `<iframe>` o `<webview>` de Electron para mostrar el preview.

> **Este es el endpoint principal del editor.** No requiere guardar nada. Cada vez que el usuario quiere ver cómo quedó, llama este endpoint con el HTML actual del editor.

**Cuándo llamarlo:**
- Al abrir el editor (con el HTML guardado)
- Cuando el usuario hace clic en "Actualizar preview" / "Ver resultado"
- Opcionalmente con debounce (~1.5s) mientras el usuario escribe

---

### Guardar cambios
```http
PUT /api/reports/templates/{id}
Content-Type: application/json
```
```json
{
  "name": "Factura Principal",
  "description": "Plantilla CFDI v4",
  "isDefault": false,
  "htmlTemplate": "<!DOCTYPE html>... html editado ...",
  "sections": []
}
```
```json
{ "data": { ...plantilla actualizada... }, "error": 0 }
```

---

### Activar una plantilla
```http
POST /api/reports/templates/{id}/set-default
```
Sin body. Desactiva automáticamente todas las demás del mismo tipo.

```json
{ "message": "Plantilla marcada como predeterminada", "error": 0 }
```

---

### Crear nueva plantilla
```http
POST /api/reports/templates
Content-Type: application/json
```
```json
{
  "name": "Mi nueva plantilla",
  "reportType": "Invoice",
  "description": "Descripción opcional",
  "isDefault": false,
  "htmlTemplate": "<!DOCTYPE html><html><body>Empieza aquí</body></html>",
  "sections": []
}
```

---

### Eliminar plantilla
```http
DELETE /api/reports/templates/{id}
```
> No se puede eliminar la plantilla activa. El backend devuelve `400`. Deshabilita el botón si `isDefault === true`.

---

### Obtener variables disponibles para un tipo
```http
GET /api/reports/fields/{type}
```
```json
{
  "data": {
    "reportType": "Invoice",
    "fields": [
      { "key": "emisorNombre",  "label": "Nombre del emisor",  "defaultFormat": "text" },
      { "key": "receptorRfc",   "label": "RFC del receptor",   "defaultFormat": "text" },
      { "key": "total",         "label": "Total",              "defaultFormat": "currency" },
      { "key": "uuid",          "label": "UUID CFDI",          "defaultFormat": "text" }
    ]
  },
  "error": 0
}
```
Muestra estos campos en un **panel lateral** del editor. Al hacer clic en uno, inserta `{{ key }}` en la posición del cursor en el editor.

Para campos de tabla (dentro del `{% for item in items %}`):
```
{{ item.key }}
```

---

## Implementación del editor

### Librería recomendada: Monaco Editor

```bash
npm install @monaco-editor/react
```

```jsx
import Editor from '@monaco-editor/react';

<Editor
  height="100%"
  defaultLanguage="html"
  value={htmlContent}
  onChange={(value) => setHtmlContent(value)}
  options={{
    minimap: { enabled: false },
    fontSize: 13,
    wordWrap: 'on',
    formatOnPaste: true,
  }}
/>
```

### Live preview con iframe

```jsx
const [previewHtml, setPreviewHtml] = useState('');

const updatePreview = async () => {
  const response = await api.post('/api/reports/templates/live-preview', {
    reportType: template.reportType,
    htmlTemplate: htmlContent,
  });
  // response.data es el string HTML directamente (Content-Type: text/html)
  setPreviewHtml(response.data);
};

// Renderizar en iframe via blob URL (recomendado en Electron)
const previewUrl = useMemo(() => {
  if (!previewHtml) return '';
  const blob = new Blob([previewHtml], { type: 'text/html' });
  return URL.createObjectURL(blob);
}, [previewHtml]);

<iframe src={previewUrl} style={{ width: '100%', height: '100%', border: 'none' }} />
```

> En Electron usa `<webview>` en lugar de `<iframe>` si necesitas mayor aislamiento.

### Guardar con confirmación

```jsx
const handleSave = async () => {
  await api.put(`/api/reports/templates/${template.id}`, {
    name: template.name,
    description: template.description,
    isDefault: template.isDefault,
    htmlTemplate: htmlContent,
    sections: [],
  });
  toast.success('Plantilla guardada');
};
```

### Activar con confirmación

```jsx
const handleActivate = async () => {
  const confirmed = await confirm(
    `Al activar "${template.name}", la plantilla actualmente activa para ${template.reportType} quedará desactivada. ¿Continuar?`
  );
  if (!confirmed) return;

  await api.post(`/api/reports/templates/${template.id}/set-default`);
  // Refresca la lista
};
```

---

## Panel de Variables (insertar en el cursor)

```jsx
const insertVariable = (key, isTableField = false) => {
  const snippet = isTableField ? `{{ item.${key} }}` : `{{ ${key} }}`;
  // Con Monaco Editor:
  editorRef.current.trigger('keyboard', 'type', { text: snippet });
};

// Renderizado del panel
fields.map(field => (
  <button key={field.key} onClick={() => insertVariable(field.key)}>
    <span className="field-key">{{ {field.key} }}</span>
    <span className="field-label">{field.label}</span>
  </button>
))
```

---

## Tipos TypeScript

```ts
interface ReportSectionField {
  field: string;
  label: string;
  bold: boolean;
  fontSize: number;
  align: 'left' | 'center' | 'right';
  format: 'text' | 'currency' | 'date' | 'datetime' | 'number' | 'percentage' | 'image';
  inline: boolean;
  /** true = se muestra en el PDF. false = disponible para agregar pero actualmente oculto */
  visible: boolean;
}

interface ReportTableColumn {
  field: string;
  label: string;
  width: number;
  align: 'left' | 'center' | 'right';
  format: 'text' | 'currency' | 'date' | 'datetime' | 'number' | 'percentage' | 'image';
  bold: boolean;
  /** true = se muestra en el PDF. false = disponible para agregar pero actualmente oculto */
  visible: boolean;
}

interface ReportSectionDefinition {
  /** Identificador único de la sección. Coincide con id="sec-{sectionId}" en el htmlTemplate */
  sectionId: string;
  type: 'Header' | 'Table' | 'Summary' | 'Footer';
  title: string;
  order: number;
  /** true = se muestra en el PDF. false = disponible para agregar pero actualmente oculta */
  visible: boolean;
  showTitle: boolean;
  fields: ReportSectionField[];    // para Header / Summary / Footer
  columns: ReportTableColumn[];    // para Table
  titleBackground?: string;
  titleColor?: string;
  bodyBackground?: string;
  borderColor?: string;
  variant?: string;
}

interface ReportTemplate {
  id: number;
  name: string;
  reportType: ReportType;
  description: string | null;
  isDefault: boolean;   // true = activa
  isActive: boolean;    // siempre igual a isDefault
  htmlTemplate: string | null;
  /** Configuración de secciones y campos. visible=true → se muestra; visible=false → disponible para agregar */
  sections: ReportSectionDefinition[];
  engine: 'html' | 'sections';
  companyId: number | null;
  createdByUserId: number | null;
  createdByName: string | null;
  createdAt: string;
  updatedAt: string | null;
}

type ReportType =
  | 'Sales'
  | 'Quotation'
  | 'Purchase'
  | 'CashierShift'
  | 'Inventory'
  | 'Delivery'
  | 'Invoice'
  | 'Payment';

interface LivePreviewRequest {
  reportType: ReportType;
  htmlTemplate: string;
}

interface UpdateTemplateRequest {
  name: string;
  description?: string;
  isDefault: boolean;
  htmlTemplate: string;
  /** Enviar el array sections actualizado para guardar la configuración de visibilidad */
  sections: ReportSectionDefinition[];
}
```

---

## Panel de configuración de campos (sections)

`sections` te permite construir un panel de "agregar / quitar" información del PDF sin tocar el HTML.

```ts
// Obtener campos visibles
const visibleFields = section.fields.filter(f => f.visible);
// Obtener campos que se pueden agregar
const hiddenFields  = section.fields.filter(f => !f.visible);

// Activar un campo
const toggleField = (sectionIdx: number, fieldKey: string, show: boolean) => {
  setSections(prev => prev.map((s, i) => i !== sectionIdx ? s : {
    ...s,
    fields: s.fields.map(f => f.field === fieldKey ? { ...f, visible: show } : f),
  }));
};

// Activar una columna de tabla
const toggleColumn = (sectionIdx: number, fieldKey: string, show: boolean) => {
  setSections(prev => prev.map((s, i) => i !== sectionIdx ? s : {
    ...s,
    columns: s.columns.map(c => c.field === fieldKey ? { ...c, visible: show } : c),
  }));
};
```

Al guardar (`PUT /api/reports/templates/{id}`), envía el `sections` actualizado en el body. El backend lo persiste tal cual.

> **Importante**: cambiar `visible` en `sections` **no modifica automáticamente el `htmlTemplate`**. Los dos campos son independientes. Para ocultar contenido en el HTML generado, usa inyección de CSS (ver sección siguiente).

---

## Ocultar secciones / campos / columnas con CSS (motor HTML)

Cada elemento del `htmlTemplate` tiene atributos de targeting que coinciden con `sectionId`, `field` y columnas de tabla:

| Elemento | Atributo HTML | Selector CSS |
|---|---|---|
| Sección wrapper | `id="sec-{sectionId}"` | `#sec-{sectionId}` |
| Div de campo | `data-field="{field}"` | `[data-field="{field}"]` |
| `<th>` / `<td>` de tabla | `class="col-{field}"` | `.col-{field}` |

```ts
// Generar CSS para ocultar una sección completa
const hideSectionCss = (sectionId: string) =>
  `#sec-${sectionId} { display: none !important; }`;

// Generar CSS para ocultar un campo específico
const hideFieldCss = (fieldKey: string) =>
  `[data-field="${fieldKey}"] { display: none !important; }`;

// Generar CSS para ocultar una columna de tabla (th + td)
const hideColumnCss = (colKey: string) =>
  `.col-${colKey} { display: none !important; }`;

// Construir el <style> completo a partir del estado de sections
const buildVisibilityCss = (sections: ReportSectionDefinition[]): string => {
  const rules: string[] = [];
  for (const sec of sections) {
    if (!sec.visible) {
      rules.push(hideSectionCss(sec.sectionId));
      continue; // si la sección está oculta, no necesitamos revisar sus campos
    }
    for (const f of sec.fields) {
      if (!f.visible) rules.push(hideFieldCss(f.field));
    }
    for (const c of sec.columns) {
      if (!c.visible) rules.push(hideColumnCss(c.field));
    }
  }
  return rules.join('\n');
};

// Inyectar en el htmlTemplate antes de enviarlo al live-preview o al PDF
const injectVisibility = (html: string, sections: ReportSectionDefinition[]): string => {
  const css = buildVisibilityCss(sections);
  if (!css) return html;
  return html.replace('</style>', `\n/* visibility overrides */\n${css}\n</style>`);
};
```

### sectionId por tipo de reporte

| Reporte | sectionId | Descripción |
|---|---|---|
| Sales | `doc` | Encabezado del documento (folio, fecha, vendedor) |
| Sales | `cliente` | Datos del cliente |
| Sales | `productos` | Tabla de productos |
| Sales | `totales` | Totales y resumen |
| Quotation | `encabezado` | Encabezado de la cotización |
| Quotation | `cliente` | Datos del cliente |
| Quotation | `partidas` | Partidas cotizadas |
| Quotation | `totales` | Totales |
| Purchase | `doc` | Encabezado + datos del proveedor |
| Purchase | `partidas` | Partidas de la OC |
| Purchase | `totales` | Totales de la OC |
| CashierShift | `encabezado` | Encabezado del turno |
| CashierShift | `ventas` | Ventas del turno |
| CashierShift | `totales` | Desglose por forma de pago |
| Inventory | `kardex` | Encabezado del kardex |
| Inventory | `movimientos` | Movimientos de inventario |
| Delivery | `entrega` | Encabezado + datos de entrega |
| Delivery | `productos` | Artículos a entregar |
| Delivery | `totales` | Totales |
| Invoice | `comprobante` | Encabezado del CFDI |
| Invoice | `receptor` | Datos del receptor |
| Invoice | `conceptos` | Conceptos / partidas |
| Invoice | `pago` | Forma de pago y totales |
| Invoice | `timbrado` | Sello y QR del SAT |
| Payment | `emisor` | Datos del emisor |
| Payment | `receptor` | Datos del receptor |
| Payment | `pago` | Resumen del pago |
| Payment | `facturas` | Facturas aplicadas |
| Payment | `timbrado` | Sello y QR del SAT |



---

## UX — Puntos importantes

- **Botón "Activar"** solo visible cuando `isDefault === false`. Si ya está activa, muestra solo un badge informativo.
- **Botón "Eliminar"** deshabilitado cuando `isDefault === true`. Tooltip: "No puedes eliminar la plantilla activa".
- **Indicador de cambios sin guardar**: detecta si el contenido del editor difiere del `htmlTemplate` cargado originalmente → muestra un punto/asterisco en el título.
- **Preview inicial**: al abrir el editor, carga automáticamente el preview con el HTML guardado (llama `live-preview` al montar).
- **Debounce en el editor**: si decides actualizar el preview automáticamente, usa 1500ms de debounce para no saturar el backend con cada tecla.
- **Separar editor y preview** con un divisor arrastrable (split pane) — el HTML suele ser largo.
