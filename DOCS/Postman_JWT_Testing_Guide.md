# Guía de Testing en Postman - Autenticación JWT

## ?? Objetivo
Probar correctamente los endpoints protegidos con autenticación JWT después de la corrección del error 401.

---

## ?? Paso a Paso

### Paso 1: Hacer Login

**Método:** `POST`  
**URL:** `http://localhost:7254/api/Login/login`  
**Headers:**
```
Content-Type: application/json
```

**Body (raw JSON):**
```json
{
  "code": "ADMIN001",
  "password": "admin123"
}
```

**Response Esperada (200 OK):**
```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxIiwidXNlckNvZGUiOiJBRE1JTjAwMSIsInVzZXJOYW1lIjoiQWRtaW5pc3RyYWRvciIsImVtYWlsIjoiYWRtaW5Ac2lzdGVtYS5jb20iLCJyb2xlSWQiOiIxIiwicm9sZU5hbWUiOiJBZG1pbmlzdHJhZG9yIiwiZXhwIjoxNzQxNzY4OTU1LCJpc3MiOiJZb3VyQVBJIiwiYXVkIjoiWW91ckFQSVVzZXJzIn0.0OWRTYiVCVGCN9_c3hAk5I_PLQ3MiQiUdUfwGBJWJvg",
  "tokenType": "Bearer",
  "expiresAt": "2026-03-11T07:02:35Z",
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

### Paso 2: Copiar el Token

Del response anterior, copia **TODO** el valor del campo `token` (sin comillas).

**Ejemplo:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxIiwidXNlckNvZGUiOiJBRE1JTjAwMSIsInVzZXJOYW1lIjoiQWRtaW5pc3RyYWRvciIsImVtYWlsIjoiYWRtaW5Ac2lzdGVtYS5jb20iLCJyb2xlSWQiOiIxIiwicm9sZU5hbWUiOiJBZG1pbmlzdHJhZG9yIiwiZXhwIjoxNzQxNzY4OTU1LCJpc3MiOiJZb3VyQVBJIiwiYXVkIjoiWW91ckFQSVVzZXJzIn0.0OWRTYiVCVGCN9_c3hAk5I_PLQ3MiQiUdUfwGBJWJvg
```

---

### Paso 3: Configurar Authorization en Postman

#### Opción A: Configuración por Request (Recomendado para testing)

1. Abre la petición que quieres probar (ej: GET Receivings)
2. Ve a la pestaña **"Authorization"**
3. En el dropdown **"Type"**, selecciona **"Bearer Token"**
4. En el campo **"Token"**, pega el token copiado en el Paso 2
5. Postman agregará automáticamente el header: `Authorization: Bearer {token}`

#### Opción B: Configuración a nivel de Colección (Para toda la colección)

1. Clic derecho en tu colección ? **"Edit"**
2. Ve a la pestaña **"Authorization"**
3. Selecciona **Type: "Bearer Token"**
4. Pega el token
5. Marca **"Inherit auth from parent"** en cada request individual

---

### Paso 4: Probar Endpoint Protegido

**Método:** `GET`  
**URL:** `http://localhost:7254/api/PurchaseOrderReceivings`  

**Headers (automáticos desde Authorization):**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Esperada (200 OK):**
```json
{
  "message": "Recepciones obtenidas exitosamente",
  "error": 0,
  "receivings": [],
  "totalReceivings": 0,
  "pendingToPost": 0,
  "posted": 0
}
```

---

## ?? Endpoints para Probar

### 1. Purchase Order Receivings

#### GET - Todas las recepciones
```
GET http://localhost:7254/api/PurchaseOrderReceivings
Authorization: Bearer {token}
```

#### GET - Recepciones paginadas
```
GET http://localhost:7254/api/PurchaseOrderReceivings/paged?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

#### GET - Recepción por ID
```
GET http://localhost:7254/api/PurchaseOrderReceivings/1
Authorization: Bearer {token}
```

#### GET - Recepciones pendientes
```
GET http://localhost:7254/api/PurchaseOrderReceivings/pending-to-post
Authorization: Bearer {token}
```

#### POST - Crear recepción
```
POST http://localhost:7254/api/PurchaseOrderReceivings
Authorization: Bearer {token}
Content-Type: application/json

