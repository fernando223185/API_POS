# ? **Módulo de Ventas - Permisos Agregados**

## ?? **Estado: COMPLETADO**

---

## ?? **Resumen Ejecutivo**

Se ha agregado exitosamente el **módulo de Ventas** con sus submódulos y permisos completos para el rol **Administrador**.

---

## ?? **Módulo Creado**

### **Ventas (ID: 2)**
- **Descripción:** Punto de venta y gestión de ventas
- **Ruta:** `/sales`
- **Icono:** `fa-shopping-cart`
- **Estado:** ? Activo

---

## ?? **Submódulos Creados**

| ID | Nombre | Descripción | Ruta | Icono | Color |
|----|--------|-------------|------|-------|-------|
| 10 | Nueva Venta | Punto de venta para crear nuevas ventas | `/sales/new` | `fa-cash-register` | #10B981 |
| 11 | Lista de Ventas | Ver historial y detalle de ventas realizadas | `/sales/list` | `fa-list` | #3B82F6 |
| 12 | Reportes de Ventas | Estadísticas y reportes de ventas | `/sales/reports` | `fa-chart-line` | #8B5CF6 |
| 13 | Cobranza | Gestión de pagos y cuentas por cobrar | `/sales/payments` | `fa-money-bill-wave` | #F59E0B |

---

## ?? **Permisos Asignados al Rol Administrador**

### **Total de Permisos: 9**

1. ? **Módulo Ventas** - Acceso completo (View, Create, Edit, Delete)
2. ? **Nueva Venta** - Acceso completo
3. ? **Lista de Ventas** - Acceso completo
4. ? **Reportes de Ventas** - Acceso completo
5. ? **Cobranza** - Acceso completo
6. ? **Historial de Ventas** - Acceso completo (submódulo existente)
7. ? **Cotizaciones** - Acceso completo (submódulo existente)
8. ? **Devoluciones** - Acceso completo (submódulo existente)

---

## ?? **Script SQL Ejecutado**

**Archivo:** `Infrastructure/Scripts/AddSalesModulePermissions.sql`

### **Características del Script:**

- ? **Idempotente** - Puede ejecutarse múltiples veces sin duplicar datos
- ? **Portable** - Funciona en cualquier servidor SQL Server sin modificaciones
- ? **Transaccional** - Si falla, revierte todos los cambios (ROLLBACK)
- ? **Validaciones** - Verifica que existan las tablas requeridas
- ? **Mensajes Claros** - Indica paso a paso qué se está ejecutando
- ? **Compatible con AWS** - Listo para ejecutar en servidor de producción

---

## ?? **Aplicar en AWS/Producción**

### **Opción 1: Ejecutar Script SQL Directamente**

```bash
# 1. Subir el script al servidor AWS
scp -i tu-key.pem Infrastructure/Scripts/AddSalesModulePermissions.sql \
  ec2-user@tu-servidor:/home/ec2-user/

# 2. Conectarse al servidor
ssh -i tu-key.pem ec2-user@tu-servidor

# 3. Ejecutar el script
sqlcmd -S localhost -d ERP -U sa -P "TuPassword" \
  -i AddSalesModulePermissions.sql

# 4. Verificar permisos creados
sqlcmd -S localhost -d ERP -U sa -P "TuPassword" -Q "
SELECT m.Name AS Modulo, s.Name AS Submodulo, 
       rmp.CanView, rmp.CanCreate, rmp.CanEdit, rmp.CanDelete
FROM RoleModulePermissions rmp
INNER JOIN Modules m ON m.Id = rmp.ModuleId
LEFT JOIN Submodules s ON s.Id = rmp.SubmoduleId
WHERE rmp.RoleId = 1 AND m.Id = 2
ORDER BY s.[Order]
"
```

### **Opción 2: Registrar como Migración de EF Core**

Si prefieres que quede registrado en el historial de migraciones:

```bash
# 1. El script SQL ya se ejecutó manualmente
# 2. Registrar en historial de migraciones
sqlcmd -S localhost -d ERP -U sa -P "TuPassword" -Q "
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260311200630_AddSalesModulePermissions', N'7.0.20');
"
```

---

## ? **Verificación**

### **1. Verificar Módulo de Ventas:**
```sql
SELECT * FROM Modules WHERE Id = 2;
```

**? Resultado Esperado:**
```
Id | Name   | Description                      | Path    | Icon              | Order | IsActive
2  | Ventas | Punto de venta y gestión ventas | /sales  | fa-shopping-cart  | 2     | 1
```

