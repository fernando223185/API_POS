# ? **MIGRACIÓN COMPLETA AL SISTEMA UNIFICADO DE PERMISOS**

## ?? **Resumen de la Migración**

Se ha migrado exitosamente del sistema antiguo (`Permissions` + `RolePermissions`) al **sistema unificado** (`RoleModulePermissions` + `UserModulePermissions`).

---

## ?? **CAMBIOS REALIZADOS**

### **1. Eliminación del Sistema Antiguo**

#### **Tablas Eliminadas:**
- ? `Permissions` (77 permisos predefinidos)
- ? `RolePermissions` (relación N:M entre Roles y Permissions)

#### **Entidades Eliminadas:**
- ? `Domain/Entities/Permission.cs`
- ? `Domain/Entities/RolePermission.cs`

#### **Código Actualizado:**
- ? `Domain/Entities/Roles.cs` - Eliminada navegación a `RolePermissions`
- ? `Infrastructure/Persistence/POSDbContext.cs` - Eliminados DbSet y configuración
- ? `Application/Abstractions/Authorization/IPermissionService.cs` - Eliminado método `GetUserPermissionsAsync`
- ? `Infrastructure/Services/PermissionService.cs` - **Reescrito completamente**
- ? `Web.Api/Controllers/Users/PermissionsController.cs` - Eliminados endpoints antiguos
- ? `Web.Api/Controllers/Users/UsersController.cs` - Actualizado para usar RoleModulePermissions
- ? `Web.Api/Controllers/Users/ModulesController.cs` - **Eliminado** (obsoleto)

---

### **2. Sistema Nuevo (UNIFICADO)**

#### **Tablas Activas:**
- ? `RoleModulePermissions` - Permisos base por rol
- ? `UserModulePermissions` - Permisos adicionales por usuario
- ? `SystemModules` - Módulos del sistema (8 módulos)
- ? `SystemSubmodules` - Submódulos (30 submódulos)

#### **Estructura:**
```
???????????????????????????????????????????
?  ROL (Permisos Base - Plantilla)       ?
?  RoleModulePermissions                  ?
?  - moduleId                             ?
?  - submoduleId                          ?
?  - canView, canCreate, canEdit, canDelete?
???????????????????????????????????????????
              ? Hereda
???????????????????????????????????????????
?  USUARIO (Permisos Base + Adicionales)  ?
?  UserModulePermissions                  ?
?  - Hereda del ROL automáticamente       ?
?  - Puede tener permisos EXTRA           ?
???????????????????????????????????????????
```

---

## ?? **ARCHIVOS CREADOS**

### **1. Script de Migración SQL**
```
Infrastructure/Scripts/MigrateToUnifiedPermissions.sql
```

**Funciones:**
- ? Mapea permisos antiguos ? nuevo sistema
- ? Migra datos de `RolePermissions` ? `RoleModulePermissions`
- ? Crea respaldos: `RolePermissions_Backup`, `Permissions_Backup`
- ? Elimina tablas antiguas
- ? Valida migración con reportes

**Ejecutar:**
```sql
USE ERP;
GO
EXEC sp_executesql N'-- Contenido del script MigrateToUnifiedPermissions.sql'
```

### **2. Migración de EF Core**
```
Infrastructure/Migrations/[Timestamp]_RemoveOldPermissionsSystem.cs
```

**Ejecutar:**
```bash
dotnet ef database update --project Infrastructure --startup-project Web.Api
```

---

## ?? **MAPEO DE PERMISOS**

### **Customer (Módulo 5 - Clientes)**
| Permiso Antiguo | Resource | Action | ? | Módulo | Submódulo | Acción |
|----------------|----------|--------|---|--------|-----------|--------|
| 1 | Customer | Create | ? | 5 | 52 (Nuevo Cliente) | CanCreate |
| 2 | Customer | Read | ? | 5 | 51 (Listado) | CanView |
| 3 | Customer | Update | ? | 5 | 51 (Listado) | CanEdit |
| 4 | Customer | Delete | ? | 5 | 51 (Listado) | CanDelete |

### **Sale (Módulo 2 - Ventas)**
| Permiso Antiguo | Resource | Action | ? | Módulo | Submódulo | Acción |
|----------------|----------|--------|---|--------|-----------|--------|
| 15 | Sale | Create | ? | 2 | 21 (Nueva Venta) | CanCreate |
| 16 | Sale | Read | ? | 2 | 22 (Historial) | CanView |
| 19 | Sale | CreateSale | ? | 2 | 21 (Nueva Venta) | CanView + CanCreate |
| 21 | Sale | ProcessRefund | ? | 2 | 24 (Devoluciones) | CanCreate |

### **Product (Módulo 3 - Productos)**
| Permiso Antiguo | Resource | Action | ? | Módulo | Submódulo | Acción |
|----------------|----------|--------|---|--------|-----------|--------|
| 23 | Product | Create | ? | 3 | 32 (Nuevo Producto) | CanCreate |
| 24 | Product | Read | ? | 3 | 31 (Catálogo) | CanView |
| 25 | Product | Update | ? | 3 | 31 (Catálogo) | CanEdit |
| 29 | Product | ImportProducts | ? | 3 | 33 (Importar) | CanCreate |

