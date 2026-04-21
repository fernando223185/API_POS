# Prompt para Implementar Módulos de Delivery y Cotizaciones en Electron + React

Eres un experto en **React** y **Electron**. Voy a pedirte que implementes dos módulos nuevos en mi aplicación de punto de venta. Antes de escribir código, lee todo el contexto.

---

## MI STACK

- **Framework:** Electron + React 18
- **Bundler:** Vite o Webpack (ajusta según tu configuración)
- **Componentes UI:** Material-UI (MUI) v5 o tu librería de componentes actual
- **Routing:** React Router v6
- **Estado global:** Context API o Redux (especifica cuál usas)
- **HTTP:** `fetch` o `axios` con Bearer token en header `Authorization`
- **Formularios:** React Hook Form o Formik (especifica cuál usas)
- **Estilos:** CSS Modules, Styled Components o MUI theming (especifica)

**Colores del tema:**
- Primary: `#1a3c6e` (azul oscuro)
- Secondary: `#5b6472` (gris)
- Success: `#4caf50`
- Error: `#f44336`
- Warning: `#ff9800`
- Background: `#f5f5f5`
- Card: `#ffffff`

---

## API BASE

```javascript
const API_BASE_URL = "https://tu-servidor.com/api";

// Headers por defecto
const headers = {
  "Authorization": `Bearer ${localStorage.getItem('token')}`,
  "Content-Type": "application/json"
};
```

**Estructura de respuesta estándar:**
```json
{
  "error": 0,
  "message": "Operación exitosa",
  "data": { /* resultado */ }
}
```

Si `error !== 0`, mostrar `message` como notificación de error usando tu sistema de alertas (ej: `toast.error(message)`).

---

## MÓDULO 1 — VENTAS DELIVERY

### Descripción
Permite crear pedidos con dirección de entrega programada. El inventario se descuenta y el pago se procesa al momento de confirmar la entrega física del pedido.

### Pantallas a crear

#### **Pantalla 1: Lista de Deliveries Pendientes** (`/deliveries`)

**Componentes:**
- Header con título "Deliveries Pendientes" y botón "+ Nuevo Delivery"
- Tabla o Grid de cards con las ventas tipo `Delivery` en estado `Draft`
- Filtros: por cliente, rango de fechas, almacén
- Paginación

**Cada card/fila muestra:**
- Código de venta (ej: `VD-000042`)
- Cliente (nombre o "Cliente General")
- Dirección de entrega
- Total formateado (ej: `$1,234.56`)
- Fecha estimada de entrega
- Botón "Confirmar Entrega" → navega a Pantalla 3

**Endpoint:**
```
GET /api/sales?saleType=Delivery&status=Draft&page=1&pageSize=20
```

---

#### **Pantalla 2: Nueva Venta Delivery** (`/deliveries/new`)

**Formulario con secciones:**

**Sección 1: Información General**
- **Cliente** (opcional): Autocomplete que busca clientes por nombre/RFC
  - Endpoint: `GET /api/customers/search?query={texto}`
- **Almacén** (requerido): Select con almacenes disponibles
  - Endpoint: `GET /api/warehouses`
- **Lista de Precios** (opcional): Select con listas de precios
  - Endpoint: `GET /api/price-lists`
- **Dirección de Entrega** (opcional): TextArea
- **Fecha Estimada de Entrega** (opcional): DatePicker (Material-UI `DatePicker` o `react-datepicker`)
- **Descuento General** (opcional): Input numérico (0-100%)
- **Requiere Factura**: Checkbox
- **Notas**: TextArea

**Sección 2: Productos**
- Botón "+ Agregar Producto" → Abre modal/drawer
- Tabla de productos agregados con columnas:
  - Código
  - Descripción
  - Cantidad (editable)
  - Precio Unitario (editable)
  - Descuento (%)
  - Subtotal
  - Botón eliminar (❌)

