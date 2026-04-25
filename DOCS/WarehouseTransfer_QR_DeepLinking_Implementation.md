# Implementación de Deep Linking para QR de Traspasos de Almacén

**Objetivo:** Permitir que un mismo QR funcione tanto en la app de Electron (escritorio) como en la app de React Native (móvil), redirigiendo al usuario a la pantalla de registro de entrada de mercancía.

**Estrategia:** Landing Page HTTP con detección automática de plataforma.

---

## PARTE 1: Electron + React (Desktop)

### Paso 1: Agregar nueva ruta en tu router

**Archivo:** `src/router.jsx` o `src/App.jsx` (donde defines tus rutas)

```jsx
import WarehouseTransferReceiveLanding from './modules/warehouse-transfers/pages/WarehouseTransferReceiveLanding';

// En tus rutas
<Routes>
  {/* ... otras rutas ... */}
  
  {/* Ruta landing page para QR (detección automática) */}
  <Route 
    path="/warehouse-transfers/:id/receive" 
    element={<WarehouseTransferReceiveLanding />} 
  />
  
  {/* Ruta del formulario de entrada (versión completa) */}
  <Route 
    path="/warehouse-transfers/:id/receive-form" 
    element={<WarehouseTransferReceiveFormPage />} 
  />
  
  {/* ... otras rutas ... */}
</Routes>
```

---

### Paso 2: Crear componente Landing Page

**Archivo:** `src/modules/warehouse-transfers/pages/WarehouseTransferReceiveLanding.jsx`

```jsx
import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, CircularProgress, Typography } from '@mui/material';

/**
 * Landing page para QR de registro de entrada.
 * Detecta la plataforma y redirige automáticamente.
 */
export default function WarehouseTransferReceiveLanding() {
  const { id } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    const userAgent = navigator.userAgent;
    
    // Detectar si estamos en Electron
    const isElectron = userAgent.includes('Electron') || window?.electron !== undefined;
    
    // Detectar si es un dispositivo móvil
    const isMobile = /Android|iPhone|iPad|iPod/i.test(userAgent);

    console.log('[QR Landing] Plataforma detectada:', { isElectron, isMobile, userAgent });

    if (isElectron) {
      // Ya estamos en la app de Electron, simplemente navega a la pantalla de entrada
      console.log('[QR Landing] → Redirigiendo a pantalla de entrada (Electron)');
      navigate(`/warehouse-transfers/${id}/receive-form`, { replace: true });
      
    } else if (isMobile) {
      // Dispositivo móvil: intenta abrir la app nativa con deep link
      console.log('[QR Landing] → Intentando abrir app móvil');
      const deepLink = `easypos://warehouse-transfers/${id}/receive`;
      
      // Intenta abrir la app
      window.location.href = deepLink;
      
      // Si después de 800ms no se abrió la app, asume que no está instalada y usa web
      setTimeout(() => {
        console.log('[QR Landing] → App no detectada, usando versión web');
        navigate(`/warehouse-transfers/${id}/receive-form`, { replace: true });
      }, 800);
      
    } else {
      // Desktop en navegador web → usa la versión web
      console.log('[QR Landing] → Navegador web, usando versión web');
      navigate(`/warehouse-transfers/${id}/receive-form`, { replace: true });
    }
  }, [id, navigate]);

  // Pantalla de carga mientras se detecta la plataforma
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        gap: 2,
      }}
    >
      <CircularProgress size={48} />
      <Typography variant="body1" color="text.secondary">
        Redirigiendo...
      </Typography>
    </Box>
  );
}
```

---

### Paso 3: Crear pantalla de formulario de entrada

**Opción A: Convertir tu modal en página completa**

**Archivo:** `src/modules/warehouse-transfers/pages/WarehouseTransferReceiveFormPage.jsx`

```jsx
import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box, Card, CardContent, Typography, Button, TextField,
  Table, TableHead, TableRow, TableCell, TableBody,
  FormControlLabel, Switch, Alert, CircularProgress,
} from '@mui/material';
import { ArrowBack, MoveToInbox } from '@mui/icons-material';
import { useWarehouseTransferDetail } from '../hooks/useWarehouseTransferDetail';
import toast from 'react-hot-toast';

export default function WarehouseTransferReceiveFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { transfer, loading, createReceiving } = useWarehouseTransferDetail(id);

  const [receivingDate, setReceivingDate] = useState(new Date().toISOString().split('T')[0]);
  const [notes, setNotes] = useState('');
  const [receiveAll, setReceiveAll] = useState(false);
  const [quantities, setQuantities] = useState({});
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const pendingDetails = transfer?.details?.filter((d) => d.pendingQuantity > 0) ?? [];

  useEffect(() => {
    if (transfer) {
      const initial = {};
      pendingDetails.forEach((d) => {
        initial[d.id] = receiveAll ? d.pendingQuantity : 0;
      });
      setQuantities(initial);
    }
  }, [transfer, receiveAll]);

  const handleQuantityChange = (detailId, value, max) => {
    const parsed = Math.max(0, Math.min(Number(value) || 0, max));
    setQuantities((prev) => ({ ...prev, [detailId]: parsed }));
    setReceiveAll(false);
  };

  const handleSubmit = async () => {
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

    setSubmitting(true);
    try {
      await createReceiving({
        receivingDate: new Date(receivingDate).toISOString(),
        notes,
        details,
      });
      toast.success('Entrada registrada correctamente');
      navigate(`/warehouse-transfers/${id}`);
    } catch (err) {
      toast.error(err?.response?.data?.message || 'Error al registrar entrada');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!transfer) {
    return (
      <Box sx={{ p: 3 }}>
        <Typography>Traspaso no encontrado</Typography>
        <Button onClick={() => navigate('/warehouse-transfers')} sx={{ mt: 2 }}>
          Volver al listado
        </Button>
      </Box>
    );
  }

  if (transfer.status !== 'Dispatched' && transfer.status !== 'PartiallyReceived') {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning" sx={{ mb: 2 }}>
          Este traspaso no permite registrar entradas (Estado actual: {transfer.status})
        </Alert>
        <Button onClick={() => navigate(`/warehouse-transfers/${id}`)} startIcon={<ArrowBack />}>
          Ver detalle
        </Button>
      </Box>
    );
  }

  if (pendingDetails.length === 0) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="success" sx={{ mb: 2 }}>
          ¡Todo recibido! No hay productos pendientes.
        </Alert>
        <Button onClick={() => navigate(`/warehouse-transfers/${id}`)} startIcon={<ArrowBack />}>
          Ver detalle
        </Button>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3, maxWidth: 900, mx: 'auto' }}>
      {/* Header */}
      <Box sx={{ mb: 3 }}>
        <Button
          startIcon={<ArrowBack />}
          onClick={() => navigate(`/warehouse-transfers/${id}`)}
          sx={{ mb: 2 }}
        >
          Volver
        </Button>
        <Typography variant="h5" fontWeight="bold">
          Registrar Entrada de Mercancía — {transfer.code}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Almacén destino: <strong>{transfer.destinationWarehouseName}</strong>
        </Typography>
      </Box>

      <Card>
        <CardContent>
          {/* Fecha + Notas */}
          <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
            <TextField
              label="Fecha de recepción"
              type="date"
              value={receivingDate}
              onChange={(e) => setReceivingDate(e.target.value)}
              InputLabelProps={{ shrink: true }}
              size="small"
              sx={{ flex: '1 1 200px' }}
            />
            <TextField
              label="Notas (opcional)"
              size="small"
              fullWidth
              multiline
              rows={2}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              sx={{ flex: '1 1 100%' }}
            />
          </Box>

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

          {/* Botones */}
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 3 }}>
            <Button
              variant="outlined"
              onClick={() => navigate(`/warehouse-transfers/${id}`)}
              disabled={submitting}
            >
              Cancelar
            </Button>
            <Button
              variant="contained"
              startIcon={submitting ? <CircularProgress size={16} /> : <MoveToInbox />}
              onClick={handleSubmit}
              disabled={submitting}
            >
              Confirmar Entrada
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
```

---

### Paso 4: Actualizar exports (index.js)

Si usas un archivo de exports centralizado:

```javascript
// src/modules/warehouse-transfers/index.js
export { default as WarehouseTransferListPage } from './pages/WarehouseTransferListPage';
export { default as WarehouseTransferDetailPage } from './pages/WarehouseTransferDetailPage';
export { default as WarehouseTransferFormPage } from './pages/WarehouseTransferFormPage';
export { default as WarehouseTransferReceiveLanding } from './pages/WarehouseTransferReceiveLanding';
export { default as WarehouseTransferReceiveFormPage } from './pages/WarehouseTransferReceiveFormPage';
```

---

### Paso 5: Configurar generación de PDF con URL correcta

Cuando generes el PDF de despacho, asegúrate de pasar el `appBaseUrl` correcto:

**⚠️ IMPORTANTE:** NO uses `window.location.origin` en desarrollo porque devuelve `http://localhost:3000`, y **localhost NO funciona desde celulares**. Usa la **IP de tu red local**.

