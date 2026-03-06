# ? **FORMATO JSON UNIFICADO - Roles y Usuarios**

## ?? **Objetivo**

El formato JSON para guardar permisos ahora es **IDÉNTICO** tanto para **Roles** como para **Usuarios**.

---

## ?? **FORMATO UNIFICADO**

### **Para Roles:**
```
POST /api/Roles/{id}/module-permissions
```

### **Para Usuarios:**
```
POST /api/Permissions/user/save-custom
```

---

## ?? **ESTRUCTURA JSON IDÉNTICA**

### **Request para Roles:**

```json
{
  "roleId": 3,
  "modules": [
    {
      "moduleId": 2,
      "moduleName": "Ventas",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 21,
          "submoduleName": "Nueva Venta",
          "hasAccess": true,
          "canView": true,
          "canCreate": true,
          "canEdit": false,
          "canDelete": false
        },
        {
          "submoduleId": 22,
          "submoduleName": "Historial de Ventas",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        }
      ]
    },
    {
      "moduleId": 3,
      "moduleName": "Productos",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 31,
          "submoduleName": "Catálogo de Productos",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        }
      ]
    }
  ]
}
```

### **Request para Usuarios (IDÉNTICO):**

```json
{
  "userId": 7,
  "modules": [
    {
      "moduleId": 2,
      "moduleName": "Ventas",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 21,
          "submoduleName": "Nueva Venta",
          "hasAccess": true,
          "canView": true,
          "canCreate": true,
          "canEdit": false,
          "canDelete": false
        },
        {
          "submoduleId": 22,
          "submoduleName": "Historial de Ventas",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        }
      ]
    },
    {
      "moduleId": 3,
      "moduleName": "Productos",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 31,
          "submoduleName": "Catálogo de Productos",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        }
      ]
    }
  ]
}
```

---

## ?? **DIFERENCIA MÍNIMA**

La **única diferencia** es el campo raíz:
- **Roles:** `roleId`
- **Usuarios:** `userId`

Todo lo demás es **100% idéntico**.

---

## ?? **COMPARACIÓN DE CAMPOS**

| Campo | Roles | Usuarios | Tipo | Descripción |
|-------|-------|----------|------|-------------|
| **ID** | `roleId` | `userId` | `int` | Identificador del rol/usuario |
| **modules** | ? | ? | `array` | Lista de módulos con permisos |
| **moduleId** | ? | ? | `int` | ID del módulo |
| **moduleName** | ? | ? | `string` | Nombre del módulo |
| **hasAccess** | ? | ? | `bool` | Si tiene acceso al módulo |
| **submodules** | ? | ? | `array` | Lista de submódulos |
| **submoduleId** | ? | ? | `int` | ID del submódulo |
| **submoduleName** | ? | ? | `string` | Nombre del submódulo |
| **canView** | ? | ? | `bool` | Permiso de lectura |
| **canCreate** | ? | ? | `bool` | Permiso de creación |
| **canEdit** | ? | ? | `bool` | Permiso de edición |
| **canDelete** | ? | ? | `bool` | Permiso de eliminación |

---

## ? **VENTAJAS DEL FORMATO UNIFICADO**

### **1. Consistencia**
- Mismo formato para roles y usuarios
- Fácil de aprender y usar
- Menor probabilidad de errores

### **2. Reutilización de Código**
- Misma lógica de validación
- Mismos componentes de UI en frontend
- DTOs reutilizables

### **3. Mantenibilidad**
- Cambios en un lugar afectan a ambos
- Documentación simplificada
- Testing más sencillo

---

## ?? **EJEMPLOS DE USO**

### **Ejemplo 1: Asignar Permisos al Rol Vendedor**

```bash
POST http://localhost:7254/api/Roles/3/module-permissions
Content-Type: application/json
Authorization: Bearer {token}

{
  "roleId": 3,
  "modules": [
    {
      "moduleId": 2,
      "moduleName": "Ventas",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 21,
          "submoduleName": "Nueva Venta",
          "hasAccess": true,
          "canView": true,
          "canCreate": true,
          "canEdit": true,
          "canDelete": false
        },
        {
          "submoduleId": 22,
          "submoduleName": "Historial de Ventas",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        }
      ]
    },
    {
      "moduleId": 5,
      "moduleName": "Clientes",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 51,
          "submoduleName": "Listado de Clientes",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        },
        {
          "submoduleId": 52,
          "submoduleName": "Nuevo Cliente",
          "hasAccess": true,
          "canView": true,
          "canCreate": true,
          "canEdit": false,
          "canDelete": false
        }
      ]
    }
  ]
}
```

**Response:**
```json
{
  "message": "Permisos del rol guardados exitosamente",
  "error": 0,
  "roleId": 3,
  "roleName": "Vendedor",
  "totalModules": 2,
  "totalSubmodules": 4,
  "permissionsCreated": 6
}
```

---

### **Ejemplo 2: Asignar Permisos Personalizados a un Usuario**

```bash
POST http://localhost:7254/api/Permissions/user/save-custom
Content-Type: application/json
Authorization: Bearer {token}

{
  "userId": 7,
  "modules": [
    {
      "moduleId": 6,
      "moduleName": "CFDI",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 62,
          "submoduleName": "Facturas Emitidas",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        }
      ]
    },
    {
      "moduleId": 7,
      "moduleName": "Reportes",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 71,
          "submoduleName": "Reporte de Ventas",
          "hasAccess": true,
          "canView": true,
          "canCreate": false,
          "canEdit": false,
          "canDelete": false
        }
      ]
    }
  ]
}
```

