# Prompt — Módulo de Traspasos de Almacén (Warehouse Transfers) en Electron + React

Eres un experto en **React** y **Electron**. Voy a pedirte que implementes el módulo de **Traspasos de Almacén** en mi aplicación de punto de venta. Lee todo el contexto antes de escribir código.

---

## MI STACK

- **Framework:** Electron + React 18
- **Bundler:** Vite o Webpack (ajusta según tu configuración)
- **Componentes UI:** Material-UI (MUI) v5
- **Routing:** React Router v6
- **Estado global:** Context API o Redux (especifica cuál usas)
- **HTTP:** `axios` con Bearer token en header `Authorization`
- **Formularios:** React Hook Form
- **Estilos:** MUI theming

**Colores del tema:**
- Primary: `#1a3c6e` (azul oscuro)
- Secondary: `#5b6472` (gris)
- Success: `#4caf50`
- Error: `#f44336`
- Warning: `#ff9800`
- Info: `#2196f3`
- Background: `#f5f5f5`
- Card: `#ffffff`

---

## API BASE

```javascript
const API_BASE_URL = "https://tu-servidor.com/api";

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
  "data": { }
}
```

Si `error !== 0`, mostrar `message` como `toast.error(message)`.

---

## CONTEXTO DEL MÓDULO

Este módulo maneja el traslado de mercancía **de un almacén A (origen) a un almacén B (destino)**.

### Flujo de estados de una orden

```
Draft → [Despachar] → Dispatched → [Registrar Entrada] → PartiallyReceived
                                                        → Received
Draft → [Cancelar] → Cancelled
```

| Estado              | Color sugerido | Descripción |
|---------------------|---------------|-------------|
| `Draft`             | Gris          | Orden creada, pendiente de despacho |
| `Dispatched`        | Azul          | Mercancía salió del almacén origen |
| `PartiallyReceived` | Naranja       | Almacén destino recibió parte de la mercancía |
| `Received`          | Verde         | Mercancía completamente recibida |
| `Cancelled`         | Rojo          | Orden cancelada |

**Concepto clave — Entrada Parcial:**
- El almacén destino puede registrar múltiples entradas parciales hasta completar la cantidad despachada.
- En cada entrada, el usuario elige qué productos recibir y en qué cantidad (sin exceder lo pendiente).

---

## ENDPOINTS DE LA API

### Listado paginado
```
GET /api/warehouse-transfers?page=1&pageSize=20
  &search=WTR-001
  &sourceWarehouseId=1
  &destinationWarehouseId=2
  &status=Draft
  &companyId=1
```

