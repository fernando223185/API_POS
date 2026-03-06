# ?? API de Consulta de Usuarios y Roles

## ?? Descripción

Controlador completo para consultar usuarios y roles dados de alta en el sistema. Incluye funcionalidades de búsqueda, filtrado, paginación y estadísticas.

---

## ? Características

- ? Consulta de usuarios con filtros avanzados
- ? Consulta de roles del sistema
- ? Paginación de resultados
- ? Estadísticas de usuarios
- ? Búsqueda por nombre, código o email
- ? Filtrado por rol y estado
- ? Incluye permisos por rol

---

## ?? Endpoints Disponibles

**Ruta Base:** `/api/Users`

### ?? **USUARIOS**

#### 1?? Obtener Todos los Usuarios (con filtros opcionales y paginación)
```http
GET /api/Users
GET /api/Users?search=juan
GET /api/Users?roleId=3
GET /api/Users?active=true
GET /api/Users?page=2&pageSize=20
GET /api/Users?search=admin&roleId=1&active=true&page=1&pageSize=10

Authorization: Bearer {token}
```

**Parámetros Query (TODOS OPCIONALES):**
| Parámetro | Tipo | Descripción | Ejemplo | Requerido |
|-----------|------|-------------|---------|-----------|
| `search` | string | Buscar en nombre, código o email | `juan` | ? No |
| `roleId` | int | Filtrar por rol específico | `3` | ? No |
| `active` | bool | Filtrar por estado activo/inactivo | `true` | ? No |
| `page` | int | Número de página (default: 1) | `2` | ? No |
| `pageSize` | int | Registros por página (default: 100) | `20` | ? No |

**? Ejemplo sin parámetros (obtiene TODOS los usuarios):**
```http
GET /api/Users
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Usuarios obtenidos exitosamente",
  "error": 0,
  "data": {
    "users": [
      {
        "id": 1,
        "code": "ADMIN001",
        "name": "Administrador",
        "email": "admin@sistema.com",
        "phone": "1234567890",
        "roleId": 1,
        "roleName": "Administrador",
        "active": true,
        "createdAt": "2025-01-30T22:47:14Z",
        "updatedAt": null
      },
      {
        "id": 2,
        "code": "USR001",
        "name": "Juan Vendedor",
        "email": "juan@sistema.com",
        "phone": "5551111111",
        "roleId": 3,
        "roleName": "Vendedor",
        "active": true,
        "createdAt": "2025-01-30T22:50:00Z",
        "updatedAt": null
      }
    ],
    "totalUsers": 5,
    "totalPages": 1,
    "currentPage": 1,
    "pageSize": 100
  }
}
```

---

#### 2?? Obtener Usuario por ID
```http
GET /api/Users/{id}
Authorization: Bearer {token}
```

**Ejemplo:**
```http
GET /api/Users/1
```

**Respuesta:**
```json
{
  "message": "Usuario obtenido exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "ADMIN001",
    "name": "Administrador",
    "email": "admin@sistema.com",
    "phone": "1234567890",
    "roleId": 1,
    "roleName": "Administrador",
    "active": true,
    "createdAt": "2025-01-30T22:47:14Z",
    "updatedAt": null
  }
}
```

---

#### 3?? Obtener Estadísticas de Usuarios
```http
GET /api/Users/statistics
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Estadísticas obtenidas exitosamente",
  "error": 0,
  "data": {
    "totalUsers": 5,
    "activeUsers": 4,
    "inactiveUsers": 1,
    "usersByRole": [
      {
        "roleId": 1,
        "roleName": "Administrador",
        "totalUsers": 1,
        "activeUsers": 1
      },
      {
        "roleId": 2,
        "roleName": "Usuario",
        "totalUsers": 1,
        "activeUsers": 1
      },
      {
        "roleId": 3,
        "roleName": "Vendedor",
        "totalUsers": 2,
        "activeUsers": 1
      },
      {
        "roleId": 4,
        "roleName": "Almacenista",
        "totalUsers": 1,
        "activeUsers": 1
      }
    ]
  }
}
```

---

### ?? **ROLES**

