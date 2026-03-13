# ? **SOLUCIÓN DEFINITIVA: Limpiar BD y Aplicar Migraciones Desde Cero**

## ?? **LA MEJOR SOLUCIÓN**

En lugar de usar scripts SQL manuales, vamos a:

1. ? **Eliminar la base de datos**
2. ? **Dejar que Entity Framework cree todo desde cero**
3. ? **Las migraciones se aplicarán automáticamente**

**Ventajas:**
- ? Todo controlado por Entity Framework
- ? No hay conflictos de nombres de tablas
- ? Migraciones limpias y consistentes
- ? Funciona igual en desarrollo, AWS y cualquier entorno

---

## ?? **PASOS COMPLETOS**

### **PASO 1: En tu servidor AWS - Eliminar y Recrear BD**

```bash
# Conectarse a tu servidor AWS
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

# Conectarse a SQL Server
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C
```

En `sqlcmd`, ejecutar:

```sql
-- Ver bases de datos existentes
SELECT name FROM sys.databases;
GO

-- Eliminar base de datos ERP (ADVERTENCIA: Borrará todos los datos)
DROP DATABASE ERP;
GO

-- Crear base de datos limpia
CREATE DATABASE ERP;
GO

-- Verificar que se creó
SELECT name FROM sys.databases WHERE name = 'ERP';
GO

-- Salir
EXIT
```

---

### **PASO 2: Detener la API (si está corriendo)**

```bash
# Si está corriendo manualmente, presiona Ctrl+C

# Si está como servicio:
sudo systemctl stop api-pos
```

---

### **PASO 3: Iniciar la API - Migraciones Automáticas**

```bash
cd ~/api/publish
dotnet Web.Api.dll
```

**? Resultado esperado:**

```
?? Verificando migraciones de base de datos...
??  Hay XX migraciones pendientes
?? Aplicando migraciones...

info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (XXms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE TABLE [__EFMigrationsHistory] ...

info: Microsoft.EntityFrameworkCore.Migrations[20402]
      Applying migration '20251013181709_InitialCreate'.
      
info: Microsoft.EntityFrameworkCore.Migrations[20402]
      Applying migration '20251013184000_RenameUserTableToUsers'.
      
... (todas las migraciones)

? Migraciones aplicadas exitosamente
? Base de datos actualizada - No hay migraciones pendientes

?? Verificando roles del sistema...
??  No hay roles en la base de datos, creando roles básicos...
? 8 roles creados exitosamente

?? Verificando usuario administrador...
??  Usuario ADMIN001 no existe, creando...
? Usuario ADMIN001 creado exitosamente
   ?? Email: admin@sistema.com
   ?? Password: admin123

?? Resumen de Base de Datos:
   ?? Usuarios: 1
   ?? Roles: 8
   ?? Módulos: 0

?? ERP POS API Server started successfully
```

---

### **PASO 4: Verificar que Todo Funciona**

#### **Test 1: Login**

```bash
curl -X POST http://localhost:7254/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{"code":"ADMIN001","password":"admin123"}'
```

**? Deberías recibir:**
```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGci..."
}
```

#### **Test 2: Swagger**

Abre en tu navegador:
```
http://tu-ec2-ip:7254/swagger
```

#### **Test 3: Verificar Tablas**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C -Q "SELECT name FROM sys.tables ORDER BY name;"
```

**? Deberías ver:**
```
Customers
Modules                      ? NOMBRE CORRECTO
PriceLists
ProductCategories
ProductImages
Products
ProductSubcategories
RoleModulePermissions
Roles
Sales
Submodules                   ? NOMBRE CORRECTO
Suppliers
UserModulePermissions
Users
```

---

## ?? **QUÉ HACE AUTOMÁTICAMENTE `Program.cs`**

El código en `Program.cs` ya tiene lógica para:

### **1. Aplicar migraciones automáticamente:**

```csharp
var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
if (pendingMigrations.Any())
{
    await context.Database.MigrateAsync();
    Console.WriteLine("? Migraciones aplicadas exitosamente");
}
```

### **2. Crear roles si no existen:**

```csharp
var rolesCount = await context.Roles.CountAsync();
if (rolesCount == 0)
{
    // Crear 8 roles básicos
    context.Roles.AddRange(roles);
    await context.SaveChangesAsync();
}
```

### **3. Crear usuario ADMIN001:**

```csharp
var adminUser = await context.User.FirstOrDefaultAsync(u => u.Code == "ADMIN001");
if (adminUser == null)
{
    // Crear usuario administrador
    context.User.Add(newAdmin);
    await context.SaveChangesAsync();
}
```

---

## ?? **SI NECESITAS HACERLO DESDE TU MÁQUINA LOCAL**

### **Opción A: Conexión Directa a SQL Server en AWS**

```bash
# Asegúrate de que el puerto 1433 esté abierto en el Security Group

