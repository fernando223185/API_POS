# ?? **SOLUCIÓN DEFINITIVA: El Problema de las Migraciones**

## ?? **EL PROBLEMA QUE IDENTIFICASTE**

Tienes toda la razón. El problema es:

```
DESARROLLO (Tu máquina)                    PRODUCCIÓN (AWS)
========================                   =================
Código fuente + Migraciones       ?        Solo DLLs compilados
                                            (con migraciones embebidas)
```

**Cuando publicas:**
1. ? Las migraciones **SÍ van** (embebidas en `Infrastructure.dll`)
2. ? Pero si hay conflictos con la BD, fallarán
3. ? Los archivos `.cs` de migraciones NO van (solo DLLs)

---

## ?? **ESTADO ACTUAL DE TUS MIGRACIONES**

Tienes estas migraciones en tu proyecto:

```
Infrastructure/Migrations/
??? 20251013181709_InitialCreate.cs
??? 20251013184000_RenameUserTableToUsers.cs
??? 20251013200919_UpdateAdminPasswordHash.cs
??? 20251013220056_AddPermissionSystem.cs
??? 20251013223059_AddModulePathIconAndMenuItems.cs
??? 20251013223701_RevertModulePathIconAndMenuItems.cs
??? 20251013224415_RestoreAdminUserAndRoles.cs
??? 20251111193648_AddCustomerERPFieldsClean.cs
??? 20251111194114_RestorePermissionsSystem.cs
??? 20251111194641_RestoreRolesData.cs
??? 20251111195036_RestoreAdminUser.cs
??? 20251111195911_FixUserPasswordHashes.cs
??? 20251111200816_FixDiscountPercentagePrecision.cs
??? 20251111201415_CreateProductManagementSystem.cs
??? 20251111201838_InsertPriceListData.cs
??? 20251119175425_AddAdvancedProductFields.cs
??? 20260106185246_SeedProductCategories.cs
??? 20260303195156_AddUserModulePermissions.cs
??? 20260303230356_AddAppModulesAndSubmodules.cs
??? 20260303234231_RenameToModulesAndSubmodules.cs  ? PROBLEMÁTICA
??? 20260304190142_RemoveOldPermissionsSystem.cs    ? PROBLEMÁTICA
??? POSDbContextModelSnapshot.cs
```

---

## ? **SOLUCIÓN EN 3 PASOS**

### **PASO 1: Eliminar físicamente las migraciones problemáticas**

Desde PowerShell en tu máquina local:

```powershell
# Ir a la carpeta de migraciones
cd C:\Users\PCX\source\repos\API_POS\Infrastructure\Migrations

# Eliminar las migraciones problemáticas
Remove-Item "20260303234231_RenameToModulesAndSubmodules.cs"
Remove-Item "20260303234231_RenameToModulesAndSubmodules.Designer.cs"
Remove-Item "20260304190142_RemoveOldPermissionsSystem.cs"
Remove-Item "20260304190142_RemoveOldPermissionsSystem.Designer.cs"

# Verificar que se eliminaron
dir | Where-Object { $_.Name -like "*Rename*" -or $_.Name -like "*RemoveOld*" }
# Debería retornar vacío
```

**O desde Visual Studio:**
1. Ir a `Infrastructure/Migrations/`
2. Eliminar manualmente los 4 archivos

---

### **PASO 2: Compilar y verificar**

```powershell
# Compilar
dotnet build

# Verificar que no hay errores
# Si hay errores, significa que algo del código depende de esas migraciones
```

---

### **PASO 3: Publicar limpio a AWS**

```powershell
# 1. Publicar (SIN las migraciones problemáticas)
dotnet publish Web.Api -c Release -o ./publish

# 2. Comprimir
Compress-Archive -Path .\publish\* -DestinationPath api-publish.zip -Force

# 3. Subir a AWS
scp -i "C:\ruta\tu-clave.pem" api-publish.zip ubuntu@tu-ec2-ip:/home/ubuntu/
```

---

### **PASO 4: En AWS - Limpiar y recrear BD**

```bash
# Conectarse
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

# Eliminar BD
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C \
  -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"

# Descomprimir nueva API
rm -rf ~/api/publish/*
unzip -o ~/api-publish.zip -d ~/api/publish/
chmod +x ~/api/publish/Web.Api.dll

# Iniciar API
cd ~/api/publish
dotnet Web.Api.dll
```