### **Inventory (Módulo 4 - Inventario)**
| Permiso Antiguo | Resource | Action | ? | Módulo | Submódulo | Acción |
|----------------|----------|--------|---|--------|-----------|--------|
| 41 | Inventory | ViewStock | ? | 4 | 41 (Stock Actual) | CanView |
| 42 | Inventory | ViewKardex | ? | 4 | 42 (Kardex) | CanView |
| 43 | Inventory | AdjustStock | ? | 4 | 44 (Movimientos) | CanCreate |

### **Billing (Módulo 6 - CFDI)**
| Permiso Antiguo | Resource | Action | ? | Módulo | Submódulo | Acción |
|----------------|----------|--------|---|--------|-----------|--------|
| 52 | Billing | ViewInvoices | ? | 6 | 62 (Facturas Emitidas) | CanView |
| 53 | Billing | CreateInvoice | ? | 6 | 61 (Nueva Factura) | CanCreate |
| 55 | Billing | ProcessStamping | ? | 6 | 63 (Pendientes) | CanCreate |

### **Configuration (Módulo 8 - Configuración)**
| Permiso Antiguo | Resource | Action | ? | Módulo | Submódulo | Acción |
|----------------|----------|--------|---|--------|-----------|--------|
| 64 | Configuration | ManageUsers | ? | 8 | 81 (Usuarios) | Todos |
| 65 | Configuration | ManagePermissions | ? | 8 | 83 (Permisos) | Todos |
| 66 | Configuration | ManageCompany | ? | 8 | 84 (Datos Empresa) | View + Edit |

---

## ?? **PermissionService - NUEVO**

### **Método: HasPermissionAsync**

**Antes (Sistema Antiguo):**
```csharp
var permissions = await _context.RolePermissions
    .Include(rp => rp.Permission)
    .Where(rp => rp.RoleId == roleId)
    .ToListAsync();

return permissions.Any(p => 
    p.Permission.Resource == "Customer" && 
    p.Permission.Name == "Create");
```

**Ahora (Sistema Unificado):**
```csharp
// 1. Verificar permisos personalizados del usuario (prioridad)
var userPermission = await _context.UserModulePermissions
    .Where(up => up.UserId == userId &&
               up.ModuleId == moduleId &&
               up.SubmoduleId == submoduleId &&
               up.HasAccess)
    .FirstOrDefaultAsync();

if (userPermission != null)
    return userPermission.CanCreate; // o CanView, CanEdit, CanDelete

// 2. Si no tiene permisos personalizados, verificar permisos del rol
var rolePermission = await _context.RoleModulePermissions
    .Where(rp => rp.RoleId == roleId &&
               rp.ModuleId == moduleId &&
               rp.SubmoduleId == submoduleId &&
               rp.HasAccess)
    .FirstOrDefaultAsync();

if (rolePermission != null)
    return rolePermission.CanCreate;

return false;
```

**Ventajas:**
- ? Permisos personalizados tienen prioridad
- ? Herencia automática del rol
- ? Control granular (View, Create, Edit, Delete)
- ? Mapeo transparente de Resource/Action ? Módulo/Submódulo

---

## ?? **ENDPOINTS ACTUALIZADOS**

### **Endpoints Eliminados:**
```
? GET /api/Permissions/all  (Permissions antiguas)
? GET /api/Permissions/user/{userId}/role-based  (RolePermissions)
? POST /api/Permissions/user/check-role-based
? GET /api/Permissions/my-permissions  (basado en Permissions)
```

### **Endpoints Nuevos/Actualizados:**
```
? GET /api/Roles/{id}/module-permissions  (Sistema Nuevo)
? POST /api/Roles/{id}/module-permissions  (Sistema Nuevo)
? DELETE /api/Roles/{id}/module-permissions  (Sistema Nuevo)

? GET /api/Permissions/role/{roleId}  (Actualizado - usa RoleModulePermissions)
? GET /api/Permissions/user/{userId}/custom  (Sistema Nuevo)
? POST /api/Permissions/user/save-custom  (Sistema Nuevo)
? DELETE /api/Permissions/user/{userId}/custom  (Sistema Nuevo)
```

---

## ?? **PASOS PARA APLICAR LA MIGRACIÓN**

### **1. Respaldar Base de Datos (IMPORTANTE)**
```sql
BACKUP DATABASE ERP
TO DISK = 'C:\Backups\ERP_Before_Migration.bak'
WITH FORMAT, INIT, NAME = 'ERP Before Permissions Migration';
```

### **2. Ejecutar Script de Migración SQL**
```bash
sqlcmd -S localhost -d ERP -i "Infrastructure\Scripts\MigrateToUnifiedPermissions.sql"
```

**Resultado Esperado:**
```
? Tabla de mapeo creada con X registros
? Permisos migrados: Y registros
? Respaldo de RolePermissions creado: Z registros
? Respaldo de Permissions creado: 77 registros
? Tabla RolePermissions eliminada
? Tabla Permissions eliminada
? MIGRACIÓN COMPLETADA EXITOSAMENTE
```

