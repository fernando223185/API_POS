using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Application;
using Application.Abstractions.Authorization;
using Application.Abstractions.Security;
using Application.Abstractions.CRM;
using Application.Abstractions.Catalogue;
using Application.Abstractions.Login;
using Application.Abstractions.Config;
using Application.Abstractions.Storage;
using Application.Abstractions.Common;
using Application.Abstractions.Purchasing;
using Application.Abstractions.Inventory;
using Application.Abstractions.Documents;  // ✅ NUEVO
using Application.Abstractions.Sales;      // ✅ NUEVO - Sistema de ventas
using Application.Abstractions.Billing;    // ✅ NUEVO - Timbrado CFDI
using Application.Abstractions.Sat;        // ✅ NUEVO - Catálogos SAT
using Application.Common.Services; 
using Infrastructure;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Infrastructure.Persistence;
using Domain.Entities;
using Web.Api.Configuration;
using Web.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURAR KESTREL - HTTP SOLO (SIN HTTPS EN LINUX)
// ============================================
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Escuchar en todas las interfaces (0.0.0.0) en el puerto 7254 (HTTP)
    serverOptions.Listen(IPAddress.Any, 7254);
    
    // Solo configurar HTTPS en Windows (donde tenemos certificado de desarrollo)
    if (OperatingSystem.IsWindows())
    {
        // También escuchar en localhost con HTTPS (solo Windows)
        serverOptions.Listen(IPAddress.Loopback, 7255, listenOptions =>
        {
            listenOptions.UseHttps(); 
        });
        
        Console.WriteLine("🌐 Kestrel configured to listen on:");
        Console.WriteLine("   - http://0.0.0.0:7254 (All interfaces - HTTP)");
        Console.WriteLine("   - https://localhost:7255 (Localhost only - HTTPS - Windows)");
    }
    else
    {
        // En Linux/macOS solo HTTP
        Console.WriteLine("🌐 Kestrel configured to listen on:");
        Console.WriteLine("   - http://0.0.0.0:7254 (All interfaces - HTTP)");
        Console.WriteLine("   - HTTPS disabled (Linux/macOS - no certificate)");
    }
});

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ CONFIGURAR SWAGGER
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ERP POS API",
        Version = "v1",
        Description = "API ERP con sistema de clientes, productos y autenticación JWT"
    });
    
    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ✅ CONFIGURACIÓN CORS - PERMITIR TODO (PARA AWS/DEMO)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Agregar autenticación con JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default-key")),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    });

// ✅ CONFIGURACIÓN DE SERVICIOS
builder.Services
    .AddDatabase(builder.Configuration)
    .AddApplication()
    .AddInfrastructure();

// ✅ REGISTRAR HTTPCLIENTFACTORY
builder.Services.AddHttpClient();

// ✅ REGISTRAR REPOSITORIOS
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
builder.Services.AddScoped<ISystemModuleRepository, SystemModuleRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IPriceListRepository, PriceListRepository>();  // ✅ NUEVO - Listas de Precios
builder.Services.AddScoped<IBranchRepository, BranchRepository>();  // ✅ NUEVO
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();  // ✅ NUEVO
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();  // ✅ NUEVO - Órdenes de Compra
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();  // ✅ NUEVO - Proveedores
builder.Services.AddScoped<IPurchaseOrderReceivingRepository, PurchaseOrderReceivingRepository>();  // ✅ NUEVO - Recepciones
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();  // ✅ NUEVO - Empresas

// ✅ NUEVO: Sistema de ventas con cobranza
builder.Services.AddScoped<Application.Abstractions.Sales.ISaleRepository, SaleRepository>();

// ✅ NUEVO: Sistema de facturación CFDI
builder.Services.AddScoped<Application.Abstractions.Billing.IInvoiceRepository, InvoiceRepository>();

// ✅ NUEVO: Catálogos oficiales del SAT
builder.Services.AddScoped<Application.Abstractions.Sat.ISatCatalogRepository, SatCatalogRepository>();

