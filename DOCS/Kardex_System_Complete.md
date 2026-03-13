# ?? **Sistema de Kardex/Movimientos de Inventario - IMPLEMENTADO**

## ?? **COMPLETADO - ENDPOINT LISTO**

---

## ?? **Resumen**

Se ha implementado un **sistema completo de kardex** (historial de movimientos de inventario) con:

1. ? **Consulta paginada** con múltiples filtros
2. ? **Estadísticas en tiempo real** (entradas hoy, salidas hoy, valor movido)
3. ? **Exportación a Excel y PDF**
4. ? **Consulta por producto específico**

---

## ?? **Endpoints Disponibles**

### **1. Obtener Kardex con Filtros y Paginación**

```http
GET /api/kardex?productId=1&warehouseId=1&movementType=IN&fromDate=2026-01-01&toDate=2026-03-11&page=1&pageSize=20
Authorization: Bearer {token}
```

**Parámetros de Query:**

| Parámetro | Tipo | Descripción | Requerido | Default |
|-----------|------|-------------|-----------|---------|
| `productId` | int | ID del producto | No | null |
| `productSearch` | string | Búsqueda por código o nombre | No | null |
| `warehouseId` | int | ID del almacén | No | null |
| `movementType` | string | IN (entradas) o OUT (salidas) | No | null |
| `fromDate` | DateTime | Fecha inicial | No | null |
| `toDate` | DateTime | Fecha final | No | null |
| `page` | int | Número de página | No | 1 |
| `pageSize` | int | Tamaño de página (máx 100) | No | 20 |

**Respuesta:**
```json
{
  "message": "Kardex obtenido exitosamente",
  "error": 0,
  "data": [
    {
      "id": 1,
      "movementCode": "MOV-001",
      "movementDate": "2026-03-11T10:30:00",
      "movementType": "IN/PURCHASE",
      "movementTypeName": "Entrada - Compra",
      "productId": 1,
      "productCode": "PROD-001",
      "productName": "iPhone 15 Pro",
      "warehouseId": 1,
      "warehouseName": "Almacén Principal",
      "warehouseCode": "ALM-001",
      "quantity": 10.00,
      "stockBefore": 5.00,
      "stockAfter": 15.00,
      "unitCost": 1000.00,
      "totalCost": 10000.00,
      "purchaseOrderReceivingId": 1,
      "purchaseOrderReceivingCode": "REC-001",
      "saleId": null,
      "saleCode": null,
      "notes": "Recepción de compra",
      "createdByUserName": "Admin"
    }
  ],
  "statistics": {
    "totalMovements": 50,
    "entriesToday": 3,
    "exitsToday": 5,
    "totalValue": 191875.00,
    "totalEntriesQuantity": 100.00,
    "totalExitsQuantity": 45.00,
    "totalEntriesValue": 150000.00,
    "totalExitsValue": 41875.00
  },
  "page": 1,
  "pageSize": 20,
  "totalRecords": 50,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

### **2. Obtener Estadísticas del Kardex**

```http
GET /api/kardex/statistics?productId=1&warehouseId=1&fromDate=2026-01-01&toDate=2026-03-11
Authorization: Bearer {token}
```

**Parámetros:**
| Parámetro | Tipo | Descripción | Requerido |
|-----------|------|-------------|-----------|
| `productId` | int | Filtrar por producto | No |
| `warehouseId` | int | Filtrar por almacén | No |
| `fromDate` | DateTime | Fecha inicial | No |
| `toDate` | DateTime | Fecha final | No |

**Respuesta:**
```json
{
  "message": "Estadísticas del kardex obtenidas exitosamente",
  "error": 0,
  "data": {
    "totalMovements": 5,
    "entriesToday": 0,
    "exitsToday": 0,
    "totalValue": 1918.75,
    "totalEntriesQuantity": 15.00,
    "totalExitsQuantity": 3.00,
    "totalEntriesValue": 2000.00,
    "totalExitsValue": 151.00
  }
}
```

---

### **3. Exportar Kardex a Excel**

```http
GET /api/kardex/export/excel?productId=1&warehouseId=1&fromDate=2026-01-01&toDate=2026-03-11
Authorization: Bearer {token}
```

**Parámetros:** Los mismos que el endpoint principal

**Respuesta:**
- **Content-Type:** `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- **Archivo:** `kardex_20260311_143022.xlsx`

