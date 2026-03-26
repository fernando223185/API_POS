# Frontend Requirements - Módulo de Cuentas por Cobrar

## 🎯 CONTEXTO DEL PROYECTO

Necesito implementar el módulo completo de **Cuentas por Cobrar (CxC)** en el frontend de mi sistema POS. El backend ya está implementado con .NET Core y expone una API RESTful completa.

**Stack de Frontend** (especifica el tuyo):
- Framework: React / Angular / Vue.js / Next.js
- UI Library: Material-UI / Ant Design / Tailwind / Bootstrap
- State Management: Redux / Zustand / Context API
- HTTP Client: Axios / Fetch API

---

## 📋 DESCRIPCIÓN DEL MÓDULO

El módulo de Cuentas por Cobrar permite a la empresa:

1. **Registrar facturas PPD** (Pago en Parcialidades o Diferido)
2. **Controlar el crédito de clientes** con límites y días de crédito
3. **Registrar pagos** individuales o masivos (batch)
4. **Generar complementos de pago SAT** automáticamente
5. **Visualizar reportes y métricas** en tiempo real
6. **Gestionar cobranza** con alertas y seguimiento

---

## 🔌 API ENDPOINTS DISPONIBLES

### Base URL
```
GET/POST/PUT: /api/accounts-receivable
```

### Facturas PPD
```http
GET    /api/accounts-receivable/invoices-ppd
       Query params: pageNumber, pageSize, customerId, status, fromDate, toDate, 
                     minDaysOverdue, minAmount, searchTerm

GET    /api/accounts-receivable/invoices-ppd/{id}

POST   /api/accounts-receivable/invoices-ppd
       Body: { invoiceId, customerId, customerName, customerRFC, companyId, 
               branchId, folioUUID, serie, folio, invoiceDate, creditDays, 
               totalAmount, ... }
```

### Pagos
```http
POST   /api/accounts-receivable/payments
       Body: { customerId, companyId, branchId, paymentDate, paymentMethodSAT, 
               reference, invoices: [{ invoicePPDId, amountToPay }], ... }

GET    /api/accounts-receivable/payments/{id}

POST   /api/accounts-receivable/payments/{id}/generate-complements
       Body: { sendEmailsAutomatically }
```

### Lotes de Pago
```http
POST   /api/accounts-receivable/batches
       Body: { companyId, branchId, paymentDate, payments: [...], ... }

GET    /api/accounts-receivable/batches/{id}

POST   /api/accounts-receivable/batches/{id}/generate-complements
       Body: { sendEmailsAutomatically }
```

### Política de Crédito
```http
GET    /api/accounts-receivable/customers/{customerId}/credit-policy

PUT    /api/accounts-receivable/customers/{customerId}/credit-policy
       Body: { companyId, creditLimit, creditDays, overdueGraceDays, 
               autoBlockOnOverdue, notes }

PUT    /api/accounts-receivable/customers/{customerId}/credit-status
       Body: { status, reason }
```

### Reportes y Dashboard
```http
GET    /api/accounts-receivable/dashboard?companyId={guid}&branchId={guid}

GET    /api/accounts-receivable/customers/{customerId}/statement?companyId={guid}

GET    /api/accounts-receivable/reports/overdue?companyId={guid}&minDaysOverdue={int}

GET    /api/accounts-receivable/reports/forecast?companyId={guid}&days={int}

GET    /api/accounts-receivable/metrics?companyId={guid}
```

---

## 🖥️ PANTALLAS REQUERIDAS

### 1. **Dashboard de Cuentas por Cobrar** (Pantalla Principal)

**Ruta sugerida**: `/accounts-receivable/dashboard`

**Componentes visuales**:

#### **KPIs en Cards** (4 cards en fila)
```
┌─────────────────────┐ ┌─────────────────────┐ ┌─────────────────────┐ ┌─────────────────────┐
│ 💰 Total por Cobrar │ │ 🔴 Vencido          │ │ 🟢 Por Vencer       │ │ 📅 Vence Hoy        │
│                     │ │                     │ │                     │ │                     │
│   $285,450.00       │ │   $67,300.00        │ │   $218,150.00       │ │   $12,400.00        │
│   127 facturas      │ │   23.6%             │ │   45 clientes       │ │   5 facturas        │
└─────────────────────┘ └─────────────────────┘ └─────────────────────┘ └─────────────────────┘
```

