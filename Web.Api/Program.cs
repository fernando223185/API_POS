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
using Application;
using Application.Abstractions.Authorization;
using Application.Abstractions.Security;
using Application.Abstractions.CRM;
using Application.Abstractions.Catalogue;
using Application.Abstractions.Sales_Orders;
using Application.Abstractions.Login;
using Application.Abstractions.Config;
using Application.Common.Services; 
using Infrastructure;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Web.Api.Configuration;
using Web.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURAR KESTREL PARA ESCUCHAR EN TODAS LAS INTERFACES
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Escuchar en todas las interfaces (0.0.0.0) en el puerto 7254
    serverOptions.Listen(IPAddress.Any, 7254);
    
    // También escuchar en localhost con HTTPS (si tienes certificado)
    serverOptions.Listen(IPAddress.Loopback, 7255, listenOptions =>
    {
        listenOptions.UseHttps(); 
    });
    
    Console.WriteLine("🌐 Kestrel configured to listen on:");
    Console.WriteLine("   - http://0.0.0.0:7254 (All interfaces - HTTP)");
    Console.WriteLine("   - https://localhost:7255 (Localhost only - HTTPS)");
});

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ CONFIGURAR SWAGGER PARA MÚLTIPLES URLs
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

// ✅ CONFIGURACIÓN CORS MEJORADA PARA DESARROLLO CON IPs DE RED
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevelopment", policy =>
    {
        policy.WithOrigins(
                // React/Next.js dev servers - localhost
                "http://localhost:3000",
                "https://localhost:3000", 
                "http://localhost:3001",
                "https://localhost:3001",
                "http://localhost:3002",
                "https://localhost:3002",
                "http://localhost:3003",
                "https://localhost:3003",
                
                // Vite dev servers - localhost
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5174",
                "https://localhost:5174",
                
                // Angular dev servers - localhost
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:4201",
                "https://localhost:4201",
                
                // IPv4 localhost
                "http://127.0.0.1:3000",
                "https://127.0.0.1:3000",
                "http://127.0.0.1:5173",
                "https://127.0.0.1:5173",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200",
                
                // ✅ TU IP ESPECÍFICA - 192.168.0.72
                "http://192.168.0.72:3000",
                "https://192.168.0.72:3000",
                "http://192.168.0.72:3001",
                "https://192.168.0.72:3001",
                "http://192.168.0.72:5173",
                "https://192.168.0.72:5173",
                "http://192.168.0.72:4200",
                "https://192.168.0.72:4200",
                "http://192.168.0.72:8080",
                "https://192.168.0.72:8080",

                // ✅ NUEVA IP ESPECÍFICA - 192.168.192.57
                "http://192.168.192.57:3000",
                "https://192.168.192.57:3000",
                "http://192.168.192.57:3001",
                "https://192.168.192.57:3001",
                "http://192.168.192.57:5173",
                "https://192.168.192.57:5173",
                "http://192.168.192.57:4200",
                "https://192.168.192.57:4200",
                "http://192.168.192.57:8080",
                "https://192.168.192.57:8080",

                // Puertos adicionales comunes - localhost
                "http://localhost:8080",
                "https://localhost:8080",
                "http://localhost:8081",
                "https://localhost:8081"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });

    // ✅ POLÍTICA PERMISIVA PARA DESARROLLO LOCAL Y RED (usar solo en desarrollo)
    options.AddPolicy("AllowAllLocal", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
        {
            if (string.IsNullOrEmpty(origin)) return false;
            
            var uri = new Uri(origin);
            
            // Permitir localhost
            var isLocalhost = uri.Host == "localhost" || 
                             uri.Host == "127.0.0.1" || 
                             uri.Host == "::1";
            
            // ✅ Permitir IPs de red local (192.168.x.x, 10.x.x.x, 172.16-31.x.x)
            var isPrivateNetwork = uri.Host.StartsWith("192.168.") ||
                                  uri.Host.StartsWith("10.") ||
                                  (uri.Host.StartsWith("172.") && 
                                   int.TryParse(uri.Host.Split('.')[1], out var secondOctet) && 
                                   secondOctet >= 16 && secondOctet <= 31);
            
            var isAllowed = isLocalhost || isPrivateNetwork;
            
            if (isAllowed)
            {
                Console.WriteLine($"🌐 CORS: Allowing origin {origin} (localhost: {isLocalhost}, private: {isPrivateNetwork})");
            }
            
            return isAllowed;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });

    // Política más restrictiva para producción
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://tudominio.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero // Eliminar tolerancia de tiempo
        };

        // Configurar eventos para debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for user: {context.Principal.Identity.Name}");
                return Task.CompletedTask;
            }
        };
    });