**Respuesta:**
```json
{
  "items": [
    {
      "id": 1,
      "code": "WTR-001",
      "status": "Draft",
      "sourceWarehouseName": "Almacén Central",
      "destinationWarehouseName": "Almacén Sucursal Norte",
      "transferDate": "2026-04-24T00:00:00Z",
      "dispatchedAt": null,
      "createdAt": "2026-04-24T10:00:00Z",
      "createdByUserName": "Juan Pérez",
      "totalProducts": 3,
      "totalQuantityRequested": 150,
      "totalQuantityReceived": 0
    }
  ],
  "totalRecords": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### Obtener detalle por ID
```
GET /api/warehouse-transfers/{id}
```

**Respuesta:**
```json
{
  "id": 1,
  "code": "WTR-001",
  "status": "Dispatched",
  "sourceWarehouseId": 1,
  "sourceWarehouseName": "Almacén Central",
  "sourceWarehouseCode": "ALM-001",
  "destinationWarehouseId": 2,
  "destinationWarehouseName": "Almacén Sucursal Norte",
  "destinationWarehouseCode": "ALM-002",
  "companyId": 1,
  "transferDate": "2026-04-24T00:00:00Z",
  "notes": "Reposición mensual",
  "dispatchedAt": "2026-04-24T14:30:00Z",
  "dispatchedByUserName": "Carlos López",
  "createdAt": "2026-04-24T10:00:00Z",
  "createdByUserName": "Juan Pérez",
  "totalProducts": 2,
  "totalQuantityRequested": 100,
  "totalQuantityDispatched": 100,
  "totalQuantityReceived": 40,
  "totalPendingQuantity": 60,
  "details": [
    {
      "id": 1,
      "productId": 5,
      "productCode": "PROD-005",
      "productName": "Refresco 600ml",
      "quantityRequested": 60,
      "quantityDispatched": 60,
      "quantityReceived": 40,
      "pendingQuantity": 20,
      "unitCost": 12.50,
      "notes": null
    },
    {
      "id": 2,
      "productId": 8,
      "productCode": "PROD-008",
      "productName": "Agua 1L",
      "quantityRequested": 40,
      "quantityDispatched": 40,
      "quantityReceived": 0,
      "pendingQuantity": 40,
      "unitCost": 8.00,
      "notes": null
    }
  ],
  "receivings": [
    {
      "id": 1,
      "code": "WRV-001",
      "receivingDate": "2026-04-24T16:00:00Z",
      "receivingType": "Partial",
      "totalProducts": 1,
      "totalQuantityReceived": 40,
      "createdAt": "2026-04-24T16:00:00Z",
      "createdByUserName": "Ana García"
    }
  ]
}
```

### Crear orden (Draft)
```
POST /api/warehouse-transfers
```
```json
{
  "sourceWarehouseId": 1,
  "destinationWarehouseId": 2,
  "companyId": 1,
  "transferDate": "2026-04-24T00:00:00Z",
  "notes": "Reposición mensual",
  "details": [
    { "productId": 5, "quantityRequested": 60, "notes": null },
    { "productId": 8, "quantityRequested": 40, "notes": null }
  ]
}
```

### Actualizar orden (solo Draft)
```
PUT /api/warehouse-transfers/{id}
```
Mismo body que crear (reemplaza todos los detalles).

### Cancelar orden (solo Draft)
```
POST /api/warehouse-transfers/{id}/cancel
```
Sin body.

### Despachar mercancía desde almacén origen
```
POST /api/warehouse-transfers/{id}/dispatch
```
```json
{
  "notes": "Todo completo al despacho"
}
```

**Respuesta exitosa:**
```json
{
  "message": "Mercancía despachada correctamente. 2 movimiento(s) de salida creados.",
  "error": 0,
  "data": {
    "transferId": 1,
    "transferCode": "WTR-001",
    "status": "Dispatched",
    "dispatchedAt": "2026-04-24T14:30:00Z",
    "totalMovementsCreated": 2,
    "movements": [
      {
        "productCode": "PROD-005",
        "productName": "Refresco 600ml",
        "quantityDispatched": 60,
        "movementCode": "MOV-041",
        "stockBefore": 200,
        "stockAfter": 140
      }
    ]
  }
}
```

### Registrar entrada en almacén destino (parcial o completa)
```
POST /api/warehouse-transfers/{id}/receivings
```
```json
{
  "receivingDate": "2026-04-24T16:00:00Z",
  "notes": "Llegó el camión, faltaron 20 unidades de refresco",
  "details": [
    {
      "warehouseTransferDetailId": 1,
      "quantityReceived": 40,
      "notes": "Recibidos con golpes leves en empaque"
    }
  ]
}
```

> **Nota:** Solo se incluyen los productos que realmente se reciben. Si un producto no está en el body, se asume que aún no llegó.

**Respuesta exitosa:**
```json
{
  "message": "Entrada registrada correctamente (Partial). 1 producto(s), 40 unidad(es) recibidas.",
  "error": 0,
  "data": {
    "id": 1,
    "code": "WRV-001",
    "receivingType": "Partial",
    "totalProducts": 1,
    "totalQuantityReceived": 40
  }
}
```

### Obtener entradas de una orden
```
GET /api/warehouse-transfers/{id}/receivings
```

---

## PANTALLAS A IMPLEMENTAR

---

### PANTALLA 1: Lista de Traspasos (`/warehouse-transfers`)

**Layout:**
- Header: título "Traspasos de Almacén" + botón primario "+ Nuevo Traspaso"
- Barra de filtros: campo de búsqueda (código/notas), selector de almacén origen, selector de almacén destino, selector de estado, botón "Buscar"
- Tabla con paginación

**Columnas de la tabla:**

| Columna | Campo |
|---------|-------|
| Código | `code` con badge de color según `status` |
| Origen | `sourceWarehouseName` |
| Destino | `destinationWarehouseName` |
| Fecha | `transferDate` formateada |
| Productos | `totalProducts` |
| Solicitado | `totalQuantityRequested` |
| Recibido | `totalQuantityReceived` |
| Estado | Chip de color según estado |
| Acciones | Ver, Editar (solo Draft), Cancelar (solo Draft) |

**Colores de chips por estado:**
```javascript
const statusColors = {
  Draft: 'default',
  Dispatched: 'info',
  PartiallyReceived: 'warning',
  Received: 'success',
  Cancelled: 'error'
};

