# ? **Guía Completa: Cómo Verificar Permisos del Módulo de Ventas**

## ?? **Objetivo**

Verificar que el módulo de Ventas y sus permisos estén correctamente configurados para que los usuarios puedan acceder sin errores de "Insufficient permissions".

---

## ?? **Métodos de Verificación**

---

### **Método 1: Script SQL de Verificación Automática** ? (Recomendado)

Este script verifica todo automáticamente y genera un reporte completo.

#### **Ejecutar:**
```bash
sqlcmd -S localhost -d ERP -U sa -P "TuPassword" \
  -i "Infrastructure/Scripts/VerifySalesModulePermissions.sql"
```

#### **? Resultado Esperado:**
```
?? ====================================
? SISTEMA CORRECTAMENTE CONFIGURADO
?? ====================================

El módulo de Ventas está listo para usarse.
Los usuarios con rol Administrador tienen acceso completo.
```

#### **? Resumen que debe mostrar:**
```
   ? Módulo de Ventas: EXISTE
   ?? Submódulos de Ventas: 4-8
   ? Rol Administrador: EXISTE
   ?? Permisos asignados: 5-9
   ?? Usuarios Administradores: 1+
```

---

### **Método 2: Verificación Manual por SQL**

#### **2.1. Verificar Módulo de Ventas:**
```sql
SELECT * FROM Modules WHERE Id = 2;
```

**? Debe retornar:**
| Id | Name   | Path    | Icon              | IsActive |
|----|--------|---------|-------------------|----------|
| 2  | Ventas | /sales  | fa-shopping-cart  | 1        |

---

#### **2.2. Verificar Submódulos:**
```sql
SELECT Id, Name, Path, Icon, [Order], IsActive
FROM Submodules 
WHERE ModuleId = 2
ORDER BY [Order];
```

**? Debe retornar al menos 4 submódulos:**
| Id | Name                | Path              | IsActive |
|----|---------------------|-------------------|----------|
| 10 | Nueva Venta         | /sales/new        | 1        |
| 11 | Lista de Ventas     | /sales/list       | 1        |
| 12 | Reportes de Ventas  | /sales/reports    | 1        |
| 13 | Cobranza            | /sales/payments   | 1        |

---

#### **2.3. Verificar Permisos del Administrador:**
```sql
SELECT 
    m.Name AS Modulo,
    s.Name AS Submodulo,
    rmp.CanView,
    rmp.CanCreate,
    rmp.CanEdit,
    rmp.CanDelete
FROM RoleModulePermissions rmp
INNER JOIN Modules m ON m.Id = rmp.ModuleId
LEFT JOIN Submodules s ON s.Id = rmp.SubmoduleId
WHERE rmp.RoleId = 1 AND m.Id = 2
ORDER BY ISNULL(s.[Order], 0);
```

**? Debe retornar al menos 5 permisos (1 módulo + 4 submódulos):**
| Modulo | Submodulo           | CanView | CanCreate | CanEdit | CanDelete |
|--------|---------------------|---------|-----------|---------|-----------|
| Ventas | NULL                | 1       | 1         | 1       | 1         |
| Ventas | Nueva Venta         | 1       | 1         | 1       | 1         |
| Ventas | Lista de Ventas     | 1       | 1         | 1       | 1         |
| Ventas | Reportes de Ventas  | 1       | 1         | 1       | 1         |
| Ventas | Cobranza            | 1       | 1         | 1       | 1         |

---

#### **2.4. Verificar Usuarios con Rol Administrador:**
```sql
SELECT 
    u.Id,
    u.Code,
    u.Name,
    u.Email,
    r.Name AS RoleName
FROM Users u
INNER JOIN Roles r ON r.Id = u.RoleId
WHERE u.RoleId = 1 AND u.Active = 1;
```

**? Debe retornar al menos 1 usuario:**
| Id | Code     | Name           | RoleName      |
|----|----------|----------------|---------------|
| 1  | ADMIN001 | Administrador  | Administrador |

---

### **Método 3: Verificación desde la API** ??