```javascript
const handlePrintDispatch = async () => {
  try {
    // ❌ NO HACER ESTO en desarrollo:
    // const appBaseUrl = window.location.origin; // Devuelve http://localhost:3000
    
    // ✅ USAR IP DE RED LOCAL:
    const appBaseUrl = 'http://192.168.192.57:3000'; // Cambia por TU IP local
    
    // O MEJOR: Configurar en variable de entorno
    const appBaseUrl = import.meta.env.VITE_APP_BASE_URL || 'http://192.168.192.57:3000';
    
    const response = await reportApi.generatePdf({
      reportType: 'WarehouseTransferDispatch',
      documentIds: [Number(id)],
      appBaseUrl, // ← El QR apuntará a {appBaseUrl}/warehouse-transfers/{id}/receive
    });
    
    const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
    window.open(url, '_blank');
  } catch (err) {
    toast.error('Error al generar el PDF');
  }
};
```

**¿Cómo obtener tu IP local?**

**Windows (PowerShell):**
```powershell
ipconfig
# Busca "IPv4 Address" en "Wireless LAN adapter Wi-Fi" o "Ethernet adapter"
# Ejemplo: 192.168.1.100
```

**Linux/Mac:**
```bash
ifconfig | grep inet
# o
ip addr show
```

**Recomendación para producción:**

Crea un archivo `.env` en la raíz de tu proyecto:

```env
# .env.development
VITE_APP_BASE_URL=http://192.168.192.57:3000

# .env.production
VITE_APP_BASE_URL=https://tudominio.com
```

Luego en tu código:

```javascript
const appBaseUrl = import.meta.env.VITE_APP_BASE_URL;
```

---

## PARTE 2: React Native (Móvil)

### Paso 1: Instalar dependencias

```bash
npm install react-native-deep-linking
# o
yarn add react-native-deep-linking
```

Si usas Expo:
```bash
npx expo install expo-linking
```

---

### Paso 2: Configurar deep linking en Android

**Archivo:** `android/app/src/main/AndroidManifest.xml`

Dentro de la etiqueta `<activity>` de tu MainActivity, agrega:

```xml
<activity
  android:name=".MainActivity"
  android:label="@string/app_name"
  android:configChanges="keyboard|keyboardHidden|orientation|screenSize|uiMode"
  android:launchMode="singleTask"
  android:windowSoftInputMode="adjustResize">
  
  <intent-filter>
    <action android:name="android.intent.action.MAIN" />
    <category android:name="android.intent.category.LAUNCHER" />
  </intent-filter>
  
  <!-- Deep linking para esquema easypos:// -->
  <intent-filter android:label="EasyPOS Deep Link">
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="easypos" />
  </intent-filter>
  
</activity>
```

---

### Paso 3: Configurar deep linking en iOS

**Archivo:** `ios/EasyPOS/Info.plist`

Agrega dentro del `<dict>` principal:

```xml
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleTypeRole</key>
    <string>Editor</string>
    <key>CFBundleURLName</key>
    <string>com.easypos.app</string>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>easypos</string>
    </array>
  </dict>
</array>
```

---

### Paso 4: Configurar listener de deep links en tu app

**Archivo:** `App.tsx` o `App.jsx` (raíz de tu app React Native)

#### Opción A: Con React Navigation (recomendado)

```jsx
import React, { useEffect } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { Linking } from 'react-native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';

const Stack = createNativeStackNavigator();

// Configuración de linking para React Navigation
const linking = {
  prefixes: ['easypos://', 'https://tu-dominio.com'],
  config: {
    screens: {
      WarehouseTransferReceive: {
        path: 'warehouse-transfers/:id/receive',
        parse: {
          id: (id) => `${id}`, // Asegurar que sea string
        },
      },
      // ... otras rutas ...
    },
  },
};

export default function App() {
  useEffect(() => {
    // Listener para deep links cuando la app está cerrada
    Linking.getInitialURL().then((url) => {
      if (url) {
        console.log('[Deep Link] App abierta desde URL:', url);
        // React Navigation maneja automáticamente la navegación
      }
    });

    // Listener para deep links cuando la app está en background
    const subscription = Linking.addEventListener('url', ({ url }) => {
      console.log('[Deep Link] URL recibida:', url);
      // React Navigation maneja automáticamente la navegación
    });

    return () => subscription.remove();
  }, []);

  return (
    <NavigationContainer linking={linking}>
      <Stack.Navigator>
        {/* ... tus pantallas ... */}
        <Stack.Screen 
          name="WarehouseTransferReceive" 
          component={WarehouseTransferReceiveScreen} 
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
```

