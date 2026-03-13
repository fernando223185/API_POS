# ============================================
# SCRIPT DE PUBLICACIÓN - API ERP POS
# Compila y publica la API para producción
# ============================================

param(
    [string]$Configuration = "Release",
    [string]$Framework = "net8.0",
    [string]$Runtime = "",  # Dejar vacío para framework-dependent, o "win-x64", "linux-x64"
    [switch]$SelfContained = $false
)

Write-Host "?? Iniciando publicación de API ERP POS" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# ============================================
# CONFIGURACIÓN
# ============================================

$projectPath = Join-Path $PSScriptRoot ".." "Web.Api"
$projectFile = Join-Path $projectPath "Web.Api.csproj"
$publishPath = Join-Path $projectPath "publish"

Write-Host "?? Configuración:" -ForegroundColor Cyan
Write-Host "   Proyecto: Web.Api" -ForegroundColor White
Write-Host "   Configuración: $Configuration" -ForegroundColor White
Write-Host "   Framework: $Framework" -ForegroundColor White
Write-Host "   Runtime: $(if($Runtime) {$Runtime} else {'Framework-Dependent'})" -ForegroundColor White
Write-Host "   Self-Contained: $SelfContained" -ForegroundColor White
Write-Host "   Carpeta destino: $publishPath" -ForegroundColor White
Write-Host ""

# ============================================
# VALIDACIONES
# ============================================

Write-Host "?? Verificando requisitos..." -ForegroundColor Yellow

# Verificar .NET SDK
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "? .NET SDK no está instalado" -ForegroundColor Red
    Write-Host "   Descarga desde: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

$dotnetVersion = dotnet --version
Write-Host "? .NET SDK instalado: $dotnetVersion" -ForegroundColor Green

# Verificar que el proyecto existe
if (!(Test-Path $projectFile)) {
    Write-Host "? No se encontró el archivo del proyecto: $projectFile" -ForegroundColor Red
    exit 1
}

Write-Host "? Proyecto encontrado: $projectFile" -ForegroundColor Green
Write-Host ""

# ============================================
# LIMPIAR PUBLICACIÓN ANTERIOR
# ============================================

if (Test-Path $publishPath) {
    Write-Host "?? Limpiando publicación anterior..." -ForegroundColor Yellow
    Remove-Item -Path $publishPath -Recurse -Force
    Write-Host "? Carpeta publish eliminada" -ForegroundColor Green
} else {
    Write-Host "?? Creando carpeta publish..." -ForegroundColor Yellow
}

Write-Host ""

# ============================================
# RESTAURAR DEPENDENCIAS
# ============================================

Write-Host "?? Restaurando dependencias..." -ForegroundColor Cyan
Set-Location $projectPath
dotnet restore --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al restaurar dependencias" -ForegroundColor Red
    exit 1
}

Write-Host "? Dependencias restauradas" -ForegroundColor Green
Write-Host ""

# ============================================
# COMPILAR PROYECTO
# ============================================

Write-Host "?? Compilando proyecto..." -ForegroundColor Cyan

$buildArgs = @(
    "build",
    "--configuration", $Configuration,
    "--nologo",
    "--no-restore"
)

dotnet @buildArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al compilar el proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "? Proyecto compilado exitosamente" -ForegroundColor Green
Write-Host ""

# ============================================
# PUBLICAR PROYECTO
# ============================================

Write-Host "?? Publicando proyecto..." -ForegroundColor Cyan

$publishArgs = @(
    "publish",
    "--configuration", $Configuration,
    "--output", $publishPath,
    "--nologo",
    "--no-restore",
    "--no-build"
)

# Agregar framework si se especificó
if ($Framework) {
    $publishArgs += "--framework"
    $publishArgs += $Framework
}

# Agregar runtime si se especificó
if ($Runtime) {
    $publishArgs += "--runtime"
    $publishArgs += $Runtime
}

# Agregar self-contained si se especificó
if ($SelfContained) {
    $publishArgs += "--self-contained"
    $publishArgs += "true"
} else {
    $publishArgs += "--self-contained"
    $publishArgs += "false"
}

dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al publicar el proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "? Proyecto publicado exitosamente" -ForegroundColor Green
Write-Host ""

# ============================================
# CREAR ZIP PARA DESPLIEGUE
# ============================================

Write-Host "?? Creando archivo ZIP para despliegue..." -ForegroundColor Cyan