**Modal "Agregar Producto":**
- Campo de búsqueda (por nombre o código)
  - Endpoint: `GET /api/products/search?query={texto}&warehouseId={id}`
- Lista de resultados clickeables
- Al seleccionar: pide Cantidad y opcionalmente Precio (si no trae de lista de precios)

**Sección 3: Resumen**
- Subtotal
- Descuento
- Impuestos (IVA 16%)
- **Total**

**Botones:**
- "Cancelar" → vuelve a lista
- "Crear Delivery" → Llama endpoint y navega a lista

**Endpoint:**
```javascript
POST /api/sales/delivery
Body: {
  customerId: number | null,
  warehouseId: number,              // REQUERIDO
  priceListId: number | null,
  discountPercentage: number,       // 0-100
  requiresInvoice: boolean,
  deliveryAddress: string | null,
  scheduledDeliveryDate: string | null,  // ISO 8601: "2026-04-25T14:00:00Z"
  notes: string | null,
  details: [
    {
      productId: number,
      quantity: number,
      unitPrice: number,
      discountPercentage: number,   // 0-100
      notes: string | null
    }
  ]
}

Response:
{
  error: 0,
  message: "Venta delivery creada exitosamente",
  data: {
    id: 42,
    code: "VD-000042",
    saleType: "Delivery",
    status: "Draft",
    ...
  }
}
```

---

#### **Pantalla 3: Confirmar Entrega** (`/deliveries/:id/confirm`)

**Componente de resumen (read-only):**
- Código de venta
- Cliente
- Productos (tabla)
- Total

**Formulario de confirmación:**
- **Dirección Real de Entrega** (opcional): TextArea prellenado con la dirección original, editable
- **Notas de Entrega** (opcional): TextArea

**Sección de Pagos:**
- Botón "+ Agregar Pago" → Abre modal
- Lista de pagos agregados:
  - Forma de pago (Efectivo, Tarjeta Crédito, Tarjeta Débito, Transferencia)
  - Monto
  - Botón eliminar

**Modal "Agregar Pago":**
- Select: Forma de pago
  - Opciones: `Efectivo`, `Tarjeta de Crédito`, `Tarjeta de Débito`, `Transferencia`, `Cheque`
- Input numérico: Monto
- Validación: suma de pagos debe ser ≥ total

**Resumen de pagos:**
- Total a pagar: `$X,XXX.XX`
- Total pagado: `$X,XXX.XX`
- Cambio (si efectivo > total): `$XX.XX`
- Adeudo (si total pagado < total): `$XX.XX` en rojo

**Botones:**
- "Cancelar" → vuelve a lista
- "Confirmar Entrega y Cobrar" → Llama endpoint, muestra confirmación, navega a lista

**Endpoint:**
```javascript
PUT /api/sales/{id}/deliver
Body: {
  deliveryAddress: string | null,
  payments: [
    {
      paymentMethod: "Efectivo" | "Tarjeta de Crédito" | "Tarjeta de Débito" | "Transferencia" | "Cheque",
      amount: number
    }
  ],
  notes: string | null
}

Response:
{
  error: 0,
  message: "Entrega confirmada y pago registrado",
  data: {
    id: 42,
    status: "Completed",  // cambió de Draft a Completed
    deliveredAt: "2026-04-20T18:45:00Z",
    ...
  }
}
```

---

## MÓDULO 2 — COTIZACIONES

### Descripción
Permite crear cotizaciones que **NO mueven inventario**. Cada cotización genera un código único (ej: `COT-000007`) que se codifica en un **código QR**. Al escanear el QR o convertir manualmente, se genera una venta real (POS o Delivery).

### Pantallas a crear

#### **Pantalla 1: Lista de Cotizaciones** (`/quotations`)

**Componentes:**
- Header con título "Cotizaciones" y botón "+ Nueva Cotización"
- Botón "📷 Escanear QR" → abre Pantalla 4
- Tabla o Grid de cards con cotizaciones
- Filtros: por estado (`Draft`, `Converted`, `Cancelled`, `Expired`), cliente, rango de fechas
- Paginación

