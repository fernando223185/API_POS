# Prompt: Implementar QR Deep Linking para Warehouse Transfers - React Native + Expo

## CONTEXTO DEL PROYECTO

### Stack Tecnológico
- **Framework:** React Native + Expo
- **Entorno de desarrollo:** Expo Go
- **Expo Metro:** `exp://192.168.192.57:8081`
- **Comando de inicio:** `$env:REACT_NATIVE_PACKAGER_HOSTNAME="192.168.192.57"; npx expo start --lan`
- **Navegación:** React Navigation (probablemente)
- **HTTP Client:** axios con Bearer token
- **UI:** React Native components nativos

### Arquitectura
```
App Móvil (React Native)
├── Expo Go (desarrollo)
├── Expo Metro → exp://192.168.192.57:8081
└── Backend API separado (ya implementado)
```

### Electron (Ya Implementado ✅)
- Landing page creada y funcional
- PDF con QR generado correctamente
- URL del QR: `http://192.168.0.72:3000/warehouse-transfers/{id}/receive`

---

## OBJETIVO

Implementar funcionalidad de QR con deep linking en la app móvil para recibir traspasos de almacén.

### Dos Flujos Soportados:

**A) Flujo Externo** (Escáner de cámara nativa - TAREAS 1-4):
1. Usuario escanea QR del PDF con cámara del celular (fuera de la app)
2. Se abre landing page (Electron) en el navegador
3. Landing page redirige a: `exp://192.168.192.57:8081/--/warehouse-transfers/{id}/receive`
4. Sistema operativo pregunta: "Abrir con Expo Go?"
5. Expo Go valida sesión y abre la pantalla de recepción

**B) Flujo Interno** (Escáner integrado en la app - TAREA 5 OPCIONAL):
1. Usuario abre pantalla de escáner dentro de la app
2. Escanea QR del PDF
3. App extrae ID directamente del QR
4. App valida sesión y navega a pantalla de recepción
5. Todo sucede dentro de la app (sin salir al navegador)

**Importante:** El mismo QR funciona para ambos flujos. Puedes implementar solo el flujo externo (obligatorio) o ambos (recomendado para mejor UX).

### Flujo Visual (Externo)
```
┌──────────────────────────────────────────────────────┐
│  1. Celular: Escanea QR con cámara nativa           │
│     Se abre Chrome/Safari                            │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  2. Landing Page: Detecta móvil                      │
│     Redirige a: exp://192.168.192.57:8081/...       │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  3. Sistema: Pregunta "Abrir con Expo Go?"           │
│     Usuario toca "Abrir"                             │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  4. App: Valida sesión                               │
│     ┌─────────────────┬─────────────────┐           │
│     │ Sin sesión      │ Con sesión      │           │
│     │ ↓               │ ↓               │           │
│     │ Guarda deep link│ Navega directo  │           │
│     │ → Login         │ a recepción     │           │
│     │ → Procesa link  │                 │           │
│     └─────────────────┴─────────────────┘           │
└──────────────────────────────────────────────────────┘
```

### Flujo Visual (Interno - Opcional)
```
┌──────────────────────────────────────────────────────┐
│  1. Usuario: Abre pantalla de escáner en la app     │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  2. Escanea QR → App extrae ID con regex            │
│     /warehouse-transfers/(\d+)/receive/             │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  3. App: Valida sesión                               │
│     ┌─────────────────┬─────────────────┐           │
│     │ Sin sesión      │ Con sesión      │           │
│     │ ↓               │ ↓               │           │
│     │ Guarda ID       │ Navega directo  │           │
│     │ → Login         │ a recepción     │           │
│     │ → Procesa ID    │                 │           │
│     └─────────────────┴─────────────────┘           │
└──────────────────────────────────────────────────────┘
```

---

## TAREAS A IMPLEMENTAR

### ✅ TAREA 1: Configurar Deep Linking en app.json

**Archivo:** `app.json` (raíz del proyecto React Native)

**Cambio requerido:**

```json
{
  "expo": {
    "name": "TuApp",
    "slug": "tu-app",
    "version": "1.0.0",
    
    // ✅ AGREGAR ESTAS LÍNEAS
    "scheme": "easypos",
    
    // ... resto de configuración ...
  }
}
```

**Explicación:**
- `scheme: "easypos"` permite que la app responda a URLs `easypos://...`
- En desarrollo, Expo Go maneja automáticamente los deep links con `exp://`
- En producción (build standalone), usará `easypos://`

---

### ✅ TAREA 2: Configurar React Navigation con Deep Linking

**Archivo:** `App.tsx` o `App.jsx` (componente raíz)

**Cambio requerido:**

