# Sistema de Turnos de Cajero (Corte de Caja) - Guía de Implementación Frontend

## 📋 Contexto

Se ha implementado un **sistema completo de gestión de turnos de cajero** (corte de caja) en el backend. Este sistema permite:

- ✅ Abrir turnos con fondo inicial
- ✅ Registrar ventas durante el turno
- ✅ Cerrar turnos con cálculo automático de efectivo esperado
- ✅ Detectar sobrantes/faltantes de efectivo
- ✅ Generar reportes detallados por turno
- ✅ Cancelar turnos con razón documentada
- ✅ Consultar histórico de turnos con filtros avanzados

## 🎯 Objetivo

Implementar la interfaz de usuario completa para que los cajeros gestionen sus turnos de forma intuitiva, consumiendo todos los endpoints del API que ya están funcionales.

---

## 📡 API Endpoints Disponibles

### **Base URL:** `http://localhost:5000/api/cashier-shifts`

### 1. Abrir Turno
```http
POST /api/cashier-shifts/open
Authorization: Bearer {token}
Content-Type: application/json

{
  "branchId": 1,
  "initialCash": 500.00,
  "openingNotes": "Turno matutino - billetes verificados"
}
```

**Respuesta:**
```json
{
  "message": "Turno TURNO-000001 abierto exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "TURNO-000001",
    "cashierId": 5,
    "cashierName": "Juan Pérez",
    "cashierCode": "USR-00005",
    "warehouseId": 1,
    "warehouseName": "Almacén Central",
    "branchId": 1,
    "branchName": "Sucursal Matriz",
    "companyId": 1,
    "companyName": "Mi Empresa S.A. de C.V.",
    "openingDate": "2026-03-31T14:30:00Z",
    "closingDate": null,
    "status": "Open",
    "initialCash": 500.00,
    "expectedCash": 500.00,
    "finalCash": null,
    "difference": null,
    "totalSales": 0,
    "cancelledSales": 0,
    "totalSalesAmount": 0.00,
    "cancelledSalesAmount": 0.00,
    "cashSales": 0.00,
    "cardSales": 0.00,
    "transferSales": 0.00,
    "otherSales": 0.00,
    "cashWithdrawals": 0.00,
    "cashDeposits": 0.00,
    "openingNotes": "Turno matutino - billetes verificados",
    "closingNotes": null,
    "cancellationReason": null,
    "createdAt": "2026-03-31T14:30:00Z",
    "closedByUserId": null,
    "closedByName": null,
    "cancelledByUserId": null,
    "cancelledByName": null,
    "cancelledAt": null,
    "duration": "02:15:30"
  }
}
```

**Posibles Errores:**
- `400 Bad Request`: "Ya existe un turno activo para este cajero en la sucursal 'Sucursal Matriz'. Debes cerrar el turno actual antes de abrir uno nuevo."
- `400 Bad Request`: "No se encontró ningún almacén activo en la sucursal 'X'. Contacta al administrador para configurar un almacén."
- `404 Not Found`: "Sucursal con ID X no encontrada"
- `401 Unauthorized`: "Usuario no autenticado"

---

### 2. Cerrar Turno
```http
PUT /api/cashier-shifts/{shiftId}/close
Authorization: Bearer {token}
Content-Type: application/json

{
  "finalCash": 3850.50,
  "closingNotes": "Turno cerrado sin novedades",
  "cashBreakdown": {
    "coins001": 0,
    "coins005": 4,
    "coins01": 5,
    "coins02": 10,
    "coins05": 8,
    "coins10": 15,
    "bills20": 20,
    "bills50": 30,
    "bills100": 15,
    "bills200": 3,
    "bills500": 1,
    "bills1000": 0
  }
}
```

**Respuesta:**
```json
{
  "message": "Turno TURNO-000001 cerrado exitosamente. Sobrante de $5.50",
  "error": 0,
  "data": {
    "id": 1,
    "code": "TURNO-000001",
    "status": "Closed",
    "closingDate": "2026-03-31T20:00:00Z",
    "expectedCash": 3845.00,
    "finalCash": 3850.50,
    "difference": 5.50,
    "totalSales": 25,
    "totalSalesAmount": 8450.00,
    "cashSales": 3345.00,
    "cardSales": 4200.00,
    "transferSales": 905.00,
    "duration": "05:30:00",
    ...
  }
}
```

**Mensajes de Resultado:**
- Diferencia = 0: "Cuadrado"
- Diferencia > 0: "Sobrante de $X.XX"
- Diferencia < 0: "Faltante de $X.XX"

**Posibles Errores:**
- `404 Not Found`: "Turno con ID X no encontrado"
- `400 Bad Request`: "El turno TURNO-000001 no puede cerrarse porque su estado es 'Closed'. Solo se pueden cerrar turnos con estado 'Open'."

---

### 3. Cancelar Turno
```http
PUT /api/cashier-shifts/{shiftId}/cancel
Authorization: Bearer {token}
Content-Type: application/json

{
  "reason": "Turno cancelado por error en apertura - se abrió almacén incorrecto"
}
```

**Respuesta:**
```json
{
  "message": "Turno TURNO-000002 cancelado exitosamente",
  "error": 0,
  "data": {
    "id": 2,
    "status": "Cancelled",
    "cancellationReason": "Turno cancelado por error en apertura...",
    "cancelledByUserId": 5,
    "cancelledByName": "Juan Pérez",
    "cancelledAt": "2026-03-31T14:35:00Z",
    ...
  }
}
```

