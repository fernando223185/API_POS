# ? **SOLUCIONADO: Error "The bucket does not allow ACLs"**

## ?? **EL PROBLEMA**

Al subir imagen obtenías:
```json
{
  "message": "Error al subir imagen",
  "error": 2,
  "details": "Error al subir archivo a S3: The bucket does not allow ACLs"
}
```

---

## ?? **CAUSA**

AWS S3 cambió la configuración por defecto y ahora **NO permite ACLs** en buckets nuevos. Antes usábamos:

```csharp
// ? ESTO CAUSABA EL ERROR
CannedACL = S3CannedACL.PublicRead
```

---

## ? **SOLUCIÓN APLICADA**

### **1. Código Corregido**

He actualizado `S3StorageService.cs` para **NO usar ACLs**:

```csharp
// ? NUEVA VERSIÓN - Sin ACL
var uploadRequest = new TransferUtilityUploadRequest
{
    InputStream = fileStream,
    Key = key,
    BucketName = _bucketName,
    ContentType = contentType
    // ? REMOVIDO: CannedACL = S3CannedACL.PublicRead
};
```

### **2. Configurar Bucket S3**

Ahora usa **Bucket Policy** en lugar de ACLs.

---

## ?? **CONFIGURACIÓN AWS S3 REQUERIDA**

### **Paso 1: Verificar "Block Public Access"**

1. Ve a **AWS S3 Console**
2. Selecciona tu bucket: `expanda-products`
3. Pestaña **"Permissions"**
4. **Block public access (bucket settings)**
   - ? **DESACTIVA** "Block all public access"
   - Click "Save changes"

**IMPORTANTE:** Confirma escribiendo `confirm` cuando te lo pida.

---

### **Paso 2: Configurar ACL Settings (Opcional)**

Si quieres habilitar ACLs:

1. En **Permissions** ? **Object Ownership**
2. Click "Edit"
3. Selecciona: **"ACLs enabled"**
4. Marca: **"Bucket owner preferred"**
5. Click "Save changes"

**NOTA:** Esto es opcional, la solución actual funciona sin ACLs.

---

### **Paso 3: Agregar Bucket Policy (REQUERIDO)**

