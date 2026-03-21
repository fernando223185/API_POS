# DOCUMENTACIÓN DE ENDPOINTS - CATÁLOGOS SAT

## Base URL
```
http://localhost:7254/api/sat
```

## Autenticación
Todos los endpoints requieren token JWT en el header:
```
Authorization: Bearer <token>
```

---

## 1. GET /api/sat/uso-cfdi
**Descripción:** Obtiene el catálogo de Uso del CFDI (c_UsoCFDI)

**Query Parameters:**
- `personaFisica` (optional, boolean): Filtrar por aplicable a persona física
- `personaMoral` (optional, boolean): Filtrar por aplicable a persona moral

**Ejemplos de uso:**
```
GET /api/sat/uso-cfdi
GET /api/sat/uso-cfdi?personaFisica=true
GET /api/sat/uso-cfdi?personaMoral=true
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Catálogo obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "codigo": "G01",
      "descripcion": "Adquisición de mercancías",
      "aplicaPersonaFisica": true,
      "aplicaPersonaMoral": true
    },
    {
      "codigo": "G03",
      "descripcion": "Gastos en general",
      "aplicaPersonaFisica": true,
      "aplicaPersonaMoral": true
    },
    {
      "codigo": "S01",
      "descripcion": "Sin efectos fiscales",
      "aplicaPersonaFisica": true,
      "aplicaPersonaMoral": true
    },
    {
      "codigo": "D01",
      "descripcion": "Honorarios médicos, dentales y gastos hospitalarios",
      "aplicaPersonaFisica": true,
      "aplicaPersonaMoral": false
    }
  ],
  "total": 24
}
```

**Para Select/Dropdown usar:**
- **value:** `data[].codigo` (ej: "G01", "G03", "S01")
- **label:** `data[].descripcion` (ej: "Gastos en general")
- **Formato sugerido:** `"G01 - Adquisición de mercancías"` (codigo + descripcion)

---

## 2. GET /api/sat/regimen-fiscal
**Descripción:** Obtiene el catálogo de Régimen Fiscal (c_RegimenFiscal)

**Query Parameters:**
- `personaFisica` (optional, boolean): Filtrar por aplicable a persona física
- `personaMoral` (optional, boolean): Filtrar por aplicable a persona moral

**Ejemplos de uso:**
```
GET /api/sat/regimen-fiscal
GET /api/sat/regimen-fiscal?personaFisica=true
GET /api/sat/regimen-fiscal?personaMoral=true
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Catálogo obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "codigo": "601",
      "descripcion": "General de Ley Personas Morales",
      "aplicaPersonaFisica": false,
      "aplicaPersonaMoral": true
    },
    {
      "codigo": "612",
      "descripcion": "Personas Físicas con Actividades Empresariales y Profesionales",
      "aplicaPersonaFisica": true,
      "aplicaPersonaMoral": false
    },
    {
      "codigo": "616",
      "descripcion": "Sin obligaciones fiscales",
      "aplicaPersonaFisica": true,
      "aplicaPersonaMoral": false
    },
    {
      "codigo": "626",
      "descripcion": "Régimen Simplificado de Confianza",
      "aplicaPersonaFisica": true,
      "aplicaPersonaMoral": true
    }
  ],
  "total": 19
}
```

**Para Select/Dropdown usar:**
- **value:** `data[].codigo` (ej: "601", "612", "616")
- **label:** `data[].descripcion`
- **Formato sugerido:** `"612 - Personas Físicas con Actividades Empresariales"`

---

## 3. GET /api/sat/forma-pago
**Descripción:** Obtiene el catálogo de Forma de Pago (c_FormaPago)

**Ejemplos de uso:**
```
GET /api/sat/forma-pago
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Catálogo obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "codigo": "01",
      "descripcion": "Efectivo",
      "bancarizado": null
    },
    {
      "codigo": "03",
      "descripcion": "Transferencia electrónica de fondos",
      "bancarizado": "Sí"
    },
    {
      "codigo": "04",
      "descripcion": "Tarjeta de crédito",
      "bancarizado": "Sí"
    },
    {
      "codigo": "28",
      "descripcion": "Tarjeta de débito",
      "bancarizado": "Sí"
    },
    {
      "codigo": "99",
      "descripcion": "Por definir",
      "bancarizado": null
    }
  ],
  "total": 22
}
```

