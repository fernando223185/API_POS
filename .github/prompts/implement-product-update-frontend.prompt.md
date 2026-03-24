

# Implementar Frontend para Actualización de Productos

Implementa una interfaz frontend completa para actualizar productos utilizando los siguientes endpoints del API:

## 📋 Endpoints Disponibles

### 0. GET /api/Products/{id}
**Obtener producto por ID con TODA su información**

Este endpoint retorna el ProductResponseDto completo con TODOS los campos necesarios para poblar el formulario de edición.

**Request:**
```http
GET /api/Products/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Producto obtenido exitosamente",
  "error": 0,
  "data": {
    // ✅ INFORMACIÓN BÁSICA
    "id": 123,
    "name": "Nombre del Producto",
    "description": "Descripción completa",
    "code": "PROD-001",
    "barcode": "1234567890",
    "brand": "Marca",
    "model": "Modelo",
    
    // ✅ CLASIFICACIÓN
    "categoryId": 5,
    "categoryName": "Electrónica",
    "subcategoryId": 12,
    "subcategoryName": "Laptops",
    
    // ✅ INFORMACIÓN FISCAL
    "satCode": "43211500",
    "satUnit": "PZA",
    "satTaxType": "Tasa",
    "customsCode": "8471.30.01",
    "countryOfOrigin": "China",
    
    // ✅ PRECIOS Y COSTOS
    "baseCost": 100.00,
    "price": 150.00,
    "taxRate": 0.16,
    "priceWithTax": 174.00,
    
    // ✅ INVENTARIO
    "minimumStock": 5,
    "maximumStock": 50,
    "reorderPoint": 10,
    "unit": "PZA",
    
    // ✅ CARACTERÍSTICAS FÍSICAS
    "weight": 2.5,
    "length": 35.0,
    "width": 25.0,
    "height": 2.0,
    "color": "Negro",
    "size": "15 pulgadas",
    
    // ✅ INFORMACIÓN COMERCIAL
    "warranty": 12,
    "warrantyType": "Garantía del fabricante",
    "location": "Almacén A",
    "aisle": "A1",
    "shelf": "S3",
    "bin": "B5",
    
    // ✅ CLASIFICACIÓN AVANZADA
    "tags": "laptop,computadora,portátil",
    "season": "Todo el año",
    "targetGender": "Unisex",
    "ageGroup": "Adultos",
    
    // ✅ INFORMACIÓN DE VENTAS
    "maxQuantityPerSale": 10,
    "minQuantityPerSale": 1,
    "salesNotes": "Producto de alta rotación",
    "isDiscountAllowed": true,
    "maxDiscountPercentage": 15.0,
    
    // ✅ E-COMMERCE Y MARKETING
    "seoTitle": "Laptop 15 pulgadas - Marca Modelo",
    "seoDescription": "Laptop de alto rendimiento ideal para trabajo y estudio",
    "seoKeywords": "laptop,computadora,notebook",
    "isWebVisible": true,
    "isFeatured": false,
    "launchDate": "2026-01-15T00:00:00Z",
    "discontinuedDate": null,
    
    // ✅ INFORMACIÓN TÉCNICA
    "technicalSpecs": "Intel i7, 16GB RAM, 512GB SSD",
    "manufacturerPartNumber": "LP-15-2026",
    "upc": "012345678901",
    "ean": "0123456789012",
    "isbn": null,
    
    // ✅ LOGÍSTICA Y ENVÍO
    "isFragile": true,
    "requiresSpecialHandling": true,
    "shippingClass": "Electrónica",
    "packageLength": 40.0,
    "packageWidth": 30.0,
    "packageHeight": 10.0,
    "packageWeight": 3.5,
    
    // ✅ CONTROL DE CALIDAD
    "qualityGrade": "A",
    "lastQualityCheck": "2026-03-20T10:00:00Z",
    "defectRate": 0.5,
    "returnRate": 1.2,
    
    // ✅ ANÁLISIS Y REPORTES
    "abcClassification": "A",
    "velocityCode": "Fast",
    "profitMarginPercentage": 33.33,
    "lastSaleDate": "2026-03-23T15:30:00Z",
    "totalSalesQuantity": 45,
    
    // ✅ INFORMACIÓN ADICIONAL
    "internalNotes": "Verificar stock semanalmente",
    "customerNotes": "Incluye bolsa de transporte",
    "maintenanceInstructions": "Limpiar pantalla con paño suave",
    "safetyWarnings": "No exponer a líquidos",
    
    // ✅ CONFIGURACIÓN
    "isActive": true,
    "isService": false,
    "allowFractionalQuantities": false,
    "trackSerial": true,
    "trackExpiry": false,
    
    // ✅ PROVEEDORES
    "primarySupplierId": 8,
    "primarySupplierName": "Proveedor Tech SA",
    "supplierCode": "TECH-LP-001",
    
    // ✅ METADATA
    "createdAt": "2026-01-15T10:30:00Z",
    "updatedAt": "2026-03-20T14:00:00Z",
    "createdByName": "Admin User",
    "updatedByName": "Admin User",
    
    // ✅ IMAGEN (si existe)
    "imageUrl": "https://bucket.s3.amazonaws.com/products/123/laptop.jpg"
  }
}
```