{
  "purchaseOrderId": 1,
  "warehouseId": 1,
  "receivedDate": "2026-03-10T12:00:00Z",
  "receivedBy": "Juan Pérez",
  "supplierInvoiceNumber": "FACT-001",
  "notes": "Primera recepción de prueba",
  "details": [
    {
      "purchaseOrderDetailId": 1,
      "quantityReceived": 10,
      "quantityAccepted": 8,
      "quantityRejected": 2,
      "notes": "2 unidades dañadas"
    }
  ]
}
```

---

### 2. Purchase Orders

#### GET - Todas las órdenes
```
GET http://localhost:7254/api/PurchaseOrders
Authorization: Bearer {token}
```

#### GET - Órdenes paginadas
```
GET http://localhost:7254/api/PurchaseOrders/paged?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

#### GET - Orden por ID
```
GET http://localhost:7254/api/PurchaseOrders/1
Authorization: Bearer {token}
```

#### POST - Crear orden
```
POST http://localhost:7254/api/PurchaseOrders
Authorization: Bearer {token}
Content-Type: application/json

{
  "supplierId": 1,
  "warehouseId": 1,
  "orderDate": "2026-03-10T12:00:00Z",
  "expectedDeliveryDate": "2026-03-15T12:00:00Z",
  "notes": "Orden de compra de prueba",
  "paymentTerms": "30 días",
  "deliveryTerms": "FOB",
  "details": [
    {
      "productId": 1,
      "quantityOrdered": 100,
      "unitPrice": 25.50,
      "discount": 0,
      "taxPercentage": 16,
      "notes": ""
    }
  ]
}
```

---

### 3. Suppliers

#### GET - Todos los proveedores
```
GET http://localhost:7254/api/Suppliers
Authorization: Bearer {token}
```

#### POST - Crear proveedor
```
POST http://localhost:7254/api/Suppliers
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Proveedor Demo",
  "taxId": "RFC123456789",
  "contactPerson": "Juan García",
  "email": "contacto@proveedor.com",
  "phone": "5551234567",
  "address": "Calle Principal 123",
  "city": "Ciudad de México",
  "state": "CDMX",
  "country": "México",
  "paymentTermsDays": 30
}
```

---

## ? Errores Comunes

### Error 401: "Usuario no autenticado"

**Causa:** No se envió el token o está mal configurado.

**Solución:**
1. Verifica que hayas hecho login primero
2. Verifica que hayas copiado el token completo
3. Verifica que en la pestaña "Authorization" esté seleccionado "Bearer Token"
4. Verifica que no haya espacios al inicio o final del token

---

### Error 401: Token expirado

**Causa:** El token JWT tiene un tiempo de vida de 8 horas.

**Solución:**
1. Vuelve a hacer login (Paso 1)
2. Copia el nuevo token
3. Actualiza el token en las peticiones

---

### Error 403: "Insufficient permissions"

**Causa:** El usuario no tiene permisos para ese recurso.

**Solución:**
1. Verifica que el usuario tenga el rol adecuado
2. Para ADMIN001, debería tener acceso a todos los endpoints
3. Verifica los permisos en la base de datos

---

## ?? Verificar el Token

### Decodificar en JWT.io