```tsx
import React, { useEffect } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import * as Linking from 'expo-linking';

const Stack = createNativeStackNavigator();

// Configuración de deep linking
const linking = {
  prefixes: [
    'exp://192.168.192.57:8081',  // Expo Go en desarrollo
    'easypos://',                  // Producción (build standalone)
  ],
  config: {
    screens: {
      // ... otras pantallas existentes ...
      
      // ✅ AGREGAR ESTA CONFIGURACIÓN
      WarehouseTransferReceive: {
        path: 'warehouse-transfers/:id/receive',
        parse: {
          id: (id) => `${id}`, // Asegura que sea string
        },
      },
    },
  },
};

export default function App() {
  const navigationRef = React.useRef();
  const [isAuthenticated, setIsAuthenticated] = React.useState(false);
  const [pendingDeepLink, setPendingDeepLink] = React.useState(null);

  // Verificar sesión al iniciar
  useEffect(() => {
    checkAuthStatus();
  }, []);

  const checkAuthStatus = async () => {
    const token = await AsyncStorage.getItem('token');
    setIsAuthenticated(!!token);
  };

  // Procesar deep link pendiente después de login
  useEffect(() => {
    if (isAuthenticated && pendingDeepLink && navigationRef.current) {
      console.log('[Deep Link] Procesando link pendiente:', pendingDeepLink);
      // React Navigation procesará automáticamente el deep link
      setPendingDeepLink(null);
    }
  }, [isAuthenticated, pendingDeepLink]);

  useEffect(() => {
    const handleDeepLink = async (url) => {
      console.log('[Deep Link] URL recibida:', url);
      
      // Verificar si hay sesión
      const token = await AsyncStorage.getItem('token');
      
      if (!token) {
        console.log('[Deep Link] Sin sesión - guardando link pendiente');
        setPendingDeepLink(url);
        // Redirigir a login
        navigationRef.current?.navigate('Login');
        return;
      }
      
      // Si hay sesión, React Navigation maneja la navegación automáticamente
      console.log('[Deep Link] Con sesión - navegando');
    };

    // Listener para deep links cuando la app está cerrada
    Linking.getInitialURL().then((url) => {
      if (url) handleDeepLink(url);
    });

    // Listener para deep links cuando la app está en background
    const subscription = Linking.addEventListener('url', ({ url }) => {
      handleDeepLink(url);
    });

    return () => subscription.remove();
  }, []);

  return (
    <NavigationContainer 
      ref={navigationRef}
      linking={linking}
    >
      <Stack.Navigator>
        {/* ... tus pantallas existentes ... */}
        
        {/* ✅ AGREGAR ESTA PANTALLA */}
        <Stack.Screen 
          name="WarehouseTransferReceive" 
          component={WarehouseTransferReceiveScreen}
          options={{ title: 'Registrar Entrada' }}
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
```

**Si NO usas React Navigation:**

```tsx
import React, { useEffect } from 'react';
import { Linking } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

export default function App() {
  const [pendingDeepLink, setPendingDeepLink] = React.useState(null);

  useEffect(() => {
    // Handler para procesar deep links manualmente
    const handleDeepLink = async (url) => {
      console.log('[Deep Link] URL recibida:', url);
      
      // ✅ VALIDAR SESIÓN PRIMERO
      const token = await AsyncStorage.getItem('token');
      
      if (!token) {
        console.log('[Deep Link] Sin sesión - guardando link pendiente');
        setPendingDeepLink(url);
        // Navegar a login
        // yourNavigator.navigate('Login');
        return;
      }
      
      // Extraer ID del traspaso
      // Ejemplo: exp://192.168.192.57:8081/--/warehouse-transfers/123/receive
      const match = url.match(/warehouse-transfers\/(\d+)\/receive/);
      
      if (match) {
        const transferId = match[1];
        console.log('[Deep Link] Navegando a traspaso ID:', transferId);
        
        // AQUÍ: Navega a tu pantalla de entrada usando tu sistema de navegación
        // yourNavigator.navigate('WarehouseTransferReceive', { id: transferId });
      }
    };

    // Listener para cuando la app está cerrada
    Linking.getInitialURL()
      .then((url) => {
        if (url) handleDeepLink(url);
      })
      .catch((err) => console.error('[Deep Link] Error:', err));

    // Listener para cuando la app está en background
    const subscription = Linking.addEventListener('url', ({ url }) => {
      handleDeepLink(url);
    });

    return () => subscription.remove();
  }, []);

  // Procesar deep link pendiente después de login exitoso
  useEffect(() => {
    if (pendingDeepLink && yourCurrentUserState.isLoggedIn) {
      console.log('[Deep Link] Procesando link pendiente después de login');
      // Extraer y navegar al link guardado
      const match = pendingDeepLink.match(/warehouse-transfers\/(\d+)\/receive/);
      if (match) {
        // yourNavigator.navigate('WarehouseTransferReceive', { id: match[1] });
      }
      setPendingDeepLink(null);
    }
  }, [pendingDeepLink, yourCurrentUserState.isLoggedIn]);

  return (
    // Tu app normal
  );
}
```

