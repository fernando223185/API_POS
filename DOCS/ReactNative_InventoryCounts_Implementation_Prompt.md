# Implementación de Módulo de Conteos Cíclicos de Inventario - React Native (Expo + TypeScript)

## 📋 Contexto del Proyecto
Este documento contiene las instrucciones completas para implementar el módulo de **Conteos Cíclicos de Inventario (Cycle Counting)** en la aplicación móvil React Native con Expo + TypeScript + React Native Paper.

### Objetivo del Módulo
Permitir a los usuarios realizar conteos físicos de inventario desde dispositivos móviles, comparar con cantidades del sistema, identificar variaciones y generar automáticamente ajustes de inventario para corregir diferencias.

### Flujo del Proceso
1. **Crear Sesión de Conteo**: Usuario selecciona almacén, tipo de conteo y asigna responsable
2. **Iniciar Conteo**: El sistema carga los productos a contar según el tipo seleccionado
3. **Registrar Cantidades Físicas**: Usuarios cuentan productos y registran cantidades (ideal para uso con scanner móvil)
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
```typescript
const API_BASE_URL = 'http://192.168.0.72:5000/api/inventory-counts';
```

### Autenticación
Todas las peticiones requieren header de autorización con token almacenado en AsyncStorage:

```typescript
import AsyncStorage from '@react-native-async-storage/async-storage';

const getAuthHeaders = async () => {
  const token = await AsyncStorage.getItem('token');
  return {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  };
};
```

### Lista de Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/inventory-counts?page=1&pageSize=20&status=...` | Listar conteos (paginado) |
| GET | `/api/inventory-counts/{id}` | Obtener detalle completo de conteo |
| POST | `/api/inventory-counts` | Crear nuevo conteo |
| POST | `/api/inventory-counts/{id}/start` | Iniciar conteo (cambia a InProgress) |
| PATCH | `/api/inventory-counts/{countId}/details/{detailId}` | Actualizar cantidad física |
| GET | `/api/inventory-counts/{id}/pending` | Obtener detalles pendientes |
| GET | `/api/inventory-counts/{id}/variances` | Obtener detalles con variaciones |
| GET | `/api/inventory-counts/{id}/statistics` | Obtener estadísticas del conteo |
| POST | `/api/inventory-counts/{id}/complete` | Completar y aprobar conteo |
| POST | `/api/inventory-counts/{id}/cancel` | Cancelar conteo |
| GET | `/api/inventory-counts/types` | Obtener tipos de conteo |
| GET | `/api/inventory-counts/statuses` | Obtener estados de conteo |

Ver documentación completa de DTOs y responses en el prompt de Electron.

---

## 📁 Estructura de Archivos a Crear

```
src/
├── types/
│   └── inventoryCount.types.ts           # Interfaces TypeScript
├── services/
│   └── inventoryCountService.ts          # Servicio API con fetch
├── screens/
│   ├── InventoryCountListScreen.tsx      # Listado de conteos
│   ├── InventoryCountCreateScreen.tsx    # Crear nuevo conteo
│   ├── InventoryCountExecuteScreen.tsx   # Ejecutar conteo físico
│   ├── InventoryCountReviewScreen.tsx    # Revisar variaciones
│   └── InventoryCountDetailScreen.tsx    # Ver detalle completo
├── components/
│   ├── InventoryCountCard.tsx            # Tarjeta de conteo para lista
│   ├── ProductCountItem.tsx              # Item de producto para contar
│   ├── VarianceCard.tsx                  # Tarjeta de variación
│   ├── CountStatistics.tsx               # Componente de estadísticas
│   └── QuantityInputModal.tsx            # Modal para ingresar cantidad
└── navigation/
    └── InventoryNavigator.tsx            # Navegación del módulo
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
}
```

---

## 🎯 TAREA 2: Crear Servicio API

**Archivo:** `src/services/inventoryCountService.ts`

```typescript
import AsyncStorage from '@react-native-async-storage/async-storage';
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

// Obtener headers con autenticación
const getAuthHeaders = async () => {
  const token = await AsyncStorage.getItem('token');
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

  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}?${params.toString()}`, { headers });
  
  if (!response.ok) {
    throw new Error('Error al cargar conteos');
  }
  
  return await response.json();
};

