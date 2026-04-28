# Implementación de Módulo de Conteos Cíclicos de Inventario - Electron (React + TypeScript)

## 📋 Contexto del Proyecto
Este documento contiene las instrucciones completas para implementar el módulo de **Conteos Cíclicos de Inventario (Cycle Counting)** en la aplicación Electron con React + TypeScript + Material-UI.

### Objetivo del Módulo
Permitir a los usuarios realizar conteos físicos de inventario de manera sistemática, comparar con cantidades del sistema, identificar variaciones y generar automáticamente ajustes de inventario para corregir diferencias.

### Flujo del Proceso
1. **Crear Sesión de Conteo**: Usuario selecciona almacén, tipo de conteo y asigna responsable
2. **Iniciar Conteo**: El sistema carga los productos a contar según el tipo seleccionado
3. **Registrar Cantidades Físicas**: Usuarios cuentan productos y registran cantidades
4. **Revisar Variaciones**: Sistema calcula diferencias y muestra discrepancias
5. **Aprobar y Completar**: Al aprobar, se generan automáticamente ajustes de inventario y se actualiza stock

### Tipos de Conteo Soportados
- **CYCLE**: Conteo cíclico por rotación (productos seleccionados aleatoriamente)
- **FULL**: Conteo completo del almacén (todos los productos)
- **CATEGORY**: Conteo por categoría específica
- **LOCATION**: Conteo por ubicación física

---

## 🔌 Endpoints del Backend

### Base URL
```
http://192.168.0.72:5000/api/inventory-counts
```

### Autenticación
Todas las peticiones requieren header:
```typescript
headers: {
  'Authorization': `Bearer ${token}`
}
```

### 1. Listar Conteos (Paginado)
```http
GET /api/inventory-counts?page=1&pageSize=20&status=InProgress&countType=CYCLE&warehouseId=1
```

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "code": "CIC-000001",
      "warehouseId": 1,
      "warehouseName": "Almacén Central",
      "countType": "CYCLE",
      "countTypeLabel": "Conteo Cíclico",
      "status": "InProgress",
      "statusLabel": "En Progreso",
      "scheduledDate": "2024-04-28T10:00:00",
      "startedAt": "2024-04-28T10:15:00",
      "completedAt": null,
      "assignedToUserId": 5,
      "assignedToUserName": "Juan Pérez",
      "approvedByUserId": null,
      "approvedByUserName": null,
      "categoryId": null,
      "categoryName": null,
      "totalProducts": 25,
      "countedProducts": 18,
      "productsWithVariance": 5,
      "totalVarianceCost": 1250.50,
      "notes": "Conteo mensual programado",
      "createdByUserId": 1,
      "createdByUserName": "Admin",
      "createdAt": "2024-04-28T09:00:00",
      "companyId": 1
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### 2. Obtener Detalle de Conteo
```http
GET /api/inventory-counts/{id}
```

**Response:**
```json
{
  "id": 1,
  "code": "CIC-000001",
  "warehouseId": 1,
  "warehouseName": "Almacén Central",
  "countType": "CYCLE",
  "countTypeLabel": "Conteo Cíclico",
  "status": "InProgress",
  "statusLabel": "En Progreso",
  "scheduledDate": "2024-04-28T10:00:00",
  "startedAt": "2024-04-28T10:15:00",
  "completedAt": null,
  "assignedToUserId": 5,
  "assignedToUserName": "Juan Pérez",
  "approvedByUserId": null,
  "approvedByUserName": null,
  "categoryId": null,
  "categoryName": null,
  "totalProducts": 25,
  "countedProducts": 18,
  "productsWithVariance": 5,
  "totalVarianceCost": 1250.50,
  "notes": "Conteo mensual programado",
  "createdByUserId": 1,
  "createdByUserName": "Admin",
  "createdAt": "2024-04-28T09:00:00",
  "companyId": 1,
  "details": [
    {
      "id": 1,
      "inventoryCountId": 1,
      "productId": 101,
      "productCode": "PROD-001",
      "productName": "Laptop HP Pavilion",
      "systemQuantity": 50.00,
      "physicalQuantity": 48.00,
      "variance": -2.00,
      "variancePercentage": -4.00,
      "varianceCost": -1800.00,
      "unitCost": 900.00,
      "status": "Counted",
      "statusLabel": "Contado",
      "notes": "2 unidades faltantes",
      "countedByUserId": 5,
      "countedByUserName": "Juan Pérez",
      "countedAt": "2024-04-28T10:30:00",
      "recountRequested": false,
      "recountedByUserId": null,
      "recountedByUserName": null,
      "recountedAt": null
    }
  ]
}
```

### 3. Crear Nuevo Conteo
```http
POST /api/inventory-counts
Content-Type: application/json

{
  "warehouseId": 1,
  "countType": "CYCLE",
  "scheduledDate": "2024-04-30T08:00:00",
  "assignedToUserId": 5,
  "categoryId": null,
  "notes": "Conteo programado fin de mes"
}
```

**Response:** Objeto InventoryCountResponseDto (sin detalles todavía, status "Draft")

### 4. Iniciar Conteo
```http
POST /api/inventory-counts/{id}/start
```

**Response:** Objeto InventoryCountResponseDto completo con detalles generados (status cambia a "InProgress")

### 5. Actualizar Cantidad Física
```http
PATCH /api/inventory-counts/{countId}/details/{detailId}
Content-Type: application/json

{
  "physicalQuantity": 48.00,
  "notes": "2 unidades faltantes en estante A3"
}
```

**Response:** Objeto InventoryCountDetailResponseDto actualizado con variance calculada

### 6. Obtener Detalles Pendientes
```http
GET /api/inventory-counts/{id}/pending
```

**Response:** Array de InventoryCountDetailResponseDto con status "Pending"

### 7. Obtener Detalles con Variaciones
```http
GET /api/inventory-counts/{id}/variances
```

**Response:** Array de InventoryCountDetailResponseDto ordenados por variancePercentage descendente