---

### ✅ TAREA 3: Crear Pantalla de Recepción

**Archivo nuevo:** `src/screens/WarehouseTransferReceiveScreen.jsx` (o `.tsx`)

**Ubicación sugerida:** Donde tengas tus otras pantallas (screens/, pages/, etc.)

**Código completo:**

```jsx
import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  ScrollView,
  TextInput,
  TouchableOpacity,
  Switch,
  ActivityIndicator,
  Alert,
  StyleSheet,
} from 'react-native';
import { useRoute, useNavigation } from '@react-navigation/native';
import axios from 'axios';

// Configurar base URL de tu API
const API_BASE_URL = 'http://TU_API_URL_AQUI/api'; // ← CAMBIAR

const headers = () => ({
  Authorization: `Bearer ${/* TU TOKEN AQUI */}`,
  'Content-Type': 'application/json',
});

export default function WarehouseTransferReceiveScreen() {
  const route = useRoute();
  const navigation = useNavigation();
  const { id } = route.params;

  // Estados
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [transfer, setTransfer] = useState(null);
  const [receivingDate, setReceivingDate] = useState(
    new Date().toISOString().split('T')[0]
  );
  const [notes, setNotes] = useState('');
  const [receiveAll, setReceiveAll] = useState(false);
  const [quantities, setQuantities] = useState({});

  // Solo productos con pendiente > 0
  const pendingDetails = transfer?.details?.filter((d) => d.pendingQuantity > 0) ?? [];

  // Cargar datos del traspaso
  useEffect(() => {
    fetchTransfer();
  }, [id]);

  // Actualizar cantidades cuando se activa "Recibir todo"
  useEffect(() => {
    if (transfer && receiveAll) {
      const newQty = {};
      pendingDetails.forEach((d) => {
        newQty[d.id] = d.pendingQuantity;
      });
      setQuantities(newQty);
    }
  }, [receiveAll, transfer]);

  const fetchTransfer = async () => {
    setLoading(true);
    try {
      const response = await axios.get(
        `${API_BASE_URL}/warehouse-transfers/${id}`,
        { headers: headers() }
      );
      
      const data = response.data;
      setTransfer(data);

      // Inicializar cantidades en 0
      const initial = {};
      data.details.forEach((d) => {
        initial[d.id] = 0;
      });
      setQuantities(initial);
    } catch (err) {
      Alert.alert(
        'Error',
        err?.response?.data?.message || 'No se pudo cargar el traspaso'
      );
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
    // Validar que al menos una cantidad > 0
    const details = pendingDetails
      .filter((d) => quantities[d.id] > 0)
      .map((d) => ({
        warehouseTransferDetailId: d.id,
        quantityReceived: quantities[d.id],
        notes: null,
      }));

    if (details.length === 0) {
      Alert.alert('Error', 'Debes ingresar al menos una cantidad mayor a 0.');
      return;
    }

    setSubmitting(true);
    try {
      await axios.post(
        `${API_BASE_URL}/warehouse-transfers/${id}/receivings`,
        {
          receivingDate: new Date(receivingDate).toISOString(),
          notes,
          details,
        },
        { headers: headers() }
      );

      Alert.alert('Éxito', 'Entrada registrada correctamente', [
        { text: 'OK', onPress: () => navigation.goBack() },
      ]);
    } catch (err) {
      Alert.alert(
        'Error',
        err?.response?.data?.message || 'Error al registrar entrada'
      );
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color="#1a3c6e" />
        <Text style={styles.loadingText}>Cargando traspaso...</Text>
      </View>
    );
  }

  if (!transfer) {
    return (
      <View style={styles.centered}>
        <Text style={styles.errorText}>Traspaso no encontrado</Text>
      </View>
    );
  }

  if (transfer.status !== 'Dispatched' && transfer.status !== 'PartiallyReceived') {
    return (
      <View style={styles.container}>
        <Text style={styles.warningText}>
          Este traspaso no permite registrar entradas
        </Text>
        <Text style={styles.subtitle}>Estado actual: {transfer.status}</Text>
        <TouchableOpacity
          style={styles.button}
          onPress={() => navigation.goBack()}
        >
          <Text style={styles.buttonText}>Volver</Text>
        </TouchableOpacity>
      </View>
    );
  }

  if (pendingDetails.length === 0) {
    return (
      <View style={styles.container}>
        <Text style={styles.successText}>
          ¡Todo recibido! No hay productos pendientes.
        </Text>
        <TouchableOpacity
          style={styles.button}
          onPress={() => navigation.goBack()}
        >
          <Text style={styles.buttonText}>Volver</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      {/* Header */}
      <Text style={styles.title}>Registrar Entrada — {transfer.code}</Text>
      <Text style={styles.subtitle}>
        Destino: {transfer.destinationWarehouseName}
      </Text>

      {/* Fecha */}
      <TextInput
        style={styles.input}
        placeholder="Fecha (YYYY-MM-DD)"
        value={receivingDate}
        onChangeText={setReceivingDate}
      />

      {/* Notas */}
      <TextInput
        style={[styles.input, styles.textArea]}
        placeholder="Notas (opcional)"
        multiline
        numberOfLines={3}
        value={notes}
        onChangeText={setNotes}
      />

      {/* Recibir todo */}
      <View style={styles.switchRow}>
        <Text style={styles.switchLabel}>Recibir todo lo pendiente</Text>
        <Switch value={receiveAll} onValueChange={setReceiveAll} />
      </View>

      {/* Lista de productos pendientes */}
      {pendingDetails.map((d) => (
        <View key={d.id} style={styles.productCard}>
          <Text style={styles.productCode}>{d.productCode}</Text>
          <Text style={styles.productName}>{d.productName}</Text>
          <Text style={styles.pending}>
            Pendiente: {d.pendingQuantity.toLocaleString()}
          </Text>

          <TextInput
            style={styles.quantityInput}
            keyboardType="numeric"
            placeholder="Cantidad a recibir"
            value={String(quantities[d.id] ?? 0)}
            onChangeText={(val) => {
              const parsed = Math.max(
                0,
                Math.min(Number(val) || 0, d.pendingQuantity)
              );
              setQuantities((prev) => ({ ...prev, [d.id]: parsed }));
              setReceiveAll(false);
            }}
          />
        </View>
      ))}

      {/* Botón confirmar */}
      <TouchableOpacity
        style={[styles.button, submitting && styles.buttonDisabled]}
        onPress={handleSubmit}
        disabled={submitting}
      >
        <Text style={styles.buttonText}>
          {submitting ? 'Procesando...' : 'Confirmar Entrada'}
        </Text>
      </TouchableOpacity>

      <View style={{ height: 40 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 16,
    backgroundColor: '#f5f5f5',
  },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  title: {
    fontSize: 20,
    fontWeight: 'bold',
    marginBottom: 8,
    color: '#1a3c6e',
  },
  subtitle: {
    fontSize: 14,
    color: '#666',
    marginBottom: 16,
  },
  loadingText: {
    marginTop: 10,
    fontSize: 14,
    color: '#666',
  },
  errorText: {
    fontSize: 16,
    color: '#f44336',
  },
  warningText: {
    fontSize: 16,
    color: '#ff9800',
    marginBottom: 10,
  },
  successText: {
    fontSize: 16,
    color: '#4caf50',
    marginBottom: 20,
  },
  input: {
    backgroundColor: '#fff',
    padding: 12,
    borderRadius: 8,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#ddd',
    fontSize: 14,
  },
  textArea: {
    height: 80,
    textAlignVertical: 'top',
  },
  switchRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
    padding: 12,
    backgroundColor: '#fff',
    borderRadius: 8,
  },
  switchLabel: {
    fontSize: 14,
    color: '#333',
  },
  productCard: {
    backgroundColor: '#fff',
    padding: 12,
    borderRadius: 8,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#e0e0e0',
  },
  productCode: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#1a3c6e',
  },
  productName: {
    fontSize: 14,
    marginTop: 4,
    color: '#333',
  },
  pending: {
    fontSize: 12,
    color: '#ff9800',
    marginTop: 4,
    fontWeight: 'bold',
  },
  quantityInput: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 4,
    padding: 8,
    marginTop: 8,
    backgroundColor: '#fff',
    fontSize: 14,
  },
  button: {
    backgroundColor: '#1a3c6e',
    padding: 16,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 16,
  },
  buttonDisabled: {
    backgroundColor: '#ccc',
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
});
```