**Para Select/Dropdown usar:**
- **value:** `data[].codigo` (ej: "01", "03", "04")
- **label:** `data[].descripcion`
- **Formato sugerido:** `"01 - Efectivo"`

---

## 4. GET /api/sat/metodo-pago
**Descripción:** Obtiene el catálogo de Método de Pago (c_MetodoPago)

**Ejemplos de uso:**
```
GET /api/sat/metodo-pago
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Catálogo obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "codigo": "PUE",
      "descripcion": "Pago en una sola exhibición"
    },
    {
      "codigo": "PPD",
      "descripcion": "Pago en parcialidades o diferido"
    }
  ],
  "total": 2
}
```

**Para Select/Dropdown usar:**
- **value:** `data[].codigo` (ej: "PUE", "PPD")
- **label:** `data[].descripcion`
- **Formato sugerido:** `"PUE - Pago en una sola exhibición"`

---

## 5. GET /api/sat/tipo-comprobante
**Descripción:** Obtiene el catálogo de Tipo de Comprobante (c_TipoDeComprobante)

**Ejemplos de uso:**
```
GET /api/sat/tipo-comprobante
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Catálogo obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "codigo": "I",
      "descripcion": "Ingreso"
    },
    {
      "codigo": "E",
      "descripcion": "Egreso"
    },
    {
      "codigo": "T",
      "descripcion": "Traslado"
    },
    {
      "codigo": "N",
      "descripcion": "Nómina"
    },
    {
      "codigo": "P",
      "descripcion": "Pago"
    }
  ],
  "total": 5
}
```

**Para Select/Dropdown usar:**
- **value:** `data[].codigo` (ej: "I", "E", "T")
- **label:** `data[].descripcion`
- **Formato sugerido:** `"I - Ingreso"`

---

## 6. GET /api/sat/unidad-medida
**Descripción:** Obtiene el catálogo de Unidad de Medida (c_ClaveUnidad)

**Query Parameters:**
- `search` (optional, string): Búsqueda por código, nombre o descripción

**Ejemplos de uso:**
```
GET /api/sat/unidad-medida
GET /api/sat/unidad-medida?search=pieza
GET /api/sat/unidad-medida?search=kg
GET /api/sat/unidad-medida?search=H87
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Catálogo obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "claveUnidad": "H87",
      "nombre": "Pieza",
      "simbolo": "Pieza",
      "descripcion": null
    },
    {
      "claveUnidad": "KGM",
      "nombre": "Kilogramo",
      "simbolo": "kg",
      "descripcion": null
    },
    {
      "claveUnidad": "LTR",
      "nombre": "Litro",
      "simbolo": "l",
      "descripcion": null
    },
    {
      "claveUnidad": "MTR",
      "nombre": "Metro",
      "simbolo": "m",
      "descripcion": null
    },
    {
      "claveUnidad": "XUN",
      "nombre": "Unidad",
      "simbolo": "Unidad",
      "descripcion": null
    }
  ],
  "total": 20
}
```

**Para Select/Dropdown usar:**
- **value:** `data[].claveUnidad` (ej: "H87", "KGM", "LTR")
- **label:** `data[].nombre` o `data[].simbolo`
- **Formato sugerido:** `"H87 - Pieza"` o `"KGM - kg"`
- **Búsqueda:** Implementar autocomplete con el parámetro `search`

---

## 7. GET /api/sat/producto-servicio
**Descripción:** Obtiene el catálogo de Producto/Servicio (c_ClaveProdServ)

**Query Parameters:**
- `search` (optional, string): Búsqueda por código o descripción
- `limit` (optional, int): Límite de resultados (default: 50, max sugerido: 100)

**Ejemplos de uso:**
```
GET /api/sat/producto-servicio
GET /api/sat/producto-servicio?search=software
GET /api/sat/producto-servicio?search=43231500
GET /api/sat/producto-servicio?search=café&limit=20
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Catálogo obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "claveProdServ": "01010101",
      "descripcion": "No existe en el catálogo",
      "incluyeIva": "No",
      "incluyeIeps": "No"
    },
    {
      "claveProdServ": "43222600",
      "descripcion": "Software",
      "incluyeIva": "Sí",
      "incluyeIeps": "No"
    },
    {
      "claveProdServ": "50202201",
      "descripcion": "Café",
      "incluyeIva": "Sí",
      "incluyeIeps": "No"
    },
    {
      "claveProdServ": "76121500",
      "descripcion": "Servicios de restaurante",
      "incluyeIva": "Sí",
      "incluyeIeps": "No"
    }
  ],
  "total": 20
}
```