// ✅ REGISTRAR SERVICIOS ADICIONALES
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICustomerCodeGeneratorService, CustomerCodeGeneratorService>();
builder.Services.AddScoped<IS3StorageService, S3StorageService>();  // ✅ NUEVO - Servicio S3
builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();  // ✅ NUEVO - Generador de códigos centralizado
builder.Services.AddScoped<IInventoryService, InventoryService>();  // ✅ NUEVO - Servicio de inventario
builder.Services.AddScoped<IPurchaseDocumentService, PurchaseDocumentService>();  // ✅ NUEVO - Generación de PDFs
builder.Services.AddScoped<IThermalTicketService, ThermalTicketService>();  // ✅ NUEVO - Tickets térmicos
builder.Services.AddScoped<ISaleDocumentService, SaleDocumentService>();  // ✅ NUEVO - Documentos de venta
builder.Services.AddScoped<IKardexDocumentService, KardexDocumentService>();  // ✅ NUEVO - Documentos de kardex

// ✅ SERVICIO COMPLETO DE SAPIENS (AUTENTICACIÓN Y TIMBRADO CFDI)
builder.Services.AddScoped<Application.Abstractions.Billing.ISapiensService, SapiensService>();

// ✅ CERTIFICADOS SAT (.cer / .key) – extrae NoCertificado, Certificado y genera Sello
builder.Services.AddScoped<Application.Abstractions.Billing.ICertificateService, CertificateService>();

// ✅ REGISTRAR MEDIATR (Handlers automáticos)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly); // ✅ NUEVO - Escanear Infrastructure
});

// Licencia comunitaria de QuestPDF
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var app = builder.Build();

// ============================================
// ✅ EJECUTAR MIGRACIONES AUTOMÁTICAMENTE
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<POSDbContext>();
        
        Console.WriteLine("🔄 Verificando migraciones de base de datos...");
        
        // Verificar si hay migraciones pendientes
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"⚠️  Hay {pendingMigrations.Count()} migraciones pendientes");
            Console.WriteLine("🔄 Aplicando migraciones...");
            
            await context.Database.MigrateAsync();
            
            Console.WriteLine("✅ Migraciones aplicadas exitosamente");
        }
        else
        {
            Console.WriteLine("✅ Base de datos actualizada - No hay migraciones pendientes");
        }

        // ============================================
        // ✅ VERIFICAR Y CREAR ROLES SI NO EXISTEN
        // ============================================
        Console.WriteLine("🔄 Verificando roles del sistema...");
        
        var rolesCount = await context.Roles.CountAsync();
        
        if (rolesCount == 0)
        {
            Console.WriteLine("⚠️  No hay roles en la base de datos, creando roles básicos...");
            
            var roles = new[]
            {
                new Role { Id = 1, Name = "Administrador", Description = "Acceso completo al sistema ERP", IsActive = true },
                new Role { Id = 2, Name = "Usuario", Description = "Acceso básico al sistema", IsActive = true },
                new Role { Id = 3, Name = "Vendedor", Description = "Personal de ventas y atención a clientes", IsActive = true },
                new Role { Id = 4, Name = "Almacenista", Description = "Gestión de inventario y productos", IsActive = true },
                new Role { Id = 5, Name = "Gerente", Description = "Supervisión y reportes", IsActive = true },
                new Role { Id = 6, Name = "Cajero", Description = "Operación de punto de venta", IsActive = true },
                new Role { Id = 7, Name = "Contador", Description = "Gestión fiscal y contable", IsActive = true },
                new Role { Id = 8, Name = "Comprador", Description = "Gestión de compras y proveedores", IsActive = true }
            };
            
            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"✅ {roles.Length} roles creados exitosamente");
        }
        else
        {
            Console.WriteLine($"✅ Roles ya existen en la base de datos ({rolesCount} roles)");
        }

        // ============================================
        // ✅ VERIFICAR Y CREAR USUARIO ADMINISTRADOR
        // ============================================
        Console.WriteLine("🔄 Verificando usuario administrador...");
        
        var adminUser = await context.User.FirstOrDefaultAsync(u => u.Code == "ADMIN001");
        
        if (adminUser == null)
        {
            Console.WriteLine("⚠️  Usuario ADMIN001 no existe, creando...");
            
            // Verificar que el rol Administrador existe
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Id == 1);
            if (adminRole == null)
            {
                Console.WriteLine("❌ Error: Rol Administrador (ID=1) no existe. No se puede crear el usuario.");
            }
            else
            {
                // Hash correcto de "admin123" - el mismo que usan las migraciones
                var passwordHash = new byte[] { 97, 100, 109, 105, 110, 49, 50, 51 }; // "admin123" en bytes
                
                var newAdmin = new User
                {
                    Code = "ADMIN001",
                    Name = "Administrador",
                    Email = "admin@sistema.com",
                    Phone = "1234567890",
                    RoleId = 1,
                    PasswordHash = passwordHash,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                context.User.Add(newAdmin);
                await context.SaveChangesAsync();
                
                Console.WriteLine("✅ Usuario ADMIN001 creado exitosamente");
                Console.WriteLine("   📧 Email: admin@sistema.com");
                Console.WriteLine("   🔑 Password: admin123");
            }
        }
        else
        {
            Console.WriteLine("✅ Usuario ADMIN001 ya existe");
            Console.WriteLine($"   📧 Email: {adminUser.Email}");
            Console.WriteLine($"   👤 Rol: {adminUser.RoleId}");
            Console.WriteLine("   🔑 Password: admin123");
            
            // Verificar que el password hash sea correcto
            var correctHash = new byte[] { 97, 100, 109, 105, 110, 49, 50, 51 };
            if (!adminUser.PasswordHash.SequenceEqual(correctHash))
            {
                Console.WriteLine("⚠️  Password hash incorrecto, actualizando...");
                adminUser.PasswordHash = correctHash;
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Password hash actualizado correctamente");
            }
        }
        
        // ============================================
        // ✅ RESUMEN DE CONFIGURACIÓN
        // ============================================
        var totalUsers = await context.User.CountAsync();
        var totalRoles = await context.Roles.CountAsync();
        
        // ✅ CORREGIDO: Manejar error si la tabla SystemModules no existe
        int totalModules = 0;
        try
        {
            totalModules = await context.Modules.CountAsync();
        }
        catch (Exception)
        {
            // Si falla, intentar contar en la tabla sin validar existencia
            Console.WriteLine("⚠️  No se pudo contar módulos (tabla puede no existir aún)");
        }
        
        Console.WriteLine();
        Console.WriteLine("📊 Resumen de Base de Datos:");
        Console.WriteLine($"   👥 Usuarios: {totalUsers}");
        Console.WriteLine($"   🔐 Roles: {totalRoles}");
        Console.WriteLine($"   📦 Módulos: {totalModules}");
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al aplicar migraciones: {ex.Message}");
        Console.WriteLine($"   Detalles: {ex.InnerException?.Message}");
        Console.WriteLine("⚠️  La API continuará ejecutándose, pero puede tener problemas");
    }
}

