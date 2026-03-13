# ?? **Guía Completa de Publicación - ERP POS API**

## ?? **Comandos de Publicación**

### **1. Publicación Básica (Desarrollo)**

```bash
# Navegar a la raíz del proyecto
cd C:\Users\PCX\Source\Repos\API_POS

# Publicar para desarrollo
dotnet publish Web.Api/Web.Api.csproj -c Release -o ./publish
```

---

### **2. Publicación Optimizada (Producción)** ? Recomendado

```bash
# Publicación con optimizaciones para AWS/Linux
dotnet publish Web.Api/Web.Api.csproj \
  -c Release \
  -o ./publish \
  --runtime linux-x64 \
  --self-contained false \
  /p:PublishReadyToRun=true \
  /p:PublishSingleFile=false \
  /p:PublishTrimmed=false
```

**Explicación de parámetros:**
- `-c Release`: Compilación en modo Release (optimizada)
- `-o ./publish`: Carpeta de salida
- `--runtime linux-x64`: Para servidores Linux (AWS EC2)
- `--self-contained false`: Requiere .NET Runtime instalado en el servidor
- `PublishReadyToRun=true`: Pre-compilación para mejor rendimiento
- `PublishSingleFile=false`: NO crear un solo archivo (mejor para debugging)
- `PublishTrimmed=false`: NO eliminar código no usado (más seguro)

---

### **3. Publicación Auto-Contenida (Sin .NET en servidor)**

```bash
# Incluye el runtime de .NET en la publicación
dotnet publish Web.Api/Web.Api.csproj \
  -c Release \
  -o ./publish \
  --runtime linux-x64 \
  --self-contained true \
  /p:PublishReadyToRun=true
```

**?? Ventajas:**
- No necesitas instalar .NET en el servidor
- Independiente de versiones de .NET en el servidor

**?? Desventajas:**
- Tamaño más grande (~60MB vs ~10MB)
- Actualizaciones de seguridad de .NET requieren re-publicar

---

### **4. Publicación para Windows Server**

```bash
dotnet publish Web.Api/Web.Api.csproj \
  -c Release \
  -o ./publish \
  --runtime win-x64 \
  --self-contained false
```

---

## ?? **Scripts de PowerShell Automatizados**

### **Script 1: Publicación Simple**

```powershell
# Publish-Simple.ps1
$ErrorActionPreference = "Stop"

Write-Host "?? Iniciando publicación..." -ForegroundColor Green

# Limpiar carpeta de publicación anterior
if (Test-Path "./publish") {
    Write-Host "?? Limpiando carpeta anterior..." -ForegroundColor Yellow
    Remove-Item -Path "./publish" -Recurse -Force
}

# Publicar
Write-Host "?? Compilando en modo Release..." -ForegroundColor Cyan
dotnet publish Web.Api/Web.Api.csproj `
    -c Release `
    -o ./publish `
    --runtime linux-x64 `
    --self-contained false `
    /p:PublishReadyToRun=true

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Publicación completada exitosamente!" -ForegroundColor Green
    Write-Host "?? Archivos en: $(Get-Location)\publish" -ForegroundColor Cyan
    
    # Mostrar tamaño
    $size = (Get-ChildItem ./publish -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "?? Tamaño total: $([math]::Round($size, 2)) MB" -ForegroundColor Yellow
} else {
    Write-Host "? Error en la publicación" -ForegroundColor Red
    exit 1
}
```

**Uso:**
```powershell
.\Publish-Simple.ps1
```

---

### **Script 2: Publicación + Despliegue a AWS** ?

```powershell
# Publish-And-Deploy.ps1
param(
    [string]$ServerIP = "3.88.123.45",  # Cambiar por tu IP de AWS
    [string]$KeyPath = "C:\Users\PCX\.ssh\tu-key.pem",
    [string]$ServerUser = "ec2-user",
    [string]$ServerPath = "/var/www/erpapi"
)

$ErrorActionPreference = "Stop"

Write-Host "?? Iniciando publicación y despliegue..." -ForegroundColor Green

# 1. Limpiar
if (Test-Path "./publish") {
    Write-Host "?? Limpiando carpeta anterior..." -ForegroundColor Yellow
    Remove-Item -Path "./publish" -Recurse -Force
}

