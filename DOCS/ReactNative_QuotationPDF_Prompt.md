# Prompt — PDF de Cotización en React Native (Mobile)

## CONTEXTO

Tienes una app móvil en **React Native** que ya tiene el módulo de Cotizaciones implementado con las siguientes pantallas:
- `QuotationListScreen` — Lista de cotizaciones
- `NewQuotationScreen` — Nueva cotización
- `QuotationDetailScreen` — Detalle de cotización
- `QRScannerScreen` — Escanear QR

## STACK ASUMIDO

- **React Native** (CLI o Expo — indica cuál usas)
- **Navigation:** React Navigation v6
- **HTTP:** `fetch` o `axios` con Bearer token
- **Notificaciones:** `react-native-toast-message` o similar
- **Estilos:** StyleSheet nativo o `styled-components/native`

Si usas **Expo**, las librerías de manejo de archivos son distintas. Se indica cuándo aplica cada caso.

---

## NUEVO ENDPOINT DISPONIBLE

```
GET /api/quotations/{id}/pdf
Authorization: Bearer {token}
```

**Respuesta:** archivo binario `application/pdf` con nombre `cotizacion-{id}.pdf`

El PDF incluye:
- Header con datos de la empresa y logo
- Código de cotización grande (ej: `COT-000007`) con badge de estado
- Tabla de productos detallada
- Panel de totales (Subtotal, Descuento, IVA, Total)
- **Código QR** impreso en el PDF — al escanearse convierte la cotización en venta
- Notas si las hay

---

## DEPENDENCIAS A INSTALAR

### React Native CLI

```bash
npm install react-native-blob-util
# iOS
cd ios && pod install
```

### Expo

```bash
npx expo install expo-file-system expo-sharing expo-print
```

---

## LO QUE NECESITAS IMPLEMENTAR

### 1. Servicio — agregar `getPdf` a `quotationService.js`

```javascript
// src/services/api/quotationService.js

/**
 * Descarga el PDF de una cotización como base64.
 * Retorna: { base64: string, filename: string }
 */
async getPdf(id) {
  const token = await getToken(); // tu función para obtener el token JWT

  const response = await fetch(`${API_BASE_URL}/quotations/${id}/pdf`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Error al obtener el PDF');
  }

  // Convertir a base64 para React Native
  const arrayBuffer = await response.arrayBuffer();
  const bytes = new Uint8Array(arrayBuffer);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  const base64 = btoa(binary);

  return {
    base64,
    filename: `cotizacion-${id}.pdf`,
    mimeType: 'application/pdf',
  };
}
```

---

### 2. Hook `useQuotationPdf.js`

#### Versión React Native CLI (`react-native-blob-util`)

```javascript
// src/hooks/useQuotationPdf.js
import { useState } from 'react';
import ReactNativeBlobUtil from 'react-native-blob-util';
import { quotationService } from '../services/api/quotationService';
import Toast from 'react-native-toast-message';

export function useQuotationPdf() {
  const [loading, setLoading] = useState(false);

  /**
   * Abre el PDF con el visor del sistema operativo.
   */
  const openPdf = async (id) => {
    setLoading(true);
    try {
      const { base64, filename } = await quotationService.getPdf(id);

      const dirs = ReactNativeBlobUtil.fs.dirs;
      const path = `${dirs.CacheDir}/${filename}`;

      await ReactNativeBlobUtil.fs.writeFile(path, base64, 'base64');

      await ReactNativeBlobUtil.android
        ? ReactNativeBlobUtil.android.actionViewIntent(path, 'application/pdf')
        : ReactNativeBlobUtil.ios.openDocument(path);
    } catch (err) {
      Toast.show({ type: 'error', text1: 'Error', text2: err.message || 'No se pudo abrir el PDF' });
    } finally {
      setLoading(false);
    }
  };

  /**
   * Guarda el PDF en la carpeta de Descargas del dispositivo.
   */
  const downloadPdf = async (id, code) => {
    setLoading(true);
    try {
      const { base64, filename } = await quotationService.getPdf(id);
      const name = `${code || filename}`;

      const dirs = ReactNativeBlobUtil.fs.dirs;
      const path = `${dirs.DownloadDir}/${name}`;

      await ReactNativeBlobUtil.fs.writeFile(path, base64, 'base64');

      // Notificar al sistema de archivos de Android para que aparezca en Descargas
      if (ReactNativeBlobUtil.android) {
        ReactNativeBlobUtil.android.addCompleteDownload({
          title: name,
          description: 'Cotización EasyPOS',
          mime: 'application/pdf',
          path,
          showNotification: true,
        });
      }

      Toast.show({ type: 'success', text1: 'Descargado', text2: `Guardado como ${name}` });
    } catch (err) {
      Toast.show({ type: 'error', text1: 'Error', text2: err.message || 'No se pudo descargar el PDF' });
    } finally {
      setLoading(false);
    }
  };

  return { openPdf, downloadPdf, loading };
}
```

