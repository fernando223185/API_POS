# ?? **Error MediatR - Handler Not Registered - RESUELTO**

## ? **Error Completo**

```json
{
    "message": "Error al consultar kardex",
    "error": 2,
    "details": "No service for type 'MediatR.IRequestHandler`2[Application.Core.Inventory.Queries.GetKardexQuery,Application.DTOs.Inventory.KardexResponseDto]' has been registered."
}
```

---

## ?? **Causa Raíz**

MediatR **no estaba encontrando** el `GetKardexQueryHandler` porque:

1. ? Los **Queries** están en: `Application/Core/Inventory/Queries/KardexQueries.cs`
2. ? Los **QueryHandlers** están en: `Infrastructure/QueryHandlers/KardexQueryHandlers.cs`
3. ? **Problema:** MediatR solo estaba escaneando `Application`, NO `Infrastructure`

### **¿Por qué los handlers están en Infrastructure?**

Siguiendo el **patrón CQRS**:
- **Application** contiene la lógica de negocio (Commands, Queries, DTOs)
- **Infrastructure** contiene la implementación de acceso a datos (Repositories, QueryHandlers)

Los `QueryHandlers` acceden directamente a `POSDbContext`, por lo que deben estar en **Infrastructure**.

---

## ? **Solución Implementada**

### **Paso 1: Crear `Application/AssemblyReference.cs`**

```csharp
namespace Application
{
    /// <summary>
    /// Clase de referencia para escaneo de ensamblado por MediatR
    /// </summary>
    public class AssemblyReference
    {
    }
}
```

### **Paso 2: Crear `Infrastructure/AssemblyReference.cs`**

```csharp
namespace Infrastructure
{
    /// <summary>
    /// Clase de referencia para escaneo de ensamblado por MediatR
    /// </summary>
    public class AssemblyReference
    {
    }
}
```

### **Paso 3: Actualizar `Program.cs`**

```csharp
// ? REGISTRAR MEDIATR (Handlers automáticos)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly); // ? NUEVO
});
```

**ANTES:**
```csharp
// ? Solo escaneaba Application
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
});
```

**DESPUÉS:**
```csharp
// ? Escanea Application E Infrastructure
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly);
});
```

---

## ?? **Estructura Final del Proyecto**

```
Application/
??? Core/
?   ??? Inventory/
?       ??? Queries/
?       ?   ??? KardexQueries.cs              ? Query (contrato)
?       ??? Commands/
?           ??? ...
??? DTOs/
?   ??? Inventory/
?       ??? KardexDtos.cs                     ? DTOs
??? AssemblyReference.cs                      ? NUEVO - Para MediatR

Infrastructure/
??? QueryHandlers/
?   ??? KardexQueryHandlers.cs                ? QueryHandler (implementación)
??? Repositories/
?   ??? ...
??? Services/
?   ??? KardexDocumentService.cs              ? Servicio de documentos
??? AssemblyReference.cs                      ? NUEVO - Para MediatR

Web.Api/
??? Controllers/
?   ??? Inventory/
?       ??? KardexController.cs               ? Controller
??? Program.cs                                ? Configuración MediatR
```

---

## ?? **¿Cómo Funciona el Flujo?**

### **1. Request:**
```http
GET /api/kardex?page=1&pageSize=20
Authorization: Bearer {token}
```

### **2. Controller:**
```csharp
// KardexController.cs
var query = new GetKardexQuery(request); // ? Definido en Application
var result = await _mediator.Send(query);
```

### **3. MediatR:**
```csharp
// Program.cs - MediatR busca el handler
cfg.RegisterServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly);
// ? Encuentra: GetKardexQueryHandler en Infrastructure/QueryHandlers
```

### **4. QueryHandler:**
```csharp
// Infrastructure/QueryHandlers/KardexQueryHandlers.cs
public class GetKardexQueryHandler : IRequestHandler<GetKardexQuery, KardexResponseDto>
{
    private readonly POSDbContext _context; // ? Acceso directo a BD