# 2. Compilar tests (opcional)
Write-Host "?? Compilando tests..." -ForegroundColor Cyan
dotnet build --configuration Release

# 3. Publicar
Write-Host "?? Publicando aplicación..." -ForegroundColor Cyan
dotnet publish Web.Api/Web.Api.csproj `
    -c Release `
    -o ./publish `
    --runtime linux-x64 `
    --self-contained false `
    /p:PublishReadyToRun=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error en la publicación" -ForegroundColor Red
    exit 1
}

Write-Host "? Publicación completada" -ForegroundColor Green

# 4. Crear archivo ZIP para transferencia
Write-Host "?? Comprimiendo archivos..." -ForegroundColor Cyan
Compress-Archive -Path "./publish/*" -DestinationPath "./publish.zip" -Force

$zipSize = (Get-Item ./publish.zip).Length / 1MB
Write-Host "?? Tamaño del ZIP: $([math]::Round($zipSize, 2)) MB" -ForegroundColor Yellow

# 5. Subir a AWS
Write-Host "?? Subiendo a AWS EC2..." -ForegroundColor Cyan
Write-Host "Servidor: $ServerIP" -ForegroundColor Gray
Write-Host "Usuario: $ServerUser" -ForegroundColor Gray

# Usar SCP para transferir
scp -i $KeyPath ./publish.zip "${ServerUser}@${ServerIP}:/tmp/erpapi.zip"

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al subir archivos" -ForegroundColor Red
    exit 1
}

Write-Host "? Archivos subidos" -ForegroundColor Green

# 6. Desplegar en servidor
Write-Host "?? Desplegando en servidor..." -ForegroundColor Cyan

$deployScript = @"
#!/bin/bash
set -e

echo '?? Deteniendo servicio...'
sudo systemctl stop erpapi

echo '?? Limpiando carpeta anterior...'
sudo rm -rf $ServerPath/*

echo '?? Descomprimiendo archivos...'
sudo unzip -o /tmp/erpapi.zip -d $ServerPath

echo '?? Ajustando permisos...'
sudo chown -R $ServerUser:$ServerUser $ServerPath
sudo chmod -R 755 $ServerPath

echo '?? Reiniciando servicio...'
sudo systemctl start erpapi

echo '? Despliegue completado'
sudo systemctl status erpapi --no-pager

echo '?? Últimos logs:'
sudo journalctl -u erpapi -n 20 --no-pager
"@

# Ejecutar script de despliegue en servidor
ssh -i $KeyPath "${ServerUser}@${ServerIP}" $deployScript

Write-Host "? Despliegue completado exitosamente!" -ForegroundColor Green
Write-Host "?? API disponible en: http://$ServerIP:7254" -ForegroundColor Cyan
Write-Host "?? Swagger: http://$ServerIP:7254/swagger" -ForegroundColor Cyan

# Limpiar archivo ZIP local
Remove-Item ./publish.zip -Force
```

**Uso:**
```powershell
.\Publish-And-Deploy.ps1 -ServerIP "3.88.123.45" -KeyPath "C:\Users\PCX\.ssh\mi-key.pem"
```

---

### **Script 3: Publicación con Backup Automático**

```powershell
# Publish-With-Backup.ps1
param(
    [string]$ServerIP = "3.88.123.45",
    [string]$KeyPath = "C:\Users\PCX\.ssh\tu-key.pem"
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFolder = "./backups/backup_$timestamp"

Write-Host "?? Publicación con backup automático..." -ForegroundColor Green

# 1. Crear backup del servidor actual
Write-Host "?? Creando backup del servidor..." -ForegroundColor Cyan
ssh -i $KeyPath "ec2-user@$ServerIP" "sudo tar -czf /tmp/erpapi_backup_$timestamp.tar.gz -C /var/www erpapi"

# Descargar backup
New-Item -ItemType Directory -Force -Path "./backups" | Out-Null
scp -i $KeyPath "ec2-user@$ServerIP:/tmp/erpapi_backup_$timestamp.tar.gz" "./backups/"

Write-Host "? Backup creado: ./backups/erpapi_backup_$timestamp.tar.gz" -ForegroundColor Green

# 2. Publicar nueva versión
Write-Host "?? Publicando nueva versión..." -ForegroundColor Cyan

if (Test-Path "./publish") {
    Remove-Item -Path "./publish" -Recurse -Force
}

dotnet publish Web.Api/Web.Api.csproj `
    -c Release `
    -o ./publish `
    --runtime linux-x64 `
    --self-contained false

# ... resto del script de despliegue
```

