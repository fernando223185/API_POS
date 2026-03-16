# 🛠️ Configuración del Proyecto

## Configuración Local para Desarrollo

### 1️⃣ Primer paso después de clonar

Después de clonar el repositorio, necesitas crear tu archivo de configuración local:

```bash
# Copiar el archivo de ejemplo
cp Web.Api/appsettings.Example.json Web.Api/appsettings.json
```

### 2️⃣ Configurar tus credenciales locales

Edita `Web.Api/appsettings.json` y reemplaza los valores de ejemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ERP;User Id=sa;Password=TU_PASSWORD_AQUI;..."
  },
  "Jwt": {
    "Key": "TuClaveSecretaLocal123456789",
    "Issuer": "TuAplicacion",
    "Audience": "TuAplicacion"
  },
  "AWS": {
    "AccessKey": "TU_AWS_ACCESS_KEY",
    "SecretKey": "TU_AWS_SECRET_KEY",
    "Region": "us-east-1",
    "S3": {
      "BucketName": "tu-bucket"
    }
  }
}
```

### 3️⃣ Verificar SQL Server

Asegúrate de tener SQL Server corriendo (Docker o local):

```bash
# Verificar contenedor de SQL Server (si usas Docker)
docker ps | grep sql

# Si no está corriendo, iniciarlo
docker start sql
```

### 4️⃣ Aplicar migraciones

```bash
cd /ruta/al/proyecto
dotnet ef database update --project Infrastructure --startup-project Web.Api
```

### 5️⃣ Ejecutar la aplicación

```bash
# Opción 1: Desde el directorio Web.Api
cd Web.Api
dotnet run

# Opción 2: Desde el directorio raíz
dotnet run --project Web.Api
```

La API estará disponible en: http://localhost:7254

---

## 🔒 Seguridad

⚠️ **IMPORTANTE:**
- El archivo `appsettings.json` está en `.gitignore`
- **NUNCA** agregues credenciales reales a `appsettings.Example.json`
- Las credenciales de producción se manejan con GitHub Secrets en el CI/CD

---

## 📋 Archivo único de configuración

Este proyecto usa una estrategia simplificada:

| Archivo | Ubicación | Propósito |
|---------|-----------|-----------|
| `appsettings.Example.json` | ✅ En Git | Plantilla sin credenciales |
| `appsettings.json` | ❌ Ignorado | Configuración real (local o producción) |

**En desarrollo local:**
- Creas tu `appsettings.json` copiando el Example
- Lo editas con tus credenciales locales

**En producción (CI/CD):**
- GitHub Actions genera `appsettings.json` automáticamente
- Usa los secrets configurados en GitHub

---

## 🚀 Deploy

Para configurar el deployment, consulta [.github/SECRETS_TEMPLATE.md](.github/SECRETS_TEMPLATE.md)