**Validaciones:**
- Campo `reason` requerido (no puede ser vacío)
- Solo turnos con estado "Open" pueden cancelarse

---

### 4. Obtener Turno Activo
```httpbranch
GET /api/cashier-shifts/active?warehouseId=1
Authorization: Bearer {token}
```

**Respuesta (si hay turno activo):**
```json
{
  "message": "Turno activo obtenido exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "TURNO-000001",
    "status": "Open",
    "duration": "02:15:30",
    ...
  }
}
```

**Respuesta (sin turno activo):**
```json
{
  "message": "No hay turno activo",
  "error": 0,
  "data": null
}
```

**Uso:** Consultar al iniciar sesión o al entrar al módulo de ventas para saber si el cajero tiene un turno abierto.

---

### 5. Obtener Turno por ID
```http
GET /api/cashier-shifts/{shiftId}
Authorization: Bearer {token}
```

**Respuesta:** Objeto completo del turno con todos los campos.

---

### 6. Listar Turnos (Paginado con Filtros)
```http
GET /api/cashier-shifts?page=1&pageSize=10&cashierId=5&warehouseId=1&status=Closed&fromDate=2026-03-01&toDate=2026-03-31
Authorization: Bearer {token}
```

**Parámetros Query (todos opcionales):**
- `page` (default: 1)
- `pageSize` (default: 10)
- `cashierId` - Filtrar por cajero específico
- `warehouseId` - Filtrar por almacén
- `branchId` - Filtrar por sucursal
- `companyId` - Filtrar por empresa
- `status` - Filtrar por estado: "Open", "Closed", "Cancelled"
- `fromDate` - Fecha inicial (ISO 8601)
- `toDate` - Fecha final (ISO 8601)

**Respuesta:**
```json
{
  "message": "Turnos obtenidos exitosamente",
  "error": 0,
  "data": {
    "message": "Turnos obtenidos exitosamente",
    "error": 0,
    "data": [
      { /* turno 1 */ },
      { /* turno 2 */ }
    ],
    "totalRecords": 45,
    "page": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

---

### 7. Obtener Reporte Detallado
```http
GET /api/cashier-shifts/{shiftId}/report
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Reporte de turno generado exitosamente",
  "error": 0,
  "data": {
    "shift": { /* datos del turno */ },
    "paymentMethodSummary": [
      {
        "paymentMethod": "Efectivo",
        "count": 15,
        "amount": 3345.00,
        "percentage": 39.6
      },
      {
        "paymentMethod": "Tarjeta",
        "count": 8,
        "amount": 4200.00,
        "percentage": 49.7
      },
      {
        "paymentMethod": "Transferencia",
        "count": 2,
        "amount": 905.00,
        "percentage": 10.7
      }
    ],
    "cashFlow": {
      "initialCash": 500.00,
      "cashSalesIn": 3345.00,
      "cashDepositsIn": 0.00,
      "cashWithdrawalsOut": 0.00,
      "expectedCash": 3845.00,
      "finalCash": 3850.50,
      "difference": 5.50,
      "differenceStatus": "Sobrante"
    },
    "sales": [
      {
        "id": 123,
        "code": "VTA-000123",
        "saleDate": "2026-03-31T15:30:00Z",
        "customerName": "Público General",
        "total": 450.00,
        "status": "Completed",
        "paymentMethods": ["Efectivo"]
      },
      // ... más ventas
    ]
  }
}
```

**Estados de Diferencia:**
- `"Exacto"` - Sin diferencia (difference = 0)
- `"Sobrante"` - Hay más efectivo del esperado (difference > 0)
- `"Faltante"` - Falta efectivo (difference < 0)

---

### 8. Exportar Corte de Caja en PDF
```http
GET /api/cashier-shifts/{shiftId}/export-pdf
Authorization: Bearer {token}
```

**Descripción:**
Este endpoint genera y descarga un PDF estructurado con todos los detalles del corte de caja. El documento incluye:
- Información del turno (código, cajero, sucursal, fechas)
- Resumen de ventas (totales, cancelaciones, montos)
- Desglose por forma de pago (efectivo, tarjeta, transferencia, otros)
- Flujo de efectivo (inicial, entradas, salidas, esperado, final, diferencia)
- Notas de apertura y cierre
- Espacio para firma del cajero

**Respuesta:**
- `Content-Type: application/pdf`
- `Content-Disposition: attachment; filename="Corte-Caja-{shiftId}-{timestamp}.pdf"`
- Retorna el archivo PDF directamente como descarga

**Ejemplo de nombre de archivo:**
```
Corte-Caja-1-20260331-143000.pdf
```

**Uso en Frontend (JavaScript/TypeScript):**
```typescript
async function downloadShiftPdf(shiftId: number) {
  try {
    const response = await fetch(
      `${API_BASE_URL}/cashier-shifts/${shiftId}/export-pdf`,
      {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );

    if (!response.ok) {
      throw new Error('Error al generar PDF');
    }

    // Crear blob del PDF
    const blob = await response.blob();
    
    // Obtener nombre del archivo del header
    const contentDisposition = response.headers.get('Content-Disposition');
    const fileName = contentDisposition
      ? contentDisposition.split('filename=')[1].replace(/"/g, '')
      : `Corte-Caja-${shiftId}.pdf`;

    // Descargar automáticamente
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);

    console.log('✅ PDF descargado exitosamente');
  } catch (error) {
    console.error('❌ Error al descargar PDF:', error);
    alert('Error al generar el PDF del corte de caja');
  }
}
```

**Uso en React:**
```tsx
const handleExportPdf = async (shiftId: number) => {
  setLoading(true);
  try {
    const response = await axios.get(
      `/api/cashier-shifts/${shiftId}/export-pdf`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        },
        responseType: 'blob' // Importante para archivos binarios
      }
    );

    // Crear URL del blob y descargar
    const blob = new Blob([response.data], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `Corte-Caja-${shiftId}-${Date.now()}.pdf`;
    link.click();
    window.URL.revokeObjectURL(url);

    toast.success('PDF descargado exitosamente');
  } catch (error) {
    console.error('Error al exportar PDF:', error);
    toast.error('Error al generar el PDF del corte de caja');
  } finally {
    setLoading(false);
  }
};
```

**Botón en UI:**
```jsx
<Button
  variant="outline"
  icon={<DownloadIcon />}
  onClick={() => downloadShiftPdf(shift.id)}
  disabled={loading}
