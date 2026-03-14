# ?? Módulos y Submódulos: API CRUD Completo

## ? Endpoints Disponibles

Ya existen **endpoints completos** para crear, leer, actualizar y eliminar módulos y submódulos en el sistema. No necesitas usar scripts SQL, todo se puede hacer por API.

---

## ?? Endpoints de Módulos

### Base URL: `/api/modules`

| Método | Endpoint | Descripción | Permiso |
|--------|----------|-------------|---------|
| GET | `/api/modules` | Obtener todos los módulos | Autenticado |
| GET | `/api/modules/{id}` | Obtener módulo por ID | Autenticado |
| POST | `/api/modules` | **Crear nuevo módulo** | `Configuration.ManageModules` |
| PUT | `/api/modules/{id}` | Actualizar módulo | `Configuration.ManageModules` |
| DELETE | `/api/modules/{id}` | Eliminar módulo (soft delete) | `Configuration.ManageModules` |

---

## ?? Endpoints de Submódulos

### Base URL: `/api/modules/submodules`

| Método | Endpoint | Descripción | Permiso |
|--------|----------|-------------|---------|
| GET | `/api/modules/{moduleId}/submodules` | Obtener submódulos de un módulo | Autenticado |
| GET | `/api/modules/submodules/{id}` | Obtener submódulo por ID | Autenticado |
| POST | `/api/modules/submodules` | **Crear nuevo submódulo** | `Configuration.ManageModules` |
| PUT | `/api/modules/submodules/{id}` | Actualizar submódulo | `Configuration.ManageModules` |
| DELETE | `/api/modules/submodules/{id}` | Eliminar submódulo (soft delete) | `Configuration.ManageModules` |

---

## ?? Crear Módulo

### **POST** `/api/modules`

**Permiso requerido:** `Configuration.ManageModules`

### Request Body

```json
{
  "name": "Billing",
  "description": "Módulo de Facturación Electrónica",
  "path": "/billing",
  "icon": "fa-file-invoice",
  "order": 10,
  "isActive": true
}
```

### Campos del Request

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `name` | string | ? | Nombre del módulo |
| `description` | string | ? | Descripción del módulo |
| `path` | string | ? | Ruta del módulo (ej: `/billing`) |
| `icon` | string | ? | Icono FontAwesome (ej: `fa-file-invoice`) |
| `order` | int | ? | Orden de visualización |
| `isActive` | bool | No | Si está activo (default: `true`) |

### Response Exitoso (200 OK)

```json
{
  "message": "Módulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 10,
    "name": "Billing",
    "description": "Módulo de Facturación Electrónica",
    "path": "/billing",
    "icon": "fa-file-invoice",
    "order": 10,
    "isActive": true,
    "createdAt": "2026-03-13T22:30:00Z",
    "submodules": []
  }
}
```

### Ejemplo en Postman

**URL:**
```
POST https://api.tuempresa.com/api/modules
```

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

**Body (raw JSON):**
```json
{
  "name": "Billing",
  "description": "Módulo de Facturación Electrónica",
  "path": "/billing",
  "icon": "fa-file-invoice",
  "order": 10,
  "isActive": true
}
```

---

## ?? Crear Submódulo

### **POST** `/api/modules/submodules`

**Permiso requerido:** `Configuration.ManageModules`

### Request Body

```json
{
  "moduleId": 10,
  "name": "Facturas Pendientes",
  "description": "Gestión de ventas pendientes de timbrar",
  "path": "/billing/pending",
  "icon": "fa-clock",
  "order": 1,
  "color": "#FF6B6B",
  "isActive": true
}
```

### Campos del Request

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `moduleId` | int | ? | ID del módulo padre |
| `name` | string | ? | Nombre del submódulo |
| `description` | string | ? | Descripción del submódulo |
| `path` | string | ? | Ruta del submódulo (ej: `/billing/pending`) |
| `icon` | string | ? | Icono FontAwesome (ej: `fa-clock`) |
| `order` | int | ? | Orden dentro del módulo |
| `color` | string | ? | Color hexadecimal (ej: `#FF6B6B`) |
| `isActive` | bool | No | Si está activo (default: `true`) |

