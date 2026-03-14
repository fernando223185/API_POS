# ?? Guía Rápida: Crear Módulos y Submódulos por API

## ? Resumen Ejecutivo

**¡Ya NO necesitas scripts SQL INSERT!**

Todos los módulos y submódulos se pueden crear, actualizar y eliminar mediante **API REST**.

---

## ?? Endpoints Principales

### Módulos
```
POST   /api/modules                    ? Crear módulo
GET    /api/modules                    ? Listar todos
GET    /api/modules/{id}               ? Obtener uno
PUT    /api/modules/{id}               ? Actualizar
DELETE /api/modules/{id}               ? Eliminar
```

### Submódulos
```
POST   /api/modules/submodules         ? Crear submódulo
GET    /api/modules/{id}/submodules    ? Listar por módulo
GET    /api/modules/submodules/{id}    ? Obtener uno
PUT    /api/modules/submodules/{id}    ? Actualizar
DELETE /api/modules/submodules/{id}    ? Eliminar
```

---

## ?? Crear Módulo

### Request
```http
POST /api/modules
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Billing",
  "description": "Módulo de Facturación",
  "path": "/billing",
  "icon": "fa-file-invoice",
  "order": 10,
  "isActive": true
}
```

### Response
```json
{
  "message": "Módulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 10,
    "name": "Billing",
    "path": "/billing",
    "icon": "fa-file-invoice"
  }
}
```

---

## ?? Crear Submódulo

### Request
```http
POST /api/modules/submodules
Authorization: Bearer {token}
Content-Type: application/json

{
  "moduleId": 10,
  "name": "Facturas Pendientes",
  "description": "Ventas pendientes de timbrar",
  "path": "/billing/pending",
  "icon": "fa-clock",
  "order": 1,
  "color": "#FF6B6B",
  "isActive": true
}
```

### Response
```json
{
  "message": "Submódulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 45,
    "moduleId": 10,
    "name": "Facturas Pendientes",
    "path": "/billing/pending"
  }
}
```

---

## ?? Permiso Requerido

```
Módulo: Configuration
Permiso: ManageModules
```

Si obtienes error 403, ejecuta este SQL:

```sql
DECLARE @RoleId INT = (SELECT Id FROM Roles WHERE RoleName = 'Admin');
DECLARE @ModuleId INT = (SELECT Id FROM SystemModules WHERE Name = 'Configuration');

INSERT INTO RoleModulePermissions (RoleId, ModuleId, SubmoduleId, PermissionKey, CanView, CanCreate, CanEdit, CanDelete)
SELECT @RoleId, @ModuleId, NULL, 'ManageModules', 1, 1, 1, 1
WHERE NOT EXISTS (
    SELECT 1 FROM RoleModulePermissions 
    WHERE RoleId = @RoleId AND PermissionKey = 'ManageModules'
);
```

---

## ?? Ejemplo Completo en Postman

### 1. Crear Módulo "Billing"
```json
POST /api/modules
{
  "name": "Billing",
  "description": "Facturación Electrónica",
  "path": "/billing",
  "icon": "fa-file-invoice",
  "order": 10,
  "isActive": true
}
```
**? Respuesta: `id: 10`**

### 2. Crear Submódulo "Pendientes"
```json
POST /api/modules/submodules
{
  "moduleId": 10,
  "name": "Pendientes",
  "description": "Ventas por timbrar",
  "path": "/billing/pending",
  "icon": "fa-clock",
  "order": 1,
  "color": "#FF6B6B",
  "isActive": true
}
```

### 3. Crear Submódulo "Timbradas"
```json
POST /api/modules/submodules
{
  "moduleId": 10,
  "name": "Timbradas",
  "description": "Facturas timbradas",
  "path": "/billing/invoiced",
  "icon": "fa-check-circle",
  "order": 2,
  "color": "#51CF66",
  "isActive": true
}
```

### 4. Crear Submódulo "Cancelar"
```json
POST /api/modules/submodules
{
  "moduleId": 10,
  "name": "Cancelar",
  "description": "Cancelación de facturas",
  "path": "/billing/cancel",
  "icon": "fa-ban",
  "order": 3,
  "color": "#FA5252",
  "isActive": true
}
```

---

## ?? Colores Recomendados

```
#FF6B6B - Rojo (Pendientes)
#51CF66 - Verde (Completado)
#FA5252 - Rojo oscuro (Cancelaciones)
#339AF0 - Azul (Información)
#FCC419 - Amarillo (En proceso)
#AE3EC9 - Morado (Reportes)
```

---

## ?? Ver Módulos Creados

### Request
```http
GET /api/modules
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
      "submodules": [
        {
          "id": 45,
          "name": "Pendientes",
          "path": "/billing/pending",
          "color": "#FF6B6B"
        },
        {
          "id": 46,
          "name": "Timbradas",
          "path": "/billing/invoiced",
          "color": "#51CF66"
        }
      ]
    }
  ]
}
```

---

## ?? Actualizar Módulo

```http
PUT /api/modules/10
Authorization: Bearer {token}

{
  "name": "Billing - Actualizado",
  "description": "CFDI 4.0",
  "path": "/billing",
  "icon": "fa-file-invoice",
  "order": 10,
  "isActive": true
}
```

---

## ??? Eliminar Módulo

```http
DELETE /api/modules/10
Authorization: Bearer {token}
```

**Nota:** Es soft delete (no elimina físicamente)

---

## ?? Códigos de Error

| Código | Descripción |
|--------|-------------|
| `error: 0` | ? Éxito |
| `error: 1` | ? Validación/No encontrado |
| `error: 2` | ? Error del servidor |

---

## ?? Documentación Completa

Ver: `DOCS/Modules_Submodules_API_Complete.md`

---

## ? Ventajas de Usar API

- ? No necesitas acceso directo a BD
- ? Validaciones automáticas
- ? Auditoría incluida
- ? Respuestas estructuradas
- ? Arquitectura CQRS + MediatR
- ? Soft delete (no destruye datos)

---

## ?? ¿Qué Usar?

| Tarea | Usa |
|-------|-----|
| Crear módulo | ? API `/api/modules` |
| Crear submódulo | ? API `/api/modules/submodules` |
| Listar módulos | ? API `/api/modules` |
| Actualizar | ? API (PUT) |
| Eliminar | ? API (DELETE) |
| Scripts SQL | ? Ya no necesarios |

---

**Fecha:** 2026-03-13  
**Estado:** ? **FUNCIONAL**  
**Controller:** `ModulesController.cs`

---

¡Usa Postman y olvídate de los scripts SQL! ??