>
  {loading ? 'Generando PDF...' : 'Descargar PDF'}
</Button>
```

**Posibles Errores:**
- `404 Not Found`: "Turno con ID X no encontrado"
- `500 Internal Server Error`: Error al generar el PDF (problema en el servidor)

**Recomendación UX:**
- Mostrar este botón en:
  - **Modal de cierre de turno**: Después de cerrar exitosamente, ofrecer descarga inmediata
  - **Tabla de histórico**: Botón de acción por cada turno cerrado
  - **Vista de detalle de turno**: En la sección de acciones/herramientas
- Mostrar loading spinner mientras se genera el PDF
- Usar iconos de descarga (📥 o download icon) para identificar la acción visualmente

---

## 🎨 Pantallas/Componentes a Implementar

### **1. Dashboard de Turno Activo** (Pantalla Principal)

**Ubicación:** Mostrar al entrar a módulo de Ventas o POS

**Mostrar si hay turno activo:**
- Badge/Tarjeta destacada:
  ```
  ✅ Turno Activo: TURNO-000001
  Sucursal: Sucursal Matriz
  Almacén: Almacén Central
  Apertura: 31/03/2026 14:30
  Duración: 2h 15m
  Fondo inicial: $500.00
  ```
- Botón: "Cerrar Turno"
- Botón secundario: "Cancelar Turno"

**Mostrar si NO hay turno activo:**
- Mensaje: "No tienes un turno activo"
- Botón destacado: "Abrir Turno de Caja"

**Lógica:**
```javascript
// Al cargar componente
onMounted(async () => {
  const activeShift = await getActiveShift();
  if (activeShift.data) {
    // Mostrar turno activo
    currentShift.value = activeShift.data;
  } else {
    // Mostrar botón de abrir
    showOpenShiftButton.value = true;
  }
});
```

---

### **2. Modal: Abrir Turno**

**Campos del Formulario:**

1. **Sucursal** (select, requerido)
   - Obtener lista de sucursales del usuario
   - Endpoint: `GET /api/branches`
   - Validación: Requerido
   - Nota: El sistema seleccionará automáticamente el almacén principal de la sucursal

2. **Fondo Inicial** (number, requerido)
   - Input type="number" step="0.01"
   - Placeholder: "Ej: 500.00"
   - Validación: 
     - Requerido
     - Debe ser >= 0
     - Máximo 2 decimales

3. **Notas de Apertura** (textarea, opcional)
   - Placeholder: "Ej: Billetes verificados, sin monedas de 10 pesos"
   - MaxLength: 1000 caracteres
   - Opcional

**Botones:**
- "Cancelar" (cerrar modal)
- "Abrir Turno" (primario, enviar)

**Validaciones Frontend:**
```javascript
cobranchId: [
    { required: true, message: 'Selecciona una sucursal
    { required: true, message: 'Selecciona un almacén' }
  ],
  initialCash: [
    { required: true, message: 'El fondo inicial es requerido' },
    { type: 'number', min: 0, message: 'Debe ser mayor o igual a 0' },
    { validator: (rule, value) => {
      const decimals = (value.toString().split('.')[1] || '').length;
      if (decimals > 2) {
        return Promise.reject('Máximo 2 decimales');
      }
      return Promise.resolve();
    }}
  ],
  openingNotes: [
    { max: 1000, message: 'Máximo 1000 caracteres' }
  ]
};
```

**Flujo tras éxito:**
1. Mostrar notificación: "Turno TURNO-000001 abierto exitosamente"
2. Cerrar modal
3. Actualizar estado del turno activo
4. Redirigir/habilitar módulo de ventas

---

### **3. Modal: Cerrar Turno**

**Información a Mostrar (solo lectura):**
- Código del turno: TURNO-000001
- Fecha/hora apertura
- Duración del turno (calculada)
- Fondo inicial: $500.00
- Ventas completadas: 25
- Monto total vendido: $8,450.00
- **Efectivo esperado: $3,845.00** (destacado)

**Desglose de Ventas por Método de Pago:**
```
Efectivo:       $3,345.00 (15 ventas)
Tarjeta:        $4,200.00 (8 ventas)
Transferencia:  $905.00   (2 ventas)
```

**Campos del Formulario:**

1. **Efectivo Final** (number, requerido)
   - Input destacado, grande
   - Placeholder: "Ej: 3850.50"
   - Validación:
     - Requerido
     - Debe ser >= 0
     - Máximo 2 decimales
   - **Mostrar diferencia en tiempo real:**
     ```
     Diferencia: $5.50 (Sobrante) ✅
     Diferencia: -$10.00 (Faltante) ❌
     Diferencia: $0.00 (Cuadrado) ✅
     ```

2. **Desglose de Billetes y Monedas** (opcional, expandible)
   - Tabla con denominaciones:
     ```
     Monedas:
     [ ] x $0.10  = $0.00
     [ ] x $0.50  = $0.00
     [ ] x $1.00  = $0.00
     [ ] x $2.00  = $0.00
     [ ] x $5.00  = $0.00
     [ ] x $10.00 = $0.00
     
     Billetes:
     [ ] x $20    = $0.00
     [ ] x $50    = $0.00
     [ ] x $100   = $0.00
     [ ] x $200   = $0.00
     [ ] x $500   = $0.00
     [ ] x $1000  = $0.00
     
     TOTAL CALCULADO: $X,XXX.XX
     ```
   - Al completar, calcular total automáticamente
   - Botón: "Aplicar al Efectivo Final"

3. **Notas de Cierre** (textarea, opcional)
   - Placeholder: "Ej: Todo en orden, sin novedades"
   - MaxLength: 1000 caracteres

**Validaciones:**
```javascript
const closeShiftRules = {
  finalCash: [
    { required: true, message: 'El efectivo final es requerido' },
    { type: 'number', min: 0, message: 'Debe ser mayor o igual a 0' }
  ],
  closingNotes: [
    { max: 1000, message: 'Máximo 1000 caracteres' }
  ]
};
```

**Alertas según Diferencia:**
- Diferencia > $50 o < -$50: Mostrar warning "La diferencia es significativa. ¿Estás seguro?"
- Diferencia = 0: Mostrar mensaje positivo "¡Excelente! El corte está cuadrado."

**Botones:**
- "Cancelar"
- "Cerrar Turno" (primario, color según diferencia: verde si cuadrado, amarillo si hay diferencia)

**Flujo tras éxito:**
1. Mostrar notificación con resultado: "Turno cerrado. Sobrante de $5.50"
2. **IMPORTANTE**: Ofrecer descarga inmediata del PDF del corte de caja:
   ```jsx
   // Modal de éxito después de cerrar
   <SuccessModal
     title="✅ Turno cerrado exitosamente"
     message={`Turno ${shiftCode} cerrado. ${differenceStatus}: $${Math.abs(difference)}`}
   >
     <div className="actions">
       <Button variant="primary" onClick={() => downloadShiftPdf(shiftId)}>
         📥 Descargar PDF del Corte
       </Button>
       <Button variant="secondary" onClick={() => viewShiftReport(shiftId)}>
         👁️ Ver Reporte Detallado
       </Button>
     </div>
   </SuccessModal>
   ```
3. Cerrar modal
4. Limpiar estado de turno activo
5. Actualizar lista/histórico de turnos

---

### **4. Modal: Cancelar Turno**

**Información a Mostrar:**
- Código del turno
- Fecha/hora apertura
- Almacén

**Advertencia destacada:**
```
⚠️ ATENCIÓN: Esta acción es irreversible.
Al cancelar el turno, no podrás cerrarlo ni generar reportes.
Las ventas realizadas permanecerán en el sistema pero sin asociación al turno.
```

**Campo del Formulario:**

1. **Razón de Cancelación** (textarea, requerido)
   - Placeholder: "Ej: Se abrió el almacén incorrecto"
   - Validación:
     - Requerido
     - MinLength: 10 caracteres
     - MaxLength: 500 caracteres

**Validaciones:**
```javascript
const cancelShiftRules = {
  reason: [
    { required: true, message: 'Debes proporcionar una razón' },
    { min: 10, message: 'Mínimo 10 caracteres' },
    { max: 500, message: 'Máximo 500 caracteres' }
  ]
};
```

**Botones:**
- "No, regresar" (secundario)
- "Sí, cancelar turno" (peligro/destructivo, color rojo)

**Confirmación adicional:**
- Requerir confirmación con checkbox: "Confirmo que deseo cancelar este turno"

**Flujo tras éxito:**
1. Mostrar notificación: "Turno cancelado exitosamente"
2. Cerrar modal
3. Limpiar estado de turno activo

---

### **5. Pantalla: Historial de Turnos**

**Ubicación:** Menú Reportes → Turnos de Caja

**Filtros Disponibles:**

Formulario de filtros (colapsable):
```
[ Cajero          ▼ ]  [ Almacén      ▼ ]  [ Estado       ▼ ]
[ Sucursal        ▼ ]  [ Empresa      ▼ ]
[ Fecha Inicio: __/__/__ ]  [ Fecha Fin: __/__/__ ]
                               [🔍 Buscar] [🗑️ Limpiar]
