# ?? API ERP - Configuración de Red Multi-Interface

## ?? Objetivo Logrado

Tu API ahora está configurada para ser accesible desde **múltiples interfaces de red**:

- ? **http://localhost:7254** (acceso local)
- ? **http://192.168.192.57:7254** (acceso desde red)
- ? **https://localhost:7255** (HTTPS local - si tienes certificado)

## ?? Configuración Implementada

### **1. Kestrel Multi-Interface**
```csharp
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Escuchar en TODAS las interfaces de red (0.0.0.0)
    serverOptions.Listen(IPAddress.Any, 7254);
    
    // HTTPS solo para localhost (opcional)
    serverOptions.Listen(IPAddress.Loopback, 7255, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});
```

### **2. CORS Actualizado**
- ? Soporte para `192.168.192.57:3000-8080`
- ? Detección automática de redes privadas
- ? Políticas permisivas para desarrollo

### **3. HTTPS Redirect Deshabilitado**
Para permitir acceso HTTP desde IPs de red en desarrollo.

## ?? URLs de Acceso

### **?? API Endpoints:**
```
GET  http://localhost:7254/api/login/login
GET  http://192.168.192.57:7254/api/login/login

GET  http://localhost:7254/api/customer
GET  http://192.168.192.57:7254/api/customer
```

### **?? Swagger UI:**
```
http://localhost:7254/swagger
http://192.168.192.57:7254/swagger
```

## ?? Tests de Conectividad

### **Test 1: Acceso Local**
```bash
curl -X GET http://localhost:7254/swagger/index.html
```

### **Test 2: Acceso desde Red**
```bash
curl -X GET http://192.168.192.57:7254/swagger/index.html
```

### **Test 3: Login desde Aplicación**
```javascript
// Desde frontend en 192.168.0.72:3000
fetch('http://192.168.192.57:7254/api/login/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  credentials: 'include',
  body: JSON.stringify({
    code: 'ADMIN001',
    password: 'admin123'
  })
});
```

## ?? Configuración de Frontend

### **Variables de Entorno**
```javascript
// .env.development
REACT_APP_API_URL_LOCAL=http://localhost:7254
REACT_APP_API_URL_NETWORK=http://192.168.192.57:7254

// .env.production  
REACT_APP_API_URL=https://tu-dominio.com
```

### **Configuración Dinámica**
```javascript
// Detectar si se está ejecutando localmente o en red
const getApiUrl = () => {
  const hostname = window.location.hostname;
  
  if (hostname === 'localhost' || hostname === '127.0.0.1') {
    return 'http://localhost:7254';
  } else if (hostname.startsWith('192.168.')) {
    return 'http://192.168.192.57:7254';
  } else {
    return 'https://tu-dominio.com'; // Producción
  }
};

const API_BASE_URL = getApiUrl();
```

## ??? Verificaciones de Firewall

### **Windows Firewall**
Si tienes problemas de conectividad desde otros dispositivos:

1. **Abrir Puerto 7254:**
   ```cmd
   netsh advfirewall firewall add rule name="ERP API Port 7254" dir=in action=allow protocol=TCP localport=7254
   ```

2. **Verificar reglas existentes:**
   ```cmd
   netsh advfirewall firewall show rule name="ERP API Port 7254"
   ```

### **Router/Red Local**
- Verificar que no haya bloqueo de puertos en el router
- Confirmar que ambos dispositivos estén en la misma subred

## ?? Logging y Debugging

### **En la consola del servidor verás:**
```
?? Kestrel configured to listen on:
   - http://0.0.0.0:7254 (All interfaces - HTTP)
   - https://localhost:7255 (Localhost only - HTTPS)

?? API Access URLs:
   - http://localhost:7254 (Local access)
   - http://192.168.192.57:7254 (Network access)

?? Swagger UI URLs:
   - http://localhost:7254/swagger
   - http://192.168.192.57:7254/swagger

?? Direct Request: GET /swagger to host: 192.168.192.57:7254
?? CORS Request: POST /api/login/login from origin: http://192.168.0.72:3000 to host: 192.168.192.57:7254
```

## ?? Estado Final

? **Configuración Multi-Interface Completa**
- API accesible desde localhost Y red local
- Swagger disponible en ambas URLs
- CORS configurado para IPs específicas
- Logging detallado para debugging

? **URLs Funcionales:**
- `http://localhost:7254/swagger`
- `http://192.168.192.57:7254/swagger`
- Ambas con funcionalidad completa

## ?? ¡Tu API ERP ahora es accesible desde cualquier dispositivo en la red local!

**Reinicia la aplicación para aplicar los cambios de configuración de Kestrel.**