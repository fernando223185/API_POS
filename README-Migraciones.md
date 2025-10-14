# 📋 Guía de Migraciones - Entity Framework Core

## 🚀 Configuración Inicial

### Instalar herramientas EF Core
```bash
# Herramientas globales
dotnet tool install --global dotnet-ef

# Paquetes necesarios en el proyecto Web.Api
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.EntityFrameworkCore.Design

# En Visual Studio - Consola del Administrador de paquetes
Install-Package Microsoft.EntityFrameworkCore.SqlServer -Version 7.0.20
```

### Verificar instalación
```bash
dotnet ef --version
```

## 📁 Estructura del Proyecto
```
📁 API_POS/
├── 📄 API_POS.sln               ← Archivo de solución
├── 📁 Web.Api/
│   ├── 📄 appsettings.json      ← Cadena de conexión
│   ├── 📄 Program.cs            ← Configuración de servicios
│   └── 📁 Configuration/
│       └── DependencyInjection.cs ← Configuración EF
├── 📁 Infrastructure/
│   ├── 📁 Persistence/
│   │   └── POSDbContext.cs      ← DbContext principal
│   ├── 📁 Migrations/           ← Archivos de migración (auto-generados)
│   └── 📁 Repositories/         ← Repositorios
├── 📁 Domain/
│   └── 📁 Entities/             ← Entidades del modelo
└── 📁 Application/              ← Lógica de aplicación
```

## 🔧 Comandos Principales

### 1. Crear Nueva Migración
```bash
# Consola del Administrador de paquetes (Visual Studio)
# Proyecto predeterminado: Infrastructure
Add-Migration NombreDeLaMigracion

# Terminal (dotnet CLI) - desde carpeta raíz de la solución
dotnet ef migrations add NombreDeLaMigracion --project Infrastructure --startup-project Web.Api

# Ejemplos de nombres descriptivos
Add-Migration InitialCreate
Add-Migration AddProductTable
Add-Migration UpdateUserEmailConstraints
```

### 2. Aplicar Migraciones
```bash
# Consola del Administrador de paquetes
Update-Database

# Terminal (dotnet CLI)
dotnet ef database update --project Infrastructure --startup-project Web.Api

# Aplicar con información detallada
dotnet ef database update --verbose --project Infrastructure --startup-project Web.Api
```

### 3. Aplicar a Migración Específica
```bash
# Ir a una migración específica
Update-Database 20251013181709_InitialCreate

# dotnet CLI
dotnet ef database update 20251013181709_InitialCreate --project Infrastructure --startup-project Web.Api

# Ir a la migración anterior (rollback)
Update-Database PreviousMigrationName
```

### 4. Listar Migraciones
```bash
# Ver todas las migraciones (aplicadas y pendientes)
dotnet ef migrations list --project Infrastructure --startup-project Web.Api

# Ver solo migraciones pendientes
dotnet ef migrations list --no-connect --project Infrastructure --startup-project Web.Api
```

### 5. Eliminar Última Migración
```bash
# Consola del Administrador de paquetes
Remove-Migration

# Terminal (dotnet CLI)
dotnet ef migrations remove --project Infrastructure --startup-project Web.Api

# Forzar eliminación (si ya fue aplicada)
dotnet ef migrations remove --force --project Infrastructure --startup-project Web.Api
```

### 6. Revertir Base de Datos
```bash
# Volver a migración anterior específica
Update-Database NombreMigracionAnterior

# Revertir TODAS las migraciones (base de datos vacía)
Update-Database 0

# dotnet CLI
dotnet ef database update 0 --project Infrastructure --startup-project Web.Api
```

### 7. Generar Script SQL
```bash
# Generar script para TODAS las migraciones
dotnet ef migrations script --project Infrastructure --startup-project Web.Api --output migration.sql

# Script desde migración específica hasta la más reciente
dotnet ef migrations script InitialCreate --project Infrastructure --startup-project Web.Api

# Script entre dos migraciones específicas
dotnet ef migrations script InitialCreate AddProductTable --project Infrastructure --startup-project Web.Api

# Script solo para migración específica
dotnet ef migrations script PreviousMigration CurrentMigration --project Infrastructure --startup-project Web.Api
```

## 🗄️ Configuración de Base de Datos

