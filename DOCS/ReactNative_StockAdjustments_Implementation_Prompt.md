# 📱 IMPLEMENTACIÓN DE AJUSTES DE INVENTARIO - REACT NATIVE FRONTEND

## 🎯 OBJETIVO
Implementar el módulo completo de **Ajustes de Inventario** en la aplicación móvil React Native (Expo) para permitir la corrección de discrepancias entre el stock físico y el sistema desde dispositivos móviles. Este módulo se integra automáticamente con el Kardex para registrar todos los movimientos.

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
- React Native + Expo
- TypeScript
- React Navigation
- axios con Bearer tokens
- AsyncStorage para sesión
- React Native Paper (UI components)
- react-hook-form para formularios

---

## 🎨 DISEÑO DE NAVEGACIÓN

### **Estructura de Navegación:**
```
Inventario (Stack)
  └─ AjustesInventario (Tab o Drawer)
      ├─ StockAdjustmentsListScreen
      ├─ CreateStockAdjustmentScreen
      └─ StockAdjustmentDetailScreen
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
import AsyncStorage from '@react-native-async-storage/async-storage';
import axios from 'axios';
import {
  PagedStockAdjustments,
  StockAdjustment,
  CreateStockAdjustmentDto,
  ProductStock
} from '../types/stockAdjustment.types';

const API_BASE_URL = 'http://192.168.0.72:7254'; // Ajustar según tu configuración

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
    const token = await AsyncStorage.getItem('userToken');
    const response = await axios.get(`${API_BASE_URL}/api/stock-adjustments`, {
      params,
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    return response.data;
  },

  // Obtener detalle de un ajuste
  async getById(id: number): Promise<StockAdjustment> {
    const token = await AsyncStorage.getItem('userToken');
    const response = await axios.get(`${API_BASE_URL}/api/stock-adjustments/${id}`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    return response.data;
  },

  // Crear nuevo ajuste
  async create(data: CreateStockAdjustmentDto): Promise<{ message: string; error: number; data: StockAdjustment }> {
    const token = await AsyncStorage.getItem('userToken');
    const response = await axios.post(`${API_BASE_URL}/api/stock-adjustments`, data, {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    return response.data;
  },

  // Obtener razones disponibles
  async getReasons(): Promise<{ code: string; label: string }[]> {
    const token = await AsyncStorage.getItem('userToken');
    const response = await axios.get(`${API_BASE_URL}/api/stock-adjustments/reasons`, {
      headers: {
        Authorization: `Bearer ${token}`
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
    const token = await AsyncStorage.getItem('userToken');
    const response = await axios.get(`${API_BASE_URL}/api/product-stock`, {
      params,
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    return response.data;
  }
};
```

---

## 📝 TAREA 2: PANTALLA DE LISTADO

### **2.1. Crear `src/screens/Inventory/StockAdjustmentsListScreen.tsx`:**