# Desde PowerShell en tu máquina local
sqlcmd -S tu-ec2-ip,1433 -U sa -P 'Syst3m2270*' -C
```

```sql
DROP DATABASE ERP;
GO
CREATE DATABASE ERP;
GO
EXIT
```

### **Opción B: Desde Visual Studio (Recomendado para desarrollo)**

1. **Abrir Package Manager Console**
2. **Ejecutar:**

```powershell
# Eliminar base de datos
Drop-Database -Context POSDbContext

# Aplicar todas las migraciones
Update-Database -Context POSDbContext

# Verificar
Get-Migrations -Context POSDbContext
```

---

## ?? **IMPORTANTE: DATOS DE SEED**

Después de recrear la BD, necesitarás datos iniciales:

### **1. Módulos del Sistema**

Ejecutar script portable (opcional, si tu sistema lo requiere):

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -i SeedSystemModules_Portable.sql -C
```

**O mejor:** Crear una migración de seed:

```bash
dotnet ef migrations add SeedInitialData --project Infrastructure --startup-project Web.Api
```

### **2. Permisos del Administrador**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -i AssignFullPermissionsToAdmin_Portable.sql -C
```

---

## ?? **VERIFICACIÓN COMPLETA**

### **1. Ver migraciones aplicadas**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C -Q "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;"
```

### **2. Ver tablas creadas**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C -Q "SELECT name FROM sys.tables ORDER BY name;"
```

### **3. Ver datos iniciales**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C -Q "SELECT Id, Name FROM Roles;"
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C -Q "SELECT Code, Name, Email FROM Users;"
```

---

## ?? **RESUMEN EJECUTIVO**

### **Comandos Rápidos (Copiar y Pegar):**

```bash
# 1. Conectarse a AWS
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

# 2. Eliminar y recrear BD
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"

# 3. Iniciar API (aplicará migraciones automáticamente)
cd ~/api/publish
dotnet Web.Api.dll

# 4. Verificar login
curl -X POST http://localhost:7254/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{"code":"ADMIN001","password":"admin123"}'
```

---

## ? **VENTAJAS DE ESTE MÉTODO**

| Ventaja | Descripción |
|---------|-------------|
| **? Limpio** | Sin conflictos de nombres de tablas |
| **? Consistente** | Igual en todos los entornos |
| **? Automático** | Entity Framework gestiona todo |
| **? Rastreable** | Todas las migraciones en historial |
| **? Reversible** | Puedes hacer rollback si es necesario |
| **? Sin scripts SQL manuales** | Todo desde código |

---

## ?? **LO QUE YA NO NECESITAS**

- ? `AWS_FixMigrations.sql`
- ? `REVERT_TableNames.sql`
- ? Scripts manuales de renombrado
- ? Editar `__EFMigrationsHistory` manualmente

**Todo se gestiona con migraciones de Entity Framework** ?

---

## ?? **CHECKLIST FINAL**

```
? 1. Conectarse a AWS
? 2. DROP DATABASE ERP
? 3. CREATE DATABASE ERP
? 4. Iniciar API (dotnet Web.Api.dll)
? 5. Ver que se aplican todas las migraciones ?
? 6. Ver que se crean roles ?
? 7. Ver que se crea ADMIN001 ?
? 8. Probar login ?
? 9. Probar Swagger ?
? 10. Verificar tablas correctas ?
```

---

## ?? **RESULTADO FINAL**

```
? Base de datos limpia
? Todas las migraciones aplicadas en orden
? Tablas con nombres correctos (Modules, Submodules)
? Roles creados
? Usuario ADMIN001 creado
? Sistema funcionando perfectamente
```

---

**?? ESTE ES EL MÉTODO PROFESIONAL Y RECOMENDADO** - Sin scripts SQL manuales, todo desde Entity Framework