### 8. Obtener Estadísticas del Conteo
```http
GET /api/inventory-counts/{id}/statistics
```

**Response:**
```json
{
  "totalProducts": 25,
  "countedProducts": 25,
  "pendingProducts": 0,
  "productsWithVariance": 7,
  "accuracyPercentage": 72.00,
  "totalPositiveVariance": 150.50,
  "totalNegativeVariance": -1401.00,
  "totalVarianceCost": -1250.50,
  "averageVariancePercentage": -2.34
}
```

### 9. Completar Conteo (Generar Ajustes)
```http
POST /api/inventory-counts/{id}/complete
Content-Type: application/json

{
  "approvedByUserId": 1,
  "notes": "Conteo aprobado, ajustes generados automáticamente"
}
```

**Response:** Objeto InventoryCountResponseDto con status "Completed" y ajuste generado

### 10. Cancelar Conteo
```http
POST /api/inventory-counts/{id}/cancel
Content-Type: application/json

{
  "notes": "Cancelado por error en proceso"
}
```

### 11. Obtener Tipos de Conteo
```http
GET /api/inventory-counts/types
```

**Response:**
```json
[
  { "value": "CYCLE", "label": "Conteo Cíclico" },
  { "value": "FULL", "label": "Conteo Completo" },
  { "value": "CATEGORY", "label": "Por Categoría" },
  { "value": "LOCATION", "label": "Por Ubicación" }
]
```

### 12. Obtener Estados de Conteo
```http
GET /api/inventory-counts/statuses
```

**Response:**
```json
[
  { "value": "Draft", "label": "Borrador" },
  { "value": "InProgress", "label": "En Progreso" },
  { "value": "Completed", "label": "Completado" },
  { "value": "Cancelled", "label": "Cancelado" }
]
```

---

## 📁 Estructura de Archivos a Crear

```
src/
├── types/
│   └── inventoryCount.types.ts           # Interfaces TypeScript
├── services/
│   └── inventoryCountService.ts          # Servicio API con axios
├── pages/
│   ├── InventoryCountList.tsx            # Listado de conteos
│   ├── InventoryCountCreate.tsx          # Crear nuevo conteo
│   ├── InventoryCountExecute.tsx         # Ejecutar conteo físico
│   ├── InventoryCountReview.tsx          # Revisar variaciones
│   └── InventoryCountDetail.tsx          # Ver detalle completo
├── components/
│   ├── InventoryCountStatusChip.tsx      # Chip de estado
│   ├── InventoryCountFilters.tsx         # Filtros del listado
│   ├── ProductCountRow.tsx               # Fila de producto para contar
│   ├── VarianceIndicator.tsx             # Indicador visual de variación
│   └── CountStatisticsCard.tsx           # Tarjeta de estadísticas
└── routes.tsx                             # Agregar rutas
```

---

## 🎯 TAREA 1: Crear Tipos TypeScript

**Archivo:** `src/types/inventoryCount.types.ts`

```typescript
// ============================================
// TIPOS Y ENUMS
// ============================================

export type CountType = 'CYCLE' | 'FULL' | 'CATEGORY' | 'LOCATION';
export type CountStatus = 'Draft' | 'InProgress' | 'Completed' | 'Cancelled';
export type CountDetailStatus = 'Pending' | 'Counted' | 'Recounted';

export interface CountTypeOption {
  value: CountType;
  label: string;
}

export interface CountStatusOption {
  value: CountStatus;
  label: string;
}

// ============================================
// DTOs DE RESPUESTA
// ============================================

export interface InventoryCountResponseDto {
  id: number;
  code: string;
  warehouseId: number;
  warehouseName: string;
  countType: CountType;
  countTypeLabel: string;
  status: CountStatus;
  statusLabel: string;
  scheduledDate: string;
  startedAt: string | null;
  completedAt: string | null;
  assignedToUserId: number;
  assignedToUserName: string;
  approvedByUserId: number | null;
  approvedByUserName: string | null;
  categoryId: number | null;
  categoryName: string | null;
  totalProducts: number;
  countedProducts: number;
  productsWithVariance: number;
  totalVarianceCost: number;
  notes: string;
  createdByUserId: number;
  createdByUserName: string;
  createdAt: string;
  companyId: number;
  details?: InventoryCountDetailResponseDto[];
}

export interface InventoryCountDetailResponseDto {
  id: number;
  inventoryCountId: number;
  productId: number;
  productCode: string;
  productName: string;
  systemQuantity: number;
  physicalQuantity: number | null;
  variance: number | null;
  variancePercentage: number | null;
  varianceCost: number | null;
  unitCost: number;
  status: CountDetailStatus;
  statusLabel: string;
  notes: string;
  countedByUserId: number | null;
  countedByUserName: string | null;
  countedAt: string | null;
  recountRequested: boolean;
  recountedByUserId: number | null;
  recountedByUserName: string | null;
  recountedAt: string | null;
}

export interface PagedInventoryCountsResponseDto {
  items: InventoryCountResponseDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CountStatisticsDto {
  totalProducts: number;
  countedProducts: number;
  pendingProducts: number;
  productsWithVariance: number;
  accuracyPercentage: number;
  totalPositiveVariance: number;
  totalNegativeVariance: number;
  totalVarianceCost: number;
  averageVariancePercentage: number;
}

// ============================================
// DTOs DE ENTRADA
// ============================================

export interface CreateInventoryCountDto {
  warehouseId: number;
  countType: CountType;
  scheduledDate: string;
  assignedToUserId: number;
  categoryId?: number | null;
  notes: string;
}

export interface UpdateCountDetailDto {
  physicalQuantity: number;
  notes?: string;
}

export interface CompleteInventoryCountDto {
  approvedByUserId: number;
  notes?: string;
}

export interface CancelInventoryCountDto {
  notes: string;
}

// ============================================
// FILTROS
// ============================================

export interface InventoryCountFilters {
  page: number;
  pageSize: number;
  status?: CountStatus;
  countType?: CountType;
  warehouseId?: number;
  assignedToUserId?: number;
  dateFrom?: string;
  dateTo?: string;
}
```