**Cada card/fila muestra:**
- Código de cotización (ej: `COT-000007`)
- Cliente (nombre o "Cliente General")
- Total formateado
- Fecha de creación
- Fecha de vencimiento ("Válida hasta")
- **Badge de estado:**
  - `Draft` → gris
  - `Converted` → verde
  - `Cancelled` → rojo
  - `Expired` → naranja

**Al hacer clic en una cotización** → navega a Pantalla 3 (detalle)

**Endpoint:**
```
GET /api/quotations?page=1&pageSize=20&status=Draft
```

---

#### **Pantalla 2: Nueva Cotización** (`/quotations/new`)

**Formulario similar al de Nueva Venta Delivery, pero SIN:**
- Dirección de entrega
- Fecha estimada de entrega

**Campos adicionales:**
- **Válida Hasta** (opcional): DatePicker → fecha de vencimiento de la cotización

**Al crear exitosamente:**
- Muestra modal con:
  - Mensaje de éxito
  - Código generado: `COT-000007`
  - Código QR generado (usando librería `qrcode.react` o `react-qr-code`)
  - Botones: "Imprimir QR", "Descargar QR", "Cerrar"

**Endpoint:**
```javascript
POST /api/quotations
Body: {
  customerId: number | null,
  warehouseId: number,              // REQUERIDO
  priceListId: number | null,
  discountPercentage: number,       // 0-100
  requiresInvoice: boolean,
  validUntil: string | null,        // ISO 8601
  notes: string | null,
  details: [
    {
      productId: number,
      quantity: number,
      unitPrice: number,
      discountPercentage: number,
      notes: string | null
    }
  ]
}

Response:
{
  error: 0,
  message: "Cotización creada exitosamente",
  data: {
    id: 7,
    code: "COT-000007",
    status: "Draft",
    validUntil: "2026-05-20T23:59:59Z",
    ...
  }
}
```

---

#### **Pantalla 3: Detalle de Cotización** (`/quotations/:id`)

**Modo lectura:**
- Muestra toda la información de la cotización
- Tabla de productos
- Resumen de totales

**Si status === "Draft":**
- Botón "🗑️ Cancelar Cotización" → Muestra diálogo de confirmación con input "Razón de cancelación"
  - Endpoint: `DELETE /api/quotations/{id}?reason={razón}`
- Botón "✅ Convertir a Venta" → Abre modal con opciones:
  - Radio buttons: "Venta POS" o "Venta Delivery"
  - Si selecciona "Delivery": muestra inputs para dirección y fecha estimada
  - Botón "Confirmar Conversión"

**Si status === "Converted":**
- Muestra mensaje: "Esta cotización ya fue convertida"
- Link: "Ver Venta Generada" → navega a `/sales/{saleId}` (si tienes módulo de ventas)
- Muestra código de venta generada: `VD-000042` o `V-000123`

**Si status === "Cancelled":**
- Muestra motivo de cancelación (read-only)

**Si status === "Expired":**
- Muestra mensaje: "Esta cotización ha expirado"
- Opción para reactivar (crear nueva cotización con los mismos datos)

**Endpoint de conversión:**
```javascript
POST /api/quotations/{id}/convert
Body: {
  saleType: "POS" | "Delivery",
  deliveryAddress: string | null,        // Solo si saleType = "Delivery"
  scheduledDeliveryDate: string | null,  // Solo si saleType = "Delivery"
  notes: string | null
}

Response:
{
  error: 0,
  message: "Cotización convertida exitosamente a venta",
  data: {
    saleId: 42,
    saleCode: "VD-000042",  // o "V-000123"
    quotation: {
      id: 7,
      status: "Converted",  // cambió de Draft a Converted
      convertedAt: "2026-04-20T19:00:00Z"
    }
  }
}
```

---