---

### ✅ TAREA 4: Configurar API y Autenticación

**Archivo:** Donde manejes la configuración de axios (probablemente `src/api/` o `src/services/`)

**Actualizar:**

```javascript
// api/config.js o similar
import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

export const API_BASE_URL = 'http://TU_API_URL_AQUI/api'; // ← CAMBIAR

export const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para agregar token automáticamente
api.interceptors.request.use(async (config) => {
  const token = await AsyncStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Exportar funciones de API
export const warehouseTransferApi = {
  getById: (id) => api.get(`/warehouse-transfers/${id}`),
  createReceiving: (id, data) => api.post(`/warehouse-transfers/${id}/receivings`, data),
};
```

**Nota:** Adapta esto a tu estructura de proyecto existente.

---

### ✅ TAREA 5 (OPCIONAL): Integrar Escáner de QR Interno

**Si ya tienes un módulo de escáner de QR integrado en tu app** (ej: `expo-barcode-scanner`, `react-native-camera`), puedes procesar el QR directamente sin pasar por el navegador.

#### Ventajas del Escáner Interno:
- ✅ Más rápido (no sale de la app)
- ✅ Mejor UX (no hay redirecciones)
- ✅ Funciona offline para extraer el ID
- ✅ Mismo QR funciona para ambos flujos

