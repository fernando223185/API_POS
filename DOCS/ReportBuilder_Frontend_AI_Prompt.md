# Prompt para AI Frontend — Sistema de Report Builder

> Usa este prompt directamente en tu herramienta de AI para frontend (Cursor, Copilot Chat, Claude, etc.)

---

## PROMPT

Necesito que implementes un **Report Builder** completo en React Native (con Expo + TypeScript) para una app POS. El backend ya está construido. A continuación te doy todos los detalles del API, modelos y UX esperada.

---

## BASE URL

```
https://<tu-dominio>/api
```

---

## ENDPOINTS DISPONIBLES

### 1. Obtener catálogo de campos disponibles
```
GET /api/reports/fields/{reportType}
```
`reportType` puede ser: `Sales`, `Delivery`, `Quotation`, `Purchase`, `Inventory`, `CashierShift`

**Response:**
```json
{
  "success": true,
  "data": {
    "reportType": "Sales",
    "fields": [
      { "field": "saleCode",      "label": "N° de Venta",    "defaultFormat": "Text",    "section": "Header"  },
      { "field": "customerName",  "label": "Cliente",        "defaultFormat": "Text",    "section": "Header"  },
      { "field": "total",         "label": "Total",          "defaultFormat": "Currency","section": "Summary" },
      { "field": "productName",   "label": "Producto",       "defaultFormat": "Text",    "section": "Table"   },
      { "field": "quantity",      "label": "Cantidad",       "defaultFormat": "Number",  "section": "Table"   },
      { "field": "unitPrice",     "label": "Precio Unit.",   "defaultFormat": "Currency","section": "Table"   },
      { "field": "subtotal",      "label": "Subtotal",       "defaultFormat": "Currency","section": "Table"   }
    ]
  }
}
```

---

### 2. Listar plantillas de un tipo
```
GET /api/reports/templates?type=Sales&companyId=1
```
**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Reporte de Ventas Estándar",
      "reportType": "Sales",
      "isDefault": true,
      "isActive": true,
      "description": "Plantilla por defecto",
      "companyId": 1,
      "createdAt": "2025-01-15T10:00:00Z"
    }
  ]
}
```

---

### 3. Obtener plantilla por ID (con secciones completas)
```
GET /api/reports/templates/{id}
```
**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Reporte de Ventas Estándar",
    "reportType": "Sales",
    "isDefault": true,
    "isActive": true,
    "description": "Plantilla por defecto",
    "companyId": 1,
    "sections": [
      {
        "type": "Header",
        "title": "Información de la Venta",
        "columns": 2,
        "fields": [
          { "field": "saleCode",     "label": "N° de Venta", "fontSize": 10, "bold": true,  "align": "Left", "format": "Text"     },
          { "field": "customerName", "label": "Cliente",     "fontSize": 10, "bold": false, "align": "Left", "format": "Text"     },
          { "field": "saleDate",     "label": "Fecha",       "fontSize": 10, "bold": false, "align": "Left", "format": "Date"     },
          { "field": "total",        "label": "Total",       "fontSize": 11, "bold": true,  "align": "Right","format": "Currency" }
        ],
        "tableColumns": []
      },
      {
        "type": "Table",
        "title": "Detalle de Productos",
        "columns": 1,
        "fields": [],
        "tableColumns": [
          { "field": "productCode", "label": "Código",   "widthPercent": 15, "bold": false, "align": "Left",  "format": "Text"     },
          { "field": "productName", "label": "Producto", "widthPercent": 40, "bold": false, "align": "Left",  "format": "Text"     },
          { "field": "quantity",    "label": "Cant.",    "widthPercent": 10, "bold": false, "align": "Center","format": "Number"   },
          { "field": "unitPrice",   "label": "P.Unit",  "widthPercent": 15, "bold": false, "align": "Right", "format": "Currency" },
          { "field": "subtotal",    "label": "Subtotal", "widthPercent": 20, "bold": true,  "align": "Right", "format": "Currency" }
        ]
      },
      {
        "type": "Summary",
        "title": "Resumen",
        "columns": 1,
        "fields": [
          { "field": "subtotal",    "label": "Subtotal",  "fontSize": 10, "bold": false, "align": "Right", "format": "Currency" },
          { "field": "tax",         "label": "IVA",       "fontSize": 10, "bold": false, "align": "Right", "format": "Currency" },
          { "field": "total",       "label": "TOTAL",     "fontSize": 12, "bold": true,  "align": "Right", "format": "Currency" }
        ],
        "tableColumns": []
      }
    ]
  }
}
```