#### **3.1. Obtener Permisos del Rol Administrador**

**Request:**
```http
GET http://localhost:7254/api/Roles/1/module-permissions
Authorization: Bearer {tu_token_jwt}
```

**? Respuesta Esperada:**
```json
{
  "error": 0,
  "message": "Permisos obtenidos exitosamente",
  "roleId": 1,
  "roleName": "Administrador",
  "modules": [
    {
      "moduleId": 2,
      "moduleName": "Ventas",
      "canView": true,
      "canCreate": true,
      "canEdit": true,
      "canDelete": true,
      "submodules": [
        {
          "submoduleId": 10,
          "submoduleName": "Nueva Venta",
          "canView": true,
          "canCreate": true,
          "canEdit": true,
          "canDelete": true
        },
        // ... más submódulos
      ]
    }
  ]
}
```

---

#### **3.2. Obtener Menú del Usuario**

**Request:**
```http
GET http://localhost:7254/api/Modules/user/1/menu
Authorization: Bearer {tu_token_jwt}
```

**? Respuesta Esperada:**
Debe incluir el módulo de Ventas:
```json
{
  "error": 0,
  "message": "Menú obtenido exitosamente",
  "modules": [
    {
      "id": 2,
      "name": "Ventas",
      "path": "/sales",
      "icon": "fa-shopping-cart",
      "hasAccess": true,
      "canView": true,
      "canCreate": true,
      "canEdit": true,
      "canDelete": true,
      "submodules": [
        {
          "id": 10,
          "name": "Nueva Venta",
          "path": "/sales/new",
          "icon": "fa-cash-register",
          "hasAccess": true
        }
      ]
    }
  ]
}
```

---

#### **3.3. Verificar Acceso al Endpoint de Ventas**

**Request:**
```http
GET http://localhost:7254/api/sales
Authorization: Bearer {tu_token_jwt}
```

**? Respuesta Esperada:**
- **HTTP 200 OK** (lista de ventas vacía o con datos)
- **NO debe retornar HTTP 403** (Insufficient permissions)

**? Error de Permisos:**
```json
{
  "message": "Insufficient permissions"
}
```

---

### **Método 4: Verificación desde Postman** ??

#### **Paso 1: Login**
```http
POST http://localhost:7254/api/login
Content-Type: application/json

{
  "code": "ADMIN001",
  "password": "admin123"
}
```

**Copiar el `token` de la respuesta.**

---

#### **Paso 2: Verificar Permisos**
```http
GET http://localhost:7254/api/Roles/1/module-permissions
Authorization: Bearer {token_del_paso_1}
```

**Buscar el módulo "Ventas" en la respuesta.**

---

#### **Paso 3: Probar Endpoint de Ventas**
```http
POST http://localhost:7254/api/sales
Authorization: Bearer {token_del_paso_1}
Content-Type: application/json

{
  "warehouseId": 1,
  "discountPercentage": 0,
  "requiresInvoice": false,
  "details": [
    {
      "productId": 1,
      "quantity": 1,
      "unitPrice": 100,
      "discountPercentage": 0
    }
  ]
}
```

**? Si funciona:** Los permisos están correctos  
**? Si retorna 403:** Faltan permisos

---

## ?? **Solución de Problemas**

### **Problema 1: "Insufficient permissions"**

**Verificar:**
```sql
-- ¿El módulo de Ventas existe?
SELECT * FROM Modules WHERE Id = 2;

-- ¿El usuario tiene rol Administrador?
SELECT u.Code, u.Name, r.Name AS RoleName
FROM Users u
INNER JOIN Roles r ON r.Id = u.RoleId
WHERE u.Code = 'TU_USUARIO';

-- ¿El rol Administrador tiene permisos en Ventas?
SELECT COUNT(*) AS TotalPermisos
FROM RoleModulePermissions
WHERE RoleId = 1 AND ModuleId = 2;
```

**Solución:**
```bash
# Ejecutar el script de permisos
sqlcmd -S localhost -d ERP -U sa -P "Password" \
  -i "Infrastructure/Scripts/AddSalesModulePermissions.sql"
```