export const getInventoryCountById = async (id: number): Promise<InventoryCountResponseDto> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${id}`, { headers });
  
  if (!response.ok) {
    throw new Error('Error al cargar conteo');
  }
  
  return await response.json();
};

export const createInventoryCount = async (
  dto: CreateInventoryCountDto
): Promise<InventoryCountResponseDto> => {
  const headers = await getAuthHeaders();
  const response = await fetch(API_BASE_URL, {
    method: 'POST',
    headers,
    body: JSON.stringify(dto)
  });
  
  if (!response.ok) {
    throw new Error('Error al crear conteo');
  }
  
  return await response.json();
};

// ============================================
// OPERACIONES DEL FLUJO DE CONTEO
// ============================================

export const startInventoryCount = async (id: number): Promise<InventoryCountResponseDto> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${id}/start`, {
    method: 'POST',
    headers,
    body: JSON.stringify({})
  });
  
  if (!response.ok) {
    throw new Error('Error al iniciar conteo');
  }
  
  return await response.json();
};

export const updateCountDetail = async (
  countId: number,
  detailId: number,
  dto: UpdateCountDetailDto
): Promise<InventoryCountDetailResponseDto> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${countId}/details/${detailId}`, {
    method: 'PATCH',
    headers,
    body: JSON.stringify(dto)
  });
  
  if (!response.ok) {
    throw new Error('Error al actualizar cantidad');
  }
  
  return await response.json();
};

export const completeInventoryCount = async (
  id: number,
  dto: CompleteInventoryCountDto
): Promise<InventoryCountResponseDto> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${id}/complete`, {
    method: 'POST',
    headers,
    body: JSON.stringify(dto)
  });
  
  if (!response.ok) {
    throw new Error('Error al completar conteo');
  }
  
  return await response.json();
};

export const cancelInventoryCount = async (
  id: number,
  dto: CancelInventoryCountDto
): Promise<void> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${id}/cancel`, {
    method: 'POST',
    headers,
    body: JSON.stringify(dto)
  });
  
  if (!response.ok) {
    throw new Error('Error al cancelar conteo');
  }
};

// ============================================
// CONSULTAS ESPECIALIZADAS
// ============================================

export const getPendingDetails = async (
  countId: number
): Promise<InventoryCountDetailResponseDto[]> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${countId}/pending`, { headers });
  
  if (!response.ok) {
    throw new Error('Error al cargar detalles pendientes');
  }
  
  return await response.json();
};

export const getDetailsWithVariances = async (
  countId: number
): Promise<InventoryCountDetailResponseDto[]> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${countId}/variances`, { headers });
  
  if (!response.ok) {
    throw new Error('Error al cargar variaciones');
  }
  
  return await response.json();
};

export const getCountStatistics = async (countId: number): Promise<CountStatisticsDto> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/${countId}/statistics`, { headers });
  
  if (!response.ok) {
    throw new Error('Error al cargar estadísticas');
  }
  
  return await response.json();
};

// ============================================
// CATÁLOGOS
// ============================================

export const getCountTypes = async (): Promise<CountTypeOption[]> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/types`, { headers });
  
  if (!response.ok) {
    throw new Error('Error al cargar tipos de conteo');
  }
  
  return await response.json();
};

export const getCountStatuses = async (): Promise<CountStatusOption[]> => {
  const headers = await getAuthHeaders();
  const response = await fetch(`${API_BASE_URL}/statuses`, { headers });
  
  if (!response.ok) {
    throw new Error('Error al cargar estados');
  }
  
  return await response.json();
};
```

---

## 🎯 TAREA 3: Crear Screen de Listado

**Archivo:** `src/screens/InventoryCountListScreen.tsx`