---

## ?? **Comandos Útiles**

### **Compilar sin publicar:**
```bash
dotnet build Web.Api/Web.Api.csproj -c Release
```

### **Limpiar compilaciones anteriores:**
```bash
dotnet clean Web.Api/Web.Api.csproj
```

### **Restaurar paquetes NuGet:**
```bash
dotnet restore Web.Api/Web.Api.csproj
```

### **Ver tamaño de publicación:**
```bash
# Windows PowerShell
(Get-ChildItem ./publish -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

# Linux/Mac
du -sh ./publish
```

---

## ?? **Comparación de Métodos**

| Método | Tamaño | Velocidad | Requiere .NET | Recomendado Para |
|--------|--------|-----------|---------------|------------------|
| **Framework-Dependent** | ~10 MB | Rápido | ? Sí | AWS con .NET instalado ? |
| **Self-Contained** | ~60 MB | Medio | ? No | Servidores sin .NET |
| **Single-File** | ~60 MB | Lento inicio | ? No | Distribución simple |
| **Trimmed** | ~30 MB | Medio | ? No | Optimización extrema |

---

## ?? **Configuración Avanzada**

### **Archivo `Web.Api.csproj` optimizado:**

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  
  <!-- Optimizaciones de publicación -->
  <PublishReadyToRun>true</PublishReadyToRun>
  <PublishTrimmed>false</PublishTrimmed>
  <TieredCompilation>true</TieredCompilation>
  <TieredCompilationQuickJit>true</TieredCompilationQuickJit>
  
  <!-- Información de versión -->
  <Version>1.0.0</Version>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
</PropertyGroup>
```

---

## ?? **Publicación desde Linux/Mac**

```bash
#!/bin/bash
# publish.sh

# Limpiar
rm -rf ./publish

# Publicar
dotnet publish Web.Api/Web.Api.csproj \
  -c Release \
  -o ./publish \
  --runtime linux-x64 \
  --self-contained false \
  /p:PublishReadyToRun=true

# Comprimir
tar -czf publish.tar.gz -C ./publish .

echo "? Publicación completada"
echo "?? Archivo: publish.tar.gz"
ls -lh publish.tar.gz
```

---

## ?? **Checklist Pre-Publicación**

- [ ] Código compilado sin errores: `dotnet build`
- [ ] Tests pasando (si existen): `dotnet test`
- [ ] Configuración de producción lista: `appsettings.json`
- [ ] Variables de entorno configuradas en servidor
- [ ] Base de datos migrada: `dotnet ef database update`
- [ ] Permisos de archivos verificados
- [ ] Servicio systemd configurado (Linux)
- [ ] Firewall/Security Group con puerto 7254 abierto
- [ ] Backup de versión anterior creado

---

## ?? **Solución de Problemas**

### **Error: "No se puede encontrar el proyecto"**
```bash
# Verificar ruta
ls Web.Api/Web.Api.csproj

# Si no existe, ajustar ruta
dotnet publish ./Web.Api/Web.Api.csproj -c Release -o ./publish
```

### **Error: "Runtime no soportado"**
```bash
# Ver runtimes disponibles
dotnet --list-runtimes

# Publicar sin runtime específico
dotnet publish -c Release -o ./publish
```

### **Error: "Falta SDK de .NET"**
```bash
# Verificar versión
dotnet --version

# Instalar .NET 8 SDK si falta
https://dotnet.microsoft.com/download/dotnet/8.0
```

---

## ?? **Recursos Adicionales**

- [Documentación oficial de `dotnet publish`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)
- [Guía de despliegue en Linux](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx)
- [Optimización de publicación](https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run)

---

**?? GUÍA COMPLETA DE PUBLICACIÓN** ?

**Fecha:** 2026-03-11  
**Proyecto:** ERP POS API  
**Target:** .NET 8.0 (Web.Api), .NET 7.0 (otros proyectos)  
**Estado:** ? **LISTO PARA USAR**
