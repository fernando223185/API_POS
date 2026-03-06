# ?? Sistema CRUD de Módulos y Submódulos

## ?? Descripción

Sistema completo para gestionar la estructura de módulos y submódulos del ERP desde la base de datos. Permite configurar dinámicamente el menú del sistema sin necesidad de modificar código.

---

## ? Implementación Completada

### **1?? Base de Datos**
- ? Tabla `Modules` - 8 módulos principales
- ? Tabla `Submodules` - 30 submódulos
- ? Relación FK con CASCADE DELETE
- ? Índices optimizados (Order, IsActive, ModuleId)
- ? Seed data ejecutado

### **2?? Entidades**
- ? `Domain/Entities/SystemModule.cs` ? Tabla `Modules`
- ? `Domain/Entities/SystemSubmodule.cs` ? Tabla `Submodules`

### **3?? DTOs**
- ? `Application/DTOs/SystemModules/SystemModuleDtos.cs`

### **4?? Controller CRUD Completo**
- ? `Web.Api/Controllers/Config/SystemModulesController.cs` ? `AppModulesController`
- ? Ruta base: `/api/Modules`

---

## ??? Estructura de Tablas

### **Tabla: Modules**
```sql
CREATE TABLE [Modules] (
    [Id] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Path] nvarchar(200) NOT NULL,
    [Icon] nvarchar(50) NOT NULL,
    [Order] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Modules] PRIMARY KEY ([Id])
);
```

### **Tabla: Submodules**
```sql
CREATE TABLE [Submodules] (
    [Id] int NOT NULL,
    [ModuleId] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Path] nvarchar(200) NOT NULL,
    [Icon] nvarchar(50) NOT NULL,
    [Order] int NOT NULL,
    [Color] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Submodules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Submodules_Modules] 
        FOREIGN KEY ([ModuleId]) REFERENCES [Modules] ([Id]) ON DELETE CASCADE
);
```

---

## ?? Endpoints Disponibles

**Ruta Base:** `/api/Modules`

### ?? **MÓDULOS**

#### 1?? Obtener Todos los Módulos
```http
GET /api/Modules
GET /api/Modules?includeInactive=true
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Módulos obtenidos exitosamente",
  "error": 0,
  "modules": [
    {
      "id": 1,
      "name": "Inicio",
      "description": "Panel principal y dashboard del sistema",
      "path": "/dashboard",
      "icon": "faHome",
      "order": 1,
      "isActive": true,
      "createdAt": "2025-01-30T22:47:14Z",
      "updatedAt": null,
      "submodules": []
    },
    {
      "id": 2,
      "name": "Ventas",
      "description": "Gestión de ventas, cotizaciones y devoluciones",
      "path": "/sales",
      "icon": "faShoppingCart",
      "order": 2,
      "isActive": true,
      "submodules": [...]
    }
  ],
  "totalModules": 8,
  "totalSubmodules": 30
}
```

---

#### 2?? Obtener Módulo por ID
```http
GET /api/Modules/{id}
Authorization: Bearer {token}
```

**Ejemplo:**
```http
GET /api/Modules/2
```

**Respuesta:**
```json
{
  "message": "Módulo obtenido exitosamente",
  "error": 0,
  "data": {
    "id": 2,
    "name": "Ventas",
    "description": "Gestión de ventas, cotizaciones y devoluciones",
    "path": "/sales",
    "icon": "faShoppingCart",
    "order": 2,
    "isActive": true,
    "submodules": [...]
  }
}
```

---

#### 3?? Crear Nuevo Módulo
```http
POST /api/Modules
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": 9,
  "name": "Compras",
  "description": "Gestión de compras y proveedores",
  "path": "/purchases",
  "icon": "faShoppingBag",
  "order": 9,
  "isActive": true
}
```

**Respuesta:**
```json
{
  "message": "Módulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 9,
    "name": "Compras",
    "description": "Gestión de compras y proveedores",
    "path": "/purchases",
    "icon": "faShoppingBag",
    "order": 9,
    "isActive": true,
    "createdAt": "2025-01-30T22:50:00Z",
    "submodules": []
  }
}
```

---