#### Opción B: Sin React Navigation (manual)

```jsx
import React, { useEffect } from 'react';
import { Linking, Alert } from 'react-native';

export default function App() {
  useEffect(() => {
    // Handler para procesar deep links
    const handleDeepLink = (url) => {
      console.log('[Deep Link] URL recibida:', url);
      
      // Ejemplo: easypos://warehouse-transfers/123/receive
      const match = url.match(/warehouse-transfers\/(\d+)\/receive/);
      
      if (match) {
        const transferId = match[1];
        console.log('[Deep Link] Navegando a traspaso ID:', transferId);
        
        // Aquí navega a tu pantalla de entrada usando tu sistema de navegación
        // navigation.navigate('WarehouseTransferReceive', { id: transferId });
      }
    };

    // Listener para cuando la app está cerrada
    Linking.getInitialURL()
      .then((url) => {
        if (url) {
          handleDeepLink(url);
        }
      })
      .catch((err) => console.error('[Deep Link] Error:', err));

    // Listener para cuando la app está en background
    const subscription = Linking.addEventListener('url', ({ url }) => {
      handleDeepLink(url);
    });

    return () => subscription.remove();
  }, []);

  return (
    // Tu app normal
  );
}
```

---

### Paso 5: Crear pantalla de recepción en React Native

**Archivo:** `src/screens/WarehouseTransferReceiveScreen.jsx`

```jsx
import React, { useState, useEffect } from 'react';
import {
  View, Text, ScrollView, TextInput, TouchableOpacity,
  Switch, ActivityIndicator, Alert, StyleSheet,
} from 'react-native';
import { useRoute, useNavigation } from '@react-navigation/native';
import { warehouseTransferApi } from '../api/warehouseTransferApi';

export default function WarehouseTransferReceiveScreen() {
  const route = useRoute();
  const navigation = useNavigation();
  const { id } = route.params;

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [transfer, setTransfer] = useState(null);
  const [receivingDate, setReceivingDate] = useState(new Date().toISOString().split('T')[0]);
  const [notes, setNotes] = useState('');
  const [receiveAll, setReceiveAll] = useState(false);
  const [quantities, setQuantities] = useState({});

  const pendingDetails = transfer?.details?.filter((d) => d.pendingQuantity > 0) ?? [];

  useEffect(() => {
    fetchTransfer();
  }, [id]);

  useEffect(() => {
    if (transfer && receiveAll) {
      const newQty = {};
      pendingDetails.forEach((d) => { newQty[d.id] = d.pendingQuantity; });
      setQuantities(newQty);
    }
  }, [receiveAll, transfer]);

  const fetchTransfer = async () => {
    setLoading(true);
    try {
      const { data } = await warehouseTransferApi.getById(id);
      setTransfer(data);
      
      // Inicializar cantidades en 0
      const initial = {};
      data.details.forEach((d) => { initial[d.id] = 0; });
      setQuantities(initial);
    } catch (err) {
      Alert.alert('Error', err?.response?.data?.message || 'No se pudo cargar el traspaso');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
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
      await warehouseTransferApi.createReceiving(id, {
        receivingDate: new Date(receivingDate).toISOString(),
        notes,
        details,
      });
      
      Alert.alert('Éxito', 'Entrada registrada correctamente', [
        { text: 'OK', onPress: () => navigation.goBack() },
      ]);
    } catch (err) {
      Alert.alert('Error', err?.response?.data?.message || 'Error al registrar entrada');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color="#1a3c6e" />
      </View>
    );
  }

  if (!transfer) {
    return (
      <View style={styles.centered}>
        <Text>Traspaso no encontrado</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container}>
      <Text style={styles.title}>Registrar Entrada — {transfer.code}</Text>
      <Text style={styles.subtitle}>Destino: {transfer.destinationWarehouseName}</Text>

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
        <Text>Recibir todo lo pendiente</Text>
        <Switch value={receiveAll} onValueChange={setReceiveAll} />
      </View>

      {/* Lista de productos */}
      {pendingDetails.map((d) => (
        <View key={d.id} style={styles.productCard}>
          <Text style={styles.productCode}>{d.productCode}</Text>
          <Text style={styles.productName}>{d.productName}</Text>
          <Text style={styles.pending}>Pendiente: {d.pendingQuantity}</Text>
          
          <TextInput
            style={styles.quantityInput}
            keyboardType="numeric"
            placeholder="Cantidad a recibir"
            value={String(quantities[d.id] ?? 0)}
            onChangeText={(val) => {
              const parsed = Math.max(0, Math.min(Number(val) || 0, d.pendingQuantity));
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
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 16, backgroundColor: '#f5f5f5' },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  title: { fontSize: 20, fontWeight: 'bold', marginBottom: 8 },
  subtitle: { fontSize: 14, color: '#666', marginBottom: 16 },
  input: { backgroundColor: '#fff', padding: 12, borderRadius: 8, marginBottom: 12, borderWidth: 1, borderColor: '#ddd' },
  textArea: { height: 80, textAlignVertical: 'top' },
  switchRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16, padding: 12, backgroundColor: '#fff', borderRadius: 8 },
  productCard: { backgroundColor: '#fff', padding: 12, borderRadius: 8, marginBottom: 12 },
  productCode: { fontSize: 12, fontWeight: 'bold', color: '#1a3c6e' },
  productName: { fontSize: 14, marginTop: 4 },
  pending: { fontSize: 12, color: '#ff9800', marginTop: 4 },
  quantityInput: { borderWidth: 1, borderColor: '#ddd', borderRadius: 4, padding: 8, marginTop: 8 },
  button: { backgroundColor: '#1a3c6e', padding: 16, borderRadius: 8, alignItems: 'center', marginTop: 16, marginBottom: 32 },
  buttonDisabled: { backgroundColor: '#ccc' },
  buttonText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
});
```

