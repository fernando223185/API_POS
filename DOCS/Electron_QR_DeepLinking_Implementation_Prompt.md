# Prompt: Implementar QR Deep Linking para Warehouse Transfers - Electron + React

## CONTEXTO DEL PROYECTO

### Stack Tecnológico
- **Framework:** Electron + React 18
- **Bundler:** Create React App (react-scripts)
- **Dev Server:** webpack-dev-server
- **Puerto:** 3000
- **IP de red local:** `192.168.0.72` (obtener con `ipconfig` si cambió)
- **UI Components:** Material-UI (MUI) v5
- **Routing:** React Router v6
- **HTTP Client:** axios con Bearer token
- **Notificaciones:** react-hot-toast

### Arquitectura
```
Expanda (Electron App)
├── webpack-dev-server → http://192.168.0.72:3000
├── App React corre dentro de Electron
└── Backend API separado (ya implementado)
```

### Backend (Ya Implementado ✅)
- **Endpoint:** `POST /api/reports/generate`
- **Acepta:** `GenerateReportDto` con campo `appBaseUrl`
- **Genera:** PDF con QR code automáticamente
- **QR contiene:** URL que se pasa en `appBaseUrl`

---

## OBJETIVO

Implementar funcionalidad de QR con deep linking para el módulo de **Traspasos de Almacén**:

1. Usuario genera PDF de despacho desde Electron
2. PDF contiene QR con URL: `http://192.168.0.72:3000/warehouse-transfers/{id}/receive`
3. Trabajador escanea QR con cámara del celular
4. Se abre landing page que detecta plataforma
5. Redirige a app móvil (Expo Go) o versión web según el caso

### Flujo Visual
```
┌──────────────────────────────────────────────────────┐
│  1. Electron: Usuario genera PDF                    │
│     POST /api/reports/generate                       │
│     { appBaseUrl: "http://192.168.0.72:3000" }       │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  2. Backend: Genera PDF con QR                      │
│     QR = "http://192.168.0.72:3000/warehouse-transfers/123/receive" │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  3. Celular: Escanea QR con cámara nativa           │
│     Chrome/Safari abre la URL                        │
└──────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────────┐
│  4. Landing Page: Detecta plataforma                │
│     - Electron → /receive-form                       │
│     - Móvil → exp://192.168.192.57:8081/...         │
│     - Desktop web → /receive-form                    │
└──────────────────────────────────────────────────────┘
```

---

## TAREAS A IMPLEMENTAR

### ✅ TAREA 1: Modificar generación de PDF

**Archivo:** Buscar el componente de detalle de traspaso que tiene el botón "Imprimir Despacho"

Probablemente se llama algo como:
- `WarehouseTransferDetailPage.jsx`
- `WarehouseTransferDetail.jsx`
- O similar en `src/modules/warehouse-transfers/pages/`

**Cambio requerido:**

Buscar la función que genera el PDF (probablemente algo como `handlePrintDispatch` o `handlePrint`):

```javascript
// ❌ ANTES (código actual, probablemente similar a esto)
const handlePrintDispatch = async () => {
  try {
    const response = await reportApi.generatePdf({
      reportType: 'WarehouseTransferDispatch',
      documentIds: [Number(id)],
      // Sin appBaseUrl
    });
    
    const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
    window.open(url, '_blank');
  } catch (err) {
    toast.error('Error al generar el PDF');
  }
};
```

```javascript
// ✅ DESPUÉS (agregar appBaseUrl)
const handlePrintDispatch = async () => {
  try {
    const response = await reportApi.generatePdf({
      reportType: 'WarehouseTransferDispatch',
      documentIds: [Number(id)],
      appBaseUrl: 'http://192.168.0.72:3000', // ← AGREGAR ESTA LÍNEA
    });
    
    const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
    window.open(url, '_blank');
  } catch (err) {
    toast.error('Error al generar el PDF');
  }
};
```

**Nota:** Si prefieres usar variable de entorno, crear archivo `.env`:
```env
REACT_APP_BASE_URL=http://192.168.0.72:3000
```

Y usar:
```javascript
appBaseUrl: process.env.REACT_APP_BASE_URL,
```

---

### ✅ TAREA 2: Crear Landing Page

**Archivo nuevo:** `src/modules/warehouse-transfers/pages/WarehouseTransferReceiveLanding.jsx`

**Propósito:** Detectar desde dónde se escanea el QR y redirigir apropiadamente.

**Código completo:**

