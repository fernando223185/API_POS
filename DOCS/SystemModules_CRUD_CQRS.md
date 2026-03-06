# ?? **CRUD de Módulos y Submódulos con CQRS**

## ?? **Implementación Completa**

Se ha implementado un **CRUD completo** para administrar módulos y submódulos del sistema usando el **patrón CQRS** (Command Query Responsibility Segregation).

---

## ??? **Arquitectura CQRS Implementada**

```
???????????????????????????????????????????????????????????????
?                    CONTROLADOR                               ?
?         SystemModulesCQRSController                          ?
?           /api/system/modules                                ?
???????????????????????????????????????????????????????????????
             ?
             ???? QUERIES (Consultas) ?????????????????????????
             ?                                                 ?
             ?    GetAllModulesQuery                          ?
             ?    GetModuleByIdQuery                           ?
             ?    GetSubmodulesByModuleQuery                   ?
             ?    GetSubmoduleByIdQuery                        ?
             ?                                                 ?
             ?    ? Handler ?                                 ?
             ?    GetAllModulesQueryHandler                    ?
             ?    GetModuleByIdQueryHandler                    ?
             ?    GetSubmodulesByModuleQueryHandler            ?
             ?    GetSubmoduleByIdQueryHandler                 ?
             ?                                                 ?
             ???????????????????????????????????????????????????
             ?
             ???? COMMANDS (Modificaciones) ???????????????????
             ?                                                 ?
             ?    CreateModuleCommand                          ?
             ?    UpdateModuleCommand                          ?
             ?    DeleteModuleCommand                          ?
             ?    CreateSubmoduleCommand                       ?
             ?    UpdateSubmoduleCommand                       ?
             ?    DeleteSubmoduleCommand                       ?
             ?                                                 ?
             ?    ? Handler ?                                 ?
             ?    CreateModuleCommandHandler                   ?
             ?    UpdateModuleCommandHandler                   ?
             ?    DeleteModuleCommandHandler                   ?
             ?    CreateSubmoduleCommandHandler                ?
             ?    UpdateSubmoduleCommandHandler                ?
             ?    DeleteSubmoduleCommandHandler                ?
             ?                                                 ?
             ???????????????????????????????????????????????????
             ?
             ?
   ???????????????????????????
   ?   REPOSITORIO            ?
   ?  ISystemModuleRepository ?
   ?  SystemModuleRepository  ?
   ????????????????????????????
              ?
              ?
   ????????????????????
   ?   BASE DE DATOS  ?
   ?   Modules        ?
   ?   Submodules     ?
   ????????????????????
```

---

## ?? **Archivos Creados**

### **1. DTOs:**
- `Application/DTOs/SystemModules/SystemModuleCommandDtos.cs`
  - `CreateModuleDto`
  - `UpdateModuleDto`
  - `CreateSubmoduleDto`
  - `UpdateSubmoduleDto`
  - `ModuleCommandResponseDto`
  - `SubmoduleCommandResponseDto`
  - `DeleteResponseDto`

### **2. Commands:**
- `Application/Core/SystemModules/Commands/SystemModuleCommands.cs`
  - `CreateModuleCommand`
  - `UpdateModuleCommand`
  - `DeleteModuleCommand`
  - `CreateSubmoduleCommand`
  - `UpdateSubmoduleCommand`
  - `DeleteSubmoduleCommand`

### **3. Queries:**
- `Application/Core/SystemModules/Queries/SystemModuleQueries.cs`
  - `GetAllModulesQuery`
  - `GetModuleByIdQuery`
  - `GetSubmodulesByModuleQuery`
  - `GetSubmoduleByIdQuery`

### **4. Command Handlers:**
- `Application/Core/SystemModules/CommandHandlers/SystemModuleCommandHandlers.cs`
  - `CreateModuleCommandHandler`
  - `UpdateModuleCommandHandler`
  - `DeleteModuleCommandHandler`
  - `CreateSubmoduleCommandHandler`
  - `UpdateSubmoduleCommandHandler`
  - `DeleteSubmoduleCommandHandler`

### **5. Query Handlers:**
- `Application/Core/SystemModules/QueryHandlers/SystemModuleQueryHandlers.cs`
  - `GetAllModulesQueryHandler`
  - `GetModuleByIdQueryHandler`
  - `GetSubmodulesByModuleQueryHandler`
  - `GetSubmoduleByIdQueryHandler`

### **6. Repositorio:**
- `Application/Abstractions/Config/ISystemModuleRepository.cs` (Interfaz)
- `Infrastructure/Repositories/SystemModuleRepository.cs` (Implementación)

### **7. Controlador:**
- `Web.Api/Controllers/Config/SystemModulesCQRSController.cs`

---

## ?? **Endpoints Disponibles**

### **BASE URL:** `/api/system/modules`

---

## ?? **MÓDULOS**

### **1. Listar Módulos**