#### **Gráfica de Antigüedad de Saldos** (Bar Chart)
- 4 barras: 0-30 días, 31-60 días, 61-90 días, +90 días
- Mostrar monto y porcentaje
- Colores: Verde, Amarillo, Naranja, Rojo

#### **Top 5 Clientes con Mayor Saldo** (Tabla)
| Cliente              | Saldo Pendiente | Facturas | Vencido   | Estado   |
|---------------------|----------------|----------|-----------|----------|
| Constructora López  | $45,200        | 5        | $28,000   | ⚠️ Alerta |
| Ferretería Norte    | $38,900        | 3        | $0        | ✅ OK    |
| ...                 | ...            | ...      | ...       | ...      |

#### **Métricas Clave** (Cards pequeñas)
- DSO (Days Sales Outstanding): 38 días
- Índice de Morosidad: 27%
- Tasa de Recuperación: 94%
- Promedio de Pago: 42 días

#### **Acciones Rápidas** (Botones principales)
- [Registrar Pago]
- [Ver Facturas Vencidas]
- [Procesar Lote]
- [Reportes]

**Endpoint a consumir**: `GET /api/accounts-receivable/dashboard?companyId={id}`

---

### 2. **Listado de Facturas PPD Pendientes**

**Ruta sugerida**: `/accounts-receivable/invoices`

**Componentes**:

#### **Filtros** (Barra superior)
- Búsqueda: Por folio, UUID, cliente
- Cliente: Dropdown con autocomplete
- Estado: Dropdown (Pendiente, Parcialmente Pagado, Vencido)
- Rango de fechas: DateRangePicker
- Días vencido: Input numérico (mínimo)
- [Botón Filtrar] [Botón Limpiar]

#### **Tabla de Facturas** (DataTable con paginación)

| ☑️ | Cliente           | Folio    | Fecha Factura | Vencimiento | Original   | Pagado    | Saldo      | Días Venc. | Estado           | Acciones |
|----|------------------|----------|---------------|-------------|------------|-----------|------------|------------|------------------|----------|
| ☑️ | Juan Pérez       | A-1001   | 2026-01-15   | 2026-02-14  | $12,000.00 | $0.00     | $12,000.00 | 39 días    | 🔴 Vencido       | [👁️] [💰] |
| ☑️ | María López      | A-1005   | 2026-02-01   | 2026-03-03  | $8,500.00  | $2,500.00 | $6,000.00  | 22 días    | 🔴 Parcial Venc. | [👁️] [💰] |
| ☑️ | Pedro García     | A-1010   | 2026-02-20   | 2026-03-22  | $15,200.00 | $0.00     | $15,200.00 | 0 días     | 🟢 Pendiente     | [👁️] [💰] |

**Features**:
- Checkbox para selección múltiple
- Ordenamiento por columnas clickeables
- Paginación: 10, 20, 50, 100 por página
- Colores según estado:
  - Rojo: Vencido
  - Amarillo: Por vencer (menos de 5 días)
  - Verde: Pendiente (más de 5 días para vencer)

**Acciones masivas** (cuando hay selección):
- [Registrar Pago Múltiple] (abre modal)
- [Procesar Lote] (abre wizard)
- [Exportar a Excel]

**Acciones individuales**:
- 👁️ Ver detalle (modal con info completa + historial de pagos)
- 💰 Registrar pago (abre modal de pago)

**Endpoint**: `GET /api/accounts-receivable/invoices-ppd` con query params

---

### 3. **Modal: Registrar Pago Individual**

**Trigger**: Click en botón [💰 Pagar] en tabla de facturas

**Componentes del Modal**:

```
┌─────────────────────────────────────────────────────────────┐
│  Registrar Pago                                         [X]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Cliente: Juan Pérez - JPE850215XX9                        │
│                                                             │
│  📋 Facturas Seleccionadas:                                │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ Folio      │ Saldo      │ [Monto a Pagar] │ [100%]  │  │
│  │ A-1234     │ $12,000.00 │ [_12,000.00__]  │ [☑️]    │  │
│  │ A-1456     │ $8,500.00  │ [_8,500.00___]  │ [☑️]    │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  Total a Pagar: $20,500.00                                 │
│                                                             │
│  📅 Datos del Pago:                                        │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ Fecha de Pago*:     [2026-03-25] (DatePicker)        │  │
│  │ Método de Pago*:    [03 - Transferencia ▼]           │  │
│  │ Banco Origen:       [_________________]               │  │
│  │ Cuenta Origen:      [_________________]               │  │
│  │ Banco Destino:      [_________________]               │  │
│  │ Cuenta Destino:     [_________________]               │  │
│  │ Referencia*:        [TRF-123456_______]               │  │
│  │ Notas:              [_______________]                 │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                             │
│  ☑️ Generar complementos automáticamente                   │
│  ☑️ Enviar complementos por email al cliente               │
│                                                             │
│  [Cancelar]                      [Registrar Pago]          │
└─────────────────────────────────────────────────────────────┘
```

**Validaciones**:
- Fecha de pago no puede ser futura
- Método de pago es requerido (dropdown con catálogo SAT)
- Al menos una factura debe tener monto > 0
- Monto a pagar no puede exceder el saldo de la factura

**Flujo**:
1. Usuario llena el formulario
2. Click en [Registrar Pago]
3. POST a `/api/accounts-receivable/payments` con el body
4. Si check de "Generar complementos" está activo:
   - POST a `/api/accounts-receivable/payments/{id}/generate-complements`
5. Mostrar resultado (éxito/error)
6. Si éxito, mostrar modal de resultado con opciones:
   - [Descargar Complementos]
   - [Ver Detalle del Pago]
   - [Registrar Otro Pago]

**Endpoint**: `POST /api/accounts-receivable/payments`

---

### 4. **Wizard: Procesar Lote de Pagos**

**Trigger**: Botón [Procesar Lote] con facturas seleccionadas

**Pasos del Wizard**:

#### **Paso 1: Selección de Facturas** ✅
```
┌─────────────────────────────────────────────────────────────┐
│  Paso 1 de 3: Selección de Facturas                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Has seleccionado 25 facturas de 10 clientes diferentes    │
│  Total: $145,280.00                                         │
│                                                             │
│  [Tabla resumen agrupada por cliente]                      │
│                                                             │
│  [Atrás]                                    [Continuar →]   │
└─────────────────────────────────────────────────────────────┘
```

#### **Paso 2: Datos del Lote**
```
┌─────────────────────────────────────────────────────────────┐
│  Paso 2 de 3: Datos del Lote                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Fecha de Pago Común*:    [2026-03-25] (DatePicker)        │
│  Método de Pago Default*: [03 - Transferencia ▼]           │
│  Banco Destino:           [_________________]               │
│  Cuenta Destino:          [_________________]               │
│  Descripción del Lote:    [_________________]               │
│  Notas:                   [_______________]                 │
│                                                             │
│  [← Atrás]                                 [Continuar →]   │
└─────────────────────────────────────────────────────────────┘
```

#### **Paso 3: Confirmación y Procesamiento**
```
┌─────────────────────────────────────────────────────────────┐
│  Paso 3 de 3: Confirmación                                 │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  📊 Resumen del Lote:                                       │
│  • Total facturas: 25                                       │
│  • Total clientes: 10                                       │
│  • Monto total: $145,280.00                                 │
│  • Fecha de pago: 25/03/2026                                │
│                                                             │
│  ☑️ Generar complementos automáticamente después del lote   │
│  ☑️ Enviar complementos por email a los clientes            │
│                                                             │
│  [← Atrás]           [Cancelar]      [Procesar Lote]       │
└─────────────────────────────────────────────────────────────┘
```

**Después de procesar**:
```
┌─────────────────────────────────────────────────────────────┐
│  ✅ Lote Procesado Exitosamente                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Lote: LOTE-2026-001                                        │
│                                                             │
│  📊 Generando Complementos...                               │
│  ████████████████░░░░ 18/25 (72%)                          │
│                                                             │
│  ✅ Exitosos: 18                                            │
│  ⏳ En proceso: 2                                           │
│  ❌ Errores: 0                                              │
│  ⏸️ Pendientes: 5                                           │
│                                                             │
│  [Ver Detalle del Lote] [Descargar ZIP] [Cerrar]           │
└─────────────────────────────────────────────────────────────┘
```