---

## 🎯 TAREA 2: Crear Servicio API

**Archivo:** `src/services/inventoryCountService.ts`

```typescript
import axios, { AxiosResponse } from 'axios';
import {
  InventoryCountResponseDto,
  PagedInventoryCountsResponseDto,
  CreateInventoryCountDto,
  UpdateCountDetailDto,
  CompleteInventoryCountDto,
  CancelInventoryCountDto,
  InventoryCountDetailResponseDto,
  CountStatisticsDto,
  InventoryCountFilters,
  CountTypeOption,
  CountStatusOption
} from '../types/inventoryCount.types';

const API_BASE_URL = 'http://192.168.0.72:5000/api/inventory-counts';

// Obtener token del localStorage
const getAuthHeaders = () => {
  const token = localStorage.getItem('token');
  return {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  };
};

// ============================================
// OPERACIONES CRUD
// ============================================

export const getInventoryCounts = async (
  filters: InventoryCountFilters
): Promise<PagedInventoryCountsResponseDto> => {
  const params = new URLSearchParams();
  params.append('page', filters.page.toString());
  params.append('pageSize', filters.pageSize.toString());
  
  if (filters.status) params.append('status', filters.status);
  if (filters.countType) params.append('countType', filters.countType);
  if (filters.warehouseId) params.append('warehouseId', filters.warehouseId.toString());
  if (filters.assignedToUserId) params.append('assignedToUserId', filters.assignedToUserId.toString());
  if (filters.dateFrom) params.append('dateFrom', filters.dateFrom);
  if (filters.dateTo) params.append('dateTo', filters.dateTo);

  const response: AxiosResponse<PagedInventoryCountsResponseDto> = await axios.get(
    `${API_BASE_URL}?${params.toString()}`,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const getInventoryCountById = async (id: number): Promise<InventoryCountResponseDto> => {
  const response: AxiosResponse<InventoryCountResponseDto> = await axios.get(
    `${API_BASE_URL}/${id}`,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const createInventoryCount = async (
  dto: CreateInventoryCountDto
): Promise<InventoryCountResponseDto> => {
  const response: AxiosResponse<InventoryCountResponseDto> = await axios.post(
    API_BASE_URL,
    dto,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

// ============================================
// OPERACIONES DEL FLUJO DE CONTEO
// ============================================

export const startInventoryCount = async (id: number): Promise<InventoryCountResponseDto> => {
  const response: AxiosResponse<InventoryCountResponseDto> = await axios.post(
    `${API_BASE_URL}/${id}/start`,
    {},
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const updateCountDetail = async (
  countId: number,
  detailId: number,
  dto: UpdateCountDetailDto
): Promise<InventoryCountDetailResponseDto> => {
  const response: AxiosResponse<InventoryCountDetailResponseDto> = await axios.patch(
    `${API_BASE_URL}/${countId}/details/${detailId}`,
    dto,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const completeInventoryCount = async (
  id: number,
  dto: CompleteInventoryCountDto
): Promise<InventoryCountResponseDto> => {
  const response: AxiosResponse<InventoryCountResponseDto> = await axios.post(
    `${API_BASE_URL}/${id}/complete`,
    dto,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const cancelInventoryCount = async (
  id: number,
  dto: CancelInventoryCountDto
): Promise<void> => {
  await axios.post(
    `${API_BASE_URL}/${id}/cancel`,
    dto,
    { headers: getAuthHeaders() }
  );
};

// ============================================
// CONSULTAS ESPECIALIZADAS
// ============================================

export const getPendingDetails = async (
  countId: number
): Promise<InventoryCountDetailResponseDto[]> => {
  const response: AxiosResponse<InventoryCountDetailResponseDto[]> = await axios.get(
    `${API_BASE_URL}/${countId}/pending`,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const getDetailsWithVariances = async (
  countId: number
): Promise<InventoryCountDetailResponseDto[]> => {
  const response: AxiosResponse<InventoryCountDetailResponseDto[]> = await axios.get(
    `${API_BASE_URL}/${countId}/variances`,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const getCountStatistics = async (countId: number): Promise<CountStatisticsDto> => {
  const response: AxiosResponse<CountStatisticsDto> = await axios.get(
    `${API_BASE_URL}/${countId}/statistics`,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

// ============================================
// CATÁLOGOS
// ============================================

export const getCountTypes = async (): Promise<CountTypeOption[]> => {
  const response: AxiosResponse<CountTypeOption[]> = await axios.get(
    `${API_BASE_URL}/types`,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};

export const getCountStatuses = async (): Promise<CountStatusOption[]> => {
  const response: AxiosResponse<CountStatusOption[]> = await axios.get(
    `${API_BASE_URL}/statuses`,
    { headers: getAuthHeaders() }
  );
  
  return response.data;
};
```

---

## 🎯 TAREA 3: Crear Página de Listado

**Archivo:** `src/pages/InventoryCountList.tsx`