```typescript
import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  FlatList,
  StyleSheet,
  RefreshControl,
  Alert
} from 'react-native';
import {
  Text,
  Card,
  Title,
  Paragraph,
  Chip,
  FAB,
  Searchbar,
  ActivityIndicator,
  Button,
  Portal,
  Modal,
  List
} from 'react-native-paper';
import { useNavigation, useFocusEffect } from '@react-navigation/native';
import { stockAdjustmentService } from '../../services/stockAdjustmentService';
import { PagedStockAdjustments, AdjustmentReason, AdjustmentReasonLabels } from '../../types/stockAdjustment.types';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

export default function StockAdjustmentsListScreen() {
  const navigation = useNavigation();
  const [data, setData] = useState<PagedStockAdjustments | null>(null);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [page, setPage] = useState(1);
  const [loadingMore, setLoadingMore] = useState(false);

  // Filtros
  const [showFilters, setShowFilters] = useState(false);
  const [filterReason, setFilterReason] = useState<string>('');

  const loadAdjustments = async (pageNumber = 1, refresh = false) => {
    if (refresh) {
      setRefreshing(true);
    } else if (pageNumber === 1) {
      setLoading(true);
    } else {
      setLoadingMore(true);
    }

    try {
      const params: any = {
        page: pageNumber,
        pageSize: 20
      };

      if (filterReason) params.reason = filterReason;

      const result = await stockAdjustmentService.getAll(params);

      if (pageNumber === 1) {
        setData(result);
      } else {
        setData(prev => ({
          ...result,
          items: [...(prev?.items || []), ...result.items]
        }));
      }
      setPage(pageNumber);
    } catch (error: any) {
      Alert.alert('Error', error.response?.data?.message || 'Error al cargar ajustes');
    } finally {
      setLoading(false);
      setRefreshing(false);
      setLoadingMore(false);
    }
  };

  useFocusEffect(
    useCallback(() => {
      loadAdjustments(1);
    }, [filterReason])
  );

  const handleLoadMore = () => {
    if (data && page < data.totalPages && !loadingMore) {
      loadAdjustments(page + 1);
    }
  };

  const getReasonColor = (reason: AdjustmentReason): string => {
    const colors: Record<AdjustmentReason, string> = {
      PHYSICAL_COUNT: '#2196F3',
      DAMAGE: '#F44336',
      LOSS: '#F44336',
      EXPIRATION: '#FF9800',
      ERROR: '#00BCD4',
      SAMPLE: '#4CAF50',
      PRODUCTION_WASTE: '#FF9800',
      OTHER: '#9E9E9E'
    };
    return colors[reason] || '#9E9E9E';
  };

  const renderItem = ({ item }: any) => (
    <Card
      style={styles.card}
      onPress={() => navigation.navigate('StockAdjustmentDetail', { id: item.id })}
    >
      <Card.Content>
        <View style={styles.cardHeader}>
          <Title style={styles.code}>{item.code}</Title>
          <Chip
            mode="flat"
            style={{ backgroundColor: getReasonColor(item.reason) }}
            textStyle={{ color: 'white', fontSize: 10 }}
          >
            {item.reasonLabel}
          </Chip>
        </View>

        <Paragraph style={styles.warehouse}>📦 {item.warehouseName}</Paragraph>
        <Paragraph style={styles.date}>
          📅 {format(new Date(item.adjustmentDate), 'dd/MM/yyyy', { locale: es })}
        </Paragraph>

        <View style={styles.footer}>
          <Text style={styles.footerText}>
            {item.totalProducts} producto{item.totalProducts !== 1 ? 's' : ''}
          </Text>
          <Text style={[styles.footerText, { 
            color: item.totalAdjustmentCost >= 0 ? '#4CAF50' : '#F44336',
            fontWeight: 'bold'
          }]}>
            {new Intl.NumberFormat('es-MX', { 
              style: 'currency', 
              currency: 'MXN' 
            }).format(item.totalAdjustmentCost)}
          </Text>
        </View>

        <Text style={styles.user}>👤 {item.createdByUserName}</Text>
      </Card.Content>
    </Card>
  );

  if (loading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" />
        <Text style={{ marginTop: 10 }}>Cargando ajustes...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {/* Botón de filtros */}
      <View style={styles.filterContainer}>
        <Button
          mode="outlined"
          onPress={() => setShowFilters(true)}
          icon="filter-variant"
        >
          Filtros
        </Button>
      </View>

      {/* Lista */}
      <FlatList
        data={data?.items || []}
        renderItem={renderItem}
        keyExtractor={(item) => item.id.toString()}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => loadAdjustments(1, true)}
          />
        }
        onEndReached={handleLoadMore}
        onEndReachedThreshold={0.5}
        ListFooterComponent={
          loadingMore ? <ActivityIndicator style={{ padding: 10 }} /> : null
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyText}>No hay ajustes de inventario</Text>
          </View>
        }
      />

      {/* FAB para crear nuevo */}
      <FAB
        icon="plus"
        style={styles.fab}
        onPress={() => navigation.navigate('CreateStockAdjustment')}
      />

      {/* Modal de Filtros */}
      <Portal>
        <Modal
          visible={showFilters}
          onDismiss={() => setShowFilters(false)}
          contentContainerStyle={styles.modal}
        >
          <Title>Filtros</Title>
          <List.Section>
            <List.Subheader>Razón del Ajuste</List.Subheader>
            <List.Item
              title="Todas"
              onPress={() => {
                setFilterReason('');
                setShowFilters(false);
              }}
              left={props => <List.Icon {...props} icon={filterReason === '' ? 'check' : 'circle-outline'} />}
            />
            {Object.entries(AdjustmentReasonLabels).map(([key, label]) => (
              <List.Item
                key={key}
                title={label}
                onPress={() => {
                  setFilterReason(key);
                  setShowFilters(false);
                }}
                left={props => <List.Icon {...props} icon={filterReason === key ? 'check' : 'circle-outline'} />}
              />
            ))}
          </List.Section>
          <Button mode="outlined" onPress={() => setShowFilters(false)} style={{ marginTop: 10 }}>
            Cerrar
          </Button>
        </Modal>
      </Portal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5'
  },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center'
  },
  filterContainer: {
    padding: 10,
    backgroundColor: 'white',
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0'
  },
  list: {
    padding: 10
  },
  card: {
    marginBottom: 10,
    elevation: 2
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8
  },
  code: {
    fontSize: 18,
    fontWeight: 'bold',
    margin: 0
  },
  warehouse: {
    fontSize: 14,
    marginVertical: 2
  },
  date: {
    fontSize: 14,
    marginVertical: 2,
    color: '#666'
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 8,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: '#e0e0e0'
  },
  footerText: {
    fontSize: 14
  },
  user: {
    fontSize: 12,
    marginTop: 4,
    color: '#999'
  },
  emptyContainer: {
    padding: 20,
    alignItems: 'center'
  },
  emptyText: {
    fontSize: 16,
    color: '#999'
  },
  fab: {
    position: 'absolute',
    margin: 16,
    right: 0,
    bottom: 0
  },
  modal: {
    backgroundColor: 'white',
    padding: 20,
    margin: 20,
    borderRadius: 8
  }
});
```

