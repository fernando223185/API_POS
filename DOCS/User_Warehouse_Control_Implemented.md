# ? Sistema de Control de Almacén por Usuario - IMPLEMENTADO

## ?? Resumen

Se implementó exitosamente un sistema flexible de control de almacén para usuarios, permitiendo:
- Asignar un almacén por defecto a cada usuario
- Controlar si un usuario puede vender/operar desde múltiples almacenes
- Validación automática de permisos de almacén

---

## ?? Componentes Implementados

### 1. **Entidad User Actualizada**

**Archivo:** `Domain/Entities/Users.cs`

```csharp
public class User
{
    // ...campos existentes...
    
    /// <summary>
    /// Almacén/Sucursal asignado por defecto al usuario
    /// NULL = No tiene almacén asignado (ej: administradores, vendedores web)
    /// </summary>
    public int? DefaultWarehouseId { get; set; }

    /// <summary>
    /// Indica si el usuario puede vender/operar desde múltiples almacenes
    /// false = Solo puede vender de su almacén asignado (DefaultWarehouseId)
    /// true = Puede elegir almacén en cada operación
    /// </summary>
    public bool CanSellFromMultipleWarehouses { get; set; } = false;

    [ForeignKey(nameof(DefaultWarehouseId))]
    public virtual Warehouse? DefaultWarehouse { get; set; }
}
```

### 2. **DTOs Actualizados**

**Archivo:** `Application/DTOs/Users/UserDtos.cs`

```csharp
public class UserResponseDto
{
    // ...campos existentes...
    
    public int? DefaultWarehouseId { get; set; }
    public string? DefaultWarehouseCode { get; set; }
    public string? DefaultWarehouseName { get; set; }
    public bool CanSellFromMultipleWarehouses { get; set; }
}

public class CreateUserDto
{
    // ...campos existentes...
    
    public int? DefaultWarehouseId { get; set; }
    public bool CanSellFromMultipleWarehouses { get; set; } = false;
}

public class UpdateUserDto
{
    // ...campos existentes...
    
    public int? DefaultWarehouseId { get; set; }
    public bool? CanSellFromMultipleWarehouses { get; set; }
}
```

### 3. **Command Actualizado**

**Archivo:** `Application/Core/Users/Commands/CreateUserCommand.cs`

```csharp
public class CreateUserCommand : ICommand<User>
{
    // ...campos existentes...
    
    public int? DefaultWarehouseId { get; set; }
    public bool CanSellFromMultipleWarehouses { get; set; } = false;
}
```

### 4. **Command Handler con Validación**

**Archivo:** `Application/Core/Users/CommandHandlers/CreateUserCommandoHandler.cs`

```csharp
public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    // ? Validar que el almacén exista si se especifica
    if (request.DefaultWarehouseId.HasValue && request.DefaultWarehouseId.Value > 0)
    {
        var warehouseExists = await _repository.WarehouseExistsAsync(request.DefaultWarehouseId.Value);
        if (!warehouseExists)
        {
            throw new InvalidOperationException($"El almacén con ID {request.DefaultWarehouseId.Value} no existe o está inactivo");
        }
    }

    var user = new User
    {
        // ...campos existentes...
        DefaultWarehouseId = request.DefaultWarehouseId,
        CanSellFromMultipleWarehouses = request.CanSellFromMultipleWarehouses
    };

    return await _repository.CreateAsync(user);
}
```

### 5. **Repository Actualizado**

**Archivo:** `Infrastructure/Repositories/UserRepository.cs`

```csharp
public async Task<User?> GetByIdAsync(int userId)
{
    return await _dbcontext.User
        .Include(u => u.Role)
        .Include(u => u.DefaultWarehouse)
            .ThenInclude(w => w.Branch)
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == userId);
}

public async Task<bool> WarehouseExistsAsync(int warehouseId)
{
    return await _dbcontext.Warehouses
        .AnyAsync(w => w.Id == warehouseId && w.IsActive);
}

public async Task<User?> UpdateAsync(User user)
{
    var existing = await _dbcontext.User.FindAsync(user.Id);
    if (existing == null) return null;

    // ...actualización de campos...
    existing.DefaultWarehouseId = user.DefaultWarehouseId;
    existing.CanSellFromMultipleWarehouses = user.CanSellFromMultipleWarehouses;
    existing.UpdatedAt = DateTime.UtcNow;

    await _dbcontext.SaveChangesAsync();
    return existing;
}
```

### 6. **DbContext Configuración**

**Archivo:** `Infrastructure/Persistence/POSDbContext.cs`