#### Código ejemplo con expo-barcode-scanner:

**Archivo:** Tu pantalla de escáner existente (ej: `QRScannerScreen.jsx`)

```jsx
import { useState } from 'react';
import { View, Alert, StyleSheet } from 'react-native';
import { BarCodeScanner } from 'expo-barcode-scanner';
import { useNavigation } from '@react-navigation/native';
import AsyncStorage from '@react-native-async-storage/async-storage';

export default function QRScannerScreen() {
  const navigation = useNavigation();
  const [scanned, setScanned] = useState(false);

  const handleBarCodeScanned = async ({ type, data }) => {
    setScanned(true);
    console.log('[QR Scanner] Código escaneado:', data);

    // ✅ Procesar el QR
    await processWarehouseTransferQR(data);
  };

  const processWarehouseTransferQR = async (qrData) => {
    // Extraer ID del traspaso del QR
    // Ejemplo: "http://192.168.0.72:3000/warehouse-transfers/123/receive"
    const match = qrData.match(/warehouse-transfers\/(\d+)\/receive/);
    
    if (!match) {
      Alert.alert('QR Inválido', 'Este código QR no corresponde a un traspaso.');
      setScanned(false);
      return;
    }

    const transferId = match[1];
    console.log('[QR Scanner] ID del traspaso:', transferId);

    // ✅ VALIDAR SESIÓN (igual que en deep links)
    const token = await AsyncStorage.getItem('token');
    
    if (!token) {
      console.log('[QR Scanner] Sin sesión - guardando ID pendiente');
      // Guardar ID pendiente
      await AsyncStorage.setItem('pendingTransferId', transferId);
      
      Alert.alert(
        'Iniciar Sesión',
        'Necesitas iniciar sesión para registrar esta entrada.',
        [
          {
            text: 'Cancelar',
            style: 'cancel',
            onPress: () => setScanned(false),
          },
          {
            text: 'Ir a Login',
            onPress: () => navigation.navigate('Login'),
          },
        ]
      );
      return;
    }

    // ✅ CON SESIÓN: Navegar directo a la pantalla de recepción
    console.log('[QR Scanner] Con sesión - navegando directo');
    navigation.navigate('WarehouseTransferReceive', { id: transferId });
  };

  return (
    <View style={styles.container}>
      <BarCodeScanner
        onBarCodeScanned={scanned ? undefined : handleBarCodeScanned}
        style={StyleSheet.absoluteFillObject}
      />
      {/* Aquí puedes agregar UI: overlay, botón cerrar, etc. */}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'black',
  },
});
```

#### Procesar ID pendiente después de login:

**Actualizar tu pantalla de Login:**

```jsx
// LoginScreen.jsx (o el nombre que uses)
import AsyncStorage from '@react-native-async-storage/async-storage';

const handleLogin = async (credentials) => {
  try {
    const response = await authApi.login(credentials);
    const { token } = response.data;
    
    // Guardar token
    await AsyncStorage.setItem('token', token);
    
    // ✅ Verificar si hay un traspaso pendiente desde el escáner
    const pendingTransferId = await AsyncStorage.getItem('pendingTransferId');
    
    if (pendingTransferId) {
      console.log('[Login] Procesando traspaso pendiente:', pendingTransferId);
      await AsyncStorage.removeItem('pendingTransferId');
      
      // Navegar directo a la pantalla de recepción
      navigation.replace('WarehouseTransferReceive', { id: pendingTransferId });
    } else {
      // Login normal, ir al home
      navigation.replace('Home');
    }
  } catch (err) {
    Alert.alert('Error', 'Credenciales inválidas');
  }
};
```

#### Comparación de Flujos:

| Aspecto | Escáner Externo (Cámara) | Escáner Interno (App) |
|---------|-------------------------|----------------------|
| **Herramienta** | Cámara nativa del celular | `expo-barcode-scanner` en la app |
| **Proceso** | QR → Navegador → Landing → exp:// → App | QR → Extraer ID → Navegar directo |
| **Código leído** | URL completa | URL completa (mismo QR) |
| **Validación sesión** | En `App.tsx` (deep link handler) | En `QRScannerScreen.jsx` |
| **Ventaja** | Funciona sin tener la app abierta | Más rápido, no sale de la app |
| **Desventaja** | Varias redirecciones | Requiere tener la app abierta |

#### Instalación de dependencias (si no las tienes):

```bash
npx expo install expo-barcode-scanner
npx expo install expo-camera
```

#### Permisos necesarios:

**En tu App.tsx o componente raíz:**