---

## 📝 TAREA 3: PANTALLA DE CREACIÓN

### **3.1. Crear `src/screens/Inventory/CreateStockAdjustmentScreen.tsx`:**

```typescript
import React, { useState } from 'react';
import {
  View,
  ScrollView,
  StyleSheet,
  Alert,
  TouchableOpacity
} from 'react-native';
import {
  Text,
  TextInput,
  Button,
  Card,
  Title,
  Portal,
  Modal,
  List,
  Chip,
  DataTable,
  IconButton,
  HelperText,
  ActivityIndicator
} from 'react-native-paper';
import { useNavigation } from '@react-navigation/native';
import { Picker } from '@react-native-picker/picker';
import { stockAdjustmentService } from '../../services/stockAdjustmentService';
import {
  CreateStockAdjustmentDto,
  AdjustmentReasonLabels,
  ProductStock
} from '../../types/stockAdjustment.types';

interface DetailRow extends ProductStock {
  physicalQuantity: number;
  detailNotes?: string;
}

export default function CreateStockAdjustmentScreen() {
  const navigation = useNavigation();
  const [loading, setLoading] = useState(false);

  // Form data
  const [warehouseId, setWarehouseId] = useState<number | null>(null);
  const [adjustmentDate, setAdjustmentDate] = useState(new Date().toISOString().split('T')[0]);
  const [reason, setReason] = useState<string>('');
  const [notes, setNotes] = useState('');
  const [details, setDetails] = useState<DetailRow[]>([]);

  // Validaciones
  const [errors, setErrors] = useState<any>({});

  // Modal de búsqueda de productos
  const [showProductSearch, setShowProductSearch] = useState(false);
  const [searchText, setSearchText] = useState('');
  const [searchResults, setSearchResults] = useState<ProductStock[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);

  // TODO: Cargar almacenes desde tu estado global o API
  const warehouses = [];

  // Buscar productos
  const handleSearchProducts = async () => {
    if (!warehouseId) {
      Alert.alert('Error', 'Primero seleccione un almacén');
      return;
    }

    setSearchLoading(true);
    try {
      const result = await stockAdjustmentService.searchProductStock({
        warehouseId: warehouseId,
        search: searchText,
        page: 1,
        pageSize: 20
      });
      setSearchResults(result.items);
    } catch (error) {
      Alert.alert('Error', 'Error al buscar productos');
    } finally {
      setSearchLoading(false);
    }
  };

  // Agregar producto
  const handleAddProduct = (product: ProductStock) => {
    if (details.some(d => d.productId === product.productId)) {
      Alert.alert('Advertencia', 'Este producto ya está en la lista');
      return;
    }

    setDetails([...details, {
      ...product,
      physicalQuantity: product.quantity,
      detailNotes: ''
    }]);

    setShowProductSearch(false);
    setSearchText('');
    setSearchResults([]);
  };

  // Eliminar producto
  const handleRemoveProduct = (productId: number) => {
    Alert.alert(
      'Confirmar',
      '¿Eliminar este producto?',
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Eliminar',
          onPress: () => setDetails(details.filter(d => d.productId !== productId)),
          style: 'destructive'
        }
      ]
    );
  };

  // Actualizar cantidad física
  const handleUpdatePhysicalQuantity = (productId: number, value: string) => {
    const numValue = parseFloat(value) || 0;
    setDetails(details.map(d =>
      d.productId === productId ? { ...d, physicalQuantity: numValue } : d
    ));
  };

  // Validar y enviar
  const handleSubmit = async () => {
    const newErrors: any = {};

    if (!warehouseId) newErrors.warehouseId = 'Seleccione un almacén';
    if (!reason) newErrors.reason = 'Seleccione una razón';
    if (details.length === 0) newErrors.details = 'Agregue al menos un producto';

    setErrors(newErrors);

    if (Object.keys(newErrors).length > 0) {
      Alert.alert('Error', 'Complete todos los campos requeridos');
      return;
    }

    setLoading(true);

    try {
      const dto: CreateStockAdjustmentDto = {
        warehouseId: warehouseId!,
        adjustmentDate: new Date(adjustmentDate).toISOString(),
        reason: reason as any,
        notes: notes || undefined,
        details: details.map(d => ({
          productId: d.productId,
          systemQuantity: d.quantity,
          physicalQuantity: d.physicalQuantity,
          notes: d.detailNotes || undefined
        }))
      };

      const response = await stockAdjustmentService.create(dto);

      Alert.alert('Éxito', response.message, [
        {
          text: 'OK',
          onPress: () => navigation.navigate('StockAdjustmentDetail', { id: response.data.id })
        }
      ]);
    } catch (error: any) {
      Alert.alert('Error', error.response?.data?.message || 'Error al crear ajuste');
    } finally {
      setLoading(false);
    }
  };

  return (
    <ScrollView style={styles.container}>
      <Card style={styles.card}>
        <Card.Content>
          <Title>Información General</Title>

          {/* Almacén */}
          <Text style={styles.label}>Almacén *</Text>
          <View style={styles.pickerContainer}>
            <Picker
              selectedValue={warehouseId}
              onValueChange={(value) => setWarehouseId(value)}
              style={styles.picker}
            >
              <Picker.Item label="Seleccione..." value={null} />
              {warehouses.map((w: any) => (
                <Picker.Item key={w.id} label={w.name} value={w.id} />
              ))}
            </Picker>
          </View>
          {errors.warehouseId && <HelperText type="error">{errors.warehouseId}</HelperText>}

          {/* Fecha */}
          <TextInput
            label="Fecha de Ajuste *"
            value={adjustmentDate}
            onChangeText={setAdjustmentDate}
            mode="outlined"
            style={styles.input}
          />

          {/* Razón */}
          <Text style={styles.label}>Razón del Ajuste *</Text>
          <View style={styles.pickerContainer}>
            <Picker
              selectedValue={reason}
              onValueChange={(value) => setReason(value)}
              style={styles.picker}
            >
              <Picker.Item label="Seleccione..." value="" />
              {Object.entries(AdjustmentReasonLabels).map(([key, label]) => (
                <Picker.Item key={key} label={label} value={key} />
              ))}
            </Picker>
          </View>
          {errors.reason && <HelperText type="error">{errors.reason}</HelperText>}

          {/* Notas */}
          <TextInput
            label="Notas Generales"
            value={notes}
            onChangeText={setNotes}
            mode="outlined"
            multiline
            numberOfLines={3}
            style={styles.input}
          />
        </Card.Content>
      </Card>

      {/* Productos */}
      <Card style={styles.card}>
        <Card.Content>
          <View style={styles.productHeader}>
            <Title>Productos</Title>
            <Button
              mode="outlined"
              icon="plus"
              onPress={() => setShowProductSearch(true)}
              disabled={!warehouseId}
            >
              Agregar
            </Button>
          </View>

          {errors.details && <HelperText type="error">{errors.details}</HelperText>}

          {details.length === 0 ? (
            <Text style={styles.emptyText}>No hay productos agregados</Text>
          ) : (
            details.map((detail) => {
              const diff = detail.physicalQuantity - detail.quantity;
              return (
                <Card key={detail.productId} style={styles.productCard}>
                  <Card.Content>
                    <View style={styles.productCardHeader}>
                      <View style={{ flex: 1 }}>
                        <Text style={styles.productCode}>{detail.productCode}</Text>
                        <Text style={styles.productName}>{detail.productName}</Text>
                      </View>
                      <IconButton
                        icon="delete"
                        iconColor="#F44336"
                        size={20}
                        onPress={() => handleRemoveProduct(detail.productId)}
                      />
                    </View>

                    <View style={styles.quantityRow}>
                      <View style={{ flex: 1 }}>
                        <Text style={styles.quantityLabel}>Sistema:</Text>
                        <Text style={styles.quantityValue}>{detail.quantity.toFixed(2)}</Text>
                      </View>
                      <View style={{ flex: 1 }}>
                        <Text style={styles.quantityLabel}>Física:</Text>
                        <TextInput
                          value={detail.physicalQuantity.toString()}
                          onChangeText={(value) => handleUpdatePhysicalQuantity(detail.productId, value)}
                          keyboardType="numeric"
                          mode="outlined"
                          dense
                          style={{ height: 40 }}
                        />
                      </View>
                      <View style={{ flex: 1, alignItems: 'center' }}>
                        <Text style={styles.quantityLabel}>Diferencia:</Text>
                        <Chip
                          mode="flat"
                          style={{
                            backgroundColor: diff > 0 ? '#4CAF50' : diff < 0 ? '#F44336' : '#9E9E9E'
                          }}
                          textStyle={{ color: 'white', fontSize: 12 }}
                        >
                          {diff > 0 ? `+${diff.toFixed(2)}` : diff.toFixed(2)}
                        </Chip>
                      </View>
                    </View>
                  </Card.Content>
                </Card>
              );
            })
          )}
        </Card.Content>
      </Card>

      {/* Botones */}
      <View style={styles.buttonContainer}>
        <Button
          mode="outlined"
          onPress={() => navigation.goBack()}
          style={styles.button}
          disabled={loading}
        >
          Cancelar
        </Button>
        <Button
          mode="contained"
          onPress={handleSubmit}
          style={styles.button}
          loading={loading}
          disabled={loading || details.length === 0}
        >
          Guardar
        </Button>
      </View>

      {/* Modal de búsqueda */}
      <Portal>
        <Modal
          visible={showProductSearch}
          onDismiss={() => setShowProductSearch(false)}
          contentContainerStyle={styles.modal}
        >
          <Title>Buscar Producto</Title>
          <View style={styles.searchContainer}>
            <TextInput
              placeholder="Buscar por código o nombre..."
              value={searchText}
              onChangeText={setSearchText}
              mode="outlined"
              style={{ flex: 1 }}
            />
            <IconButton
              icon="magnify"
              mode="contained"
              onPress={handleSearchProducts}
              disabled={searchLoading}
            />
          </View>

          {searchLoading && <ActivityIndicator style={{ marginVertical: 10 }} />}

          <ScrollView style={styles.searchResults}>
            {searchResults.map((product) => (
              <TouchableOpacity
                key={product.productId}
                onPress={() => handleAddProduct(product)}
                style={styles.searchResultItem}
              >
                <View>
                  <Text style={styles.searchResultCode}>{product.productCode}</Text>
                  <Text style={styles.searchResultName}>{product.productName}</Text>
                  <Text style={styles.searchResultStock}>Stock: {product.quantity.toFixed(2)}</Text>
                </View>
              </TouchableOpacity>
            ))}
          </ScrollView>

          <Button mode="outlined" onPress={() => setShowProductSearch(false)} style={{ marginTop: 10 }}>
            Cerrar
          </Button>
        </Modal>
      </Portal>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5'
  },
  card: {
    margin: 10,
    marginBottom: 10
  },
  input: {
    marginVertical: 8
  },
  label: {
    fontSize: 16,
    marginTop: 10,
    marginBottom: 5,
    fontWeight: '500'
  },
  pickerContainer: {
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 4,
    marginVertical: 8
  },
  picker: {
    height: 50
  },
  productHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10
  },
  emptyText: {
    textAlign: 'center',
    color: '#999',
    padding: 20
  },
  productCard: {
    marginBottom: 10,
    backgroundColor: '#fafafa'
  },
  productCardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10
  },
  productCode: {
    fontSize: 12,
    color: '#666'
  },
  productName: {
    fontSize: 16,
    fontWeight: '500'
  },
  quantityRow: {
    flexDirection: 'row',
    gap: 10
  },
  quantityLabel: {
    fontSize: 12,
    color: '#666',
    marginBottom: 4
  },
  quantityValue: {
    fontSize: 16,
    fontWeight: 'bold'
  },
  buttonContainer: {
    flexDirection: 'row',
    padding: 10,
    gap: 10
  },
  button: {
    flex: 1
  },
  modal: {
    backgroundColor: 'white',
    padding: 20,
    margin: 20,
    borderRadius: 8,
    maxHeight: '80%'
  },
  searchContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    marginVertical: 10
  },
  searchResults: {
    maxHeight: 300
  },
  searchResultItem: {
    padding: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0'
  },
  searchResultCode: {
    fontSize: 12,
    color: '#666'
  },
  searchResultName: {
    fontSize: 16,
    fontWeight: '500',
    marginVertical: 2
  },
  searchResultStock: {
    fontSize: 14,
    color: '#2196F3'
  }
});
```