**⚠️ IMPORTANTE:** Este endpoint retorna TODOS los campos del producto incluyendo la **imagen primaria** (si existe). Úsalo para:
1. Cargar los datos iniciales del formulario de edición
2. Poblar todos los campos con los valores actuales
3. Mostrar la imagen actual del producto en el formulario
4. Permitir que el usuario modifique solo lo que necesite

**Nota sobre la imagen:** El campo `imageUrl` contendrá la URL pública de S3 de la imagen primaria si el producto tiene una. Si no tiene imagen, este campo será `null`.

### 1. PUT /api/Products/{id}
**Actualizar solo datos del producto (sin imagen)**

**Request:**
```http
PUT /api/Products/{id}
Content-Type: application/json
Authorization: Bearer {token}

Body: UpdateProductRequestDto (JSON)
```

**Response:**
```json
{
  "message": "Producto actualizado exitosamente",
  "error": 0,
  "data": { /* ProductResponseDto */ },
  "updatedBy": "Usuario",
  "summary": {
    "productId": 123,
    "productCode": "PROD-001",
    "productName": "Nombre del Producto",
    "price": 150.00,
    "priceWithTax": 174.00,
    "profitMargin": 33.33,
    "category": "Electrónica",
    "isActive": true,
    "isService": false,
    "trackSerial": false,
    "trackExpiry": false,
    "updatedAt": "2026-03-24T10:30:00Z"
  }
}
```

### 2. PUT /api/Products/{id}/with-image
**Actualizar producto con reemplazo de imagen en S3**

**Request:**
```http
PUT /api/Products/{id}/with-image
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form Data:
- productData: string (JSON serializado de UpdateProductRequestDto)
- file: File (imagen opcional)
```

**Response:**
```json
{
  "message": "Producto e imagen actualizados exitosamente",
  "error": 0,
  "data": { /* ProductResponseDto */ },
  "image": {
    "s3Key": "products/123/imagen.jpg",
    "publicUrl": "https://bucket.s3.amazonaws.com/products/123/imagen.jpg",
    "fileName": "producto.jpg",
    "size": 245678,
    "uploadedAt": "2026-03-24T10:30:00Z",
    "uploadedBy": "Usuario"
  },
  "updatedBy": "Usuario",
  "updatedAt": "2026-03-24T10:30:00Z"
}
```

## 📦 DTO: UpdateProductRequestDto