**Contenido del Excel:**
| Fecha/Hora | Código Movimiento | Tipo | Producto | Almacén | Cantidad | Saldo Anterior | Saldo Nuevo | Costo Unit. | Total | Referencia | Usuario | Notas |
|------------|-------------------|------|----------|---------|----------|----------------|-------------|-------------|-------|------------|---------|-------|
| 11/03/2026 10:30 | MOV-001 | Entrada - Compra | PROD-001 - iPhone 15 Pro | Almacén Principal | 10.00 | 5.00 | 15.00 | $1,000.00 | $10,000.00 | REC-001 | Admin | Recepción de compra |

---

### **4. Exportar Kardex a PDF**

```http
GET /api/kardex/export/pdf?productId=1&warehouseId=1&fromDate=2026-01-01&toDate=2026-03-11
Authorization: Bearer {token}
```

**Parámetros:** Los mismos que el endpoint principal

**Respuesta:**
- **Content-Type:** `application/pdf`
- **Archivo:** `kardex_20260311_143022.pdf`

**Características del PDF:**
- ? Formato horizontal (landscape) para más columnas
- ? Tabla completa con todos los datos
- ? Entradas en verde claro
- ? Salidas en rojo claro
- ? Encabezado con título y fecha de generación
- ? Información del período consultado
- ? Pie de página con número de página

---

### **5. Obtener Kardex de un Producto Específico**

```http
GET /api/kardex/product/1?warehouseId=1&fromDate=2026-01-01&toDate=2026-03-11&page=1&pageSize=50
Authorization: Bearer {token}
```

**Parámetros:**
| Parámetro | Tipo | Descripción | Requerido |
|-----------|------|-------------|-----------|
| `productId` | int (ruta) | ID del producto | Sí |
| `warehouseId` | int | Filtrar por almacén | No |
| `fromDate` | DateTime | Fecha inicial | No |
| `toDate` | DateTime | Fecha final | No |
| `page` | int | Página | No |
| `pageSize` | int | Tamaño (default: 50) | No |

---

## ?? **Estadísticas Incluidas**

Cada consulta incluye estadísticas en tiempo real:

```json
{
  "statistics": {
    "totalMovements": 50,           // Total de movimientos en el período
    "entriesToday": 3,               // Entradas HOY
    "exitsToday": 5,                 // Salidas HOY
    "totalValue": 191875.00,         // Valor total movido
    "totalEntriesQuantity": 100.00,  // Cantidad total de entradas
    "totalExitsQuantity": 45.00,     // Cantidad total de salidas
    "totalEntriesValue": 150000.00,  // Valor total de entradas
    "totalExitsValue": 41875.00      // Valor total de salidas
  }
}
```

---

## ?? **Tipos de Movimientos**

| Código | Descripción |
|--------|-------------|
| `IN/PURCHASE` | Entrada - Compra |
| `IN/ADJUSTMENT` | Entrada - Ajuste |
| `IN/TRANSFER` | Entrada - Traspaso |
| `IN/RETURN` | Entrada - Devolución |
| `OUT/SALE` | Salida - Venta |
| `OUT/ADJUSTMENT` | Salida - Ajuste |
| `OUT/TRANSFER` | Salida - Traspaso |
| `OUT/RETURN` | Salida - Devolución |

---

## ?? **Integración con Frontend**

### **React/Vue/Angular Example:**