---

## 📝 TAREA 4: PANTALLA DE DETALLE

### **4.1. Crear `src/screens/Inventory/StockAdjustmentDetailScreen.tsx`:**

```typescript
import React, { useState, useEffect } from 'react';
import {
  View,
  ScrollView,
  StyleSheet,
  Alert
} from 'react-native';
import {
  Text,
  Card,
  Title,
  Paragraph,
  Chip,
  DataTable,
  ActivityIndicator
} from 'react-native-paper';
import { useRoute } from '@react-navigation/native';
import { stockAdjustmentService } from '../../services/stockAdjustmentService';
import { StockAdjustment, AdjustmentReason } from '../../types/stockAdjustment.types';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

export default function StockAdjustmentDetailScreen() {
  const route = useRoute();
  const { id } = route.params as { id: number };
  
  const [data, setData] = useState<StockAdjustment | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAdjustment();
  }, [id]);

  const loadAdjustment = async () => {
    try {
      const result = await stockAdjustmentService.getById(id);
      setData(result);
    } catch (error: any) {
      Alert.alert('Error', error.response?.data?.message || 'Error al cargar ajuste');
    } finally {
      setLoading(false);
    }
  };

  const getReasonColor = (reason: AdjustmentReason): string => {
    const colors: Record<AdjustmentReason, string> = {
      PHYSICAL_COUNT: '#2196F3',
      DAMAGE: '#F44336',
      LOSS: '#F44336',
      EXPIRATION: '#FF9800',
      ERROR: '#00BCD4',
      SAMPLE: '#4CAF50',
      PRODUCTION_WASTE: '#FF9800',
      OTHER: '#9E9E9E'
    };
    return colors[reason] || '#9E9E9E';
  };

  if (loading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" />
        <Text style={{ marginTop: 10 }}>Cargando ajuste...</Text>
      </View>
    );
  }

  if (!data) {
    return (
      <View style={styles.centered}>
        <Text>Ajuste no encontrado</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      {/* Información General */}
      <Card style={styles.card}>
        <Card.Content>
          <Title>Información General</Title>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Código:</Text>
            <Text style={styles.infoValue}>{data.code}</Text>
          </View>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Almacén:</Text>
            <Text style={styles.infoValue}>{data.warehouseName}</Text>
          </View>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Fecha:</Text>
            <Text style={styles.infoValue}>
              {format(new Date(data.adjustmentDate), 'dd/MM/yyyy', { locale: es })}
            </Text>
          </View>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Razón:</Text>
            <Chip
              mode="flat"
              style={{ backgroundColor: getReasonColor(data.reason) }}
              textStyle={{ color: 'white', fontSize: 12 }}
            >
              {data.reasonLabel}
            </Chip>
          </View>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Creado por:</Text>
            <Text style={styles.infoValue}>{data.createdByUserName}</Text>
          </View>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Productos:</Text>
            <Text style={styles.infoValue}>{data.totalProducts}</Text>
          </View>

          <View style={styles.infoRow}>
            <Text style={styles.infoLabel}>Costo Total:</Text>
            <Text style={[
              styles.infoValue,
              {
                color: data.totalAdjustmentCost >= 0 ? '#4CAF50' : '#F44336',
                fontWeight: 'bold'
              }
            ]}>
              {new Intl.NumberFormat('es-MX', {
                style: 'currency',
                currency: 'MXN'
              }).format(data.totalAdjustmentCost)}
            </Text>
          </View>

          {data.notes && (
            <View style={styles.notesContainer}>
              <Text style={styles.infoLabel}>Notas:</Text>
              <Text style={styles.notesText}>{data.notes}</Text>
            </View>
          )}
        </Card.Content>
      </Card>

      {/* Productos */}
      <Card style={styles.card}>
        <Card.Content>
          <Title>Productos Ajustados</Title>
          
          {data.details.map((detail, index) => (
            <Card key={detail.id} style={styles.detailCard}>
              <Card.Content>
                <Text style={styles.detailCode}>{detail.productCode}</Text>
                <Text style={styles.detailName}>{detail.productName}</Text>

                <View style={styles.detailQuantities}>
                  <View style={{ flex: 1 }}>
                    <Text style={styles.detailLabel}>Sistema:</Text>
                    <Text style={styles.detailValue}>{detail.systemQuantity.toFixed(2)}</Text>
                  </View>
                  <View style={{ flex: 1 }}>
                    <Text style={styles.detailLabel}>Física:</Text>
                    <Text style={styles.detailValue}>{detail.physicalQuantity.toFixed(2)}</Text>
                  </View>
                  <View style={{ flex: 1, alignItems: 'center' }}>
                    <Text style={styles.detailLabel}>Diferencia:</Text>
                    <Chip
                      mode="flat"
                      style={{
                        backgroundColor: detail.adjustmentQuantity! > 0 ? '#4CAF50' : detail.adjustmentQuantity! < 0 ? '#F44336' : '#9E9E9E'
                      }}
                      textStyle={{ color: 'white', fontSize: 12 }}
                    >
                      {detail.adjustmentQuantity! > 0 ? `+${detail.adjustmentQuantity!.toFixed(2)}` : detail.adjustmentQuantity!.toFixed(2)}
                    </Chip>
                  </View>
                </View>

                {detail.unitCost && (
                  <View style={styles.costRow}>
                    <Text style={styles.costLabel}>Costo Unit: ${detail.unitCost.toFixed(2)}</Text>
                    <Text style={[
                      styles.costTotal,
                      { color: detail.totalCost && detail.totalCost >= 0 ? '#4CAF50' : '#F44336' }
                    ]}>
                      {detail.totalCost ? new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(detail.totalCost) : 'N/A'}
                    </Text>
                  </View>
                )}

                {detail.notes && (
                  <Text style={styles.detailNotes}>📝 {detail.notes}</Text>
                )}
              </Card.Content>
            </Card>
          ))}
        </Card.Content>
      </Card>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5'
  },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center'
  },
  card: {
    margin: 10
  },
  infoRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0'
  },
  infoLabel: {
    fontSize: 14,
    color: '#666',
    fontWeight: '500'
  },
  infoValue: {
    fontSize: 14,
    fontWeight: '500'
  },
  notesContainer: {
    marginTop: 10,
    paddingTop: 10,
    borderTopWidth: 1,
    borderTopColor: '#e0e0e0'
  },
  notesText: {
    fontSize: 14,
    marginTop: 5,
    color: '#666'
  },
  detailCard: {
    marginBottom: 10,
    backgroundColor: '#fafafa'
  },
  detailCode: {
    fontSize: 12,
    color: '#666'
  },
  detailName: {
    fontSize: 16,
    fontWeight: '500',
    marginVertical: 4
  },
  detailQuantities: {
    flexDirection: 'row',
    marginTop: 10,
    gap: 10
  },
  detailLabel: {
    fontSize: 12,
    color: '#666',
    marginBottom: 4
  },
  detailValue: {
    fontSize: 16,
    fontWeight: 'bold'
  },
  costRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 8,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: '#e0e0e0'
  },
  costLabel: {
    fontSize: 14,
    color: '#666'
  },
  costTotal: {
    fontSize: 14,
    fontWeight: 'bold'
  },
  detailNotes: {
    fontSize: 12,
    color: '#666',
    marginTop: 8,
    fontStyle: 'italic'
  }
});
```