---

### Paso 6: Probar deep linking en React Native

#### En desarrollo (emulador/dispositivo físico):

**Android:**
```bash
adb shell am start -W -a android.intent.action.VIEW -d "easypos://warehouse-transfers/123/receive" com.tuapp
```

**iOS (simulador):**
```bash
xcrun simctl openurl booted "easypos://warehouse-transfers/123/receive"
```

**iOS (dispositivo físico):**
Usa Safari y navega a: `easypos://warehouse-transfers/123/receive`

---

## PARTE 3: Flujo Completo de Uso

### ⚠️ Prerrequisito CRÍTICO: Configurar IP correcta

**ANTES de escanear QR desde celular, verifica:**

1. Tu celular y tu PC deben estar en la **misma red WiFi**
2. El PDF debe generarse con la **IP de red local** (NO `localhost`)
   ```javascript
   // ✅ Correcto:
   appBaseUrl: 'http://192.168.192.57:3000'
   
   // ❌ Incorrecto:
   appBaseUrl: 'http://localhost:3000'  // Solo funciona en la misma PC
   ```

3. Verifica tu IP con `ipconfig` (Windows) o `ifconfig` (Mac/Linux)

4. Asegúrate que el firewall permita conexiones en el puerto 3000

---

### Escenario: Trabajador de almacén escanea QR del PDF

1. **PDF generado contiene QR con URL:**
   ```
   http://192.168.192.57:3000/warehouse-transfers/123/receive
   ```

2. **Trabajador escanea con app móvil:**
   - El navegador abre: `http://192.168.192.57:3000/warehouse-transfers/123/receive`
   - La landing page detecta: `isMobile = true`
   - Intenta abrir: `easypos://warehouse-transfers/123/receive`
   - **Si la app está instalada:** Se abre directamente la pantalla de entrada
   - **Si la app NO está instalada:** Después de 800ms, redirige a la versión web del formulario

3. **Trabajador escanea con tablet/PC con Electron:**
   - El navegador abre: `http://192.168.192.57:3000/warehouse-transfers/123/receive`
   - La landing page detecta: `isElectron = true`
   - Navega directamente: `/warehouse-transfers/123/receive-form`
   - Muestra el formulario dentro de la app de Electron

4. **Trabajador escanea con navegador web (sin apps instaladas):**
   - El navegador abre: `http://192.168.192.57:3000/warehouse-transfers/123/receive`
   - La landing page detecta: `isMobile = false`, `isElectron = false`
   - Navega directamente: `/warehouse-transfers/123/receive-form`
   - Muestra el formulario en el navegador web

---

## PARTE 4: Testing y Debugging

### Logs útiles para debugging:

```javascript
// En la landing page (Electron/React)
console.log('[QR Landing] User Agent:', navigator.userAgent);
console.log('[QR Landing] Is Electron:', navigator.userAgent.includes('Electron'));
console.log('[QR Landing] Is Mobile:', /Android|iPhone|iPad/i.test(navigator.userAgent));
console.log('[QR Landing] Window electron object:', window?.electron);
```

```javascript
// En React Native
console.log('[Deep Link] URL recibida:', url);
console.log('[Deep Link] Transfer ID extraído:', transferId);
```

### Checklist de verificación:

- [ ] Ruta `/warehouse-transfers/:id/receive` existe en React (Electron)
- [ ] Ruta `/warehouse-transfers/:id/receive-form` existe en React (Electron)
- [ ] Componente `WarehouseTransferReceiveLanding.jsx` creado
- [ ] Componente `WarehouseTransferReceiveFormPage.jsx` creado
- [ ] `AndroidManifest.xml` configurado con `<intent-filter>` para `easypos://`
- [ ] `Info.plist` configurado con `CFBundleURLSchemes`
- [ ] Listener de deep links configurado en `App.tsx` (React Native)
- [ ] Pantalla de recepción creada en React Native
- [ ] PDF genera QR con `appBaseUrl` correcto
- [ ] Probado desde dispositivo Android
- [ ] Probado desde dispositivo iOS
- [ ] Probado desde app Electron
- [ ] Probado desde navegador web (fallback)

---

## PARTE 5: Troubleshooting

### Problema: "El QR abre localhost en el celular" ⚠️ MUY COMÚN

**Causa:** El PDF fue generado con `appBaseUrl: "http://localhost:3000"`.

**Por qué pasa:**
- `localhost` significa "esta computadora"
- En tu PC, `localhost` = tu PC ✅
- En tu celular, `localhost` = el celular ❌
- Resultado: El celular busca en sí mismo, no encuentra nada

**Solución:**
1. Abre PowerShell en tu PC y obtén tu IP local:
   ```powershell
   ipconfig
   # Busca "IPv4 Address" → Ejemplo: 192.168.1.100
   ```

2. Cambia el código donde generas el PDF:
   ```javascript
   // ❌ ANTES (mal):
   const appBaseUrl = window.location.origin; // http://localhost:3000
   
   // ✅ AHORA (bien):
   const appBaseUrl = 'http://192.168.1.100:3000'; // Tu IP local
   ```

3. Regenera el PDF y vuelve a escanear el QR

**Verificación rápida:**
- Abre el PDF
- Mira debajo del QR donde dice la URL
- Si dice `localhost` → ❌ Mal configurado
- Si dice `192.168.x.x` → ✅ Bien configurado

---

### Problema: "Deep link no abre la app móvil"

**Causa:** El esquema `easypos://` no está registrado correctamente.

**Solución:**
1. Verifica que `AndroidManifest.xml` tiene el `<intent-filter>` correcto
2. Desinstala y reinstala la app
3. Verifica con: `adb shell dumpsys package com.tuapp | grep -A 5 "android.intent.action.VIEW"`

### Problema: "Landing page no detecta Electron"

**Causa:** El user agent no contiene "Electron".

**Solución:**
1. Verifica en consola: `console.log(navigator.userAgent)`
2. Si no dice "Electron", agrega una variable global:
   ```javascript
   // En tu main.js de Electron
   mainWindow.webContents.executeJavaScript('window.isElectronApp = true;');
   
   // En la landing page
   const isElectron = window.isElectronApp || navigator.userAgent.includes('Electron');
   ```

### Problema: "Timeout de 800ms muy corto en móvil"

**Solución:** Aumenta el timeout a 1200ms si la app tarda en abrir:
```javascript
setTimeout(() => {
  navigate(`/warehouse-transfers/${id}/receive-form`, { replace: true });
}, 1200); // ← Aumentado de 800ms a 1200ms
```

### Problema: "CORS error al cargar desde IP local"

**Solución:** Asegúrate de que tu backend tiene CORS configurado para aceptar tu IP:
```csharp
// En tu Startup.cs o Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalNetwork", policy =>
    {
        policy.WithOrigins("http://192.168.192.57:3000", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

---

## Resumen Final

✅ **Un solo QR** funciona para:
- App móvil Android (React Native)
- App móvil iOS (React Native)
- App de escritorio (Electron)
- Navegador web (fallback)

✅ **Flujo automático** sin intervención del usuario

✅ **Fallback robusto** si la app no está instalada

✅ **Fácil de debuggear** con logs en cada paso

---

**¿Necesitas ayuda con algún paso específico? Házmelo saber.**