```typescript
import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  IconButton,
  LinearProgress,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Typography,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Tooltip
} from '@mui/material';
import {
  Add as AddIcon,
  Visibility as ViewIcon,
  PlayArrow as StartIcon,
  Assessment as StatsIcon,
  FilterList as FilterIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { getInventoryCounts, getCountTypes, getCountStatuses } from '../services/inventoryCountService';
import {
  InventoryCountResponseDto,
  CountStatus,
  CountType,
  InventoryCountFilters
} from '../types/inventoryCount.types';
import { format } from 'date-fns';

const InventoryCountList: React.FC = () => {
  const navigate = useNavigate();
  
  const [counts, setCounts] = useState<InventoryCountResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  
  const [filters, setFilters] = useState<InventoryCountFilters>({
    page: 1,
    pageSize: 20,
    status: undefined,
    countType: undefined,
    warehouseId: undefined
  });

  const [countTypes, setCountTypes] = useState<any[]>([]);
  const [statuses, setStatuses] = useState<any[]>([]);

  useEffect(() => {
    loadCatalogs();
    loadCounts();
  }, [filters]);

  const loadCatalogs = async () => {
    try {
      const [typesData, statusesData] = await Promise.all([
        getCountTypes(),
        getCountStatuses()
      ]);
      setCountTypes(typesData);
      setStatuses(statusesData);
    } catch (error) {
      console.error('Error cargando catálogos:', error);
    }
  };

  const loadCounts = async () => {
    setLoading(true);
    try {
      const response = await getInventoryCounts(filters);
      setCounts(response.items);
      setTotalCount(response.totalCount);
    } catch (error) {
      console.error('Error cargando conteos:', error);
    } finally {
      setLoading(false);
    }
  };

  const handlePageChange = (event: unknown, newPage: number) => {
    setFilters({ ...filters, page: newPage + 1 });
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setFilters({ ...filters, pageSize: parseInt(event.target.value, 10), page: 1 });
  };

  const getStatusColor = (status: CountStatus): 'default' | 'primary' | 'success' | 'error' => {
    switch (status) {
      case 'Draft': return 'default';
      case 'InProgress': return 'primary';
      case 'Completed': return 'success';
      case 'Cancelled': return 'error';
      default: return 'default';
    }
  };

  const getProgressPercentage = (count: InventoryCountResponseDto): number => {
    if (count.totalProducts === 0) return 0;
    return (count.countedProducts / count.totalProducts) * 100;
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4">
          Conteos Cíclicos de Inventario
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/inventory-counts/create')}
        >
          Nuevo Conteo
        </Button>
      </Box>

      {/* Filtros */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Estado</InputLabel>
                <Select
                  value={filters.status || ''}
                  label="Estado"
                  onChange={(e) => setFilters({ ...filters, status: e.target.value as CountStatus || undefined, page: 1 })}
                >
                  <MenuItem value="">Todos</MenuItem>
                  {statuses.map((s) => (
                    <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid item xs={12} sm={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Tipo de Conteo</InputLabel>
                <Select
                  value={filters.countType || ''}
                  label="Tipo de Conteo"
                  onChange={(e) => setFilters({ ...filters, countType: e.target.value as CountType || undefined, page: 1 })}
                >
                  <MenuItem value="">Todos</MenuItem>
                  {countTypes.map((t) => (
                    <MenuItem key={t.value} value={t.value}>{t.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>

            <Grid item xs={12} sm={6}>
              <Typography variant="body2" color="text.secondary">
                Total de conteos: {totalCount}
              </Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Tabla */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Código</TableCell>
              <TableCell>Almacén</TableCell>
              <TableCell>Tipo</TableCell>
              <TableCell>Estado</TableCell>
              <TableCell>Progreso</TableCell>
              <TableCell>Asignado a</TableCell>
              <TableCell>Fecha Programada</TableCell>
              <TableCell>Variaciones</TableCell>
              <TableCell align="right">Acciones</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading && (
              <TableRow>
                <TableCell colSpan={9}>
                  <LinearProgress />
                </TableCell>
              </TableRow>
            )}
            {!loading && counts.length === 0 && (
              <TableRow>
                <TableCell colSpan={9} align="center">
                  <Typography variant="body2" color="text.secondary">
                    No se encontraron conteos
                  </Typography>
                </TableCell>
              </TableRow>
            )}
            {!loading && counts.map((count) => (
              <TableRow key={count.id} hover>
                <TableCell>
                  <Typography variant="body2" fontWeight="bold">
                    {count.code}
                  </Typography>
                </TableCell>
                <TableCell>{count.warehouseName}</TableCell>
                <TableCell>
                  <Chip label={count.countTypeLabel} size="small" variant="outlined" />
                </TableCell>
                <TableCell>
                  <Chip 
                    label={count.statusLabel} 
                    size="small" 
                    color={getStatusColor(count.status)}
                  />
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Box sx={{ width: '100%' }}>
                      <LinearProgress 
                        variant="determinate" 
                        value={getProgressPercentage(count)}
                        sx={{ height: 8, borderRadius: 1 }}
                      />
                    </Box>
                    <Typography variant="caption">
                      {count.countedProducts}/{count.totalProducts}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell>{count.assignedToUserName}</TableCell>
                <TableCell>
                  {format(new Date(count.scheduledDate), 'dd/MM/yyyy HH:mm')}
                </TableCell>
                <TableCell>
                  {count.productsWithVariance > 0 && (
                    <Chip 
                      label={`${count.productsWithVariance} productos`}
                      size="small"
                      color="warning"
                    />
                  )}
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Ver Detalle">
                    <IconButton 
                      size="small"
                      onClick={() => navigate(`/inventory-counts/${count.id}`)}
                    >
                      <ViewIcon />
                    </IconButton>
                  </Tooltip>
                  {count.status === 'Draft' && (
                    <Tooltip title="Iniciar Conteo">
                      <IconButton 
                        size="small"
                        color="primary"
                        onClick={() => navigate(`/inventory-counts/${count.id}/execute`)}
                      >
                        <StartIcon />
                      </IconButton>
                    </Tooltip>
                  )}
                  {count.status === 'InProgress' && (
                    <Tooltip title="Continuar Conteo">
                      <IconButton 
                        size="small"
                        color="primary"
                        onClick={() => navigate(`/inventory-counts/${count.id}/execute`)}
                      >
                        <PlayArrowIcon />
                      </IconButton>
                    </Tooltip>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={totalCount}
          page={filters.page - 1}
          onPageChange={handlePageChange}
          rowsPerPage={filters.pageSize}
          onRowsPerPageChange={handleRowsPerPageChange}
          labelRowsPerPage="Filas por página"
          labelDisplayedRows={({ from, to, count }) => `${from}-${to} de ${count}`}
        />
      </TableContainer>
    </Box>
  );
};

export default InventoryCountList;
```

---

## 🎯 TAREA 4: Crear Página de Creación

**Archivo:** `src/pages/InventoryCountCreate.tsx`