### Cadena de Conexión (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ERP;User Id=sa;Password=Syst3m2270*;Trusted_Connection=false;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "TuClaveSecretaMuyLargaYSegura123456789",
    "Issuer": "TuAplicacion",
    "Audience": "TuAplicacion"
  }
}
```

### Configuración del DbContext (Web.Api/Configuration/DependencyInjection.cs)
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddDB(
        this IServiceCollection services,
        IConfiguration configuration) 
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<POSDbContext>(options => 
            options.UseSqlServer(connectionString, b => 
                b.MigrationsAssembly("Infrastructure")));
        return services;
    }
}
```

## 📊 Datos Iniciales (Seed Data)

### En el DbContext (Infrastructure/Persistence/POSDbContext.cs)
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configurar precisión decimal
    modelBuilder.Entity<Sales>()
        .Property(s => s.Importe)
        .HasPrecision(18, 2);

    // Configurar relaciones
    modelBuilder.Entity<User>()
        .HasOne(u => u.Role)
        .WithMany(r => r.Users)
        .HasForeignKey(u => u.RoleId);

    // Datos por defecto (seed data)
    modelBuilder.Entity<Role>().HasData(
        new Role { Id = 1, Name = "Administrador", Description = "Acceso completo al sistema" },
        new Role { Id = 2, Name = "Usuario", Description = "Acceso básico al sistema" }
    );

    modelBuilder.Entity<User>().HasData(
        new User
        {
            Id = 1,
            Code = "ADMIN001",
            Name = "Administrador",
            PasswordHash = System.Text.Encoding.UTF8.GetBytes("admin123"),
            Email = "admin@sistema.com",
            Phone = "1234567890",
            RoleId = 1,
            Active = true,
            CreatedAt = DateTime.UtcNow
        }
    );
}
```

## 🛠️ Solución de Problemas Comunes

### Error: "No se puede crear DbContext"
```bash
# Soluciones:
# 1. Verificar que el proyecto predeterminado sea Infrastructure en Visual Studio
# 2. En CLI, siempre usar --startup-project Web.Api
# 3. Verificar que Web.Api esté como proyecto de inicio

dotnet ef migrations add TestMigration --project Infrastructure --startup-project Web.Api
```

### Error: "Assembly no coincide"
```csharp
// Agregar en Web.Api/Configuration/DependencyInjection.cs
services.AddDbContext<POSDbContext>(options => 
    options.UseSqlServer(connectionString, b => 
        b.MigrationsAssembly("Infrastructure"))); // ← Importante!
```

### Error: "Versiones incompatibles de EF Core"
```xml
<!-- Verificar versiones en todos los proyectos -->
<!-- Domain.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="7.0.20" />

<!-- Infrastructure.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="7.0.20" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.20" />

<!-- Web.Api.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.20" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.20" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7.0.20" />
```

### Error: "Conexión a base de datos"
- ✅ Verificar cadena de conexión en `appsettings.json`
- ✅ Comprobar que SQL Server esté ejecutándose
- ✅ Validar credenciales de usuario/contraseña
- ✅ Verificar que la base de datos existe (se creará automáticamente si no existe)
- ✅ Probar conexión con SQL Server Management Studio

### Error: "Precision warnings para decimales"
```csharp
// Agregar en OnModelCreating
modelBuilder.Entity<Sales>()
    .Property(s => s.Importe)
    .HasPrecision(18, 2);

modelBuilder.Entity<Products>()
    .Property(p => p.price)
    .HasPrecision(18, 2);
```

## 🔄 Flujo de Trabajo Recomendado

### Para Nuevos Desarrolladores
1. **Clonar repositorio**
```bash
   git clone <url-repositorio>
   cd API_POS
```

2. **Restaurar paquetes**
```bash
   dotnet restore
   dotnet build
```

3. **Configurar cadena de conexión local**
- Editar `Web.Api/appsettings.json`
- Actualizar servidor, base de datos y credenciales

4. **Aplicar migraciones**
```bash
   dotnet ef database update --project Infrastructure --startup-project Web.Api
```

5. **¡Listo para trabajar!**
- Base de datos creada con tablas y datos iniciales
- Usuario admin: admin@sistema.com / admin123

### Para Cambios en Modelo
1. **Modificar entidades** en `Domain/Entities`
2. **Actualizar DbContext** si es necesario (relaciones, configuraciones)
3. **Crear migración**
```bash
   Add-Migration DescripcionDelCambio
```
4. **Revisar archivos generados** en `Infrastructure/Migrations`
5. **Aplicar migración**
```bash
   Update-Database
