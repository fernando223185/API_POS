# 📦 IMPLEMENTACIÓN DE AJUSTES DE INVENTARIO - ELECTRON FRONTEND

## 🎯 OBJETIVO
Implementar el módulo completo de **Ajustes de Inventario** en el frontend Electron para permitir la corrección de discrepancias entre el stock físico y el sistema. Este módulo se integra automáticamente con el Kardex para registrar todos los movimientos.

---

## 📋 CONTEXTO DEL SISTEMA

### **Backend ya implementado:**
- ✅ Entidades: `StockAdjustment`, `StockAdjustmentDetail`
- ✅ Endpoints REST disponibles
- ✅ Integración con Kardex automática
- ✅ Generación de códigos automática (ADJ-000001, ADJ-000002...)
- ✅ Cálculo automático de diferencias y costos
- ✅ Permisos: `Inventario` → `View`, `Create`

### **Endpoints Disponibles:**
```http
GET    /api/stock-adjustments?warehouseId=1&reason=PHYSICAL_COUNT&fromDate=2026-01-01&toDate=2026-12-31&page=1&pageSize=20
GET    /api/stock-adjustments/{id}
POST   /api/stock-adjustments
GET    /api/stock-adjustments/reasons
GET    /api/product-stock?warehouseId=1&search=producto&page=1&pageSize=10
```

### **Tecnologías Frontend:**
- React 18 + TypeScript
- Material-UI v5
- React Router v6
- axios con Bearer tokens
- react-hook-form + yup para validación

---

## 🎨 DISEÑO DE INTERFAZ

### **Estructura de Navegación:**
```
Inventario (Sidebar)
  └─ Ajustes de Inventario (nueva opción)
      ├─ Lista de Ajustes (/inventory/adjustments)
      └─ Nuevo Ajuste (/inventory/adjustments/new)
      └─ Ver Detalle (/inventory/adjustments/:id)
```

---

## 📝 TAREA 1: CREAR TIPOS Y SERVICIOS

### **1.1. Crear archivo `src/types/stockAdjustment.types.ts`:**

```typescript
export type AdjustmentReason = 
  | 'PHYSICAL_COUNT'
  | 'DAMAGE'
  | 'LOSS'
  | 'EXPIRATION'
  | 'ERROR'
  | 'SAMPLE'
  | 'PRODUCTION_WASTE'
  | 'OTHER';

export const AdjustmentReasonLabels: Record<AdjustmentReason, string> = {
  PHYSICAL_COUNT: 'Inventario Físico',
  DAMAGE: 'Daño / Deterioro',
  LOSS: 'Pérdida / Robo',
  EXPIRATION: 'Producto Vencido',
  ERROR: 'Corrección de Error',
  SAMPLE: 'Muestras / Degustaciones',
  PRODUCTION_WASTE: 'Merma de Producción',
  OTHER: 'Otros'
};

export interface StockAdjustmentDetail {
  id?: number;
  productId: number;
  productCode?: string;
  productName?: string;
  systemQuantity: number;
  physicalQuantity: number;
  adjustmentQuantity?: number;
  unitCost?: number;
  totalCost?: number;
  notes?: string;
}

export interface StockAdjustment {
  id: number;
  code: string;
  warehouseId: number;
  warehouseName: string;
  adjustmentDate: string;
  reason: AdjustmentReason;
  reasonLabel: string;
  notes?: string;
  createdByUserName: string;
  createdAt: string;
  totalProducts: number;
  totalAdjustmentCost: number;
  details: StockAdjustmentDetail[];
}

export interface CreateStockAdjustmentDto {
  warehouseId: number;
  adjustmentDate: string;
  reason: AdjustmentReason;
  notes?: string;
  details: {
    productId: number;
    systemQuantity: number;
    physicalQuantity: number;
    notes?: string;
  }[];
}

export interface StockAdjustmentSummary {
  id: number;
  code: string;
  warehouseName: string;
  adjustmentDate: string;
  reason: AdjustmentReason;
  reasonLabel: string;
  createdByUserName: string;
  totalProducts: number;
  totalAdjustmentCost: number;
  createdAt: string;
}

export interface PagedStockAdjustments {
  items: StockAdjustmentSummary[];
  totalRecords: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ProductStock {
  productId: number;
  productCode: string;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  averageCost?: number;
  minimumStock?: number;
  maximumStock?: number;
}
```

