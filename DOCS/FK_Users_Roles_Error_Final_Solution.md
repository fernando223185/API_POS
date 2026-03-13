# ?? **SOLUCIÓN FINAL: Error FK Constraint en Users**

## ?? **EL PROBLEMA**

```
? Error: The INSERT statement conflicted with the FOREIGN KEY constraint 
   "FK_Users_Roles_RoleId". The conflict occurred in database "ERP", 
   table "dbo.Roles", column 'Id'.
```

**Causa:** La migración `FixUserPasswordHashes` intenta insertar usuarios con `RoleId` que **NO EXISTEN** en la tabla Roles.

---

## ?? **DIAGNÓSTICO**

### **Verificar qué roles existen en AWS:**

```bash
# Conectarse a AWS
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

# Ver roles existentes
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C \
  -Q "SELECT Id, Name FROM Roles ORDER BY Id;"
```

**? Roles que DEBERÍAN existir (según migraciones):**
```
Id  | Name
----|------------------
1   | Administrador
2   | Usuario  
3   | Vendedor
4   | Almacenista
10  | Cajero
11  | Gerente
```

**? Roles que la migración intentaba usar (INCORRECTOS):**
```
RoleId 5 ? NO EXISTE (debería ser 11 para Gerente)
RoleId 6 ? NO EXISTE (debería ser 10 para Cajero)
```

---

## ? **SOLUCIÓN: Republicar con Migración Corregida**

Ya corregí el código, ahora necesitas republicar:

### **PASO 1: Publicar en tu máquina local**

```powershell
# Ir a la raíz del proyecto
cd C:\Users\PCX\source\repos\API_POS

# Publicar
dotnet publish Web.Api -c Release -o ./publish

# Comprimir
Compress-Archive -Path .\publish\* -DestinationPath api-publish.zip -Force

# Verificar que se creó
dir api-publish.zip
```

### **PASO 2: Subir a AWS**

```powershell
# Subir archivo (reemplaza con tu clave y IP)
scp -i "C:\ruta\tu-clave.pem" api-publish.zip ubuntu@tu-ec2-ip:/home/ubuntu/
```

### **PASO 3: En AWS - Descomprimir y Reiniciar**

```bash
# Conectarse
ssh -i "tu-clave.pem" ubuntu@tu-ec2-ip

# Detener API (si está corriendo)
# Ctrl+C o sudo systemctl stop api-pos

# Limpiar carpeta
rm -rf ~/api/publish/*

# Descomprimir nueva versión
unzip -o ~/api-publish.zip -d ~/api/publish/

# Dar permisos
chmod +x ~/api/publish/Web.Api.dll

# Limpiar BD y aplicar migraciones desde cero
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C \
  -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"

# Iniciar API
cd ~/api/publish
dotnet Web.Api.dll
```

---

## ?? **RESULTADO ESPERADO**

**? Sin errores:**
```
?? Verificando migraciones de base de datos...
??  Hay XX migraciones pendientes
?? Aplicando migraciones...

info: Applying migration '20251013181709_InitialCreate'.
info: Applying migration '20251013184000_RenameUserTableToUsers'.
...
info: Applying migration '20251111195911_FixUserPasswordHashes'.
? Usuarios creados:
   - ADMIN001 (RoleId: 1)
   - VEND001 (RoleId: 3)
   - CAJ001 (RoleId: 10) ? CORREGIDO
   - GER001 (RoleId: 11) ? CORREGIDO
   - ALM001 (RoleId: 4)

? Migraciones aplicadas exitosamente
? Base de datos actualizada

?? ERP POS API Server started successfully
```

---

## ?? **VERIFICACIÓN FINAL**

### **1. Verificar Roles**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C \
  -Q "SELECT Id, Name, Description FROM Roles ORDER BY Id;"
```

**? Deberías ver:**
```
Id  | Name          | Description
----|---------------|---------------------------
1   | Administrador | Acceso completo al sistema
2   | Usuario       | Acceso básico al sistema
3   | Vendedor      | Personal de ventas
4   | Almacenista   | Gestión de inventario
10  | Cajero        | Operación de punto de venta
11  | Gerente       | Supervisión y reportes
```

### **2. Verificar Usuarios**

```bash
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -d ERP -C \
  -Q "SELECT Id, Code, Name, Email, RoleId FROM Users ORDER BY Id;"