const statusLabels = {
  Draft: 'Borrador',
  Dispatched: 'Despachado',
  PartiallyReceived: 'Recibido Parcial',
  Received: 'Recibido',
  Cancelled: 'Cancelado'
};
```

---

### PANTALLA 2: Crear / Editar Orden (`/warehouse-transfers/new` y `/warehouse-transfers/{id}/edit`)

**Sección superior — Datos generales:**
- Select: Almacén Origen (carga desde `GET /api/warehouses`)
- Select: Almacén Destino (excluye el almacén origen seleccionado)
- DatePicker: Fecha de traspaso
- Textarea: Notas (opcional)

**Sección inferior — Detalle de productos:**
- Tabla editable con columnas: Producto, Cantidad Solicitada, Notas, Acciones
- Botón "+ Agregar Producto" abre un buscador/modal de productos (`GET /api/products?search=xxx`)
- No permitir el mismo producto dos veces
- Validar cantidad > 0

**Botones:**
- "Guardar como Borrador" → `POST /api/warehouse-transfers` o `PUT /api/warehouse-transfers/{id}`
- "Cancelar" → regresa a lista

**Validaciones en frontend:**
- Almacén origen ≠ destino
- Al menos 1 producto
- Todos los productos con cantidad > 0

---

### PANTALLA 3: Detalle de Orden (`/warehouse-transfers/{id}`)

**Layout en dos columnas:**

**Columna izquierda — Información general:**
- Card con: Código, Estado (chip), Almacén Origen → Almacén Destino (con flecha ↔), Fecha del traspaso, Notas, Creado por, Fecha despacho (si aplica), Despachado por (si aplica)
- Barra de progreso de recepción: `totalQuantityReceived / totalQuantityDispatched`

**Columna derecha — Tabla de productos:**

| Campo | Descripción |
|-------|-------------|
| Producto | código + nombre |
| Solicitado | `quantityRequested` |
| Despachado | `quantityDispatched` |
| Recibido | `quantityReceived` |
| Pendiente | `pendingQuantity` (rojo si > 0) |
| Costo Unit. | `unitCost` formateado |

**Sección de Entradas Registradas** (tabla histórica):
- Código WRV, Fecha, Tipo (Partial/Complete), Productos, Unidades, Registrado por

**Botones de acción (condicionales por estado):**

| Estado | Botones visibles |
|--------|-----------------|
| `Draft` | "Editar", "Despachar", "Cancelar" |
| `Dispatched` | "Registrar Entrada" |
| `PartiallyReceived` | "Registrar Entrada" |
| `Received` | _(ninguno)_ |
| `Cancelled` | _(ninguno)_ |

---

### MODAL: Confirmar Despacho

Se abre al presionar "Despachar" en la pantalla de detalle.

**Contenido:**
- Advertencia: "Esta acción descontará el stock del almacén origen y no se puede deshacer"
- Resumen de productos a despachar (tabla solo lectura)
- Campo texto: Notas de despacho (opcional)
- Botones: "Confirmar Despacho" (color warning), "Cancelar"

**Al confirmar:** `POST /api/warehouse-transfers/{id}/dispatch`

Mostrar resultado con los movimientos creados (código MOV, producto, stock antes/después).

---

### MODAL: Registrar Entrada de Mercancía

Se abre al presionar "Registrar Entrada" en la pantalla de detalle (estado `Dispatched` o `PartiallyReceived`).

**Contenido:**
- DatePicker: Fecha de recepción (default: hoy)
- Textarea: Notas (opcional)
- **Tabla de productos pendientes** (solo los que tienen `pendingQuantity > 0`):

| Campo | Descripción |
|-------|-------------|
| Producto | código + nombre |
| Pendiente | `pendingQuantity` |
| Cantidad a recibir | Input numérico (0 a `pendingQuantity`) — 0 significa que este producto no se recibe ahora |

- Checkbox o switch: "Recibir todo lo pendiente" → llena automáticamente todos los inputs con su `pendingQuantity`

**Validaciones:**
- Al menos un producto con cantidad > 0
- Ninguna cantidad puede superar `pendingQuantity`

**Al confirmar:** `POST /api/warehouse-transfers/{id}/receivings`

```javascript
// Construir body (solo los que tienen cantidad > 0)
const body = {
  receivingDate: fechaSeleccionada,
  notes: notas,
  details: productos
    .filter(p => p.cantidadARecibir > 0)
    .map(p => ({
      warehouseTransferDetailId: p.id,
      quantityReceived: p.cantidadARecibir,
      notes: p.notas
    }))
};
```

Mostrar resultado con mensaje de si fue `Partial` o `Complete`.

---

## COMPONENTES AUXILIARES SUGERIDOS

```
/src/modules/warehouse-transfers/
  ├── pages/
  │   ├── WarehouseTransferListPage.jsx
  │   ├── WarehouseTransferFormPage.jsx   (crear/editar)
  │   └── WarehouseTransferDetailPage.jsx
  ├── components/
  │   ├── StatusChip.jsx
  │   ├── DispatchModal.jsx
  │   ├── ReceivingModal.jsx
  │   ├── ReceivingHistoryTable.jsx
  │   └── ProductDetailTable.jsx
  ├── hooks/
  │   ├── useWarehouseTransfers.js        (listado + paginación)
  │   └── useWarehouseTransferDetail.js   (detalle + acciones)
  └── api/
      └── warehouseTransferApi.js
```

---

## ARCHIVO API COMPLETO

```javascript
// src/modules/warehouse-transfers/api/warehouseTransferApi.js
import axios from 'axios';

const BASE = '/api/warehouse-transfers';

const auth = () => ({
  headers: {
    Authorization: `Bearer ${localStorage.getItem('token')}`,
    'Content-Type': 'application/json',
  },
});

export const warehouseTransferApi = {
  // GET /api/warehouse-transfers?page=1&pageSize=20&search=&status=&sourceWarehouseId=&destinationWarehouseId=
  getAll: (params) => axios.get(BASE, { params, ...auth() }),

  // GET /api/warehouse-transfers/{id}
  getById: (id) => axios.get(`${BASE}/${id}`, auth()),

  // POST /api/warehouse-transfers
  create: (data) => axios.post(BASE, data, auth()),

  // PUT /api/warehouse-transfers/{id}
  update: (id, data) => axios.put(`${BASE}/${id}`, data, auth()),

  // POST /api/warehouse-transfers/{id}/cancel
  cancel: (id) => axios.post(`${BASE}/${id}/cancel`, {}, auth()),

  // POST /api/warehouse-transfers/{id}/dispatch
  dispatch: (id, data) => axios.post(`${BASE}/${id}/dispatch`, data, auth()),

  // POST /api/warehouse-transfers/{id}/receivings
  createReceiving: (id, data) => axios.post(`${BASE}/${id}/receivings`, data, auth()),

  // GET /api/warehouse-transfers/{id}/receivings
  getReceivings: (id) => axios.get(`${BASE}/${id}/receivings`, auth()),

  // GET /api/warehouse-transfers/receivings/{receivingId}
  getReceivingById: (receivingId) => axios.get(`${BASE}/receivings/${receivingId}`, auth()),
};

// ─────────────────────────────────────────────────────────────────────────
// API DE REPORTES (sistema centralizado para generar PDFs)
// ─────────────────────────────────────────────────────────────────────────