### Response Exitoso (200 OK)

```json
{
  "message": "Submódulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 45,
    "moduleId": 10,
    "name": "Facturas Pendientes",
    "description": "Gestión de ventas pendientes de timbrar",
    "path": "/billing/pending",
    "icon": "fa-clock",
    "order": 1,
    "color": "#FF6B6B",
    "isActive": true,
    "createdAt": "2026-03-13T22:32:00Z"
  }
}
```

### Response Error: Módulo no existe (400 Bad Request)

```json
{
  "message": "El módulo padre con ID 999 no existe",
  "error": 1,
  "data": null
}
```

### Ejemplo en Postman

**URL:**
```
POST https://api.tuempresa.com/api/modules/submodules
```

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

**Body (raw JSON):**
```json
{
  "moduleId": 10,
  "name": "Facturas Pendientes",
  "description": "Gestión de ventas pendientes de timbrar",
  "path": "/billing/pending",
  "icon": "fa-clock",
  "order": 1,
  "color": "#FF6B6B",
  "isActive": true
}
```

---

## ?? Obtener Todos los Módulos

### **GET** `/api/modules`

**Permiso:** Autenticado

### Query Parameters

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `includeInactive` | bool | false | Incluir módulos inactivos |

### Request Ejemplo

```
GET https://api.tuempresa.com/api/modules?includeInactive=false
Authorization: Bearer {token}
```

### Response

```json
{
  "message": "Módulos obtenidos exitosamente",
  "error": 0,
  "data": [
    {
      "id": 10,
      "name": "Billing",
      "description": "Módulo de Facturación Electrónica",
      "path": "/billing",
      "icon": "fa-file-invoice",
      "order": 10,
      "isActive": true,
      "createdAt": "2026-03-13T22:30:00Z",
      "submodules": [
        {
          "id": 45,
          "moduleId": 10,
          "name": "Facturas Pendientes",
          "description": "Gestión de ventas pendientes de timbrar",
          "path": "/billing/pending",
          "icon": "fa-clock",
          "order": 1,
          "color": "#FF6B6B",
          "isActive": true
        }
      ]
    }
  ]
}
```

---

## ?? Actualizar Módulo

### **PUT** `/api/modules/{id}`

**Permiso requerido:** `Configuration.ManageModules`

### Request Body

```json
{
  "name": "Billing - Actualizado",
  "description": "Módulo de Facturación Electrónica CFDI 4.0",
  "path": "/billing",
  "icon": "fa-file-invoice-dollar",
  "order": 10,
  "isActive": true
}
```

### Ejemplo en Postman

```
PUT https://api.tuempresa.com/api/modules/10
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Billing - Actualizado",
  "description": "Módulo de Facturación Electrónica CFDI 4.0",
  "path": "/billing",
  "icon": "fa-file-invoice-dollar",
  "order": 10,
  "isActive": true
}
```

---

## ?? Actualizar Submódulo

### **PUT** `/api/modules/submodules/{id}`

**Permiso requerido:** `Configuration.ManageModules`

### Request Body

```json
{
  "name": "Facturas Pendientes - Actualizado",
  "description": "Gestión completa de ventas pendientes de timbrar",
  "path": "/billing/pending",
  "icon": "fa-clock",
  "order": 1,
  "color": "#FF6B6B",
  "isActive": true
}
```

### Ejemplo en Postman

```
PUT https://api.tuempresa.com/api/modules/submodules/45
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Facturas Pendientes - Actualizado",
  "description": "Gestión completa de ventas pendientes de timbrar",
  "path": "/billing/pending",
  "icon": "fa-clock",
  "order": 1,
  "color": "#FF6B6B",
  "isActive": true
}
```

---

## ??? Eliminar Módulo

### **DELETE** `/api/modules/{id}`

**Permiso requerido:** `Configuration.ManageModules`

**Nota:** Es un **soft delete** (marca como inactivo, no elimina físicamente)

### Request Ejemplo

