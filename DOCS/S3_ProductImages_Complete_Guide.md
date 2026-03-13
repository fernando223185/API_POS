# ?? **SISTEMA COMPLETO DE SUBIDA DE IMÁGENES A AWS S3**

## ? **IMPLEMENTACIÓN COMPLETADA**

Se ha implementado un sistema completo para subir imágenes de productos a AWS S3 con las siguientes características:

---

## ?? **CARACTERÍSTICAS IMPLEMENTADAS**

### **1. Upload de Imágenes a S3**
- ? Subida directa a AWS S3
- ? Organización por carpetas (`products/{productId}/`)
- ? Nombres únicos con timestamp
- ? Validación de formatos (JPG, JPEG, PNG, GIF, WebP)
- ? Validación de tamaño (máximo 5MB)
- ? **Retorna el S3 Key** para referencia futura

### **2. Gestión de Base de Datos**
- ? Registro en tabla `ProductImages`
- ? Relación con producto y usuario
- ? Soporte para múltiples imágenes por producto
- ? Imagen principal (primary)
- ? Orden de visualización (displayOrder)
- ? Soft delete

### **3. Seguridad**
- ? Autenticación JWT requerida
- ? Permisos específicos (`Product.Update`)
- ? Validación de producto existente
- ? Auditoría de usuario (quién subió cada imagen)

---

## ?? **ENDPOINTS DISPONIBLES**

### **1. ?? Subir Imagen de Producto**

```http
POST /api/Products/{productId}/upload-image
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Parámetros:**
- `productId` (path): ID del producto
- `file` (form-data): Archivo de imagen
- `isPrimary` (form-data, opcional): `true/false`
- `altText` (form-data, opcional): Texto alternativo para SEO

**Respuesta exitosa:**
```json
{
  "message": "Imagen subida exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "productId": 123,
    "s3Key": "products/123/producto_20260307135030.jpg",  // ? S3 KEY
    "publicUrl": "https://mi-bucket.s3.us-east-1.amazonaws.com/products/123/producto_20260307135030.jpg",
    "imageName": "producto.jpg",
    "altText": "Producto ejemplo",
    "isPrimary": true,
    "displayOrder": 1,
    "uploadedAt": "2026-03-07T13:50:30Z",
    "uploadedBy": "admin"
  }
}
```

---

### **2. ??? Obtener Imágenes de un Producto**

```http
GET /api/Products/{productId}/images
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Imágenes obtenidas exitosamente",
  "error": 0,
  "data": [
    {
      "id": 1,
      "productId": 123,
      "s3Key": "products/123/producto_20260307135030.jpg",
      "publicUrl": "https://mi-bucket.s3.us-east-1.amazonaws.com/products/123/producto_20260307135030.jpg",
      "imageName": "producto.jpg",
      "altText": "Producto ejemplo",
      "isPrimary": true,
      "displayOrder": 1,
      "uploadedAt": "2026-03-07T13:50:30Z",
      "uploadedBy": "admin"
    }
  ],
  "totalImages": 1
}
```

---

### **3. ??? Eliminar Imagen**

```http
DELETE /api/Products/images/{imageId}
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Imagen eliminada exitosamente",
  "error": 0,
  "imageId": 1,
  "deletedFromS3": true,
  "deletedFromDatabase": true
}
```

---

### **4. ? Establecer Imagen como Principal**

```http
PUT /api/Products/images/{imageId}/set-primary
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Imagen establecida como principal",
  "error": 0,
  "imageId": 1,
  "productId": 123
}
```

---

## ?? **CONFIGURACIÓN REQUERIDA**

### **1. Configurar AWS en `appsettings.json`**

```json
{
  "AWS": {
    "AccessKey": "TU_ACCESS_KEY_AQUI",
    "SecretKey": "TU_SECRET_KEY_AQUI",
    "Region": "us-east-1",
    "S3": {
      "BucketName": "tu-bucket-name"
    }
  }
}
```

**Opción alternativa (IAM Role en EC2/ECS):**

Si estás en AWS EC2/ECS con IAM Role, puedes dejar vacías las credenciales:

```json
{
  "AWS": {
    "AccessKey": "",
    "SecretKey": "",
    "Region": "us-east-1",
    "S3": {
      "BucketName": "tu-bucket-name"
    }
  }
}
```

---

### **2. Configurar Bucket S3**

#### **Opción A: Bucket Público (Recomendado para imágenes de productos)**

1. **Crear bucket en AWS S3**
2. **Deshabilitar "Block all public access"**
3. **Agregar Bucket Policy:**

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::tu-bucket-name/*"
    }
  ]
}
```