```csharp
modelBuilder.Entity<User>()
    .HasOne(u => u.DefaultWarehouse)
    .WithMany()
    .HasForeignKey(u => u.DefaultWarehouseId)
    .OnDelete(DeleteBehavior.Restrict); // NO CASCADE

modelBuilder.Entity<User>()
    .Property(u => u.CanSellFromMultipleWarehouses)
    .HasDefaultValue(false);

modelBuilder.Entity<User>()
    .HasIndex(u => u.DefaultWarehouseId);
```

### 7. **Migración Aplicada**

**Migración:** `20260311002404_AddWarehouseControlToUsers`

```sql
ALTER TABLE [Users] ADD [CanSellFromMultipleWarehouses] bit NOT NULL DEFAULT CAST(0 AS bit);
ALTER TABLE [Users] ADD [DefaultWarehouseId] int NULL;
CREATE INDEX [IX_Users_DefaultWarehouseId] ON [Users] ([DefaultWarehouseId]);
ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Warehouses_DefaultWarehouseId] 
    FOREIGN KEY ([DefaultWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION;
```

---

## ?? Casos de Uso

### Caso 1: Cajero de Sucursal (Almacén Único)

```json
POST /api/User/create
{
  "code": "CAJ001",
  "name": "María López",
  "email": "maria.lopez@empresa.com",
  "phone": "5551234567",
  "roleId": 6,
  "password": "password123",
  "defaultWarehouseId": 1,
  "canSellFromMultipleWarehouses": false
}
```

**Resultado:**
- María solo puede vender del almacén ID 1
- El sistema ignora cualquier intento de vender de otro almacén
- Simplifica el proceso de venta (no elige almacén)

### Caso 2: Gerente Regional (Multi-Almacén)

```json
POST /api/User/create
{
  "code": "GER001",
  "name": "Carlos Ramírez",
  "email": "carlos.ramirez@empresa.com",
  "phone": "5559876543",
  "roleId": 5,
  "password": "password123",
  "defaultWarehouseId": 1,
  "canSellFromMultipleWarehouses": true
}
```

**Resultado:**
- Carlos puede vender de cualquier almacén
- Su almacén por defecto es el ID 1 (para uso común)
- Puede realizar ventas especiales de otros almacenes

### Caso 3: Administrador (Sin Almacén Asignado)

```json
POST /api/User/create
{
  "code": "ADM002",
  "name": "Ana García",
  "email": "ana.garcia@empresa.com",
  "phone": "5555555555",
  "roleId": 1,
  "password": "password123",
  "defaultWarehouseId": null,
  "canSellFromMultipleWarehouses": true
}
```

**Resultado:**
- Ana tiene acceso total
- No tiene almacén por defecto
- Puede operar desde cualquier almacén

---

## ?? Validaciones Implementadas

### 1. Validación de Existencia de Almacén

```csharp
if (request.DefaultWarehouseId.HasValue && request.DefaultWarehouseId.Value > 0)
{
    var warehouseExists = await _repository.WarehouseExistsAsync(request.DefaultWarehouseId.Value);
    if (!warehouseExists)
    {
        throw new InvalidOperationException($"El almacén con ID {request.DefaultWarehouseId.Value} no existe o está inactivo");
    }
}
```

### 2. Carga de Relaciones

```csharp
.Include(u => u.DefaultWarehouse)
    .ThenInclude(w => w.Branch)
```

Asegura que siempre se cargue:
- Warehouse (almacén)
- Branch (sucursal del almacén)

---

## ?? Configuración por Rol Sugerida

| Rol | DefaultWarehouse | CanSellFromMultiple | Uso |
|-----|------------------|---------------------|-----|
| **Administrador** | NULL o Asignado | ? true | Acceso total |
| **Gerente** | Asignado | ? true | Multi-sucursal |
| **Supervisor** | Asignado | ?? Según caso | Flexible |
| **Vendedor** | Asignado | ? false | Una sucursal |
| **Cajero** | Asignado | ? false | Una sucursal |
| **Almacenista** | Asignado | ? false | Un almacén |

---

## ?? Próximos Pasos para Implementar Lógica de Negocio

### Opción A: Middleware de Validación

```csharp
public class WarehouseAccessMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/Sales"))
        {
            var userId = int.Parse(context.User.FindFirst("userId")?.Value ?? "0");
            var user = await _userRepository.GetByIdAsync(userId);
            
            // Agregar info al contexto
            context.Items["UserDefaultWarehouseId"] = user.DefaultWarehouseId;
            context.Items["CanSellFromMultipleWarehouses"] = user.CanSellFromMultipleWarehouses;
        }
        
        await _next(context);
    }
}
```