```
6. **Probar cambios** localmente
7. **Commit y push** de archivos de migración

### Para Producción
```bash
# Generar script SQL para DBA
dotnet ef migrations script --project Infrastructure --startup-project Web.Api --output ProductionMigration.sql

# Script desde última migración aplicada
dotnet ef migrations script LastAppliedMigration --project Infrastructure --startup-project Web.Api --output IncrementalUpdate.sql
```

## ⚠️ Buenas Prácticas

### ✅ Hacer Siempre
- **Backup completo** antes de aplicar migraciones en producción
- **Nombres descriptivos** para las migraciones (AddUserTable, UpdateProductConstraints)
- **Revisar código SQL generado** antes de aplicar
- **Configurar precisión decimal** para propiedades monetarias (`HasPrecision(18, 2)`)
- **Probar migraciones** en ambiente de desarrollo primero
- **Incluir seed data** para datos maestros (roles, configuraciones)

### ❌ Nunca Hacer
- **Modificar migraciones** ya aplicadas en producción
- **Eliminar migraciones** aplicadas sin plan de rollback
- **Aplicar migraciones** directamente en producción sin testing
- **Hardcodear contraseñas** en seed data de producción
- **Ignorar warnings** de Entity Framework

### 🔧 Configuraciones Recomendadas
```csharp
// Configurar índices
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();

// Configurar restricciones
modelBuilder.Entity<User>()
    .Property(u => u.Code)
    .IsRequired()
    .HasMaxLength(100);

// Configurar cascada de eliminación
modelBuilder.Entity<User>()
    .HasOne(u => u.Role)
    .WithMany(r => r.Users)
    .HasForeignKey(u => u.RoleId)
    .OnDelete(DeleteBehavior.Restrict);
```

## 📋 Lista de Verificación

### Antes de Commit
- [ ] Migración creada correctamente
- [ ] Migración probada localmente con `Update-Database`
- [ ] Datos de prueba funcionando
- [ ] Sin warnings de EF Core en consola
- [ ] Archivos de migración incluidos en commit
- [ ] Nombres de migración descriptivos

### Antes de Deploy a Producción
- [ ] Script SQL generado y revisado
- [ ] Backup completo de base de datos realizado
- [ ] Plan de rollback preparado y documentado
- [ ] Testing completo en ambiente de staging
- [ ] Validación de performance con datos reales
- [ ] Coordinación con DBA si es necesario

### Después de Deploy
- [ ] Verificar que migraciones se aplicaron correctamente
- [ ] Validar integridad de datos
- [ ] Probar funcionalidades críticas
- [ ] Monitorear logs por errores

## 🆘 Comandos de Emergencia

### Rollback Completo (¡PELIGROSO!)
```bash
# Revertir TODAS las migraciones
Update-Database 0

# Eliminar todas las migraciones (solo si no están en producción)
Remove-Migration

# Recrear desde cero
Add-Migration RecreateDatabase
Update-Database
```

### Forzar Nueva Migración
```bash
# Si hay conflictos en el estado del modelo
Add-Migration ResetMigration --force

# Marcar migración como aplicada sin ejecutar SQL
dotnet ef database update MigrationName --connection "ConnectionString" --no-build
```

### Reparar Estado Inconsistente
```sql
-- Ver qué migraciones están aplicadas realmente
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;
```

```bash
# Generar snapshot del estado actual
dotnet ef migrations add FixSnapshot --project Infrastructure --startup-project Web.Api
dotnet ef migrations remove --project Infrastructure --startup-project Web.Api
```

### Migración Manual de Emergencia
```sql
-- Si necesitas aplicar cambios manuales
-- 1. Hacer el cambio en SQL Server
ALTER TABLE Users ADD NewColumn NVARCHAR(100);
```

```bash
-- 2. Crear migración vacía
dotnet ef migrations add EmptyMigration --project Infrastructure --startup-project Web.Api

-- 3. Editar el archivo de migración para que no haga nada
-- Up() method: // Manual change already applied
-- Down() method: migrationBuilder.DropColumn("NewColumn", "Users");
```

## 📞 Contacto y Soporte

Para problemas específicos:
1. Revisar logs de Entity Framework
2. Verificar documentación oficial: https://docs.microsoft.com/ef/core/
3. Consultar con el equipo de desarrollo

---
**Última actualización**: Octubre 2025  
**Versión EF Core**: 7.0.20  
**Compatibilidad**: .NET 7.0 / .NET 8.0