4. **Habilitar CORS:**

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

#### **Opción B: Bucket Privado (con URLs firmadas)**

El servicio soporta URLs firmadas temporales con el método `GetPresignedUrlAsync()`.

---

### **3. Crear IAM User con Permisos S3**

**Policy JSON:**
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::tu-bucket-name",
        "arn:aws:s3:::tu-bucket-name/*"
      ]
    }
  ]
}
```

---

## ?? **PRUEBAS CON POSTMAN**

### **Test 1: Subir Imagen**

1. **URL:** `POST http://localhost:7254/api/Products/1/upload-image`
2. **Headers:**
   ```
   Authorization: Bearer {tu-token}
   ```
3. **Body (form-data):**
   ```
   file: [Seleccionar archivo]
   isPrimary: true
   altText: Mi producto
   ```
4. **Send**

**? Resultado esperado:**
- Status: 200 OK
- JSON con `s3Key` retornado
- Imagen visible en S3

---

### **Test 2: Ver Imágenes del Producto**

1. **URL:** `GET http://localhost:7254/api/Products/1/images`
2. **Headers:**
   ```
   Authorization: Bearer {tu-token}
   ```
3. **Send**

**? Resultado esperado:**
- Lista de imágenes con S3 Keys
- URLs públicas funcionando

---

## ?? **ESTRUCTURA DE ARCHIVOS CREADOS**

```
Application/
??? Abstractions/
?   ??? Storage/
?   ?   ??? IS3StorageService.cs                    ? NUEVO
?   ??? Catalogue/
?       ??? IProductImageRepository.cs               ? NUEVO
??? DTOs/
?   ??? Product/
?       ??? ProductImageDtos.cs                      ? NUEVO
?
Infrastructure/
??? Services/
?   ??? S3StorageService.cs                          ? NUEVO
??? Repositories/
    ??? ProductImageRepository.cs                    ? NUEVO

Web.Api/
??? Controllers/
?   ??? Products/
?       ??? ProductsController.cs                    ? ACTUALIZADO
?           - UploadProductImage()
?           - GetProductImages()
?           - DeleteProductImage()
?           - SetPrimaryImage()
??? Program.cs                                       ? ACTUALIZADO
??? appsettings.json                                 ? ACTUALIZADO

Domain/
??? Entities/
    ??? ProductImage.cs                              ? YA EXISTÍA
```

---

## ?? **DIAGRAMA DE FLUJO**

```
???????????????????????????????????????????????????????????
?  Cliente (Postman/Frontend)                             ?
???????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????
?  ProductsController.UploadProductImage()                ?
?  - Valida producto existe                               ?
?  - Valida formato y tamaño                              ?
?  - Obtiene usuario del token                            ?
???????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????
?  S3StorageService.UploadImageAsync()                    ?
?  - Genera nombre único con timestamp                    ?
?  - Construye key: products/{productId}/file.jpg         ?
?  - Sube a S3                                            ?
?  - Retorna S3 Key                                       ?
???????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????
?  AWS S3                                                 ?
?  - Almacena imagen                                      ?
?  - Genera URL pública (si es público)                   ?
???????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????
?  ProductImageRepository.CreateAsync()                   ?
?  - Guarda registro en BD                                ?
?  - Asocia con producto y usuario                        ?
?  - Establece displayOrder                               ?
???????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????
?  Response al Cliente                                    ?
?  - S3 Key                                               ?
?  - URL pública                                          ?
?  - Metadata                                             ?
???????????????????????????????????????????????????????????
```