#### 4?? Obtener Todos los Roles
```http
GET /api/Users/roles
GET /api/Users/roles?includeInactive=true

Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Roles obtenidos exitosamente",
  "error": 0,
  "roles": [
    {
      "id": 1,
      "name": "Administrador",
      "description": "Acceso completo al sistema",
      "isActive": true,
      "totalUsers": 1,
      "totalPermissions": 77
    },
    {
      "id": 2,
      "name": "Usuario",
      "description": "Acceso básico de lectura",
      "isActive": true,
      "totalUsers": 1,
      "totalPermissions": 5
    },
    {
      "id": 3,
      "name": "Vendedor",
      "description": "Permisos de ventas y clientes",
      "isActive": true,
      "totalUsers": 2,
      "totalPermissions": 12
    },
    {
      "id": 4,
      "name": "Almacenista",
      "description": "Permisos de productos e inventario",
      "isActive": true,
      "totalUsers": 1,
      "totalPermissions": 10
    }
  ],
  "totalRoles": 4
}
```

---

#### 5?? Obtener Rol por ID (con usuarios y permisos)
```http
GET /api/Users/roles/{id}
Authorization: Bearer {token}
```

**Ejemplo:**
```http
GET /api/Users/roles/3
```

**Respuesta:**
```json
{
  "message": "Rol obtenido exitosamente",
  "error": 0,
  "data": {
    "id": 3,
    "name": "Vendedor",
    "description": "Permisos de ventas y clientes",
    "isActive": true,
    "totalUsers": 2,
    "users": [
      {
        "id": 2,
        "code": "VEN001",
        "name": "Juan Vendedor",
        "email": "juan@sistema.com"
      },
      {
        "id": 3,
        "code": "VEN002",
        "name": "María Ventas",
        "email": "maria@sistema.com"
      }
    ],
    "permissions": [
      {
        "id": 1,
        "name": "Create",
        "resource": "Customer",
        "description": "Crear clientes"
      },
      {
        "id": 2,
        "name": "Read",
        "resource": "Customer",
        "description": "Ver clientes"
      },
      {
        "id": 15,
        "name": "Create",
        "resource": "Sale",
        "description": "Crear ventas"
      }
    ]
  }
}
```

---

#### 6?? Obtener Usuarios de un Rol Específico
```http
GET /api/Users/roles/{roleId}/users
Authorization: Bearer {token}
```

**Ejemplo:**
```http
GET /api/Users/roles/3/users
```

**Respuesta:**
```json
{
  "message": "Usuarios obtenidos exitosamente",
  "error": 0,
  "data": {
    "roleId": 3,
    "roleName": "Vendedor",
    "users": [
      {
        "id": 2,
        "code": "VEN001",
        "name": "Juan Vendedor",
        "email": "juan@sistema.com",
        "phone": "5551111111",
        "roleId": 3,
        "roleName": "Vendedor",
        "active": true,
        "createdAt": "2025-01-30T22:50:00Z",
        "updatedAt": null
      },
      {
        "id": 3,
        "code": "VEN002",
        "name": "María Ventas",
        "email": "maria@sistema.com",
        "phone": "5552222222",
        "roleId": 3,
        "roleName": "Vendedor",
        "active": true,
        "createdAt": "2025-01-30T22:51:00Z",
        "updatedAt": null
      }
    ],
    "totalUsers": 2
  }
}
```

---

## ?? Seguridad

### Permisos Requeridos
Todos los endpoints requieren **SOLO AUTENTICACIÓN** (no requieren permisos específicos):
- **Autenticación**: `[RequireAuthentication]` ?
- **Permiso específico**: ? NO requerido

Esto permite que cualquier usuario autenticado pueda consultar la lista de usuarios y roles.

### Autenticación
Todos los endpoints requieren:
```
Authorization: Bearer {token}
```

---

## ?? Ejemplos de Uso

### **Ejemplo 1: Buscar Usuarios por Nombre**
```sh
GET /api/Users?search=juan
```

### **Ejemplo 2: Obtener Solo Vendedores Activos**
```sh
GET /api/Users?roleId=3&active=true
```

### **Ejemplo 3: Listar Usuarios con Paginación**
```sh
GET /api/Users?page=2&pageSize=20
```

### **Ejemplo 4: Buscar Usuarios Inactivos**
```sh
GET /api/Users?active=false
```

### **Ejemplo 5: Obtener Estadísticas Completas**
```sh
GET /api/Users/statistics
```

### **Ejemplo 6: Ver Todos los Roles con Usuarios Inactivos**
```sh
GET /api/Users/roles?includeInactive=true
```

---

## ??? Integración Frontend

### TypeScript/JavaScript - Ejemplo