```typescript
import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
  Alert
} from '@mui/material';
import { Save as SaveIcon, Cancel as CancelIcon } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { createInventoryCount, getCountTypes } from '../services/inventoryCountService';
import { CreateInventoryCountDto, CountType } from '../types/inventoryCount.types';

// Esquema de validación
const schema = yup.object().shape({
  warehouseId: yup.number().required('Almacén es requerido').positive(),
  countType: yup.string().required('Tipo de conteo es requerido'),
  scheduledDate: yup.string().required('Fecha programada es requerida'),
  assignedToUserId: yup.number().required('Usuario asignado es requerido').positive(),
  categoryId: yup.number().nullable(),
  notes: yup.string().max(500, 'Máximo 500 caracteres')
});

const InventoryCountCreate: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [countTypes, setCountTypes] = useState<any[]>([]);
  const [warehouses, setWarehouses] = useState<any[]>([]); // Cargar de tu servicio de almacenes
  const [users, setUsers] = useState<any[]>([]); // Cargar de tu servicio de usuarios
  const [categories, setCategories] = useState<any[]>([]); // Cargar de tu servicio de categorías

  const { control, handleSubmit, watch, formState: { errors } } = useForm<CreateInventoryCountDto>({
    resolver: yupResolver(schema),
    defaultValues: {
      warehouseId: 0,
      countType: 'CYCLE' as CountType,
      scheduledDate: new Date().toISOString().slice(0, 16),
      assignedToUserId: 0,
      categoryId: null,
      notes: ''
    }
  });

  const selectedCountType = watch('countType');

  useEffect(() => {
    loadCatalogs();
  }, []);

  const loadCatalogs = async () => {
    try {
      const typesData = await getCountTypes();
      setCountTypes(typesData);
      
      // TODO: Cargar almacenes, usuarios y categorías desde tus servicios
      // setWarehouses(await getWarehouses());
      // setUsers(await getUsers());
      // setCategories(await getCategories());
    } catch (error) {
      console.error('Error cargando catálogos:', error);
    }
  };

  const onSubmit = async (data: CreateInventoryCountDto) => {
    setLoading(true);
    setError(null);
    
    try {
      const created = await createInventoryCount(data);
      navigate(`/inventory-counts/${created.id}/execute`);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al crear conteo');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Nuevo Conteo Cíclico
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Card>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)}>
            <Grid container spacing={3}>
              {/* Almacén */}
              <Grid item xs={12} md={6}>
                <Controller
                  name="warehouseId"
                  control={control}
                  render={({ field }) => (
                    <FormControl fullWidth error={!!errors.warehouseId}>
                      <InputLabel>Almacén *</InputLabel>
                      <Select {...field} label="Almacén *">
                        <MenuItem value={0}>Seleccione almacén</MenuItem>
                        {warehouses.map((w) => (
                          <MenuItem key={w.id} value={w.id}>{w.name}</MenuItem>
                        ))}
                      </Select>
                      {errors.warehouseId && (
                        <Typography variant="caption" color="error">
                          {errors.warehouseId.message}
                        </Typography>
                      )}
                    </FormControl>
                  )}
                />
              </Grid>

              {/* Tipo de Conteo */}
              <Grid item xs={12} md={6}>
                <Controller
                  name="countType"
                  control={control}
                  render={({ field }) => (
                    <FormControl fullWidth error={!!errors.countType}>
                      <InputLabel>Tipo de Conteo *</InputLabel>
                      <Select {...field} label="Tipo de Conteo *">
                        {countTypes.map((t) => (
                          <MenuItem key={t.value} value={t.value}>{t.label}</MenuItem>
                        ))}
                      </Select>
                      {errors.countType && (
                        <Typography variant="caption" color="error">
                          {errors.countType.message}
                        </Typography>
                      )}
                    </FormControl>
                  )}
                />
              </Grid>

              {/* Fecha Programada */}
              <Grid item xs={12} md={6}>
                <Controller
                  name="scheduledDate"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Fecha Programada *"
                      type="datetime-local"
                      InputLabelProps={{ shrink: true }}
                      error={!!errors.scheduledDate}
                      helperText={errors.scheduledDate?.message}
                    />
                  )}
                />
              </Grid>

              {/* Usuario Asignado */}
              <Grid item xs={12} md={6}>
                <Controller
                  name="assignedToUserId"
                  control={control}
                  render={({ field }) => (
                    <FormControl fullWidth error={!!errors.assignedToUserId}>
                      <InputLabel>Asignado a *</InputLabel>
                      <Select {...field} label="Asignado a *">
                        <MenuItem value={0}>Seleccione usuario</MenuItem>
                        {users.map((u) => (
                          <MenuItem key={u.id} value={u.id}>{u.name}</MenuItem>
                        ))}
                      </Select>
                      {errors.assignedToUserId && (
                        <Typography variant="caption" color="error">
                          {errors.assignedToUserId.message}
                        </Typography>
                      )}
                    </FormControl>
                  )}
                />
              </Grid>

              {/* Categoría (solo si tipo es CATEGORY) */}
              {selectedCountType === 'CATEGORY' && (
                <Grid item xs={12} md={6}>
                  <Controller
                    name="categoryId"
                    control={control}
                    render={({ field }) => (
                      <FormControl fullWidth>
                        <InputLabel>Categoría *</InputLabel>
                        <Select {...field} label="Categoría *">
                          <MenuItem value={0}>Seleccione categoría</MenuItem>
                          {categories.map((c) => (
                            <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
                          ))}
                        </Select>
                      </FormControl>
                    )}
                  />
                </Grid>
              )}

              {/* Notas */}
              <Grid item xs={12}>
                <Controller
                  name="notes"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label="Notas"
                      multiline
                      rows={3}
                      error={!!errors.notes}
                      helperText={errors.notes?.message}
                    />
                  )}
                />
              </Grid>

              {/* Botones */}
              <Grid item xs={12}>
                <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                  <Button
                    variant="outlined"
                    startIcon={<CancelIcon />}
                    onClick={() => navigate('/inventory-counts')}
                  >
                    Cancelar
                  </Button>
                  <Button
                    type="submit"
                    variant="contained"
                    startIcon={<SaveIcon />}
                    disabled={loading}
                  >
                    {loading ? 'Creando...' : 'Crear e Iniciar Conteo'}
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
};

export default InventoryCountCreate;
```