---

## ?? **SEGURIDAD**

### **Validaciones Implementadas:**

1. ? **Autenticación JWT:** Requiere token válido
2. ? **Autorización:** Permiso `Product.Update` requerido
3. ? **Validación de Producto:** Verifica que el producto existe
4. ? **Validación de Archivo:**
   - Formatos permitidos: JPG, JPEG, PNG, GIF, WebP
   - Tamaño máximo: 5MB
5. ? **Auditoría:** Registra quién subió cada imagen
6. ? **Soft Delete:** No elimina físicamente de BD

---

## ?? **EJEMPLO COMPLETO EN C# (Cliente)**

```csharp
using System.Net.Http;
using System.Net.Http.Headers;

public class ProductImageClient
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public async Task<string> UploadProductImageAsync(int productId, string filePath)
    {
        using var form = new MultipartFormDataContent();
        
        // Agregar archivo
        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "file", Path.GetFileName(filePath));
        
        // Agregar parámetros opcionales
        form.Add(new StringContent("true"), "isPrimary");
        form.Add(new StringContent("Mi producto"), "altText");
        
        // Request
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/Products/{productId}/upload-image");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        request.Content = form;
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        // Parsear JSON y extraer s3Key
        return jsonResponse;
    }
}
```

---

## ?? **USO DESDE EL FRONTEND**

### **JavaScript/TypeScript:**

```javascript
async function uploadProductImage(productId, file, isPrimary = false) {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('isPrimary', isPrimary);
  formData.append('altText', file.name);
  
  const response = await fetch(`http://localhost:7254/api/Products/${productId}/upload-image`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  
  const result = await response.json();
  
  if (result.error === 0) {
    console.log('? Imagen subida:', result.data.s3Key);
    console.log('?? URL pública:', result.data.publicUrl);
    return result.data;
  } else {
    console.error('? Error:', result.message);
  }
}

// Uso
const fileInput = document.getElementById('fileInput');
fileInput.addEventListener('change', async (e) => {
  const file = e.target.files[0];
  const result = await uploadProductImage(123, file, true);
});
```

---

## ?? **CONSIDERACIONES**

### **Costos AWS S3:**
- Almacenamiento: ~$0.023 por GB/mes
- Transferencia OUT: ~$0.09 por GB (primeros 10TB)
- PUT/POST: ~$0.005 por 1,000 requests

### **Optimizaciones Sugeridas:**
1. ? Redimensionar imágenes antes de subir (reducir tamaño)
2. ? Usar CDN (CloudFront) para mejorar velocidad
3. ? Implementar caché de URLs
4. ? Comprimir imágenes con bibliotecas como ImageSharp

---

## ?? **PRÓXIMOS PASOS OPCIONALES**

### **1. Redimensionamiento Automático**
Agregar `ImageSharp` para redimensionar imágenes antes de subir a S3.

### **2. CDN con CloudFront**
Configurar CloudFront delante del bucket S3 para mejor rendimiento.

### **3. Watermark**
Agregar marca de agua a las imágenes automáticamente.

### **4. Thumbnails**
Generar miniaturas automáticamente para optimizar carga.

---

## ? **RESUMEN**

### **? Implementado:**
- ?? Upload a S3 con validaciones
- ??? Gestión en base de datos
- ?? Seguridad y permisos
- ??? CRUD completo de imágenes
- ?? **Retorno del S3 Key**

### **?? Endpoints:**
- `POST /api/Products/{id}/upload-image` - Subir
- `GET /api/Products/{id}/images` - Listar
- `DELETE /api/Products/images/{id}` - Eliminar
- `PUT /api/Products/images/{id}/set-primary` - Establecer principal

### **?? Configuración:**
- AWS SDK instalado ?
- Servicio S3 registrado ?
- Repository configurado ?
- DTOs creados ?

---

**?? SISTEMA DE IMÁGENES COMPLETAMENTE FUNCIONAL** - Listo para producción con AWS S3