```jsx
import { Camera } from 'expo-camera';
import { useEffect, useState } from 'react';

function App() {
  const [hasPermission, setHasPermission] = useState(null);

  useEffect(() => {
    (async () => {
      const { status } = await Camera.requestCameraPermissionsAsync();
      setHasPermission(status === 'granted');
    })();
  }, []);

  // ... resto de tu app
}
```

---

## VERIFICACIÓN Y PRUEBAS

### Checklist de Implementación

- [ ] Configurado `scheme: "easypos"` en `app.json`
- [ ] Configurado deep linking en `App.tsx` con prefixes y screens
- [ ] Agregado listener de `Linking.getInitialURL()` y `Linking.addEventListener()`
- [ ] **✅ IMPORTANTE: Agregada validación de sesión en handleDeepLink**
- [ ] **✅ IMPORTANTE: Implementado guardado de deep link pendiente**
- [ ] **✅ IMPORTANTE: Procesamiento de deep link después de login exitoso**
- [ ] Creada pantalla `WarehouseTransferReceiveScreen`
- [ ] Agregada pantalla a navegador (Stack.Navigator)
- [ ] Configurada API base URL
- [ ] Configurado sistema de autenticación (token)
- [ ] **(OPCIONAL) Integrado escáner de QR interno con validación de sesión**
- [ ] **(OPCIONAL) Procesamiento de ID pendiente en pantalla de Login**

### Cómo Probar

#### 1. Verificar Expo está corriendo correctamente

```powershell
$env:REACT_NATIVE_PACKAGER_HOSTNAME="192.168.192.57"
npx expo start --lan
```

Debe mostrar:
```
Metro waiting on exp://192.168.192.57:8081
```

#### 2. Probar deep link manualmente (desde terminal)

**Android (con dispositivo conectado o emulador):**
```bash
adb shell am start -W -a android.intent.action.VIEW -d "exp://192.168.192.57:8081/--/warehouse-transfers/123/receive"
```

**iOS (simulador):**
```bash
xcrun simctl openurl booted "exp://192.168.192.57:8081/--/warehouse-transfers/123/receive"
```

**Resultado esperado:**
- Expo Go se abre
- Navega a la pantalla de recepción
- Muestra traspaso con ID 123

#### 3. Probar flujo completo con QR

**Caso 1: Usuario CON sesión activa**
```
1. Desde Electron, generar PDF de despacho
2. Verificar que QR contiene: http://192.168.0.72:3000/warehouse-transfers/123/receive
3. Escanear QR con cámara del celular
4. Se abre navegador → Landing page
5. Landing page redirige a exp://...
6. Sistema pregunta "Abrir con Expo Go?"
7. Expo Go se abre
8. App valida sesión → ✅ Hay token
9. Navega directamente a pantalla de recepción
```

**Caso 2: Usuario SIN sesión (cerró sesión o primera vez)**
```
1. Escanear QR con cámara del celular
2. Se abre navegador → Landing page → exp://...
3. Expo Go se abre
4. App valida sesión → ❌ No hay token
5. Guarda deep link pendiente: "warehouse-transfers/123/receive"
6. Redirige a pantalla de Login
7. Usuario ingresa credenciales → Login exitoso
8. App procesa deep link guardado
9. Navega a pantalla de recepción con ID 123
```

#### 4. Probar funcionalidad de recepción

```
1. En pantalla de recepción, verificar datos del traspaso
2. Probar switch "Recibir todo"
3. Modificar cantidades individuales
4. Presionar "Confirmar Entrada"
5. Verificar que se registra correctamente en el backend
```

#### 5. Probar escáner interno de QR (si implementaste TAREA 5)

**Caso 1: Escáner con sesión activa**
```
1. Abrir la app con sesión iniciada
2. Navegar a la pantalla de escáner de QR
3. Escanear el QR del PDF
4. App detecta el ID del traspaso
5. Valida sesión → ✅ Hay token
6. Navega directamente a pantalla de recepción
7. Muestra datos del traspaso correctamente
```

**Caso 2: Escáner sin sesión**
```
1. Cerrar sesión en la app
2. Navegar a la pantalla de escáner de QR
3. Escanear el QR del PDF
4. App detecta el ID del traspaso
5. Valida sesión → ❌ No hay token
6. Muestra alerta "Iniciar Sesión"
7. Usuario toca "Ir a Login"
8. Ingresa credenciales → Login exitoso
9. App procesa ID pendiente guardado
10. Navega automáticamente a pantalla de recepción con ese ID
```

**Caso 3: QR inválido**
```
1. Escanear un QR que no sea de traspasos
2. App muestra alerta "QR Inválido"
3. No navega, permite escanear otro código
```

**Diferencia entre flujos:**
- **Escáner externo (cámara nativa):** Sale de la app → Navegador → Deep link → Vuelve a app
- **Escáner interno (en la app):** Nunca sale de la app → Extrae ID → Navega directo

**Ventaja del escáner interno:** Mucho más rápido y mejor UX.

### Troubleshooting