```

**Tabla de Resultados:**

| Código | Cajero | Almacén | Apertura | Cierre | Duración | Ventas | Monto | Diferencia | Estado | Acciones |
|--------|--------|---------|----------|--------|----------|--------|-------|------------|--------|----------|
| TURNO-000045 | Juan P. | Almacén Centro | 31/03 14:30 | 31/03 20:00 | 5h 30m | 25 | $8,450 | +$5.50 ✅ | Cerrado | 👁️ 🖨️ |
| TURNO-000044 | María L. | Almacén Norte | 31/03 08:00 | 31/03 14:00 | 6h | 18 | $5,200 | -$10.00 ❌ | Cerrado | 👁️ 🖨️ |
| TURNO-000043 | Pedro R. | Almacén Sur | 31/03 06:00 | - | - | - | - | - | Cancelado | 👁️ |

**Colores Indicadores:**
- Diferencia = 0: Verde (✅)
- Diferencia positiva (sobrante): Amarillo (⚠️)
- Diferencia negativa (faltante): Rojo (❌)
- Estado Cancelado: Gris (🚫)

**Paginación:**
- Mostrar: [10 ▼] elementos por página
- Páginas: [« 1 2 3 ... 5 »]
- Total: "Mostrando 1-10 de 45 registros"

**Acciones por fila:**
- 👁️ **Ver Reporte Detallado** - Modal o página con reporte completo (endpoint: `/api/cashier-shifts/{id}/report`)
- 📥 **Descargar PDF** - Genera y descarga el corte de caja en PDF profesional (endpoint: `/api/cashier-shifts/{id}/export-pdf`)
  - Solo disponible para turnos **Cerrados**
  - Deshabilitado para turnos Activos o Cancelados

**Ejemplo de implementación de acciones:**
```tsx
<TableCell>
  <ButtonGroup>
    {/* Ver reporte - disponible para todos los estados */}
    <IconButton
      icon={<EyeIcon />}
      onClick={() => viewShiftReport(shift.id)}
      tooltip="Ver reporte detallado"
    />
    
    {/* Descargar PDF - solo para turnos cerrados */}
    <IconButton
      icon={<DownloadIcon />}
      onClick={() => downloadShiftPdf(shift.id)}
      disabled={shift.status !== 'Closed'}
      tooltip={
        shift.status === 'Closed' 
          ? 'Descargar PDF del corte de caja' 
          : 'Solo disponible para turnos cerrados'
      }
    />
  </ButtonGroup>
