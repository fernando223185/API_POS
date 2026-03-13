# ?? **Configuración de Archivos Sensibles**

## ?? **IMPORTANTE: Archivos con Credenciales**

Los archivos `appsettings.json`, `appsettings.Development.json` y `appsettings.Production.json` **NO están en Git** porque contienen información sensible (credenciales de AWS, contraseñas de base de datos, claves JWT, etc.).

---

## ??? **Cómo Configurar tu Entorno Local**

### **Paso 1: Copiar archivos de ejemplo**

```bash
# En la carpeta Web.Api/
cp appsettings.Example.json appsettings.json
cp appsettings.Example.json appsettings.Development.json
cp appsettings.Production.Example.json appsettings.Production.json
```

### **Paso 2: Configurar credenciales reales**

Edita los archivos copiados y reemplaza:

#### **`appsettings.json` (desarrollo local):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ERP;User Id=sa;Password=TuPasswordReal;..."
  },
  "Jwt": {
    "Key": "TuClaveSecretaMuyLargaYSegura123456789",
    "Issuer": "TuAplicacion",
    "Audience": "TuAplicacion"
  },
  "AWS": {
    "AccessKey": "AKIAXXXXXXXXXXXXXXXX",
    "SecretKey": "tuSecretKeyRealDeAWS",
    "Region": "us-east-1",
    "S3": {
      "BucketName": "tu-bucket-real"
    }
  }
}
```

#### **`appsettings.Production.json` (AWS/Producción):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tu-rds-endpoint.rds.amazonaws.com;Database=ERP;User Id=admin;Password=TuPasswordRDS;..."
  }
}
```

---

## ?? **Qué NO hacer**

? **NUNCA** hagas commit de archivos con credenciales reales:
```bash
git add Web.Api/appsettings.json      # ? NO
git add Web.Api/appsettings.*.json    # ? NO
```

? **SÍ** puedes hacer commit de archivos de ejemplo:
```bash
git add Web.Api/appsettings.Example.json           # ? SÍ
git add Web.Api/appsettings.Production.Example.json # ? SÍ
```

---

## ?? **Archivos Protegidos por `.gitignore`**

Estos archivos **NUNCA** se subirán a Git:

- `**/appsettings.json`
- `**/appsettings.*.json` (excepto `.Example.json`)
- `*.pfx`, `*.p12`, `*.key`, `*.pem` (certificados)
- `*.bak` (backups de BD)
- `**/publish/` (archivos publicados)

---

## ?? **Para Nuevos Desarrolladores**

1. Clona el repositorio:
```bash
git clone https://github.com/fernando223185/API_POS.git
cd API_POS
```

2. Copia los archivos de ejemplo:
```bash
cd Web.Api
cp appsettings.Example.json appsettings.json
cp appsettings.Example.json appsettings.Development.json
```

3. Pide las credenciales reales al administrador del proyecto

4. Edita `appsettings.json` y `appsettings.Development.json` con las credenciales reales

5. **NO** hagas commit de estos archivos

---

## ?? **Variables de Entorno (Alternativa Recomendada)**

En lugar de usar `appsettings.json` en producción, puedes usar variables de entorno:

```bash
# En AWS/Linux:
export ConnectionStrings__DefaultConnection="Server=..."
export Jwt__Key="TuClaveSecreta..."
export AWS__AccessKey="AKIAXXXXXXXX"
export AWS__SecretKey="tuSecretKey..."

# Ejecutar la API:
dotnet Web.Api.dll
```

O configurar en `systemd` service file:

```ini
[Service]
Environment="ConnectionStrings__DefaultConnection=Server=..."
Environment="Jwt__Key=TuClaveSecreta..."
Environment="AWS__AccessKey=AKIAXXXXXXXX"
Environment="AWS__SecretKey=tuSecretKey..."
```

---

## ? **Resumen**

| Archivo | En Git | Descripción |
|---------|--------|-------------|
| `appsettings.Example.json` | ? SÍ | Plantilla sin credenciales |
| `appsettings.json` | ? NO | Configuración real (local) |
| `appsettings.Development.json` | ? NO | Configuración real (desarrollo) |
| `appsettings.Production.json` | ? NO | Configuración real (producción) |

---

**?? Mantén tus credenciales seguras** ??
