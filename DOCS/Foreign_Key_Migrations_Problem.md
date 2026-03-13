# ?? **PROBLEMA: Foreign Key Constraint al Insertar Permisos**

## ?? **EL ERROR**

```
? Error al aplicar migraciones: The INSERT statement conflicted with the FOREIGN KEY constraint 
   "FK_RolePermissions_Roles_RoleId". The conflict occurred in database "ERP", 
   table "dbo.Roles", column 'Id'.
```

---

## ?? **CAUSA RAÍZ**

La migración `20251111194114_RestorePermissionsSystem.cs` tiene un bug:

```sql
-- ? MAL: Elimina los roles
DELETE FROM [Users];
DELETE FROM [Roles];  -- ? Elimina TODOS los roles (incluyendo ID=1)

-- Luego intenta insertar permisos para un rol que NO EXISTE
INSERT INTO [RolePermissions] ([RoleId], [PermissionId], [CreatedAt])
VALUES (1, @PermissionId, GETUTCDATE());  -- ? FALLA: Rol ID=1 no existe
```

**El orden correcto debería ser:**
1. Eliminar permisos viejos
2. **Crear roles PRIMERO**
3. Crear permisos
4. Asignar permisos a roles

---

## ? **SOLUCIÓN: Eliminar Migraciones Problemáticas**

Tienes varias migraciones que eliminan y recrean roles constantemente:

```
20251013224415_RestoreAdminUserAndRoles.cs      ? Crea roles
20251111193648_AddCustomerERPFieldsClean.cs     ? Elimina roles
20251111194114_RestorePermissionsSystem.cs      ? Intenta insertar sin crear roles primero ?
20251111194641_RestoreRolesData.cs             ? Probablemente crea roles otra vez
```

**Esto es un desastre de migraciones.**

---

## ?? **PASOS PARA SOLUCIONAR**

### **PASO 1: Eliminar Migraciones Problemáticas**

Desde PowerShell en tu máquina local:

```powershell
cd C:\Users\PCX\source\repos\API_POS\Infrastructure\Migrations

# Eliminar migraciones que causan problemas
Remove-Item "20251111194114_RestorePermissionsSystem.cs"
Remove-Item "20251111194114_RestorePermissionsSystem.Designer.cs"
Remove-Item "20251111194641_RestoreRolesData.cs"
Remove-Item "20251111194641_RestoreRolesData.Designer.cs"
Remove-Item "20251111195036_RestoreAdminUser.cs"
Remove-Item "20251111195036_RestoreAdminUser.Designer.cs"

# Verificar que se eliminaron
dir | Where-Object { $_.Name -like "*Restore*" }
```

### **PASO 2: Compilar**

```powershell
dotnet build
```

### **PASO 3: Publicar**

```powershell
dotnet publish Web.Api -c Release -o ./publish
Compress-Archive -Path .\publish\* -DestinationPath api-publish.zip -Force
scp -i "tu-clave.pem" api-publish.zip ubuntu@tu-ec2-ip:/home/ubuntu/
```

### **PASO 4: En AWS - Limpiar BD**

```bash
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

# Limpiar BD
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C \
  -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"

# Descomprimir API
rm -rf ~/api/publish/*
unzip -o ~/api-publish.zip -d ~/api/publish/
chmod +x ~/api/publish/Web.Api.dll

# Iniciar
cd ~/api/publish
dotnet Web.Api.dll
```

---

## ?? **MIGRACIONES QUE QUEDARÁN (LIMPIAS)**

Después de eliminar las problemáticas, quedarán:

```
? 20251013181709_InitialCreate
? 20251013184000_RenameUserTableToUsers
? 20251013200919_UpdateAdminPasswordHash
? 20251013220056_AddPermissionSystem
? 20251013223059_AddModulePathIconAndMenuItems
? 20251013223701_RevertModulePathIconAndMenuItems
? 20251013224415_RestoreAdminUserAndRoles
? 20251111193648_AddCustomerERPFieldsClean
? ELIMINADA: 20251111194114_RestorePermissionsSystem
? ELIMINADA: 20251111194641_RestoreRolesData
? ELIMINADA: 20251111195036_RestoreAdminUser
? 20251111195911_FixUserPasswordHashes
? 20251111200816_FixDiscountPercentagePrecision
? 20251111201415_CreateProductManagementSystem
? 20251111201838_InsertPriceListData
? 20251119175425_AddAdvancedProductFields
? 20260106185246_SeedProductCategories
? 20260303195156_AddUserModulePermissions
? 20260303230356_AddAppModulesAndSubmodules
```

---

## ? **QUÉ PASARÁ DESPUÉS**

1. **Las migraciones crearán la estructura básica**
2. **Program.cs creará los roles si no existen:**

```csharp
var rolesCount = await context.Roles.CountAsync();
if (rolesCount == 0)
{
    var roles = new[]
    {
        new Role { Id = 1, Name = "Administrador", ... },
        new Role { Id = 2, Name = "Usuario", ... },
        // ... más roles
    };
    context.Roles.AddRange(roles);
    await context.SaveChangesAsync();
}
```

3. **Program.cs creará el usuario admin:**

```csharp
var adminUser = await context.User.FirstOrDefaultAsync(u => u.Code == "ADMIN001");
if (adminUser == null)
{
    // Crear admin
}
```

**Todo funcionará correctamente** sin necesidad de migraciones problemáticas.

---

## ?? **RESUMEN**

### **Problema:**
```
Migraciones eliminan roles ? Intentan crear permisos para roles inexistentes ? FK Constraint error
```

### **Solución:**
```
1. Eliminar migraciones problemáticas del código
2. Dejar que Program.cs cree roles y usuarios
3. Publicar limpio
4. DROP/CREATE database en AWS
5. Iniciar API
```

### **Resultado:**
```
? 15 migraciones limpias (sin las 3 problemáticas)
? Roles creados por Program.cs
? Usuario admin creado por Program.cs
? Sin errores FK
```

---

## ?? **COMANDOS RÁPIDOS**

```powershell
# En tu máquina local
cd Infrastructure\Migrations
del *RestorePermissionsSystem*
del *RestoreRolesData*
del *RestoreAdminUser*

dotnet build
dotnet publish Web.Api -c Release -o ./publish
Compress-Archive -Path .\publish\* -DestinationPath api-publish.zip -Force
scp -i "clave.pem" api-publish.zip ubuntu@ec2-ip:/home/ubuntu/
```

```bash
# En AWS
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"
rm -rf ~/api/publish/* && unzip -o ~/api-publish.zip -d ~/api/publish/
cd ~/api/publish && dotnet Web.Api.dll
```

---

**?? ELIMINA ESAS 3 MIGRACIONES Y TODO FUNCIONARÁ**