**? Resultado esperado:**

```
?? Verificando migraciones de base de datos...
??  Hay 18 migraciones pendientes  ? AHORA SIN LAS 2 PROBLEMÁTICAS
?? Aplicando migraciones...

info: Applying migration '20251013181709_InitialCreate'.
info: Applying migration '20251013184000_RenameUserTableToUsers'.
...
info: Applying migration '20260303230356_AddAppModulesAndSubmodules'.
? NO APLICARÁ: 20260303234231_RenameToModulesAndSubmodules
? NO APLICARÁ: 20260304190142_RemoveOldPermissionsSystem

? Migraciones aplicadas exitosamente
? Base de datos actualizada
```

---

## ?? **VERIFICACIÓN**

### **1. Verificar migraciones compiladas**

```powershell
# En tu máquina local
dotnet ef migrations list --project Infrastructure --startup-project Web.Api
```

**? NO deberías ver:**
- `20260303234231_RenameToModulesAndSubmodules`
- `20260304190142_RemoveOldPermissionsSystem`

### **2. Verificar DLL publicado**

```powershell
# Inspeccionar el DLL (opcional)
ildasm publish/Infrastructure.dll
# Buscar referencias a las migraciones eliminadas
```

### **3. Verificar en AWS**

```bash
# Ver migraciones aplicadas
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C \
  -Q "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;"
```

**? NO deberías ver:**
- `20260303234231_RenameToModulesAndSubmodules`
- `20260304190142_RemoveOldPermissionsSystem`

---

## ?? **RESUMEN DE TU OBSERVACIÓN**

### **Lo que dijiste:**

> "Creo que las migraciones se tienen que hacer a partir del código desde 0, pero cuando lo paso al servidor se pasa con todo y las migraciones que ya tengo"

**? EXACTO. El problema es:**

| Situación | Problema |
|-----------|----------|
| **Desarrollo** | Tienes 20 migraciones (incluidas las problemáticas) |
| **Publicas** | El DLL incluye las 20 migraciones embebidas |
| **AWS** | Entity Framework intenta aplicar las 20 |
| **BD en AWS** | Ya tiene cambios hechos con scripts SQL |
| **Resultado** | ? Conflicto: "IDENTITY property can't change" |

### **La solución:**

```
1. Eliminar migraciones problemáticas del CÓDIGO
2. Compilar (DLL ahora solo tiene 18 migraciones válidas)
3. Publicar
4. Limpiar BD en AWS
5. Dejar que EF aplique las 18 migraciones limpias
```

---

## ? **CHECKLIST FINAL**

```
? 1. Eliminar físicamente archivos de migraciones problemáticas
? 2. Compilar sin errores
? 3. Publicar (dotnet publish)
? 4. Verificar que DLL no tiene migraciones problemáticas
? 5. Subir a AWS
? 6. DROP DATABASE ERP
? 7. CREATE DATABASE ERP  
? 8. Descomprimir API nueva
? 9. Iniciar API
? 10. Verificar que aplica solo 18 migraciones ?
```

---

## ?? **ERRORES COMUNES**

### **Error 1: "Las migraciones siguen aplicándose"**

**Causa:** No eliminaste los archivos `.cs` físicamente

**Solución:**
```powershell
# Verificar que NO existen
dir Infrastructure\Migrations | Where-Object { $_.Name -like "*Rename*" }
# Debe retornar vacío
```

### **Error 2: "Errores de compilación después de eliminar"**

**Causa:** Algo del código depende de esas migraciones

**Solución:** Revisar el código y eliminar referencias

### **Error 3: "BD sigue teniendo conflictos"**

**Causa:** No limpiaste la BD antes de aplicar migraciones

**Solución:**
```sql
DROP DATABASE ERP;
CREATE DATABASE ERP;
```

---

## ?? **RESULTADO FINAL**

### **Antes:**

```
? 20 migraciones (incluidas 2 problemáticas)
? Conflictos con BD en AWS
? Scripts SQL causando inconsistencias
```

### **Después:**

```
? 18 migraciones limpias y válidas
? BD creada desde cero por Entity Framework
? Sin conflictos ni inconsistencias
? Sistema funcionando correctamente
```

---

**?? TU OBSERVACIÓN FUE CORRECTA** - El problema estaba en que las migraciones problemáticas se estaban incluyendo en el DLL publicado.

**Solución:** Eliminarlas del código ANTES de publicar.