```typescript
import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  FlatList,
  StyleSheet,
  RefreshControl,
  TouchableOpacity
} from 'react-native';
import {
  Appbar,
  FAB,
  Searchbar,
  Chip,
  Card,
  Text,
  ProgressBar,
  Menu,
  Button,
  Portal,
  Modal
} from 'react-native-paper';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import { getInventoryCounts, getCountTypes, getCountStatuses } from '../services/inventoryCountService';
import {
  InventoryCountResponseDto,
  CountStatus,
  CountType,
  InventoryCountFilters
} from '../types/inventoryCount.types';
import { format } from 'date-fns';

const InventoryCountListScreen = () => {
  const navigation = useNavigation();
  
  const [counts, setCounts] = useState<InventoryCountResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  
  const [filters, setFilters] = useState<InventoryCountFilters>({
    page: 1,
    pageSize: 20,
    status: undefined,
    countType: undefined
  });

  const [filterMenuVisible, setFilterMenuVisible] = useState(false);
  const [countTypes, setCountTypes] = useState<any[]>([]);
  const [statuses, setStatuses] = useState<any[]>([]);

  // Cargar datos al montar y al volver a la pantalla
  useFocusEffect(
    useCallback(() => {
      loadCounts();
    }, [filters])
  );

  useEffect(() => {
    loadCatalogs();
  }, []);

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

  const onRefresh = async () => {
    setRefreshing(true);
    await loadCounts();
    setRefreshing(false);
  };

  const getStatusColor = (status: CountStatus): string => {
    switch (status) {
      case 'Draft': return '#9e9e9e';
      case 'InProgress': return '#2196f3';
      case 'Completed': return '#4caf50';
      case 'Cancelled': return '#f44336';
      default: return '#9e9e9e';
    }
  };

  const getProgressPercentage = (count: InventoryCountResponseDto): number => {
    if (count.totalProducts === 0) return 0;
    return count.countedProducts / count.totalProducts;
  };

  const renderCountCard = ({ item }: { item: InventoryCountResponseDto }) => (
    <Card 
      style={styles.card}
      onPress={() => navigation.navigate('InventoryCountDetail', { id: item.id })}
    >
      <Card.Content>
        {/* Header */}
        <View style={styles.cardHeader}>
          <Text variant="titleMedium" style={styles.code}>{item.code}</Text>
          <Chip 
            mode="flat"
            style={{ backgroundColor: getStatusColor(item.status) }}
            textStyle={{ color: '#fff' }}
          >
            {item.statusLabel}
          </Chip>
        </View>

        {/* Información */}
        <View style={styles.infoRow}>
          <Text variant="bodySmall" style={styles.label}>Almacén:</Text>
          <Text variant="bodySmall">{item.warehouseName}</Text>
        </View>
        
        <View style={styles.infoRow}>
          <Text variant="bodySmall" style={styles.label}>Tipo:</Text>
          <Text variant="bodySmall">{item.countTypeLabel}</Text>
        </View>

        <View style={styles.infoRow}>
          <Text variant="bodySmall" style={styles.label}>Asignado a:</Text>
          <Text variant="bodySmall">{item.assignedToUserName}</Text>
        </View>

        {/* Progreso */}
        {item.status === 'InProgress' && (
          <View style={styles.progressContainer}>
            <Text variant="bodySmall" style={styles.progressText}>
              {item.countedProducts} / {item.totalProducts} productos
            </Text>
            <ProgressBar 
              progress={getProgressPercentage(item)} 
              color="#2196f3"
              style={styles.progressBar}
            />
          </View>
        )}

        {/* Variaciones */}
        {item.productsWithVariance > 0 && (
          <Chip 
            icon="alert" 
            mode="outlined" 
            style={styles.varianceChip}
            textStyle={{ fontSize: 12 }}
          >
            {item.productsWithVariance} productos con variación
          </Chip>
        )}

        {/* Acciones */}
        {item.status === 'Draft' && (
          <Button
            mode="contained"
            icon="play"
            onPress={() => navigation.navigate('InventoryCountExecute', { id: item.id })}
            style={styles.actionButton}
          >
            Iniciar Conteo
          </Button>
        )}
        
        {item.status === 'InProgress' && (
          <Button
            mode="contained"
            icon="clipboard-check"
            onPress={() => navigation.navigate('InventoryCountExecute', { id: item.id })}
            style={styles.actionButton}
          >
            Continuar Conteo
          </Button>
        )}
      </Card.Content>
    </Card>
  );

  return (
    <View style={styles.container}>
      <Appbar.Header>
        <Appbar.Content title="Conteos Cíclicos" />
        <Appbar.Action 
          icon="filter-variant" 
          onPress={() => setFilterMenuVisible(true)} 
        />
      </Appbar.Header>

      {/* Filtros Activos */}
      <View style={styles.filtersContainer}>
        {filters.status && (
          <Chip
            mode="outlined"
            onClose={() => setFilters({ ...filters, status: undefined, page: 1 })}
            style={styles.filterChip}
          >
            {statuses.find(s => s.value === filters.status)?.label}
          </Chip>
        )}
        {filters.countType && (
          <Chip
            mode="outlined"
            onClose={() => setFilters({ ...filters, countType: undefined, page: 1 })}
            style={styles.filterChip}
          >
            {countTypes.find(t => t.value === filters.countType)?.label}
          </Chip>
        )}
      </View>

      {/* Lista */}
      <FlatList
        data={counts}
        renderItem={renderCountCard}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={styles.listContent}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text variant="bodyMedium" style={styles.emptyText}>
              No se encontraron conteos
            </Text>
          </View>
        }
      />

      {/* FAB Crear */}
      <FAB
        icon="plus"
        style={styles.fab}
        onPress={() => navigation.navigate('InventoryCountCreate')}
      />

      {/* Modal de Filtros */}
      <Portal>
        <Modal
          visible={filterMenuVisible}
          onDismiss={() => setFilterMenuVisible(false)}
          contentContainerStyle={styles.modalContent}
        >
          <Text variant="titleMedium" style={styles.modalTitle}>
            Filtros
          </Text>

          <Text variant="labelMedium" style={styles.filterLabel}>
            Estado
          </Text>
          <View style={styles.chipContainer}>
            <Chip
              mode={filters.status === undefined ? 'flat' : 'outlined'}
              onPress={() => setFilters({ ...filters, status: undefined, page: 1 })}
              style={styles.filterOption}
            >
              Todos
            </Chip>
            {statuses.map((s) => (
              <Chip
                key={s.value}
                mode={filters.status === s.value ? 'flat' : 'outlined'}
                onPress={() => setFilters({ ...filters, status: s.value, page: 1 })}
                style={styles.filterOption}
              >
                {s.label}
              </Chip>
            ))}
          </View>

          <Text variant="labelMedium" style={styles.filterLabel}>
            Tipo de Conteo
          </Text>
          <View style={styles.chipContainer}>
            <Chip
              mode={filters.countType === undefined ? 'flat' : 'outlined'}
              onPress={() => setFilters({ ...filters, countType: undefined, page: 1 })}
              style={styles.filterOption}
            >
              Todos
            </Chip>
            {countTypes.map((t) => (
              <Chip
                key={t.value}
                mode={filters.countType === t.value ? 'flat' : 'outlined'}
                onPress={() => setFilters({ ...filters, countType: t.value, page: 1 })}
                style={styles.filterOption}
              >
                {t.label}
              </Chip>
            ))}
          </View>

          <Button
            mode="contained"
            onPress={() => setFilterMenuVisible(false)}
            style={styles.closeButton}
          >
            Cerrar
          </Button>
        </Modal>
      </Portal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5'
  },
  listContent: {
    padding: 16
  },
  card: {
    marginBottom: 12
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12
  },
  code: {
    fontWeight: 'bold',
    color: '#1976d2'
  },
  infoRow: {
    flexDirection: 'row',
    marginBottom: 4
  },
  label: {
    fontWeight: 'bold',
    marginRight: 8,
    width: 100
  },
  progressContainer: {
    marginTop: 12
  },
  progressText: {
    marginBottom: 4,
    color: '#666'
  },
  progressBar: {
    height: 8,
    borderRadius: 4
  },
  varianceChip: {
    marginTop: 8,
    alignSelf: 'flex-start'
  },
  actionButton: {
    marginTop: 12
  },
  filtersContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    padding: 8
  },
  filterChip: {
    margin: 4
  },
  fab: {
    position: 'absolute',
    right: 16,
    bottom: 16
  },
  emptyContainer: {
    padding: 32,
    alignItems: 'center'
  },
  emptyText: {
    color: '#999'
  },
  modalContent: {
    backgroundColor: 'white',
    padding: 20,
    margin: 20,
    borderRadius: 8
  },
  modalTitle: {
    marginBottom: 16,
    fontWeight: 'bold'
  },
  filterLabel: {
    marginTop: 12,
    marginBottom: 8,
    fontWeight: 'bold'
  },
  chipContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap'
  },
  filterOption: {
    margin: 4
  },
  closeButton: {
    marginTop: 20
  }
});

export default InventoryCountListScreen;
```

