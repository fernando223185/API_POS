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
using Infrastructure;
using Infrastructure.Services;
using Web.Api.Configuration;
using Web.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar CORS - configuración actualizada para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevelopment", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",           // React/Angular dev server
                "https://localhost:3000",          // React/Angular dev server HTTPS
                "http://localhost:3001",           // Alternate port
                "https://localhost:3001",          // Alternate port HTTPS
                "http://127.0.0.1:3000",           // IPv4 localhost
                "https://127.0.0.1:3000"           // IPv4 localhost HTTPS
              )
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

builder.Services
    .AddDB(builder.Configuration)
    .AddApplication()
    .AddInfrastructure()
    .AddRepositories()
    .AddScoped<IPermissionService, PermissionService>()
    .AddScoped<IJwtTokenService, JwtTokenService>(); // Agregar servicio JWT

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

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS API V1");
        // Configurar Swagger para JWT
        c.OAuthClientId("swagger");
        c.OAuthAppName("POS API");
        c.OAuthUsePkce();
    });
}

// Habilitar HTTPS y HSTS en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Usar CORS según el entorno
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowDevelopment");
}
else
{
    app.UseCors("Production");
}

//  Middleware de Logging de Peticiones HTTP
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    await next.Invoke();
});

// Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Middleware para extraer información del JWT
app.UseJwtUser();

app.MapControllers();
app.Run();