1. Ve a [https://jwt.io](https://jwt.io)
2. Pega tu token en el campo "Encoded"
3. Verifica el **Payload**:

```json
{
  "userId": "1",
  "userCode": "ADMIN001",
  "userName": "Administrador",
  "email": "admin@sistema.com",
  "roleId": "1",
  "roleName": "Administrador",
  "exp": 1741768955,
  "iss": "YourAPI",
  "aud": "YourAPIUsers"
}
```

4. Verifica que `exp` (expiration) no esté vencido:
   - Convertir timestamp a fecha: [https://www.epochconverter.com](https://www.epochconverter.com)
   - Si está vencido, haz login nuevamente

---

## ?? Response Codes Esperados

| Código | Significado | Acción |
|--------|-------------|--------|
| **200** | OK | ? Petición exitosa |
| **201** | Created | ? Recurso creado exitosamente |
| **400** | Bad Request | ? Datos inválidos en el body |
| **401** | Unauthorized | ? Token ausente, inválido o expirado |
| **403** | Forbidden | ? Sin permisos para el recurso |
| **404** | Not Found | ? Recurso no encontrado |
| **500** | Server Error | ? Error interno del servidor |

---

## ?? Variables de Entorno en Postman (Opcional)

Para facilitar el testing, puedes usar variables:

### 1. Crear variables de entorno

1. Clic en "Environments" (icono de ojo en la esquina superior derecha)
2. Clic en "Add" para crear nuevo entorno
3. Nombre: "API POS Local"
4. Agregar variables:

```
baseUrl: http://localhost:7254
token: (dejar vacío, se llenará automáticamente)
userId: (dejar vacío)
```

### 2. Script para guardar el token automáticamente

En la petición **POST Login**, ve a la pestaña **"Tests"** y agrega:

```javascript
// Guardar token automáticamente después del login
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.environment.set("token", jsonData.token);
    pm.environment.set("userId", jsonData.user.id);
    console.log("? Token guardado:", jsonData.token.substring(0, 20) + "...");
}
```

### 3. Usar variables en las peticiones

**URL:**
```
{{baseUrl}}/api/PurchaseOrderReceivings
```

**Authorization:**
```
Type: Bearer Token
Token: {{token}}
```

---

## ? Checklist de Testing

- [ ] Login exitoso con ADMIN001
- [ ] Token copiado correctamente
- [ ] Authorization configurada como "Bearer Token"
- [ ] GET /api/PurchaseOrderReceivings ? 200 OK
- [ ] GET /api/PurchaseOrderReceivings/paged ? 200 OK
- [ ] GET /api/PurchaseOrders ? 200 OK
- [ ] GET /api/Suppliers ? 200 OK
- [ ] POST crear proveedor ? 200 OK (si tienes permisos)
- [ ] POST crear orden de compra ? 200 OK (si tienes permisos)

---

## ?? Soporte

Si continúas teniendo problemas:

1. **Verifica los logs de la consola del servidor**
   - Busca mensajes que comiencen con ? o ?
   - Verifica si hay errores de autenticación

2. **Revisa el archivo de documentación**
   - `DOCS/Authentication_401_Error_Fixed.md` - Solución completa del error

3. **Verifica la configuración JWT**
   - `Web.Api/appsettings.json` ? Sección `Jwt`
   - La `Key` debe tener al menos 32 caracteres

4. **Prueba el endpoint de test**
   ```
   GET http://localhost:7254/api/Test/protected
   Authorization: Bearer {token}
   ```

---

## ?? Conceptos Clave

### ¿Qué es un Token JWT?

Un JSON Web Token es un string codificado que contiene:
- **Header:** Tipo de token y algoritmo de firma
- **Payload:** Claims (información del usuario)
- **Signature:** Firma para verificar autenticidad

### ¿Por qué Bearer Token?

"Bearer" significa "portador" en inglés. Cualquiera que tenga el token puede usarlo (como efectivo). Por eso:
- ? Usa HTTPS en producción
- ? No compartas tokens
- ? Tokens tienen tiempo de expiración
- ? Guarda tokens de forma segura

### Claims del Token

Los "claims" son afirmaciones sobre el usuario:
- `userId`: ID del usuario
- `userCode`: Código del usuario
- `userName`: Nombre completo
- `email`: Email del usuario
- `roleId`: ID del rol
- `roleName`: Nombre del rol
- `exp`: Timestamp de expiración
- `iss`: Issuer (quién emitió el token)
- `aud`: Audience (para quién es el token)

---

¡Ahora estás listo para probar todos los endpoints protegidos! ??
