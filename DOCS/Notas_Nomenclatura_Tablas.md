# ?? Notas sobre Nombres de Tablas

## ? ¿Por qué AppModules y AppSubmodules?

Originalmente se solicitó usar los nombres `Modules` y `Submodules`, sin embargo, estos nombres generaban un **conflicto** en la base de datos.

### ?? **Problema Detectado:**

Ya existe una tabla llamada `Modules` en el sistema que se usa para el **sistema de permisos por ROL**:

```csharp
// Domain/Entities/Module.cs
public class Module
{
    public int Id { get; set; }
    public string Name { get; set; }  // "CRM", "Sales", "Products", "Users"
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Permission> Permissions { get; set; }
}
```

Esta tabla `Modules` almacena los módulos de permisos (CRM, Sales, Products, Configuration, etc.) y tiene una relación con la tabla `Permissions` que define qué acciones puede hacer cada rol.

---

## ? **Solución Implementada:**

Se decidió usar el prefijo `App` para diferenciar claramente los dos sistemas:

### **Sistema de Permisos por ROL (Existente):**
- Tabla: `Modules`
- Propósito: Definir módulos de permisos (Create, Read, Update, Delete)
- Uso: Asignar permisos a roles
- Ejemplo: "CRM", "Sales", "Products"

### **Sistema de Menú Dinámico (Nuevo):**
- Tablas: `AppModules` + `AppSubmodules`
- Propósito: Definir estructura del menú de la aplicación
- Uso: Gestionar módulos y submódulos visibles en el sistema
- Ejemplo: "Ventas" ? "Nueva Venta", "Historial", "Cotizaciones"

---

## ?? **Ventajas de esta Nomenclatura:**

1. **Sin Conflictos**: No hay conflicto con la tabla `Modules` existente
2. **Claridad**: El prefijo `App` indica que son módulos de la aplicación (UI/Frontend)
3. **Separación**: Queda claro que hay dos sistemas diferentes:
   - `Modules` = Permisos por ROL (Backend)
   - `AppModules` = Menú de la Aplicación (Frontend)
4. **Escalabilidad**: Permite tener ambos sistemas coexistiendo sin problemas

---

## ?? **Comparación:**

| Característica | Modules (Permisos) | AppModules (Menú) |
|----------------|-------------------|-------------------|
| **Tabla** | Modules | AppModules |
| **Propósito** | Permisos por ROL | Estructura de menú |
| **Relación** | Permission ? Module | AppSubmodule ? AppModule |
| **Contenido** | CRM, Sales, Products | Ventas, Productos, Inventario |
| **ID** | Auto-incremental | Manual (1-8, 21-24, etc.) |
| **Usado en** | Sistema de autorización | Sistema de navegación |

---

## ?? **Alternativas Consideradas:**

1. ? **AppModules / AppSubmodules** (IMPLEMENTADO)
   - Pros: Claro, sin conflictos, escalable
   - Contras: Nombre un poco más largo

2. ? **MenuModules / MenuSubmodules**
   - Pros: Descriptivo
   - Contras: Muy específico, limita el uso futuro

3. ? **UIModules / UISubmodules**
   - Pros: Indica que es para UI
   - Contras: Menos semántico

4. ? **Modules / Submodules** (Solicitado originalmente)
   - Pros: Nombres simples
   - Contras: **CONFLICTO** con tabla existente `Modules`

---

## ? **Conclusión:**

La nomenclatura **AppModules** y **AppSubmodules** es la mejor solución porque:
- ? Evita conflictos con el sistema existente
- ? Es clara y descriptiva
- ? Permite que ambos sistemas coexistan
- ? Es fácil de entender para desarrolladores
- ? Sigue convenciones de nomenclatura .NET

---

**Si en el futuro se desea renombrar las tablas, se puede hacer mediante una migración, pero se recomienda mantener estos nombres para evitar confusión.**