// ✅ CONFIGURACIÓN CORRECTA DE SERVICIOS (SIN DUPLICACIONES)
builder.Services
    .AddDatabase(builder.Configuration)             // DbContext como Scoped
    .AddApplication()                               // MediatR y handlers
    .AddInfrastructure();                          // Servicios de infraestructura

// ✅ REGISTRAR REPOSITORIOS MANUALMENTE COMO SCOPED
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISalesRepository, SalesRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();  // ⚡ AGREGAR LoginRepository
builder.Services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();  // ✅ NUEVO: UserPermissionRepository
builder.Services.AddScoped<ISystemModuleRepository, SystemModuleRepository>();  // ✅ NUEVO: SystemModuleRepository

// ✅ REGISTRAR SERVICIOS ADICIONALES
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ✅ REGISTRAR SERVICIO DE GENERACIÓN DE CÓDIGOS
builder.Services.AddScoped<ICustomerCodeGeneratorService, CustomerCodeGeneratorService>();

var app = builder.Build();

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

// ✅ SWAGGER CONFIGURADO PARA MÚLTIPLES URLs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP POS API V1");
        c.RoutePrefix = "swagger"; // Swagger disponible en /swagger
        
        // Configurar Swagger para JWT
        c.OAuthClientId("swagger");
        c.OAuthAppName("ERP POS API");
        c.OAuthUsePkce();
        
        // Información adicional
        c.DocumentTitle = "ERP POS API - Swagger UI";
        c.DisplayRequestDuration();
    });
    
    Console.WriteLine("📚 Swagger UI available at:");
    Console.WriteLine("   - http://localhost:7254/swagger");
    Console.WriteLine("   - http://192.168.192.57:7254/swagger");
}

// ⚠️ DESHABILITAR HTTPS REDIRECT PARA PERMITIR HTTP EN RED
// app.UseHttpsRedirection(); // Comentado para permitir acceso HTTP desde IP de red

// Habilitar HSTS solo en producción con HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRouting();

// ✅ USAR CORS PERMISIVO EN DESARROLLO
if (app.Environment.IsDevelopment())
{
    // Usar política más permisiva para desarrollo
    app.UseCors("AllowAllLocal");
    Console.WriteLine("🌐 CORS: Using AllowAllLocal policy for development");
}
else
{
    app.UseCors("Production");
    Console.WriteLine("🌐 CORS: Using Production policy");
}

// ✅ MIDDLEWARE DE LOGGING CON INFORMACIÓN DE CORS
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    // Log información de CORS y acceso para debugging
    var origin = context.Request.Headers["Origin"].FirstOrDefault();
    var method = context.Request.Method;
    var path = context.Request.Path;
    var host = context.Request.Host.ToString();
    
    if (!string.IsNullOrEmpty(origin))
    {
        logger.LogInformation($"🌐 CORS Request: {method} {path} from origin: {origin} to host: {host}");
    }
    else
    {
        logger.LogInformation($"📡 Direct Request: {method} {path} to host: {host}");
    }
    
    await next.Invoke();
});

// Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Middleware para extraer información del JWT
app.UseJwtUser();

app.MapControllers();

// ✅ INFORMACIÓN DE INICIO MEJORADA
Console.WriteLine("🚀 ERP POS API Server started successfully");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine();
Console.WriteLine("📡 API Access URLs:");
Console.WriteLine("   - http://localhost:7254 (Local access)");
Console.WriteLine("   - http://192.168.192.57:7254 (Network access)");
Console.WriteLine("   - https://localhost:7255 (HTTPS - if certificate available)");
Console.WriteLine();
Console.WriteLine("📚 Swagger UI URLs:");
Console.WriteLine("   - http://localhost:7254/swagger");
Console.WriteLine("   - http://192.168.192.57:7254/swagger");
Console.WriteLine();
Console.WriteLine("📋 Allowed CORS origins in development:");
Console.WriteLine("   - http://localhost:3000-3003 (React/Next.js)");
Console.WriteLine("   - http://localhost:5173-5174 (Vite)");
Console.WriteLine("   - http://localhost:4200-4201 (Angular)");
Console.WriteLine("   - http://localhost:8080-8081 (General)");
Console.WriteLine("   - ✅ http://192.168.0.72:3000-8080 (IP de red 1)");
Console.WriteLine("   - ✅ http://192.168.192.57:3000-8080 (IP de red 2)");
Console.WriteLine("   - All localhost and private network IPs allowed dynamically");
Console.WriteLine();
Console.WriteLine("🔑 Test credentials: ADMIN001 / admin123");

app.Run();