```
GET /api/system/modules?includeInactive=false
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Módulos obtenidos exitosamente",
  "error": 0,
  "modules": [
    {
      "id": 1,
      "name": "Inicio",
      "description": "Panel principal",
      "path": "/dashboard",
      "icon": "faHome",
      "order": 1,
      "isActive": true,
      "createdAt": "2025-01-07T...",
      "updatedAt": null,
      "submodules": []
    }
  ],
  "totalModules": 8,
  "totalSubmodules": 29
}
```

---

### **2. Obtener Módulo por ID**

```
GET /api/system/modules/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Módulo obtenido exitosamente",
  "error": 0,
  "data": {
    "id": 2,
    "name": "Ventas",
    "description": "Gestión de ventas",
    "path": "/sales",
    "icon": "faShoppingCart",
    "order": 2,
    "isActive": true,
    "submodules": [...]
  }
}
```

---

### **3. Crear Módulo**

```
POST /api/system/modules
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Reportes Avanzados",
  "description": "Reportes y análisis del sistema",
  "path": "/advanced-reports",
  "icon": "faChartPie",
  "order": 9,
  "isActive": true
}
```

**Response:**
```json
{
  "message": "Módulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 9,
    "name": "Reportes Avanzados",
    "description": "Reportes y análisis del sistema",
    "path": "/advanced-reports",
    "icon": "faChartPie",
    "order": 9,
    "isActive": true,
    "createdAt": "2025-01-07T...",
    "submodules": []
  }
}
```

---

### **4. Actualizar Módulo**

```
PUT /api/system/modules/{id}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Reportes y Análisis",
  "description": "Reportes avanzados y análisis de datos",
  "path": "/reports-analytics",
  "icon": "faChartLine",
  "order": 9,
  "isActive": true
}
```

**Response:**
```json
{
  "message": "Módulo actualizado exitosamente",
  "error": 0,
  "data": {
    "id": 9,
    "name": "Reportes y Análisis",
    "description": "Reportes avanzados y análisis de datos",
    "path": "/reports-analytics",
    "icon": "faChartLine",
    "order": 9,
    "isActive": true,
    "createdAt": "2025-01-07T...",
    "updatedAt": "2025-01-07T..."
  }
}
```

---

### **5. Eliminar Módulo (Soft Delete)**

```
DELETE /api/system/modules/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Módulo eliminado exitosamente",
  "error": 0,
  "deletedId": 9
}
```

---

## ?? **SUBMÓDULOS**

### **6. Listar Submódulos de un Módulo**

```
GET /api/system/modules/{moduleId}/submodules?includeInactive=false
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Submódulos obtenidos exitosamente",
  "error": 0,
  "moduleId": 2,
  "moduleName": "Ventas",
  "submodules": [
    {
      "id": 21,
      "moduleId": 2,
      "name": "Nueva Venta",
      "description": "Crear ticket de venta",
      "path": "/sales/new",
      "icon": "faPlus",
      "order": 1,
      "color": "from-emerald-500 to-teal-600",
      "isActive": true,
      "createdAt": "2025-01-07T...",
      "updatedAt": null
    }
  ],
  "totalSubmodules": 4
}
```

---

### **7. Obtener Submódulo por ID**

```
GET /api/system/modules/submodules/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Submódulo obtenido exitosamente",
  "error": 0,
  "data": {
    "id": 21,
    "moduleId": 2,
    "name": "Nueva Venta",
    "description": "Crear ticket de venta",
    "path": "/sales/new",
    "icon": "faPlus",
    "order": 1,
    "color": "from-emerald-500 to-teal-600",
    "isActive": true
  }
}
```

---

### **8. Crear Submódulo**

```
POST /api/system/modules/submodules
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "moduleId": 2,
  "name": "Cotizaciones",
  "description": "Crear y gestionar cotizaciones",
  "path": "/sales/quotes",
  "icon": "faFileInvoice",
  "order": 5,
  "color": "from-blue-500 to-indigo-600",
  "isActive": true
}
```

**Response:**
```json
{
  "message": "Submódulo creado exitosamente",
  "error": 0,
  "data": {
    "id": 25,
    "moduleId": 2,
    "name": "Cotizaciones",
    "description": "Crear y gestionar cotizaciones",
    "path": "/sales/quotes",
    "icon": "faFileInvoice",
    "order": 5,
    "color": "from-blue-500 to-indigo-600",
    "isActive": true,
    "createdAt": "2025-01-07T..."
  }
}
```

---

### **9. Actualizar Submódulo**

```
PUT /api/system/modules/submodules/{id}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Gestión de Cotizaciones",
  "description": "Crear, editar y aprobar cotizaciones",
  "path": "/sales/quotations",
  "icon": "faFileContract",
  "order": 5,
  "color": "from-purple-500 to-pink-600",
  "isActive": true
}
```

**Response:**
```json
{
  "message": "Submódulo actualizado exitosamente",
  "error": 0,
  "data": {
    "id": 25,
    "moduleId": 2,
    "name": "Gestión de Cotizaciones",
    "description": "Crear, editar y aprobar cotizaciones",
    "path": "/sales/quotations",
    "icon": "faFileContract",
    "order": 5,
    "color": "from-purple-500 to-pink-600",
    "isActive": true,
    "createdAt": "2025-01-07T...",
    "updatedAt": "2025-01-07T..."
  }
}
```