#### **Pantalla 4: Escanear QR de Cotización** (`/quotations/scan`)

**Componentes:**
- Usa librería de escaneo QR para web:
  - `react-qr-reader` o `html5-qrcode`
  - O captura de imagen con `input[type=file]` y procesamiento con `jsQR`
  
**Flujo:**
1. Usuario escanea QR que contiene el código `COT-000007`
2. Llama endpoint: `GET /api/quotations/by-code/{code}`
3. Muestra modal con resumen de la cotización:
   - Código
   - Cliente
   - Productos (lista resumida)
   - Total
4. **Botones en el modal:**
   - "🛒 Convertir a Venta POS" → llama `POST /api/quotations/{id}/convert` con `saleType: "POS"`
     - Navega a pantalla de pago con el `saleId` retornado
   - "🚚 Convertir a Delivery" → Muestra inputs para dirección y fecha, luego llama endpoint con `saleType: "Delivery"`
     - Navega a lista de deliveries
   - "❌ Cancelar"

**Endpoints:**
```javascript
// Buscar cotización por código QR
GET /api/quotations/by-code/{code}

Response:
{
  error: 0,
  message: "Cotización encontrada",
  data: {
    id: 7,
    code: "COT-000007",
    status: "Draft",
    customerName: "Juan Pérez",
    total: 1234.56,
    details: [ ... ],
    ...
  }
}
```

---

## ESTRUCTURA DE ARCHIVOS SUGERIDA

```
src/
  pages/
    Delivery/
      DeliveryListPage.jsx
      NewDeliveryPage.jsx
      ConfirmDeliveryPage.jsx
    Quotations/
      QuotationListPage.jsx
      NewQuotationPage.jsx
      QuotationDetailPage.jsx
      QRScannerPage.jsx
  
  components/
    delivery/
      DeliveryCard.jsx
      DeliveryFilters.jsx
      PaymentForm.jsx
    quotations/
      QuotationCard.jsx
      QuotationStatusBadge.jsx
      QuotationQRModal.jsx
      ConvertQuotationModal.jsx
    shared/
      ProductSearchModal.jsx
      ProductTable.jsx
  
  services/
    api/
      deliveryService.js
      quotationService.js
      productService.js
      customerService.js
  
  hooks/
    useDeliveries.js
    useQuotations.js
  
  routes/
    deliveryRoutes.jsx
    quotationRoutes.jsx
```

---

## REGLAS DE IMPLEMENTACIÓN

### 1. Estilos
- Usa Material-UI theming consistente
- Tarjetas con `elevation={2}`
- Botones primarios para acciones principales (`variant="contained"`)
- Botones secundarios para acciones de cancelar (`variant="outlined"`)
- Formularios con `Grid` y `Box` para layout

### 2. Validación de formularios
- Todos los campos requeridos deben tener validación
- Mostrar errores debajo de cada campo (Material-UI `FormHelperText`)
- Deshabilitar botón de submit mientras haya errores
- Validar que la suma de pagos sea >= total antes de confirmar entrega

### 3. Manejo de errores
- Usar sistema de notificaciones (ej: `react-toastify`, MUI `Snackbar`, etc.)
- Capturar errores de red con `try/catch`
- Mostrar mensajes amigables al usuario
- Deshabilitar botones durante la carga (mostrar spinner)

### 4. Loading states
- Mostrar `CircularProgress` o skeleton mientras se cargan datos
- Deshabilitar formularios durante el submit
- Indicadores visuales en botones: "Guardando..." con spinner

### 5. Navegación
- Usar `react-router-dom` con `useNavigate()` para navegación programática
- Breadcrumbs en páginas de detalle
- Confirmación al salir de formularios con datos sin guardar

### 6. Fechas
- Usar formato ISO 8601 para enviar al backend
- Mostrar fechas en formato local legible (ej: "20/04/2026 18:45")
- Usar librería `date-fns` o `dayjs` para formateo

