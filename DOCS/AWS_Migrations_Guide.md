# ?? **GUÍA: Ejecutar Migraciones en AWS**

## ? **SOLUCIÓN IMPLEMENTADA: Migraciones Automáticas**

La API ahora ejecuta las migraciones automáticamente al iniciar.

---

## ?? **Opción 1: Migraciones Automáticas (RECOMENDADO - YA CONFIGURADO)**

### **Ventajas:**
- ? Se ejecuta automáticamente
- ? No requiere comandos adicionales
- ? Actualiza la BD cada vez que inicias la API
- ? Muestra progreso en consola

### **Cómo Funciona:**

Al iniciar la API, verás esto en la consola:

```
?? Verificando migraciones de base de datos...
??  Hay 15 migraciones pendientes
?? Aplicando migraciones...
? Migraciones aplicadas exitosamente

?? ERP POS API Server started successfully
?? Environment: Production
?? Operating System: Linux
```

### **Despliegue:**

```bash
# 1. Subir nuevo ZIP
scp -i tu-key.pem API_ERP_POS_AWS_AutoMigrate_20260306_164426.zip ubuntu@<ec2-ip>:~/

# 2. En EC2, descomprimir y ejecutar
ssh -i tu-key.pem ubuntu@<ec2-ip>
cd ~/api
rm -rf publish
unzip ~/API_ERP_POS_AWS_AutoMigrate_20260306_164426.zip -d publish
cd publish
dotnet Web.Api.dll

# ? Las migraciones se ejecutarán automáticamente
```

---

## ??? **Opción 2: Ejecutar Migraciones Manualmente con dotnet ef**

Si prefieres control manual sobre cuándo se ejecutan las migraciones:

### **Requisitos:**

```bash
# Instalar herramientas EF Core (EN EC2)
dotnet tool install --global dotnet-ef

# Verificar instalación
dotnet ef --version
```

### **Ejecutar Migraciones:**

```bash
# Navegar a la carpeta de la API
cd ~/api/publish

# Ver migraciones pendientes
dotnet ef database update --verbose

# O especificar una migración específica
dotnet ef database update [NombreMigracion]

# Ver historial de migraciones
dotnet ef migrations list
```

### **Ejemplos:**

```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update

# Aplicar hasta una migración específica
dotnet ef database update AddCustomerERPFieldsClean

# Revertir a una migración anterior
dotnet ef database update PreviousMigrationName

# Ver estado actual
dotnet ef migrations list
```

---

## ?? **Opción 3: Ejecutar Scripts SQL Directamente**

Si tienes acceso directo a SQL Server desde AWS:

### **Desde Linux (usando sqlcmd):**

```bash
# Instalar sqlcmd (si no está instalado)
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list
sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev

# Agregar sqlcmd al PATH
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
source ~/.bashrc

# Ejecutar scripts SQL portables
cd ~/api/scripts
sqlcmd -S <server-ip>,1433 -U sa -P <password> -d ERP -i SeedSystemModules_Portable.sql
sqlcmd -S <server-ip>,1433 -U sa -P <password> -d ERP -i AssignFullPermissionsToAdmin_Portable.sql
```

### **Desde Windows (remoto a BD en AWS):**

```powershell
# Si tu SQL Server está en AWS también
sqlcmd -S <rds-endpoint>,1433 -U admin -P <password> -d ERP -i "SeedSystemModules_Portable.sql"
```

---

## ?? **Verificar que las Migraciones se Aplicaron**

### **Desde la API:**

```bash
# Ver tabla de migraciones en la base de datos
# Ejecutar este query en SQL Server:
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;
```

### **Desde Swagger:**

```
1. Abrir: http://<ec2-ip>:7254/swagger
2. Probar endpoint: GET /api/Modules
3. Si devuelve datos, las migraciones funcionaron
```

### **Verificar tablas creadas:**