#### 4?? Actualizar Módulo
```http
PUT /api/Modules/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": 9,
  "name": "Compras y Proveedores",
  "description": "Gestión completa de compras",
  "path": "/purchases",
  "icon": "faShoppingBag",
  "order": 9,
  "isActive": true
}
```

**Ejemplo:**
```http
PUT /api/Modules/9
```

---

#### 5?? Eliminar Módulo (Soft Delete)
```http
DELETE /api/Modules/{id}
Authorization: Bearer {token}
```

**Ejemplo:**
```http
DELETE /api/Modules/9
```

**Respuesta:**
```json
{
  "message": "Módulo eliminado exitosamente",
  "error": 0,
  "moduleId": 9
}
```

---

### ?? **SUBMÓDULOS**

#### 6?? Obtener Submódulos de un Módulo
```http
GET /api/Modules/{moduleId}/submodules
GET /api/Modules/{moduleId}/submodules?includeInactive=true
Authorization: Bearer {token}
```

**Ejemplo:**
```http
GET /api/Modules/2/submodules
```

**Respuesta:**
```json
{
  "message": "Submódulos obtenidos exitosamente",
  "error": 0,
  "moduleId": 2,
  "moduleName": "Ventas",
  "submodules": [
    {
      "id": 21,
      "moduleId": 2,
      "name": "Nueva Venta",
      "description": "Crear ticket y capturar productos",
      "path": "/sales/new",
      "icon": "faPlus",
      "order": 1,
      "color": "from-emerald-500 to-teal-600",
      "isActive": true
    }
  ],
  "totalSubmodules": 4
}
```

---

#### 7?? Obtener Submódulo por ID
```http
GET /api/Modules/submodules/{id}
Authorization: Bearer {token}
```

**Ejemplo:**
```http
GET /api/Modules/submodules/21
```

---

#### 8?? Crear Nuevo Submódulo
```http
POST /api/Modules/submodules
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": 91,
  "moduleId": 9,
  "name": "Nueva Orden de Compra",
  "description": "Crear orden de compra a proveedores",
  "path": "/purchases/new",
  "icon": "faPlus",
  "order": 1,
  "color": "from-emerald-500 to-teal-600",
  "isActive": true
}
```

---

#### 9?? Actualizar Submódulo
```http
PUT /api/Modules/submodules/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": 91,
  "moduleId": 9,
  "name": "Crear Orden de Compra",
  "description": "Generar OC con cotizaciones",
  "path": "/purchases/new",
  "icon": "faPlus",
  "order": 1,
  "color": "from-emerald-500 to-teal-600",
  "isActive": true
}
```

**Ejemplo:**
```http
PUT /api/Modules/submodules/91
```

---

#### ?? Eliminar Submódulo (Soft Delete)
```http
DELETE /api/Modules/submodules/{id}
Authorization: Bearer {token}
```

**Ejemplo:**
```http
DELETE /api/Modules/submodules/91
```

---

## ?? Datos Iniciales (Seed Data)

### **Módulos (8 registros en Modules)**

| ID | Nombre | Path | Icon | Submódulos |
|----|--------|------|------|------------|
| 1 | Inicio | /dashboard | faHome | 0 |
| 2 | Ventas | /sales | faShoppingCart | 4 |
| 3 | Productos | /products | faBoxOpen | 4 |
| 4 | Inventario | /inventory | faWarehouse | 4 |
| 5 | Clientes | /customers | faUsers | 3 |
| 6 | CFDI | /billing | faFileInvoice | 4 |
| 7 | Reportes | /reports | faChartBar | 4 |
| 8 | Configuración | /config | faCog | 6 |

### **Submódulos (30 registros en Submodules)**

#### Ventas (4)
- 21 - Nueva Venta
- 22 - Historial de Ventas
- 23 - Cotizaciones
- 24 - Devoluciones

#### Productos (4)
- 31 - Catálogo de Productos
- 32 - Nuevo Producto
- 33 - Importar Productos
- 34 - Categorías

#### Inventario (4)
- 41 - Stock Actual
- 42 - Kardex
- 43 - Alertas de Stock
- 44 - Movimientos