```jsx
import { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, CircularProgress, Typography } from '@mui/material';

/**
 * Landing page para QR de registro de entrada de traspasos.
 * Detecta la plataforma (Electron, móvil, web) y redirige automáticamente.
 */
export default function WarehouseTransferReceiveLanding() {
  const { id } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    const userAgent = navigator.userAgent;
    
    // Detectar plataforma
    const isElectron = userAgent.includes('Electron') || window?.electron !== undefined;
    const isMobile = /Android|iPhone|iPad|iPod/i.test(userAgent);

    console.log('[QR Landing] Plataforma detectada:', {
      isElectron,
      isMobile,
      userAgent,
      url: window.location.href
    });

    if (isElectron) {
      // Dentro de Electron → navega a pantalla de formulario
      console.log('[QR Landing] → Redirigiendo a formulario (Electron)');
      navigate(`/warehouse-transfers/${id}/receive-form`, { replace: true });
      
    } else if (isMobile) {
      // Dispositivo móvil → intenta abrir Expo Go
      const expoUrl = `exp://192.168.192.57:8081/--/warehouse-transfers/${id}/receive`;
      console.log('[QR Landing] → Intentando abrir Expo Go:', expoUrl);
      
      // Redirige a Expo Go
      window.location.href = expoUrl;
      
      // ✅ Mostrar mensaje temporal (da tiempo a la app para validar sesión)
      setTimeout(() => {
        document.body.innerHTML = `
          <div style="
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            padding: 40px 20px;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background-color: #f5f5f5;
            text-align: center;
          ">
            <div style="
              background-color: white;
              padding: 32px;
              border-radius: 12px;
              box-shadow: 0 2px 8px rgba(0,0,0,0.1);
              max-width: 400px;
            ">
              <h2 style="color: #1a3c6e; margin-bottom: 16px;">
                Abriendo aplicación...
              </h2>
              <p style="color: #666; margin-bottom: 24px;">
                Traspaso #${id}
              </p>
              <p style="color: #888; font-size: 14px; margin-bottom: 16px;">
                Si la app no se abre automáticamente:
              </p>
              <a 
                href="${expoUrl}" 
                style="
                  display: inline-block;
                  padding: 12px 24px;
                  background-color: #1a3c6e;
                  color: white;
                  text-decoration: none;
                  border-radius: 6px;
                  font-weight: 500;
                "
              >
                Toca aquí para abrir
              </a>
            </div>
          </div>
        `;
      }, 500);
      
      // Fallback a web si Expo Go no está instalado (después de 5 segundos)
      setTimeout(() => {
        console.log('[QR Landing] → Timeout, redirigiendo a versión web');
        navigate(`/warehouse-transfers/${id}/receive-form`, { replace: true });
      }, 5000);
      
    } else {
      // Desktop en navegador web → versión web
      console.log('[QR Landing] → Navegador desktop, usando versión web');
      navigate(`/warehouse-transfers/${id}/receive-form`, { replace: true });
    }
  }, [id, navigate]);

  // Pantalla de carga mientras detecta plataforma
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        gap: 2,
        backgroundColor: '#f5f5f5',
      }}
    >
      <CircularProgress size={48} />
      <Typography variant="body1" color="text.secondary">
        Abriendo aplicación...
      </Typography>
      <Typography variant="caption" color="text.secondary">
        Traspaso #{id}
      </Typography>
    </Box>
  );
}
```

**Ubicación sugerida:** `src/modules/warehouse-transfers/pages/WarehouseTransferReceiveLanding.jsx`

---

### ✅ TAREA 3: Agregar Ruta en React Router

**Archivo:** Buscar el archivo de rutas de la aplicación (probablemente `src/App.jsx` o `src/router.jsx`)

**Cambio:**

```jsx
import WarehouseTransferReceiveLanding from './modules/warehouse-transfers/pages/WarehouseTransferReceiveLanding';

// Dentro de <Routes>
<Routes>
  {/* ... otras rutas existentes ... */}
  
  {/* Nueva ruta para landing page del QR */}
  <Route 
    path="/warehouse-transfers/:id/receive" 
    element={<WarehouseTransferReceiveLanding />} 
  />
  
  {/* ... otras rutas ... */}