```typescript
interface UpdateProductRequestDto {
  // ✅ INFORMACIÓN BÁSICA (Requeridos)
  name: string;           // max 200 chars
  code: string;           // max 50 chars
  
  // ✅ INFORMACIÓN BÁSICA (Opcionales)
  description?: string;   // max 1000 chars
  barcode?: string;       // max 50 chars
  brand?: string;         // max 100 chars
  model?: string;         // max 100 chars
  
  // ✅ CLASIFICACIÓN
  categoryId?: number;
  subcategoryId?: number;
  
  // ✅ INFORMACIÓN FISCAL
  satCode: string;        // default "01010101", max 10 chars
  satUnit: string;        // default "PZA", max 10 chars
  satTaxType: string;     // default "Tasa", max 20 chars
  customsCode?: string;   // max 20 chars
  countryOfOrigin: string; // default "México", max 50 chars
  
  // ✅ PRECIOS Y COSTOS
  baseCost: number;       // >= 0
  price: number;          // >= 0
  taxRate: number;        // 0-1 (0.16 = 16%)
  
  // ✅ INVENTARIO
  minimumStock: number;   // >= 0
  maximumStock: number;   // >= 0
  reorderPoint: number;   // >= 0
  unit: string;           // default "PZA", max 20 chars
  
  // ✅ CARACTERÍSTICAS FÍSICAS
  weight?: number;
  length?: number;
  width?: number;
  height?: number;
  color?: string;         // max 50 chars
  size?: string;          // max 50 chars
  
  // ✅ INFORMACIÓN COMERCIAL
  warranty?: number;
  warrantyType?: string;  // max 50 chars
  location?: string;      // max 100 chars
  aisle?: string;         // max 20 chars
  shelf?: string;         // max 20 chars
  bin?: string;           // max 20 chars
  
  // ✅ CLASIFICACIÓN AVANZADA
  tags?: string;          // max 500 chars
  season?: string;        // max 50 chars
  targetGender?: string;  // max 20 chars
  ageGroup?: string;      // max 50 chars
  
  // ✅ INFORMACIÓN DE VENTAS
  maxQuantityPerSale?: number;
  minQuantityPerSale: number;      // default 1
  salesNotes?: string;             // max 500 chars
  isDiscountAllowed: boolean;      // default true
  maxDiscountPercentage?: number;
  
  // ✅ E-COMMERCE Y MARKETING
  seoTitle?: string;           // max 200 chars
  seoDescription?: string;     // max 500 chars
  seoKeywords?: string;        // max 300 chars
  isWebVisible: boolean;       // default true
  isFeatured: boolean;         // default false
  launchDate?: string;         // ISO 8601
  discontinuedDate?: string;   // ISO 8601
  
  // ✅ INFORMACIÓN TÉCNICA
  technicalSpecs?: string;          // max 2000 chars
  manufacturerPartNumber?: string;  // max 100 chars
  upc?: string;                     // max 50 chars
  ean?: string;                     // max 50 chars
  isbn?: string;                    // max 50 chars
  
  // ✅ LOGÍSTICA Y ENVÍO
  isFragile: boolean;               // default false
  requiresSpecialHandling: boolean; // default false
  shippingClass?: string;           // max 50 chars
  packageLength?: number;
  packageWidth?: number;
  packageHeight?: number;
  packageWeight?: number;
  
  // ✅ CONTROL DE CALIDAD
  qualityGrade?: string;      // max 20 chars
  lastQualityCheck?: string;  // ISO 8601
  defectRate?: number;
  returnRate?: number;
  
  // ✅ ANÁLISIS Y REPORTES
  abcClassification?: string; // max 5 chars
  velocityCode?: string;      // max 20 chars
  profitMarginPercentage?: number;
  lastSaleDate?: string;      // ISO 8601
  totalSalesQuantity: number; // default 0
  
  // ✅ INFORMACIÓN ADICIONAL
  internalNotes?: string;            // max 1000 chars
  customerNotes?: string;            // max 500 chars
  maintenanceInstructions?: string;  // max 1000 chars
  safetyWarnings?: string;           // max 500 chars
  
  // ✅ CONFIGURACIÓN
  isActive: boolean;                  // default true
  isService: boolean;                 // default false
  allowFractionalQuantities: boolean; // default false
  trackSerial: boolean;               // default false
  trackExpiry: boolean;               // default false
  
  // ✅ PROVEEDORES
  primarySupplierId?: number;
  supplierCode?: string;              // max 100 chars
}
```

## 🎯 Requisitos de Implementación

1. **Carga Inicial de Datos:**
   - Al abrir el formulario, hacer GET /api/Products/{id}
   - El endpoint retorna ProductResponseDto con TODOS los campos
   - Poblar todos los campos del formulario con los valores actuales
   - **Mostrar la imagen actual** si `product.imageUrl` no es null
   - Mostrar indicador de carga mientras se obtienen los datos

2. **Formulario de Edición:**
   - Todos los campos editables según el DTO
   - Validación de campos requeridos (name, code)
   - Validación de rangos numéricos (valores >= 0)
   - Validación de longitudes máximas de strings
   - **Display de imagen actual** usando `product.imageUrl` (viene del GET)
   - Opción para seleccionar nueva imagen

3. **Manejo de Imagen:**
   - **Vista previa de imagen actual** (cargada desde `product.imageUrl`)
   - Selector de archivo (solo imágenes: JPEG, PNG, GIF, WEBP)
   - Validación de tamaño máximo: 5MB
   - Preview de nueva imagen antes de guardar
   - **Indicador de que la imagen actual será reemplazada** (mostrar advertencia)