---

### 4. Crear plantilla
```
POST /api/reports/templates
Content-Type: application/json
Authorization: Bearer {token}
```
**Body:** (mismo formato que el response de GET por ID, sin `id`/`createdAt`)
```json
{
  "name": "Mi Plantilla Personalizada",
  "reportType": "Sales",
  "description": "Plantilla sin IVA para clientes especiales",
  "companyId": 1,
  "sections": [ ...array de secciones... ]
}
```

---

### 5. Actualizar plantilla
```
PUT /api/reports/templates/{id}
Content-Type: application/json
Authorization: Bearer {token}
```
Body igual que crear.

---

### 6. Eliminar plantilla
```
DELETE /api/reports/templates/{id}
Authorization: Bearer {token}
```

---

### 7. Establecer como plantilla por defecto
```
POST /api/reports/templates/{id}/set-default
Authorization: Bearer {token}
```

---

### 8. Generar PDF con una plantilla
```
POST /api/reports/generate
Content-Type: application/json
Authorization: Bearer {token}
```
**Body:**
```json
{
  "templateId": 1,
  "reportType": "Sales",
  "entityId": 42,
  "companyId": 1,
  "title": "Reporte de Venta SLA-00042"
}
```
**Response:** `application/pdf` (binario) — abrir con `expo-print`, `expo-sharing`, o `react-native-pdf`.

---

## MODELOS TYPESCRIPT

```typescript
// Tipos de sección
type SectionType = 'Header' | 'Table' | 'Summary' | 'Footer';
type TextAlign   = 'Left' | 'Center' | 'Right';
type FieldFormat = 'Text' | 'Number' | 'Currency' | 'Percentage' | 'Date' | 'DateTime';

interface ReportSectionField {
  field:    string;
  label:    string;
  fontSize: number;      // 8-14, default 10
  bold:     boolean;
  align:    TextAlign;
  format:   FieldFormat;
}

interface ReportTableColumn {
  field:        string;
  label:        string;
  widthPercent: number;  // suma de todas las columnas debe ser ~100
  bold:         boolean;
  align:        TextAlign;
  format:       FieldFormat;
}

interface ReportSectionDefinition {
  type:         SectionType;
  title:        string;
  columns:      number;       // para Header/Summary: número de columnas del grid (1 o 2)
  fields:       ReportSectionField[];
  tableColumns: ReportTableColumn[];
}

interface ReportTemplate {
  id?:         number;
  name:        string;
  reportType:  string;
  isDefault?:  boolean;
  isActive?:   boolean;
  description: string;
  companyId:   number;
  sections:    ReportSectionDefinition[];
}

interface FieldDefinition {
  field:         string;
  label:         string;
  defaultFormat: FieldFormat;
  section:       SectionType;
}
```

---

## PANTALLAS A IMPLEMENTAR

### Pantalla 1: Lista de plantillas (`ReportTemplatesScreen`)
- Lista de cards con nombre, tipo, badge "Por defecto"
- Botón "+ Nueva plantilla"
- Swipe left para eliminar
- Tap en card → va a pantalla de edición
- Botón "⭐ Establecer como default" en cada card

### Pantalla 2: Editor de plantilla (`ReportTemplateEditorScreen`)
Flujo en 3 pasos:

**Paso 1 — Configuración básica:**
- Input: Nombre
- Picker: Tipo de reporte (Sales / Delivery / Quotation / Purchase / Inventory / CashierShift)
- Textarea: Descripción
- Botón "Siguiente"

