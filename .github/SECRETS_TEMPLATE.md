# 🔐 GitHub Secrets Configuration

Este archivo explica los secrets que necesitas configurar en GitHub Actions para el deployment.

## 📝 Cómo agregar secrets:

1. Ve a tu repositorio en GitHub
2. **Settings** → **Secrets and variables** → **Actions**
3. Click en **New repository secret**
4. Agrega cada uno de los siguientes secrets:

---

## 🚀 Secrets para Deployment a EC2

### `EC2_HOST`
**Descripción:** IP pública o DNS de tu servidor EC2  
**Ejemplo:** `54.123.45.67` o `ec2-54-123-45-67.compute-1.amazonaws.com`

### `EC2_USERNAME`
**Descripción:** Usuario SSH para conectarte a EC2  
**Ejemplos:**
- Ubuntu: `ubuntu`
- Amazon Linux: `ec2-user`
- Debian: `admin`

### `EC2_SSH_KEY`
**Descripción:** Contenido completo de tu clave privada SSH (.pem)  
**Formato:**
```
-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEA...
... (todo el contenido de tu archivo .pem)
...
-----END RSA PRIVATE KEY-----
```

---

## 🔧 Secrets para Configuración de la Aplicación

### `DB_CONNECTION_STRING`
**Descripción:** Cadena de conexión a tu base de datos SQL Server  
**Ejemplo:**
```
Server=tu-servidor.rds.amazonaws.com;Database=ERP;User Id=admin;Password=TuPasswordSeguro;Trusted_Connection=false;MultipleActiveResultSets=true;TrustServerCertificate=true;
```

### `JWT_SECRET_KEY`
**Descripción:** Clave secreta para firmar tokens JWT (mínimo 32 caracteres)  
**Ejemplo:** `TuClaveSecretaMuyLargaYSegura123456789012`

### `JWT_ISSUER`
**Descripción:** Emisor del token JWT  
**Ejemplo:** `API_POS_Production`

### `JWT_AUDIENCE`
**Descripción:** Audiencia del token JWT  
**Ejemplo:** `API_POS_Production`

### `AWS_ACCESS_KEY`
**Descripción:** AWS Access Key para servicios (S3, etc.)  
**Ejemplo:** `AKIAIOSFODNN7EXAMPLE`

### `AWS_SECRET_KEY`
**Descripción:** AWS Secret Key para servicios  
**Ejemplo:** `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY`

### `AWS_REGION`
**Descripción:** Región de AWS donde están tus recursos  
**Ejemplo:** `us-east-1`

### `AWS_S3_BUCKET`
**Descripción:** Nombre del bucket S3 para almacenar archivos  
**Ejemplo:** `mi-bucket-produccion`

---

## ✅ Verificación

Después de agregar todos los secrets, puedes verificar que estén configurados en:

**Settings** → **Secrets and variables** → **Actions** → **Repository secrets**

Deberías ver todos los secrets listados (GitHub no muestra los valores, solo los nombres).

---

## 🔒 Seguridad

⚠️ **IMPORTANTE:**
- Nunca compartas estos valores públicamente
- No los incluyas en commits
- Usa valores diferentes para desarrollo y producción
- Rota las credenciales periódicamente
- Los archivos `appsettings.Production.json` y `appsettings.json` con datos reales NO deben subirse a Git

---

## 📦 Archivos de configuración en Git

**Estructura simplificada:**
- Solo manejamos **UN archivo** `appsettings.json` que contiene toda la configuración
- El workflow de CI/CD genera automáticamente este archivo con los secrets de GitHub

✅ **SÍ incluir en Git:**
- `appsettings.Example.json` - Plantilla con valores de ejemplo (sin credenciales reales)

❌ **NO incluir en Git (ya está en .gitignore):**
- `appsettings.json` - Archivo con configuración real (se genera en CI/CD o localmente)

**Para desarrollo local:**
1. Copia `appsettings.Example.json` → `appsettings.json`
2. Reemplaza los valores de ejemplo con tus credenciales locales
3. El archivo `appsettings.json` está ignorado en Git, así que no se subirá accidentalmente
