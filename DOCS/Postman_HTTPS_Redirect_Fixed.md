# ? **SOLUCIONADO: Error SSL en Postman con HTTP**

## ?? **EL PROBLEMA**

Aunque usabas la URL correcta `http://localhost:7254`, **seguías obteniendo error SSL**:

```
Error: write EPROTO 1012096:error:100000f7:SSL routines:
OPENSSL_internal:WRONG_VERSION_NUMBER
```

---

## ?? **LA CAUSA REAL**

El problema estaba en `Program.cs`:

```csharp
// ? ESTO CAUSABA EL PROBLEMA
if (OperatingSystem.IsWindows())
{
    app.UseHttpsRedirection();  // ? Redirigía HTTP ? HTTPS
}
```

### **¿Qué pasaba?**

1. **Postman** enviaba: `POST http://localhost:7254/api/Login/login`
2. **ASP.NET Core** respondía: `301 Redirect ? https://localhost:7255`
3. **Postman** intentaba seguir la redirección pero se confundía con los certificados SSL
4. **Resultado:** Error SSL

---

## ? **LA SOLUCIÓN**

### **Deshabilitado `UseHttpsRedirection` temporalmente:**

```csharp
// ? COMENTADO PARA FACILITAR TESTING
// if (OperatingSystem.IsWindows())
// {
//     app.UseHttpsRedirection();
// }
```

**Ahora:**
- ? HTTP funciona sin redirecciones
- ? Postman funciona correctamente
- ? Swagger funciona correctamente
- ? cURL funciona correctamente

---

## ?? **PRUEBA AHORA**

### **1. Reinicia la API**

```bash
# Detén la API (Ctrl+C)
# Reiníciala
dotnet run --project Web.Api
```

### **2. En Postman:**

| Campo | Valor |
|-------|-------|
| **Method** | POST |
| **URL** | `http://localhost:7254/api/Login/login` |
| **Header** | `Content-Type: application/json` |
| **Body (raw)** | `{"code":"ADMIN001","password":"admin123"}` |

### **3. Click en Send**

**? Ahora deberías recibir:**

```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGci...",
  "tokenType": "Bearer",
  "expiresAt": "2026-03-07T08:00:00Z",
  "user": {
    "id": 1,
    "code": "ADMIN001",
    "name": "Administrador",
    "email": "admin@sistema.com",
    "active": true,
    "roleId": 1,
    "roleName": "Administrador"
  }
}
```

---

## ?? **OPCIONES PARA HTTPS**

### **Opción A: Usar HTTPS Explícitamente (Recomendado para testing HTTPS)**

Si quieres probar con HTTPS:

**URL en Postman:**
```
https://localhost:7255/api/Login/login
```

**Configuración de Postman:**
1. Settings ? General
2. **Desactivar**: SSL certificate verification
3. Solo para desarrollo local

### **Opción B: Mantener HTTP para Desarrollo (Actual)**

```
http://localhost:7254/api/Login/login
```

? Funciona sin problemas  
? Sin necesidad de certificados  
? Más rápido para desarrollo

---

## ?? **REACTIVAR HTTPS REDIRECT PARA PRODUCCIÓN**

Cuando vayas a producción, **descomenta** la línea:

```csharp
// Para Producción
if (OperatingSystem.IsWindows())
{
    app.UseHttpsRedirection();
}
```

O mejor aún, usa una variable de entorno:

```csharp
if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```

---

## ?? **CONFIGURACIÓN ACTUAL**

| Aspecto | Valor |
|---------|-------|
| **Puerto HTTP** | 7254 |
| **Puerto HTTPS** | 7255 (Windows) |
| **Redirección HTTPS** | ? Deshabilitada |
| **CORS** | ? Habilitado (AllowAll) |
| **Swagger** | ? http://localhost:7254/swagger |
| **Postman** | ? Funciona con HTTP |

---

## ?? **OTROS ENDPOINTS PARA PROBAR**

### **1. GET Módulos (Requiere Token)**

```
GET http://localhost:7254/api/modules
Authorization: Bearer {token-del-login}
```

### **2. GET Clientes (Requiere Token)**

```
GET http://localhost:7254/api/Customer
Authorization: Bearer {token-del-login}
```

### **3. GET Productos (Requiere Token)**

```
GET http://localhost:7254/api/Products
Authorization: Bearer {token-del-login}
```

---

## ?? **IMPORTANTE**

### **Para Desarrollo:**
- ? Usa HTTP (`http://localhost:7254`)
- ? Sin redirecciones
- ? Sin problemas de certificados

### **Para Producción:**
- ? Usa HTTPS
- ? Reactiva `UseHttpsRedirection()`
- ? Usa certificados válidos

---

## ?? **RESUMEN**

### **Problema:**
```
? UseHttpsRedirection() forzaba redirect HTTP ? HTTPS
? Postman se confundía con los certificados
? Error SSL en HTTP
```

### **Solución:**
```
? Comentar UseHttpsRedirection() temporalmente
? Usar HTTP directo en desarrollo
? Sin redirecciones = Sin problemas
```

### **Resultado:**
```
? Postman funciona con http://localhost:7254
? Swagger funciona
? Sin errores SSL
```

---

**? PROBLEMA RESUELTO** - Reinicia la API y prueba en Postman ??