```
DELETE https://api.tuempresa.com/api/modules/10
Authorization: Bearer {token}
```

### Response

```json
{
  "message": "Módulo eliminado exitosamente",
  "error": 0,
  "deletedId": 10
}
```

---

## ??? Eliminar Submódulo

### **DELETE** `/api/modules/submodules/{id}`

**Permiso requerido:** `Configuration.ManageModules`

**Nota:** Es un **soft delete**

### Request Ejemplo

```
DELETE https://api.tuempresa.com/api/modules/submodules/45
Authorization: Bearer {token}
```

### Response

```json
{
  "message": "Submódulo eliminado exitosamente",
  "error": 0,
  "deletedId": 45
}
```

---

## ?? Obtener Submódulos de un Módulo

### **GET** `/api/modules/{moduleId}/submodules`

**Permiso:** Autenticado

### Query Parameters

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `includeInactive` | bool | false | Incluir submódulos inactivos |

### Request Ejemplo

```
GET https://api.tuempresa.com/api/modules/10/submodules?includeInactive=false
Authorization: Bearer {token}
```

### Response

```json
{
  "message": "Submódulos obtenidos exitosamente",
  "error": 0,
  "data": [
    {
      "id": 45,
      "moduleId": 10,
      "name": "Facturas Pendientes",
      "description": "Gestión de ventas pendientes de timbrar",
      "path": "/billing/pending",
      "icon": "fa-clock",
      "order": 1,
      "color": "#FF6B6B",
      "isActive": true
    }
  ]
}
```

---

## ?? Ejemplo Completo: Crear Módulo Billing con Submódulos

### Paso 1: Crear Módulo "Billing"

```http
POST /api/modules
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Billing",
  "description": "Módulo de Facturación Electrónica",
  "path": "/billing",
  "icon": "fa-file-invoice",
  "order": 10,
  "isActive": true
}
```

**Response:**
```json
{
  "message": "Módulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 10,
    "name": "Billing"
  }
}
```

### Paso 2: Crear Submódulo "Facturas Pendientes"

```http
POST /api/modules/submodules
Authorization: Bearer {token}
Content-Type: application/json

{
  "moduleId": 10,
  "name": "Facturas Pendientes",
  "description": "Gestión de ventas pendientes de timbrar",
  "path": "/billing/pending",
  "icon": "fa-clock",
  "order": 1,
  "color": "#FF6B6B",
  "isActive": true
}
```

### Paso 3: Crear Submódulo "Facturas Timbradas"

```http
POST /api/modules/submodules
Authorization: Bearer {token}
Content-Type: application/json

{
  "moduleId": 10,
  "name": "Facturas Timbradas",
  "description": "Consulta de facturas timbradas",
  "path": "/billing/invoiced",
  "icon": "fa-check-circle",
  "order": 2,
  "color": "#51CF66",
  "isActive": true
}
```

### Paso 4: Crear Submódulo "Cancelar Facturas"

```http
POST /api/modules/submodules
Authorization: Bearer {token}
Content-Type: application/json

{
  "moduleId": 10,
  "name": "Cancelar Facturas",
  "description": "Gestión de cancelaciones de facturas",
  "path": "/billing/cancel",
  "icon": "fa-ban",
  "order": 3,
  "color": "#FA5252",
  "isActive": true
}
```

---

## ?? Permisos Requeridos

Para usar estos endpoints, el usuario debe tener el permiso:

```
Módulo: Configuration
Permiso: ManageModules
```

### Verificar Permisos

Si obtienes un error 403 (Forbidden), significa que tu usuario no tiene el permiso `Configuration.ManageModules`.

**Solución:** Ejecutar script SQL:

