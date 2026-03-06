# ? **ACTUALIZACIÓN: Respuesta Estructurada al Crear Usuario**

## ?? **Cambio Realizado**

Se actualizó el endpoint **`POST /api/User/create`** para:
1. Retornar una respuesta estructurada con mensaje de éxito
2. Eliminar campos innecesarios del request (`LastName`, `StatusId`, `CreatedAt`)

---

## ?? **REQUEST ACTUALIZADO**

### **? Campos Requeridos:**

```json
{
  "code": "string",        // Código único del usuario (requerido)
  "name": "string",        // Nombre completo (requerido)
  "email": "string",       // Email válido (requerido)
  "phone": "string",       // Teléfono (opcional)
  "roleId": number,        // ID del rol (requerido)
  "password": "string"     // Contraseña mínimo 6 caracteres (requerido)
}
```

### **? Campos Eliminados:**

| Campo | Razón |
|-------|-------|
| `lastName` | No se usa en la entidad `User` |
| `statusId` | No existe en la entidad `User` |
| `createdAt` | Se genera automáticamente en el servidor |

---

## ?? **ANTES vs DESPUÉS**

### **? ANTES:**

```json
POST /api/User/create

Response 200 OK:
{
    "id": 7,
    "code": "FEHE-5398",
    "name": "Fernando",
    "passwordHash": "InmRENcB7MRYYQ+ti+8n+1ESXXC8ZYR8TvtgBdYng6k=",
    "email": "elherna22@gmail.com",
    "phone": "3317898058",
    "roleId": 1,
    "role": null,
    "active": true,
    "createdAt": "2026-03-05T00:02:45.9141422Z",
    "updatedAt": null,
    "deletedAt": null
}
```

**Problemas:**
- ? Solo retorna el objeto del usuario
- ? Sin mensaje de éxito
- ? Sin estructura consistente con otros endpoints
- ? Incluye `passwordHash` en la respuesta (problema de seguridad)
- ? No indica quién creó el usuario

---

### **? DESPUÉS:**

```json
POST /api/User/create

Response 200 OK:
{
    "message": "Usuario creado exitosamente",
    "error": 0,
    "data": {
        "id": 7,
        "code": "FEHE-5398",
        "name": "Fernando",
        "email": "elherna22@gmail.com",
        "phone": "3317898058",
        "roleId": 1,
        "active": true,
        "createdAt": "2026-03-05T00:02:45.9141422Z"
    },
    "createdBy": "admin"
}
```

**Mejoras:**
- ? **Mensaje de éxito** claro
- ? **Código de error** consistente (`error: 0`)
- ? **Datos del usuario** en `data` (sin `passwordHash`)
- ? **Información de auditoría** (`createdBy`)
- ? **Formato consistente** con otros endpoints del sistema

---

## ?? **SEGURIDAD MEJORADA**

### **Campos Excluidos de la Respuesta:**

| Campo | Antes | Después | Razón |
|-------|-------|---------|-------|
| `passwordHash` | ? Incluido | ? Excluido | **Seguridad** - No exponer hashes |
| `role` | ? Incluido (null) | ? Excluido | No necesario en respuesta |
| `updatedAt` | ? Incluido (null) | ? Excluido | No relevante al crear |
| `deletedAt` | ? Incluido (null) | ? Excluido | No relevante al crear |

---

## ?? **ESTRUCTURA DE RESPUESTA**

### **Campos de Respuesta:**

```typescript
interface CreateUserResponse {
  message: string;        // Mensaje de éxito/error
  error: number;          // 0 = éxito, 1 = error de validación, 2 = error interno
  data: {
    id: number;           // ID del usuario creado
    code: string;         // Código único del usuario
    name: string;         // Nombre completo
    email: string;        // Email
    phone: string;        // Teléfono
    roleId: number;       // ID del rol asignado
    active: boolean;      // Estado del usuario
    createdAt: string;    // Fecha de creación (ISO 8601)
  };
  createdBy: string;      // Usuario que realizó la creación
}
```

---

## ?? **EJEMPLOS COMPLETOS**

### **Ejemplo 1: Crear Usuario Exitosamente**

**Request:**
```bash
POST http://localhost:7254/api/User/create
Content-Type: application/json
Authorization: Bearer {token}

{
  "code": "USER001",
  "name": "Juan Pérez",
  "email": "juan.perez@email.com",
  "phone": "3312345678",
  "roleId": 3,
  "password": "password123"
}
```

**Response 200 OK:**
```json
{
  "message": "Usuario creado exitosamente",
  "error": 0,
  "data": {
    "id": 8,
    "code": "USER001",
    "name": "Juan Pérez",
    "email": "juan.perez@email.com",
    "phone": "3312345678",
    "roleId": 3,
    "active": true,
    "createdAt": "2025-01-07T16:30:00Z"
  },
  "createdBy": "admin"
}
```

---

### **Ejemplo 2: Error de Validación**

**Request:**
```bash
POST http://localhost:7254/api/User/create
Content-Type: application/json
Authorization: Bearer {token}

{
  "code": "",
  "name": "Juan Pérez",
  "email": "email-invalido",
  "password": "123"
}
```