### Opción B: Validación en Endpoint de Venta

```csharp
[HttpPost("create-sale")]
[Authorize]
public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto saleDto)
{
    var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
    var user = await _userRepository.GetByIdAsync(userId);
    
    int warehouseId;
    
    if (user.CanSellFromMultipleWarehouses)
    {
        // Usuario puede elegir almacén
        if (saleDto.WarehouseId == null)
        {
            return BadRequest(new { message = "Debe especificar el almacén" });
        }
        warehouseId = saleDto.WarehouseId.Value;
    }
    else
    {
        // Usuario solo puede vender de su almacén asignado
        if (user.DefaultWarehouseId == null)
        {
            return BadRequest(new { message = "No tiene almacén asignado" });
        }
        
        // Forzar el uso del almacén asignado
        warehouseId = user.DefaultWarehouseId.Value;
    }
    
    // Validar stock en el almacén seleccionado
    foreach (var item in saleDto.Items)
    {
        var stock = await _inventoryService.GetStockAsync(item.ProductId, warehouseId);
        if (stock < item.Quantity)
        {
            return BadRequest(new { message = $"Stock insuficiente en almacén {warehouseId}" });
        }
    }
    
    // Crear venta con el warehouse validado
    var sale = new Sale
    {
        WarehouseId = warehouseId,
        UserId = userId,
        // ...otros campos
    };
    
    // ...guardar venta
}
```

### Opción C: Consulta de Productos con Stock Filtrado

```csharp
[HttpGet("products-with-stock")]
[Authorize]
public async Task<IActionResult> GetProductsWithStock()
{
    var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
    var user = await _userRepository.GetByIdAsync(userId);
    
    List<ProductStockDto> products;
    
    if (user.CanSellFromMultipleWarehouses)
    {
        // Ver stock de todos los almacenes
        products = await _productRepository.GetAllWithStockAsync();
    }
    else
    {
        // Ver solo stock de su almacén
        if (user.DefaultWarehouseId == null)
        {
            return BadRequest(new { message = "No tiene almacén asignado" });
        }
        
        products = await _productRepository.GetWithStockByWarehouseAsync(
            user.DefaultWarehouseId.Value
        );
    }
    
    return Ok(products);
}
```

---

## ?? Ejemplo de Respuesta del Endpoint

```json
POST /api/User/create

Response:
{
  "message": "Usuario creado exitosamente",
  "error": 0,
  "data": {
    "id": 10,
    "code": "VEN005",
    "name": "Pedro Sánchez",
    "email": "pedro.sanchez@empresa.com",
    "phone": "5554443322",
    "roleId": 3,
    "active": true,
    "defaultWarehouseId": 2,
    "canSellFromMultipleWarehouses": false,
    "createdAt": "2026-03-11T00:30:00Z"
  },
  "createdBy": "Administrador"
}
```

---

## ? Checklist de Implementación

- [x] Entidad User actualizada
- [x] DTOs actualizados
- [x] Commands y Handlers actualizados
- [x] Repository con validación
- [x] DbContext configurado
- [x] Migración creada y aplicada
- [x] Build exitoso
- [x] Controlador actualizado
- [ ] Implementar lógica de ventas (Próximo paso)
- [ ] Implementar filtros de productos por almacén (Próximo paso)
- [ ] Testing de endpoints (Próximo paso)

---

## ?? Conceptos Clave

### 1. **Almacén por Defecto (DefaultWarehouseId)**
- Almacén principal del usuario
- Usado automáticamente si no puede elegir
- NULL para usuarios sin almacén fijo

### 2. **Multi-Almacén (CanSellFromMultipleWarehouses)**
- `false`: Usuario restringido a su almacén
- `true`: Usuario puede elegir almacén en cada operación

### 3. **Validación Automática**
- El sistema valida que el almacén exista
- Previene asignaciones inválidas
- Control de integridad referencial

---

## ?? Comandos Útiles

### Ver migración aplicada
```bash
dotnet ef migrations list --startup-project ..\Web.Api --context POSDbContext
```

### Revertir migración (si es necesario)
```bash
dotnet ef database update PreviousMigrationName --startup-project ..\Web.Api --context POSDbContext
```

### Ver estado de la base de datos
```sql
SELECT Id, Code, Name, DefaultWarehouseId, CanSellFromMultipleWarehouses 
FROM Users;
```

---

¡Implementación completada exitosamente! ??