---

## 📝 TAREA 5: CONFIGURAR NAVEGACIÓN

### **5.1. Agregar navegación en tu Stack Navigator:**

```typescript
import StockAdjustmentsListScreen from './screens/Inventory/StockAdjustmentsListScreen';
import CreateStockAdjustmentScreen from './screens/Inventory/CreateStockAdjustmentScreen';
import StockAdjustmentDetailScreen from './screens/Inventory/StockAdjustmentDetailScreen';

// En tu Stack Navigator:
<Stack.Screen
  name="StockAdjustmentsList"
  component={StockAdjustmentsListScreen}
  options={{ title: 'Ajustes de Inventario' }}
/>
<Stack.Screen
  name="CreateStockAdjustment"
  component={CreateStockAdjustmentScreen}
  options={{ title: 'Nuevo Ajuste' }}
/>
<Stack.Screen
  name="StockAdjustmentDetail"
  component={StockAdjustmentDetailScreen}
  options={{ title: 'Detalle del Ajuste' }}
/>
```

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

- [ ] Tipos y servicios creados
- [ ] Pantalla de listado funcional con pull-to-refresh
- [ ] Pantalla de creación funcional
- [ ] Pantalla de detalle funcional
- [ ] Navegación configurada
- [ ] Búsqueda de productos implementada
- [ ] Validaciones de formulario
- [ ] Manejo de errores
- [ ] AsyncStorage para sesión

---

## 🎨 MEJORAS OPCIONALES

1. **Escáner de código de barras**: Integrar con `expo-barcode-scanner` para buscar productos por código de barras
2. **Filtros Avanzados**: Modal con más filtros (fecha, rango de costos, usuario)
3. **Modo Offline**: Guardar ajustes pendientes y sincronizar cuando haya conexión
4. **Cámara para evidencia**: Tomar fotos de productos dañados/vencidos
5. **Notificaciones**: Alertas cuando se completan ajustes

---

## 📚 NOTAS IMPORTANTES

- ✅ Backend ya está completo y operativo
- ✅ Todos los cálculos se hacen automáticamente en el backend
- ✅ El Kardex se actualiza automáticamente
- ✅ Los códigos se generan automáticamente (ADJ-XXXXXX)
- ✅ El stock se actualiza en tiempo real
- ⚠️ Adaptar `API_BASE_URL` a tu IP/dominio
- ⚠️ Cargar lista de almacenes desde tu estado global o API
- ⚠️ Adaptar estilos y colores a tu tema existente
- ⚠️ Instalar dependencias: `@react-native-picker/picker`, `react-native-paper`

---

**FIN DEL PROMPT - REACT NATIVE FRONTEND**