**Paso 2 — Secciones:**
- Lista de secciones añadidas (Header, Table, Summary, Footer)
- Botón "+ Agregar sección" → BottomSheet para elegir tipo
- Tap en sección → abre editor de sección (Paso 2b)
- Drag & drop para reordenar secciones (react-native-draggable-flatlist)
- Botón "Siguiente"

**Paso 2b — Editor de sección:**
- Título de sección (input)
- Si es `Header` o `Summary`:
  - Columns: Segmented control (1 col / 2 col)
  - Lista de campos (`fields`) con sus opciones:
    - Picker: Campo disponible (del catálogo de `/api/reports/fields/{type}`)
    - Input: Label personalizado
    - Slider: FontSize (8-14)
    - Toggle: Negrita
    - Segmented control: Alineación (Left/Center/Right)
    - Picker: Formato (Text/Number/Currency/Date/DateTime)
  - Botón "+ Agregar campo"
- Si es `Table`:
  - Lista de columnas (`tableColumns`) con sus opciones
  - Stepper de WidthPercent por columna (suma total mostrada)
  - Los mismos controles de align, format, bold

**Paso 3 — Guardar:**
- Resumen visual de la plantilla
- Toggle: "Establecer como plantilla por defecto"
- Botón "Guardar plantilla" → llama POST o PUT

### Pantalla 3: Preview / Generación (`ReportPreviewScreen`)
- Picker: tipo de reporte + ID de entidad
- Picker: plantilla a usar (cargada del GET /templates?type=...)
- Botón "Generar PDF"
- Muestra preview del PDF con `react-native-pdf` o abre con el viewer del sistema
- Botón "Compartir" → expo-sharing

---

## DETALLES DE IMPLEMENTACIÓN

### Llamada para generar y abrir el PDF:
```typescript
const generateAndOpenPdf = async (templateId: number, entityId: number) => {
  const response = await fetch(`${BASE_URL}/reports/generate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify({
      templateId,
      reportType: 'Sales',
      entityId,
      companyId: currentCompanyId,
      title: `Reporte #${entityId}`,
    }),
  });

  const blob = await response.blob();
  const fileUri = `${FileSystem.cacheDirectory}report_${entityId}.pdf`;
  const reader = new FileReader();
  reader.onloadend = async () => {
    const base64 = (reader.result as string).split(',')[1];
    await FileSystem.writeAsStringAsync(fileUri, base64, {
      encoding: FileSystem.EncodingType.Base64,
    });
    await Sharing.shareAsync(fileUri, { mimeType: 'application/pdf' });
  };
  reader.readAsDataURL(blob);
};
```

### Validaciones en el editor:
- Al menos una sección requerida
- `widthPercent` de columnas de tabla deben sumar entre 95 y 105
- Nombre de plantilla no puede estar vacío
- Cada sección debe tener al menos 1 campo/columna

### Librerías recomendadas:
- `react-native-draggable-flatlist` — reordenar secciones
- `expo-file-system` + `expo-sharing` — PDF
- `react-native-pdf` — preview inline
- `react-native-picker/picker` — pickers de tipo/campo/formato
- `@gorhom/bottom-sheet` — editor de sección como bottom sheet

---

## NOTAS ADICIONALES

- El endpoint `/api/reports/generate` retorna `application/pdf` directo (no JSON). Descárgalo como blob/base64.
- Cada tipo de reporte tiene su propio catálogo de campos. Cárgalo en Paso 1 cuando el usuario seleccione el tipo de reporte.
- Los campos del catálogo tienen un `section` sugerido, pero el usuario puede poner cualquier campo en cualquier sección.
- El sistema soporta múltiples plantillas del mismo tipo por empresa, pero solo una puede ser `isDefault = true` a la vez.
- La sección `Table` siempre usa `tableColumns`; las secciones `Header`, `Summary`, `Footer` siempre usan `fields`.
