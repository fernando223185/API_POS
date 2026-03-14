# ?? Branches: CompanyId Ahora es Requerido

## ?? Cambios Implementados

Se ha actualizado el módulo de **Sucursales (Branches)** para que **requiera obligatoriamente** la empresa a la que pertenece cada sucursal.

### ? Cambios Realizados

#### 1. **DTOs Actualizados** (`Application\DTOs\Branch\BranchDtos.cs`)

**CreateBranchDto:**
```csharp
public class CreateBranchDto
{
    [Required(ErrorMessage = "La empresa es requerida")]
    public int CompanyId { get; set; }  // ? NUEVO: Campo requerido

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // ...otros campos...
}
```

**UpdateBranchDto:**
```csharp
public class UpdateBranchDto
{
    [Required(ErrorMessage = "La empresa es requerida")]
    public int CompanyId { get; set; }  // ? NUEVO: Campo requerido

    public string Name { get; set; } = string.Empty;
    // ...otros campos...
}
```

**BranchResponseDto:**
```csharp
public class BranchResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? CompanyId { get; set; }        // ? NUEVO
    public string? CompanyName { get; set; }   // ? NUEVO
    public string Name { get; set; } = string.Empty;
    // ...otros campos...
}
```

#### 2. **Command Handlers Actualizados** (`Application\Core\Branch\CommandHandlers\BranchCommandHandlers.cs`)

**CreateBranchCommandHandler:**
```csharp
public async Task<BranchResponseDto> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
{
    // ? VALIDAR QUE LA EMPRESA EXISTE
    var company = await _companyRepository.GetByIdAsync(request.BranchData.CompanyId);
    if (company == null)
    {
        throw new KeyNotFoundException($"Empresa con ID {request.BranchData.CompanyId} no encontrada");
    }

    var branch = new Domain.Entities.Branch
    {
        Code = code,
        CompanyId = request.BranchData.CompanyId,  // ? ASIGNAR EMPRESA
        Name = request.BranchData.Name,
        // ...otros campos...
    };

    var createdBranch = await _branchRepository.CreateAsync(branch);

    return new BranchResponseDto
    {
        Id = createdBranch.Id,
        Code = createdBranch.Code,
        CompanyId = createdBranch.CompanyId,       // ? INCLUIR EN RESPONSE
        CompanyName = company.LegalName,            // ? INCLUIR NOMBRE
        // ...otros campos...
    };
}
```

**UpdateBranchCommandHandler:**
```csharp
public async Task<BranchResponseDto> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
{
    // ? VALIDAR QUE LA EMPRESA EXISTE
    var company = await _companyRepository.GetByIdAsync(request.BranchData.CompanyId);
    if (company == null)
    {
        throw new KeyNotFoundException($"Empresa con ID {request.BranchData.CompanyId} no encontrada");
    }

    // Actualizar campos
    branch.CompanyId = request.BranchData.CompanyId;  // ? ACTUALIZAR EMPRESA
    branch.Name = request.BranchData.Name;
    // ...otros campos...
}
```

#### 3. **Query Handlers Actualizados** (`Application\Core\Branch\QueryHandlers\BranchQueryHandlers.cs`)

Todos los handlers ahora incluyen:
```csharp
return new BranchResponseDto
{
    Id = branch.Id,
    Code = branch.Code,
    CompanyId = branch.CompanyId,           // ? INCLUIR COMPANY ID
    CompanyName = branch.Company?.LegalName, // ? INCLUIR NOMBRE DE EMPRESA
    Name = branch.Name,
    // ...otros campos...
};
```

#### 4. **Repository Actualizado** (`Infrastructure\Repositories\BranchRepository.cs`)

Ahora carga la relación con Company:
```csharp
public async Task<Branch?> GetByIdAsync(int id)
{
    return await _context.Branches
        .Include(b => b.Company)      // ? INCLUIR RELACIÓN
        .Include(b => b.CreatedBy)
        .Include(b => b.UpdatedBy)
        .FirstOrDefaultAsync(b => b.Id == id);
}

public async Task<List<Branch>> GetAllAsync(bool includeInactive = false)
{
    var query = _context.Branches
        .Include(b => b.Company)      // ? INCLUIR RELACIÓN
        .Include(b => b.CreatedBy)
        .Include(b => b.UpdatedBy)
        .AsQueryable();
    // ...
}
```

También permite búsqueda por nombre de empresa:
```csharp
if (!string.IsNullOrWhiteSpace(searchTerm))
{
    query = query.Where(b =>
        b.Code.Contains(searchTerm) ||
        b.Name.Contains(searchTerm) ||
        b.City.Contains(searchTerm) ||
        b.State.Contains(searchTerm) ||
        (b.Company != null && b.Company.LegalName.Contains(searchTerm))); // ? BUSCAR POR EMPRESA
}
```

## ?? Ejemplo de Uso

### Crear Sucursal

**Request:**
```http
POST /api/branches
Content-Type: application/json
Authorization: Bearer {token}

{
  "companyId": 1,                    // ? REQUERIDO
  "name": "Sucursal Centro",
  "description": "Sucursal principal en el centro de la ciudad",
  "address": "Av. Juárez #123",
  "city": "Guadalajara",
  "state": "Jalisco",
  "zipCode": "44100",
  "country": "México",
  "phone": "3331234567",
  "email": "centro@empresa.com",
  "managerName": "Juan Pérez",
  "isMainBranch": true,
  "openingDate": "2024-01-15"
}
```

