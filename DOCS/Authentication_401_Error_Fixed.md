# Error 401 "Usuario no autenticado" - SOLUCIONADO

## ?? Problema

Al consumir el endpoint `POST http://localhost:7254/api/PurchaseOrderReceivings` con un token JWT válido en el header de Authorization, se recibe:

```json
{
  "message": "Usuario no autenticado",
  "error": 1
}
```

### Evidencia del Error

**Request Headers enviados:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxIiwi...
Content-Type: application/json
Accept: application/json
```

**Status Code:** `401 Unauthorized`

**Response:**
```json
{
  "message": "Usuario no autenticado",
  "error": 1
}
```

---

## ?? Causa Raíz

El atributo `RequireAuthenticationAttribute` verifica si el usuario está autenticado usando:

```csharp
public class RequireAuthenticationAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(
                new { message = "Usuario no autenticado", error = 1 });
        }
    }
}
```

El problema es que **`context.HttpContext.User.Identity.IsAuthenticated` es `false`** aunque el token JWT está presente y es válido.

### ¿Por qué?

1. **El middleware de autenticación JWT NO se está ejecutando correctamente**
2. **El orden de los middlewares en `Program.cs` puede estar incorrecto**
3. **La configuración JWT puede tener un problema**

---

## ? Solución

### Opción 1: Usar `[Authorize]` en lugar de `[RequireAuthentication]` (RECOMENDADO)

El atributo estándar de ASP.NET Core `[Authorize]` funciona correctamente con JWT:

**Antes:**
```csharp
[HttpGet]
[RequireAuthentication]
public async Task<IActionResult> GetAllReceivings([FromQuery] bool includePosted = true)
{
    // ...
}
```

**Después:**
```csharp
[HttpGet]
[Authorize] // ? Usa el atributo estándar de ASP.NET Core
public async Task<IActionResult> GetAllReceivings([FromQuery] bool includePosted = true)
{
    // ...
}
```

### Opción 2: Corregir `RequireAuthenticationAttribute`

Si prefieres mantener tu atributo personalizado, corrígelo así:

```csharp
public class RequireAuthenticationAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // ? Verificar si el usuario está autenticado
        var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated ?? false;
        
        if (!isAuthenticated)
        {
            // ?? Log para debugging
            Console.WriteLine("? RequireAuthentication: Usuario NO autenticado");
            Console.WriteLine($"   User.Identity: {context.HttpContext.User?.Identity}");
            Console.WriteLine($"   IsAuthenticated: {context.HttpContext.User?.Identity?.IsAuthenticated}");
            
            context.Result = new UnauthorizedObjectResult(new 
            { 
                message = "Usuario no autenticado", 
                error = 1 
            });
            return;
        }

        // ? Log de éxito
        Console.WriteLine("? RequireAuthentication: Usuario autenticado");
        var userId = context.HttpContext.User.FindFirst("userId")?.Value;
        Console.WriteLine($"   UserId: {userId}");
    }
}
```

### Opción 3: Verificar el orden de middlewares en `Program.cs`

El orden **DEBE** ser exactamente este:

```csharp
app.UseRouting();

// ? CORS antes de Authentication
app.UseCors("AllowAll");

// ? Authentication ANTES de Authorization
app.UseAuthentication();
app.UseAuthorization();

// ? Middleware personalizado DESPUÉS de Authentication
app.UseJwtUser();

app.MapControllers();
```

---

## ?? Verificación del Token JWT

### 1. Validar que el token se está recibiendo correctamente

Agrega logs temporales en `JwtUserMiddleware`:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // ?? Log para debugging
    var authHeader = context.Request.Headers["Authorization"].ToString();
    Console.WriteLine($"?? Authorization Header: {authHeader}");
    Console.WriteLine($"?? IsAuthenticated: {context.User.Identity?.IsAuthenticated}");
    
    if (context.User.Identity?.IsAuthenticated == true)
    {
        Console.WriteLine("? Usuario autenticado en middleware");
        
        // Extraer información del usuario del token JWT
        var userIdClaim = context.User.FindFirst("userId")?.Value;
        var userCodeClaim = context.User.FindFirst("userCode")?.Value;
        var userNameClaim = context.User.FindFirst("userName")?.Value;

        Console.WriteLine($"   UserId: {userIdClaim}");
        Console.WriteLine($"   UserCode: {userCodeClaim}");
        Console.WriteLine($"   UserName: {userNameClaim}");

        // Agregar al HttpContext para uso posterior
        if (int.TryParse(userIdClaim, out var userId))
        {
            context.Items["UserId"] = userId;
        }

        context.Items["UserCode"] = userCodeClaim;
        context.Items["UserName"] = userNameClaim;
    }
    else
    {
        Console.WriteLine("? Usuario NO autenticado en middleware");
    }

    await _next(context);
}
```

### 2. Verificar la configuración JWT en `appsettings.json`

```json
{
  "Jwt": {
    "Key": "tu-super-secret-key-de-al-menos-32-caracteres-para-que-sea-seguro",
    "Issuer": "YourAPI",
    "Audience": "YourAPIUsers"
  }
}
```

?? **IMPORTANTE:** La `Key` debe tener **al menos 32 caracteres** (256 bits).

### 3. Verificar que el token sea válido