3. **Lógica de Guardado:**
   - Si NO hay nueva imagen → usar endpoint PUT /api/Products/{id}
   - Si HAY nueva imagen → usar endpoint PUT /api/Products/{id}/with-image
   - Loading state durante la actualización
   - Manejo de errores (401, 400, 500)
   - Mensaje de éxito con detalles del producto actualizado

4. **UX/UI:**
   - Botón "Cancelar" que descarta cambios
   - Botón "Guardar" que muestra loading
   - Confirmación antes de reemplazar imagen
   - Indicador visual de campos modificados
   - Notificación de éxito/error

5. **Validaciones Frontend:**
   - Campos requeridos: name, code
   - Máxima longitud de strings
   - Valores numéricos >= 0
   - Formato de imagen correcto
   - Tamaño de imagen <= 5MB

## 📝 Ejemplo de Implementación (Fetch)

### Cargar datos del producto:
```javascript
const loadProduct = async (productId) => {
  try {
    const response = await fetch(`/api/Products/${productId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Error al cargar producto');
    }
    
    const result = await response.json();
    
    // result.data contiene ProductResponseDto con TODOS los campos
    // result.data.imageUrl contiene la URL de la imagen primaria (si existe)
    return result.data;
  } catch (error) {
    console.error('Error loading product:', error);
    throw error;
  }
};

// Uso en el componente:
// const productData = await loadProduct(123);
// populateForm(productData); // Llenar todos los campos del formulario
// if (productData.imageUrl) {
//   showCurrentImage(productData.imageUrl); // Mostrar imagen actual
// }
```

### Actualizar sin imagen:
```javascript
const updateProduct = async (productId, productData) => {
  const response = await fetch(`/api/Products/${productId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(productData)
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Error al actualizar producto');
  }
  
  return await response.json();
};
```

### Actualizar con imagen:
```javascript
const updateProductWithImage = async (productId, productData, imageFile) => {
  const formData = new FormData();
  formData.append('productData', JSON.stringify(productData));
  
  if (imageFile) {
    formData.append('file', imageFile);
  }
  
  const response = await fetch(`/api/Products/${productId}/with-image`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Error al actualizar producto con imagen');
  }
  
  return await response.json();
};
```

## ⚠️ Consideraciones Importantes

1. **Reemplazo de Imagen:** 
   - El endpoint `/with-image` ELIMINA la imagen primaria anterior de S3
   - No hay manera de recuperar la imagen anterior una vez reemplazada
   - Considerar mostrar advertencia al usuario

2. **Form Data:**
   - Para el endpoint `/with-image`, los datos del producto deben ir como string JSON en el campo `productData`
   - El archivo va en el campo `file`
   - No usar `Content-Type: application/json` cuando se envía FormData

3. **Errores Comunes:**
   - Error 401: Token inválido o expirado
   - Error 400: Validación fallida (revisar `errors` en respuesta)
   - Error 404: Producto no encontrado
   - Error 500: Error del servidor

4. **Optimización:**
   - Usar debouncing para validaciones en tiempo real
   - Comprimir imagen antes de enviar (opcional)
   - Mostrar progreso de carga para archivos grandes

## 🚀 Tarea

Implementa una interfaz completa de edición de productos que:
- **Cargue los datos actuales del producto usando GET /api/Products/{id}**
- **Poblar TODOS los campos del formulario con los valores del ProductResponseDto**
- **Mostrar la imagen actual del producto** (viene en `data.imageUrl` del GET)
- Permita editar todos los campos relevantes según el DTO
- Maneje la selección y preview de imágenes (actual y nueva)
- Use el endpoint correcto según si hay o no nueva imagen:
  - Sin imagen: PUT /api/Products/{id}
  - Con imagen: PUT /api/Products/{id}/with-image
- Proporcione feedback claro al usuario (loading, success, errors)
- Maneje errores apropiadamente (401, 400, 404, 500)
- Valide los datos antes de enviar
- Muestre confirmación antes de reemplazar la imagen

**Flujo completo:**
1. Usuario abre formulario de edición → GET /api/Products/{id}
2. Sistema carga TODOS los datos en el formulario
3. **Sistema muestra la imagen actual** si existe (`data.imageUrl`)
4. Usuario modifica los campos que desee
5. Si selecciona nueva imagen → mostrar preview y advertencia de reemplazo
6. Al guardar → validar datos → enviar al endpoint correcto
7. Mostrar resultado (éxito o error) → redirigir o actualizar vista

Framework a utilizar: **{argument}**
