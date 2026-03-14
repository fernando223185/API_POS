# ================================================================
# Script PowerShell para aplicar migración de Sales
# ================================================================

Write-Host "?? Aplicando migración: CompanyId y BranchId a Sales" -ForegroundColor Cyan
Write-Host ""

# Leer appsettings para obtener connection string
$appsettingsPath = "Web.Api/appsettings.Development.json"

if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    
    Write-Host "? Connection string encontrado" -ForegroundColor Green
    Write-Host "?? $connectionString" -ForegroundColor Gray
    Write-Host ""
    
    # Extraer componentes del connection string
    if ($connectionString -match "Server=([^;]+)") {
        $server = $Matches[1]
    }
    if ($connectionString -match "Database=([^;]+)") {
        $database = $Matches[1]
    }
    
    Write-Host "?? Servidor: $server" -ForegroundColor Yellow
    Write-Host "?? Base de datos: $database" -ForegroundColor Yellow
    Write-Host ""
    
    # Ejecutar script SQL
    $scriptPath = "Infrastructure/Scripts/AddCompanyAndBranchToSales.sql"
    
    if (Test-Path $scriptPath) {
        Write-Host "?? Ejecutando script SQL..." -ForegroundColor Cyan
        
        try {
            # Usar SqlCmd con Trusted Connection
            $result = & sqlcmd -S $server -d $database -E -i $scriptPath 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host ""
                Write-Host "? ¡Script ejecutado exitosamente!" -ForegroundColor Green
                Write-Host ""
                Write-Host "?? Resultado:" -ForegroundColor Cyan
                $result | ForEach-Object { Write-Host $_ }
            } else {
                Write-Host ""
                Write-Host "? Error al ejecutar script" -ForegroundColor Red
                Write-Host $result
            }
        }
        catch {
            Write-Host ""
            Write-Host "? Excepción al ejecutar script: $_" -ForegroundColor Red
            Write-Host ""
            Write-Host "?? Alternativa: Ejecuta manualmente el script en SQL Server Management Studio" -ForegroundColor Yellow
            Write-Host "   Ruta: $scriptPath" -ForegroundColor Gray
        }
    } else {
        Write-Host "? No se encontró el script SQL en: $scriptPath" -ForegroundColor Red
    }
} else {
    Write-Host "? No se encontró appsettings.Development.json" -ForegroundColor Red
}

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