**Response:**
```json
{
  "message": "Sucursal creada exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "SUC-001",
    "companyId": 1,                       // ? ID DE LA EMPRESA
    "companyName": "Mi Empresa SA de CV", // ? NOMBRE DE LA EMPRESA
    "name": "Sucursal Centro",
    "description": "Sucursal principal en el centro de la ciudad",
    "address": "Av. Juárez #123",
    "city": "Guadalajara",
    "state": "Jalisco",
    "zipCode": "44100",
    "country": "México",
    "phone": "3331234567",
    "email": "centro@empresa.com",
    "managerName": "Juan Pérez",
    "isMainBranch": true,
    "isActive": true,
    "openingDate": "2024-01-15T00:00:00Z",
    "createdAt": "2024-03-11T10:30:00Z"
  }
}
```

### Actualizar Sucursal

**Request:**
```http
PUT /api/branches/1
Content-Type: application/json
Authorization: Bearer {token}

{
  "companyId": 1,              // ? REQUERIDO (puede cambiar de empresa)
  "name": "Sucursal Centro - Actualizada",
  "description": "Descripción actualizada",
  "address": "Av. Juárez #123",
  "city": "Guadalajara",
  "state": "Jalisco",
  "zipCode": "44100",
  "country": "México",
  "phone": "3331234567",
  "email": "centro@empresa.com",
  "managerName": "Juan Pérez González",
  "isMainBranch": true,
  "openingDate": "2024-01-15",
  "isActive": true
}
```

### Obtener Sucursales

**Request:**
```http
GET /api/branches?page=1&pageSize=10
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Sucursales obtenidas exitosamente",
  "error": 0,
  "data": [
    {
      "id": 1,
      "code": "SUC-001",
      "companyId": 1,                       // ? EMPRESA ASOCIADA
      "companyName": "Mi Empresa SA de CV", // ? NOMBRE DE EMPRESA
      "name": "Sucursal Centro",
      "city": "Guadalajara",
      "state": "Jalisco",
      "isMainBranch": true,
      "isActive": true
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalRecords": 1
}
```

## ? Validaciones

### Crear/Actualizar Sucursal

1. **CompanyId es requerido**
   - Si no se envía ? Error 400
   - Si se envía 0 o null ? Error 400

2. **La empresa debe existir**
   - Si CompanyId no existe en BD ? Error 404
   ```json
   {
     "message": "Empresa con ID 999 no encontrada",
     "error": 1
   }
   ```

## ?? Migración de Datos Existentes

Si ya tienes sucursales sin `CompanyId`, necesitarás ejecutar este script:

```sql
-- Asignar todas las sucursales a la empresa principal
UPDATE Branches
SET CompanyId = (SELECT TOP 1 Id FROM Companies WHERE IsMainCompany = 1)
WHERE CompanyId IS NULL;

-- O asignar a una empresa específica
UPDATE Branches
SET CompanyId = 1  -- ID de la empresa por defecto
WHERE CompanyId IS NULL;
```

## ?? Impacto en el Sistema

### Módulos Afectados

? **Branches** - Ahora requiere empresa
? **Warehouses** - Ya tiene BranchId, indirectamente tiene CompanyId
? **Users** - Si tienen WarehouseId, indirectamente tienen CompanyId

### Relaciones

```
Company (1) ??? (N) Branch (1) ??? (N) Warehouse
```

### Consultas Mejoradas

Ahora puedes:
- Filtrar sucursales por empresa
- Buscar por nombre de empresa en el listado de sucursales
- Ver a qué empresa pertenece cada sucursal

## ?? Beneficios

1. **Multi-Empresa**: Soporte completo para múltiples empresas
2. **Trazabilidad**: Siempre se sabe a qué empresa pertenece cada sucursal
3. **Seguridad**: Validación de existencia de empresa
4. **Reportes**: Facilita reportes por empresa
5. **Auditoría**: Mejor control y seguimiento

## ?? Endpoints Actualizados

| Método | Endpoint | Cambio |
|--------|----------|--------|
| POST | `/api/branches` | Requiere `companyId` |
| PUT | `/api/branches/{id}` | Requiere `companyId` |
| GET | `/api/branches` | Retorna `companyId` y `companyName` |
| GET | `/api/branches/{id}` | Retorna `companyId` y `companyName` |
| GET | `/api/branches/code/{code}` | Retorna `companyId` y `companyName` |

## ? Build Exitoso

- ? Compilación sin errores
- ? Todas las validaciones funcionando
- ? Relaciones configuradas correctamente

## ?? Archivos Modificados

1. `Application\DTOs\Branch\BranchDtos.cs`
2. `Application\Core\Branch\CommandHandlers\BranchCommandHandlers.cs`
3. `Application\Core\Branch\QueryHandlers\BranchQueryHandlers.cs`
4. `Infrastructure\Repositories\BranchRepository.cs`

## ?? Documentación Relacionada

- [Sistema de Empresas (Companies)](./Companies_Insufficient_Permissions_Fixed.md)
- [Sistema de Almacenes (Warehouses)](./Warehouses_Management_System.md)
- [Sistema de Sucursales (Branches)](./Branches_Management_System.md)