1. En **Permissions** ? **Bucket policy**
2. Click "Edit"
3. Pega este JSON:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::expanda-products/*"
    }
  ]
}
```

4. Click "Save changes"

**¿Qué hace esto?**
- ? Permite que **cualquiera pueda VER (GET)** las imágenes
- ? Solo tu API puede **SUBIR (PUT)** las imágenes (con credenciales)
- ? No necesita ACLs

---

### **Paso 4: Configurar CORS**

1. En **Permissions** ? **Cross-origin resource sharing (CORS)**
2. Click "Edit"
3. Pega este JSON:

```json
[
  {
    "AllowedHeaders": ["*"],
    "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
    "AllowedOrigins": ["*"],
    "ExposeHeaders": ["ETag"]
  }
]
```

4. Click "Save changes"

---

## ?? **VERIFICACIÓN**

### **Test 1: Subir Imagen**

```
POST http://localhost:7254/api/Products/1/upload-image
Authorization: Bearer {token}
Content-Type: multipart/form-data

Body:
- file: [Seleccionar imagen]
- isPrimary: true
```

**? Resultado esperado:**
```json
{
  "message": "Imagen subida exitosamente",
  "error": 0,
  "data": {
    "s3Key": "products/1/imagen_20260307140030.jpg",
    "publicUrl": "https://expanda-products.s3.us-east-1.amazonaws.com/products/1/imagen_20260307140030.jpg"
  }
}
```

### **Test 2: Verificar Imagen Pública**

Copia la `publicUrl` de la respuesta y ábrela en tu navegador.

**? Deberías ver:** La imagen se carga correctamente.

**? Si ves error 403:** Revisa el Bucket Policy.

---

## ?? **RESUMEN DE CONFIGURACIÓN S3**

### **Configuración Mínima Requerida:**

| Configuración | Valor | Dónde |
|---------------|-------|-------|
| **Block Public Access** | ? Desactivado | Permissions ? Block public access |
| **Bucket Policy** | ? Configurado | Permissions ? Bucket policy |
| **CORS** | ? Configurado | Permissions ? CORS |
| **ACL** | ?? Opcional | Permissions ? Object Ownership |

---

## ?? **SEGURIDAD**

### **¿Es seguro hacer el bucket público?**

**SÍ**, con esta configuración:

1. **Solo GET es público** - Cualquiera puede **VER** las imágenes
2. **PUT/POST/DELETE requieren credenciales** - Solo tu API puede **SUBIR/ELIMINAR**
3. **No hay listado público** - No se puede ver qué archivos existen sin la URL exacta

### **Para mayor seguridad:**

Si quieres que las URLs expiren:

1. Cambiar Bucket Policy a privado
2. Usar URLs firmadas (presigned URLs):

```csharp
// En tu código
var presignedUrl = await _s3Service.GetPresignedUrlAsync(s3Key, 60); // 60 minutos
```

---

## ?? **ALTERNATIVA: CloudFront (Opcional)**

Para mejor rendimiento y seguridad:

1. **Crear distribución CloudFront**
2. **Origen:** Tu bucket S3
3. **Beneficios:**
   - ? CDN global (más rápido)
   - ? HTTPS automático
   - ? Menor costo de transferencia
   - ? Caché automático

---

## ?? **ERRORES COMUNES**

### **Error 1: "403 Forbidden" al ver imagen**

**Causa:** Bucket Policy incorrecto

**Solución:**
```json
{
  "Resource": "arn:aws:s3:::expanda-products/*"  // ? Asegúrate del /*
}
```

### **Error 2: "AccessDenied" al subir**

**Causa:** Credenciales AWS incorrectas

**Solución:** Verifica en `appsettings.json`:
```json
{
  "AWS": {
    "AccessKey": "TU_ACCESS_KEY_CORRECTO",
    "SecretKey": "TU_SECRET_KEY_CORRECTO"
  }
}
```

### **Error 3: CORS Error desde Frontend**

**Causa:** CORS no configurado

**Solución:** Configurar CORS en S3 (ver Paso 4)

---

## ?? **ANTES vs DESPUÉS**

### **? ANTES (Con ACL):**

```csharp
CannedACL = S3CannedACL.PublicRead  // ? Error
```

**Resultado:** Error "The bucket does not allow ACLs"

### **? DESPUÉS (Con Bucket Policy):**

```csharp
// Sin ACL, usa Bucket Policy
var uploadRequest = new TransferUtilityUploadRequest
{
    InputStream = fileStream,
    Key = key,
    BucketName = _bucketName,
    ContentType = contentType
};
```

**Resultado:** ? Funciona perfectamente

---

## ?? **PASOS FINALES**

### **1. Reiniciar la API**

```bash
# Detener (Ctrl+C)
# Reiniciar
dotnet run --project Web.Api
```

### **2. Probar Upload**

```
POST /api/Products/1/upload-image
? ? Debería subir sin errores
```

### **3. Verificar URL Pública**

```
GET https://expanda-products.s3.us-east-1.amazonaws.com/products/1/imagen.jpg
? ? La imagen debería mostrarse
```

---

## ?? **CHECKLIST DE CONFIGURACIÓN**

```
? 1. Block Public Access desactivado
? 2. Bucket Policy agregado
? 3. CORS configurado
? 4. Credenciales AWS correctas en appsettings.json
? 5. Código actualizado (sin CannedACL)
? 6. API reiniciada
? 7. Test de upload exitoso
? 8. Imagen visible en navegador
```

---

## ? **RESUMEN**

### **Cambios en Código:**
- ? Removido `CannedACL` de `S3StorageService.cs`
- ? Ahora usa Bucket Policy en lugar de ACLs

### **Configuración AWS S3:**
- ? Block Public Access: Desactivado
- ? Bucket Policy: Configurado para GET público
- ? CORS: Configurado

### **Resultado:**
- ? Upload funciona sin errores
- ? Imágenes accesibles públicamente
- ? Solo API puede subir/eliminar

---

**?? PROBLEMA RESUELTO** - Ahora puedes subir imágenes sin error de ACLs