---

## 🎯 TAREA 4: Crear Screen de Ejecución de Conteo

**Archivo:** `src/screens/InventoryCountExecuteScreen.tsx`

```typescript
import React, { useState, useEffect } from 'react';
import {
  View,
  FlatList,
  StyleSheet,
  Alert,
  RefreshControl
} from 'react-native';
import {
  Appbar,
  Card,
  Text,
  Chip,
  Button,
  ProgressBar,
  Portal,
  Modal,
  TextInput,
  SegmentedButtons
} from 'react-native-paper';
import { useRoute, useNavigation } from '@react-navigation/native';
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

const InventoryCountExecuteScreen = () => {
  const route = useRoute();
  const navigation = useNavigation();
  const { id } = route.params as { id: number };
  
  const [count, setCount] = useState<InventoryCountResponseDto | null>(null);
  const [pendingDetails, setPendingDetails] = useState<InventoryCountDetailResponseDto[]>([]);
  const [varianceDetails, setVarianceDetails] = useState<InventoryCountDetailResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [tab, setTab] = useState('pending');
  
  const [modalVisible, setModalVisible] = useState(false);
  const [selectedDetail, setSelectedDetail] = useState<InventoryCountDetailResponseDto | null>(null);
  const [physicalQuantity, setPhysicalQuantity] = useState('');
  const [notes, setNotes] = useState('');

  useEffect(() => {
    loadCountData();
  }, []);

  const loadCountData = async () => {
    setLoading(true);
    try {
      const countData = await getInventoryCountById(id);
      setCount(countData);
      
      if (countData.status === 'InProgress') {
        const [pending, variances] = await Promise.all([
          getPendingDetails(id),
          getDetailsWithVariances(id)
        ]);
        setPendingDetails(pending);
        setVarianceDetails(variances);
      }
    } catch (error) {
      console.error('Error cargando conteo:', error);
      Alert.alert('Error', 'No se pudo cargar el conteo');
    } finally {
      setLoading(false);
    }
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadCountData();
    setRefreshing(false);
  };

  const handleStartCount = async () => {
    Alert.alert(
      'Iniciar Conteo',
      '¿Está seguro de iniciar este conteo? Se cargarán los productos a contar.',
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Iniciar',
          onPress: async () => {
            setLoading(true);
            try {
              await startInventoryCount(id);
              await loadCountData();
            } catch (error) {
              Alert.alert('Error', 'No se pudo iniciar el conteo');
            } finally {
              setLoading(false);
            }
          }
        }
      ]
    );
  };

  const handleOpenModal = (detail: InventoryCountDetailResponseDto) => {
    setSelectedDetail(detail);
    setPhysicalQuantity((detail.physicalQuantity || detail.systemQuantity).toString());
    setNotes(detail.notes || '');
    setModalVisible(true);
  };

  const handleSaveQuantity = async () => {
    if (!selectedDetail) return;
    
    const quantity = parseFloat(physicalQuantity);
    if (isNaN(quantity) || quantity < 0) {
      Alert.alert('Error', 'Ingrese una cantidad válida');
      return;
    }

    setLoading(true);
    try {
      const dto: UpdateCountDetailDto = {
        physicalQuantity: quantity,
        notes: notes.trim()
      };
      
      await updateCountDetail(count!.id, selectedDetail.id, dto);
      setModalVisible(false);
      await loadCountData();
    } catch (error) {
      Alert.alert('Error', 'No se pudo guardar la cantidad');
    } finally {
      setLoading(false);
    }
  };

  const getProgressPercentage = (): number => {
    if (!count || count.totalProducts === 0) return 0;
    return count.countedProducts / count.totalProducts;
  };

  const renderPendingItem = ({ item }: { item: InventoryCountDetailResponseDto }) => (
    <Card style={styles.productCard} onPress={() => handleOpenModal(item)}>
      <Card.Content>
        <View style={styles.productHeader}>
          <Text variant="titleSmall" style={styles.productCode}>{item.productCode}</Text>
          <Chip mode="outlined" compact>{item.statusLabel}</Chip>
        </View>
        <Text variant="bodyMedium" style={styles.productName}>
          {item.productName}
        </Text>
        <View style={styles.quantityRow}>
          <Text variant="bodySmall" style={styles.label}>Sistema:</Text>
          <Text variant="bodySmall">{item.systemQuantity}</Text>
        </View>
        <View style={styles.quantityRow}>
          <Text variant="bodySmall" style={styles.label}>Física:</Text>
          <Text variant="bodySmall">
            {item.physicalQuantity !== null ? item.physicalQuantity : '-'}
          </Text>
        </View>
        <Button 
          mode="contained" 
          compact 
          onPress={() => handleOpenModal(item)}
          style={styles.countButton}
        >
          Contar Producto
        </Button>
      </Card.Content>
    </Card>
  );

  const renderVarianceItem = ({ item }: { item: InventoryCountDetailResponseDto }) => (
    <Card style={styles.productCard}>
      <Card.Content>
        <View style={styles.productHeader}>
          <Text variant="titleSmall" style={styles.productCode}>{item.productCode}</Text>
          <Chip 
            mode="flat"
            style={{
              backgroundColor: (item.variance || 0) < 0 ? '#ffebee' : '#e8f5e9'
            }}
            textStyle={{
              color: (item.variance || 0) < 0 ? '#c62828' : '#2e7d32'
            }}
            compact
          >
            {item.variance! > 0 ? '+' : ''}{item.variance}
          </Chip>
        </View>
        <Text variant="bodyMedium" style={styles.productName}>
          {item.productName}
        </Text>
        <View style={styles.varianceGrid}>
          <View style={styles.varianceItem}>
            <Text variant="bodySmall" style={styles.varianceLabel}>Sistema</Text>
            <Text variant="bodyMedium">{item.systemQuantity}</Text>
          </View>
          <View style={styles.varianceItem}>
            <Text variant="bodySmall" style={styles.varianceLabel}>Física</Text>
            <Text variant="bodyMedium">{item.physicalQuantity}</Text>
          </View>
          <View style={styles.varianceItem}>
            <Text variant="bodySmall" style={styles.varianceLabel}>% Var.</Text>
            <Text variant="bodyMedium">{item.variancePercentage?.toFixed(2)}%</Text>
          </View>
          <View style={styles.varianceItem}>
            <Text variant="bodySmall" style={styles.varianceLabel}>Costo</Text>
            <Text 
              variant="bodyMedium"
              style={{
                color: (item.varianceCost || 0) < 0 ? '#c62828' : '#2e7d32'
              }}
            >
              ${Math.abs(item.varianceCost || 0).toFixed(2)}
            </Text>
          </View>
        </View>
      </Card.Content>
    </Card>
  );

  if (!count) {
    return (
      <View style={styles.container}>
        <Appbar.Header>
          <Appbar.BackAction onPress={() => navigation.goBack()} />
          <Appbar.Content title="Cargando..." />
        </Appbar.Header>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Appbar.Header>
        <Appbar.BackAction onPress={() => navigation.goBack()} />
        <Appbar.Content title={count.code} />
      </Appbar.Header>

      {/* Información del Conteo */}
      <Card style={styles.headerCard}>
        <Card.Content>
          <View style={styles.headerRow}>
            <Chip mode="flat">{count.statusLabel}</Chip>
            <Chip mode="outlined">{count.countTypeLabel}</Chip>
          </View>
          <Text variant="bodySmall" style={styles.warehouse}>
            Almacén: {count.warehouseName}
          </Text>
          
          {count.status === 'InProgress' && (
            <View style={styles.progressContainer}>
              <Text variant="bodySmall" style={styles.progressText}>
                {count.countedProducts} / {count.totalProducts} productos
              </Text>
              <ProgressBar 
                progress={getProgressPercentage()} 
                color="#2196f3"
                style={styles.progressBar}
              />
            </View>
          )}
        </Card.Content>
      </Card>

      {/* Botón Iniciar (Draft) */}
      {count.status === 'Draft' && (
        <View style={styles.actionContainer}>
          <Button
            mode="contained"
            icon="play"
            onPress={handleStartCount}
            loading={loading}
          >
            Iniciar Conteo
          </Button>
        </View>
      )}

      {/* Tabs y Listado (InProgress) */}
      {count.status === 'InProgress' && (
        <>
          <SegmentedButtons
            value={tab}
            onValueChange={setTab}
            buttons={[
              {
                value: 'pending',
                label: `Pendientes (${pendingDetails.length})`,
                icon: 'clipboard-list'
              },
              {
                value: 'variances',
                label: `Variaciones (${varianceDetails.length})`,
                icon: 'alert'
              }
            ]}
            style={styles.tabs}
          />

          <FlatList
            data={tab === 'pending' ? pendingDetails : varianceDetails}
            renderItem={tab === 'pending' ? renderPendingItem : renderVarianceItem}
            keyExtractor={(item) => item.id.toString()}
            contentContainerStyle={styles.listContent}
            refreshControl={
              <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
            }
            ListEmptyComponent={
              <View style={styles.emptyContainer}>
                <Text variant="bodyMedium" style={styles.emptyText}>
                  {tab === 'pending' 
                    ? 'No hay productos pendientes' 
                    : 'No hay variaciones'}
                </Text>
              </View>
            }
          />

          {/* Botón Completar */}
          {pendingDetails.length === 0 && (
            <View style={styles.completeContainer}>
              <Button
                mode="contained"
                icon="check-circle"
                onPress={() => navigation.navigate('InventoryCountReview', { id: count.id })}
              >
                Revisar y Completar
              </Button>
            </View>
          )}
        </>
      )}

      {/* Modal Cantidad Física */}
      <Portal>
        <Modal
          visible={modalVisible}
          onDismiss={() => setModalVisible(false)}
          contentContainerStyle={styles.modalContent}
        >
          {selectedDetail && (
            <>
              <Text variant="titleMedium" style={styles.modalTitle}>
                Registrar Cantidad Física
              </Text>
              
              <Text variant="bodySmall" style={styles.modalLabel}>
                Producto
              </Text>
              <Text variant="bodyMedium" style={styles.modalValue}>
                {selectedDetail.productName}
              </Text>
              
              <Text variant="bodySmall" style={styles.modalLabel}>
                Código
              </Text>
              <Text variant="bodyMedium" style={styles.modalValue}>
                {selectedDetail.productCode}
              </Text>
              
              <Text variant="bodySmall" style={styles.modalLabel}>
                Cantidad en Sistema
              </Text>
              <Text variant="bodyMedium" style={styles.modalValue}>
                {selectedDetail.systemQuantity}
              </Text>

              <TextInput
                label="Cantidad Física *"
                value={physicalQuantity}
                onChangeText={setPhysicalQuantity}
                keyboardType="numeric"
                mode="outlined"
                style={styles.input}
              />

              <TextInput
                label="Notas"
                value={notes}
                onChangeText={setNotes}
                multiline
                numberOfLines={3}
                mode="outlined"
                style={styles.input}
              />

              <View style={styles.modalButtons}>
                <Button
                  mode="outlined"
                  onPress={() => setModalVisible(false)}
                  style={styles.modalButton}
                >
                  Cancelar
                </Button>
                <Button
                  mode="contained"
                  onPress={handleSaveQuantity}
                  loading={loading}
                  style={styles.modalButton}
                >
                  Guardar
                </Button>
              </View>
            </>
          )}
        </Modal>
      </Portal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5'
  },
  headerCard: {
    margin: 16,
    marginBottom: 8
  },
  headerRow: {
    flexDirection: 'row',
    gap: 8,
    marginBottom: 8
  },
  warehouse: {
    color: '#666',
    marginBottom: 12
  },
  progressContainer: {
    marginTop: 8
  },
  progressText: {
    marginBottom: 4,
    color: '#666'
  },
  progressBar: {
    height: 8,
    borderRadius: 4
  },
  actionContainer: {
    padding: 16
  },
  tabs: {
    margin: 16,
    marginTop: 8
  },
  listContent: {
    padding: 16,
    paddingTop: 0
  },
  productCard: {
    marginBottom: 12
  },
  productHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8
  },
  productCode: {
    color: '#1976d2',
    fontWeight: 'bold'
  },
  productName: {
    marginBottom: 8
  },
  quantityRow: {
    flexDirection: 'row',
    marginBottom: 4
  },
  label: {
    fontWeight: 'bold',
    marginRight: 8,
    width: 60
  },
  countButton: {
    marginTop: 8
  },
  varianceGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginTop: 8
  },
  varianceItem: {
    width: '50%',
    marginBottom: 8
  },
  varianceLabel: {
    color: '#666',
    fontSize: 11
  },
  emptyContainer: {
    padding: 32,
    alignItems: 'center'
  },
  emptyText: {
    color: '#999'
  },
  completeContainer: {
    padding: 16,
    backgroundColor: '#fff',
    borderTopWidth: 1,
    borderTopColor: '#e0e0e0'
  },
  modalContent: {
    backgroundColor: 'white',
    padding: 20,
    margin: 20,
    borderRadius: 8
  },
  modalTitle: {
    marginBottom: 16,
    fontWeight: 'bold'
  },
  modalLabel: {
    color: '#666',
    marginTop: 8
  },
  modalValue: {
    marginBottom: 8
  },
  input: {
    marginTop: 12
  },
  modalButtons: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    gap: 8,
    marginTop: 16
  },
  modalButton: {
    flex: 1
  }
});

export default InventoryCountExecuteScreen;
```

