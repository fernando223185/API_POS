# ?? **Sistema de Generación de Tickets de Venta**

## ?? **COMPLETADO - ENDPOINTS LISTOS**

---

## ?? **Resumen**

Se han implementado **4 nuevos endpoints** para generar tickets de venta en diferentes formatos:

1. ? **Ticket Térmico (Texto)** - Para impresoras térmicas
2. ? **Ticket Térmico (Binario ESC/POS)** - Para envío directo a impresora
3. ? **Ticket PDF** - Para impresoras láser o compartir digitalmente
4. ? **Factura PDF** - Formato fiscal completo

---

## ?? **Endpoints Disponibles**

### **1. Ticket Térmico (Formato Texto)**

**Descripción:** Genera un ticket de venta en formato texto plano, listo para mostrar en pantalla o enviar a impresora térmica.

```http
GET /api/sales/{id}/ticket/thermal?width=48
Authorization: Bearer {token}
```

**Parámetros:**
| Parámetro | Tipo | Descripción | Requerido | Default |
|-----------|------|-------------|-----------|---------|
| `id` | int | ID de la venta | Sí | - |
| `width` | int | Ancho del papel (48=80mm, 32=58mm) | No | 48 |

**Respuesta:**
```json
{
  "message": "Ticket térmico generado exitosamente",
  "error": 0,
  "data": {
    "saleId": 1,
    "content": "*** TICKET DE VENTA ***\n      EXPANDA ERP      \n...",
    "width": 48,
    "format": "text/plain"
  }
}
```