---

## 🎯 TAREA 5: Crear Página de Ejecución de Conteo

**Archivo:** `src/pages/InventoryCountExecute.tsx`

```typescript
import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  IconButton,
  LinearProgress,
  Paper,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  TextField,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert
} from '@mui/material';
import {
  Check as CheckIcon,
  PlayArrow as StartIcon,
  Save as SaveIcon,
  Warning as WarningIcon
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import {
  getInventoryCountById,
  startInventoryCount,
  updateCountDetail,
  getPendingDetails,
  getDetailsWithVariances
} from '../services/inventoryCountService';
import {
  InventoryCountResponseDto,
  InventoryCountDetailResponseDto,
  UpdateCountDetailDto
} from '../types/inventoryCount.types';

const InventoryCountExecute: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [count, setCount] = useState<InventoryCountResponseDto | null>(null);
  const [pendingDetails, setPendingDetails] = useState<InventoryCountDetailResponseDto[]>([]);
  const [varianceDetails, setVarianceDetails] = useState<InventoryCountDetailResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [tabValue, setTabValue] = useState(0);
  
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedDetail, setSelectedDetail] = useState<InventoryCountDetailResponseDto | null>(null);
  const [physicalQuantity, setPhysicalQuantity] = useState<number>(0);
  const [notes, setNotes] = useState<string>('');

  useEffect(() => {
    loadCountData();
  }, [id]);

  const loadCountData = async () => {
    if (!id) return;
    
    setLoading(true);
    try {
      const countData = await getInventoryCountById(parseInt(id));
      setCount(countData);
      
      if (countData.status === 'InProgress') {
        const [pending, variances] = await Promise.all([
          getPendingDetails(parseInt(id)),
          getDetailsWithVariances(parseInt(id))
        ]);
        setPendingDetails(pending);
        setVarianceDetails(variances);
      }
    } catch (error) {
      console.error('Error cargando conteo:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleStartCount = async () => {
    if (!id) return;
    
    setLoading(true);
    try {
      await startInventoryCount(parseInt(id));
      await loadCountData();
    } catch (error) {
      console.error('Error iniciando conteo:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenEditDialog = (detail: InventoryCountDetailResponseDto) => {
    setSelectedDetail(detail);
    setPhysicalQuantity(detail.physicalQuantity || detail.systemQuantity);
    setNotes(detail.notes || '');
    setEditDialogOpen(true);
  };

  const handleSaveQuantity = async () => {
    if (!count || !selectedDetail) return;
    
    setLoading(true);
    try {
      const dto: UpdateCountDetailDto = {
        physicalQuantity,
        notes
      };
      
      await updateCountDetail(count.id, selectedDetail.id, dto);
      setEditDialogOpen(false);
      await loadCountData();
    } catch (error) {
      console.error('Error guardando cantidad:', error);
    } finally {
      setLoading(false);
    }
  };

  const getProgressPercentage = (): number => {
    if (!count || count.totalProducts === 0) return 0;
    return (count.countedProducts / count.totalProducts) * 100;
  };

  if (!count) {
    return <LinearProgress />;
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" gutterBottom>
          Conteo: {count.code}
        </Typography>
        <Grid container spacing={2}>
          <Grid item>
            <Chip label={count.statusLabel} color="primary" />
          </Grid>
          <Grid item>
            <Chip label={count.countTypeLabel} variant="outlined" />
          </Grid>
          <Grid item>
            <Typography variant="body2">
              Almacén: {count.warehouseName}
            </Typography>
          </Grid>
        </Grid>
      </Box>

      {/* Progreso */}
      {count.status === 'InProgress' && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Progreso del Conteo
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box sx={{ flexGrow: 1 }}>
                <LinearProgress 
                  variant="determinate" 
                  value={getProgressPercentage()}
                  sx={{ height: 12, borderRadius: 1 }}
                />
              </Box>
              <Typography variant="body2">
                {count.countedProducts} / {count.totalProducts} productos
              </Typography>
            </Box>
            {count.productsWithVariance > 0 && (
              <Alert severity="warning" icon={<WarningIcon />} sx={{ mt: 2 }}>
                Se encontraron {count.productsWithVariance} productos con variaciones
              </Alert>
            )}
          </CardContent>
        </Card>
      )}

      {/* Botón Iniciar (solo si está en Draft) */}
      {count.status === 'Draft' && (
        <Box sx={{ mb: 3 }}>
          <Button
            variant="contained"
            size="large"
            startIcon={<StartIcon />}
            onClick={handleStartCount}
            disabled={loading}
          >
            Iniciar Conteo
          </Button>
        </Box>
      )}

      {/* Tabs de Pendientes y Variaciones */}
      {count.status === 'InProgress' && (
        <>
          <Tabs value={tabValue} onChange={(e, v) => setTabValue(v)} sx={{ mb: 2 }}>
            <Tab label={`Pendientes (${pendingDetails.length})`} />
            <Tab label={`Variaciones (${varianceDetails.length})`} />
          </Tabs>

          {/* Tab Pendientes */}
          {tabValue === 0 && (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Código</TableCell>
                    <TableCell>Producto</TableCell>
                    <TableCell align="right">Cant. Sistema</TableCell>
                    <TableCell align="right">Cant. Física</TableCell>
                    <TableCell>Estado</TableCell>
                    <TableCell align="right">Acciones</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {pendingDetails.map((detail) => (
                    <TableRow key={detail.id}>
                      <TableCell>{detail.productCode}</TableCell>
                      <TableCell>{detail.productName}</TableCell>
                      <TableCell align="right">{detail.systemQuantity}</TableCell>
                      <TableCell align="right">
                        {detail.physicalQuantity !== null ? detail.physicalQuantity : '-'}
                      </TableCell>
                      <TableCell>
                        <Chip label={detail.statusLabel} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={() => handleOpenEditDialog(detail)}
                        >
                          Contar
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          {/* Tab Variaciones */}
          {tabValue === 1 && (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Código</TableCell>
                    <TableCell>Producto</TableCell>
                    <TableCell align="right">Cant. Sistema</TableCell>
                    <TableCell align="right">Cant. Física</TableCell>
                    <TableCell align="right">Variación</TableCell>
                    <TableCell align="right">% Variación</TableCell>
                    <TableCell align="right">Costo Variación</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {varianceDetails.map((detail) => (
                    <TableRow key={detail.id}>
                      <TableCell>{detail.productCode}</TableCell>
                      <TableCell>{detail.productName}</TableCell>
                      <TableCell align="right">{detail.systemQuantity}</TableCell>
                      <TableCell align="right">{detail.physicalQuantity}</TableCell>
                      <TableCell 
                        align="right"
                        sx={{ 
                          color: (detail.variance || 0) < 0 ? 'error.main' : 'success.main',
                          fontWeight: 'bold'
                        }}
                      >
                        {detail.variance}
                      </TableCell>
                      <TableCell align="right">
                        {detail.variancePercentage?.toFixed(2)}%
                      </TableCell>
                      <TableCell 
                        align="right"
                        sx={{ 
                          color: (detail.varianceCost || 0) < 0 ? 'error.main' : 'success.main'
                        }}
                      >
                        ${detail.varianceCost?.toFixed(2)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          {/* Botón Completar */}
          {pendingDetails.length === 0 && (
            <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
              <Button
                variant="contained"
                color="success"
                startIcon={<CheckIcon />}
                onClick={() => navigate(`/inventory-counts/${count.id}/review`)}
              >
                Revisar y Completar
              </Button>
            </Box>
          )}
        </>
      )}

      {/* Dialog para Ingresar Cantidad */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Registrar Cantidad Física
        </DialogTitle>
        <DialogContent>
          {selectedDetail && (
            <Box sx={{ pt: 2 }}>
              <Typography variant="body2" gutterBottom>
                <strong>Producto:</strong> {selectedDetail.productName}
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Código:</strong> {selectedDetail.productCode}
              </Typography>
              <Typography variant="body2" gutterBottom>
                <strong>Cantidad en Sistema:</strong> {selectedDetail.systemQuantity}
              </Typography>
              
              <TextField
                fullWidth
                type="number"
                label="Cantidad Física *"
                value={physicalQuantity}
                onChange={(e) => setPhysicalQuantity(parseFloat(e.target.value))}
                sx={{ mt: 2 }}
                inputProps={{ step: 0.01 }}
              />
              
              <TextField
                fullWidth
                label="Notas"
                multiline
                rows={3}
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                sx={{ mt: 2 }}
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>
            Cancelar
          </Button>
          <Button 
            variant="contained" 
            onClick={handleSaveQuantity}
            disabled={loading}
            startIcon={<SaveIcon />}
          >
            Guardar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default InventoryCountExecute;
```