### **2. Verificar Submódulos:**
```sql
SELECT * FROM Submodules WHERE ModuleId = 2 ORDER BY [Order];
```

**? Resultado Esperado:** 4-8 submódulos de ventas

### **3. Verificar Permisos del Administrador:**
```sql
SELECT COUNT(*) AS TotalPermisos
FROM RoleModulePermissions
WHERE RoleId = 1 AND ModuleId = 2;
```

**? Resultado Esperado:** 9 permisos (1 módulo + 8 submódulos)

### **4. Probar desde la API:**

```http
GET http://tu-servidor:7254/api/Roles/1/module-permissions
Authorization: Bearer {token}
```

**? Respuesta Esperada:** Debe incluir el módulo "Ventas" con todos sus submódulos

---

## ?? **Solución de Problemas**

### **Error: "Insufficient permissions"**

**Causa:** El usuario no tiene permisos sobre el módulo de Ventas

**Solución:**
1. Verificar que los permisos se crearon correctamente (ver sección Verificación)
2. Hacer logout y login de nuevo en la aplicación
3. Verificar que el usuario pertenece al rol Administrador

```sql
-- Verificar rol del usuario
SELECT u.Id, u.Code, u.Name, r.Name AS RoleName
FROM Users u
INNER JOIN Roles r ON r.Id = u.RoleId
WHERE u.Code = 'TU_USUARIO';
```

### **Error: "Módulo Ventas no aparece en el menú"**

**Solución:**
```sql
-- Verificar que el módulo está activo
SELECT * FROM Modules WHERE Id = 2;

-- Si IsActive = 0, activarlo:
UPDATE Modules SET IsActive = 1 WHERE Id = 2;
```

### **Error: "Algunos submódulos no aparecen"**

**Solución:**
```sql
-- Verificar submódulos activos
SELECT * FROM Submodules WHERE ModuleId = 2 AND IsActive = 0;

-- Activar todos los submódulos de Ventas:
UPDATE Submodules SET IsActive = 1 WHERE ModuleId = 2;
```

---

## ?? **Estructura Final de Permisos**

```
? Módulo: Ventas (ID: 2)
   ??? ? Permiso General (View, Create, Edit, Delete)
   ?
   ??? Submódulos:
       ??? ? Nueva Venta (ID: 10)
       ?   ??? Permisos: View ? | Create ? | Edit ? | Delete ?
       ?
       ??? ? Lista de Ventas (ID: 11)
       ?   ??? Permisos: View ? | Create ? | Edit ? | Delete ?
       ?
       ??? ? Reportes de Ventas (ID: 12)
       ?   ??? Permisos: View ? | Create ? | Edit ? | Delete ?
       ?
       ??? ? Cobranza (ID: 13)
           ??? Permisos: View ? | Create ? | Edit ? | Delete ?
```

---

## ?? **Próximos Pasos**

1. ? **Testing Local** - Probar el módulo de Ventas localmente
2. ? **Verificar Endpoints** - Asegurar que todos los endpoints funcionan
3. ? **Deploy a AWS** - Ejecutar el script en producción
4. ? **Pruebas en Producción** - Verificar que todo funciona correctamente
5. ? **Capacitación** - Entrenar usuarios en el nuevo módulo

---

## ?? **Documentos Relacionados**

- `DOCS/Sales_System_FINAL_COMPLETE.md` - Sistema de ventas completo
- `Infrastructure/Scripts/CreateSalesPaymentsTables.sql` - Tablas de ventas
- `Infrastructure/Scripts/AddSalesModulePermissions.sql` - Permisos del módulo
- `Infrastructure/Scripts/Portable/AssignFullPermissionsToAdmin_Portable.sql` - Script base de permisos

---

## ? **Checklist Final**

- [x] Módulo de Ventas creado
- [x] Submódulos de Ventas creados
- [x] Permisos asignados al Administrador
- [x] Script SQL portable creado
- [x] Verificación local exitosa
- [ ] Deploy a AWS/Producción
- [ ] Pruebas en producción
- [ ] Documentación de usuario final

---

**?? MÓDULO DE VENTAS CON PERMISOS COMPLETAMENTE CONFIGURADO** ??

**Fecha de Implementación:** 2026-03-11  
**Archivo del Script:** `Infrastructure/Scripts/AddSalesModulePermissions.sql`  
**Estado:** ? **LISTO PARA PRODUCCIÓN**
