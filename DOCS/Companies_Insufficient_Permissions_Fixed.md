# ?? **Solución: Insufficient Permissions en Companies Controller**

## ?? **Problema**

Al intentar acceder a los endpoints de `/api/companies`, recibes:

```json
{
  "message": "Insufficient permissions",
  "error": 1
}
```

---

## ? **Solución**

El controlador `CompaniesController` usa el atributo:

```csharp
[RequirePermission("Configuration", "ManageCompanies")]
```

Esto significa que el sistema busca:
- **Módulo:** `Configuration`
- **Submódulo:** `ManageCompanies`

---

## ?? **Pasos para Solucionar**

### **Opción 1: Ejecutar Script SQL (Recomendado)**

1. **Abre SQL Server Management Studio (SSMS)**

2. **Ejecuta el script:**

```bash
# Ruta del script:
Infrastructure/Scripts/AddCompaniesPermissionsToAdmin.sql
```

3. **?? Importante:** Cambia la primera línea con el nombre de tu base de datos:

```sql
USE [ERP]; -- Cambia por tu base de datos
```

4. **Ejecuta el script completo** (F5 en SSMS)

5. **Verifica el resultado:** El script mostrará un resumen con checkmarks ?

---

### **Opción 2: SQL Manual (Rápido)**

Si prefieres hacerlo manualmente, ejecuta este SQL:

```sql
-- 1. Buscar ID del módulo Configuration
DECLARE @ConfigModuleId INT;
SELECT @ConfigModuleId = Id FROM [Modules] WHERE Name = 'Configuration';

-- 2. Buscar o crear submódulo ManageCompanies
DECLARE @SubmoduleId INT;
SELECT @SubmoduleId = Id FROM [Submodules] 
WHERE ModuleId = @ConfigModuleId AND Name = 'ManageCompanies';

IF @SubmoduleId IS NULL
BEGIN
    INSERT INTO [Submodules] (ModuleId, Name, Description, Path, Icon, [Order], Color, IsActive, CreatedAt)
    VALUES (@ConfigModuleId, 'ManageCompanies', 'Gestión de empresas', '/config/companies', 'faBuilding', 1, '#34495e', 1, GETUTCDATE());
    SET @SubmoduleId = SCOPE_IDENTITY();
END

-- 3. Insertar permisos para Rol Administrador (ID: 1)
DELETE FROM [RoleModulePermissions] WHERE RoleId = 1 AND SubmoduleId = @SubmoduleId;

INSERT INTO [RoleModulePermissions] (
    RoleId, ModuleId, Name, Path, Icon, [Order],
    HasAccess, SubmoduleId, CanView, CanCreate, CanEdit, CanDelete,
    CreatedAt, CreatedByUserId
)
SELECT 
    1, Id, Name, Path, Icon, [Order],
    1, @SubmoduleId, 1, 1, 1, 1,
    GETUTCDATE(), 1
FROM [Modules] WHERE Id = @ConfigModuleId;

-- 4. Verificar
SELECT * FROM [RoleModulePermissions] WHERE RoleId = 1 AND SubmoduleId = @SubmoduleId;
```

---

### **Opción 3: API (Postman)**

Puedes configurar permisos usando el endpoint:

```http
POST http://localhost:7254/api/Roles/1/module-permissions
Authorization: Bearer {token_admin}
Content-Type: application/json

{
  "modules": [
    {
      "moduleId": 8,  // ID del módulo Configuration
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 81,  // ID del submódulo ManageCompanies
          "hasAccess": true,
          "canView": true,
          "canCreate": true,
          "canEdit": true,
          "canDelete": true
        }
      ]
    }
  ]
}
```

---

## ?? **Verificación**

### **1. Verificar en Base de Datos:**

```sql
SELECT 
    r.Name AS Rol,
    m.Name AS Modulo,
    s.Name AS Submodulo,
    rmp.CanView,
    rmp.CanCreate,
    rmp.CanEdit,
    rmp.CanDelete
FROM [RoleModulePermissions] rmp
INNER JOIN [Roles] r ON r.Id = rmp.RoleId
INNER JOIN [Modules] m ON m.Id = rmp.ModuleId
LEFT JOIN [Submodules] s ON s.Id = rmp.SubmoduleId
WHERE r.Id = 1
  AND m.Name = 'Configuration'
  AND s.Name = 'ManageCompanies';
```

**Resultado esperado:**

| Rol | Modulo | Submodulo | CanView | CanCreate | CanEdit | CanDelete |
|-----|--------|-----------|---------|-----------|---------|-----------|
| Administrador | Configuration | ManageCompanies | 1 | 1 | 1 | 1 |

---

### **2. Probar en Postman:**

```http
GET http://localhost:7254/api/companies
Authorization: Bearer {token_admin}
```

**Respuesta esperada:**

```json
{
  "message": "Empresas obtenidas exitosamente",
  "error": 0,
  "data": [],
  "page": 1,
  "pageSize": 20,
  "totalRecords": 0,
  "totalPages": 0
}
```

? **Ya no debe mostrar "Insufficient permissions"**

---

## ?? **Endpoints Habilitados**

Una vez configurado, estos endpoints funcionarán:

| Método | Endpoint | Permiso Requerido | Descripción |
|--------|----------|-------------------|-------------|
| GET | `/api/companies` | `CanView` | Listar empresas |
| GET | `/api/companies/{id}` | `CanView` | Obtener empresa |
| GET | `/api/companies/active` | `CanView` | Empresas activas |
| GET | `/api/companies/main` | `CanView` | Empresa principal |
| POST | `/api/companies` | `CanCreate` | Crear empresa |
| PUT | `/api/companies/{id}` | `CanEdit` | Actualizar empresa |
| PUT | `/api/companies/{id}/deactivate` | `CanDelete` | Desactivar |
| PUT | `/api/companies/{id}/reactivate` | `CanEdit` | Reactivar |
| PUT | `/api/companies/{id}/fiscal-config` | `CanEdit` | Config fiscal |

---

## ?? **Usuario de Prueba**

```
Usuario: ADMIN001
Password: admin123
```

---

## ?? **Notas Importantes**

1. **El nombre del submódulo DEBE ser exactamente:** `ManageCompanies`
   - Si se llama "Empresas" o "Companies", NO funcionará
   - El código busca: `[RequirePermission("Configuration", "ManageCompanies")]`

2. **El módulo padre debe ser:** `Configuration`

3. **Los permisos se verifican en el middleware** `RequirePermissionAttribute`

4. **Si cambias el nombre del submódulo**, también debes actualizar el atributo en el controlador

---

## ?? **Resultado Final**

Después de ejecutar el script:

? Rol Administrador tiene acceso completo a Companies  
? Puede ver, crear, editar y eliminar empresas  
? Todos los endpoints de `/api/companies` funcionan  
? No más error "Insufficient permissions"  

---

## ?? **Documentación Relacionada**

- **Controller:** `Web.Api/Controllers/Config/CompaniesController.cs`
- **Script SQL:** `Infrastructure/Scripts/AddCompaniesPermissionsToAdmin.sql`
- **Sistema de Permisos:** `DOCS/Sistema_Unificado_Implementado.md`

---

**? Problema Resuelto** ??
