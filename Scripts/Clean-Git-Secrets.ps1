# ============================================
# SCRIPT PARA LIMPIAR CREDENCIALES DE GIT
# ============================================
# Este script elimina archivos sensibles del historial de Git

Write-Host "?? Limpiando archivos sensibles del repositorio Git..." -ForegroundColor Yellow

# Paso 1: Deshacer el commit que tiene las credenciales (sin perder cambios)
Write-Host "`n?? Paso 1: Deshaciendo commit con credenciales..." -ForegroundColor Cyan
git reset --soft HEAD~1

# Paso 2: Quitar archivos sensibles del staging
Write-Host "`n?? Paso 2: Removiendo archivos sensibles del staging..." -ForegroundColor Cyan
git restore --staged Web.Api/appsettings.json
git restore --staged Web.Api/appsettings.Development.json
git restore --staged Web.Api/appsettings.Production.json

# Paso 3: Agregar archivos de ejemplo y README
Write-Host "`n?? Paso 3: Agregando archivos de ejemplo..." -ForegroundColor Cyan
git add .gitignore
git add Web.Api/appsettings.Example.json
git add Web.Api/appsettings.Production.Example.json
git add Web.Api/CONFIGURATION_GUIDE.md

# Paso 4: Agregar todos los demás archivos (excepto los sensibles)
Write-Host "`n?? Paso 4: Agregando archivos del proyecto..." -ForegroundColor Cyan
git add .

# Paso 5: Crear nuevo commit SIN credenciales
Write-Host "`n?? Paso 5: Creando commit limpio..." -ForegroundColor Cyan
git commit -m "feat: Sistema de empresas completo + Archivos de configuración seguros

- ? Módulo de gestión de empresas (CQRS completo)
- ? Actualizado .gitignore para proteger credenciales
- ? Creados archivos appsettings.Example.json sin credenciales
- ? Agregado CONFIGURATION_GUIDE.md con instrucciones

SEGURIDAD:
- Removidas credenciales AWS del repositorio
- Configuración movida a archivos .Example
- Variables sensibles protegidas
"

Write-Host "`n? Commit limpio creado exitosamente!" -ForegroundColor Green
Write-Host "`n?? Siguiente paso: Hacer push al repositorio" -ForegroundColor Yellow
Write-Host "   git push origin master" -ForegroundColor White

Write-Host "`n??  IMPORTANTE: Tus archivos locales appsettings.json NO se borraron," -ForegroundColor Yellow
Write-Host "   solo se removieron del control de versiones." -ForegroundColor Yellow