**Response 400 Bad Request:**
```json
{
  "message": "Datos de entrada inválidos",
  "error": 1,
  "errors": [
    "The Code field is required.",
    "The Email field is not a valid email address.",
    "La contraseña debe tener al menos 6 caracteres.",
    "The RoleId field is required."
  ]
}
```

---

### **Ejemplo 3: Error Interno**

**Response 500 Internal Server Error:**
```json
{
  "message": "Error al crear usuario",
  "error": 2,
  "details": "Database connection timeout"
}
```

---

## ?? **VALIDACIONES**

| Campo | Validación | Mensaje de Error |
|-------|------------|------------------|
| **code** | Requerido, máximo 100 caracteres | "The Code field is required." |
| **name** | Requerido, máximo 255 caracteres | "The Name field is required." |
| **email** | Requerido, formato email válido | "The Email field is not a valid email address." |
| **phone** | Opcional, máximo 20 caracteres | - |
| **roleId** | Requerido, debe existir en BD | "The RoleId field is required." |
| **password** | Requerido, mínimo 6 caracteres | "La contraseña debe tener al menos 6 caracteres." |

---

## ?? **CONSISTENCIA CON OTROS ENDPOINTS**

Ahora el endpoint de creación de usuarios sigue el mismo formato que otros endpoints del sistema:

### **Clientes:**
```json
POST /api/Customer
{
  "message": "Cliente creado exitosamente",
  "error": 0,
  "data": { ... },
  "createdBy": "admin"
}
```

### **Productos:**
```json
POST /api/Products
{
  "message": "Producto creado exitosamente",
  "error": 0,
  "data": { ... },
  "createdBy": "admin"
}
```

### **Usuarios (AHORA):**
```json
POST /api/User/create
{
  "message": "Usuario creado exitosamente",
  "error": 0,
  "data": { ... },
  "createdBy": "admin"
}
```

---

## ?? **CÓDIGOS DE ERROR ESTÁNDAR**

| Código | Significado | Cuándo se Usa |
|--------|-------------|---------------|
| **0** | Éxito | Operación completada correctamente |
| **1** | Error de Validación | Datos de entrada inválidos, usuario duplicado, etc. |
| **2** | Error Interno | Errores de base de datos, excepciones no controladas |

---

## ?? **VALIDACIONES IMPLEMENTADAS**

El endpoint ahora valida:

1. ? **Modelo de entrada:** Todos los campos requeridos presentes
2. ? **Código único:** El código no debe existir
3. ? **Email válido:** Formato de email correcto
4. ? **Contraseña segura:** Mínimo 6 caracteres
5. ? **Rol válido:** El roleId debe existir
6. ? **Autenticación:** Solo usuarios autenticados pueden crear usuarios

---

## ?? **USO EN FRONTEND**

### **React/TypeScript:**

```typescript
interface CreateUserResponse {
  message: string;
  error: number;
  data: {
    id: number;
    code: string;
    name: string;
    email: string;
    phone: string;
    roleId: number;
    active: boolean;
    createdAt: string;
  };
  createdBy: string;
}

const createUser = async (userData: CreateUserRequest) => {
  try {
    const response = await fetch('http://localhost:7254/api/User/create', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(userData)
    });

    const result: CreateUserResponse = await response.json();

    if (result.error === 0) {
      // ? Éxito
      console.log(result.message); // "Usuario creado exitosamente"
      console.log('Usuario ID:', result.data.id);
      console.log('Creado por:', result.createdBy);
      
      // Mostrar notificación de éxito
      toast.success(result.message);
      
      return result.data;
    } else {
      // ? Error
      console.error(result.message);
      toast.error(result.message);
      return null;
    }
  } catch (error) {
    console.error('Error de red:', error);
    toast.error('Error al crear usuario');
    return null;
  }
};
```

---

## ? **RESUMEN DE CAMBIOS**

### **Request:**
| Campo | Antes | Ahora |
|-------|-------|-------|
| `code` | ? Requerido | ? Requerido |
| `name` | ? Requerido | ? Requerido |
| `lastName` | ?? Opcional (no se usaba) | ? **Eliminado** |
| `email` | ? Requerido | ? Requerido |
| `phone` | ? Opcional | ? Opcional |
| `roleId` | ? Requerido | ? Requerido |
| `password` | ? Requerido | ? Requerido |
| `statusId` | ?? Requerido (no se usaba) | ? **Eliminado** |
| `createdAt` | ?? Opcional (no se usaba) | ? **Eliminado** |

### **Response:**
| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Formato** | Objeto usuario directo | ? Estructurado con `message`, `error`, `data` |
| **passwordHash** | ? Incluido | ? **Eliminado** (seguridad) |
| **Mensaje** | ? Sin mensaje | ? "Usuario creado exitosamente" |
| **Auditoría** | ? Sin info | ? `createdBy` incluido |

---

**? CAMPOS INNECESARIOS ELIMINADOS** ??

**Endpoint:** `POST /api/User/create`  
**Campos Request:** 6 (code, name, email, phone, roleId, password)  
**Campos Response:** Estructurado con mensaje de éxito