#### Versión Expo (`expo-file-system` + `expo-sharing`)

```javascript
// src/hooks/useQuotationPdf.js — versión Expo
import { useState } from 'react';
import * as FileSystem from 'expo-file-system';
import * as Sharing from 'expo-sharing';
import { quotationService } from '../services/api/quotationService';
import Toast from 'react-native-toast-message';

export function useQuotationPdf() {
  const [loading, setLoading] = useState(false);

  const openPdf = async (id) => {
    setLoading(true);
    try {
      const { base64, filename } = await quotationService.getPdf(id);
      const uri = `${FileSystem.cacheDirectory}${filename}`;

      await FileSystem.writeAsStringAsync(uri, base64, {
        encoding: FileSystem.EncodingType.Base64,
      });

      const canShare = await Sharing.isAvailableAsync();
      if (canShare) {
        await Sharing.shareAsync(uri, {
          mimeType: 'application/pdf',
          dialogTitle: `Cotización ${filename}`,
          UTI: 'com.adobe.pdf',
        });
      }
    } catch (err) {
      Toast.show({ type: 'error', text1: 'Error', text2: err.message || 'No se pudo abrir el PDF' });
    } finally {
      setLoading(false);
    }
  };

  // En Expo, openPdf y downloadPdf son esencialmente la misma operación
  // ya que el Sheet de compartir permite tanto abrir como guardar
  const downloadPdf = openPdf;

  return { openPdf, downloadPdf, loading };
}
```

---

### 3. Componente `QuotationPdfButtons.jsx`

```jsx
// src/components/quotations/QuotationPdfButtons.jsx
import React from 'react';
import {
  View,
  TouchableOpacity,
  Text,
  ActivityIndicator,
  StyleSheet,
} from 'react-native';
import { useQuotationPdf } from '../../hooks/useQuotationPdf';

const PRIMARY = '#1a3c6e';

/**
 * @param {number} id    - ID de la cotización
 * @param {string} code  - Código (ej: COT-000007)
 * @param {'row'|'column'} [direction] - Dirección del layout (default: 'row')
 */
export function QuotationPdfButtons({ id, code, direction = 'row' }) {
  const { openPdf, downloadPdf, loading } = useQuotationPdf();

  return (
    <View style={[styles.container, { flexDirection: direction }]}>
      <TouchableOpacity
        style={[styles.button, styles.outlined, loading && styles.disabled]}
        onPress={() => openPdf(id)}
        disabled={loading}
        activeOpacity={0.75}
      >
        {loading ? (
          <ActivityIndicator size="small" color={PRIMARY} />
        ) : (
          <Text style={styles.outlinedText}>📄 Ver PDF</Text>
        )}
      </TouchableOpacity>

      <TouchableOpacity
        style={[styles.button, styles.filled, loading && styles.disabled]}
        onPress={() => downloadPdf(id, code)}
        disabled={loading}
        activeOpacity={0.75}
      >
        <Text style={styles.filledText}>⬇️ Descargar</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    gap: 8,
    flexWrap: 'wrap',
  },
  button: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 8,
    alignItems: 'center',
    justifyContent: 'center',
    minWidth: 120,
  },
  outlined: {
    borderWidth: 1.5,
    borderColor: PRIMARY,
    backgroundColor: '#fff',
  },
  filled: {
    backgroundColor: PRIMARY,
  },
  outlinedText: {
    color: PRIMARY,
    fontWeight: '600',
    fontSize: 14,
  },
  filledText: {
    color: '#fff',
    fontWeight: '600',
    fontSize: 14,
  },
  disabled: {
    opacity: 0.5,
  },
});
```

---

## DÓNDE AGREGAR LOS BOTONES

### A) En `QuotationDetailScreen.jsx`

```jsx
import { QuotationPdfButtons } from '../../components/quotations/QuotationPdfButtons';

// Dentro del JSX, debajo del resumen de la cotización:
<View style={styles.actionsContainer}>
  <QuotationPdfButtons id={quotation.id} code={quotation.code} />

  {quotation.status === 'Draft' && (
    <View style={styles.draftActions}>
      <TouchableOpacity style={styles.cancelBtn} onPress={handleCancel}>
        <Text style={styles.cancelText}>🗑️ Cancelar Cotización</Text>
      </TouchableOpacity>
      <TouchableOpacity style={styles.convertBtn} onPress={handleConvert}>
        <Text style={styles.convertText}>✅ Convertir a Venta</Text>
      </TouchableOpacity>
    </View>
  )}
</View>
```

### B) En `QuotationListScreen.jsx` — botón por cada item de la lista