export const reportApi = {
  // POST /api/reports/generate
  // Genera un PDF usando el sistema de plantillas dinámicas
  generatePdf: (data) =>
    axios.post('/api/reports/generate', data, {
      responseType: 'blob',
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json',
      },
    }),
};
```

---

## HOOK: useWarehouseTransfers (listado)

```javascript
// src/modules/warehouse-transfers/hooks/useWarehouseTransfers.js
import { useState, useEffect, useCallback } from 'react';
import { warehouseTransferApi } from '../api/warehouseTransferApi';

export function useWarehouseTransfers() {
  const [transfers, setTransfers] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 20,
    search: '',
    status: '',
    sourceWarehouseId: '',
    destinationWarehouseId: '',
  });

  const fetchTransfers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      // Limpiar parámetros vacíos antes de enviar
      const params = Object.fromEntries(
        Object.entries(filters).filter(([, v]) => v !== '' && v !== null)
      );
      const { data } = await warehouseTransferApi.getAll(params);
      setTransfers(data.items);
      setTotalRecords(data.totalRecords);
    } catch (err) {
      setError(err?.response?.data?.message || 'Error al cargar traspasos');
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchTransfers();
  }, [fetchTransfers]);

  const handleFilterChange = (newFilters) => {
    setFilters((prev) => ({ ...prev, ...newFilters, page: 1 }));
  };

  const handlePageChange = (newPage) => {
    setFilters((prev) => ({ ...prev, page: newPage }));
  };

  return {
    transfers,
    totalRecords,
    loading,
    error,
    filters,
    handleFilterChange,
    handlePageChange,
    refresh: fetchTransfers,
  };
}
```

---

## HOOK: useWarehouseTransferDetail (detalle + acciones)

```javascript
// src/modules/warehouse-transfers/hooks/useWarehouseTransferDetail.js
import { useState, useEffect, useCallback } from 'react';
import { warehouseTransferApi } from '../api/warehouseTransferApi';
import toast from 'react-hot-toast'; // o tu librería de notificaciones

export function useWarehouseTransferDetail(id) {
  const [transfer, setTransfer] = useState(null);
  const [loading, setLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);

  const fetchDetail = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    try {
      const { data } = await warehouseTransferApi.getById(id);
      setTransfer(data);
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Error al cargar el traspaso');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchDetail();
  }, [fetchDetail]);

  const dispatch = async (dispatchData) => {
    setActionLoading(true);
    try {
      const { data } = await warehouseTransferApi.dispatch(id, dispatchData);
      toast.success(data.message);
      await fetchDetail(); // recargar estado actualizado
      return data.data;
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Error al despachar');
      throw err;
    } finally {
      setActionLoading(false);
    }
  };

  const createReceiving = async (receivingData) => {
    setActionLoading(true);
    try {
      const { data } = await warehouseTransferApi.createReceiving(id, receivingData);
      toast.success(data.message);
      await fetchDetail();
      return data.data;
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Error al registrar entrada');
      throw err;
    } finally {
      setActionLoading(false);
    }
  };

  const cancel = async () => {
    setActionLoading(true);
    try {
      await warehouseTransferApi.cancel(id);
      toast.success('Orden cancelada correctamente');
      await fetchDetail();
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Error al cancelar');
      throw err;
    } finally {
      setActionLoading(false);
    }
  };

  return { transfer, loading, actionLoading, refresh: fetchDetail, dispatch, createReceiving, cancel };
}
```

---

## COMPONENTE: StatusChip

```jsx
// src/modules/warehouse-transfers/components/StatusChip.jsx
import { Chip } from '@mui/material';

const statusConfig = {
  Draft:             { label: 'Borrador',        color: 'default' },
  Dispatched:        { label: 'Despachado',       color: 'info'    },
  PartiallyReceived: { label: 'Recibido Parcial', color: 'warning' },
  Received:          { label: 'Recibido',         color: 'success' },
  Cancelled:         { label: 'Cancelado',        color: 'error'   },
};

export default function StatusChip({ status }) {
  const config = statusConfig[status] ?? { label: status, color: 'default' };
  return <Chip label={config.label} color={config.color} size="small" />;
}
```

---

## PANTALLA 1 COMPLETA: Lista de Traspasos

```jsx
// src/modules/warehouse-transfers/pages/WarehouseTransferListPage.jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Button, TextField, Select, MenuItem, FormControl, InputLabel,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, IconButton, Tooltip, Pagination, CircularProgress, Typography,
} from '@mui/material';
import { Visibility, Edit, Cancel, Add } from '@mui/icons-material';
import { useWarehouseTransfers } from '../hooks/useWarehouseTransfers';
import StatusChip from '../components/StatusChip';
import { warehouseTransferApi } from '../api/warehouseTransferApi';
import toast from 'react-hot-toast';

const STATUS_OPTIONS = ['Draft', 'Dispatched', 'PartiallyReceived', 'Received', 'Cancelled'];