</TableCell>
```

---

### **6. Modal/Pantalla: Reporte Detallado de Turno**

**Secciones del Reporte:**

#### **A. Encabezado**
```
╔═══════════════════════════════════════════════════════════╗
║           REPORTE DE CORTE DE CAJA                        ║
║           Turno: TURNO-000045                             ║
╚═══════════════════════════════════════════════════════════╝

Cajero:     Juan Pérez (USR-00005)
Almacén:    Almacén Central
Sucursal:   Sucursal Matriz
Empresa:    Mi Empresa S.A. de C.V.

Apertura:   31/03/2026 14:30:00
Cierre:     31/03/2026 20:00:00
Duración:   5 horas 30 minutos
Estado:     Cerrado por: Juan Pérez
```

#### **B. Resumen de Efectivo**
```
┌─────────────────────────────────────────┐
│   FLUJO DE EFECTIVO                     │
├─────────────────────────────────────────┤
│ Fondo Inicial           │    $500.00    │
│ + Ventas en Efectivo    │  $3,345.00    │
│ + Depósitos             │      $0.00    │
│ - Retiros               │      $0.00    │
├─────────────────────────────────────────┤
│ = EFECTIVO ESPERADO     │  $3,845.00    │
│ EFECTIVO FINAL CONTADO  │  $3,850.50    │
├─────────────────────────────────────────┤
│ 📊 DIFERENCIA           │     +$5.50 ✅ │
│    Estado: SOBRANTE                     │
└─────────────────────────────────────────┘
```

#### **C. Resumen de Ventas**
```
┌─────────────────────────────────────────┐
│   RESUMEN DE VENTAS                     │
├─────────────────────────────────────────┤
│ Ventas Completadas      │      25       │
│ Monto Total             │  $8,450.00    │
│ Ventas Canceladas       │       2       │
│ Monto Cancelado         │    $380.00    │
└─────────────────────────────────────────┘
```

#### **D. Ventas por Método de Pago**
```
┌──────────────────────────────────────────────────────┐
│   DESGLOSE POR FORMA DE PAGO                         │
├──────────────────────┬───────┬──────────┬────────────┤
│ Método de Pago       │ Cant. │  Monto   │    %       │
├──────────────────────┼───────┼──────────┼────────────┤
│ 💵 Efectivo          │  15   │ $3,345   │   39.6%    │
│ 💳 Tarjeta           │   8   │ $4,200   │   49.7%    │
│ 🏦 Transferencia     │   2   │  $905    │   10.7%    │
│ 📱 Otros             │   0   │    $0    │    0.0%    │
├──────────────────────┼───────┼──────────┼────────────┤
│ TOTAL                │  25   │ $8,450   │  100.0%    │
└──────────────────────┴───────┴──────────┴────────────┘
```

#### **E. Detalle de Ventas** (Tabla expandible/colapsable)
```
┌────────────────────────────────────────────────────────────────┐
│   LISTADO DETALLADO DE VENTAS (25)                             │
├─────────┬──────────────┬─────────────────┬─────────┬──────────┤
│ Código  │   Fecha      │    Cliente      │  Pago   │  Total   │
├─────────┼──────────────┼─────────────────┼─────────┼──────────┤
│VTA-0123 │ 31/03 15:30  │ Público General │ Efectivo│  $450.00 │
│VTA-0124 │ 31/03 15:45  │ María López     │ Tarjeta │  $890.00 │
│VTA-0125 │ 31/03 16:00  │ Público General │ Efectivo│  $125.50 │
│   ...   │     ...      │      ...        │   ...   │   ...    │
└─────────┴──────────────┴─────────────────┴─────────┴──────────┘
```

#### **F. Notas**
```
═══════════════════════════════════════════════════════════
NOTAS DE APERTURA:
Turno matutino - billetes verificados