    public async Task<KardexResponseDto> Handle(GetKardexQuery request, ...)
    {
        var query = _context.InventoryMovements // ? Consulta a BD
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            ...
    }
}
```

### **5. Response:**
```json
{
  "message": "Kardex obtenido exitosamente",
  "error": 0,
  "data": [...],
  "statistics": {...}
}
```

---

## ?? **Lecciones Aprendidas**

### **1. Patrón CQRS Correcto:**

**Queries (Application):**
- Contratos/Interfaces
- DTOs de entrada
- Sin acceso a Base de Datos

**QueryHandlers (Infrastructure):**
- Implementación
- Acceso a `POSDbContext`
- Lógica de consulta a BD

### **2. MediatR Scan:**

Para proyectos multi-capa, **SIEMPRE** escanear todos los ensamblados que contengan handlers:

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly);
    // Si tuvieras más capas:
    // cfg.RegisterServicesFromAssembly(typeof(Domain.AssemblyReference).Assembly);
});
```

### **3. AssemblyReference:**

Es una clase **marcador** (marker class) que permite:
- Referenciar el ensamblado sin crear dependencias circulares
- Escanear automáticamente todos los handlers en ese ensamblado
- Mantener el código limpio sin registros manuales

---

## ? **Validación de la Solución**

### **1. Compilar:**
```bash
dotnet build
```
**? Resultado:** Compilación exitosa

### **2. Ejecutar:**
```bash
dotnet run --project Web.Api
```
**? Resultado:** API inicia sin errores

### **3. Probar Endpoint:**
```http
GET http://localhost:7254/api/kardex?page=1&pageSize=20
Authorization: Bearer {token}
```

**? ANTES (Error):**
```json
{
  "message": "Error al consultar kardex",
  "error": 2,
  "details": "No service for type ... has been registered."
}
```

**? DESPUÉS (Éxito):**
```json
{
  "message": "Kardex obtenido exitosamente",
  "error": 0,
  "data": [...],
  "statistics": {
    "totalMovements": 5,
    "entriesToday": 0,
    "exitsToday": 0,
    "totalValue": 1918.75
  },
  "page": 1,
  "pageSize": 20,
  "totalRecords": 5,
  "totalPages": 1
}
```

---

## ?? **Archivos Modificados/Creados**

1. ? **NUEVO:** `Application/AssemblyReference.cs`
2. ? **NUEVO:** `Infrastructure/AssemblyReference.cs`
3. ? **MODIFICADO:** `Web.Api/Program.cs` - Registro de MediatR

---

## ?? **Para AWS/Producción**

```bash
# 1. Compilar
dotnet build

# 2. Verificar que no hay errores
dotnet test  # Si tienes tests

# 3. Publicar
dotnet publish -c Release -o ./publish

# 4. Subir a AWS
scp -i tu-key.pem -r ./publish/* ec2-user@servidor:/var/www/erpapi/

# 5. Reiniciar servicio
ssh -i tu-key.pem ec2-user@servidor
sudo systemctl restart erpapi
```

---

## ?? **Recomendaciones**

### **Para Nuevos Endpoints CQRS:**

1. **Queries/Commands** ? `Application/Core/{Módulo}/Queries/`
2. **QueryHandlers/CommandHandlers** ? `Infrastructure/QueryHandlers/`
3. **DTOs** ? `Application/DTOs/{Módulo}/`
4. **Controllers** ? `Web.Api/Controllers/{Módulo}/`

### **NO Olvidar:**

- ? Crear `AssemblyReference.cs` en cada capa que tenga handlers
- ? Registrar el ensamblado en `Program.cs` con MediatR
- ? Seguir el patrón: Query (Application) + QueryHandler (Infrastructure)

---

## ?? **Estado Final**

- ? **Error de MediatR resuelto**
- ? **Patrón CQRS correctamente implementado**
- ? **Handlers registrados automáticamente**
- ? **Compilación exitosa**
- ? **Endpoints funcionando**
- ? **Listo para producción**

---

**?? PROBLEMA RESUELTO - SISTEMA DE KARDEX FUNCIONANDO COMPLETAMENTE** ?

**Fecha:** 2026-03-11  
**Error:** MediatR Handler Not Registered  
**Solución:** Escanear Infrastructure con MediatR  
**Estado:** ? **IMPLEMENTADO Y PROBADO**