### **1.2. Crear servicio `src/services/stockAdjustmentService.ts`:**

```typescript
import axios from 'axios';
import {
  PagedStockAdjustments,
  StockAdjustment,
  CreateStockAdjustmentDto,
  ProductStock
} from '../types/stockAdjustment.types';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://192.168.0.72:7254';

export const stockAdjustmentService = {
  // Obtener listado paginado con filtros
  async getAll(params: {
    warehouseId?: number;
    reason?: string;
    fromDate?: string;
    toDate?: string;
    page?: number;
    pageSize?: number;
  }): Promise<PagedStockAdjustments> {
    const response = await axios.get(`${API_BASE_URL}/api/stock-adjustments`, {
      params,
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
    return response.data;
  },

  // Obtener detalle de un ajuste
  async getById(id: number): Promise<StockAdjustment> {
    const response = await axios.get(`${API_BASE_URL}/api/stock-adjustments/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
    return response.data;
  },

  // Crear nuevo ajuste
  async create(data: CreateStockAdjustmentDto): Promise<{ message: string; error: number; data: StockAdjustment }> {
    const response = await axios.post(`${API_BASE_URL}/api/stock-adjustments`, data, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json'
      }
    });
    return response.data;
  },

  // Obtener razones disponibles
  async getReasons(): Promise<{ code: string; label: string }[]> {
    const response = await axios.get(`${API_BASE_URL}/api/stock-adjustments/reasons`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
    return response.data.data;
  },

  // Buscar stock de productos
  async searchProductStock(params: {
    warehouseId: number;
    search?: string;
    page?: number;
    pageSize?: number;
  }): Promise<{ items: ProductStock[]; totalRecords: number }> {
    const response = await axios.get(`${API_BASE_URL}/api/product-stock`, {
      params,
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`
      }
    });
    return response.data;
  }
};
```

---

## 📝 TAREA 2: PÁGINA DE LISTADO DE AJUSTES

### **2.1. Crear `src/pages/Inventory/StockAdjustments/StockAdjustmentsList.tsx`:**

```typescript
import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  TextField,
  MenuItem,
  Grid,
  Typography,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  IconButton,
  CircularProgress,
  Alert
} from '@mui/material';
import {
  Add as AddIcon,
  Visibility as ViewIcon,
  FilterList as FilterIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { stockAdjustmentService } from '../../../services/stockAdjustmentService';
import { PagedStockAdjustments, AdjustmentReason, AdjustmentReasonLabels } from '../../../types/stockAdjustment.types';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

export default function StockAdjustmentsList() {
  const navigate = useNavigate();
  const [data, setData] = useState<PagedStockAdjustments | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Filtros
  const [warehouseId, setWarehouseId] = useState<number | ''>('');
  const [reason, setReason] = useState<string>('');
  const [fromDate, setFromDate] = useState<string>('');
  const [toDate, setToDate] = useState<string>('');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);

  // Cargar almacenes (implementar según tu sistema)
  const warehouses = []; // TODO: Cargar desde tu estado global o API

  const loadAdjustments = async () => {
    setLoading(true);
    setError(null);
    try {
      const params: any = {
        page: page + 1,
        pageSize
      };

      if (warehouseId) params.warehouseId = warehouseId;
      if (reason) params.reason = reason;
      if (fromDate) params.fromDate = fromDate;
      if (toDate) params.toDate = toDate;

      const result = await stockAdjustmentService.getAll(params);
      setData(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al cargar ajustes');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAdjustments();
  }, [page, pageSize]);

  const handleFilter = () => {
    setPage(0);
    loadAdjustments();
  };

  const getReasonColor = (reason: AdjustmentReason): "default" | "primary" | "warning" | "error" | "info" | "success" => {
    const colors: Record<AdjustmentReason, any> = {
      PHYSICAL_COUNT: 'primary',
      DAMAGE: 'error',
      LOSS: 'error',
      EXPIRATION: 'warning',
      ERROR: 'info',
      SAMPLE: 'success',
      PRODUCTION_WASTE: 'warning',
      OTHER: 'default'
    };
    return colors[reason] || 'default';
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>
          📦 Ajustes de Inventario
        </Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => navigate('/inventory/adjustments/new')}
        >
          Nuevo Ajuste
        </Button>
      </Box>

      {/* Filtros */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
            <FilterIcon /> Filtros
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <TextField
                fullWidth
                select
                label="Almacén"
                value={warehouseId}
                onChange={(e) => setWarehouseId(e.target.value as number | '')}
                size="small"
              >
                <MenuItem value="">Todos</MenuItem>
                {warehouses.map((w: any) => (
                  <MenuItem key={w.id} value={w.id}>{w.name}</MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6} md={3}>
              <TextField
                fullWidth
                select
                label="Razón del Ajuste"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                size="small"
              >
                <MenuItem value="">Todas</MenuItem>
                {Object.entries(AdjustmentReasonLabels).map(([key, label]) => (
                  <MenuItem key={key} value={key}>{label}</MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6} md={2}>
              <TextField
                fullWidth
                type="date"
                label="Desde"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                size="small"
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            <Grid item xs={12} sm={6} md={2}>
              <TextField
                fullWidth
                type="date"
                label="Hasta"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                size="small"
                InputLabelProps={{ shrink: true }}
              />
            </Grid>

            <Grid item xs={12} sm={12} md={2}>
              <Button
                fullWidth
                variant="contained"
                onClick={handleFilter}
                sx={{ height: '40px' }}
              >
                Filtrar
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Tabla */}
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Card>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Código</strong></TableCell>
                <TableCell><strong>Almacén</strong></TableCell>
                <TableCell><strong>Fecha</strong></TableCell>
                <TableCell><strong>Razón</strong></TableCell>
                <TableCell align="center"><strong>Productos</strong></TableCell>
                <TableCell align="right"><strong>Costo Total</strong></TableCell>
                <TableCell><strong>Usuario</strong></TableCell>
                <TableCell align="center"><strong>Acciones</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                    <CircularProgress />
                  </TableCell>
                </TableRow>
              ) : data && data.items.length > 0 ? (
                data.items.map((adjustment) => (
                  <TableRow key={adjustment.id} hover>
                    <TableCell>{adjustment.code}</TableCell>
                    <TableCell>{adjustment.warehouseName}</TableCell>
                    <TableCell>
                      {format(new Date(adjustment.adjustmentDate), 'dd/MM/yyyy', { locale: es })}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={adjustment.reasonLabel}
                        color={getReasonColor(adjustment.reason)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">{adjustment.totalProducts}</TableCell>
                    <TableCell align="right">
                      {new Intl.NumberFormat('es-MX', {
                        style: 'currency',
                        currency: 'MXN'
                      }).format(adjustment.totalAdjustmentCost)}
                    </TableCell>
                    <TableCell>{adjustment.createdByUserName}</TableCell>
                    <TableCell align="center">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => navigate(`/inventory/adjustments/${adjustment.id}`)}
                        title="Ver detalle"
                      >
                        <ViewIcon />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={8} align="center" sx={{ py: 4 }}>
                    No hay ajustes de inventario
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {data && (
          <TablePagination
            component="div"
            count={data.totalRecords}
            page={page}
            onPageChange={(_, newPage) => setPage(newPage)}
            rowsPerPage={pageSize}
            onRowsPerPageChange={(e) => {
              setPageSize(parseInt(e.target.value, 10));
              setPage(0);
            }}
            labelRowsPerPage="Filas por página:"
            labelDisplayedRows={({ from, to, count }) => `${from}-${to} de ${count}`}
          />
        )}
      </Card>
    </Box>
  );
}
```

---

## 📝 TAREA 3: PÁGINA DE CREACIÓN DE AJUSTE

### **3.1. Crear `src/pages/Inventory/StockAdjustments/CreateStockAdjustment.tsx`:**

```typescript
import React, { useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  TextField,
  MenuItem,
  Grid,
  Typography,
  Alert,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Autocomplete,
  Chip
} from '@mui/material';
import {
  ArrowBack as BackIcon,
  Add as AddIcon,
  Delete as DeleteIcon,
  Save as SaveIcon,
  Search as SearchIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { stockAdjustmentService } from '../../../services/stockAdjustmentService';
import { CreateStockAdjustmentDto, AdjustmentReasonLabels, ProductStock } from '../../../types/stockAdjustment.types';

// Validación
const schema = yup.object().shape({
  warehouseId: yup.number().required('Seleccione un almacén'),
  adjustmentDate: yup.string().required('Ingrese la fecha'),
  reason: yup.string().required('Seleccione una razón'),
  notes: yup.string()
});

interface DetailRow extends ProductStock {
  physicalQuantity: number;
  detailNotes?: string;
}

export default function CreateStockAdjustment() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [details, setDetails] = useState<DetailRow[]>([]);
  
  // Dialog para buscar productos
  const [openDialog, setOpenDialog] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [searchResults, setSearchResults] = useState<ProductStock[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);

  const { control, handleSubmit, watch, formState: { errors } } = useForm({
    resolver: yupResolver(schema),
    defaultValues: {
      warehouseId: '',
      adjustmentDate: new Date().toISOString().split('T')[0],
      reason: '',
      notes: ''
    }
  });

  const selectedWarehouse = watch('warehouseId');

  // Cargar almacenes (implementar según tu sistema)
  const warehouses = []; // TODO: Cargar desde tu estado global o API

  // Buscar productos
  const handleSearchProducts = async () => {
    if (!selectedWarehouse) {
      alert('Primero seleccione un almacén');
      return;
    }

    setSearchLoading(true);
    try {
      const result = await stockAdjustmentService.searchProductStock({
        warehouseId: Number(selectedWarehouse),
        search: searchText,
        page: 1,
        pageSize: 20
      });
      setSearchResults(result.items);
    } catch (err) {
      console.error('Error al buscar productos:', err);
    } finally {
      setSearchLoading(false);
    }
  };

  // Agregar producto a la lista
  const handleAddProduct = (product: ProductStock) => {
    // Verificar si ya está en la lista
    if (details.some(d => d.productId === product.productId)) {
      alert('Este producto ya está en la lista');
      return;
    }

    setDetails([...details, {
      ...product,
      physicalQuantity: product.quantity, // Por defecto, física = sistema
      detailNotes: ''
    }]);

    setOpenDialog(false);
    setSearchText('');
    setSearchResults([]);
  };

  // Eliminar producto
  const handleRemoveProduct = (productId: number) => {
    setDetails(details.filter(d => d.productId !== productId));
  };

  // Actualizar cantidad física
  const handleUpdatePhysicalQuantity = (productId: number, value: number) => {
    setDetails(details.map(d => 
      d.productId === productId ? { ...d, physicalQuantity: value } : d
    ));
  };

  // Actualizar notas del detalle
  const handleUpdateDetailNotes = (productId: number, value: string) => {
    setDetails(details.map(d => 
      d.productId === productId ? { ...d, detailNotes: value } : d
    ));
  };

  // Calcular diferencia
  const getAdjustmentQuantity = (detail: DetailRow) => {
    return detail.physicalQuantity - detail.quantity;
  };

  // Enviar formulario
  const onSubmit = async (formData: any) => {
    if (details.length === 0) {
      setError('Debe agregar al menos un producto');
      return;
    }

    setLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const dto: CreateStockAdjustmentDto = {
        warehouseId: Number(formData.warehouseId),
        adjustmentDate: new Date(formData.adjustmentDate).toISOString(),
        reason: formData.reason,
        notes: formData.notes || undefined,
        details: details.map(d => ({
          productId: d.productId,
          systemQuantity: d.quantity,
          physicalQuantity: d.physicalQuantity,
          notes: d.detailNotes || undefined
        }))
      };

      const response = await stockAdjustmentService.create(dto);
      setSuccess(response.message);

      // Redirigir después de 2 segundos
      setTimeout(() => {
        navigate(`/inventory/adjustments/${response.data.id}`);
      }, 2000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al crear ajuste');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <IconButton onClick={() => navigate('/inventory/adjustments')}>
          <BackIcon />
        </IconButton>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }}>
          📝 Nuevo Ajuste de Inventario
        </Typography>
      </Box>

      <form onSubmit={handleSubmit(onSubmit)}>
        {/* Información General */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>Información General</Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <Controller
                  name="warehouseId"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      select
                      label="Almacén *"
                      error={!!errors.warehouseId}
                      helperText={errors.warehouseId?.message}
                    >
                      {warehouses.map((w: any) => (
                        <MenuItem key={w.id} value={w.id}>{w.name}</MenuItem>
                      ))}
                    </TextField>
                  )}
                />
              </Grid>

              <Grid item xs={12} md={4}>
                <Controller
                  name="adjustmentDate"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      type="date"
                      label="Fecha de Ajuste *"
                      InputLabelProps={{ shrink: true }}
                      error={!!errors.adjustmentDate}
                      helperText={errors.adjustmentDate?.message}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} md={4}>
                <Controller
                  name="reason"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      select
                      label="Razón del Ajuste *"
                      error={!!errors.reason}
                      helperText={errors.reason?.message}
                    >
                      {Object.entries(AdjustmentReasonLabels).map(([key, label]) => (
                        <MenuItem key={key} value={key}>{label}</MenuItem>
                      ))}
                    </TextField>
                  )}
                />
              </Grid>

              <Grid item xs={12}>
                <Controller
                  name="notes"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      multiline
                      rows={2}
                      label="Notas Generales"
                      placeholder="Observaciones del ajuste..."
                    />
                  )}
                />
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Productos */}
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">Productos a Ajustar</Typography>
              <Button
                variant="outlined"
                startIcon={<AddIcon />}
                onClick={() => setOpenDialog(true)}
                disabled={!selectedWarehouse}
              >
                Agregar Producto
              </Button>
            </Box>

            {details.length === 0 ? (
              <Alert severity="info">
                No hay productos agregados. Haga clic en "Agregar Producto" para comenzar.
              </Alert>
            ) : (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell><strong>Código</strong></TableCell>
                      <TableCell><strong>Producto</strong></TableCell>
                      <TableCell align="right"><strong>Stock Sistema</strong></TableCell>
                      <TableCell align="right"><strong>Stock Físico *</strong></TableCell>
                      <TableCell align="right"><strong>Diferencia</strong></TableCell>
                      <TableCell><strong>Notas</strong></TableCell>
                      <TableCell align="center"><strong>Acciones</strong></TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {details.map((detail) => {
                      const diff = getAdjustmentQuantity(detail);
                      return (
                        <TableRow key={detail.productId}>
                          <TableCell>{detail.productCode}</TableCell>
                          <TableCell>{detail.productName}</TableCell>
                          <TableCell align="right">{detail.quantity.toFixed(2)}</TableCell>
                          <TableCell align="right">
                            <TextField
                              type="number"
                              size="small"
                              value={detail.physicalQuantity}
                              onChange={(e) => handleUpdatePhysicalQuantity(detail.productId, Number(e.target.value))}
                              inputProps={{ step: '0.01', min: 0 }}
                              sx={{ width: '100px' }}
                            />
                          </TableCell>
                          <TableCell align="right">
                            <Chip
                              label={diff > 0 ? `+${diff.toFixed(2)}` : diff.toFixed(2)}
                              color={diff > 0 ? 'success' : diff < 0 ? 'error' : 'default'}
                              size="small"
                            />
                          </TableCell>
                          <TableCell>
                            <TextField
                              size="small"
                              placeholder="Notas..."
                              value={detail.detailNotes || ''}
                              onChange={(e) => handleUpdateDetailNotes(detail.productId, e.target.value)}
                              sx={{ width: '200px' }}
                            />
                          </TableCell>
                          <TableCell align="center">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleRemoveProduct(detail.productId)}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </CardContent>
        </Card>

        {/* Alertas */}
        {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
        {success && <Alert severity="success" sx={{ mt: 2 }}>{success}</Alert>}

        {/* Botones de Acción */}
        <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button
            variant="outlined"
            onClick={() => navigate('/inventory/adjustments')}
            disabled={loading}
          >
            Cancelar
          </Button>
          <Button
            variant="contained"
            type="submit"
            startIcon={<SaveIcon />}
            disabled={loading || details.length === 0}
          >
            {loading ? 'Guardando...' : 'Guardar Ajuste'}
          </Button>
        </Box>
      </form>

      {/* Dialog para buscar productos */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>Buscar Producto</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', gap: 1, mb: 2, mt: 1 }}>
            <TextField
              fullWidth
              placeholder="Buscar por código o nombre..."
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSearchProducts()}
            />
            <Button variant="contained" onClick={handleSearchProducts} disabled={searchLoading}>
              <SearchIcon />
            </Button>
          </Box>

          <TableContainer sx={{ maxHeight: 400 }}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Código</TableCell>
                  <TableCell>Producto</TableCell>
                  <TableCell align="right">Stock Actual</TableCell>
                  <TableCell align="center">Acción</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {searchResults.map((product) => (
                  <TableRow key={product.productId} hover>
                    <TableCell>{product.productCode}</TableCell>
                    <TableCell>{product.productName}</TableCell>
                    <TableCell align="right">{product.quantity.toFixed(2)}</TableCell>
                    <TableCell align="center">
                      <Button
                        size="small"
                        variant="outlined"
                        onClick={() => handleAddProduct(product)}
                      >
                        Agregar
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Cerrar</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
```

---

## 📝 TAREA 4: PÁGINA DE DETALLE

### **4.1. Crear `src/pages/Inventory/StockAdjustments/StockAdjustmentDetail.tsx`:**

```typescript
import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  CircularProgress,
  Alert,
  IconButton
} from '@mui/material';
import { ArrowBack as BackIcon, Print as PrintIcon } from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { stockAdjustmentService } from '../../../services/stockAdjustmentService';
import { StockAdjustment, AdjustmentReason } from '../../../types/stockAdjustment.types';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

export default function StockAdjustmentDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [data, setData] = useState<StockAdjustment | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadAdjustment();
  }, [id]);

  const loadAdjustment = async () => {
    try {
      const result = await stockAdjustmentService.getById(Number(id));
      setData(result);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al cargar ajuste');
    } finally {
      setLoading(false);
    }
  };

  const getReasonColor = (reason: AdjustmentReason): "default" | "primary" | "warning" | "error" | "info" | "success" => {
    const colors: Record<AdjustmentReason, any> = {
      PHYSICAL_COUNT: 'primary',
      DAMAGE: 'error',
      LOSS: 'error',
      EXPIRATION: 'warning',
      ERROR: 'info',
      SAMPLE: 'success',
      PRODUCTION_WASTE: 'warning',
      OTHER: 'default'
    };
    return colors[reason] || 'default';
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !data) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error">{error || 'Ajuste no encontrado'}</Alert>
        <Button onClick={() => navigate('/inventory/adjustments')} sx={{ mt: 2 }}>
          Volver al listado
        </Button>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton onClick={() => navigate('/inventory/adjustments')}>
            <BackIcon />
          </IconButton>
          <Typography variant="h4" sx={{ fontWeight: 'bold' }}>
            Ajuste {data.code}
          </Typography>
        </Box>
        <Button variant="outlined" startIcon={<PrintIcon />}>
          Imprimir
        </Button>
      </Box>

      {/* Información General */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 2 }}>Información General</Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Código</Typography>
              <Typography variant="body1" sx={{ fontWeight: 'bold' }}>{data.code}</Typography>
            </Grid>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Almacén</Typography>
              <Typography variant="body1">{data.warehouseName}</Typography>
            </Grid>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Fecha</Typography>
              <Typography variant="body1">
                {format(new Date(data.adjustmentDate), 'dd/MM/yyyy', { locale: es })}
              </Typography>
            </Grid>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Razón</Typography>
              <Box sx={{ mt: 0.5 }}>
                <Chip label={data.reasonLabel} color={getReasonColor(data.reason)} size="small" />
              </Box>
            </Grid>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Creado por</Typography>
              <Typography variant="body1">{data.createdByUserName}</Typography>
            </Grid>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Fecha de Creación</Typography>
              <Typography variant="body1">
                {format(new Date(data.createdAt), 'dd/MM/yyyy HH:mm', { locale: es })}
              </Typography>
            </Grid>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Total Productos</Typography>
              <Typography variant="body1" sx={{ fontWeight: 'bold' }}>{data.totalProducts}</Typography>
            </Grid>
            <Grid item xs={12} md={3}>
              <Typography variant="caption" color="text.secondary">Costo Total del Ajuste</Typography>
              <Typography variant="body1" sx={{ fontWeight: 'bold', color: data.totalAdjustmentCost >= 0 ? 'success.main' : 'error.main' }}>
                {new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(data.totalAdjustmentCost)}
              </Typography>
            </Grid>
            {data.notes && (
              <Grid item xs={12}>
                <Typography variant="caption" color="text.secondary">Notas</Typography>
                <Typography variant="body1">{data.notes}</Typography>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      {/* Detalle de Productos */}
      <Card>
        <CardContent>
          <Typography variant="h6" sx={{ mb: 2 }}>Productos Ajustados</Typography>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell><strong>Código</strong></TableCell>
                  <TableCell><strong>Producto</strong></TableCell>
                  <TableCell align="right"><strong>Stock Sistema</strong></TableCell>
                  <TableCell align="right"><strong>Stock Físico</strong></TableCell>
                  <TableCell align="right"><strong>Diferencia</strong></TableCell>
                  <TableCell align="right"><strong>Costo Unit.</strong></TableCell>
                  <TableCell align="right"><strong>Costo Total</strong></TableCell>
                  <TableCell><strong>Notas</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.details.map((detail) => (
                  <TableRow key={detail.id}>
                    <TableCell>{detail.productCode}</TableCell>
                    <TableCell>{detail.productName}</TableCell>
                    <TableCell align="right">{detail.systemQuantity.toFixed(2)}</TableCell>
                    <TableCell align="right">{detail.physicalQuantity.toFixed(2)}</TableCell>
                    <TableCell align="right">
                      <Chip
                        label={detail.adjustmentQuantity! > 0 ? `+${detail.adjustmentQuantity!.toFixed(2)}` : detail.adjustmentQuantity!.toFixed(2)}
                        color={detail.adjustmentQuantity! > 0 ? 'success' : detail.adjustmentQuantity! < 0 ? 'error' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      {detail.unitCost ? `$${detail.unitCost.toFixed(2)}` : 'N/A'}
                    </TableCell>
                    <TableCell align="right" sx={{ fontWeight: 'bold', color: detail.totalCost && detail.totalCost >= 0 ? 'success.main' : 'error.main' }}>
                      {detail.totalCost ? new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(detail.totalCost) : 'N/A'}
                    </TableCell>
                    <TableCell>{detail.notes || '-'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Box>
  );
}
```

---

## 📝 TAREA 5: AGREGAR RUTAS

### **5.1. Actualizar `src/App.tsx` o archivo de rutas:**

```typescript
import StockAdjustmentsList from './pages/Inventory/StockAdjustments/StockAdjustmentsList';
import CreateStockAdjustment from './pages/Inventory/StockAdjustments/CreateStockAdjustment';
import StockAdjustmentDetail from './pages/Inventory/StockAdjustments/StockAdjustmentDetail';

// Agregar rutas en tu configuración de React Router:
<Route path="/inventory/adjustments" element={<StockAdjustmentsList />} />
<Route path="/inventory/adjustments/new" element={<CreateStockAdjustment />} />
<Route path="/inventory/adjustments/:id" element={<StockAdjustmentDetail />} />
```

---

## 📝 TAREA 6: AGREGAR OPCIÓN EN SIDEBAR

### **6.1. Actualizar menú de navegación:**

Agregar en la sección de **Inventario** del sidebar:

```typescript
{
  title: 'Ajustes de Inventario',
  path: '/inventory/adjustments',
  icon: <AdjustIcon />, // O el icono que prefieras
  permission: 'Inventario.View'
}
```

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

- [ ] Tipos y servicios creados
- [ ] Página de listado funcional
- [ ] Página de creación funcional
- [ ] Página de detalle funcional
- [ ] Rutas configuradas
- [ ] Opción en sidebar agregada
- [ ] Permisos implementados
- [ ] Búsqueda de productos funcional
- [ ] Validaciones de formulario
- [ ] Manejo de errores

---

## 🎨 MEJORAS OPCIONALES

1. **Exportar a Excel**: Agregar botón para exportar listado
2. **Filtros Avanzados**: Agregar filtros por usuario, rango de costos
3. **Dashboard**: Vista de estadísticas de ajustes
4. **Historial**: Ver historial de ajustes por producto
5. **Impresión**: Generar reporte PDF del ajuste

---

## 📚 NOTAS IMPORTANTES

- ✅ Backend ya está completo y operativo
- ✅ Todos los cálculos se hacen automáticamente en el backend
- ✅ El Kardex se actualiza automáticamente
- ✅ Los códigos se generan automáticamente (ADJ-XXXXXX)
- ✅ El stock se actualiza en tiempo real
- ⚠️ Implementar manejo de permisos según tu sistema
- ⚠️ Cargar lista de almacenes desde tu estado global o API
- ⚠️ Adaptar estilos a tu tema existente

---

**FIN DEL PROMPT - ELECTRON FRONTEND**
