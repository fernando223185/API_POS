# ?? **SOLUCIÓN: Error SSL en Postman**

## ?? **PROBLEMA**

Al intentar hacer una petición en Postman, obtienes:

```
Error: write EPROTO 1012096:error:100000f7:SSL routines:
OPENSSL_internal:WRONG_VERSION_NUMBER
```

---

## ?? **CAUSA DEL ERROR**

Este error ocurre cuando intentas hacer una petición **HTTPS** a un servidor que solo acepta **HTTP** (o viceversa).

### **Tu Configuración Actual:**

Según `Program.cs`, la API escucha en:

- ? **Puerto 7254:** HTTP (todas las plataformas)
- ? **Puerto 7255:** HTTPS (solo Windows con certificado de desarrollo)

---

## ? **SOLUCIÓN**

### **Cambia la URL en Postman de HTTPS a HTTP:**

**? INCORRECTO (causa el error):**
```
https://localhost:7254/api/Login/login
```

**? CORRECTO:**
```
http://localhost:7254/api/Login/login
```

---

## ?? **CONFIGURACIÓN COMPLETA EN POSTMAN**

### **Pestaña: Request**

| Campo | Valor |
|-------|-------|
| **Method** | `POST` |
| **URL** | `http://localhost:7254/api/Login/login` |

### **Pestaña: Headers**

| Key | Value |
|-----|-------|
| `Content-Type` | `application/json` |

### **Pestaña: Body**

Selecciona: **raw** ? **JSON**

```json
{
  "code": "ADMIN001",
  "password": "admin123"
}
```

---

## ? **RESPUESTA ESPERADA**

Si todo está correcto, deberías recibir:

**Status:** `200 OK`

**Body:**
```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
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

## ?? **USAR HTTPS (Solo Windows)**

Si estás en **Windows** y quieres usar HTTPS:

### **URL con HTTPS:**
```
https://localhost:7255/api/Login/login
```

**Nota:** Usa el puerto **7255** para HTTPS.

### **Advertencia de Certificado:**

Postman mostrará una advertencia sobre el certificado autofirmado.

**Solución temporal en Postman:**
1. Settings (??) ? General
2. Desactiva: **SSL certificate verification**

?? Solo para desarrollo local.

---

## ?? **TABLA DE PUERTOS**

| Puerto | Protocolo | Plataforma | URL Ejemplo |
|--------|-----------|------------|-------------|
| **7254** | HTTP | ? Windows / Linux / macOS | `http://localhost:7254` |
| **7255** | HTTPS | ? Solo Windows | `https://localhost:7255` |

---

## ?? **PRUEBA CON cURL**

Si Postman sigue dando problemas, prueba con cURL:

### **Windows (PowerShell):**
```powershell
Invoke-RestMethod -Uri "http://localhost:7254/api/Login/login" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"code":"ADMIN001","password":"admin123"}'
```

### **Linux/macOS (Terminal):**
```bash
curl -X POST http://localhost:7254/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{"code":"ADMIN001","password":"admin123"}'
```

---

## ?? **OTROS ENDPOINTS PARA PROBAR**

### **1. Validar Token:**

**URL:** `http://localhost:7254/api/Login/validate-token`  
**Method:** `POST`  
**Headers:**
```
Authorization: Bearer {tu-token-aqui}
```

### **2. Obtener Módulos:**

**URL:** `http://localhost:7254/api/modules`  
**Method:** `GET`  
**Headers:**
```
Authorization: Bearer {tu-token-aqui}
```

### **3. Obtener Clientes:**

**URL:** `http://localhost:7254/api/Customer`  
**Method:** `GET`  
**Headers:**
```
Authorization: Bearer {tu-token-aqui}
```

---

## ?? **ERRORES COMUNES**

### **1. Error: `Failed to fetch`**
**Causa:** API no está corriendo  
**Solución:** 
```bash
dotnet run --project Web.Api
```

### **2. Error: `401 Unauthorized`**
**Causa:** Token inválido o ausente  
**Solución:** Primero haz login para obtener un token válido

### **3. Error: `Cannot reach this page`**
**Causa:** Puerto incorrecto o firewall  
**Solución:** Verifica que usas el puerto `7254` para HTTP

---

## ?? **COLECCIÓN DE POSTMAN**

Puedes importar esta colección en Postman:

```json
{
  "info": {
    "name": "ERP POS API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Login",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"code\": \"ADMIN001\",\n  \"password\": \"admin123\"\n}"
        },
        "url": {
          "raw": "http://localhost:7254/api/Login/login",
          "protocol": "http",
          "host": ["localhost"],
          "port": "7254",
          "path": ["api", "Login", "login"]
        }
      }
    },
    {
      "name": "Get Modules",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "url": {
          "raw": "http://localhost:7254/api/modules",
          "protocol": "http",
          "host": ["localhost"],
          "port": "7254",
          "path": ["api", "modules"]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "token",
      "value": ""
    }
  ]
}
```

**Cómo importar:**
1. Postman ? Import
2. Pega el JSON arriba
3. Click en Import
4. Usa la colección

---

## ? **RESUMEN**

### **Problema:**
- ? Intentabas usar `https://` en puerto `7254`
- ? Error SSL porque el puerto solo acepta HTTP

### **Solución:**
- ? Usa `http://localhost:7254` para HTTP
- ? O usa `https://localhost:7255` si estás en Windows

### **Recuerda:**
- ?? Puerto **7254** = HTTP
- ?? Puerto **7255** = HTTPS (solo Windows)

---

**? PROBLEMA RESUELTO** - Cambia HTTPS por HTTP en Postman ??