### 7. Números y moneda
- Formatear montos con separadores de miles y 2 decimales
- Usar Intl.NumberFormat o librería de formateo
- Ejemplo: `$1,234.56`

### 8. QR Codes
- Usar librería: `qrcode.react` o `react-qr-code` para generación
- Tamaño recomendado: 256x256px
- Permitir descargar como imagen PNG
- Para escaneo: `html5-qrcode` o `react-qr-reader`

### 9. Permisos
- Si tu app maneja permisos de usuario, verifica:
  - `Sales.Create` para crear deliveries
  - `Quotations.Create` para crear cotizaciones
  - `Quotations.Convert` para convertir cotizaciones
- Ocultar/deshabilitar botones según permisos

### 10. Responsive
- Layout responsive usando Material-UI `Grid` y `Container`
- Tablas deben tener scroll horizontal en móviles
- Modales deben ser fullscreen en móviles

---

## ENDPOINTS RESUMIDOS

### Delivery
```
GET    /api/sales?saleType=Delivery&status=Draft&page=1&pageSize=20
POST   /api/sales/delivery
GET    /api/sales/{id}
PUT    /api/sales/{id}/deliver
```

### Quotations
```
GET    /api/quotations?page=1&pageSize=20&status=Draft
POST   /api/quotations
GET    /api/quotations/{id}
GET    /api/quotations/by-code/{code}
POST   /api/quotations/{id}/convert
DELETE /api/quotations/{id}?reason={razón}
```

### Auxiliares
```
GET /api/customers/search?query={texto}
GET /api/products/search?query={texto}&warehouseId={id}
GET /api/warehouses
GET /api/price-lists
```

---

## NOTAS FINALES

- **Reutiliza componentes:** El formulario de productos es casi idéntico en Delivery y Cotizaciones, crea un componente compartido
- **Estado global:** Si manejas carrito o session, considera almacenar los drafts temporalmente
- **Impresión:** Considera integrar impresión de PDFs para cotizaciones (puedes llamar endpoint de generación de PDF del backend si existe)
- **Permisos:** Implementa guards de ruta si tu app requiere autenticación/autorización
- **Testing:** Escribe tests para los componentes críticos (formularios, conversión de cotizaciones)

---

## EJEMPLO DE CÓDIGO INICIAL

```jsx
// src/services/api/deliveryService.js
import { apiClient } from './apiClient';

export const deliveryService = {
  async getAll(filters = {}) {
    const params = new URLSearchParams({
      saleType: 'Delivery',
      status: filters.status || 'Draft',
      page: filters.page || 1,
      pageSize: filters.pageSize || 20
    });
    
    const response = await apiClient.get(`/sales?${params}`);
    return response.data;
  },

  async create(data) {
    const response = await apiClient.post('/sales/delivery', data);
    return response.data;
  },

  async getById(id) {
    const response = await apiClient.get(`/sales/${id}`);
    return response.data;
  },

  async confirmDelivery(id, data) {
    const response = await apiClient.put(`/sales/${id}/deliver`, data);
    return response.data;
  }
};
```

```jsx
// src/components/quotations/QuotationStatusBadge.jsx
import { Chip } from '@mui/material';

const statusConfig = {
  Draft: { label: 'Borrador', color: 'default' },
  Converted: { label: 'Convertida', color: 'success' },
  Cancelled: { label: 'Cancelada', color: 'error' },
  Expired: { label: 'Expirada', color: 'warning' }
};

export const QuotationStatusBadge = ({ status }) => {
  const config = statusConfig[status] || statusConfig.Draft;
  
  return (
    <Chip 
      label={config.label} 
      color={config.color} 
      size="small" 
    />
  );
};
```

---

## COMIENZA AHORA

Implementa primero el **Módulo 1 (Delivery)** completo y funcional. Luego continúa con el **Módulo 2 (Cotizaciones)**.

Pregunta si necesitas aclaración sobre algún endpoint o flujo antes de comenzar a codificar.