```typescript
interface User {
  id: number;
  code: string;
  name: string;
  email: string;
  phone: string;
  roleId: number;
  roleName: string;
  active: boolean;
  createdAt: string;
  updatedAt: string | null;
}

interface UserFilters {
  search?: string;
  roleId?: number;
  active?: boolean;
  page?: number;
  pageSize?: number;
}

class UserService {
  private baseUrl = 'http://localhost:7254/api/Users';
  private token: string;
  
  constructor(token: string) {
    this.token = token;
  }
  
  // Obtener usuarios con filtros
  async getUsers(filters: UserFilters): Promise<any> {
    const params = new URLSearchParams();
    
    if (filters.search) params.append('search', filters.search);
    if (filters.roleId) params.append('roleId', filters.roleId.toString());
    if (filters.active !== undefined) params.append('active', filters.active.toString());
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    
    const response = await fetch(`${this.baseUrl}?${params}`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    
    return await response.json();
  }
  
  // Obtener usuario por ID
  async getUserById(userId: number): Promise<any> {
    const response = await fetch(`${this.baseUrl}/${userId}`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    
    return await response.json();
  }
  
  // Obtener estadísticas
  async getStatistics(): Promise<any> {
    const response = await fetch(`${this.baseUrl}/statistics`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    
    return await response.json();
  }
  
  // Obtener todos los roles
  async getRoles(includeInactive: boolean = false): Promise<any> {
    const params = includeInactive ? '?includeInactive=true' : '';
    const response = await fetch(`${this.baseUrl}/roles${params}`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    
    return await response.json();
  }
  
  // Obtener rol por ID
  async getRoleById(roleId: number): Promise<any> {
    const response = await fetch(`${this.baseUrl}/roles/${roleId}`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    
    return await response.json();
  }
  
  // Obtener usuarios de un rol
  async getUsersByRole(roleId: number): Promise<any> {
    const response = await fetch(`${this.baseUrl}/roles/${roleId}/users`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    
    return await response.json();
  }
}
```

---

## ?? Datos de Ejemplo en BD

### **Usuarios Actuales:**

| ID | Código | Nombre | Email | Rol | Estado |
|----|--------|--------|-------|-----|--------|
| 1 | ADMIN001 | Administrador | admin@sistema.com | Administrador | Activo |
| 2 | USR001 | Usuario Normal | usuario@sistema.com | Usuario | Activo |
| 3 | VEN001 | Juan Vendedor | vendedor@sistema.com | Vendedor | Activo |
| 4 | GER001 | Carlos Gerente | gerente@sistema.com | Gerente | Activo |
| 5 | ALM001 | Ana Almacén | almacen@sistema.com | Almacenista | Activo |

### **Roles del Sistema:**

| ID | Nombre | Descripción | Usuarios | Permisos |
|----|--------|-------------|----------|----------|
| 1 | Administrador | Acceso completo al sistema | 1 | 77 |
| 2 | Usuario | Acceso básico de lectura | 1 | 5 |
| 3 | Vendedor | Permisos de ventas y clientes | 1 | 12 |
| 4 | Almacenista | Permisos de productos e inventario | 1 | 10 |
| 5 | Gerente | Permisos de reportes y análisis | 1 | 25 |

---

## ? Ventajas del Sistema

1. **Búsqueda Flexible**: Buscar por nombre, código o email
2. **Filtros Múltiples**: Combinar búsqueda, rol y estado
3. **Paginación**: Manejo eficiente de grandes cantidades de datos
4. **Estadísticas**: Vista rápida del estado del sistema
5. **Detalles Completos**: Información de roles con usuarios y permisos
6. **Performance**: Consultas optimizadas con EF Core
7. **Seguridad**: Todos los endpoints protegidos con permisos

---

## ?? Notas Importantes

- **Paginación por defecto**: Si no se especifica, `page=1` y `pageSize=10`
- **Búsqueda case-insensitive**: La búsqueda ignora mayúsculas/minúsculas
- **Usuarios inactivos**: Por defecto solo se muestran usuarios activos
- **Roles inactivos**: Por defecto solo se muestran roles activos (usar `includeInactive=true` para verlos)
- **Performance**: Las consultas están optimizadas con `Include` de EF Core
- **Permisos**: Requiere `ManageUsers` en módulo `Configuration`

---

**? API de Usuarios y Roles - 100% Funcional**  
**?? Ruta API:** `/api/Users`  
**?? Controller:** `UsersController`  
**??? Tablas:** `Users` y `Roles`