**Endpoints**:
1. `POST /api/accounts-receivable/batches`
2. `POST /api/accounts-receivable/batches/{id}/generate-complements`
3. Polling: `GET /api/accounts-receivable/batches/{id}` (cada 2 segundos para actualizar progreso)

---

### 5. **Estado de Cuenta del Cliente**

**Ruta sugerida**: `/accounts-receivable/customers/{id}/statement`

**Componentes**:

#### **Header del Cliente**
```
┌─────────────────────────────────────────────────────────────┐
│  ESTADO DE CUENTA                                           │
│                                                             │
│  Juan Pérez Construcciones S.A. de C.V.                    │
│  RFC: JPE850215XX9                                          │
│  Email: juan@example.com | Tel: 555-1234                   │
└─────────────────────────────────────────────────────────────┘
```

#### **Política de Crédito** (Card editable)
```
┌─────────────────────────────────────────────────────────────┐
│  💳 POLÍTICA DE CRÉDITO                          [✏️ Editar] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Límite de Crédito:    $100,000.00                         │
│  Disponible:           $35,000.00   (35%)                   │
│  Utilizado:            $65,000.00   ████████░░░             │
│  Días de Crédito:      30 días                              │
│  Estado:               🟢 ACTIVO                            │
│                                                             │
│  📊 Indicadores:                                            │
│  • Promedio de pago: 42 días                                │
│  • Puntualidad: 72% (últimos 6 meses)                       │
│  • Último pago: 10/03/2026 ($6,500.00)                      │
└─────────────────────────────────────────────────────────────┘
```

**Acciones**: Al click en [✏️ Editar], abrir modal para modificar:
- Límite de crédito
- Días de crédito
- Estado (Activo/Bloqueado/Suspendido)
- Notas

#### **Facturas Pendientes** (Tabla)
| Folio  | Fecha      | Vencimiento | Original   | Pagado    | Saldo      | Estado    |
|--------|------------|-------------|------------|-----------|------------|-----------|
| A-1234 | 2026-01-15 | 2026-02-14  | $12,000.00 | $0.00     | $12,000.00 | 🔴 Vencido |
| A-1456 | 2026-02-01 | 2026-03-03  | $8,500.00  | $6,000.00 | $2,500.00  | 🔴 Vencido |
| ...    | ...        | ...         | ...        | ...       | ...        | ...       |

**Totales**:
- Total Pendiente: $65,000.00
- Total Vencido: $18,000.00

#### **Historial de Pagos Recientes** (Tabla)
| Fecha      | No. Pago   | Facturas Pagadas | Monto      | Referencia  | Complementos |
|------------|------------|------------------|------------|-------------|--------------|
| 2026-03-10 | PAG-00123  | A-1456           | $6,500.00  | TRF-456789  | [📄 Ver]     |
| 2026-02-25 | PAG-00098  | A-1442, A-1443   | $15,000.00 | TRF-445566  | [📄 Ver]     |

#### **Historial de Eventos** (Timeline)
```
⏱️ 2026-03-10 10:30 - Pago recibido - $6,500.00 (Factura A-1456)
⏱️ 2026-03-01 - Factura A-1234 pasó a estado "Vencido"
⏱️ 2026-02-25 - Pago atrasado recibido - $5,000.00 (15 días tarde)
⏱️ 2026-02-15 - Límite de crédito aumentado a $100,000
```

**Endpoint**: `GET /api/accounts-receivable/customers/{id}/statement?companyId={id}`

---

### 6. **Reportes**

**Ruta sugerida**: `/accounts-receivable/reports`

#### **Reporte de Facturas Vencidas**

**Filtros**:
- Días vencido mínimo: [Input]
- Sucursal: [Dropdown]
- [Generar Reporte]

**Tabla Agrupada por Cliente**:
| Cliente              | Facturas | Total Vencido | Más Antigua  | Días Máx | Estado   | Acciones          |
|---------------------|----------|---------------|--------------|----------|----------|-------------------|
| Constructora López  | 5        | $45,200.00   | 15/12/2025   | 98       | ⚠️ Blocked | [Ver] [Contactar] |
| Ferretería Norte    | 3        | $28,300.00   | 20/01/2026   | 64       | ⚠️ Warning | [Ver] [Contactar] |

**Acciones**:
- [Exportar a Excel]
- [Enviar Recordatorios Masivos]
- [Imprimir]

**Endpoint**: `GET /api/accounts-receivable/reports/overdue`