NOTAS DE CIERRE:
Todo en orden, sin novedades
═══════════════════════════════════════════════════════════
```

**Botones de Acción:**
- � **Descargar PDF** - Genera PDF del corte de caja con formato profesional (usa endpoint `/export-pdf`)
- 🖨️ **Imprimir** - Opción alternativa: imprimir el reporte desde el navegador (window.print())
- 📧 **Enviar por Email** (opcional) - Requiere implementación adicional en backend
- ❌ **Cerrar** - Cerrar modal/vista

**Implementación del botón de PDF:**
```tsx
<Button
  variant="primary"
  icon={<DownloadIcon />}
  onClick={() => downloadShiftPdf(shift.id)}
  disabled={loading}
>
  {loading ? 'Generando PDF...' : 'Descargar PDF'}
</Button>
```

---

## 🔐 Validaciones y Reglas de Negocio

### **Reglas Globales:**
sucursal**
   - Antes de abrir, verificar si existe turno activo
   - Si existe, mostrar error con opción de ir a cerrarlo
   - El sistema selecciona automáticamente el almacén principal de la sucursal
   - Si existe, mostrar error con opción de ir a cerrarlo

2. **Solo el mismo cajero puede cerrar/cancelar su turno**
   - Backend valida automáticamente con el token JWT
   - Frontend puede ocultar botones si el turno no pertenece al usuario actual

3. **Turnos solo pueden cerrarse/cancelarse si están en estado "Open"**
   - Deshabilitar botones si status ≠ "Open"
   - Mostrar mensaje: "Este turno ya fue cerrado/cancelado"

4. **Cálculo de diferencia en tiempo real:**
   ```javascript
   const calculateDifference = () => {
     const expected = parseFloat(expectedCash.value) || 0;
     const final = parseFloat(finalCash.value) || 0;
     difference.value = final - expected;
     
     if (difference.value > 0) {
       differenceStatus.value = 'Sobrante';
       statusColor.value = 'warning'; // amarillo
     } else if (difference.value < 0) {
       differenceStatus.value = 'Faltante';
       statusColor.value = 'danger'; // rojo
     } else {
       differenceStatus.value = 'Exacto';
       statusColor.value = 'success'; // verde
     }
   };
   ```

5. **Desglose de efectivo auto-calculable:**
   ```javascript
   const calculateCashBreakdown = () => {
     const total = 
       (breakdown.coins001 * 0.10) +
       (breakdown.coins005 * 0.50) +
       (breakdown.coins01 * 1) +
       (breakdown.coins02 * 2) +
       (breakdown.coins05 * 5) +
       (breakdown.coins10 * 10) +
       (breakdown.bills20 * 20) +
       (breakdown.bills50 * 50) +
       (breakdown.bills100 * 100) +
       (breakdown.bills200 * 200) +
       (breakdown.bills500 * 500) +
       (breakdown.bills1000 * 1000);
     
     breakdownTotal.value = total;
   };
   
   const applyBreakdownToFinalCash = () => {
     finalCash.value = breakdownTotal.value;
   };
   ```

6. **Formato de moneda consistente:**
   - Usar siempre formato: `$X,XXX.XX`
   - 2 decimales obligatorios
   - Separador de miles con coma

7. **Duración del turno (cálculo en frontend):**
   ```javascript
   const calculateDuration = (openingDate, closingDate) => {
     const start = new Date(openingDate);
     const end = closingDate ? new Date(closingDate) : new Date();
     const diff = end - start;
     
     const hours = Math.floor(diff / 3600000);
     const minutes = Math.floor((diff % 3600000) / 60000);
     
     return `${hours}h ${minutes}m`;
   };
   ```

8. **Actualización automática del turno activo:**
   - Usar polling cada 30 segundos o WebSocket
   - Actualizar estadísticas en tiempo real (ventas, monto)

---

## 🎨 Consideraciones de UX/UI

### **Estados Visuales:**

1. **Turno Activo (Dashboard):**
   - Badge verde "Activo" parpadeante
   - Mostrar duración actualizada cada minuto
   - Botón "Cerrar Turno" siempre visible y accesible

2. **Diferencias en Cierre:**
   - **Cuadrado (0):** Badge verde ✅ "Perfecto"
   - **Sobrante (>0):** Badge amarillo ⚠️ "Sobrante: $X.XX"
   - **Faltante (<0):** Badge rojo ❌ "Faltante: $X.XX"

3. **Estados de Turno en Historial:**
   - **Open:** Badge azul con animación
   - **Closed:** Badge verde
   - **Cancelled:** Badge gris

### **Notificaciones:**

- **Turno abierto:** Toast verde, 3 segundos
- **Turno cerrado:** Toast con resultado (color según diferencia), 5 segundos
- **Turno cancelado:** Toast naranja, 3 segundos
- **Error al abrir (turno duplicado):** Toast rojo con botón "Ir a Cerrar", 0 timeout (manual)

### **Responsive:**

- En móvil:
  - Desglose de billetes en formato vertical
  - Tabla de ventas con scroll horizontal
  - Filtros colapsables por defecto

### **Accesibilidad:**

- Labels claros en todos los inputs
- Colores con suficiente contraste
- Botones con indicadores de estado (loading, disabled)

---

## 📊 Integración con Módulo de Ventas

### **Al Crear una Venta:**

1. **Verificar turno activo antes de permitir venta:**
   ```javascript
   const canCreateSale = async () => {
     const activeShift = await getActiveShift();
     
     if (!activeShift.data) {
       showAlert({
         type: 'warning',
         title: 'Sin turno activo',
         message: 'Debes abrir un turno de caja antes de realizar ventas',
         actions: [
           { text: 'Abrir Turno', handler: () => openShiftModal() },
           { text: 'Cancelar' }
         ]
       });
       return false;
     }
     
     return true;
   };
   ```

2. **Mostrar información del turno en POS:**
   - Header del POS: "Turno: TURNO-000001 | Duración: 2h 15m"

3. **Actualizar estadísticas tras cada venta:**
   - Refrescar datos del turno activo
   - Mostrar contador de ventas del turno

---

## 🧪 Casos de Prueba

### **Test 1: Flujo Completo Exitoso**
1. Usuario sin turno activo → Mostrar botón "Abrir Turno"
2. Click en "Abrir Turno" → Modal con formulario
3. Llenar: Almacén = Central, Fondo = $500, Notas = "Test"
4. Submit → Turno creado (TURNO-XXXXXX)
5. Realizar 3 ventas (2 efectivo, 1 tarjeta)
6. Click "Cerrar Turno" → Modal muestra efectivo esperado
7. Ingresar efectivo final = esperado → Diferencia = $0.00 ✅
8. Submit → Turno cerrado, mensaje "Cuadrado"
9. Ver reporte → Datos correctos

### **Test 2: Intento de Abrir Turno Duplicado**
1. Usuario con turno activo en la misma sucursal
3. API retorna error 400
4. Mostrar notificación: "Ya existe un turno activo en la sucursal
4. Mostrar notificación: "Ya existe un turno activo..."
5. Botón "Ir a Cerrar Turno Actual"

### **Test 3: Cierre con Faltante**
1. Turno con ventas efectivo = $1000
2. Efectivo esperado = $1500
3. Ingresar final = $1480
4. Diferencia = -$20.00 ❌
5. Mostrar warning: "Faltante significativo"
6. Confirmar cierre → Mensaje "Faltante de $20.00"

### **Test 4: Cancelación de Turno**
1. Turno abierto sin ventas
2. Click "Cancelar Turno"
3. Modal con advertencia
4. Ingresar razón (mínimo 10 chars)
5. Confirmar → Turno cancelado (status = "Cancelled")

### **Test 5: Desglose de Billetes**
1. En modal de cierre, expandir "Desglose de Efectivo"
2. Ingresar cantidades: 5x$100 + 10x$50 = $1000
3. Total auto-calculado = $1000
4. Click "Aplicar" → Campo "Efectivo Final" = $1000

---

## 🚀 Orden Sugerido de Implementación

### **Fase 1: Básico Funcional** (MVP)
1. ✅ Servicio API (axios/fetch con endpoints)
2. ✅ Hook/Composable para gestión de turnos
3. ✅ Modal "Abrir Turno"
4. ✅ Indicador de turno activo en dashboard
5. ✅ Modal "Cerrar Turno" (sin desglose de billetes)

### **Fase 2: Mejoras** 
6. ✅ Desglose de billetes y monedas
7. ✅ Modal "Cancelar Turno"
8. ✅ Validaciones completas

### **Fase 3: Reportes**
9. ✅ Pantalla "Historial de Turnos" con filtros
10. ✅ Modal/Página "Reporte Detallado"

### **Fase 4: Pulido**
11. ✅ Exportar PDF
12. ✅ Actualización en tiempo real
13. ✅ Responsive mobile
14. ✅ Tests E2E

---

## 📝 Código de Ejemplo (Vue 3 + TypeScript)

### **Servicio API:**
```typescript
// services/cashierShiftService.ts
import axios from 'axios';