---

### **Problema 2: Módulo no aparece en el menú**

**Verificar:**
```sql
-- ¿El módulo está activo?
SELECT IsActive FROM Modules WHERE Id = 2;
```

**Solución:**
```sql
-- Activar el módulo
UPDATE Modules SET IsActive = 1 WHERE Id = 2;

-- Activar todos los submódulos
UPDATE Submodules SET IsActive = 1 WHERE ModuleId = 2;
```

---

### **Problema 3: JWT Token expirado**

**Síntoma:**
```json
{
  "message": "Token has expired"
}
```

**Solución:**
1. Hacer login de nuevo
2. Obtener un nuevo token
3. Actualizar el token en Postman/Headers

---

### **Problema 4: Usuario sin permisos específicos**

Si un usuario NO es Administrador pero necesita acceso a Ventas:

```sql
-- Ver permisos personalizados del usuario
SELECT * FROM UserModulePermissions
WHERE UserId = {id_usuario} AND ModuleId = 2;
```

**Asignar permisos personalizados:**
```http
POST http://localhost:7254/api/Permissions/user/save-custom
Authorization: Bearer {token_admin}
Content-Type: application/json

{
  "userId": 8,
  "modules": [
    {
      "moduleId": 2,
      "hasAccess": true,
      "canView": true,
      "canCreate": true,
      "canEdit": true,
      "canDelete": false,
      "submodules": [
        {
          "submoduleId": 10,
          "hasAccess": true,
          "canView": true,
          "canCreate": true,
          "canEdit": true,
          "canDelete": false
        }
      ]
    }
  ]
}
```

---

## ? **Checklist de Verificación Rápida**

Marca cada ítem cuando lo verifiques:

- [ ] **Módulo de Ventas existe** (SELECT * FROM Modules WHERE Id = 2)
- [ ] **Submódulos existen** (SELECT COUNT(*) FROM Submodules WHERE ModuleId = 2)
- [ ] **Rol Administrador existe** (SELECT * FROM Roles WHERE Id = 1)
- [ ] **Permisos asignados** (SELECT COUNT(*) FROM RoleModulePermissions WHERE RoleId = 1 AND ModuleId = 2)
- [ ] **Usuario es Administrador** (SELECT RoleId FROM Users WHERE Code = 'MI_USUARIO')
- [ ] **Token JWT válido** (No expirado, incluye claims correctos)
- [ ] **Endpoint responde 200** (GET /api/sales retorna lista)
- [ ] **Menú muestra Ventas** (GET /api/Modules/user/{id}/menu)

---

## ?? **Resultado Actual de tu Sistema**

Según la última verificación:

```
? Módulo de Ventas: EXISTE (ID: 2)
? Submódulos de Ventas: 8
? Rol Administrador: EXISTE (ID: 1)
? Permisos asignados: 9
? Usuarios Administradores activos: 2
   - ADMIN001 (admin@sistema.com)
   - FEHE-5771 (elherna22@gmail.com)

?? SISTEMA CORRECTAMENTE CONFIGURADO
```

---

## ?? **Para AWS/Producción**

Cuando migres a AWS, ejecuta el script de verificación después de aplicar los permisos:

```bash
# 1. Aplicar permisos
sqlcmd -S localhost -d ERP -U sa -P "Password" \
  -i AddSalesModulePermissions.sql

# 2. Verificar que se aplicaron correctamente
sqlcmd -S localhost -d ERP -U sa -P "Password" \
  -i VerifySalesModulePermissions.sql
```

---

## ?? **Scripts Relacionados**

| Script | Propósito |
|--------|-----------|
| `AddSalesModulePermissions.sql` | Crear módulo y asignar permisos |
| `VerifySalesModulePermissions.sql` | Verificar configuración completa ? |
| `CreateSalesPaymentsTables.sql` | Crear tablas de ventas |

---

**?? ¡TUS PERMISOS ESTÁN CORRECTAMENTE CONFIGURADOS!** ??

Puedes empezar a usar el módulo de Ventas sin problemas.