```sql
-- Asignar permiso al rol Admin
DECLARE @RoleId INT = (SELECT Id FROM Roles WHERE RoleName = 'Admin');
DECLARE @ModuleId INT = (SELECT Id FROM SystemModules WHERE Name = 'Configuration');
DECLARE @SubmoduleId INT = (SELECT Id FROM SystemSubmodules WHERE Name = 'Gestión de Módulos');

IF NOT EXISTS (
    SELECT 1 FROM RoleModulePermissions 
    WHERE RoleId = @RoleId 
    AND ModuleId = @ModuleId 
    AND SubmoduleId = @SubmoduleId
    AND PermissionKey = 'ManageModules'
)
BEGIN
    INSERT INTO RoleModulePermissions (RoleId, ModuleId, SubmoduleId, PermissionKey, CanView, CanCreate, CanEdit, CanDelete)
    VALUES (@RoleId, @ModuleId, @SubmoduleId, 'ManageModules', 1, 1, 1, 1);
    PRINT '? Permiso ManageModules asignado';
END
```

---

## ?? Colección Postman

### Importar Colección

Crea una nueva colección en Postman con estos requests:

**Variables de entorno:**
- `base_url`: `https://api.tuempresa.com`
- `token`: Tu JWT token

### Requests Sugeridos

1. **GET All Modules** - `/api/modules`
2. **GET Module by ID** - `/api/modules/{{moduleId}}`
3. **POST Create Module** - `/api/modules`
4. **PUT Update Module** - `/api/modules/{{moduleId}}`
5. **DELETE Module** - `/api/modules/{{moduleId}}`
6. **GET Submodules** - `/api/modules/{{moduleId}}/submodules`
7. **POST Create Submodule** - `/api/modules/submodules`
8. **PUT Update Submodule** - `/api/modules/submodules/{{submoduleId}}`
9. **DELETE Submodule** - `/api/modules/submodules/{{submoduleId}}`

---

## ?? Colores Recomendados para Submódulos

```
#FF6B6B - Rojo (Pendientes, Alertas)
#51CF66 - Verde (Completado, Exitoso)
#FA5252 - Rojo oscuro (Cancelaciones, Errores)
#339AF0 - Azul (Información, Consultas)
#FCC419 - Amarillo (En proceso, Advertencias)
#AE3EC9 - Morado (Reportes, Analíticas)
#74C0FC - Azul claro (Configuración)
#FF922B - Naranja (Operaciones)
```

---

## ?? Manejo de Errores

### Error 400 - Bad Request

```json
{
  "message": "Datos de entrada inválidos",
  "error": 1,
  "errors": [
    "El campo Name es requerido",
    "El campo Path es requerido"
  ]
}
```

### Error 403 - Forbidden

```json
{
  "message": "No tienes permisos para realizar esta acción",
  "error": 1
}
```

### Error 404 - Not Found

```json
{
  "message": "Módulo no encontrado",
  "error": 1
}
```

### Error 500 - Internal Server Error

```json
{
  "message": "Error al crear módulo",
  "error": 2,
  "details": "Exception details..."
}
```

---

## ?? Arquitectura

### Patrón CQRS + MediatR

**Commands:**
- `CreateModuleCommand`
- `UpdateModuleCommand`
- `DeleteModuleCommand`
- `CreateSubmoduleCommand`
- `UpdateSubmoduleCommand`
- `DeleteSubmoduleCommand`

**Queries:**
- `GetAllModulesQuery`
- `GetModuleByIdQuery`
- `GetSubmodulesByModuleQuery`
- `GetSubmoduleByIdQuery`

**Handlers:**
- `Application/Core/SystemModules/CommandHandlers/`
- `Application/Core/SystemModules/QueryHandlers/`

**Repository:**
- `ISystemModuleRepository`
- `SystemModuleRepository`

---

## ? Resumen

### ¡Ya NO necesitas scripts SQL INSERT!

Ahora puedes:
- ? Crear módulos por API
- ? Crear submódulos por API
- ? Actualizar módulos/submódulos por API
- ? Eliminar módulos/submódulos por API
- ? Consultar módulos/submódulos por API

### Todo con arquitectura CQRS + MediatR

**Controller:** `Web.Api/Controllers/Config/ModulesController.cs`

**Fecha de implementación:** Ya disponible  
**Estado:** ? **FUNCIONANDO**

---

¡Usa Postman para gestionar tus módulos y submódulos! ??