$zipFileName = "API_ERP_POS_$Configuration_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"
$zipPath = Join-Path $projectPath $zipFileName

if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}

Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -Force

$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host "? ZIP creado: $zipFileName ($zipSize MB)" -ForegroundColor Green
Write-Host ""

# ============================================
# COPIAR ARCHIVOS DE CONFIGURACIÓN
# ============================================

Write-Host "??  Copiando archivos de configuración..." -ForegroundColor Cyan

# Verificar que appsettings.Production.json existe
$productionSettings = Join-Path $projectPath "appsettings.Production.json"
if (Test-Path $productionSettings) {
    Copy-Item -Path $productionSettings -Destination $publishPath -Force
    Write-Host "? appsettings.Production.json copiado" -ForegroundColor Green
} else {
    Write-Host "??  appsettings.Production.json no encontrado" -ForegroundColor Yellow
}

Write-Host ""

# ============================================
# VERIFICAR ARCHIVOS PUBLICADOS
# ============================================

Write-Host "?? Verificando archivos publicados..." -ForegroundColor Cyan

$publishedFiles = Get-ChildItem -Path $publishPath -Recurse | Measure-Object
$publishSize = [math]::Round((Get-ChildItem -Path $publishPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB, 2)

Write-Host "   Total de archivos: $($publishedFiles.Count)" -ForegroundColor White
Write-Host "   Tamaño total: $publishSize MB" -ForegroundColor White
Write-Host ""

# Listar archivos principales
Write-Host "?? Archivos principales:" -ForegroundColor Cyan
$mainFiles = @(
    "Web.Api.dll",
    "Web.Api.exe",
    "appsettings.json",
    "appsettings.Production.json"
)

foreach ($file in $mainFiles) {
    $filePath = Join-Path $publishPath $file
    if (Test-Path $filePath) {
        $fileSize = [math]::Round((Get-Item $filePath).Length / 1KB, 2)
        Write-Host "   ? $file ($fileSize KB)" -ForegroundColor Green
    } else {
        if ($file -ne "Web.Api.exe" -and $file -ne "appsettings.Production.json") {
            Write-Host "   ? $file (no encontrado)" -ForegroundColor Red
        }
    }
}

Write-Host ""

# ============================================
# INSTRUCCIONES DE DESPLIEGUE
# ============================================

Write-Host "========================================" -ForegroundColor Green
Write-Host "? PUBLICACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "?? Carpeta de publicación:" -ForegroundColor Cyan
Write-Host "   $publishPath" -ForegroundColor White
Write-Host ""

Write-Host "?? Archivo ZIP para despliegue:" -ForegroundColor Cyan
Write-Host "   $zipPath" -ForegroundColor White
Write-Host ""

Write-Host "?? PRÓXIMOS PASOS:" -ForegroundColor Yellow
Write-Host ""

Write-Host "1??  Para ejecutar localmente:" -ForegroundColor Cyan
Write-Host "   cd `"$publishPath`"" -ForegroundColor White
Write-Host "   dotnet Web.Api.dll" -ForegroundColor White
Write-Host ""

Write-Host "2??  Para desplegar en IIS:" -ForegroundColor Cyan
Write-Host "   - Copiar contenido de la carpeta publish a IIS" -ForegroundColor White
Write-Host "   - Crear Application Pool (.NET CLR Version: No Managed Code)" -ForegroundColor White
Write-Host "   - Asignar permisos a IIS_IUSRS" -ForegroundColor White
Write-Host ""

Write-Host "3??  Para desplegar en Azure:" -ForegroundColor Cyan
Write-Host "   - Usar el archivo ZIP: $zipFileName" -ForegroundColor White
Write-Host "   - O ejecutar: .\Deploy-Azure.ps1" -ForegroundColor White
Write-Host ""

Write-Host "4??  Para desplegar en Linux:" -ForegroundColor Cyan
Write-Host "   - Copiar carpeta publish al servidor" -ForegroundColor White
Write-Host "   - Ejecutar: dotnet Web.Api.dll" -ForegroundColor White
Write-Host ""

Write-Host "??  IMPORTANTE:" -ForegroundColor Yellow
Write-Host "   - Actualizar ConnectionString en appsettings.Production.json" -ForegroundColor White
Write-Host "   - Actualizar claves JWT en appsettings.Production.json" -ForegroundColor White
Write-Host "   - Configurar variables de entorno en el servidor" -ForegroundColor White
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "?? Publicación lista para despliegue!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
