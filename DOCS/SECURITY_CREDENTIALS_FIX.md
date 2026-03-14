# ?? REMEDIACIÓN DE CREDENCIALES EXPUESTAS EN GIT

## ? Problema Detectado

GitHub bloqueó el push porque detectó credenciales de AWS en:
- `Web.Api/appsettings.json:11` - AWS Access Key ID
- `Web.Api/appsettings.json:12` - AWS Secret Access Key

```
AWS Access Key: AKIAXXXXXXXXXXXXXXXXX (ejemplo)
AWS Secret Key: *********************************** (ejemplo)
```

---

## ?? ACCIÓN INMEDIATA REQUERIDA

### 1. **?? REVOCAR LAS CREDENCIALES DE AWS**

**IMPORTANTE**: Estas credenciales están comprometidas. Debes rotarlas AHORA.

#### En AWS Console:

```bash
1. Ir a: https://console.aws.amazon.com/iam/
2. Clic en "Users" ? Seleccionar tu usuario
3. Tab "Security credentials"
4. Encontrar la Access Key comprometida
5. Clic en "Actions" ? "Deactivate" ? "Delete"
6. Crear una nueva Access Key
7. Guardar las nuevas credenciales de forma SEGURA
```

---

## ? SOLUCIÓN PASO A PASO

### Paso 1: Actualizar archivos locales

Ya hemos removido las credenciales de los archivos versionados:
- ? `Web.Api/appsettings.json` - Usa placeholders
- ? `Web.Api/appsettings.Production.json` - Usa placeholders
- ? `Web.Api/appsettings.Local.json` - Creado (no versionado)

### Paso 2: Actualizar `.gitignore`

Necesitamos asegurar que los archivos con credenciales nunca se suban:

```bash
# En la raíz del proyecto
echo "" >> .gitignore
echo "# ?? Security: Never commit sensitive config files" >> .gitignore
echo "**/appsettings.json" >> .gitignore
echo "**/appsettings.*.json" >> .gitignore
echo "!**/appsettings.Example.json" >> .gitignore
echo "**/appsettings.Local.json" >> .gitignore
```

### Paso 3: Limpiar el historial de Git

**OPCIÓN A: Amend del último commit (si solo está en el último commit)**

```powershell
# Verificar el estado
git status

# Ver los cambios del último commit
git log -1

# Si las credenciales solo están en el último commit NO pusheado
git reset --soft HEAD~1
git add .
git commit -m "feat: Implement billing endpoint for invoice generation

- Add GET /api/billing/sale/{saleId} endpoint
- Include complete company and customer fiscal data
- Add DTOs for invoicing with SAT keys
- Implement validations for invoice requirements
- Update documentation

Security: Remove sensitive credentials from config files"
```

**OPCIÓN B: Usar BFG Repo-Cleaner (recomendado para limpiar TODO el historial)**

```powershell
# 1. Hacer backup del repositorio
cd C:\Users\PCX\source\repos
xcopy API_POS API_POS_BACKUP /E /I /H

# 2. Descargar BFG Repo-Cleaner
# https://rtyley.github.io/bfg-repo-cleaner/
# Guardar bfg.jar en C:\Users\PCX\Downloads\

# 3. Crear archivo con patrones a remover
$sensitivePatternsFile = "C:\Users\PCX\Downloads\sensitive-patterns.txt"
@"
[EXAMPLE-ACCESS-KEY]
[EXAMPLE-SECRET-KEY]
[EXAMPLE-PASSWORD]
[EXAMPLE-JWT-KEY]
"@ | Out-File -FilePath $sensitivePatternsFile -Encoding UTF8

# 4. Ejecutar BFG para limpiar el historial
cd C:\Users\PCX\source\repos\API_POS
java -jar C:\Users\PCX\Downloads\bfg.jar --replace-text C:\Users\PCX\Downloads\sensitive-patterns.txt

# 5. Forzar limpieza de Git
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# 6. Verificar que se limpiaron
git log --all --full-history --source --oneline -- "**/appsettings.json"
```

**OPCIÓN C: Usar git filter-repo (alternativa moderna)**

```powershell
# 1. Instalar git-filter-repo
pip install git-filter-repo

# 2. Remover archivos sensibles del historial
cd C:\Users\PCX\source\repos\API_POS
git filter-repo --path Web.Api/appsettings.json --invert-paths
git filter-repo --path Web.Api/appsettings.Production.json --invert-paths

# 3. Recrear archivos con placeholders
git checkout main  # o master
# Los archivos se recrearán con los cambios actuales
```

### Paso 4: Force Push (con precaución)

```powershell
# ?? ADVERTENCIA: Esto reescribirá el historial del repositorio
# Solo hazlo si estás seguro y nadie más está trabajando en el repo

# Verificar que todo está limpio
git status
git log --oneline -5

# Force push
git push origin master --force

# O si prefieres ser más cuidadoso
git push origin master --force-with-lease
```

---

## ?? CONFIGURACIÓN SEGURA PARA EL FUTURO

### Opción 1: User Secrets (Desarrollo Local) ? RECOMENDADO