---

## 🎯 TAREA 6: Crear Página de Revisión y Aprobación

**Archivo:** `src/pages/InventoryCountReview.tsx`

```typescript
import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions
} from '@mui/material';
import {
  CheckCircle as ApproveIcon,
  Warning as WarningIcon
} from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import {
  getInventoryCountById,
  getCountStatistics,
  completeInventoryCount
} from '../services/inventoryCountService';
import {
  InventoryCountResponseDto,
  CountStatisticsDto,
  CompleteInventoryCountDto
} from '../types/inventoryCount.types';

const InventoryCountReview: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [count, setCount] = useState<InventoryCountResponseDto | null>(null);
  const [statistics, setStatistics] = useState<CountStatisticsDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [approvalNotes, setApprovalNotes] = useState<string>('');
  const [error, setError] = useState<string | null>(null);

  // Obtener ID de usuario actual del localStorage o contexto
  const currentUserId = parseInt(localStorage.getItem('userId') || '0');

  useEffect(() => {
    loadData();
  }, [id]);

  const loadData = async () => {
    if (!id) return;
    
    setLoading(true);
    try {
      const [countData, statsData] = await Promise.all([
        getInventoryCountById(parseInt(id)),
        getCountStatistics(parseInt(id))
      ]);
      
      setCount(countData);
      setStatistics(statsData);
    } catch (error) {
      console.error('Error cargando datos:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async () => {
    if (!count) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const dto: CompleteInventoryCountDto = {
        approvedByUserId: currentUserId,
        notes: approvalNotes
      };
      
      await completeInventoryCount(count.id, dto);
      navigate(`/inventory-counts/${count.id}`);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al completar conteo');
    } finally {
      setLoading(false);
      setConfirmDialogOpen(false);
    }
  };

  if (!count || !statistics) {
    return <Typography>Cargando...</Typography>;
  }

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Revisión de Conteo: {count.code}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Estadísticas Generales */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography variant="h6" color="primary">
                {statistics.accuracyPercentage.toFixed(2)}%
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Precisión del Conteo
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography variant="h6">
                {statistics.productsWithVariance}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Productos con Variación
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography 
                variant="h6" 
                color={statistics.totalVarianceCost < 0 ? 'error.main' : 'success.main'}
              >
                ${Math.abs(statistics.totalVarianceCost).toFixed(2)}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Costo Total de Variación
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography variant="h6">
                {statistics.totalProducts}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Total de Productos Contados
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Detalles con Variación */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Productos con Variaciones
          </Typography>
          
          {count.details && count.details.filter(d => d.variance !== 0).length === 0 ? (
            <Alert severity="success">
              No se encontraron variaciones. El inventario está correcto.
            </Alert>
          ) : (
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Código</TableCell>
                    <TableCell>Producto</TableCell>
                    <TableCell align="right">Sistema</TableCell>
                    <TableCell align="right">Físico</TableCell>
                    <TableCell align="right">Variación</TableCell>
                    <TableCell align="right">% Variación</TableCell>
                    <TableCell align="right">Costo Variación</TableCell>
                    <TableCell>Notas</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {count.details
                    ?.filter(d => d.variance !== 0)
                    .sort((a, b) => Math.abs(b.variancePercentage || 0) - Math.abs(a.variancePercentage || 0))
                    .map((detail) => (
                      <TableRow key={detail.id}>
                        <TableCell>{detail.productCode}</TableCell>
                        <TableCell>{detail.productName}</TableCell>
                        <TableCell align="right">{detail.systemQuantity}</TableCell>
                        <TableCell align="right">{detail.physicalQuantity}</TableCell>
                        <TableCell 
                          align="right"
                          sx={{ 
                            color: (detail.variance || 0) < 0 ? 'error.main' : 'success.main',
                            fontWeight: 'bold'
                          }}
                        >
                          {detail.variance}
                        </TableCell>
                        <TableCell align="right">
                          {detail.variancePercentage?.toFixed(2)}%
                        </TableCell>
                        <TableCell 
                          align="right"
                          sx={{ 
                            color: (detail.varianceCost || 0) < 0 ? 'error.main' : 'success.main'
                          }}
                        >
                          ${detail.varianceCost?.toFixed(2)}
                        </TableCell>
                        <TableCell>{detail.notes}</TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Información de Aprobación */}
      <Alert severity="info" icon={<WarningIcon />} sx={{ mb: 3 }}>
        Al aprobar este conteo, se generarán automáticamente ajustes de inventario para todas las variaciones encontradas. Esta acción no se puede deshacer.
      </Alert>

      {/* Botón Aprobar */}
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
        <Button
          variant="outlined"
          onClick={() => navigate(`/inventory-counts/${count.id}/execute`)}
        >
          Volver al Conteo
        </Button>
        <Button
          variant="contained"
          color="success"
          startIcon={<ApproveIcon />}
          onClick={() => setConfirmDialogOpen(true)}
          disabled={loading}
        >
          Aprobar y Completar
        </Button>
      </Box>

      {/* Dialog de Confirmación */}
      <Dialog open={confirmDialogOpen} onClose={() => setConfirmDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Confirmar Aprobación de Conteo
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" gutterBottom>
            ¿Está seguro de aprobar este conteo? Se generarán automáticamente:
          </Typography>
          <ul>
            <li>
              <Typography variant="body2">
                {statistics.productsWithVariance} ajustes de inventario
              </Typography>
            </li>
            <li>
              <Typography variant="body2">
                Movimientos en el Kardex
              </Typography>
            </li>
            <li>
              <Typography variant="body2">
                Actualización del stock en el sistema
              </Typography>
            </li>
          </ul>
          
          <TextField
            fullWidth
            label="Notas de Aprobación"
            multiline
            rows={3}
            value={approvalNotes}
            onChange={(e) => setApprovalNotes(e.target.value)}
            sx={{ mt: 2 }}
            placeholder="Conteo aprobado, ajustes generados automáticamente..."
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDialogOpen(false)}>
            Cancelar
          </Button>
          <Button 
            variant="contained" 
            color="success"
            onClick={handleApprove}
            disabled={loading}
          >
            Confirmar Aprobación
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default InventoryCountReview;
```

