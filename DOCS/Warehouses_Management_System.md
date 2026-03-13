# ?? Sistema de Gestión de Almacenes - Documentación Completa

## ? Sistema Implementado

Se ha creado un **sistema completo de gestión de almacenes** relacionado con **sucursales**, siguiendo el patrón **CQRS** y las mejores prácticas del proyecto.

---

## ?? Componentes Creados

### 1. **Entidad** (`Domain/Entities/Warehouse.cs`)
- ? Código automático (formato: `ALM-001`, `ALM-002`, etc.)
- ? Baja lógica con campo `IsActive`
- ? Relación obligatoria con tabla `Branches`
- ? Auditoría completa (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
- ? Control de capacidad y temperatura
- ? Validaciones con Data Annotations

**Campos principales:**
```csharp
- Id (autoincremental)
- Code (generado automáticamente)
- Name, Description
- BranchId (FK a Branches) ? RELACIÓN CON SUCURSALES
- WarehouseType (General, Refrigerado, Materias Primas, etc.)
- PhysicalLocation
- MaxCapacity, CurrentCapacity (m³)
- ManagerName, ManagerEmail, ManagerPhone
- IsMainWarehouse (almacén principal de la sucursal)
- AllowsReceiving, AllowsShipping
- RequiresTemperatureControl
- MinTemperature, MaxTemperature (°C)
- IsActive (baja lógica)
- CreatedAt, UpdatedAt
- CreatedByUserId, UpdatedByUserId
```

---

### 2. **DTOs** (`Application/DTOs/Warehouse/`)
- ? `CreateWarehouseDto` - Para crear almacenes
- ? `UpdateWarehouseDto` - Para actualizar almacenes
- ? `WarehouseResponseDto` - Respuesta individual (incluye datos de capacidad calculados)
- ? `WarehousesListResponseDto` - Lista completa
- ? `WarehousesPagedResponseDto` - Paginación
- ? `WarehousesByBranchResponseDto` - Almacenes por sucursal ?

---

### 3. **Repositorio** (`Application/Abstractions/Config/IWarehouseRepository.cs`)
**Métodos implementados:**
- `GetByIdAsync()` - Obtener por ID
- `GetByCodeAsync()` - Obtener por código
- `GetAllAsync()` - Obtener todos (con opción includeInactive)
- `GetByBranchIdAsync()` - **Obtener por sucursal** ?
- `GetPagedAsync()` - Obtener paginados con filtros (incluye filtro por sucursal)
- `CreateAsync()` - Crear almacén
- `UpdateAsync()` - Actualizar almacén
- `ExistsAsync()` - Verificar existencia
- `CodeExistsAsync()` - Validar código duplicado
- `BranchExistsAsync()` - **Validar que la sucursal existe** ?
- `GenerateNextCodeAsync()` - **Generar código automático** ?
- `GetTotalCountAsync()` - Total de almacenes
- `GetActiveCountAsync()` - Total activos
- `GetCountByBranchAsync()` - **Contar por sucursal** ?

---

### 4. **CQRS Commands** (`Application/Core/Warehouse/Commands/`)
- ? `CreateWarehouseCommand` - Crear almacén
- ? `UpdateWarehouseCommand` - Actualizar almacén
- ? `DeactivateWarehouseCommand` - Baja lógica
- ? `ReactivateWarehouseCommand` - Reactivar almacén

---

### 5. **CQRS Queries** (`Application/Core/Warehouse/Queries/`)
- ? `GetAllWarehousesQuery` - Todos los almacenes
- ? `GetWarehousesPagedQuery` - Paginación con filtros
- ? `GetWarehouseByIdQuery` - Por ID
- ? `GetWarehouseByCodeQuery` - Por código
- ? `GetWarehousesByBranchQuery` - **Por sucursal** ?

---

### 6. **Handlers**
**CommandHandlers:**
- `CreateWarehouseCommandHandler` - Crea almacén con código automático y valida sucursal
- `UpdateWarehouseCommandHandler` - Actualiza datos y recalcula capacidad
- `DeactivateWarehouseCommandHandler` - Desactiva (baja lógica)
- `ReactivateWarehouseCommandHandler` - Reactiva almacén

**QueryHandlers:**
- `GetAllWarehousesQueryHandler` - Lista completa con cálculos de capacidad
- `GetWarehousesPagedQueryHandler` - Paginación
- `GetWarehouseByIdQueryHandler` - Búsqueda por ID
- `GetWarehouseByCodeQueryHandler` - Búsqueda por código
- `GetWarehousesByBranchQueryHandler` - **Almacenes de una sucursal** ?

---

### 7. **Controlador API** (`Web.Api/Controllers/Config/WarehousesController.cs`)

---

## ?? Endpoints Disponibles

### **?? Consultas (GET)**

#### 1. Obtener todos los almacenes
```http
GET /api/Warehouses
GET /api/Warehouses?includeInactive=true
```

**Respuesta:**
```json
{
  "message": "Almacenes obtenidos exitosamente",
  "error": 0,
  "warehouses": [
    {
      "id": 1,
      "code": "ALM-001",
      "name": "Almacén Principal Centro",
      "description": "Almacén general de la matriz",
      "branchId": 1,
      "branchCode": "SUC-001",
      "branchName": "Sucursal Centro",
      "warehouseType": "General",
      "physicalLocation": "Piso 2, Área A",
      "maxCapacity": 1000.00,
      "currentCapacity": 650.00,
      "availableCapacity": 350.00,
      "capacityUsagePercentage": 65.00,
      "managerName": "Carlos López",
      "managerEmail": "carlos@empresa.com",
      "managerPhone": "5551234567",
      "isMainWarehouse": true,
      "allowsReceiving": true,
      "allowsShipping": true,
      "requiresTemperatureControl": false,
      "minTemperature": null,
      "maxTemperature": null,
      "isActive": true,
      "createdAt": "2026-03-10T17:16:00",
      "updatedAt": null,
      "createdByUserName": "Administrador",
      "updatedByUserName": null
    }
  ],
  "totalWarehouses": 5,
  "activeWarehouses": 4,
  "inactiveWarehouses": 1
}
```

#### 2. Obtener almacenes paginados
```http
GET /api/Warehouses/paged?pageNumber=1&pageSize=10
GET /api/Warehouses/paged?pageNumber=1&pageSize=10&searchTerm=Refrigerado
GET /api/Warehouses/paged?pageNumber=1&pageSize=10&branchId=1
GET /api/Warehouses/paged?branchId=1&includeInactive=true
```

#### 3. Obtener almacenes por sucursal ?
```http
GET /api/Warehouses/branch/1
GET /api/Warehouses/branch/1?includeInactive=true
```

**Respuesta:**
```json
{
  "message": "Almacenes de la sucursal obtenidos exitosamente",
  "error": 0,
  "branchId": 1,
  "branchName": "Sucursal Centro",
  "warehouses": [
    {
      "id": 1,
      "code": "ALM-001",
      "name": "Almacén Principal",
      ...
    },
    {
      "id": 2,
      "code": "ALM-002",
      "name": "Almacén Refrigerado",
      ...
    }
  ],
  "totalWarehouses": 2
}
```

#### 4. Obtener almacén por ID
```http
GET /api/Warehouses/1
```

#### 5. Obtener almacén por código
```http
GET /api/Warehouses/code/ALM-001
```

---

### **? Crear Almacén (POST)**

```http
POST /api/Warehouses
Content-Type: application/json
Authorization: Bearer {token}
```

**Body:**
```json
{
  "name": "Almacén Principal",
  "description": "Almacén general de productos",
  "branchId": 1,
  "warehouseType": "General",
  "physicalLocation": "Piso 2, Área A",
  "maxCapacity": 1000.00,
  "managerName": "Carlos López",
  "managerEmail": "carlos@empresa.com",
  "managerPhone": "5551234567",
  "isMainWarehouse": true,
  "allowsReceiving": true,
  "allowsShipping": true,
  "requiresTemperatureControl": false
}
```

**Respuesta:**
```json
{
  "message": "Almacén creado exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "ALM-001",  // ? Generado automáticamente
    "name": "Almacén Principal",
    "branchId": 1,
    "branchCode": "SUC-001",
    "branchName": "Sucursal Centro",
    "maxCapacity": 1000.00,
    "currentCapacity": 0.00,
    "availableCapacity": 1000.00,
    "capacityUsagePercentage": 0.00,
    ...
  }
}
```

---

### **Ejemplo: Almacén Refrigerado**
```json
{
  "name": "Almacén Refrigerado",
  "description": "Productos perecederos",
  "branchId": 1,
  "warehouseType": "Refrigerado",
  "physicalLocation": "Sótano, Cámara 1",
  "maxCapacity": 500.00,
  "managerName": "Ana García",
  "managerEmail": "ana@empresa.com",
  "requiresTemperatureControl": true,
  "minTemperature": 2.00,
  "maxTemperature": 8.00,
  "isMainWarehouse": false,
  "allowsReceiving": true,
  "allowsShipping": true
}
```

---

### **?? Actualizar Almacén (PUT)**

```http
PUT /api/Warehouses/1
Content-Type: application/json
Authorization: Bearer {token}
```

**Body:**
```json
{
  "name": "Almacén Principal Actualizado",
  "description": "Nueva descripción",
  "branchId": 1,
  "warehouseType": "General",
  "physicalLocation": "Piso 3, Área B",
  "maxCapacity": 1200.00,
  "currentCapacity": 800.00,
  "managerName": "Carlos López",
  "managerEmail": "carlos@empresa.com",
  "managerPhone": "5551234567",
  "isMainWarehouse": true,
  "allowsReceiving": true,
  "allowsShipping": true,
  "requiresTemperatureControl": false,
  "isActive": true
}
```

---

### **??? Baja Lógica (DELETE)**

#### Desactivar almacén
```http
DELETE /api/Warehouses/1
Authorization: Bearer {token}
```

#### Reactivar almacén
```http
PATCH /api/Warehouses/1/reactivate
Authorization: Bearer {token}
```

---

## ?? Características Especiales

### ? **1. Código Automático**
El sistema genera automáticamente códigos secuenciales:
- Primer almacén: `ALM-001`
- Segundo almacén: `ALM-002`
- Tercer almacén: `ALM-003`
- etc.

### ?? **2. Relación con Sucursales**
- ? Cada almacén **DEBE** pertenecer a una sucursal
- ? Se valida que la sucursal existe antes de crear/actualizar
- ? Se puede consultar todos los almacenes de una sucursal
- ? Filtro por sucursal en consultas paginadas

### ?? **3. Cálculos Automáticos de Capacidad**
Cada respuesta incluye:
```json
{
  "maxCapacity": 1000.00,
  "currentCapacity": 650.00,
  "availableCapacity": 350.00,      // ? Calculado automáticamente
  "capacityUsagePercentage": 65.00  // ? Calculado automáticamente
}
```

### ??? **4. Control de Temperatura**
Para almacenes refrigerados:
```json
{
  "requiresTemperatureControl": true,
  "minTemperature": 2.00,
  "maxTemperature": 8.00
}
```

### ?? **5. Baja Lógica**
Los almacenes nunca se eliminan físicamente, solo se marcan como `IsActive = false`

**Ventajas:**
- ? Historial completo
- ? Posibilidad de reactivación
- ? Integridad referencial
- ? Auditoría completa

### ?? **6. Auditoría**
Cada operación registra:
- `CreatedAt` - Fecha de creación
- `CreatedByUserId` - Usuario que creó
- `UpdatedAt` - Fecha de última actualización
- `UpdatedByUserId` - Usuario que actualizó

---

## ??? Migración de Base de Datos

La migración `20260310171631_AddWarehousesTable` creó:

```sql
CREATE TABLE [Warehouses] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [BranchId] int NOT NULL,  -- ? FK a Branches
    [WarehouseType] nvarchar(50) NOT NULL,
    [PhysicalLocation] nvarchar(200) NULL,
    [MaxCapacity] decimal(18,2) NULL,
    [CurrentCapacity] decimal(18,2) NULL,
    [ManagerName] nvarchar(200) NULL,
    [ManagerEmail] nvarchar(100) NULL,
    [ManagerPhone] nvarchar(20) NULL,
    [IsMainWarehouse] bit NOT NULL,
    [AllowsReceiving] bit NOT NULL,
    [AllowsShipping] bit NOT NULL,
    [RequiresTemperatureControl] bit NOT NULL,
    [MinTemperature] decimal(5,2) NULL,
    [MaxTemperature] decimal(5,2) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedByUserId] int NULL,
    [UpdatedByUserId] int NULL,
    CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Warehouses_Branches_BranchId] FOREIGN KEY ([BranchId]) 
        REFERENCES [Branches] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Warehouses_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) 
        REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Warehouses_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) 
        REFERENCES [Users] ([Id])
);

CREATE INDEX [IX_Warehouses_BranchId] ON [Warehouses] ([BranchId]);
CREATE INDEX [IX_Warehouses_CreatedByUserId] ON [Warehouses] ([CreatedByUserId]);
CREATE INDEX [IX_Warehouses_UpdatedByUserId] ON [Warehouses] ([UpdatedByUserId]);
```

---

## ?? Ejemplos de Uso en Postman

### **Crear Almacén General**
```http
POST http://localhost:7254/api/Warehouses
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Almacén Principal",
  "description": "Almacén general de productos",
  "branchId": 1,
  "warehouseType": "General",
  "physicalLocation": "Piso 2",
  "maxCapacity": 1000.00,
  "managerName": "Carlos López",
  "isMainWarehouse": true,
  "allowsReceiving": true,
  "allowsShipping": true
}
```
? Genera código: `ALM-001`

### **Crear Almacén Refrigerado**
```http
POST http://localhost:7254/api/Warehouses
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Cámara Fría 1",
  "description": "Productos perecederos",
  "branchId": 1,
  "warehouseType": "Refrigerado",
  "maxCapacity": 300.00,
  "requiresTemperatureControl": true,
  "minTemperature": 2.00,
  "maxTemperature": 8.00,
  "isMainWarehouse": false
}
```
? Genera código: `ALM-002`

### **Obtener Almacenes de una Sucursal**
```http
GET http://localhost:7254/api/Warehouses/branch/1
Authorization: Bearer {{token}}
```

### **Buscar con Filtros**
```http
GET http://localhost:7254/api/Warehouses/paged?branchId=1&searchTerm=Refrigerado
Authorization: Bearer {{token}}
```

---

## ?? Códigos de Error

| Código | Descripción |
|--------|-------------|
| `0` | ? Operación exitosa |
| `1` | ? Recurso no encontrado (almacén o sucursal) |
| `2` | ? Error interno del servidor |

---

## ?? Relación con Sucursales

### Flujo de Trabajo:
1. **Crear Sucursal** ? `POST /api/Branches`
2. **Crear Almacén** ? `POST /api/Warehouses` (con `branchId`)
3. **Consultar Almacenes** ? `GET /api/Warehouses/branch/{branchId}`

### Ejemplo Completo:
```bash
# 1. Crear Sucursal
POST /api/Branches
{
  "name": "Sucursal Norte",
  "address": "Av. Norte 123",
  "city": "Monterrey",
  ...
}
? Código generado: SUC-001

# 2. Crear Almacén General
POST /api/Warehouses
{
  "name": "Almacén Principal",
  "branchId": 1,  // ? Sucursal creada en paso 1
  "warehouseType": "General",
  ...
}
? Código generado: ALM-001

# 3. Crear Almacén Refrigerado
POST /api/Warehouses
{
  "name": "Cámara Fría",
  "branchId": 1,  // ? Misma sucursal
  "warehouseType": "Refrigerado",
  "requiresTemperatureControl": true,
  ...
}
? Código generado: ALM-002

# 4. Consultar Almacenes de la Sucursal
GET /api/Warehouses/branch/1
? Retorna: ALM-001 y ALM-002
```

---

## ? Sistema Listo para Usar

El módulo de almacenes está **100% funcional** e integrado con:
- ? Autenticación JWT
- ? Patrón CQRS
- ? Repositorio genérico
- ? Relación con Sucursales ?
- ? Auditoría automática
- ? Baja lógica
- ? Generación automática de códigos
- ? Cálculos de capacidad
- ? Control de temperatura
- ? Migración de base de datos aplicada

**Endpoint base:** `http://localhost:7254/api/Warehouses`

---

?? **Documentado por:** GitHub Copilot  
?? **Fecha:** 10 de Marzo de 2026  
? **Versión:** 1.0.0