**Problema: "Deep link no abre la app"**

Solución:
```bash
# Verificar que Expo está corriendo
npx expo start --lan

# Verificar logs de Expo
# Debe mostrar: "Deep link opened: exp://..."
```

**Problema: "Error al cargar traspaso (404)"**

Solución:
- Verifica que `API_BASE_URL` es correcta
- Verifica que el token de autenticación está configurado
- Checa logs en Metro bundler

**Problema: "Navegación no funciona"**

Solución:
- Verifica que instalaste `react-navigation` y sus dependencias
- Verifica que el nombre de la pantalla coincide: `WarehouseTransferReceive`
- Checa logs en consola de Expo Go

**Problema: "App se cierra al presionar 'Abrir'"**

Solución:
- Reinicia Expo Go
- Limpia caché: `npx expo start --clear`
- Verifica que la pantalla está correctamente registrada en el navigator

**Problema: "Escáner de QR no funciona / Pantalla negra"**

Solución:
```bash
# Verificar permisos de cámara
# Android: Configuración > Apps > Tu App > Permisos > Cámara
# iOS: Configuración > Privacidad > Cámara > Tu App

# Reinstalar dependencias
npx expo install expo-barcode-scanner expo-camera

# Verificar que solicitaste permisos en tu código
await Camera.requestCameraPermissionsAsync();
```

**Problema: "QR escaneado pero no navega"**

Solución:
- Verifica logs en consola: `[QR Scanner] ID del traspaso: XXX`
- Verifica que el regex coincide: `/warehouse-transfers\/(\d+)\/receive/`
- Verifica que el nombre de la pantalla es correcto: `WarehouseTransferReceive`
- Checa que AsyncStorage funciona correctamente

**Problema: "Después de login no procesa el ID pendiente"**

Solución:
- Verifica que guardas el ID: `await AsyncStorage.setItem('pendingTransferId', transferId);`
- Verifica que lo lees después del login: `await AsyncStorage.getItem('pendingTransferId');`
- Verifica que lo eliminas después de procesarlo: `await AsyncStorage.removeItem('pendingTransferId');`
- Checa logs en consola de Metro bundler

---

## NOTAS IMPORTANTES

### URLs de Deep Linking

**En desarrollo (Expo Go):**
```
exp://192.168.192.57:8081/--/warehouse-transfers/123/receive
```

**En producción (build standalone - futuro):**
```
easypos://warehouse-transfers/123/receive
```

### Configuración de Red

- **Expo Metro:** `192.168.192.57:8081`
- **Backend API:** Configura la URL correcta de tu backend
- **Celular y PC:** Deben estar en la misma red WiFi

### Flujos Duales: Escáner Externo vs Interno

**Este sistema soporta DOS formas de escanear el QR:**

#### 1. Escáner Externo (Cámara Nativa del Celular)
```
Usuario escanea QR con cámara → Abre navegador
→ Landing page detecta móvil → Redirige a exp://...
→ Sistema pregunta "Abrir con Expo Go?"
→ App recibe deep link → Valida sesión → Navega
```

**Ventajas:**
- ✅ Funciona sin tener la app abierta
- ✅ Funciona desde cualquier dispositivo (web, móvil, tablet)
- ✅ Ideal para trabajadores externos o nuevos usuarios

**Cuándo usar:** Usuario no tiene la app abierta, escanea desde WhatsApp, etc.

#### 2. Escáner Interno (Integrado en la App)
```
Usuario abre pantalla de escáner en la app
→ Escanea QR → App extrae ID directamente
→ Valida sesión → Navega (todo dentro de la app)
```

**Ventajas:**
- ✅ Mucho más rápido (no sale de la app)
- ✅ Mejor UX (sin redirecciones)
- ✅ Funciona offline para extraer el ID
- ✅ Más profesional y nativo

**Cuándo usar:** Usuario ya tiene la app abierta y usa el escáner integrado.

#### El Mismo QR Funciona para Ambos

**Importante:** No necesitas generar QRs diferentes. El mismo QR (`http://192.168.0.72:3000/warehouse-transfers/123/receive`) funciona para:
- ✅ Escáner externo → Procesa la URL completa como deep link
- ✅ Escáner interno → Extrae el ID con regex

**Recomendación:** Implementa AMBOS flujos para máxima flexibilidad.

### Dependencias Necesarias

**Obligatorias (deep linking externo):**

```bash
npx expo install expo-linking
npx expo install @react-navigation/native
npx expo install @react-navigation/native-stack
npx expo install react-native-screens react-native-safe-area-context
npx expo install @react-native-async-storage/async-storage
```

**Opcionales (escáner interno de QR - TAREA 5):**

```bash
npx expo install expo-barcode-scanner
npx expo install expo-camera
```

### Autenticación