**Response:**
```json
{
  "message": "Permisos guardados exitosamente",
  "error": 0,
  "userId": 7,
  "totalPermissionsSaved": 4,
  "savedAt": "2025-01-07T15:30:00Z"
}
```

---

## ?? **CAMPOS DEL REQUEST**

### **Nivel Raíz:**
```typescript
{
  roleId?: number;      // Para roles
  userId?: number;      // Para usuarios
  modules: Module[];    // Array de módulos
}
```

### **Nivel Módulo:**
```typescript
{
  moduleId: number;     // ID del módulo (requerido)
  moduleName: string;   // Nombre del módulo (requerido)
  hasAccess: boolean;   // Si tiene acceso al módulo (requerido)
  submodules: Submodule[]; // Array de submódulos (requerido)
}
```

### **Nivel Submódulo:**
```typescript
{
  submoduleId: number;   // ID del submódulo (requerido)
  submoduleName: string; // Nombre del submódulo (requerido)
  hasAccess: boolean;    // Si tiene acceso al submódulo (requerido)
  canView: boolean;      // Permiso de lectura (requerido)
  canCreate: boolean;    // Permiso de creación (requerido)
  canEdit: boolean;      // Permiso de edición (requerido)
  canDelete: boolean;    // Permiso de eliminación (requerido)
}
```

---

## ?? **VALIDACIONES**

### **Reglas de Negocio:**

1. ? **Al menos un módulo** debe tener `hasAccess = true`
2. ? Si un submódulo tiene `hasAccess = true`, al menos una acción debe ser `true`
3. ? Los IDs de módulos y submódulos deben existir en el sistema
4. ? `moduleName` y `submoduleName` deben coincidir con los del sistema

---

## ?? **COMPARACIÓN: FORMATO ANTIGUO vs NUEVO**

### **? Formato Antiguo (UserPermissions):**

```json
{
  "userId": 7,
  "modules": [
    {
      "moduleId": 2,
      "name": "Ventas",
      "path": "/sales",
      "icon": "faShoppingCart",
      "order": 2,
      "hasAccess": true,
      "permissions": {
        "canView": false,
        "canCreate": false,
        "canEdit": false,
        "canDelete": false
      },
      "submodules": [
        {
          "submoduleId": 21,
          "moduleId": 2,
          "name": "Nueva Venta",
          "path": "/sales/new",
          "icon": "faPlus",
          "order": 1,
          "hasAccess": true,
          "permissions": {
            "canView": true,
            "canCreate": true,
            "canEdit": false,
            "canDelete": false
          }
        }
      ]
    }
  ]
}
```

**Problemas:**
- ? Demasiados campos innecesarios (`path`, `icon`, `order`)
- ? Objeto `permissions` separado (inconsistente)
- ? Diferente al formato de Roles

---

### **? Formato Nuevo (UNIFICADO):**

```json
{
  "userId": 7,
  "modules": [
    {
      "moduleId": 2,
      "moduleName": "Ventas",
      "hasAccess": true,
      "submodules": [
        {
          "submoduleId": 21,
          "submoduleName": "Nueva Venta",
          "hasAccess": true,
          "canView": true,
          "canCreate": true,
          "canEdit": false,
          "canDelete": false
        }
      ]
    }
  ]
}
```

**Ventajas:**
- ? Solo campos necesarios
- ? Permisos directos (no objeto anidado)
- ? **Idéntico al formato de Roles**
- ? Más simple y claro

---

## ?? **USO EN FRONTEND (React/TypeScript)**

```typescript
// Tipo unificado para roles y usuarios
interface SavePermissionsRequest {
  roleId?: number;
  userId?: number;
  modules: ModulePermission[];
}

interface ModulePermission {
  moduleId: number;
  moduleName: string;
  hasAccess: boolean;
  submodules: SubmodulePermission[];
}

interface SubmodulePermission {
  submoduleId: number;
  submoduleName: string;
  hasAccess: boolean;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

// Función para guardar permisos de rol
const saveRolePermissions = async (roleId: number, permissions: SavePermissionsRequest) => {
  const response = await fetch(`/api/Roles/${roleId}/module-permissions`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ ...permissions, roleId })
  });
  return response.json();
};

// Función para guardar permisos de usuario (MISMA ESTRUCTURA)
const saveUserPermissions = async (userId: number, permissions: SavePermissionsRequest) => {
  const response = await fetch('/api/Permissions/user/save-custom', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ ...permissions, userId })
  });
  return response.json();
};

// Componente reutilizable para ambos
const PermissionsEditor = ({ entityId, entityType }: { entityId: number, entityType: 'role' | 'user' }) => {
  const savePermissions = entityType === 'role' ? saveRolePermissions : saveUserPermissions;
  
  // ... lógica del componente ...
};
```

---

## ? **RESUMEN**

| Característica | Antes | Ahora |
|----------------|-------|-------|
| **Formato Roles** | ? Simple | ? Simple |
| **Formato Usuarios** | ? Complejo | ? Simple (IDÉNTICO) |
| **Consistencia** | ? Diferente | ? **100% Idéntico** |
| **Campos Innecesarios** | ? Muchos | ? Solo necesarios |
| **Reutilización Frontend** | ? Componentes diferentes | ? **Mismo componente** |
| **Documentación** | ? Duplicada | ? **Una sola** |

---

**? FORMATO JSON UNIFICADO IMPLEMENTADO** ??

**Endpoints:**
- `POST /api/Roles/{id}/module-permissions` - Permisos de rol
- `POST /api/Permissions/user/save-custom` - Permisos de usuario

**Formato:** ? **100% IDÉNTICO**