```

**? Deberías ver:**
```
Id | Code     | Name            | Email                    | RoleId
---|----------|-----------------|--------------------------|-------
1  | ADMIN001 | Administrador   | admin@sistema.com        | 1
2  | VEND001  | Juan Vendedor   | vendedor@sistema.com     | 3
3  | CAJ001   | María Cajera    | cajero@sistema.com       | 10
4  | GER001   | Carlos Gerente  | gerente@sistema.com      | 11
5  | ALM001   | Ana Almacén     | almacen@sistema.com      | 4
```

### **3. Probar Login**

```bash
# Test con cada usuario
curl -X POST http://localhost:7254/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{"code":"ADMIN001","password":"admin123"}'

curl -X POST http://localhost:7254/api/Login/login \
  -H "Content-Type: application/json" \
  -d '{"code":"CAJ001","password":"admin123"}'
```

**? Deberías recibir:**
```json
{
  "message": "Login successful",
  "error": 0,
  "token": "eyJhbGci..."
}
```

---

## ?? **ERRORES COMUNES**

### **Error: "Sigue apareciendo el mismo error"**

**Causa:** No republicaste el código, estás usando el DLL viejo

**Solución:**
```powershell
# 1. Borrar carpeta publish local
Remove-Item -Recurse -Force .\publish

# 2. Publicar de nuevo
dotnet publish Web.Api -c Release -o ./publish

# 3. Comprimir
Compress-Archive -Path .\publish\* -DestinationPath api-publish.zip -Force

# 4. Subir a AWS
scp -i "clave.pem" api-publish.zip ubuntu@ec2-ip:/home/ubuntu/
```

### **Error: "Los roles 10 y 11 no existen"**

**Causa:** Las migraciones no se aplicaron correctamente

**Solución:**
```bash
# Limpiar BD completamente
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C \
  -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"

# Reiniciar API
dotnet Web.Api.dll
```

---

## ?? **RESUMEN DE CAMBIOS**

### **Antes (INCORRECTO):**

```csharp
// Migración FixUserPasswordHashes.cs
(3, 'CAJ001', ..., 6, 1, GETUTCDATE()),   // ? RoleId 6 NO EXISTE
(4, 'GER001', ..., 5, 1, GETUTCDATE()),   // ? RoleId 5 NO EXISTE
```

### **Después (CORREGIDO):**

```csharp
// Migración FixUserPasswordHashes.cs  
(3, 'CAJ001', ..., 10, 1, GETUTCDATE()),  // ? RoleId 10 (Cajero)
(4, 'GER001', ..., 11, 1, GETUTCDATE()),  // ? RoleId 11 (Gerente)
```

---

## ?? **CHECKLIST COMPLETO**

```
? 1. Compilar: dotnet build
? 2. Publicar: dotnet publish Web.Api -c Release -o ./publish
? 3. Comprimir: Compress-Archive -Path .\publish\* -DestinationPath api-publish.zip -Force
? 4. Subir a AWS: scp api-publish.zip ubuntu@ec2-ip:/home/ubuntu/
? 5. SSH a AWS: ssh ubuntu@ec2-ip
? 6. Limpiar: rm -rf ~/api/publish/*
? 7. Descomprimir: unzip -o ~/api-publish.zip -d ~/api/publish/
? 8. DROP DATABASE ERP; CREATE DATABASE ERP;
? 9. Iniciar API: dotnet Web.Api.dll
? 10. Verificar roles: SELECT * FROM Roles;
? 11. Verificar usuarios: SELECT * FROM Users;
? 12. Probar login: curl POST /api/Login/login
```

---

## ? **SOLUCIÓN DEFINITIVA**

**El problema NO es el código (ya está corregido), es que necesitas REPUBLICAR:**

```bash
# Desde tu máquina local (PowerShell)
dotnet publish Web.Api -c Release -o ./publish
Compress-Archive -Path .\publish\* -DestinationPath api-publish.zip -Force
scp -i "clave.pem" api-publish.zip ubuntu@ec2-ip:/home/ubuntu/

# En AWS (SSH)
rm -rf ~/api/publish/*
unzip -o ~/api-publish.zip -d ~/api/publish/
sqlcmd -S localhost -U sa -P 'Syst3m2270*' -C -Q "DROP DATABASE ERP; CREATE DATABASE ERP;"
cd ~/api/publish && dotnet Web.Api.dll
```

**?? Con esto funcionará correctamente!**