```jsx
import { useQuotationPdf } from '../../hooks/useQuotationPdf';
import { TouchableOpacity, Text } from 'react-native';

// Dentro del componente de la pantalla:
const { openPdf, loading: pdfLoading } = useQuotationPdf();

// En el renderItem del FlatList:
const renderItem = ({ item }) => (
  <View style={styles.card}>
    {/* ...datos de la cotización... */}

    <View style={styles.cardActions}>
      <TouchableOpacity
        onPress={() => navigation.navigate('QuotationDetail', { id: item.id })}
        style={styles.viewBtn}
      >
        <Text style={styles.viewBtnText}>Ver detalle</Text>
      </TouchableOpacity>

      <TouchableOpacity
        onPress={() => openPdf(item.id)}
        disabled={pdfLoading}
        style={styles.pdfBtn}
      >
        <Text style={styles.pdfBtnText}>📄 PDF</Text>
      </TouchableOpacity>
    </View>
  </View>
);
```

### C) Modal de éxito al crear cotización (`QuotationSuccessModal.jsx`)

```jsx
import { QuotationPdfButtons } from '../quotations/QuotationPdfButtons';
import { Modal, View, Text, TouchableOpacity, StyleSheet } from 'react-native';

export function QuotationSuccessModal({ visible, quotation, onClose }) {
  if (!quotation) return null;

  return (
    <Modal visible={visible} transparent animationType="fade">
      <View style={styles.overlay}>
        <View style={styles.modal}>
          <Text style={styles.title}>✅ Cotización Creada</Text>
          <Text style={styles.code}>{quotation.code}</Text>
          <Text style={styles.subtitle}>
            Comparte o descarga el PDF con el código QR
            para convertirla en venta.
          </Text>

          {/* QR visual generado por el backend está en el PDF.
              Opcionalmente renderiza aquí el QR con react-native-qrcode-svg: */}
          {/* <QRCode value={quotation.code} size={160} /> */}

          <View style={styles.buttons}>
            <QuotationPdfButtons
              id={quotation.id}
              code={quotation.code}
              direction="column"
            />
            <TouchableOpacity style={styles.closeBtn} onPress={onClose}>
              <Text style={styles.closeBtnText}>Cerrar</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: 24,
  },
  modal: {
    backgroundColor: '#fff',
    borderRadius: 16,
    padding: 28,
    width: '100%',
    alignItems: 'center',
    gap: 12,
  },
  title: {
    fontSize: 20,
    fontWeight: '700',
    color: '#1a3c6e',
  },
  code: {
    fontSize: 26,
    fontWeight: '800',
    color: '#1a3c6e',
    letterSpacing: 1,
  },
  subtitle: {
    fontSize: 13,
    color: '#5b6472',
    textAlign: 'center',
    lineHeight: 20,
  },
  buttons: {
    width: '100%',
    gap: 10,
    marginTop: 8,
  },
  closeBtn: {
    paddingVertical: 10,
    alignItems: 'center',
  },
  closeBtnText: {
    color: '#5b6472',
    fontSize: 14,
  },
});
```

---

## PERMISOS REQUERIDOS

### Android — `AndroidManifest.xml`

```xml
<!-- Necesario para escribir en Descargas (Android < 10) -->
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE"
    android:maxSdkVersion="28" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE"
    android:maxSdkVersion="32" />
```

Solicita el permiso en tiempo de ejecución antes de descargar:

```javascript
import { PermissionsAndroid, Platform } from 'react-native';

async function requestStoragePermission() {
  if (Platform.OS !== 'android' || Platform.Version >= 29) return true;
  const granted = await PermissionsAndroid.request(
    PermissionsAndroid.PERMISSIONS.WRITE_EXTERNAL_STORAGE
  );
  return granted === PermissionsAndroid.RESULTS.GRANTED;
}
```

### iOS — `Info.plist`

No se requieren permisos especiales para escribir en el directorio temporal y compartir con `UIActivityViewController` (que es lo que `expo-sharing` y `react-native-blob-util` usan internamente).

---

## RESUMEN DE ARCHIVOS A CREAR/MODIFICAR

| Acción    | Archivo                                                  |
|-----------|----------------------------------------------------------|
| Crear     | `src/hooks/useQuotationPdf.js`                           |
| Crear     | `src/components/quotations/QuotationPdfButtons.jsx`      |
| Crear     | `src/components/quotations/QuotationSuccessModal.jsx`    |
| Modificar | `src/services/api/quotationService.js`                   |
| Modificar | `src/screens/Quotations/QuotationDetailScreen.jsx`       |
| Modificar | `src/screens/Quotations/QuotationListScreen.jsx`         |
| Modificar | `android/app/src/main/AndroidManifest.xml` *(si CLI)*    |

---

## NOTAS FINALES

- El PDF ya contiene el **código QR impreso** generado por el backend — no necesitas generar el QR en la app para imprimir, solo para mostrar en pantalla si lo deseas
- Para mostrar el QR visualmente en el modal de éxito instala: `npm install react-native-qrcode-svg react-native-svg`
- En Expo usa `expo-sharing` — NO uses `react-native-blob-util` ya que no es compatible con el managed workflow
- En React Native CLI prefiere `react-native-blob-util` sobre `rn-fetch-blob` (está abandonado)