export default function WarehouseTransferListPage() {
  const navigate = useNavigate();
  const { transfers, totalRecords, loading, filters, handleFilterChange, handlePageChange, refresh } =
    useWarehouseTransfers();

  const [searchInput, setSearchInput] = useState('');

  const handleSearch = () => handleFilterChange({ search: searchInput });

  const handleCancel = async (id) => {
    if (!window.confirm('¿Cancelar esta orden? Esta acción no se puede deshacer.')) return;
    try {
      await warehouseTransferApi.cancel(id);
      toast.success('Orden cancelada');
      refresh();
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Error al cancelar');
    }
  };

  const totalPages = Math.ceil(totalRecords / filters.pageSize);

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5" fontWeight="bold">Traspasos de Almacén</Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={() => navigate('/warehouse-transfers/new')}
        >
          Nuevo Traspaso
        </Button>
      </Box>

      {/* Filtros */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <TextField
          label="Buscar (código / notas)"
          size="small"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
          sx={{ width: 220 }}
        />
        <FormControl size="small" sx={{ width: 160 }}>
          <InputLabel>Estado</InputLabel>
          <Select
            value={filters.status}
            label="Estado"
            onChange={(e) => handleFilterChange({ status: e.target.value })}
          >
            <MenuItem value="">Todos</MenuItem>
            {STATUS_OPTIONS.map((s) => (
              <MenuItem key={s} value={s}>{s}</MenuItem>
            ))}
          </Select>
        </FormControl>
        <Button variant="outlined" onClick={handleSearch}>Buscar</Button>
      </Box>

      {/* Tabla */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead sx={{ backgroundColor: '#1a3c6e' }}>
              <TableRow>
                {['Código', 'Origen', 'Destino', 'Fecha', 'Productos', 'Solicitado', 'Recibido', 'Estado', 'Acciones'].map((h) => (
                  <TableCell key={h} sx={{ color: '#fff', fontWeight: 'bold' }}>{h}</TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {transfers.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} align="center" sx={{ py: 4, color: '#999' }}>
                    No se encontraron traspasos
                  </TableCell>
                </TableRow>
              ) : (
                transfers.map((t) => (
                  <TableRow key={t.id} hover>
                    <TableCell><strong>{t.code}</strong></TableCell>
                    <TableCell>{t.sourceWarehouseName}</TableCell>
                    <TableCell>{t.destinationWarehouseName}</TableCell>
                    <TableCell>{new Date(t.transferDate).toLocaleDateString('es-MX')}</TableCell>
                    <TableCell align="center">{t.totalProducts}</TableCell>
                    <TableCell align="right">{t.totalQuantityRequested.toLocaleString()}</TableCell>
                    <TableCell align="right">{t.totalQuantityReceived.toLocaleString()}</TableCell>
                    <TableCell><StatusChip status={t.status} /></TableCell>
                    <TableCell>
                      <Tooltip title="Ver detalle">
                        <IconButton size="small" onClick={() => navigate(`/warehouse-transfers/${t.id}`)}>
                          <Visibility fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      {t.status === 'Draft' && (
                        <>
                          <Tooltip title="Editar">
                            <IconButton size="small" onClick={() => navigate(`/warehouse-transfers/${t.id}/edit`)}>
                              <Edit fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Cancelar">
                            <IconButton size="small" color="error" onClick={() => handleCancel(t.id)}>
                              <Cancel fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </>
                      )}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Paginación */}
      {totalPages > 1 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
          <Pagination
            count={totalPages}
            page={filters.page}
            onChange={(_, p) => handlePageChange(p)}
            color="primary"
          />
        </Box>
      )}
    </Box>
  );
}
```

---

## MODAL: Confirmar Despacho

```jsx
// src/modules/warehouse-transfers/components/DispatchModal.jsx
import { useState } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Alert, Table, TableHead, TableRow,
  TableCell, TableBody, Typography, CircularProgress,
} from '@mui/material';
import { Warning } from '@mui/icons-material';

export default function DispatchModal({ open, transfer, onClose, onConfirm, loading }) {
  const [notes, setNotes] = useState('');

  const handleConfirm = () => onConfirm({ notes });

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ backgroundColor: '#ff9800', color: '#fff' }}>
        <Warning sx={{ mr: 1, verticalAlign: 'middle' }} />
        Confirmar Despacho de Mercancía
      </DialogTitle>

      <DialogContent sx={{ mt: 2 }}>
        <Alert severity="warning" sx={{ mb: 2 }}>
          Esta acción <strong>descontará el stock del almacén origen</strong> y no se puede deshacer.
        </Alert>

        <Typography variant="subtitle2" sx={{ mb: 1 }}>
          Almacén origen: <strong>{transfer?.sourceWarehouseName}</strong> →{' '}
          Almacén destino: <strong>{transfer?.destinationWarehouseName}</strong>
        </Typography>

        {/* Resumen de productos */}
        <Table size="small" sx={{ mb: 2 }}>
          <TableHead>
            <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
              <TableCell>Código</TableCell>
              <TableCell>Producto</TableCell>
              <TableCell align="right">Cantidad a Despachar</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {transfer?.details?.map((d) => (
              <TableRow key={d.id}>
                <TableCell>{d.productCode}</TableCell>
                <TableCell>{d.productName}</TableCell>
                <TableCell align="right"><strong>{d.quantityRequested.toLocaleString()}</strong></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>

        <TextField
          label="Notas de despacho (opcional)"
          multiline
          rows={2}
          fullWidth
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancelar</Button>
        <Button
          variant="contained"
          color="warning"
          onClick={handleConfirm}
          disabled={loading}
          startIcon={loading && <CircularProgress size={16} />}
        >
          Confirmar Despacho
        </Button>
      </DialogActions>
    </Dialog>
  );
}
```

---

## MODAL: Registrar Entrada de Mercancía

```jsx
// src/modules/warehouse-transfers/components/ReceivingModal.jsx
import { useState, useEffect } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Table, TableHead, TableRow, TableCell,
  TableBody, Typography, FormControlLabel, Switch, CircularProgress, Alert,
} from '@mui/material';
import { MoveToInbox } from '@mui/icons-material';

export default function ReceivingModal({ open, transfer, onClose, onConfirm, loading }) {
  const [receivingDate, setReceivingDate] = useState(new Date().toISOString().split('T')[0]);
  const [notes, setNotes] = useState('');
  const [receiveAll, setReceiveAll] = useState(false);
  const [quantities, setQuantities] = useState({});
  const [error, setError] = useState('');

  // Solo productos con pendiente > 0
  const pendingDetails = transfer?.details?.filter((d) => d.pendingQuantity > 0) ?? [];

  useEffect(() => {
    if (open) {
      // Inicializar con 0 o con pendiente si receiveAll activo
      const initial = {};
      pendingDetails.forEach((d) => {
        initial[d.id] = receiveAll ? d.pendingQuantity : 0;
      });
      setQuantities(initial);
      setError('');
    }
  }, [open, transfer]);

  useEffect(() => {
    if (receiveAll) {
      const newQty = {};
      pendingDetails.forEach((d) => { newQty[d.id] = d.pendingQuantity; });
      setQuantities(newQty);
    }
  }, [receiveAll]);

  const handleQuantityChange = (detailId, value, max) => {
    const parsed = Math.max(0, Math.min(Number(value) || 0, max));
    setQuantities((prev) => ({ ...prev, [detailId]: parsed }));
    setReceiveAll(false);
  };

  const handleConfirm = () => {
    setError('');
    const details = pendingDetails
      .filter((d) => quantities[d.id] > 0)
      .map((d) => ({
        warehouseTransferDetailId: d.id,
        quantityReceived: quantities[d.id],
        notes: null,
      }));

    if (details.length === 0) {
      setError('Debes ingresar al menos una cantidad mayor a 0.');
      return;
    }

    onConfirm({
      receivingDate: new Date(receivingDate).toISOString(),
      notes,
      details,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ backgroundColor: '#1a3c6e', color: '#fff' }}>
        <MoveToInbox sx={{ mr: 1, verticalAlign: 'middle' }} />
        Registrar Entrada de Mercancía — {transfer?.code}
      </DialogTitle>

      <DialogContent sx={{ mt: 2 }}>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Almacén destino: <strong>{transfer?.destinationWarehouseName}</strong>
        </Typography>

        {/* Fecha + Notas */}
        <TextField
          label="Fecha de recepción"
          type="date"
          value={receivingDate}
          onChange={(e) => setReceivingDate(e.target.value)}
          InputLabelProps={{ shrink: true }}
          size="small"
          sx={{ mr: 2, mb: 2 }}
        />
        <TextField
          label="Notas (opcional)"
          size="small"
          fullWidth
          multiline
          rows={2}
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          sx={{ mb: 2 }}
        />

        {/* Recibir todo */}
        <FormControlLabel
          control={<Switch checked={receiveAll} onChange={(e) => setReceiveAll(e.target.checked)} />}
          label="Recibir todo lo pendiente"
          sx={{ mb: 2 }}
        />

        {/* Tabla de productos pendientes */}
        <Table size="small">
          <TableHead sx={{ backgroundColor: '#f5f5f5' }}>
            <TableRow>
              <TableCell>Código</TableCell>
              <TableCell>Producto</TableCell>
              <TableCell align="right">Pendiente</TableCell>
              <TableCell align="right" sx={{ width: 140 }}>Cantidad a Recibir</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {pendingDetails.map((d) => (
              <TableRow key={d.id}>
                <TableCell>{d.productCode}</TableCell>
                <TableCell>{d.productName}</TableCell>
                <TableCell align="right" sx={{ color: '#ff9800', fontWeight: 'bold' }}>
                  {d.pendingQuantity.toLocaleString()}
                </TableCell>
                <TableCell align="right">
                  <TextField
                    type="number"
                    size="small"
                    value={quantities[d.id] ?? 0}
                    onChange={(e) => handleQuantityChange(d.id, e.target.value, d.pendingQuantity)}
                    inputProps={{ min: 0, max: d.pendingQuantity }}
                    sx={{ width: 100 }}
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>

        {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancelar</Button>
        <Button
          variant="contained"
          onClick={handleConfirm}
          disabled={loading}
          startIcon={loading && <CircularProgress size={16} />}
        >
          Confirmar Entrada
        </Button>
      </DialogActions>
    </Dialog>
  );
}
```

---

## PANTALLA 3 COMPLETA: Detalle de Orden

```jsx
// src/modules/warehouse-transfers/pages/WarehouseTransferDetailPage.jsx
import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box, Grid, Card, CardContent, Typography, Button, Divider,
  Table, TableHead, TableRow, TableCell, TableBody, LinearProgress,
  CircularProgress, Chip, IconButton, Tooltip,
} from '@mui/material';
import { ArrowForward, Edit, LocalShipping, Cancel, MoveToInbox, PictureAsPdf } from '@mui/icons-material';
import { useWarehouseTransferDetail } from '../hooks/useWarehouseTransferDetail';
import StatusChip from '../components/StatusChip';
import DispatchModal from '../components/DispatchModal';
import ReceivingModal from '../components/ReceivingModal';
import { reportApi } from '../api/warehouseTransferApi';
import toast from 'react-hot-toast';

export default function WarehouseTransferDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { transfer, loading, actionLoading, dispatch, createReceiving, cancel } =
    useWarehouseTransferDetail(id);

  const [dispatchOpen, setDispatchOpen] = useState(false);
  const [receivingOpen, setReceivingOpen] = useState(false);
  const [printingDispatch, setPrintingDispatch] = useState(false);
  const [printingReceiving, setPrintingReceiving] = useState({});

  // Descarga el PDF de despacho y lo abre en una nueva pestaña (Electron abre con el visor del OS)
  const handlePrintDispatch = async () => {
    setPrintingDispatch(true);
    try {
      const appBaseUrl = window.location.origin; // o tu constante APP_BASE_URL
      const response = await reportApi.generatePdf({
        reportType: 'WarehouseTransferDispatch',
        documentIds: [Number(id)],
        appBaseUrl,
      });
      const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
      window.open(url, '_blank');
    } catch (err) {
      toast.error('Error al generar el PDF de despacho');
    } finally {
      setPrintingDispatch(false);
    }
  };

  // Descarga el PDF de una entrada específica
  const handlePrintReceiving = async (receivingId) => {
    setPrintingReceiving((prev) => ({ ...prev, [receivingId]: true }));
    try {
      const response = await reportApi.generatePdf({
        reportType: 'WarehouseTransferReceiving',
        documentIds: [receivingId],
      });
      const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
      window.open(url, '_blank');
    } catch (err) {
      toast.error('Error al generar el PDF de entrada');
    } finally {
      setPrintingReceiving((prev) => ({ ...prev, [receivingId]: false }));
    }
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 6 }}><CircularProgress /></Box>;
  if (!transfer) return <Typography sx={{ m: 3 }}>Traspaso no encontrado</Typography>;

  const progress = transfer.totalQuantityDispatched > 0
    ? (transfer.totalQuantityReceived / transfer.totalQuantityDispatched) * 100
    : 0;

  const handleDispatch = async (data) => {
    await dispatch(data);
    setDispatchOpen(false);
  };

  const handleReceiving = async (data) => {
    await createReceiving(data);
    setReceivingOpen(false);
  };

  const handleCancel = async () => {
    if (!window.confirm('¿Cancelar esta orden?')) return;
    await cancel();
    navigate('/warehouse-transfers');
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Typography variant="h5" fontWeight="bold">{transfer.code}</Typography>
          <StatusChip status={transfer.status} />
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {transfer.status === 'Draft' && (
            <>
              <Button variant="outlined" startIcon={<Edit />} onClick={() => navigate(`/warehouse-transfers/${id}/edit`)}>
                Editar
              </Button>
              <Button variant="contained" color="warning" startIcon={<LocalShipping />} onClick={() => setDispatchOpen(true)}>
                Despachar
              </Button>
              <Button variant="outlined" color="error" startIcon={<Cancel />} onClick={handleCancel} disabled={actionLoading}>
                Cancelar
              </Button>
            </>
          )}
          {(transfer.status === 'Dispatched' || transfer.status === 'PartiallyReceived') && (
            <Button variant="contained" startIcon={<MoveToInbox />} onClick={() => setReceivingOpen(true)}>
              Registrar Entrada
            </Button>
          )}
          {/* PDF de despacho — disponible desde que la orden está despachada */}
          {transfer.status !== 'Draft' && transfer.status !== 'Cancelled' && (
            <Button
              variant="outlined"
              startIcon={<PictureAsPdf />}
              onClick={() => handlePrintDispatch()}
            >
              Imprimir Despacho
            </Button>
          )}
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Columna izquierda — Info general */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 2 }}>Información General</Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <Typography variant="body2"><strong>{transfer.sourceWarehouseName}</strong></Typography>
                <ArrowForward fontSize="small" color="action" />
                <Typography variant="body2"><strong>{transfer.destinationWarehouseName}</strong></Typography>
              </Box>
              <Typography variant="body2" color="text.secondary">
                Fecha: {new Date(transfer.transferDate).toLocaleDateString('es-MX')}
              </Typography>
              {transfer.notes && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                  Notas: {transfer.notes}
                </Typography>
              )}
              <Divider sx={{ my: 2 }} />
              <Typography variant="body2" color="text.secondary">
                Creado por: {transfer.createdByUserName}
              </Typography>
              {transfer.dispatchedAt && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  Despachado: {new Date(transfer.dispatchedAt).toLocaleString('es-MX')} — {transfer.dispatchedByUserName}
                </Typography>
              )}
              {/* Barra de progreso */}
              {transfer.status !== 'Draft' && transfer.status !== 'Cancelled' && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" sx={{ mb: 0.5 }}>
                    Recepción: {transfer.totalQuantityReceived} / {transfer.totalQuantityDispatched}
                  </Typography>
                  <LinearProgress
                    variant="determinate"
                    value={progress}
                    color={progress >= 100 ? 'success' : 'warning'}
                    sx={{ height: 8, borderRadius: 4 }}
                  />
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Columna derecha — Productos */}
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 2 }}>Detalle de Productos</Typography>
              <Table size="small">
                <TableHead sx={{ backgroundColor: '#f5f5f5' }}>
                  <TableRow>
                    <TableCell>Producto</TableCell>
                    <TableCell align="right">Solicitado</TableCell>
                    <TableCell align="right">Despachado</TableCell>
                    <TableCell align="right">Recibido</TableCell>
                    <TableCell align="right">Pendiente</TableCell>
                    <TableCell align="right">Costo Unit.</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {transfer.details.map((d) => (
                    <TableRow key={d.id} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">{d.productCode}</Typography>
                        <Typography variant="caption" color="text.secondary">{d.productName}</Typography>
                      </TableCell>
                      <TableCell align="right">{d.quantityRequested.toLocaleString()}</TableCell>
                      <TableCell align="right">{d.quantityDispatched.toLocaleString()}</TableCell>
                      <TableCell align="right">{d.quantityReceived.toLocaleString()}</TableCell>
                      <TableCell align="right" sx={{ color: d.pendingQuantity > 0 ? '#f44336' : '#4caf50', fontWeight: 'bold' }}>
                        {d.pendingQuantity.toLocaleString()}
                      </TableCell>
                      <TableCell align="right">
                        {d.unitCost != null ? `$${d.unitCost.toFixed(2)}` : '—'}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>

          {/* Historial de entradas */}
          {transfer.receivings.length > 0 && (
            <Card sx={{ mt: 2 }}>
              <CardContent>
                <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 2 }}>Entradas Registradas</Typography>
                <Table size="small">
                  <TableHead sx={{ backgroundColor: '#f5f5f5' }}>
                    <TableRow>
                      <TableCell>Código</TableCell>
                      <TableCell>Fecha</TableCell>
                      <TableCell>Tipo</TableCell>
                      <TableCell align="right">Productos</TableCell>
                      <TableCell align="right">Unidades</TableCell>
                      <TableCell>Registrado por</TableCell>
                      <TableCell></TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {transfer.receivings.map((r) => (
                      <TableRow key={r.id} hover>
                        <TableCell><strong>{r.code}</strong></TableCell>
                        <TableCell>{new Date(r.receivingDate).toLocaleString('es-MX')}</TableCell>
                        <TableCell>
                          <Chip
                            label={r.receivingType === 'Complete' ? 'Completo' : 'Parcial'}
                            color={r.receivingType === 'Complete' ? 'success' : 'warning'}
                            size="small"
                          />
                        </TableCell>
                        <TableCell align="right">{r.totalProducts}</TableCell>
                        <TableCell align="right">{r.totalQuantityReceived.toLocaleString()}</TableCell>
                        <TableCell>{r.createdByUserName}</TableCell>
                        <TableCell>
                          <Tooltip title="Imprimir PDF de entrada">
                            <IconButton
                              size="small"
                              onClick={() => handlePrintReceiving(r.id)}
                              disabled={!!printingReceiving[r.id]}
                            >
                              {printingReceiving[r.id]
                                ? <CircularProgress size={16} />
                                : <PictureAsPdf fontSize="small" />}
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          )}
        </Grid>
      </Grid>

      {/* Modales */}
      <DispatchModal
        open={dispatchOpen}
        transfer={transfer}
        onClose={() => setDispatchOpen(false)}
        onConfirm={handleDispatch}
        loading={actionLoading}
      />
      <ReceivingModal
        open={receivingOpen}
        transfer={transfer}
        onClose={() => setReceivingOpen(false)}
        onConfirm={handleReceiving}
        loading={actionLoading}
      />
    </Box>
  );
}
```

---

## NOTAS IMPORTANTES

1. **Permisos de acción:** Un usuario puede crear/despachar desde el almacén origen, y otro diferente puede registrar la entrada desde el almacén destino.
2. **Entrada parcial:** El modal de entrada muestra SOLO los productos con `pendingQuantity > 0`. Si un producto ya fue completamente recibido, no aparece.
3. **El estado cambia automáticamente:** La API determina si la recepción es `Partial` o `Complete` (y cambia el estado de la orden a `PartiallyReceived` o `Received`). El frontend solo muestra el resultado.
4. **Inmutabilidad:** Una vez despachada, la orden no puede editarse ni cancelarse.
5. **Stock:** El frontend NO calcula stock. La API valida que haya suficiente antes de despachar y retorna error 400 con mensaje descriptivo si no hay stock.
6. **PDF de Despacho con QR:** El sistema usa plantillas dinámicas editables por el usuario (módulo de `ReportTemplates`). Para generar el PDF de despacho con QR, llama al endpoint genérico `POST /api/reports/generate` con `reportType: "WarehouseTransferDispatch"` y `documentIds: [transferId]`. Incluye `appBaseUrl: window.location.origin` para que el QR apunte a `{appBaseUrl}/warehouse-transfers/{id}/receive`, permitiendo escaneo desde celular para registro de entrada. Si el usuario no ha creado una plantilla personalizada, el sistema usa una plantilla integrada por defecto.
7. **PDF de Entrada:** Igual que el despacho, usa el endpoint genérico con `reportType: "WarehouseTransferReceiving"` y `documentIds: [receivingId]`. El botón de impresión aparece en la columna de acciones de la tabla de entradas del detalle. **Importante:** Los PDFs se generan usando el sistema centralizado de plantillas que permite al usuario editar y personalizar las plantillas HTML desde la interfaz de administración de reportes.

---

## MANEJO DE ERRORES

```javascript
try {
  const response = await warehouseTransferApi.dispatch(id, data);
  toast.success(response.data.message);
  onSuccess(response.data.data);
} catch (error) {
  const msg = error?.response?.data?.message || 'Error inesperado';
  toast.error(msg);
}
```

**Errores comunes de la API:**
- `400` Stock insuficiente para '[producto]' en el almacén de origen. Disponible: X, Solicitado: Y.
- `400` Solo se pueden despachar órdenes en estado Draft. Estado actual: Dispatched.
- `400` La cantidad recibida (X) para el producto '[nombre]' supera la cantidad pendiente (Y).
- `404` Orden de traspaso con ID {id} no encontrada.
