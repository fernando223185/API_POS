# ?? **Scripts Portables - Guía de Instalación**

## ?? **Propósito**

Esta carpeta contiene scripts SQL **portables** que funcionan en **cualquier sistema** sin necesidad de modificarlos.

---

## ? **Características de los Scripts Portables**

| Característica | Descripción |
|----------------|-------------|
| **Sin `USE DATABASE`** | Ejecuta en la BD activa, no hardcoded |
| **Verificaciones previas** | Valida que las tablas existen |
| **Funciones universales** | Usa `CURRENT_TIMESTAMP` en lugar de `GETUTCDATE()` |
| **Rollback automático** | Si falla, revierte cambios |
| **Mensajes claros** | Indica éxito o error con detalles |
| **Idempotente** | Puede ejecutarse múltiples veces |

---

## ?? **Scripts Disponibles**

### **1. VerifyDatabaseStructure_Portable.sql**
**Propósito:** Verificar que la base de datos tiene la estructura correcta

**Ejecutar ANTES de cualquier otro script**

```bash
sqlcmd -S [SERVIDOR] -d [BASE_DATOS] -i "VerifyDatabaseStructure_Portable.sql"
```

**Qué verifica:**
- ? Que existen las tablas: `Roles`, `Users`, `Modules`, `Submodules`, `RoleModulePermissions`, `UserModulePermissions`
- ? Que hay roles en la BD
- ? Que hay módulos y submódulos
- ? Integridad referencial
- ? Muestra resumen de datos existentes

---

### **2. SeedSystemModules_Portable.sql**
**Propósito:** Insertar módulos y submódulos del sistema

**Ejecutar si no tienes módulos/submódulos**

```bash
sqlcmd -S [SERVIDOR] -d [BASE_DATOS] -i "SeedSystemModules_Portable.sql"
```

**Qué inserta:**
- ? 8 módulos principales (Inicio, Ventas, Productos, Inventario, Clientes, CFDI, Reportes, Configuración)
- ? 29 submódulos asociados
- ? Usa `CURRENT_TIMESTAMP` en lugar de `GETUTCDATE()`
- ? Verifica que no existan antes de insertar

---

### **3. AssignFullPermissionsToAdmin_Portable.sql**
**Propósito:** Asignar acceso completo al rol Administrador (ID: 1)

**Ejecutar después de tener módulos/submódulos**

```bash
sqlcmd -S [SERVIDOR] -d [BASE_DATOS] -i "AssignFullPermissionsToAdmin_Portable.sql"
```

**Qué hace:**
- ? Verifica que el rol ID 1 existe
- ? Limpia permisos existentes del rol
- ? Asigna acceso completo a todos los módulos
- ? Asigna todas las acciones a todos los submódulos (View, Create, Edit, Delete)
- ? Muestra resumen detallado

---

## ?? **FLUJO COMPLETO DE INSTALACIÓN**

### **Escenario 1: Nueva Base de Datos Limpia**

```bash
# Paso 1: Aplicar migraciones de EF Core
dotnet ef database update --project Infrastructure --startup-project Web.Api

# Paso 2: Verificar estructura
sqlcmd -S localhost -d MiBaseDatos -i "VerifyDatabaseStructure_Portable.sql"

# Paso 3: Insertar módulos y submódulos
sqlcmd -S localhost -d MiBaseDatos -i "SeedSystemModules_Portable.sql"

# Paso 4: Crear rol Administrador (si no existe)
# Opción A: Desde SQL
sqlcmd -S localhost -d MiBaseDatos -Q "INSERT INTO Roles (Id, Name, Description, IsActive) VALUES (1, 'Administrador', 'Acceso completo', 1);"

# Opción B: Desde el sistema (recomendado)
# POST http://localhost:7254/api/Roles
# { "name": "Administrador", "description": "Acceso completo", "isActive": true }

# Paso 5: Asignar permisos al Administrador
sqlcmd -S localhost -d MiBaseDatos -i "AssignFullPermissionsToAdmin_Portable.sql"

# Paso 6: Crear usuario administrador
# POST http://localhost:7254/api/Users
# { "code": "ADMIN001", "name": "Admin", "email": "admin@sistema.com", "roleId": 1, "password": "admin123" }

# ? Listo!
```

---

### **Escenario 2: Migrar a Otro Servidor/Base de Datos**

