# ?? API de Proveedores - Guía Completa

## ?? BASE URL
```
http://localhost:7254/api/Suppliers
```

---

## ?? ÍNDICE DE ENDPOINTS

1. [Consultas (GET)](#consultas)
   - [Obtener todos los proveedores](#1-obtener-todos-los-proveedores)
   - [Obtener proveedores paginados](#2-obtener-proveedores-paginados)
   - [Obtener proveedor por ID](#3-obtener-proveedor-por-id)
   - [Obtener proveedor por código](#4-obtener-proveedor-por-código)
2. [Crear (POST)](#crear)
   - [Crear proveedor](#5-crear-proveedor)
3. [Actualizar (PUT)](#actualizar)
   - [Actualizar proveedor](#6-actualizar-proveedor)
4. [Eliminar (DELETE)](#eliminar)
   - [Eliminar proveedor](#7-eliminar-proveedor)

---

## ?? CONSULTAS

### 1. Obtener todos los proveedores

**Endpoint:**
```http
GET /api/Suppliers
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Query Parameters:**
| Parámetro | Tipo | Requerido | Default | Descripción |
|-----------|------|-----------|---------|-------------|
| `includeInactive` | boolean | No | false | Incluir proveedores inactivos |

**Ejemplo:**
```http
GET /api/Suppliers?includeInactive=false
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200):**
```json
{
  "message": "Proveedores obtenidos exitosamente",
  "error": 0,
  "suppliers": [
    {
      "id": 1,
      "code": "PROV-001",
      "name": "Proveedor ABC S.A. de C.V.",
      "taxId": "ABC123456789",
      "contactPerson": "Juan Pérez",
      "email": "contacto@proveedor-abc.com",
      "phone": "5555-1234",
      "address": "Av. Principal #123",
      "city": "Ciudad de México",
      "state": "CDMX",
      "zipCode": "01000",
      "country": "México",
      "paymentTermsDays": 30,
      "creditLimit": 100000.00,
      "defaultDiscountPercentage": 5.00,
      "isActive": true,
      "createdAt": "2026-03-10T10:00:00",
      "updatedAt": null,
      "totalPurchaseOrders": 15,
      "totalPurchased": 250000.00
    }
  ],
  "totalSuppliers": 25,
  "activeSuppliers": 23,
  "inactiveSuppliers": 2
}
```

---

### 2. Obtener proveedores paginados

**Endpoint:**
```http
GET /api/Suppliers/paged
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Query Parameters:**
| Parámetro | Tipo | Requerido | Default | Descripción |
|-----------|------|-----------|---------|-------------|
| `pageNumber` | int | No | 1 | Número de página |
| `pageSize` | int | No | 10 | Tamaño de página (máx 100) |
| `includeInactive` | boolean | No | false | Incluir inactivos |
| `searchTerm` | string | No | null | Buscar en nombre, código, RFC, contacto, email |

**Ejemplos:**

```http
# Página 1 con 20 registros
GET /api/Suppliers/paged?pageNumber=1&pageSize=20

# Buscar proveedores que contengan "ABC"
GET /api/Suppliers/paged?searchTerm=ABC

# Incluir inactivos
GET /api/Suppliers/paged?includeInactive=true
```

**Respuesta Exitosa (200):**
```json
{
  "message": "Proveedores obtenidos exitosamente",
  "error": 0,
  "data": [
    {
      "id": 1,
      "code": "PROV-001",
      "name": "Proveedor ABC S.A.",
      "taxId": "ABC123456789",
      "contactPerson": "Juan Pérez",
      "email": "contacto@abc.com",
      "phone": "5555-1234",
      "address": "Av. Principal #123",
      "city": "Ciudad de México",
      "state": "CDMX",
      "zipCode": "01000",
      "country": "México",
      "paymentTermsDays": 30,
      "creditLimit": 100000.00,
      "defaultDiscountPercentage": 5.00,
      "isActive": true,
      "createdAt": "2026-03-10T10:00:00",
      "updatedAt": null,
      "totalPurchaseOrders": 15,
      "totalPurchased": 250000.00
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "totalPages": 3,
  "totalRecords": 25,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

### 3. Obtener proveedor por ID

**Endpoint:**
```http
GET /api/Suppliers/{id}
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Path Parameters:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `id` | int | Sí | ID del proveedor |

**Ejemplo:**
```http
GET /api/Suppliers/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200):**
```json
{
  "message": "Proveedor obtenido exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "PROV-001",
    "name": "Proveedor ABC S.A.",
    "taxId": "ABC123456789",
    "contactPerson": "Juan Pérez",
    "email": "contacto@abc.com",
    "phone": "5555-1234",
    "address": "Av. Principal #123",
    "city": "Ciudad de México",
    "state": "CDMX",
    "zipCode": "01000",
    "country": "México",
    "paymentTermsDays": 30,
    "creditLimit": 100000.00,
    "defaultDiscountPercentage": 5.00,
    "isActive": true,
    "createdAt": "2026-03-10T10:00:00",
    "updatedAt": null,
    "totalPurchaseOrders": 15,
    "totalPurchased": 250000.00
  }
}
```

**Respuesta Error - No Encontrado (404):**
```json
{
  "message": "Proveedor no encontrado",
  "error": 1
}
```

---

### 4. Obtener proveedor por código

**Endpoint:**
```http
GET /api/Suppliers/code/{code}
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Path Parameters:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `code` | string | Sí | Código del proveedor (ej: PROV-001) |

**Ejemplo:**
```http
GET /api/Suppliers/code/PROV-001
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta:** Igual que el endpoint por ID

---

## ? CREAR

### 5. Crear proveedor

**Endpoint:**
```http
POST /api/Suppliers
```

**Headers:**
```http
Authorization: Bearer {token}
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "name": "Proveedor XYZ S.A. de C.V.",
  "taxId": "XYZ987654321",
  "contactPerson": "María González",
  "email": "contacto@xyz.com",
  "phone": "5555-5678",
  "address": "Calle Secundaria #456",
  "city": "Guadalajara",
  "state": "Jalisco",
  "zipCode": "44100",
  "country": "México",
  "paymentTermsDays": 45,
  "creditLimit": 150000.00,
  "defaultDiscountPercentage": 7.50
}
```

**Campos del Body:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `name` | string | ? Sí | Nombre del proveedor (máx 200 caracteres) |
| `taxId` | string | No | RFC del proveedor (máx 20 caracteres) |
| `contactPerson` | string | No | Persona de contacto (máx 200 caracteres) |
| `email` | string | No | Email (máx 100 caracteres) |
| `phone` | string | No | Teléfono (máx 20 caracteres) |
| `address` | string | No | Dirección (máx 500 caracteres) |
| `city` | string | No | Ciudad (máx 100 caracteres) |
| `state` | string | No | Estado (máx 50 caracteres) |
| `zipCode` | string | No | Código postal (máx 10 caracteres) |
| `country` | string | No | País (default: "México") |
| `paymentTermsDays` | int | No | Días de crédito (default: 30) |
| `creditLimit` | decimal | No | Límite de crédito (default: 0) |
| `defaultDiscountPercentage` | decimal | No | % descuento default (default: 0) |

**Validaciones automáticas:**
- ? Código se genera automáticamente (PROV-001, PROV-002, etc.)
- ? RFC único (si se proporciona)
- ? Nombre obligatorio

**Respuesta Exitosa (200):**
```json
{
  "message": "Proveedor creado exitosamente",
  "error": 0,
  "data": {
    "id": 2,
    "code": "PROV-002",  // ? GENERADO AUTOMÁTICAMENTE
    "name": "Proveedor XYZ S.A. de C.V.",
    "taxId": "XYZ987654321",
    "contactPerson": "María González",
    "email": "contacto@xyz.com",
    "phone": "5555-5678",
    "address": "Calle Secundaria #456",
    "city": "Guadalajara",
    "state": "Jalisco",
    "zipCode": "44100",
    "country": "México",
    "paymentTermsDays": 45,
    "creditLimit": 150000.00,
    "defaultDiscountPercentage": 7.50,
    "isActive": true,
    "createdAt": "2026-03-10T14:30:00",
    "updatedAt": null,
    "totalPurchaseOrders": 0,  // ? NUEVO PROVEEDOR
    "totalPurchased": 0.00
  }
}
```

**Errores Posibles:**

**400 - RFC duplicado:**
```json
{
  "message": "Ya existe un proveedor con el RFC XYZ987654321",
  "error": 1
}
```

---

## ?? ACTUALIZAR

### 6. Actualizar proveedor

**Endpoint:**
```http
PUT /api/Suppliers/{id}
```

**Headers:**
```http
Authorization: Bearer {token}
Content-Type: application/json
```

**Path Parameters:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `id` | int | Sí | ID del proveedor a actualizar |

**Body (JSON):**
```json
{
  "name": "Proveedor ABC S.A. de C.V. (Actualizado)",
  "taxId": "ABC123456789",
  "contactPerson": "Juan Pérez",
  "email": "nuevo@abc.com",
  "phone": "5555-9999",
  "address": "Nueva Dirección #789",
  "city": "Ciudad de México",
  "state": "CDMX",
  "zipCode": "01000",
  "country": "México",
  "paymentTermsDays": 60,
  "creditLimit": 200000.00,
  "defaultDiscountPercentage": 10.00,
  "isActive": true
}
```

**Campos del Body:** Todos los campos de `CreateSupplierDto` + `isActive`

**Ejemplo:**
```http
PUT /api/Suppliers/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Proveedor ABC Actualizado",
  "taxId": "ABC123456789",
  "email": "nuevo@abc.com",
  "paymentTermsDays": 60,
  "creditLimit": 200000.00,
  "defaultDiscountPercentage": 10.00,
  "isActive": true
}
```

**Validaciones:**
- ? Proveedor debe existir
- ? RFC único (si cambia)

**Respuesta Exitosa (200):**
```json
{
  "message": "Proveedor actualizado exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "PROV-001",  // ? NO CAMBIA
    "name": "Proveedor ABC Actualizado",
    "taxId": "ABC123456789",
    "email": "nuevo@abc.com",
    "paymentTermsDays": 60,
    "creditLimit": 200000.00,
    "defaultDiscountPercentage": 10.00,
    "isActive": true,
    "createdAt": "2026-03-10T10:00:00",
    "updatedAt": "2026-03-10T15:00:00",  // ? ACTUALIZADO
    "totalPurchaseOrders": 15,
    "totalPurchased": 250000.00
  }
}
```

**Errores:**

**404 - Proveedor no encontrado:**
```json
{
  "message": "Proveedor con ID 99 no encontrado",
  "error": 1
}
```

**400 - RFC duplicado:**
```json
{
  "message": "Ya existe otro proveedor con el RFC XYZ987654321",
  "error": 1
}
```

---

## ??? ELIMINAR

### 7. Eliminar proveedor

**Endpoint:**
```http
DELETE /api/Suppliers/{id}
```

**Headers:**
```http
Authorization: Bearer {token}
```

**Descripción:** Desactiva el proveedor (soft delete, `IsActive = false`)

**Ejemplo:**
```http
DELETE /api/Suppliers/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Validaciones:**
- ? Proveedor debe existir

**Respuesta Exitosa (200):**
```json
{
  "message": "Proveedor desactivado exitosamente",
  "error": 0,
  "supplierId": 1
}
```

**Errores:**

**404 - No encontrado:**
```json
{
  "message": "Proveedor no encontrado",
  "error": 1
}
```

---

## ?? Ejemplos de Uso Completo

### Ejemplo 1: Crear proveedor básico

```http
POST /api/Suppliers
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Distribuidora del Norte S.A.",
  "taxId": "DNO123456789",
  "contactPerson": "Carlos Ramírez",
  "email": "carlos@delnorte.com",
  "phone": "8181-1234",
  "paymentTermsDays": 30
}
```

**Resultado:** PROV-003 creado con valores por defecto

---

### Ejemplo 2: Crear proveedor completo

```http
POST /api/Suppliers
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Importadora Global S.A. de C.V.",
  "taxId": "IGS987654321",
  "contactPerson": "Ana Martínez",
  "email": "importaciones@global.com",
  "phone": "5555-1111",
  "address": "Blvd. Internacional #2000",
  "city": "Monterrey",
  "state": "Nuevo León",
  "zipCode": "64000",
  "country": "México",
  "paymentTermsDays": 90,
  "creditLimit": 500000.00,
  "defaultDiscountPercentage": 12.50
}
```

**Resultado:** PROV-004 creado con todos los datos

---

### Ejemplo 3: Actualizar proveedor

```http
PUT /api/Suppliers/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Proveedor ABC (Nombre Actualizado)",
  "taxId": "ABC123456789",
  "contactPerson": "Juan Pérez",
  "email": "nuevo-email@abc.com",
  "phone": "5555-9999",
  "address": "Nueva dirección",
  "city": "Ciudad de México",
  "state": "CDMX",
  "zipCode": "01000",
  "country": "México",
  "paymentTermsDays": 45,
  "creditLimit": 150000.00,
  "defaultDiscountPercentage": 8.00,
  "isActive": true
}
```

---

## ?? Autenticación

Todos los endpoints requieren autenticación JWT:

1. **Login:**
```http
POST /api/Login
Content-Type: application/json

{
  "userCode": "ADMIN001",
  "password": "admin123"
}
```

2. **Usar token en headers:**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? Códigos de Error

| Código | Descripción |
|--------|-------------|
| `0` | ? Operación exitosa |
| `1` | ? Recurso no encontrado o validación fallida |
| `2` | ? Error interno del servidor |

---

## ?? Notas Importantes

1. **Código automático:**
   - ? Se genera automáticamente (PROV-001, PROV-002, etc.)
   - ? Nunca se repite (thread-safe)
   - ? Secuencial sin importar eliminaciones

2. **RFC único:**
   - ? Si se proporciona, debe ser único en la base de datos
   - ? Se valida al crear y actualizar

3. **Soft Delete:**
   - ? DELETE desactiva el proveedor (`IsActive = false`)
   - ? No se elimina físicamente de la BD
   - ? Puede reactivarse con PUT

4. **Estadísticas:**
   - ? `totalPurchaseOrders`: Cantidad de órdenes de compra
   - ? `totalPurchased`: Monto total comprado

---

## ?? Integración con Órdenes de Compra

Los proveedores se usan en:

```http
POST /api/PurchaseOrders
{
  "supplierId": 1,  // ? ID del proveedor
  "warehouseId": 1,
  "details": [...]
}
```

---

?? **Documentado por:** GitHub Copilot  
?? **Fecha:** 10 de Marzo de 2026  
? **Versión:** 1.0.0 - Módulo de Proveedores Completo