#### **Proyección de Cobranza**

**Filtros**:
- Días a proyectar: [90] (default)
- [Generar]

**Gráfica de Barras** (Por semana o mes):
```
   $125K ┤           ████
   $100K ┤           ████
    $75K ┤     ████  ████
    $50K ┤████ ████  ████  ████
    $25K ┤████ ████  ████  ████
         └────┴─────┴─────┴─────
         Mar  Abr   May   Jun
```

**Tabla de Detalle**:
| Periodo       | Facturas | Monto Proyectado | Clientes |
|--------------|----------|------------------|----------|
| Esta Semana  | 8        | $45,200.00      | 5        |
| Próxima Sem. | 12       | $62,150.00      | 8        |
| Semana 3     | 9        | $38,900.00      | 6        |

**Endpoint**: `GET /api/accounts-receivable/reports/forecast?days=90`

#### **Métricas de CxC**

Cards con los KPIs principales:
- DSO (Days Sales Outstanding)
- Índice de Morosidad
- Tasa de Recuperación
- Efectividad de Cobranza

**Endpoint**: `GET /api/accounts-receivable/metrics`

---

## 🎨 DISEÑO Y UX

### **Paleta de Colores para Estados**
- 🟢 **Verde** (#4CAF50): Activo, Saludable, OK
- 🟡 **Amarillo** (#FFC107): Atención, Alerta, Próximo a vencer
- 🟠 **Naranja** (#FF9800): Riesgo medio, 61-90 días
- 🔴 **Rojo** (#F44336): Vencido, Bloqueado, Riesgo alto
- 🔵 **Azul** (#2196F3): Procesando, En progreso
- ⚪ **Gris** (#9E9E9E): Cancelado, Inactivo

### **Iconos Sugeridos**
- 💰 Dinero/Pagos
- 📋 Facturas
- 📊 Reportes
- 📈 Métricas/Dashboard
- 👤 Clientes
- ⚠️ Alertas
- ✅ Éxito
- ❌ Error
- ⏳ En proceso
- 🔍 Buscar
- ✏️ Editar
- 👁️ Ver detalle
- 📧 Email
- 📄 Documento/Complemento
- 🔒 Bloqueado
- 🔓 Desbloqueado

### **Componentes Reutilizables a Crear**
1. **InvoiceStatusBadge** - Badge con color según estado
2. **CurrencyDisplay** - Formatea montos con $
3. **DateDisplay** - Formatea fechas consistentemente
4. **DaysOverdueBadge** - Badge con días vencido (rojo si >30)
5. **CustomerCreditBar** - Barra de progreso de crédito utilizado
6. **AgingChart** - Gráfica de antigüedad reutilizable
7. **InvoiceSelector** - Selector de facturas con checkboxes
8. **PaymentMethodSelect** - Dropdown de métodos de pago SAT
9. **ProgressModal** - Modal con barra de progreso para batch

### **Responsividad**
- Dashboard: En móvil, cards apilados verticalmente
- Tablas: Scroll horizontal en móvil o cards colapsables
- Modales: Fullscreen en móvil, centrados en desktop
- Filtros: Colapsables en móvil con botón "Filtros"

### **Notificaciones**
- **Toast/Snackbar** para acciones exitosas: "Pago registrado correctamente"
- **Alert permanente** para errores: "Error al generar complemento: [detalle]"
- **Confirmaciones** antes de acciones destructivas: "¿Bloquear crédito del cliente?"

---

## 🔧 FUNCIONALIDADES TÉCNICAS

### **Manejo de Estado**
- Cachear datos del dashboard (refresh manual o cada 5 min)
- Estado global para usuario actual, companyId, branchId
- Loading states para todas las operaciones async
- Error handling con retry automático para requests fallidos

### **Validaciones en Frontend**
- Montos no negativos
- Fechas en formato correcto
- Campos requeridos marcados con *
- Validación en tiempo real (onChange) para mejor UX

### **Paginación**
- Server-side pagination (consumir pageNumber, pageSize del API)
- Opciones: 10, 20, 50, 100 registros por página
- Mostrar "Mostrando X-Y de Z registros"

### **Búsqueda y Filtros**
- Debounce en búsqueda (300ms)
- Mantener filtros en URL (query params) para compartir enlaces
- Botón "Limpiar filtros"

### **Exportación**
- Función para descargar Excel (usar librería como xlsx o SheetJS)
- Descargar ZIP con complementos (llamar al endpoint correspondiente)

### **Polling para Procesos Largos**
- Al generar complementos de lote, hacer polling cada 2 seg
- Actualizar progress bar en tiempo real
- Detener polling automáticamente al completar o después de timeout

### **Autenticación y Permisos**
- Verificar permisos del usuario antes de mostrar botones
- Permisos necesarios:
  - `CuentasporCobrar.View` - Ver pantallas
  - `CuentasporCobrar.Create` - Registrar pagos, crear lotes
  - `CuentasporCobrar.Edit` - Editar políticas de crédito
- Si no tiene permiso, mostrar mensaje o ocultar opción

---

## 📝 DTOs Y MODELOS (TypeScript/JavaScript)

```typescript
// Factura PPD
interface InvoicePPD {
  id: string;
  invoiceId: string;
  customerId: string;
  customerName: string;
  customerRFC: string;
  folioUUID: string;
  serie?: string;
  folio?: string;
  serieAndFolio: string;
  invoiceDate: string; // ISO date
  dueDate: string;
  currency: string;
  exchangeRate: number;
  originalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  nextPartialityNumber: number;
  totalPartialities: number;
  status: 'Pending' | 'PartiallyPaid' | 'Paid' | 'Overdue' | 'Cancelled';
  daysOverdue: number;
  lastPaymentDate?: string;
  notes?: string;
  createdAt: string;
}

// Request para registrar pago
interface CreatePaymentRequest {
  customerId: string;
  companyId: string;
  branchId: string;
  paymentDate: string;
  paymentMethodSAT: string;
  paymentFormSAT?: string;
  currency: string;
  exchangeRate: number;
  bankOrigin?: string;
  bankAccountOrigin?: string;
  bankDestination?: string;
  bankAccountDestination?: string;
  reference?: string;
  notes?: string;
  invoices: PaymentInvoiceItem[];
}

interface PaymentInvoiceItem {
  invoicePPDId: string;
  amountToPay: number;
}

// Dashboard
interface AccountsReceivableDashboard {
  totalReceivable: number;
  totalOverdue: number;
  totalDueToday: number;
  totalNotDue: number;
  totalInvoicesPending: number;
  customersWithBalance: number;
  averageCollectionDays: number;
  collectedThisMonth: number;
  overduePercentage: number;
  agingReport: AgingReport;
  topCustomers: TopCustomerBalance[];
}

interface AgingReport {
  current: number;
  days31To60: number;
  days61To90: number;
  over90Days: number;
  currentCount: number;
  days31To60Count: number;
  days61To90Count: number;
  over90DaysCount: number;
}

// Política de Crédito
interface CustomerCreditPolicy {
  id: string;
  customerId: string;
  customerName: string;
  companyId: string;
  creditLimit: number;
  creditDays: number;
  overdueGraceDays: number;
  totalPendingAmount: number;
  totalOverdueAmount: number;
  availableCredit: number;
  oldestInvoiceDate?: string;
  averagePaymentDays: number;
  onTimePaymentRate: number;
  lastPaymentDate?: string;
  lastPaymentAmount: number;
  status: 'Active' | 'Warning' | 'Blocked' | 'Suspended';
  blockReason?: string;
  autoBlockOnOverdue: boolean;
  notes?: string;
}

// ... Agregar más según necesites
```

---

## 🚀 FLUJOS PRINCIPALES

### **Flujo 1: Registrar Pago Individual**
1. Usuario entra a "Facturas Pendientes"
2. Busca/filtra la factura del cliente
3. Click en [💰 Pagar]
4. Modal se abre pre-llenado con info de la factura
5. Usuario llena datos del pago
6. Click [Registrar Pago]
7. Loading...
8. Si éxito: Modal de confirmación con opciones
9. Si error: Mostrar mensaje de error

### **Flujo 2: Procesar Lote Masivo**
1. Usuario selecciona múltiples facturas (checkboxes)
2. Click [Procesar Lote]
3. Wizard paso 1: Confirma selección
4. Wizard paso 2: Datos comunes del lote
5. Wizard paso 3: Confirmación
6. Click [Procesar Lote]
7. Loading...
8. Si éxito: Modal con progreso de generación de complementos
9. Polling para actualizar progreso
10. Al completar: Opciones de descarga/envío

### **Flujo 3: Ver Estado de Cuenta**
1. Usuario entra a Estado de Cuenta del cliente
2. Sistema carga política, facturas, pagos, historial
3. Usuario puede:
   - Ver resumen completo
   - Editar política de crédito
   - Registrar nuevo pago
   - Ver detalle de pagos anteriores
   - Descargar complementos

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

### Fase 1: Setup y Estructura
- [ ] Crear estructura de carpetas del módulo
- [ ] Configurar routing (`/accounts-receivable/*`)
- [ ] Crear servicio API base con Axios/Fetch
- [ ] Implementar DTOs/Interfaces TypeScript
- [ ] Configurar state management (si aplica)

### Fase 2: Componentes Base
- [ ] InvoiceStatusBadge
- [ ] CurrencyDisplay
- [ ] DateDisplay
- [ ] DaysOverdueBadge
- [ ] CustomerCreditBar
- [ ] Loading spinner/skeleton
- [ ] Error boundary

### Fase 3: Dashboard
- [ ] Layout principal de dashboard
- [ ] Cards de KPIs
- [ ] Gráfica de antigüedad
- [ ] Top clientes tabla
- [ ] Métricas adicionales
- [ ] Acciones rápidas
- [ ] Integración con API

### Fase 4: Listado de Facturas
- [ ] Tabla de facturas con paginación
- [ ] Filtros avanzados
- [ ] Búsqueda con debounce
- [ ] Selección múltiple (checkboxes)
- [ ] Acciones individuales (ver, pagar)
- [ ] Acciones masivas
- [ ] Exportar a Excel

### Fase 5: Registro de Pagos
- [ ] Modal de pago individual
- [ ] Validaciones del formulario
- [ ] Dropdown de métodos de pago SAT
- [ ] Cálculos automáticos de totales
- [ ] Integración con API de pagos
- [ ] Modal de resultado/confirmación

### Fase 6: Lotes de Pago
- [ ] Wizard de 3 pasos
- [ ] Paso 1: Resumen de selección
- [ ] Paso 2: Datos del lote
- [ ] Paso 3: Confirmación
- [ ] Procesamiento con progress bar
- [ ] Polling de estado
- [ ] Modal de resultado con descarga

### Fase 7: Estado de Cuenta
- [ ] Layout de estado de cuenta
- [ ] Card de política de crédito
- [ ] Modal de edición de política
- [ ] Tabla de facturas pendientes
- [ ] Tabla de pagos recientes
- [ ] Timeline de historial
- [ ] Integración con API

### Fase 8: Reportes
- [ ] Layout de reportes
- [ ] Reporte de vencidos con tabla
- [ ] Proyección de cobranza con gráfica
- [ ] Métricas de CxC
- [ ] Exportación a Excel
- [ ] Integración con API

### Fase 9: Pulido y Testing
- [ ] Responsividad móvil
- [ ] Manejo de errores
- [ ] Loading states
- [ ] Toast notifications
- [ ] Validaciones frontend
- [ ] Testing en navegadores
- [ ] Performance optimization

---

## 🎯 PRIORIDADES

**ALTA** (Must have):
1. Dashboard de CxC
2. Listado de facturas con filtros
3. Modal de pago individual
4. Estado de cuenta del cliente
5. Reporte de vencidos

**MEDIA** (Should have):
1. Wizard de lotes
2. Proyección de cobranza
3. Edición de políticas de crédito
4. Exportación a Excel

**BAJA** (Nice to have):
1. Timeline de historial
2. Envío de emails desde frontend
3. Gráficas avanzadas
4. Notificaciones en tiempo real

---

## 📞 NOTAS ADICIONALES

- Consulta catálogo SAT de métodos de pago: https://www.sat.gob.mx/consulta/16053/catalogo-de-metodos-de-pago
- Todas las fechas deben manejarse en UTC y convertirse a zona horaria local para display
- Montos siempre en 2 decimales: `amount.toFixed(2)`
- Los UUIDs deben mostrarse truncados con tooltip para ver completo: `ABC...XYZ`
- Considerar modo offline: cachear dashboard para consulta sin conexión

---

Genera el código completo del módulo siguiendo esta especificación. Organiza los archivos según las mejores prácticas del framework que uses.