---

### **10. Eliminar Submódulo (Soft Delete)**

```
DELETE /api/system/modules/submodules/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Submódulo eliminado exitosamente",
  "error": 0,
  "deletedId": 25
}
```

---

## ?? **Permisos Requeridos**

| Endpoint | Método | Permiso Requerido |
|----------|--------|-------------------|
| GET módulos | GET | `RequireAuthentication` (cualquier usuario autenticado) |
| GET módulo por ID | GET | `RequireAuthentication` |
| GET submódulos | GET | `RequireAuthentication` |
| GET submódulo por ID | GET | `RequireAuthentication` |
| **Crear módulo** | POST | `RequirePermission("Configuration", "ManageModules")` |
| **Actualizar módulo** | PUT | `RequirePermission("Configuration", "ManageModules")` |
| **Eliminar módulo** | DELETE | `RequirePermission("Configuration", "ManageModules")` |
| **Crear submódulo** | POST | `RequirePermission("Configuration", "ManageModules")` |
| **Actualizar submódulo** | PUT | `RequirePermission("Configuration", "ManageModules")` |
| **Eliminar submódulo** | DELETE | `RequirePermission("Configuration", "ManageModules")` |

---

## ?? **Ventajas del Patrón CQRS**

### **1. Separación de Responsabilidades**
- **Queries:** Solo lectura, sin efectos secundarios
- **Commands:** Solo escritura, modifican el estado

### **2. Escalabilidad**
- Queries y Commands pueden optimizarse independientemente
- Fácil agregar caché a las Queries

### **3. Testabilidad**
- Cada Handler es una unidad independiente
- Fácil de probar con mocks

### **4. Mantenibilidad**
- Código más organizado y fácil de entender
- Cambios en Queries no afectan Commands y viceversa

### **5. Extensibilidad**
- Fácil agregar nuevas Queries o Commands
- Patrón Mediator (MediatR) desacopla componentes

---

## ?? **Comparación: Antes vs Después**

### **? ANTES (Sin CQRS):**

```csharp
[HttpPost]
public async Task<IActionResult> CreateModule([FromBody] CreateUpdateModuleDto dto)
{
    // Acceso directo al DbContext
    var module = new SystemModule { ... };
    _context.SystemModules.Add(module);
    await _context.SaveChangesAsync();
    return Ok(module);
}
```

**Problemas:**
- Controlador con lógica de negocio
- Difícil de testear
- Violación del principio SRP (Single Responsibility)

---

### **? DESPUÉS (Con CQRS):**

```csharp
[HttpPost]
public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto dto)
{
    var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
    var command = new CreateModuleCommand(dto, currentUserId);
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**Ventajas:**
- Controlador delgado, solo orquesta
- Lógica de negocio en el Handler
- Fácil de testear cada componente
- Repositorio abstrae la persistencia

---

## ?? **Uso en Frontend (TypeScript)**

```typescript
// Interfaces
interface CreateModuleRequest {
  name: string;
  description: string;
  path: string;
  icon: string;
  order: number;
  isActive: boolean;
}

interface ModuleResponse {
  message: string;
  error: number;
  data: {
    id: number;
    name: string;
    description: string;
    path: string;
    icon: string;
    order: number;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
  };
}

// Servicio API
class ModuleService {
  private baseUrl = 'http://localhost:7254/api/system/modules';

  async createModule(data: CreateModuleRequest): Promise<ModuleResponse> {
    const response = await fetch(this.baseUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(data)
    });
    
    const result = await response.json();
    
    if (result.error === 0) {
      console.log('? Módulo creado:', result.data);
    } else {
      console.error('? Error:', result.message);
    }
    
    return result;
  }

  async getAllModules(includeInactive = false) {
    const response = await fetch(
      `${this.baseUrl}?includeInactive=${includeInactive}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    return response.json();
  }
  
  async updateModule(id: number, data: UpdateModuleRequest) {
    const response = await fetch(`${this.baseUrl}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(data)
    });
    
    return response.json();
  }
  
  async deleteModule(id: number) {
    const response = await fetch(`${this.baseUrl}/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    return response.json();
  }
}
```

---

## ? **RESUMEN**

| Característica | Estado |
|----------------|--------|
| **Patrón CQRS** | ? Implementado |
| **Queries (Lectura)** | ? 4 Queries |
| **Commands (Escritura)** | ? 6 Commands |
| **Handlers** | ? 10 Handlers |
| **Repositorio** | ? Implementado |
| **Controlador** | ? Implementado |
| **DTOs** | ? Implementados |
| **Permisos** | ? Integrados |
| **Soft Delete** | ? Implementado |
| **Compilación** | ? Sin errores |

---

**? CRUD COMPLETO CON CQRS IMPLEMENTADO** ??

**Endpoints:** 10 endpoints RESTful  
**Base URL:** `/api/system/modules`  
**Patrón:** CQRS (Command Query Responsibility Segregation)  
**Arquitectura:** Limpia y escalable