---

## 🎯 TAREA 7: Agregar Rutas

**Archivo:** `src/routes.tsx`

```typescript
import InventoryCountList from './pages/InventoryCountList';
import InventoryCountCreate from './pages/InventoryCountCreate';
import InventoryCountExecute from './pages/InventoryCountExecute';
import InventoryCountReview from './pages/InventoryCountReview';
import InventoryCountDetail from './pages/InventoryCountDetail';

// Agregar al array de rutas existente
{
  path: '/inventory-counts',
  element: <InventoryCountList />
},
{
  path: '/inventory-counts/create',
  element: <InventoryCountCreate />
},
{
  path: '/inventory-counts/:id',
  element: <InventoryCountDetail />
},
{
  path: '/inventory-counts/:id/execute',
  element: <InventoryCountExecute />
},
{
  path: '/inventory-counts/:id/review',
  element: <InventoryCountReview />
}
```

---

## 🎯 TAREA 8: Agregar al Sidebar

Agregar en el menú de Inventario:

```typescript
{
  title: 'Conteos Cíclicos',
  icon: <InventoryIcon />,
  path: '/inventory-counts',
  permission: 'Inventario.View'
}
```

---

## 📝 Notas Finales

### Características Clave Implementadas
- ✅ Listado paginado con filtros por estado, tipo, almacén
- ✅ Creación de sesiones de conteo con 4 tipos (CYCLE, FULL, CATEGORY, LOCATION)
- ✅ Ejecución de conteo con tabs de pendientes y variaciones
- ✅ Revisión de variaciones con estadísticas completas
- ✅ Aprobación que genera ajustes automáticos
- ✅ Indicadores visuales de progreso con LinearProgress
- ✅ Chips de estado con colores según status
- ✅ Dialogs de confirmación para acciones críticas
- ✅ Manejo de errores con Alerts
- ✅ Validación con react-hook-form + yup
- ✅ Formato de código **CIC-XXXXXX** (Conteo de Inventario Cíclico)

### Mejoras Sugeridas (Opcional)
- Scanner de códigos de barras para agilizar conteo
- Exportación a Excel de reportes de variaciones
- Notificaciones push cuando se asigna un conteo
- Modo offline con sincronización posterior
- Captura de fotos de productos con variaciones
- Historial de conteos previos por producto
- Dashboard con métricas de precisión por usuario/almacén

### Dependencias Requeridas
```bash
npm install @mui/material @mui/icons-material @emotion/react @emotion/styled
npm install react-router-dom axios
npm install react-hook-form @hookform/resolvers yup
npm install date-fns
```

---

**¡Implementación completa lista para desarrollo!** 🚀