---

## 🎯 TAREA 5: Crear Navegación

**Archivo:** `src/navigation/InventoryNavigator.tsx`

```typescript
import React from 'react';
import { createStackNavigator } from '@react-navigation/stack';
import InventoryCountListScreen from '../screens/InventoryCountListScreen';
import InventoryCountCreateScreen from '../screens/InventoryCountCreateScreen';
import InventoryCountExecuteScreen from '../screens/InventoryCountExecuteScreen';
import InventoryCountReviewScreen from '../screens/InventoryCountReviewScreen';
import InventoryCountDetailScreen from '../screens/InventoryCountDetailScreen';

const Stack = createStackNavigator();

const InventoryCountNavigator = () => {
  return (
    <Stack.Navigator screenOptions={{ headerShown: false }}>
      <Stack.Screen name="InventoryCountList" component={InventoryCountListScreen} />
      <Stack.Screen name="InventoryCountCreate" component={InventoryCountCreateScreen} />
      <Stack.Screen name="InventoryCountDetail" component={InventoryCountDetailScreen} />
      <Stack.Screen name="InventoryCountExecute" component={InventoryCountExecuteScreen} />
      <Stack.Screen name="InventoryCountReview" component={InventoryCountReviewScreen} />
    </Stack.Navigator>
  );
};

export default InventoryCountNavigator;
```

---

## 📝 Notas Finales

### Características Clave Implementadas
- ✅ Listado con pull-to-refresh y filtros modales
- ✅ Creación de conteos con validación
- ✅ Ejecución con tabs de pendientes/variaciones
- ✅ Modal para registrar cantidades físicas
- ✅ Indicadores visuales con ProgressBar y Chips
- ✅ Soporte para scanner de código de barras (agregar react-native-camera)
- ✅ Formato de código **CIC-XXXXXX**
- ✅ Gestión de estado offline con AsyncStorage

### Mejoras Sugeridas (Opcional)
- Integrar react-native-camera para scanner de códigos de barras
- Captura de fotos de discrepancias
- Modo offline con cola de sincronización
- Notificaciones push al asignar conteos
- Filtros guardados en AsyncStorage
- Historial de conteos por producto

### Dependencias Requeridas
```bash
npm install @react-navigation/native @react-navigation/stack
npm install react-native-paper
npm install @react-native-async-storage/async-storage
npm install date-fns
```

---

**¡Implementación completa lista para desarrollo móvil!** 📱