#### Clientes (3)
- 51 - Listado de Clientes
- 52 - Nuevo Cliente
- 53 - Importar Clientes

#### CFDI (4)
- 61 - Nueva Factura
- 62 - Facturas Emitidas
- 63 - Facturas Pendientes
- 64 - Facturas Timbradas

#### Reportes (4)
- 71 - Reporte de Ventas
- 72 - Reporte de Inventario
- 73 - Reporte de Productos
- 74 - Reporte de Clientes

#### Configuración (6)
- 81 - Usuarios
- 82 - Roles
- 83 - Permisos
- 84 - Datos de Empresa
- 85 - Sucursales
- 86 - Apariencia

---

## ?? Seguridad

### Permisos Requeridos
- **Consultar (GET)**: Solo requiere autenticación (`[RequireAuthentication]`)
- **Crear/Actualizar/Eliminar (POST/PUT/DELETE)**: Requiere `"Configuration"` ? `"ManagePermissions"`

---

## ?? Casos de Uso

### **Caso 1: Agregar Nuevo Módulo "Compras"**

```sh
# 1. Crear el módulo
POST /api/Modules
{
  "id": 9,
  "name": "Compras",
  "description": "Gestión de compras y proveedores",
  "path": "/purchases",
  "icon": "faShoppingBag",
  "order": 9,
  "isActive": true
}

# 2. Agregar submódulos
POST /api/Modules/submodules
{
  "id": 91,
  "moduleId": 9,
  "name": "Nueva Orden de Compra",
  "description": "Crear OC",
  "path": "/purchases/new",
  "icon": "faPlus",
  "order": 1,
  "color": "from-emerald-500 to-teal-600",
  "isActive": true
}
```

---

## ??? Integración Frontend

### React/Vue/Angular - Ejemplo

```typescript
class ModuleService {
  private baseUrl = 'http://localhost:7254/api/Modules';
  
  // Obtener todos los módulos para el menú
  async getAllModules(includeInactive: boolean = false): Promise<ModulesResponse> {
    const response = await fetch(`${this.baseUrl}?includeInactive=${includeInactive}`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    return await response.json();
  }
  
  // Crear módulo dinámicamente
  async createModule(data: CreateModuleDto): Promise<any> {
    const response = await fetch(this.baseUrl, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });
    return await response.json();
  }
  
  // Obtener submódulos de un módulo
  async getSubmodules(moduleId: number): Promise<SubmodulesResponse> {
    const response = await fetch(`${this.baseUrl}/${moduleId}/submodules`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    return await response.json();
  }
}
```

---

## ? Ventajas del Sistema

1. **Configurable**: Los módulos se gestionan desde la BD, no desde código
2. **Escalable**: Fácil agregar nuevos módulos sin recompilar
3. **Flexible**: Activar/desactivar módulos dinámicamente
4. **Organizable**: Control total del orden de visualización
5. **CRUD Completo**: Crear, Leer, Actualizar, Eliminar
6. **Soft Delete**: Los registros se marcan como inactivos, no se eliminan
7. **Relaciones en Cascada**: Eliminar un módulo desactiva sus submódulos

---

## ?? Notas Importantes

- **Tablas**: Se usan `Modules` y `Submodules`
- **Controller**: `AppModulesController` con ruta `/api/Modules`
- Los IDs de módulos y submódulos son **manuales** (no auto-incrementales)
- Los submódulos con ID `X1, X2, X3` pertenecen al módulo `X`
- Al eliminar un módulo, todos sus submódulos se desactivan automáticamente
- Los módulos inactivos no aparecen en las consultas por defecto (salvo `includeInactive=true`)

---

## ?? Migración Ejecutada

```sh
# Migración manual con scripts SQL
Infrastructure/Scripts/RenameTablesModulesSubmodules.sql
Infrastructure/Scripts/SeedSystemModules.sql
```

**Tablas creadas:**
- `Modules` (8 registros)
- `Submodules` (30 registros)

---

**? Sistema CRUD de Módulos - 100% Funcional**  
**?? Ruta API:** `/api/Modules`  
**?? Controller:** `AppModulesController`  
**??? Tablas:** `Modules` y `Submodules`