Asegúrate de implementar:
- ✅ Almacenamiento del token en AsyncStorage
- ✅ Interceptor de axios para agregar token automáticamente
- ✅ Manejo de errores 401 (token expirado)
- ✅ **Validación de sesión antes de procesar deep links**
- ✅ **Guardar deep link pendiente si no hay sesión**
- ✅ **Procesar deep link pendiente después de login exitoso**

### Validaciones

La pantalla debe validar:
- ✅ Traspaso existe (no 404)
- ✅ Estado permite recibir (`Dispatched` o `PartiallyReceived`)
- ✅ Hay productos pendientes
- ✅ Cantidades no exceden pendiente
- ✅ Al menos una cantidad > 0

---

## PRÓXIMOS PASOS

Después de completar esta implementación:

### En Desarrollo:
1. ✅ Probar deep linking externo con Expo Go (cámara nativa)
2. ✅ Verificar flujo completo: QR → Landing → Expo → Recepción
3. ✅ Probar registro de entradas
4. ✅ **(OPCIONAL) Probar escáner interno si implementaste TAREA 5**
5. ✅ **(OPCIONAL) Probar flujo: Escáner → Sin sesión → Login → Procesa ID pendiente**

### Para Producción (Futuro):
1. 🔜 Hacer build standalone: `eas build -p android` / `eas build -p ios`
2. 🔜 Configurar `AndroidManifest.xml` y `Info.plist` para custom scheme `easypos://`
3. 🔜 Cambiar backend para generar `easypos://` en vez de `http://` (en producción)
4. 🔜 Solicitar permisos de cámara en tiempo de instalación
5. 🔜 Publicar en Play Store / App Store

### Recomendaciones para Producción:
- ✅ Mantén ambos flujos (externo e interno) para máxima flexibilidad
- ✅ El escáner interno mejora significativamente la UX
- ✅ Deep linking externo es backup para usuarios que escanean desde otras apps
- ✅ El mismo QR funciona para ambos, no necesitas duplicar nada

---

## PREGUNTAS FRECUENTES

**P: ¿Por qué usar `exp://` en desarrollo?**

R: Porque Expo Go no soporta custom schemes (`easypos://`). Solo funcionan en builds standalone.

**P: ¿Cómo obtengo el token de autenticación?**

R: Debes tener un sistema de login que guarde el token en AsyncStorage. Consulta tu implementación actual de login.

**P: ¿Puedo probar sin escanear el QR?**

R: Sí, usa los comandos `adb` o `xcrun` para abrir el deep link directamente desde terminal.

**P: ¿Necesito modificar el backend?**

R: NO, el backend ya está completamente implementado.

**P: ¿Qué pasa si no tengo React Navigation?**

R: Usa el código alternativo (TAREA 2, opción manual) y adapta a tu sistema de navegación.

**P: ¿Debo implementar el escáner interno de QR (TAREA 5)?**

R: Es **opcional** pero **muy recomendado**. Beneficios:
   - Mucho más rápido que el flujo externo
   - Mejor experiencia de usuario
   - El mismo QR funciona para ambos flujos
   - Puedes tener ambos activos simultáneamente

**P: ¿Necesito generar QRs diferentes para escáner interno vs externo?**

R: NO. El mismo QR funciona para ambos. El flujo externo procesa la URL completa como deep link, el flujo interno extrae el ID con regex.

**P: ¿Cómo sabe la app si viene del escáner interno o externo?**

R: No necesita saberlo. Ambos flujos terminan en la misma pantalla (`WarehouseTransferReceive`) con el mismo parámetro (`id`). La diferencia está en el camino para llegar ahí.

**P: ¿Qué pasa si escaneo con el escáner interno sin sesión?**

R: Igual que en el deep link externo: guarda el ID en AsyncStorage, redirige a Login, y después de login exitoso procesa el ID guardado.

---

## EJEMPLO DE DEEP LINK COMPLETO

### Flujo Externo (Cámara Nativa)

```
URL original del QR:
http://192.168.0.72:3000/warehouse-transfers/123/receive

↓ Landing page redirige a:

exp://192.168.192.57:8081/--/warehouse-transfers/123/receive

↓ React Navigation parsea:

{
  screen: "WarehouseTransferReceive",
  params: { id: "123" }
}

↓ Pantalla recibe:

route.params.id = "123"
```

### Flujo Interno (Escáner en la App)

```
QR escaneado:
http://192.168.0.72:3000/warehouse-transfers/123/receive

↓ Regex extrae ID:

const match = qrData.match(/warehouse-transfers\/(\d+)\/receive/);
// match[1] = "123"

↓ Valida sesión y navega:

navigation.navigate('WarehouseTransferReceive', { id: "123" });

↓ Pantalla recibe:

route.params.id = "123"
```

**Resultado:** Ambos flujos llegan al mismo destino, solo cambia el camino.

---

**Por favor implementa estos cambios siguiendo la estructura y convenciones de tu proyecto React Native.**