Decodifica tu token JWT en [jwt.io](https://jwt.io) y verifica:

? El payload debe contener:
```json
{
  "userId": "1",
  "userCode": "ADMIN001",
  "userName": "Administrador",
  "email": "admin@sistema.com",
  "roleId": "1",
  "roleName": "Administrador",
  "exp": 1234567890,
  "iss": "YourAPI",
  "aud": "YourAPIUsers"
}
```

? El `exp` (expiration) no debe estar vencido
? El `iss` (issuer) debe coincidir con `appsettings.json`
? El `aud` (audience) debe coincidir con `appsettings.json`

---

## ?? Implementación Recomendada

### Paso 1: Reemplazar `[RequireAuthentication]` por `[Authorize]`

En **TODOS** los controladores que requieren autenticación:

**Web.Api\Controllers\Purchasing\PurchaseOrderReceivingsController.cs:**
```csharp
using Microsoft.AspNetCore.Authorization; // ? Agregar este using

namespace Web.Api.Controllers.Purchasing
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ? Aplicar a nivel de controlador
    public class PurchaseOrderReceivingsController : ControllerBase
    {
        // Ya no necesitas [RequireAuthentication] en cada método
        
        [HttpGet]
        public async Task<IActionResult> GetAllReceivings([FromQuery] bool includePosted = true)
        {
            // ? El usuario ya está autenticado aquí
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            // ...
        }
    }
}
```

### Paso 2: Acceder al UserId desde el Token

**Antes (usando HttpContext.Items):**
```csharp
var userId = HttpContext.Items["UserId"] as int? ?? 0;
```

**Después (desde Claims del token):**
```csharp
var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
var userName = User.FindFirst("userName")?.Value;
var roleId = int.Parse(User.FindFirst("roleId")?.Value ?? "0");
```

### Paso 3: Mantener `RequirePermission` para permisos específicos

```csharp
[HttpPost]
[RequirePermission("PurchaseOrders", "Create")] // ? Para permisos específicos
public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
{
    // ...
}
```

---

## ?? Comparación de Soluciones

| Aspecto | `[RequireAuthentication]` | `[Authorize]` |
|---------|---------------------------|---------------|
| **Framework** | Custom | ASP.NET Core built-in |
| **Mantenimiento** | Requiere mantener código custom | Mantenido por Microsoft |
| **Compatibilidad JWT** | Puede fallar | ? 100% compatible |
| **Performance** | Similar | Similar |
| **Debugging** | Más difícil | Logs automáticos |
| **Testing** | Requiere setup custom | Fácil de mockear |

**Recomendación:** Usa `[Authorize]` para autenticación simple y `[RequirePermission]` para permisos granulares.

---

## ?? Testing en Postman

### 1. Hacer Login y obtener token

**POST** `http://localhost:7254/api/Login/login`

**Body:**
```json
{
  "code": "ADMIN001",
  "password": "admin123"
}
```

**Response:**
```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2026-03-11T07:02:35.0000000Z",
  "user": {
    "id": 1,
    "code": "ADMIN001",
    "name": "Administrador",
    "email": "admin@sistema.com",
    "active": true,
    "roleId": 1,
    "roleName": "Administrador"
  }
}
```

### 2. Copiar el token

Copia el valor de `token` (sin las comillas).

### 3. Configurar Authorization en Postman

1. Ve a la pestaña **"Authorization"**
2. Selecciona **Type: Bearer Token**
3. Pega el token en el campo **"Token"**

### 4. Hacer la petición protegida

**GET** `http://localhost:7254/api/PurchaseOrderReceivings`

**Headers automáticos:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response esperada:**
```json
{
  "message": "Recepciones obtenidas exitosamente",
  "error": 0,
  "receivings": [],
  "totalReceivings": 0,
  "pendingToPost": 0,
  "posted": 0
}
```

---

## ?? Debugging Checklist

Si aún tienes problemas:

- [ ] Verificar que el token no esté expirado (claim `exp`)
- [ ] Verificar que `Jwt:Key` en `appsettings.json` tenga al menos 32 caracteres
- [ ] Verificar que `Jwt:Issuer` y `Jwt:Audience` coincidan
- [ ] Verificar que el header `Authorization` tenga el formato: `Bearer {token}`
- [ ] Verificar logs de consola para errores de autenticación
- [ ] Verificar que `app.UseAuthentication()` esté ANTES de `app.UseAuthorization()`
- [ ] Probar con el endpoint `/api/Test/protected` para aislar el problema

---

## ?? Archivos Modificados

### Cambios Necesarios

1. **Web.Api\Controllers\Purchasing\PurchaseOrderReceivingsController.cs**
   - Reemplazar `[RequireAuthentication]` por `[Authorize]`
   - Agregar `using Microsoft.AspNetCore.Authorization;`

2. **Web.Api\Controllers\Purchasing\PurchaseOrdersController.cs**
   - Reemplazar `[RequireAuthentication]` por `[Authorize]`

3. **Web.Api\Controllers\Purchasing\SuppliersController.cs**
   - Reemplazar `[RequireAuthentication]` por `[Authorize]`

4. **Todos los demás controladores que usen `[RequireAuthentication]`**

---

## ? Resultado Final

Después de aplicar los cambios:

? Los endpoints protegidos funcionarán correctamente con JWT
? El mensaje de error será claro: `"message": "Authorization header is required"`
? Los logs mostrarán información útil para debugging
? El código será más mantenible usando atributos estándar de ASP.NET Core

---

## ?? Referencias

- [ASP.NET Core JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication)
- [Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization)
- [JWT.io - Token Decoder](https://jwt.io)
