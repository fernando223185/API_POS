# ? **SOLUCIÓN DEFINITIVA: Limpiar BD y Aplicar Migraciones**

## ?? **LA MEJOR SOLUCIÓN**

**NO uses scripts SQL manuales.** La forma correcta es:

1. ? **Eliminar la base de datos**
2. ? **Dejar que Entity Framework cree todo desde cero**
3. ? **Las migraciones se aplicarán automáticamente**

---

## ?? **SOLUCIÓN RÁPIDA (3 Comandos)**

```bash
# 1. Conectarse a AWS
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

# 2. Eliminar y recrear BD
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"

# 3. Iniciar API (aplicará migraciones automáticamente)
cd ~/api/publish
dotnet Web.Api.dll
```

**? Resultado esperado:**

```
?? Verificando migraciones de base de datos...
??  Hay XX migraciones pendientes
?? Aplicando migraciones...
? Migraciones aplicadas exitosamente
? Base de datos actualizada

? 8 roles creados exitosamente
? Usuario ADMIN001 creado exitosamente
   ?? Email: admin@sistema.com
   ?? Password: admin123

?? ERP POS API Server started successfully
```

---

## ?? **PASOS DETALLADOS**

### **PASO 1: Conectarse a SQL Server en AWS**

```bash
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C
```

### **PASO 2: Eliminar y Recrear BD**

```sql
-- Ver bases de datos
SELECT name FROM sys.databases;
GO

-- Eliminar BD (ADVERTENCIA: Borrará todos los datos)
DROP DATABASE ERP;
GO

-- Crear BD limpia
CREATE DATABASE ERP;
GO

-- Verificar
SELECT name FROM sys.databases WHERE name = 'ERP';
GO

EXIT
```

### **PASO 3: Detener API (si está corriendo)**

```bash
# Si está corriendo manualmente: Ctrl+C
# Si es servicio:
sudo systemctl stop api-pos
```

### **PASO 4: Iniciar API**

```bash
cd ~/api/publish
dotnet Web.Api.dll
```

**? La API automáticamente:**
1. Detecta que hay migraciones pendientes
2. Aplica TODAS las migraciones en orden
3. Crea las tablas con nombres correctos: `Modules`, `Submodules`
4. Crea 8 roles
5. Crea usuario ADMIN001

---

## ?? **VERIFICACIÓN**

### **Test 1: Login**

```bash
curl -X POST http://localhost:7254/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{"code":"ADMIN001","password":"admin123"}'
```

**? Respuesta esperada:**
```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGci..."
}
```

### **Test 2: Verificar Tablas**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C -Q "SELECT name FROM sys.tables ORDER BY name;"
```

**? Deberías ver:**
```
Modules          ? NOMBRE CORRECTO
Submodules       ? NOMBRE CORRECTO
RoleModulePermissions
UserModulePermissions
Users
Roles
Products
Customers
...
```

### **Test 3: Migraciones Aplicadas**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C -Q "SELECT COUNT(*) AS TotalMigraciones FROM __EFMigrationsHistory;"
```

---

## ? **VENTAJAS DE ESTE MÉTODO**

| Ventaja | Descripción |
|---------|-------------|
| **? Profesional** | Método estándar de Entity Framework |
| **? Limpio** | Sin conflictos ni tablas duplicadas |
| **? Automático** | No necesitas scripts SQL manuales |
| **? Consistente** | Funciona igual en desarrollo, AWS, producción |
| **? Rastreable** | Todas las migraciones en `__EFMigrationsHistory` |
| **? Reversible** | Puedes hacer rollback si es necesario |

---

## ?? **LO QUE YA NO NECESITAS**

- ? Scripts SQL manuales (`AWS_FixMigrations.sql`)
- ? Renombrar tablas manualmente
- ? Editar `__EFMigrationsHistory` a mano
- ? Marcar migraciones como aplicadas

**Todo lo gestiona Entity Framework automáticamente** ?

---

## ?? **CHECKLIST**

```
? 1. SSH a AWS
? 2. DROP DATABASE ERP
? 3. CREATE DATABASE ERP
? 4. dotnet Web.Api.dll
? 5. Verificar migraciones aplicadas ?
? 6. Verificar roles creados ?
? 7. Verificar ADMIN001 creado ?
? 8. Probar login ?
? 9. Probar Swagger ?
```

---

## ?? **RESUMEN**

### **Problema Original:**
```
? Error: IDENTITY property conflict
? Tablas con nombres incorrectos
? Scripts SQL causando problemas
```

### **Solución Definitiva:**
```
? DROP DATABASE ERP
? CREATE DATABASE ERP
? dotnet Web.Api.dll
   ? Entity Framework crea todo correctamente
```

### **Resultado:**
```
? Todas las migraciones aplicadas
? Tablas con nombres correctos
? Roles y usuario creados
? Sistema funcionando perfectamente
```

---

**?? ESTE ES EL MÉTODO PROFESIONAL Y RECOMENDADO**

**Documentación completa:** `DOCS/Clean_Database_Fresh_Start.md`
