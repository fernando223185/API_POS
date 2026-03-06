# ?? **Sistema UNIFICADO de Permisos: ROL + USUARIO**

## ?? **Concepto del Sistema Híbrido**

### **Flujo de Permisos:**
```
ROL (Permisos Base) ? USUARIO hereda ? USUARIO + Permisos Adicionales
```

---

## ?? **Estructura JSON UNIFICADA**

### **Para ROLES (Permisos Base):**

```
POST /api/Roles/{roleId}/module-permissions
Authorization: Bearer {token}
Content-Type: application/json
```

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

---

### **Para USUARIOS (Permisos Heredados + Adicionales):**

```
POST /api/Permissions/user/save-custom
Authorization: Bearer {token}
Content-Type: application/json
```

```json
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
    }
  ]
}
```

---

## ?? **Ejemplo Completo: Vendedor con Permisos Adicionales**

### **Paso 1: Crear Rol "Vendedor" con Permisos Base**

```json
POST /api/Roles/3/module-permissions

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

**Resultado:** Todos los usuarios con rol "Vendedor" (roleId: 3) heredan estos permisos automáticamente.

---

### **Paso 2: Asignar Usuario al Rol**

```json
POST /api/Users

{
  "code": "VEN001",
  "name": "Juan Vendedor",
  "email": "juan@sistema.com",
  "phone": "5551111111",
  "roleId": 3,          // ? Hereda permisos del rol Vendedor
  "password": "password123"
}
```

**En este punto el usuario YA TIENE los permisos del rol Vendedor:**
- ? Nueva Venta (View, Create, Edit)
- ? Historial de Ventas (View)
- ? Catálogo de Productos (View)
- ? Listado de Clientes (View)
- ? Nuevo Cliente (View, Create)

---

### **Paso 3: Dar Permisos Adicionales al Usuario**

Si Juan necesita también acceso a Facturas:

```json
POST /api/Permissions/user/save-custom

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
        },
        {
          "submoduleId": 63,
          "submoduleName": "Facturas Pendientes",
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

**Ahora Juan tiene:**
- ? **Permisos del ROL** (Vendedor): Ventas, Productos, Clientes
- ? **Permisos ADICIONALES**: Facturas Emitidas, Facturas Pendientes

---

## ?? **Comparación Final:**

| Usuario | Rol | Permisos del Rol | Permisos Adicionales | Total Módulos |
|---------|-----|------------------|---------------------|---------------|
| Juan | Vendedor | Ventas, Productos, Clientes | CFDI | 4 |
| María | Vendedor | Ventas, Productos, Clientes | - | 3 |
| Pedro | Vendedor | Ventas, Productos, Clientes | CFDI, Reportes | 5 |

---

## ? **Ventajas del Sistema Unificado:**

1. **Misma estructura JSON** para roles y usuarios
2. **Mismo componente React** funciona para ambos
3. **Herencia de permisos:** Usuario hereda automáticamente los del rol
4. **Flexibilidad:** Se pueden agregar permisos adicionales por usuario
5. **Simplicidad:** No hay que mantener dos sistemas diferentes

---

## ?? **Endpoints Finales:**

### **Para ROLES:**
```
GET    /api/Roles/{id}/module-permissions          # Obtener permisos del rol
POST   /api/Roles/{id}/module-permissions          # Guardar permisos del rol
DELETE /api/Roles/{id}/module-permissions          # Eliminar todos los permisos
```

### **Para USUARIOS:**
```
GET    /api/Permissions/user/{userId}/custom       # Obtener permisos adicionales
POST   /api/Permissions/user/save-custom           # Guardar permisos adicionales
DELETE /api/Permissions/user/{userId}/custom       # Eliminar permisos adicionales
```

---

## ?? **Uso en React:**

Tu componente React actual ya funciona, solo necesitas cambiar el endpoint según si editas un ROL o un USUARIO:

```javascript
// Para ROL
const savePermissions = async (roleId, permissions) => {
  await fetch(`/api/Roles/${roleId}/module-permissions`, {
    method: 'POST',
    body: JSON.stringify({ roleId, modules: permissions })
  });
};

// Para USUARIO
const savePermissions = async (userId, permissions) => {
  await fetch(`/api/Permissions/user/save-custom`, {
    method: 'POST',
    body: JSON.stringify({ userId, modules: permissions })
  });
};
```

**El JSON es idéntico**, solo cambia el endpoint y el ID.

---

## ??? **Tablas en Base de Datos:**

```
RoleModulePermissions           UserModulePermissions
??? RoleId                      ??? UserId
??? ModuleId                    ??? ModuleId
??? SubmoduleId                 ??? SubmoduleId
??? CanView                     ??? CanView
??? CanCreate                   ??? CanCreate
??? CanEdit                     ??? CanEdit
??? CanDelete                   ??? CanDelete
```

**Mismo diseño, diferentes tablas** ?

---

**? Sistema Unificado Completo - Listo para Implementar**