```typescript
// services/kardexApi.ts
export const KardexApi = {
  // Obtener kardex con filtros
  async getKardex(filters: {
    productId?: number;
    warehouseId?: number;
    movementType?: string;
    fromDate?: string;
    toDate?: string;
    page?: number;
    pageSize?: number;
  }) {
    const params = new URLSearchParams();
    
    if (filters.productId) params.append('productId', filters.productId.toString());
    if (filters.warehouseId) params.append('warehouseId', filters.warehouseId.toString());
    if (filters.movementType) params.append('movementType', filters.movementType);
    if (filters.fromDate) params.append('fromDate', filters.fromDate);
    if (filters.toDate) params.append('toDate', filters.toDate);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());
    
    const response = await fetch(
      `${API_URL}/api/kardex?${params.toString()}`,
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`
        }
      }
    );
    
    return await response.json();
  },

  // Obtener estadísticas
  async getStatistics(filters: {
    productId?: number;
    warehouseId?: number;
    fromDate?: string;
    toDate?: string;
  }) {
    const params = new URLSearchParams();
    
    if (filters.productId) params.append('productId', filters.productId.toString());
    if (filters.warehouseId) params.append('warehouseId', filters.warehouseId.toString());
    if (filters.fromDate) params.append('fromDate', filters.fromDate);
    if (filters.toDate) params.append('toDate', filters.toDate);
    
    const response = await fetch(
      `${API_URL}/api/kardex/statistics?${params.toString()}`,
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`
        }
      }
    );
    
    const result = await response.json();
    return result.data;
  },

  // Exportar a Excel
  async exportExcel(filters: any) {
    const params = new URLSearchParams();
    Object.keys(filters).forEach(key => {
      if (filters[key]) params.append(key, filters[key].toString());
    });
    
    const response = await fetch(
      `${API_URL}/api/kardex/export/excel?${params.toString()}`,
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`
        }
      }
    );
    
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = url;
    a.download = `kardex_${new Date().toISOString().slice(0,10)}.xlsx`;
    a.click();
    
    URL.revokeObjectURL(url);
  },

  // Exportar a PDF
  async exportPdf(filters: any) {
    const params = new URLSearchParams();
    Object.keys(filters).forEach(key => {
      if (filters[key]) params.append(key, filters[key].toString());
    });
    
    const response = await fetch(
      `${API_URL}/api/kardex/export/pdf?${params.toString()}`,
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`
        }
      }
    );
    
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
  }
};
```

### **Componente de Vista de Kardex:**

```vue
<template>
  <div class="kardex-view">
    <!-- Estadísticas -->
    <div class="stats-row">
      <div class="stat-card">
        <h3>Total Movimientos</h3>
        <p>{{ statistics.totalMovements }}</p>
      </div>
      <div class="stat-card green">
        <h3>Entradas Hoy</h3>
        <p>{{ statistics.entriesToday }}</p>
      </div>
      <div class="stat-card red">
        <h3>Salidas Hoy</h3>
        <p>{{ statistics.exitsToday }}</p>
      </div>
      <div class="stat-card blue">
        <h3>Valor Movido</h3>
        <p>${{ statistics.totalValue.toFixed(2) }}</p>
      </div>
    </div>

    <!-- Filtros -->
    <div class="filters">
      <input v-model="filters.productSearch" placeholder="Buscar producto..." />
      <select v-model="filters.warehouseId">
        <option :value="null">Todos los almacenes</option>
        <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
      </select>
      <select v-model="filters.movementType">
        <option :value="null">Todos los tipos</option>
        <option value="IN">Entradas</option>
        <option value="OUT">Salidas</option>
      </select>
      <input v-model="filters.fromDate" type="date" />
      <input v-model="filters.toDate" type="date" />
      <button @click="exportExcel">?? Excel</button>
      <button @click="exportPdf">?? PDF</button>
    </div>

    <!-- Tabla de movimientos -->
    <table class="kardex-table">
      <thead>
        <tr>
          <th>Fecha/Hora</th>
          <th>Tipo</th>
          <th>Producto</th>
          <th>Almacén</th>
          <th>Cantidad</th>
          <th>Saldo</th>
          <th>Costo Unit.</th>
          <th>Total</th>
          <th>Ref/Usuario</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="m in movements" :key="m.id" :class="{ 'entry': m.movementType.startsWith('IN'), 'exit': m.movementType.startsWith('OUT') }">
          <td>{{ formatDate(m.movementDate) }}</td>
          <td>{{ m.movementTypeName }}</td>
          <td>{{ m.productCode }} - {{ m.productName }}</td>
          <td>{{ m.warehouseName }}</td>
          <td>{{ m.quantity.toFixed(2) }}</td>
          <td>{{ m.stockAfter.toFixed(2) }}</td>
          <td>${{ m.unitCost?.toFixed(2) || '-' }}</td>
          <td>${{ m.totalCost?.toFixed(2) || '-' }}</td>
          <td>{{ m.purchaseOrderReceivingCode || m.saleCode || '-' }} / {{ m.createdByUserName }}</td>
          <td>
            <button @click="viewDetails(m)">???</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Paginación -->
    <div class="pagination">
      <button @click="previousPage" :disabled="!hasPreviousPage">Anterior</button>
      <span>Página {{ currentPage }} de {{ totalPages }}</span>
      <button @click="nextPage" :disabled="!hasNextPage">Siguiente</button>
    </div>
  </div>
</template>

<script>
import { KardexApi } from '@/services/kardexApi';

export default {
  data() {
    return {
      movements: [],
      statistics: {},
      filters: {
        productSearch: '',
        warehouseId: null,
        movementType: null,
        fromDate: null,
        toDate: null
      },
      currentPage: 1,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false
    };
  },
  async mounted() {
    await this.loadKardex();
  },
  methods: {
    async loadKardex() {
      const result = await KardexApi.getKardex({
        ...this.filters,
        page: this.currentPage,
        pageSize: 20
      });
      
      this.movements = result.data;
      this.statistics = result.statistics;
      this.currentPage = result.page;
      this.totalPages = result.totalPages;
      this.hasPreviousPage = result.hasPreviousPage;
      this.hasNextPage = result.hasNextPage;
    },
    async exportExcel() {
      await KardexApi.exportExcel(this.filters);
    },
    async exportPdf() {
      await KardexApi.exportPdf(this.filters);
    },
    async nextPage() {
      if (this.hasNextPage) {
        this.currentPage++;
        await this.loadKardex();
      }
    },
    async previousPage() {
      if (this.hasPreviousPage) {
        this.currentPage--;
        await this.loadKardex();
      }
    }
  }
};
</script>
```

---

## ? **Archivos Creados**

1. ? `Application/DTOs/Inventory/KardexDtos.cs`
2. ? `Application/Core/Inventory/Queries/KardexQueries.cs`
3. ? `Infrastructure/QueryHandlers/KardexQueryHandlers.cs`
4. ? `Application/Abstractions/Documents/IKardexDocumentService.cs`
5. ? `Infrastructure/Services/KardexDocumentService.cs`
6. ? `Web.Api/Controllers/Inventory/KardexController.cs`

---

## ?? **Pruebas en Postman**

### **1. Consultar Kardex:**
```
GET http://localhost:7254/api/kardex?page=1&pageSize=20
Authorization: Bearer {token}
```

### **2. Filtrar por Producto:**
```
GET http://localhost:7254/api/kardex?productId=1&fromDate=2026-01-01&toDate=2026-03-11
Authorization: Bearer {token}
```

### **3. Filtrar por Almacén:**
```
GET http://localhost:7254/api/kardex?warehouseId=1
Authorization: Bearer {token}
```

### **4. Solo Entradas:**
```
GET http://localhost:7254/api/kardex?movementType=IN
Authorization: Bearer {token}
```

### **5. Exportar a Excel:**
```
GET http://localhost:7254/api/kardex/export/excel?fromDate=2026-01-01&toDate=2026-03-11
Authorization: Bearer {token}
```

### **6. Exportar a PDF:**
```
GET http://localhost:7254/api/kardex/export/pdf?productId=1
Authorization: Bearer {token}
```

---

## ?? **Para AWS/Producción**

```bash
# 1. Compilar
dotnet build

# 2. Publicar
dotnet publish -c Release -o ./publish

# 3. Subir a AWS
scp -i tu-key.pem -r ./publish/* ec2-user@servidor:/var/www/erpapi/

# 4. Reiniciar servicio
ssh -i tu-key.pem ec2-user@servidor
sudo systemctl restart erpapi
```

**? No requiere cambios en base de datos** - Solo desplegar código actualizado

---

## ?? **Estado Final**

- ? **5 endpoints** implementados
- ? **Consulta con filtros** múltiples
- ? **Estadísticas en tiempo real**
- ? **Exportación Excel y PDF**
- ? **Compilación exitosa**
- ? **Handlers de MediatR registrados correctamente**
- ? **Patrón CQRS completo** (Queries + QueryHandlers en Infrastructure)
- ? **Listo para producción**

---

## ?? **Importante: Configuración de MediatR**

Para que el sistema funcione correctamente, MediatR debe escanear **tanto Application como Infrastructure**:

```csharp
// En Program.cs
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Infrastructure.AssemblyReference).Assembly);
});
```

**Archivos requeridos:**
- ? `Application/AssemblyReference.cs`
- ? `Infrastructure/AssemblyReference.cs`

Estos archivos permiten que MediatR encuentre los handlers en ambos proyectos.

---

**?? SISTEMA DE KARDEX COMPLETAMENTE IMPLEMENTADO** ?

**Fecha:** 2026-03-11  
**Endpoints:** 5 endpoints de kardex  
**Exportación:** Excel + PDF  
**Patrón:** CQRS completo  
**Estado:** ? **LISTO PARA USAR**
