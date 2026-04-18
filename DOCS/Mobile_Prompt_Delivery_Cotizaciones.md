Eres un experto en React Native. Voy a pedirte que implementes dos módulos nuevos en mi app. Antes de escribir código, lee todo el contexto.

---

## MI STACK

- **React Native** sin Expo managed (componentes nativos puros)
- **Componentes base:** `View`, `Text`, `TouchableOpacity`, `ScrollView`, `SafeAreaView`, `StyleSheet`, `TextInput`, `ActivityIndicator`, `Modal`, `FlatList`
- **Iconos:** `MaterialCommunityIcons` de `@expo/vector-icons`
- **Navegación:** React Navigation (Stack + Tab ya configurados)
- **Estado global:** Context API
- **Sin librerías UI externas** — todos los componentes son propios con `StyleSheet`
- **HTTP:** `fetch` con Bearer token en el header `Authorization`

El estilo visual de la app usa colores oscuros tipo POS/dashboard, botones con `borderRadius: 8`, textos blancos sobre fondos grises oscuros. Sigue el mismo patrón de los componentes existentes.

---

## API BASE

```
BASE_URL = "https://<servidor>/api"
Headers: { "Authorization": "Bearer {token}", "Content-Type": "application/json" }
```

Todas las respuestas tienen la forma:
```json
{ "error": 0, "message": "...", "data": { ... } }
```
Si `error !== 0` mostrar el `message` como alerta de error.

---

## MÓDULO 1 — VENTAS DELIVERY

### Qué hace
Permite crear pedidos con dirección de entrega. El inventario y el pago se procesan al confirmar la entrega.

### Pantallas a crear

**Pantalla A: Lista de Deliveries Pendientes**
- `FlatList` con las ventas de tipo Delivery en estado `Draft`
- Cada card muestra: código, nombre cliente, dirección, total, fecha estimada de entrega
- Botón "Confirmar Entrega" en cada card → navega a Pantalla C
- Botón flotante "+" → navega a Pantalla B

**Pantalla B: Nueva Venta Delivery**
- Campos: Cliente (opcional, campo de texto con ID), Almacén (requerido), Dirección de entrega (opcional), Fecha estimada (DatePicker, opcional), Notas (opcional)
- Lista de productos: botón "Agregar Producto" abre un modal con `TextInput` para buscar por nombre/código, muestra resultados, al seleccionar pide cantidad y precio
- Resumen: subtotal, impuestos, total
- Botón "Crear Pedido"

**Pantalla C: Confirmar Entrega**
- Muestra resumen de la venta (cliente, productos, total)
- Campo para actualizar dirección real de entrega (opcional)
- Sección de pagos: botón "Agregar Pago" → modal con forma de pago (`Efectivo`, `Tarjeta de Crédito`, `Tarjeta de Débito`, `Transferencia`) y monto
- Permite múltiples formas de pago
- Botón "Confirmar Entrega y Cobrar"

### Endpoints

```
// Crear venta delivery
POST /api/sales/delivery
Body: {
  customerId: number | null,
  warehouseId: number,           // REQUERIDO
  priceListId: number | null,
  discountPercentage: number,    // 0-100
  requiresInvoice: boolean,
  deliveryAddress: string | null,
  scheduledDeliveryDate: string | null,  // ISO 8601
  notes: string | null,
  details: [
    { productId, quantity, unitPrice, discountPercentage, notes }
  ]
}

// Listar deliveries pendientes
GET /api/sales?saleType=Delivery&status=Draft&page=1&pageSize=20

// Ver detalle de venta
GET /api/sales/{id}

// Confirmar entrega y pago
PUT /api/sales/{id}/deliver
Body: {
  deliveryAddress: string | null,
  payments: [{ paymentMethod, amount }],
  notes: string | null
}
```

---

## MÓDULO 2 — COTIZACIONES

### Qué hace
Crea cotizaciones que NO mueven inventario. Cada cotización tiene un código único (ej: `COT-000007`) que se codifica en un QR. Al escanear el QR se convierte en una venta real (POS o Delivery).