**Para Select/Dropdown usar:**
- **value:** `data[].claveProdServ` (ej: "43222600", "01010101")
- **label:** `data[].descripcion`
- **Formato sugerido:** `"43222600 - Software"`
- **Búsqueda:** Implementar autocomplete con el parámetro `search`
- **Límite:** Usar `limit` para controlar cantidad de resultados en búsquedas

---

## RESUMEN PARA INTEGRACIÓN EN FRONTEND

### Endpoints a consumir en cada sección:

**Formulario de Facturación:**
1. **Uso CFDI** → `/api/sat/uso-cfdi?personaMoral=true` (si es empresa) o `?personaFisica=true` (si es persona)
2. **Régimen Fiscal** → `/api/sat/regimen-fiscal?personaMoral=true` (para empresas)
3. **Forma de Pago** → `/api/sat/forma-pago`
4. **Método de Pago** → `/api/sat/metodo-pago`

**Formulario de Cliente/Empresa:**
1. **Régimen Fiscal** → `/api/sat/regimen-fiscal` (con filtros según tipo de persona)
2. **Uso CFDI predeterminado** → `/api/sat/uso-cfdi`

**Formulario de Producto:**
1. **Clave Producto/Servicio SAT** → `/api/sat/producto-servicio?search={texto}`
2. **Unidad de Medida SAT** → `/api/sat/unidad-medida?search={texto}`

### Patrón de respuesta estandarizado:
```typescript
interface SatCatalogResponse<T> {
  message: string;
  error: number;
  data: T[];
  total: number;
}
```

### Manejo de errores:
- **200 OK**: Catálogo obtenido correctamente
- **401 Unauthorized**: Token JWT inválido o expirado
- **500 Internal Server Error**: Error en el servidor

### Recomendaciones de uso:
1. **Cache local**: Guardar catálogos en localStorage/sessionStorage para evitar peticiones repetidas
2. **Autocomplete**: Usar parámetro `search` en tiempo real para unidades y productos
3. **Filtrado**: Aplicar filtros `personaFisica`/`personaMoral` según el tipo de entidad
4. **Formato display**: Combinar `codigo + " - " + descripcion` para mejor UX
5. **Precargar**: Cargar catálogos base (Método Pago, Forma Pago, Tipo Comprobante) al inicio

---

## EJEMPLO DE IMPLEMENTACIÓN EN JAVASCRIPT/TYPESCRIPT

```javascript
// Servicio para catálogos SAT
const API_BASE = 'http://localhost:7254/api/sat';

async function getSatCatalog(endpoint, params = {}) {
  const queryString = new URLSearchParams(params).toString();
  const url = `${API_BASE}/${endpoint}${queryString ? '?' + queryString : ''}`;
  
  const response = await fetch(url, {
    headers: {
      'Authorization': `Bearer ${getToken()}`,
      'Content-Type': 'application/json'
    }
  });
  
  if (!response.ok) {
    throw new Error(`Error ${response.status}: ${response.statusText}`);
  }
  
  return await response.json();
}

// Ejemplos de uso:
const usoCfdi = await getSatCatalog('uso-cfdi', { personaMoral: true });
const formaPago = await getSatCatalog('forma-pago');
const productos = await getSatCatalog('producto-servicio', { search: 'software', limit: 20 });

// Poblar select
function populateSelect(selectElement, data, valueKey, labelKey) {
  selectElement.innerHTML = '<option value="">Seleccione...</option>';
  data.data.forEach(item => {
    const option = document.createElement('option');
    option.value = item[valueKey];
    option.textContent = `${item[valueKey]} - ${item[labelKey]}`;
    selectElement.appendChild(option);
  });
}

// Uso específico
const selectUsoCfdi = document.getElementById('usoCfdi');
const catalogoUsoCfdi = await getSatCatalog('uso-cfdi');
populateSelect(selectUsoCfdi, catalogoUsoCfdi, 'codigo', 'descripcion');
```

---

**NOTA FINAL:** Todos los endpoints devuelven datos activos del SAT (IsActive = true). Los catálogos están precargados con los valores oficiales vigentes en 2026.