</Routes>
```

---

### ✅ TAREA 4 (OPCIONAL): Crear Pantalla de Formulario Standalone

**Solo si quieres versión web completa del formulario de entrada (no solo modal).**

**Archivo nuevo:** `src/modules/warehouse-transfers/pages/WarehouseTransferReceiveFormPage.jsx`

**Ruta:** `/warehouse-transfers/:id/receive-form`

**Contenido:** Mismo que el modal `ReceivingModal` pero como página completa con:
- Botón "Volver" en el header
- Card contenedor
- Toda la funcionalidad del modal (fecha, notas, productos, cantidades)
- Hook `useWarehouseTransferDetail` para obtener datos
- Validaciones y submit

**Puedes reutilizar componentes:**
- Si ya tienes `ReceivingModal.jsx`, puedes extraer la lógica a un hook compartido
- O convertir el modal en página con props opcionales

**Si no quieres crear página standalone:**
- La landing page puede abrir el modal existente en vez de navegar a `/receive-form`
- Pero ten en cuenta que el modal necesita estar dentro de una página que cargue el contexto del traspaso

---

## VERIFICACIÓN Y PRUEBAS

### Checklist de Implementación

- [ ] Modificada función de generación de PDF con `appBaseUrl`
- [ ] Creado componente `WarehouseTransferReceiveLanding.jsx`
- [ ] **✅ IMPORTANTE: Implementado mensaje temporal en landing page (500ms)**
- [ ] **✅ IMPORTANTE: Implementado timeout de fallback a web (5s)**
- [ ] Agregada ruta `/warehouse-transfers/:id/receive` en React Router
- [ ] Importado componente en archivo de rutas
- [ ] (Opcional) Creada página de formulario `/receive-form`

### Cómo Probar

#### 1. Generar PDF
```
1. Abrir Electron
2. Ir a detalle de un traspaso en estado Dispatched
3. Presionar botón "Imprimir Despacho" (o similar)
4. Verificar que el PDF se genera correctamente
```

#### 2. Verificar QR
```
1. Abrir el PDF generado
2. Buscar el código QR
3. Debajo del QR debe aparecer la URL:
   http://192.168.0.72:3000/warehouse-transfers/123/receive
4. Si dice "localhost" → ❌ Mal configurado
5. Si dice "192.168.x.x" → ✅ Correcto
```

#### 3. Probar Landing Page en Electron
```
1. En la barra de direcciones de Electron, navegar manualmente a:
   /warehouse-transfers/123/receive
2. Debe redirigir automáticamente a /receive-form (o abrir modal)
3. Ver logs en consola: "[QR Landing] → Redirigiendo a formulario (Electron)"
```

#### 4. Probar Landing Page en Navegador Desktop
```
1. Abrir Chrome/Edge en tu PC
2. Ir a: http://192.168.0.72:3000/warehouse-transfers/123/receive
3. Debe cargar landing page y redirigir a formulario web
4. Ver logs en consola
```

#### 5. Probar con Celular (requiere React Native configurado)

**Caso 1: Usuario CON sesión activa en la app móvil**
```
1. Celular y PC en mismo WiFi
2. Tener Expo Go instalado y app corriendo con sesión iniciada
3. Escanear QR con cámara del celular
4. Se abre navegador → Landing page
5. Landing page redirige a exp://... (ver mensaje temporal)
6. Sistema pregunta "Abrir con Expo Go?"
7. Expo Go se abre
8. App valida sesión → ✅ Hay token
9. Navega directamente a pantalla de recepción
```

**Caso 2: Usuario SIN sesión en la app móvil**
```
1. Escanear QR con cámara del celular
2. Se abre navegador → Landing page
3. Landing page redirige a exp://... (ver mensaje temporal)
4. Expo Go se abre
5. App valida sesión → ❌ No hay token
6. App guarda deep link pendiente
7. App redirige a pantalla de Login
8. Usuario ingresa credenciales → Login exitoso
9. App procesa deep link guardado
10. Navega a pantalla de recepción con ID del traspaso
```

**Caso 3: Expo Go no instalado**
```
1. Escanear QR con cámara del celular
2. Se abre navegador → Landing page
3. Landing page intenta abrir Expo Go
4. Sistema no encuentra app (no hay "Abrir con")
5. Después de 5 segundos → Fallback a versión web
6. Se muestra formulario de recepción en navegador móvil
```

### Troubleshooting

**Problema: "Landing page no redirige"**
- Verifica que React Router está configurado correctamente
- Verifica que `useNavigate` funciona en otros componentes
- Checa logs en consola del navegador

**Problema: "QR tiene localhost en vez de IP"**
- Verifica que agregaste `appBaseUrl: 'http://192.168.0.72:3000'`
- Verifica tu IP actual con `ipconfig` (puede haber cambiado)
- Regenera el PDF después de hacer el cambio

**Problema: "Celular no puede acceder"**
- Verifica que celular y PC están en la misma red WiFi
- Desconecta VPN si está activa
- Verifica firewall de Windows no bloquee puerto 3000
- Prueba abrir `http://192.168.0.72:3000` en navegador del celular

---

## NOTAS IMPORTANTES

### Variables de Red

- **IP Local:** `192.168.0.72` - Obtener ejecutando `ipconfig` en PowerShell
  - Buscar "Wireless LAN adapter Wi-Fi" → "IPv4 Address"
  - NO usar IP de VPN (192.168.192.x)
  - NO usar localhost (solo funciona en la misma PC)