### Pantallas a crear

**Pantalla A: Lista de Cotizaciones**
- `FlatList` con cotizaciones
- Cada card: código, cliente, total, fecha vencimiento, badge de estado (`Draft`=gris, `Converted`=verde, `Cancelled`=rojo, `Expired`=naranja)
- Al presionar una card → Pantalla C (detalle)
- Botón flotante "+" → Pantalla B
- Botón de escáner QR → Pantalla D

**Pantalla B: Nueva Cotización**
- Mismo formulario que Nueva Venta Delivery pero sin dirección ni fecha de entrega
- Campo extra: "Válida hasta" (fecha de vencimiento, opcional)
- Al crear exitosamente → mostrar modal con el código `COT-XXXXXX` y opción de ver QR

**Pantalla C: Detalle de Cotización**
- Muestra todos los campos y la lista de productos
- Si status es `Draft`: botón "Cancelar Cotización" y botón "Convertir a Venta" → abre modal para elegir POS o Delivery
- Si status es `Converted`: muestra el código de la venta generada (`saleCode`)

**Pantalla D: Escáner QR**
- Usa `expo-camera` o `expo-barcode-scanner` para leer el QR
- Al leer el código `COT-XXXXXX` → llama `GET /api/quotations/by-code/{code}`
- Muestra resumen de la cotización en un modal
- Botones: "Venta POS" / "Venta Delivery"
- Si Delivery → pide dirección y fecha → convierte
- Si POS → convierte directamente → navega al flujo de pago con `saleId` retornado

### Endpoints

```
// Crear cotización
POST /api/quotations
Body: {
  customerId: number | null,
  warehouseId: number,           // REQUERIDO
  priceListId: number | null,
  discountPercentage: number,
  requiresInvoice: boolean,
  validUntil: string | null,     // ISO 8601
  notes: string | null,
  details: [
    { productId, quantity, unitPrice, discountPercentage, notes }
  ]
}

// Listar cotizaciones (con filtros opcionales)
GET /api/quotations?page=1&pageSize=20&status=Draft

// Ver por ID
GET /api/quotations/{id}

// Buscar por código QR
GET /api/quotations/by-code/{code}

// Convertir en venta (tras escanear QR o desde detalle)
POST /api/quotations/{id}/convert
Body: {
  saleType: "POS" | "Delivery",
  deliveryAddress: string | null,
  scheduledDeliveryDate: string | null,
  notes: string | null
}
// Respuesta: { data: { saleId, saleCode, quotation: { status: "Converted" } } }

// Cancelar cotización
DELETE /api/quotations/{id}?reason=motivo
```

---

## ESTRUCTURA DE ARCHIVOS ESPERADA

```
src/
  screens/
    Delivery/
      DeliveryListScreen.js
      NewDeliveryScreen.js
      ConfirmDeliveryScreen.js
    Quotations/
      QuotationListScreen.js
      NewQuotationScreen.js
      QuotationDetailScreen.js
      QRScannerScreen.js
  components/
    delivery/
      DeliveryCard.js
    quotations/
      QuotationCard.js
      QuotationStatusBadge.js
  services/
    deliveryService.js
    quotationService.js
```

---

## REGLAS DE IMPLEMENTACIÓN

1. Todos los estilos con `StyleSheet.create` — sin `styled-components` ni inline styles sueltos
2. Iconos solo con `MaterialCommunityIcons`
3. Manejo de errores: `try/catch` en cada llamada al API, mostrar `Alert.alert` con el `message` del error
4. Loading states: mostrar `ActivityIndicator` mientras cargan los datos
5. Validación básica en formularios antes de llamar al API (campos requeridos)
6. Los servicios (`deliveryService.js`, `quotationService.js`) deben recibir el token como parámetro
7. Usar `useCallback` y `useMemo` donde corresponda para evitar re-renders innecesarios

---

**Empieza por el módulo que prefieras. Pídeme confirmación antes de pasar al siguiente módulo.**