const API_URL = 'http://localhost:5000/api/cashier-shifts';
branch
export interface OpenShiftRequest {
  warehouseId: number;
  initialCash: number;
  openingNotes?: string;
}

export interface CloseShiftRequest {
  finalCash: number;
  closingNotes?: string;
  cashBreakdown?: CashBreakdown;
}

export interface CashBreakdown {
  coins001: number;
  coins005: number;
  coins01: number;
  coins02: number;
  coins05: number;
  coins10: number;
  bills20: number;
  bills50: number;
  bills100: number;
  bills200: number;
  bills500: number;
  bills1000: number;
}

export const cashierShiftService = {
  async openShift(data: OpenShiftRequest) {
    const response = await axios.post(`${API_URL}/open`, data);
    return response.data;
  },

  async closeShift(shiftId: number, data: CloseShiftRequest) {
    const response = await axios.put(`${API_URL}/${shiftId}/close`, data);
    return response.data;
  },

  async cancelShift(shiftId: number, reason: string) {
    const response = await axios.put(`${API_URL}/${shiftId}/cancel`, { reason });
    return response.data;
  },branchId?: number) {
    const params = branchId ? { branch
  async getActiveShift(warehouseId?: number) {
    const params = warehouseId ? { warehouseId } : {};
    const response = await axios.get(`${API_URL}/active`, { params });
    return response.data;
  },

  async getShiftById(shiftId: number) {
    const response = await axios.get(`${API_URL}/${shiftId}`);
    return response.data;
  },

  async getShifts(params: any) {
    const response = await axios.get(API_URL, { params });
    return response.data;
  },

  async getShiftReport(shiftId: number) {
    const response = await axios.get(`${API_URL}/${shiftId}/report`);
    return response.data;
  }
};
```

### **Composable:**
```typescript
// composables/useCashierShift.ts
import { ref, computed } from 'vue';
import { cashierShiftService } from '@/services/cashierShiftService';

export function useCashierShift() {
  const activeShift = ref(null);
  const loading = ref(false);
  const error = ref(null);

  const hasActiveShift = computed(() => !!activeShift.value);
  
  const isShiftOpen = computed(() => 
    activeShift.value?.status === 'Open'
  );

  const loadActiveShift = async () => {
    loading.value = true;
    error.value = null;
    
    try {
      const response = await cashierShiftService.getActiveShift();
      activeShift.value = response.data;
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al cargar turno';
    } finally {
      loading.value = false;
    }
  };

  const openShift = async (data: OpenShiftRequest) => {
    loading.value = true;
    error.value = null;

    try {
      const response = await cashierShiftService.openShift(data);
      activeShift.value = response.data;
      return response;
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al abrir turno';
      throw err;
    } finally {
      loading.value = false;
    }
  };

  const closeShift = async (shiftId: number, data: CloseShiftRequest) => {
    loading.value = true;
    error.value = null;

    try {
      const response = await cashierShiftService.closeShift(shiftId, data);
      activeShift.value = null; // Limpiar turno activo
      return response;
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al cerrar turno';
      throw err;
    } finally {
      loading.value = false;
    }
  };

  const cancelShift = async (shiftId: number, reason: string) => {
    loading.value = true;
    error.value = null;

    try {
      const response = await cashierShiftService.cancelShift(shiftId, reason);
      activeShift.value = null;
      return response;
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al cancelar turno';
      throw err;
    } finally {
      loading.value = false;
    }
  };

  const calculateDifference = (expectedCash: number, finalCash: number) => {
    const diff = finalCash - expectedCash;
    return {
      value: diff,
      status: diff > 0 ? 'Sobrante' : diff < 0 ? 'Faltante' : 'Exacto',
      color: diff > 0 ? 'warning' : diff < 0 ? 'danger' : 'success'
    };
  };

  return {
    activeShift,
    loading,
    error,
    hasActiveShift,
    isShiftOpen,
    loadActiveShift,
    openShift,
    closeShift,
    cancelShift,
    calculateDifference
  };
}
```

---

## ✅ Checklist de Implementación

### **Backend (Ya Implementado)** ✅
- [x] 8 Endpoints funcionando (incluye export-pdf)
- [x] Validaciones de negocio
- [x] Base de datos migrada
- [x] Cálculo automático de efectivo esperado
- [x] Generación de PDF profesional con QuestPDF
- [x] Permisos configurados

### **Frontend (Por Implementar)**
- [ ] Servicio API con todos los endpoints
  - [ ] POST /open
  - [ ] PUT /{id}/close
  - [ ] PUT /{id}/cancel
  - [ ] GET /{id}
  - [ ] GET /active
  - [ ] GET / (paginado)
  - [ ] GET /{id}/report
  - [ ] **GET /{id}/export-pdf** (nuevo)
- [ ] Composable/Hook de gestión de turnos
- [ ] Modal: Abrir Turno
- [ ] Modal: Cerrar Turno
  - [ ] Cálculo de diferencia en tiempo real
  - [ ] Desglose de billetes y monedas
  - [ ] **Botón de descarga PDF en modal de éxito**
- [ ] Modal: Cancelar Turno
- [ ] Indicador de turno activo en Dashboard/POS
- [ ] Pantalla: Historial de Turnos
  - [ ] Filtros avanzados
  - [ ] Paginación
  - [ ] **Botón de descarga PDF por turno cerrado**
- [ ] Modal/Página: Reporte Detallado
  - [ ] Todas las secciones
  - [ ] **Botón de exportar PDF**
- [ ] Validaciones frontend completas
- [ ] Manejo de errores
- [ ] Notificaciones toast
- [ ] Responsive design
- [ ] Tests unitarios
- [ ] Tests E2E

---

## 🎯 Resultado Esperado

Al finalizar la implementación, el cajero podrá:

1. ✅ **Abrir su turno** al iniciar la jornada con el fondo de caja
2. ✅ **Registrar ventas** durante todo el turno (ya implementado en sistema de ventas)
3. ✅ **Cerrar su turno** al final del día con conteo de efectivo
4. ✅ **Ver instantáneamente** si hay sobrante o faltante
5. ✅ **Generar reportes** detallados con todas las transacciones
6. ✅ **Descargar PDF profesional** del corte de caja con formato de impresión
7. ✅ **Consultar histórico** de turnos anteriores con filtros
8. ✅ **Cancelar turnos** en caso de error con documentación

El sistema proporcionará **control total** sobre el flujo de efectivo, **auditoría completa** de operaciones, **reportes detallados** para análisis financiero y **documentos PDF** listos para imprimir y archivar.

---

## 📞 Soporte

Si encuentras algún error en los endpoints o necesitas modificaciones en el backend, documenta:
1. Endpoint afectado
2. Request enviado (JSON)
3. Response recibido
4. Comportamiento esperado

El backend está completamente funcional y probado. La implementación frontend debe consumir estos endpoints tal como están documentados aquí.

---

**¡Mucho éxito con la implementación!** 🚀