- **Expo Metro:** `192.168.192.57:8081` - IP donde corre Expo en desarrollo
  - Verificar con: `$env:REACT_NATIVE_PACKAGER_HOSTNAME="192.168.192.57"; npx expo start --lan`

### URLs Importantes

```
Electron Dev Server:    http://192.168.0.72:3000
Landing Page:           http://192.168.0.72:3000/warehouse-transfers/123/receive
Formulario:             http://192.168.0.72:3000/warehouse-transfers/123/receive-form
Expo Deep Link:         exp://192.168.192.57:8081/--/warehouse-transfers/123/receive
```

### Backend

- **Endpoint:** `POST /api/reports/generate`
- **Campos del DTO:**
  ```json
  {
    "reportType": "WarehouseTransferDispatch",
    "documentIds": [123],
    "appBaseUrl": "http://192.168.0.72:3000"
  }
  ```
- **Respuesta:** Blob (PDF file)

### Convenciones del Proyecto

- Usar hooks de Material-UI para estilos (`sx` prop)
- Usar `toast.error()` y `toast.success()` para notificaciones
- Imports absolutos o relativos según configuración del proyecto
- Logs con prefijo `[QR Landing]` para facilitar debugging

### Coordinación con React Native

**⚠️ IMPORTANTE: La landing page debe dar tiempo a la app móvil para validar sesión**

La landing page actualizada:
1. ✅ Redirige a `exp://...` inmediatamente
2. ✅ Muestra mensaje temporal con link manual (500ms después)
3. ✅ Espera 5 segundos antes de hacer fallback a web

Esto permite que la app móvil:
- Reciba el deep link
- Valide si hay sesión activa (token en AsyncStorage)
- Si NO hay sesión → Guarde el deep link y redirija a Login
- Si SÍ hay sesión → Navegue directo a la pantalla de recepción
- Después del login → Procese el deep link guardado

**Flujo temporal:**
```
0.0s → window.location.href = "exp://..."
0.5s → Muestra mensaje "Abriendo aplicación..."
2.0s → Usuario toca "Abrir con Expo Go" (si está disponible)
2.1s → App valida sesión y decide qué hacer
5.0s → Timeout: si no abrió Expo, fallback a web
```

---

## PRÓXIMOS PASOS

Después de completar esta implementación en Electron + React:

1. ✅ Probar generación de PDF con QR correcto
2. ✅ Verificar landing page funciona en Electron
3. ✅ Verificar landing page funciona en navegador web
4. ✅ Verificar mensaje temporal se muestra correctamente (500ms después de redirección)
5. 🔜 Implementar React Native con validación de sesión (ver prompt separado)
6. 🔜 Probar flujo completo: QR → Celular → Expo Go → Validación sesión
7. 🔜 Probar casos: con sesión, sin sesión, Expo no instalado

### Coordinación Electron ↔ React Native

**Electron (Landing Page):**
- Detecta móvil
- Redirige a `exp://...`
- Muestra mensaje temporal
- Espera 5s antes de fallback

**React Native (App Móvil):**
- Recibe deep link
- Valida sesión en AsyncStorage
- Si NO hay token → Login primero
- Si SÍ hay token → Directo a recepción
- Ver prompt: `ReactNative_QR_DeepLinking_Implementation_Prompt.md`

---

## PREGUNTAS FRECUENTES

**P: ¿Por qué no usar custom scheme `easypos://` directamente?**

R: Porque estamos usando Expo Go para desarrollo, que no soporta custom schemes. En producción (con build standalone), SÍ usaremos `easypos://`.

**P: ¿La landing page es necesaria?**

R: Sí, cumple dos funciones críticas:
   1. Detecta la plataforma y redirige apropiadamente
   2. Da tiempo a la app móvil para validar sesión antes de procesar el deep link

**P: ¿Por qué esperar 5 segundos antes del fallback?**

R: Para dar tiempo a que:
   - El sistema operativo muestre "Abrir con Expo Go?"
   - El usuario toque "Abrir"
   - La app móvil valide sesión y redirija a Login si es necesario

**P: ¿Puedo usar localhost en vez de la IP?**

R: NO. `localhost` en el celular apunta al propio celular, no a tu PC. Debes usar la IP de red local.

**P: ¿Necesito configurar algo en el backend?**

R: NO, el backend ya está completamente implementado y funcional.

**P: ¿Qué pasa si el usuario no tiene sesión en la app móvil?**

R: La app móvil debe:
   1. Detectar que no hay token en AsyncStorage
   2. Guardar el deep link (el ID del traspaso)
   3. Redirigir a pantalla de Login
   4. Después de login exitoso, procesar el link guardado
   (Ver prompt de React Native para implementación completa)

---

**Por favor implementa estos cambios siguiendo la estructura y convenciones del proyecto Expanda.**