```bash
# Paso 1: Respaldar BD origen
sqlcmd -S servidorOrigen -d ERPOriginal -Q "BACKUP DATABASE ERPOriginal TO DISK = 'C:\backup.bak';"

# Paso 2: Restaurar en nuevo servidor (opcional)
sqlcmd -S servidorNuevo -Q "RESTORE DATABASE ERPNuevo FROM DISK = 'C:\backup.bak';"

# Paso 3: Verificar estructura en nuevo servidor
sqlcmd -S servidorNuevo -d ERPNuevo -i "VerifyDatabaseStructure_Portable.sql"

# Paso 4: Si faltan módulos, ejecutar seed
sqlcmd -S servidorNuevo -d ERPNuevo -i "SeedSystemModules_Portable.sql"

# Paso 5: Si faltan permisos, asignar
sqlcmd -S servidorNuevo -d ERPNuevo -i "AssignFullPermissionsToAdmin_Portable.sql"

# ? Listo!
```

---

### **Escenario 3: Actualizar Sistema Existente**

Si ya tienes un sistema funcionando y solo quieres actualizar permisos:

```bash
# Paso 1: Verificar estado actual
sqlcmd -S localhost -d ERP -i "VerifyDatabaseStructure_Portable.sql"

# Paso 2: Asignar/Actualizar permisos del Administrador
sqlcmd -S localhost -d ERP -i "AssignFullPermissionsToAdmin_Portable.sql"

# ? Listo!
```

---

## ?? **VERIFICACIÓN POST-INSTALACIÓN**

### **Verificar con SQL:**

```sql
-- Ver permisos del Administrador
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
WHERE rmp.RoleId = 1
ORDER BY m.Id, s.[Order];

-- Contar permisos
SELECT COUNT(*) AS TotalPermisos
FROM RoleModulePermissions
WHERE RoleId = 1;
-- Resultado esperado: 37 permisos (8 módulos + 29 submódulos)
```

### **Verificar con API:**

```bash
# Obtener permisos del rol Administrador
GET http://localhost:7254/api/Roles/1/module-permissions

# Obtener menú del usuario Administrador
GET http://localhost:7254/api/Modules/user/1/menu
```

---

## ?? **IMPORTANTE**

### **Antes de Ejecutar:**

1. ? **Respaldar base de datos**
   ```bash
   BACKUP DATABASE [NombreDB] TO DISK = 'C:\backups\backup.bak';
   ```

2. ? **Verificar conexión**
   ```bash
   sqlcmd -S [servidor] -d [baseDatos] -Q "SELECT @@VERSION;"
   ```

3. ? **Verificar permisos de usuario SQL**
   - Necesitas permisos de `INSERT`, `UPDATE`, `DELETE` en las tablas

### **Después de Ejecutar:**

1. ? **Verificar resultado**
   - Cada script muestra un resumen al final
   - Debe terminar con "? ... exitosamente"

2. ? **Probar con API**
   - Hacer login con usuario administrador
   - Verificar que tiene acceso a todos los módulos

---

## ?? **SOLUCIÓN DE PROBLEMAS**

### **Error: "Tabla X no existe"**

**Causa:** No has ejecutado las migraciones de EF Core

**Solución:**
```bash
dotnet ef database update --project Infrastructure --startup-project Web.Api
```

---

### **Error: "Rol ID 1 no existe"**

**Causa:** No has creado el rol Administrador

**Solución:**
```sql
INSERT INTO Roles (Id, Name, Description, IsActive)
VALUES (1, 'Administrador', 'Acceso completo al sistema', 1);
```

---

### **Error: "No hay módulos en la base de datos"**

**Causa:** No has ejecutado el seed de módulos

**Solución:**
```bash
sqlcmd -S localhost -d [BD] -i "SeedSystemModules_Portable.sql"
```

---

### **Script no muestra mensajes PRINT**

**Causa:** `sqlcmd` no muestra PRINT por defecto

**Solución:**
```bash
sqlcmd -S localhost -d [BD] -i "script.sql" -o "output.txt"
# O usar SQL Server Management Studio (SSMS)
```

---

## ?? **COMPATIBILIDAD**

| Base de Datos | Compatible | Notas |
|---------------|------------|-------|
| **SQL Server 2016+** | ? | 100% compatible |
| **SQL Server 2014** | ? | Compatible (probar CURRENT_TIMESTAMP) |
| **Azure SQL Database** | ? | Compatible |
| **MySQL 8.0+** | ?? | Requiere cambios menores (sintaxis) |
| **PostgreSQL** | ?? | Requiere cambios menores (sintaxis) |

---

## ?? **RESUMEN EJECUTIVO**

| Script | Propósito | Cuándo Ejecutar |
|--------|-----------|-----------------|
| **VerifyDatabaseStructure_Portable.sql** | Verificar estructura | ANTES de cualquier script |
| **SeedSystemModules_Portable.sql** | Insertar módulos | Si no hay módulos |
| **AssignFullPermissionsToAdmin_Portable.sql** | Permisos admin | Después de módulos |

---

**? Scripts portables listos para usar en cualquier sistema** ??

**Ubicación:** `Infrastructure/Scripts/Portable/`

**Documentación completa:** `DOCS/Scripts_Portability_Guide.md`