**Ejemplo de Uso (JavaScript):**
```javascript
const response = await fetch(`http://localhost:7254/api/sales/1/ticket/thermal?width=48`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const result = await response.json();
console.log(result.data.content); // Imprimir contenido del ticket
```

---

### **2. Ticket Térmico (Formato Binario ESC/POS)**

**Descripción:** Genera un ticket de venta en formato binario con comandos ESC/POS completos, listo para enviar directamente a una impresora térmica.

```http
GET /api/sales/{id}/ticket/thermal/binary?width=48
Authorization: Bearer {token}
```

**Parámetros:**
| Parámetro | Tipo | Descripción | Requerido | Default |
|-----------|------|-------------|-----------|---------|
| `id` | int | ID de la venta | Sí | - |
| `width` | int | Ancho del papel (48=80mm, 32=58mm) | No | 48 |

**Respuesta:**
- **Content-Type:** `application/octet-stream`
- **Archivo:** `ticket-{id}.bin`

**Características del Formato Binario:**
- ? Comandos ESC/POS completos
- ? Inicialización de impresora
- ? Texto en negrita para encabezados
- ? Alineación automática (centro/izquierda)
- ? Avance de papel (6 líneas)
- ? Corte automático de papel
- ? Apertura de cajón de efectivo (si está conectado)

**Ejemplo de Uso (JavaScript para imprimir):**
```javascript
const response = await fetch(`http://localhost:7254/api/sales/1/ticket/thermal/binary`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const blob = await response.blob();
const url = URL.createObjectURL(blob);

// Opción 1: Descargar archivo
const a = document.createElement('a');
a.href = url;
a.download = 'ticket.bin';
a.click();

// Opción 2: Enviar a impresora (usando plugin o extensión del navegador)
```

---

### **3. Ticket PDF**

**Descripción:** Genera un ticket de venta en formato PDF profesional, ideal para impresoras láser, envío por email, o compartir digitalmente.

```http
GET /api/sales/{id}/ticket/pdf?includeLogo=true
Authorization: Bearer {token}
```

**Parámetros:**
| Parámetro | Tipo | Descripción | Requerido | Default |
|-----------|------|-------------|-----------|---------|
| `id` | int | ID de la venta | Sí | - |
| `includeLogo` | bool | Incluir logo de la empresa | No | true |

**Respuesta:**
- **Content-Type:** `application/pdf`
- **Archivo:** `ticket-{id}.pdf`

**Características del PDF:**
- ? Formato profesional tamaño carta (A4)
- ? Encabezado con logo y datos de la empresa
- ? Información detallada de la venta (folio, fecha, almacén, vendedor)
- ? Tabla de productos con cantidades, precios y descuentos
- ? Cálculo automático de totales (subtotal, descuentos, IVA, total)
- ? Formas de pago detalladas
- ? Pie de página con agradecimiento y fecha de generación
- ? Colores corporativos (azul: #1E40AF)

**Ejemplo de Uso (JavaScript):**
```javascript
const response = await fetch(`http://localhost:7254/api/sales/1/ticket/pdf`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const blob = await response.blob();
const url = URL.createObjectURL(blob);

// Abrir en nueva ventana
window.open(url, '_blank');

// O descargar
const a = document.createElement('a');
a.href = url;
a.download = 'ticket-1.pdf';
a.click();
```

---

### **4. Factura PDF**

**Descripción:** Genera una factura fiscal en formato PDF, con formato completo para timbrado CFDI.

```http
GET /api/sales/{id}/invoice/pdf
Authorization: Bearer {token}
```

**Parámetros:**
| Parámetro | Tipo | Descripción | Requerido | Default |
|-----------|------|-------------|-----------|---------|
| `id` | int | ID de la venta | Sí | - |

**Respuesta:**
- **Content-Type:** `application/pdf`
- **Archivo:** `factura-{id}.pdf`

**Validaciones:**
- ? La venta debe tener `RequiresInvoice = true`
- ? Si no requiere factura, retorna `HTTP 400` (Bad Request)

**Características del PDF de Factura:**
- ? Encabezado fiscal completo
- ? RFC de la empresa
- ? Régimen fiscal
- ? Datos del cliente (RFC, nombre, dirección)
- ? Desglose de productos con IVA
- ? UUID del CFDI (si está timbrado)
- ? Sello digital del SAT
- ? Código QR (si está timbrado)

**Ejemplo de Uso:**
```javascript
const response = await fetch(`http://localhost:7254/api/sales/1/invoice/pdf`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

if (response.ok) {
  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  window.open(url, '_blank');
} else {
  const error = await response.json();
  console.error(error.message); // "Esta venta no requiere factura"
}
```

---

## ?? **Comparación de Formatos**

| Característica | Térmico Texto | Térmico Binario | PDF Ticket | PDF Factura |
|----------------|---------------|-----------------|------------|-------------|
| **Tamaño** | Pequeño (~2 KB) | Pequeño (~3 KB) | Medio (~50 KB) | Grande (~100 KB) |
| **Impresora** | Térmica 58/80mm | Térmica 58/80mm | Láser/Inkjet | Láser profesional |
| **Corte automático** | ? | ? | N/A | N/A |
| **Apertura cajón** | ? | ? | N/A | N/A |
| **Logo empresa** | ? | ? | ? | ? |
| **Colores** | ? | ? | ? | ? |
| **Compartir digital** | ? (texto) | ? | ? | ? |
| **Enviar por email** | ? (texto) | ? | ? | ? |
| **Valor fiscal** | ? | ? | ? | ? |
| **Velocidad** | Muy rápida | Muy rápida | Rápida | Media |

---

## ?? **Casos de Uso**

### **Caso 1: Punto de Venta con Impresora Térmica**
```
1. Cliente compra productos
2. Se procesa el pago
3. Frontend llama: GET /api/sales/1/ticket/thermal/binary
4. Se descarga archivo .bin
5. Se envía directamente a la impresora térmica
6. Ticket impreso + cajón abierto ?
```

### **Caso 2: Envío de Ticket por WhatsApp**
```
1. Venta completada
2. Frontend llama: GET /api/sales/1/ticket/pdf
3. Se genera PDF
4. Se sube a servidor/cloud
5. Se envía link por WhatsApp ?
```

### **Caso 3: Ticket para Cliente sin Impresora**
```
1. Venta completada
2. Frontend llama: GET /api/sales/1/ticket/thermal
3. Se muestra el contenido en pantalla
4. Cliente toma captura de pantalla ?
```

### **Caso 4: Facturación Electrónica**
```
1. Venta con RequiresInvoice = true
2. Se timbra CFDI (UUID generado)
3. Frontend llama: GET /api/sales/1/invoice/pdf
4. Se genera factura con UUID
5. Se envía al cliente por email ?
```

---

## ?? **Integración con Frontend**

### **React/Vue/Angular Example:**

```typescript
// services/salesApi.ts
export const SalesApi = {
  // Obtener ticket térmico (texto)
  async getThermalTicket(saleId: number, width: number = 48): Promise<string> {
    const response = await fetch(
      `${API_URL}/api/sales/${saleId}/ticket/thermal?width=${width}`,
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`
        }
      }
    );
    
    const result = await response.json();
    return result.data.content;
  },

  // Descargar ticket térmico binario
  async downloadThermalTicketBinary(saleId: number): Promise<void> {
    const response = await fetch(
      `${API_URL}/api/sales/${saleId}/ticket/thermal/binary`,
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
    a.download = `ticket-${saleId}.bin`;
    a.click();
    
    URL.revokeObjectURL(url);
  },

  // Abrir ticket PDF en nueva ventana
  async openTicketPdf(saleId: number): Promise<void> {
    const response = await fetch(
      `${API_URL}/api/sales/${saleId}/ticket/pdf`,
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`
        }
      }
    );
    
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
  },

  // Descargar factura PDF
  async downloadInvoicePdf(saleId: number): Promise<void> {
    const response = await fetch(
      `${API_URL}/api/sales/${saleId}/invoice/pdf`,
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`
        }
      }
    );
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }
    
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = url;
    a.download = `factura-${saleId}.pdf`;
    a.click();
    
    URL.revokeObjectURL(url);
  }
};
```

### **Componente de Botones de Impresión:**

```vue
<template>
  <div class="print-buttons">
    <button @click="printThermal" class="btn btn-primary">
      ??? Imprimir Térmico
    </button>
    
    <button @click="downloadPdf" class="btn btn-secondary">
      ?? Descargar PDF
    </button>
    
    <button @click="downloadInvoice" class="btn btn-success" v-if="sale.requiresInvoice">
      ?? Descargar Factura
    </button>
  </div>
</template>

<script>
import { SalesApi } from '@/services/salesApi';

export default {
  props: ['sale'],
  methods: {
    async printThermal() {
      try {
        await SalesApi.downloadThermalTicketBinary(this.sale.id);
        this.$toast.success('Ticket descargado. Envíelo a la impresora térmica.');
      } catch (error) {
        this.$toast.error('Error al generar ticket: ' + error.message);
      }
    },
    
    async downloadPdf() {
      try {
        await SalesApi.openTicketPdf(this.sale.id);
      } catch (error) {
        this.$toast.error('Error al generar PDF: ' + error.message);
      }
    },
    
    async downloadInvoice() {
      try {
        await SalesApi.downloadInvoicePdf(this.sale.id);
        this.$toast.success('Factura descargada exitosamente');
      } catch (error) {
        this.$toast.error('Error: ' + error.message);
      }
    }
  }
};
</script>
```

---

## ?? **Dependencias Necesarias**

### **QuestPDF (para generación de PDFs):**

```bash
cd Infrastructure
dotnet add package QuestPDF
```

**? Ya está instalado** si usaste el servicio `PurchaseDocumentService` anteriormente.

---

## ?? **Personalización**

### **Cambiar Logo de la Empresa:**

Editar `SaleDocumentService.cs`:

```csharp
private void ComposeHeader(IContainer container)
{
    container.Column(column =>
    {
        // Agregar logo
        column.Item().Image("path/to/logo.png").FitWidth();
        
        column.Item().AlignCenter().Text("TU EMPRESA")
            .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
        
        // ... resto del código
    });
}
```

### **Cambiar Ancho de Papel Térmico:**

```csharp
// Para papel de 58mm (32 caracteres):
GET /api/sales/1/ticket/thermal?width=32

// Para papel de 80mm (48 caracteres):
GET /api/sales/1/ticket/thermal?width=48
```

### **Agregar Pie de Página Personalizado:**

Editar `ThermalTicketService.cs`:

```csharp
// Pie de página
sb.AppendLine(new string('=', width));
sb.AppendLine(CenterText("¡GRACIAS POR SU COMPRA!", width));
sb.AppendLine(CenterText("TU EMPRESA S.A. DE C.V.", width)); // ? NUEVO
sb.AppendLine(CenterText("RFC: XAXX010101000", width)); // ? NUEVO
sb.AppendLine(CenterText("Tel: 555-1234-5678", width)); // ? NUEVO
sb.AppendLine(CenterText("www.tuempresa.com", width)); // ? NUEVO
```

---

## ? **Archivos Creados**

1. ? `Application/Abstractions/Documents/IThermalTicketService.cs`
2. ? `Application/Abstractions/Documents/ISaleDocumentService.cs`
3. ? `Infrastructure/Services/ThermalTicketService.cs`
4. ? `Infrastructure/Services/SaleDocumentService.cs`
5. ? `Web.Api/Controllers/Sales/SalesController.cs` (actualizado)
6. ? `Web.Api/Program.cs` (actualizado)

---

## ?? **Pruebas en Postman**

### **1. Ticket Térmico (Texto):**
```
GET http://localhost:7254/api/sales/1/ticket/thermal?width=48
Authorization: Bearer {token}
```

**? Respuesta esperada:** JSON con el contenido del ticket

### **2. Ticket Térmico (Binario):**
```
GET http://localhost:7254/api/sales/1/ticket/thermal/binary
Authorization: Bearer {token}
```

**? Respuesta esperada:** Descarga de archivo `ticket-1.bin`

### **3. Ticket PDF:**
```
GET http://localhost:7254/api/sales/1/ticket/pdf
Authorization: Bearer {token}
```

**? Respuesta esperada:** Descarga de archivo `ticket-1.pdf`

### **4. Factura PDF:**
```
GET http://localhost:7254/api/sales/1/invoice/pdf
Authorization: Bearer {token}
```

**? Respuesta esperada:** Descarga de archivo `factura-1.pdf` (si la venta requiere factura)

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

- ? **4 endpoints de tickets** implementados
- ? **Formato térmico** (texto y binario ESC/POS)
- ? **Formato PDF** (ticket y factura)
- ? **Compilación exitosa**
- ? **Listo para producción**

---

**?? SISTEMA DE TICKETS COMPLETAMENTE IMPLEMENTADO** ?

**Fecha:** 2026-03-11  
**Endpoints:** 4 nuevos endpoints de generación de tickets  
**Estado:** ? **LISTO PARA USAR**