```sql
-- Ver todas las tablas creadas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Verificar datos de ejemplo
SELECT COUNT(*) FROM Modules;        -- Debe tener 8 módulos
SELECT COUNT(*) FROM Submodules;     -- Debe tener 29 submódulos
SELECT COUNT(*) FROM Roles;          -- Debe tener roles
SELECT COUNT(*) FROM Users;          -- Debe tener usuario admin
```

---

## ?? **SOLUCIÓN DE PROBLEMAS**

### **Problema: Migraciones fallan con error de conexión**

```bash
# Verificar connection string en appsettings.json
cat ~/api/publish/appsettings.json

# Probar conexión a SQL Server
sqlcmd -S <server-ip>,1433 -U sa -P <password> -Q "SELECT @@VERSION"
```

**Solución:**
```bash
# Actualizar connection string
nano ~/api/publish/appsettings.json

# Cambiar:
"ConnectionStrings": {
  "DefaultConnection": "Server=<TU-IP-BD>,1433;Database=ERP;User Id=sa;Password=<TU-PASSWORD>;TrustServerCertificate=True;"
}
```

---

### **Problema: Error "Login failed for user"**

**Causa:** Usuario SQL Server no tiene permisos

**Solución:**
```sql
-- En SQL Server, ejecutar:
USE master;
GO

ALTER LOGIN sa ENABLE;
GO

ALTER LOGIN sa WITH PASSWORD = '<nueva-password>';
GO

USE ERP;
GO

-- Dar permisos al usuario
ALTER ROLE db_owner ADD MEMBER sa;
GO
```

---

### **Problema: Error "Cannot open database ERP"**

**Causa:** La base de datos no existe

**Solución:**
```sql
-- Crear base de datos manualmente
CREATE DATABASE ERP;
GO

-- Luego ejecutar la API, las migraciones crearán las tablas
```

---

## ?? **Comparación de Opciones**

| Opción | Ventajas | Desventajas | Recomendado Para |
|--------|----------|-------------|------------------|
| **Automático** | No requiere comandos extras, simple | Se ejecuta siempre al iniciar | ? Producción, Demo |
| **dotnet ef** | Control manual, versionado | Requiere instalar herramientas | Desarrollo, Debugging |
| **Scripts SQL** | Funciona sin .NET, portable | Requiere acceso a SQL Server | Migración entre servidores |

---

## ? **RESUMEN**

### **Tu API Ahora:**

1. ? **Detecta** migraciones pendientes al iniciar
2. ? **Aplica** automáticamente las migraciones
3. ? **Muestra** progreso en consola
4. ? **Continúa** ejecutándose incluso si hay error en migraciones

### **Al Desplegar:**

```bash
# Solo necesitas:
1. Subir ZIP
2. Descomprimir
3. Ejecutar dotnet Web.Api.dll
4. ? Las migraciones se ejecutan automáticamente
```

---

## ?? **Logs de Ejemplo**

### **Primera Ejecución (con migraciones pendientes):**

```
?? Verificando migraciones de base de datos...
??  Hay 15 migraciones pendientes
?? Aplicando migraciones...
   - Aplicando: 20251111193648_AddCustomerERPFieldsClean
   - Aplicando: 20251111194114_RestorePermissionsSystem
   - Aplicando: 20251111195911_FixUserPasswordHashes
   - ...
? Migraciones aplicadas exitosamente

?? ERP POS API Server started successfully
?? Environment: Production
?? Operating System: Linux
```

### **Ejecuciones Posteriores (BD actualizada):**

```
?? Verificando migraciones de base de datos...
? Base de datos actualizada - No hay migraciones pendientes

?? ERP POS API Server started successfully
?? Environment: Production
?? Operating System: Linux
```

---

**? MIGRACIONES AUTOMÁTICAS CONFIGURADAS** ??

**Archivo:** `API_ERP_POS_AWS_AutoMigrate_20260306_164426.zip`  
**Tamaño:** 7.15 MB  
**Listo para:** AWS EC2, Azure, IIS, Linux