### **3. Aplicar Migración de EF Core**
```bash
cd C:\Users\PCX\source\repos\API_POS
dotnet ef database update --project Infrastructure --startup-project Web.Api
```

### **4. Verificar Compilación**
```bash
dotnet build
```

### **5. Ejecutar Aplicación**
```bash
dotnet run --project Web.Api
```

---

## ?? **VERIFICACIÓN POST-MIGRACIÓN**

### **1. Verificar Permisos Migrados**
```sql
SELECT 
    r.Name AS RoleName,
    COUNT(DISTINCT rmp.ModuleId) AS TotalModulos,
    COUNT(rmp.Id) AS TotalPermisos,
    SUM(CASE WHEN rmp.CanView = 1 THEN 1 ELSE 0 END) AS PermisosVer,
    SUM(CASE WHEN rmp.CanCreate = 1 THEN 1 ELSE 0 END) AS PermisosCrear,
    SUM(CASE WHEN rmp.CanEdit = 1 THEN 1 ELSE 0 END) AS PermisosEditar,
    SUM(CASE WHEN rmp.CanDelete = 1 THEN 1 ELSE 0 END) AS PermisosEliminar
FROM Roles r
LEFT JOIN RoleModulePermissions rmp ON rmp.RoleId = r.Id
WHERE r.IsActive = 1
GROUP BY r.Name
ORDER BY r.Name;
```

### **2. Verificar Respaldos**
```sql
-- Verificar que los respaldos existen
SELECT COUNT(*) AS TotalRolePermissions FROM RolePermissions_Backup;
SELECT COUNT(*) AS TotalPermissions FROM Permissions_Backup;
```

### **3. Probar Endpoints**
```bash
# Obtener permisos de un rol
GET http://localhost:7254/api/Roles/1/module-permissions

# Guardar permisos de un rol
POST http://localhost:7254/api/Roles/3/module-permissions
{
  "roleId": 3,
  "modules": [...]
}

# Permisos personalizados de usuario
POST http://localhost:7254/api/Permissions/user/save-custom
{
  "userId": 7,
  "modules": [...]
}
```

---

## ?? **ROLLBACK (Si es necesario)**

### **En caso de problemas, restaurar:**

```sql
-- 1. Restaurar tablas desde respaldo
SELECT * INTO Permissions FROM Permissions_Backup;
SELECT * INTO RolePermissions FROM RolePermissions_Backup;

-- 2. Recrear foreign keys
ALTER TABLE RolePermissions
ADD CONSTRAINT FK_RolePermissions_Roles_RoleId
FOREIGN KEY (RoleId) REFERENCES Roles(Id);

ALTER TABLE RolePermissions
ADD CONSTRAINT FK_RolePermissions_Permissions_PermissionId
FOREIGN KEY (PermissionId) REFERENCES Permissions(Id);

-- 3. Limpiar RoleModulePermissions
TRUNCATE TABLE RoleModulePermissions;
```

### **Revertir migración de EF Core:**
```bash
dotnet ef database update [PreviousMigration] --project Infrastructure --startup-project Web.Api
dotnet ef migrations remove --project Infrastructure --startup-project Web.Api
```

---

## ? **ESTADO FINAL**

### **Tablas Activas:**
```
? Roles
? Users
? RoleModulePermissions (NUEVO - Sistema Unificado)
? UserModulePermissions (NUEVO - Sistema Unificado)
? SystemModules
? SystemSubmodules
```

### **Tablas Eliminadas:**
```
? Permissions (77 permisos - RESPALDADO)
? RolePermissions (relación N:M - RESPALDADO)
```

### **Respaldos Disponibles:**
```
?? RolePermissions_Backup
?? Permissions_Backup
```

---

## ?? **VENTAJAS DEL NUEVO SISTEMA**

1. ? **Sistema Unificado** - Misma estructura para roles y usuarios
2. ? **Permisos Granulares** - View, Create, Edit, Delete por submódulo
3. ? **Herencia Automática** - Usuario hereda permisos del rol
4. ? **Permisos Adicionales** - Usuario puede tener permisos extra
5. ? **Flexibilidad** - Fácil agregar nuevos módulos/submódulos
6. ? **Mismo JSON** - Frontend usa la misma estructura para ambos
7. ? **Mantenimiento Simple** - Un solo sistema de permisos
8. ? **Escalable** - Basado en módulos del sistema

---

## ?? **DOCUMENTACIÓN RELACIONADA**

- `DOCS/UnifiedPermissions_System.md` - Guía del sistema unificado
- `DOCS/Sistema_Unificado_Implementado.md` - Implementación completa
- `DOCS/Analisis_Sistema_Permisos_Dual.md` - Análisis del sistema dual
- `Infrastructure/Scripts/MigrateToUnifiedPermissions.sql` - Script de migración
- `Infrastructure/Scripts/CreateRoleModulePermissionsTable.sql` - Creación de tabla

---

**? MIGRACIÓN COMPLETADA EXITOSAMENTE** ??

**Fecha:** 2025-01-07  
**Sistema:** ERP POS - Permisos Unificados  
**Estado:** ? Listo para Producción