```powershell
# En el directorio Web.Api
cd Web.Api

# Inicializar user secrets
dotnet user-secrets init

# Agregar credenciales (REEMPLAZA CON TUS CREDENCIALES REALES)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ERP;User Id=sa;Password=YOUR_PASSWORD;..."
dotnet user-secrets set "Jwt:Key" "YOUR_JWT_SECRET_KEY_AT_LEAST_32_CHARACTERS"
dotnet user-secrets set "AWS:AccessKey" "YOUR_NEW_AWS_ACCESS_KEY"
dotnet user-secrets set "AWS:SecretKey" "YOUR_NEW_AWS_SECRET_KEY"

# Listar secrets configurados
dotnet user-secrets list

# Los secrets se guardan en:
# Windows: %APPDATA%\Microsoft\UserSecrets\{user-secrets-id}\secrets.json
# Linux: ~/.microsoft/usersecrets/{user-secrets-id}/secrets.json
```

### Opción 2: Variables de Entorno

**Windows (PowerShell):**
```powershell
# Temporal (solo sesión actual)
$env:ConnectionStrings__DefaultConnection = "Server=..."
$env:Jwt__Key = "TuClaveSecreta..."
$env:AWS__AccessKey = "YOUR_ACCESS_KEY"
$env:AWS__SecretKey = "YOUR_SECRET_KEY"

# Permanente (usuario)
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=...", "User")
[System.Environment]::SetEnvironmentVariable("AWS__AccessKey", "YOUR_ACCESS_KEY", "User")
[System.Environment]::SetEnvironmentVariable("AWS__SecretKey", "YOUR_SECRET_KEY", "User")
```

**Linux (AWS EC2):**
```bash
# Agregar al ~/.bashrc o /etc/environment
export ConnectionStrings__DefaultConnection="Server=..."
export Jwt__Key="TuClaveSecreta..."
export AWS__AccessKey="YOUR_ACCESS_KEY"
export AWS__SecretKey="YOUR_SECRET_KEY"

# Recargar
source ~/.bashrc
```

### Opción 3: AWS Systems Manager Parameter Store (Producción)

```bash
# Guardar credenciales en Parameter Store
aws ssm put-parameter \
    --name "/erp-pos/prod/db-connection" \
    --value "Server=..." \
    --type "SecureString"

aws ssm put-parameter \
    --name "/erp-pos/prod/jwt-key" \
    --value "TuClaveSecreta..." \
    --type "SecureString"
```

Luego modificar `Program.cs`:
```csharp
// Cargar desde AWS Parameter Store
var ssmClient = new AmazonSimpleSystemsManagementClient();
var dbConnection = await ssmClient.GetParameterAsync(new GetParameterRequest
{
    Name = "/erp-pos/prod/db-connection",
    WithDecryption = true
});
builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnection.Parameter.Value;
```

---

## ?? Crear archivo appsettings.Example.json

```powershell
# Crear archivo de ejemplo (para que otros desarrolladores sepan qué configurar)
cd Web.Api

$exampleContent = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ERP;User Id=sa;Password=YOUR_PASSWORD;Trusted_Connection=false;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "YOUR_JWT_SECRET_KEY_HERE_AT_LEAST_32_CHARACTERS",
    "Issuer": "TuAplicacion",
    "Audience": "TuAplicacion"
  },
  "AWS": {
    "AccessKey": "YOUR_AWS_ACCESS_KEY_ID",
    "SecretKey": "YOUR_AWS_SECRET_ACCESS_KEY",
    "Region": "us-east-1",
    "S3": {
      "BucketName": "your-bucket-name"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
"@

$exampleContent | Out-File -FilePath "appsettings.Example.json" -Encoding UTF8
```

---

## ?? Verificación

### 1. Verificar que los archivos sensibles están ignorados

```powershell
# Verificar .gitignore
git check-ignore -v Web.Api/appsettings.json
git check-ignore -v Web.Api/appsettings.Local.json
git check-ignore -v Web.Api/appsettings.Production.json

# Debe mostrar:
# .gitignore:XX:**/appsettings.json   Web.Api/appsettings.json
```

### 2. Verificar que no hay credenciales en el historial

```powershell
# Buscar en todo el historial
git log --all --full-history --source -- "**/appsettings*.json"

# Buscar texto específico en el historial (REEMPLAZA CON TUS CREDENCIALES)
git log -S "[YOUR-ACCESS-KEY-PREFIX]" --source --all

# No debe encontrar nada
```

### 3. Verificar archivos que se van a pushear

```powershell
git status
git diff --cached
git log origin/master..HEAD
```

---

## ? Checklist de Seguridad

- [ ] Revocar credenciales comprometidas en AWS
- [ ] Crear nuevas credenciales de AWS
- [ ] Actualizar `.gitignore` con patrones de archivos sensibles
- [ ] Remover credenciales de archivos versionados
- [ ] Crear `appsettings.Example.json` como plantilla
- [ ] Configurar User Secrets localmente
- [ ] Limpiar historial de Git (si es necesario)
- [ ] Hacer push de los cambios
- [ ] Documentar proceso para el equipo
- [ ] Configurar rotation policy en AWS (cada 90 días)

---

## ?? Soporte

Si tienes problemas con algún paso, contacta al equipo de DevSecOps.

**Fecha**: 13 de Marzo de 2026  
**Prioridad**: ?? CRÍTICA - RESOLVER INMEDIATAMENTE

**NOTA**: Todas las credenciales mostradas en este documento son ejemplos y deben ser reemplazadas con tus credenciales reales de forma segura.