// Configurar manejo de errores global
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        var response = new { message = "Ocurrió un error interno en el servidor" };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    });
});

// ✅ SWAGGER (HABILITADO EN PRODUCCIÓN PARA DEMO)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP POS API V1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "ERP POS API - Swagger UI";
    c.DisplayRequestDuration();
});

// ⚠️ NO USAR HTTPS REDIRECT (COMENTADO PARA FACILITAR TESTING EN POSTMAN)
// if (OperatingSystem.IsWindows())
// {
//     app.UseHttpsRedirection();
// }

app.UseRouting();

// ✅ USAR CORS PERMISIVO
app.UseCors("AllowAll");

// Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Middleware para extraer información del JWT
app.UseJwtUser();

app.MapControllers();

// ✅ INFORMACIÓN DE INICIO
Console.WriteLine("🚀 ERP POS API Server started successfully");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"💻 Operating System: {(OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsLinux() ? "Linux" : "macOS")}");
Console.WriteLine();
Console.WriteLine("📡 API Access URLs:");
Console.WriteLine("   - http://localhost:7254 (Local access)");
Console.WriteLine("   - http://0.0.0.0:7254 (All interfaces)");

if (OperatingSystem.IsWindows())
{
    Console.WriteLine("   - https://localhost:7255 (HTTPS - Windows only)");
}
else
{
    Console.WriteLine("   - http://<server-ip>:7254 (Network access - Linux/AWS)");
}

Console.WriteLine();
Console.WriteLine("📚 Swagger UI:");
Console.WriteLine("   - http://localhost:7254/swagger");

if (!OperatingSystem.IsWindows())
{
    Console.WriteLine("   - http://<ec2-public-ip>:7254/swagger");
}

Console.WriteLine();
Console.WriteLine("🔑 Test credentials: ADMIN001 / admin123");
Console.WriteLine();

if (!OperatingSystem.IsWindows())
{
    Console.WriteLine("⚠️  AWS/Linux Notes:");
    Console.WriteLine("   - HTTPS is disabled (no certificate)");
    Console.WriteLine("   - Make sure port 7254 is open in Security Group");
    Console.WriteLine("   - Access via: http://<ec2-public-ip>:7254");
    Console.WriteLine();
}

app.Run();
