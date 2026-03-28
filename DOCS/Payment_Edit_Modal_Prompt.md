# PASO 1: Implementar Modo EDITAR para Pagos PPD

## CONTEXTO
Ya tenemos implementado RegisterPaymentModal para crear pagos. Ahora necesitamos un componente similar para EDITAR pagos existentes, con restricciones específicas según el estado de timbrado.

## BACKEND ENDPOINT DISPONIBLE

### Consultar Pago
```http
GET /api/accounts-receivable/payments/{id}
```
**Respuesta:**
```json
{
  "id": 2,
  "paymentNumber": "PAG-2026-0002",
  "customerId": 1,
  "customerName": "PÚBLICO EN GENERAL",
  "companyId": 1,
  "paymentDate": "2026-03-27",
  "totalAmount": 100.00,
  "currency": "MXN",
  "paymentFormSAT": "03",
  "reference": "TRANSFERENCIA #12345",
  
  "emisorRfc": "AAA010101AAA",
  "emisorNombre": "MI EMPRESA SA DE CV",
  "emisorRegimenFiscal": "601",
  "lugarExpedicion": "12345",
  
  "receptorRfc": "XAXX010101000",
  "receptorNombre": "PÚBLICO EN GENERAL",
  "receptorDomicilioFiscal": "67890",
  "receptorRegimenFiscal": "616",
  "receptorUsoCfdi": "CP01",
  
  "status": "Applied",
  "uuid": null,
  "timbradoAt": null,
  
  "applications": [
    {
      "id": 2,
      "invoiceId": 1,
      "serieAndFolio": "A-1",
      "amountApplied": 100.00,
      "previousBalance": 35958.26,
      "newBalance": 35858.26
    }
  ]
}
```

### Crear Pago (POST)
```http
POST /api/accounts-receivable/payments
Content-Type: application/json

{
  "customerId": 1,
  "companyId": 1,
  "paymentDate": "2026-03-27",
  "paymentFormSAT": "03",
  "currency": "MXN",
  "emisorRfc": "AAA010101AAA",
  "emisorNombre": "MI EMPRESA SA DE CV",
  "emisorRegimenFiscal": "601",
  "lugarExpedicion": "12345",
  "receptorRfc": "XAXX010101000",
  "receptorNombre": "PÚBLICO EN GENERAL",
  "receptorDomicilioFiscal": "67890",
  "receptorRegimenFiscal": "616",
  "receptorUsoCfdi": "CP01",
  "bankDestination": "BBVA",
  "accountDestination": "1234567890",
  "reference": "TRANSFERENCIA",
  "notes": "Pago por transferencia",
  "invoices": [
    { "invoicePPDId": 1, "amountToPay": 100.00 }
  ]
}
```
**NOTA:** Los campos fiscales (emisor/receptor) son **opcionales**. Si no se envían, el backend los toma automáticamente de la Company y de la primera factura.

### Actualizar Pago (PUT)
```http
PUT /api/accounts-receivable/payments/{id}
Content-Type: application/json

{
  "emisorRfc": "AAA010101AAA",
  "emisorNombre": "MI EMPRESA SA DE CV",
  "emisorRegimenFiscal": "601",
  "lugarExpedicion": "12345",
  "receptorRfc": "XAXX010101000",
  "receptorNombre": "PÚBLICO EN GENERAL",
  "receptorDomicilioFiscal": "67890",
  "receptorRegimenFiscal": "616",
  "receptorUsoCfdi": "CP01",
  "bankDestination": "BBVA",
  "accountDestination": "1234567890",
  "reference": "TRANSFERENCIA",
  "notes": "Notas actualizadas"
}
```
**RESTRICCIÓN CRÍTICA:** Solo permite editar si `uuid === null`
**IMPORTANTE:** El JSON de PUT tiene los mismos campos que el POST (excepto invoices/customerId/companyId que no se pueden cambiar)

**Respuesta Exitosa:**
```json
{
  "message": "Pago actualizado exitosamente",
  "error": 0,
  "data": { ... }
}
```

**Respuesta Error (ya timbrado):**
```json
{
  "message": "No se puede editar el pago PAG-2026-0002 porque ya está timbrado (UUID: xxx).",
  "error": 1
}
```

## VENTAJA: MISMO JSON PARA CREATE Y EDIT

El backend ahora acepta el **mismo JSON** para crear (POST) y editar (PUT). Esto simplifica enormemente el frontend:

```javascript
// Datos del formulario (usados tanto para CREATE como EDIT)
const paymentData = {
  // Campos de creación (solo para POST)
  customerId: 1,
  companyId: 1,
  paymentDate: "2026-03-27",
  paymentFormSAT: "03",
  currency: "MXN",
  invoices: [{ invoicePPDId: 1, amountToPay: 100 }],
  
  // Campos editables (se usan en POST y PUT)
  emisorRfc: "AAA010101AAA",
  emisorNombre: "MI EMPRESA SA DE CV",
  emisorRegimenFiscal: "601",
  lugarExpedicion: "12345",
  receptorRfc: "XAXX010101000",
  receptorNombre: "PÚBLICO EN GENERAL",
  receptorDomicilioFiscal: "67890",
  receptorRegimenFiscal: "616",
  receptorUsoCfdi: "CP01",
  bankDestination: "BBVA",
  accountDestination: "1234567890",
  reference: "TRANSFERENCIA",
  notes: "Notas"
};

// Para CREAR:
POST /api/accounts-receivable/payments
Body: paymentData (todos los campos)

// Para EDITAR:
PUT /api/accounts-receivable/payments/{id}
Body: paymentData (omitir customerId, companyId, paymentDate, paymentFormSAT, invoices)
```

**BENEFICIOS:**
1. Un solo formulario puede servir para crear y editar
2. El usuario puede personalizar datos fiscales desde la creación
3. Si no envías campos fiscales en POST, el backend los toma automáticamente de Company/Factura

---

## REQUISITOS DEL COMPONENTE

Crea **EditPaymentModal.jsx** con las siguientes características:

### 1. PROPS
```javascript
{
  isOpen: boolean,
  onClose: function,
  paymentId: number,
  onSuccess: function(updatedPayment)
}
```

### 2. LÓGICA DE ESTADOS

**Al abrir el modal:**
1. Cargar datos del pago con `GET /api/accounts-receivable/payments/{paymentId}`
2. Verificar `payment.uuid`:
   - Si `uuid !== null` → Pago TIMBRADO (solo lectura)
   - Si `uuid === null` → Pago NO timbrado (editable)

### 3. UI SEGÚN ESTADO DE TIMBRADO

#### A) SI ESTÁ TIMBRADO (uuid !== null):

**Mostrar:**
- Badge prominente: `🔒 PAGO TIMBRADO` (bg-green-100, text-green-800)
- Mensaje de advertencia: "Este pago ya está timbrado y no puede editarse"
- Todos los campos en SOLO LECTURA:
  * Aplicar `disabled={true}`
  * Estilo: `bg-gray-100 text-gray-600 cursor-not-allowed`
- Sección adicional "Información del Timbrado":
  ```
  UUID: {payment.uuid}
  Fecha: {payment.timbradoAt}
  ```
- Solo un botón: `[Cerrar]`

#### B) SI NO ESTÁ TIMBRADO (uuid === null):

**Campos EDITABLES (con formulario controlado):**
```javascript
✅ Datos del Emisor:
   - emisorRfc (input text, validar RFC)
   - emisorNombre (input text)
   - emisorRegimenFiscal (input text)
   - lugarExpedicion (input text, 5 dígitos)

✅ Datos del Receptor:
   - receptorRfc (input text, validar RFC)
   - receptorNombre (input text)
   - receptorDomicilioFiscal (input text, 5 dígitos)
   - receptorRegimenFiscal (input text)
   - receptorUsoCfdi (input DESHABILITADO, siempre "CP01")

✅ Datos Bancarios:
   - bankDestination (input text, opcional)
   - accountDestination (input text, opcional)

✅ Otros:
   - reference (input text, opcional)
   - notes (textarea, opcional)
```

**Campos NO EDITABLES (mostrar como texto o disabled):**
```javascript
❌ Cliente (customerName)
❌ Fecha del pago (paymentDate)
❌ Forma de pago SAT (paymentFormSAT)
❌ Total del pago (totalAmount)
❌ Facturas aplicadas (applications)
```

**Mostrar tabla de facturas aplicadas en solo lectura:**
| Folio | Monto Aplicado | Saldo Anterior | Saldo Nuevo |
|-------|----------------|----------------|-------------|
| A-1   | $100.00        | $35,958.26     | $35,858.26  |

**Botones:**
- `[Cancelar]` - Cierra el modal sin guardar
- `[Actualizar Pago]` - Envía PUT al backend (habilitado si hay cambios)

---

## VALIDACIONES

```javascript
// Validar RFC (13 caracteres, formato SAT)
const validateRFC = (rfc) => {
  const rfcRegex = /^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$/;
  return rfcRegex.test(rfc);
};

// Validar Código Postal (5 dígitos)
const validateCP = (cp) => {
  return /^\d{5}$/.test(cp);
};

// Validaciones a implementar:
- RFC del emisor debe ser válido
- RFC del receptor debe ser válido
- CP de expedición debe tener 5 dígitos
- Domicilio fiscal (CP) debe tener 5 dígitos
- Mostrar mensajes de error bajo cada campo inválido
```

---

## MANEJO DE ERRORES

```javascript
const handleUpdate = async () => {
  try {
    setLoading(true);
    
    const response = await fetch(`/api/accounts-receivable/payments/${paymentId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        emisorRfc: formData.emisorRfc,
        emisorNombre: formData.emisorNombre,
        emisorRegimenFiscal: formData.emisorRegimenFiscal,
        lugarExpedicion: formData.lugarExpedicion,
        receptorRfc: formData.receptorRfc,
        receptorNombre: formData.receptorNombre,
        receptorDomicilioFiscal: formData.receptorDomicilioFiscal,
        receptorRegimenFiscal: formData.receptorRegimenFiscal,
        receptorUsoCfdi: formData.receptorUsoCfdi,
        bankDestination: formData.bankDestination,
        accountDestination: formData.accountDestination,
        reference: formData.reference,
        notes: formData.notes
      })
    });
    
    const data = await response.json();
    
    if (data.error === 0) {
      toast.success(data.message);
      onSuccess(data.data); // Callback con el pago actualizado
      onClose();
    } else {
      toast.error(data.message);
    }
  } catch (error) {
    console.error('Error updating payment:', error);
    toast.error('Error al actualizar el pago');
  } finally {
    setLoading(false);
  }
};
```

---

## ESTRUCTURA DEL COMPONENTE

```jsx
import { useState, useEffect } from 'react';
import { toast } from 'react-toastify';

export default function EditPaymentModal({ isOpen, onClose, paymentId, onSuccess }) {
  const [loading, setLoading] = useState(false);
  const [payment, setPayment] = useState(null);
  const [formData, setFormData] = useState({});
  const [errors, setErrors] = useState({});
  
  const isTimbrado = payment?.uuid !== null;
  
  useEffect(() => {
    if (isOpen && paymentId) {
      loadPayment();
    }
  }, [isOpen, paymentId]);
  
  const loadPayment = async () => {
    try {
      setLoading(true);
      const response = await fetch(`/api/accounts-receivable/payments/${paymentId}`);
      const data = await response.json();
      setPayment(data);
      setFormData({
        emisorRfc: data.emisorRfc || '',
        emisorNombre: data.emisorNombre || '',
        emisorRegimenFiscal: data.emisorRegimenFiscal || '',
        lugarExpedicion: data.lugarExpedicion || '',
        receptorRfc: data.receptorRfc || '',
        receptorNombre: data.receptorNombre || '',
        receptorDomicilioFiscal: data.receptorDomicilioFiscal || '',
        receptorRegimenFiscal: data.receptorRegimenFiscal || '',
        receptorUsoCfdi: data.receptorUsoCfdi || 'CP01',
        bankDestination: data.bankDestination || '',
        accountDestination: data.accountDestination || '',
        reference: data.reference || '',
        notes: data.notes || ''
      });
    } catch (error) {
      toast.error('Error al cargar el pago');
    } finally {
      setLoading(false);
    }
  };
  
  const validateRFC = (rfc) => {
    const rfcRegex = /^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$/;
    return rfcRegex.test(rfc);
  };
  
  const validateCP = (cp) => {
    return /^\d{5}$/.test(cp);
  };
  
  const handleSubmit = (e) => {
    e.preventDefault();
    
    // Validar campos
    const newErrors = {};
    if (!validateRFC(formData.emisorRfc)) {
      newErrors.emisorRfc = 'RFC inválido';
    }
    if (!validateRFC(formData.receptorRfc)) {
      newErrors.receptorRfc = 'RFC inválido';
    }
    if (!validateCP(formData.lugarExpedicion)) {
      newErrors.lugarExpedicion = 'CP debe tener 5 dígitos';
    }
    if (!validateCP(formData.receptorDomicilioFiscal)) {
      newErrors.receptorDomicilioFiscal = 'CP debe tener 5 dígitos';
    }
    
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }
    
    handleUpdate();
  };
  
  const handleUpdate = async () => {
    try {
      setLoading(true);
      
      const response = await fetch(`/api/accounts-receivable/payments/${paymentId}`, {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(formData)
      });
      
      const data = await response.json();
      
      if (data.error === 0) {
        toast.success(data.message);
        onSuccess(data.data);
        onClose();
      } else {
        toast.error(data.message);
      }
    } catch (error) {
      console.error('Error updating payment:', error);
      toast.error('Error al actualizar el pago');
    } finally {
      setLoading(false);
    }
  };
  
  if (!isOpen) return null;
  
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-2xl font-bold">
              Editar Pago {payment?.paymentNumber}
            </h2>
            {isTimbrado && (
              <span className="px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm font-medium">
                🔒 Timbrado
              </span>
            )}
          </div>
          
          {loading && !payment ? (
            <div className="text-center py-8">Cargando...</div>
          ) : (
            <>
              {isTimbrado && (
                <div className="bg-yellow-50 border border-yellow-200 rounded p-4 mb-4">
                  ⚠️ Este pago ya está timbrado y no puede editarse
                </div>
              )}
              
              {/* Formulario */}
              <form onSubmit={handleSubmit}>
                {/* Sección Datos NO editables */}
                <div className="mb-6">
                  <h3 className="font-semibold mb-2 text-lg">📋 Información General</h3>
                  <div className="grid grid-cols-2 gap-4 bg-gray-50 p-4 rounded">
                    <div><strong>Cliente:</strong> {payment?.customerName}</div>
                    <div><strong>Fecha:</strong> {new Date(payment?.paymentDate).toLocaleDateString()}</div>
                    <div><strong>Total:</strong> ${payment?.totalAmount?.toFixed(2)}</div>
                    <div><strong>Forma de Pago:</strong> {payment?.paymentFormSAT}</div>
                  </div>
                </div>
                
                {/* Facturas Aplicadas (tabla solo lectura) */}
                <div className="mb-6">
                  <h3 className="font-semibold mb-2 text-lg">📄 Facturas Aplicadas</h3>
                  <div className="overflow-x-auto">
                    <table className="w-full border border-gray-200 rounded">
                      <thead className="bg-gray-100">
                        <tr>
                          <th className="px-4 py-2 text-left">Folio</th>
                          <th className="px-4 py-2 text-right">Monto Aplicado</th>
                          <th className="px-4 py-2 text-right">Saldo Anterior</th>
                          <th className="px-4 py-2 text-right">Saldo Nuevo</th>
                        </tr>
                      </thead>
                      <tbody>
                        {payment?.applications?.map(app => (
                          <tr key={app.id} className="border-t">
                            <td className="px-4 py-2">{app.serieAndFolio}</td>
                            <td className="px-4 py-2 text-right">${app.amountApplied?.toFixed(2)}</td>
                            <td className="px-4 py-2 text-right">${app.previousBalance?.toFixed(2)}</td>
                            <td className="px-4 py-2 text-right">${app.newBalance?.toFixed(2)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
                
                {/* Datos Fiscales EDITABLES */}
                <div className="mb-6">
                  <h3 className="font-semibold mb-2 text-lg">👤 Datos del Emisor</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium mb-1">RFC *</label>
                      <input
                        type="text"
                        value={formData.emisorRfc}
                        onChange={(e) => setFormData({...formData, emisorRfc: e.target.value.toUpperCase()})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        maxLength={13}
                      />
                      {errors.emisorRfc && <span className="text-red-500 text-sm">{errors.emisorRfc}</span>}
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">Nombre/Razón Social *</label>
                      <input
                        type="text"
                        value={formData.emisorNombre}
                        onChange={(e) => setFormData({...formData, emisorNombre: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">Régimen Fiscal *</label>
                      <input
                        type="text"
                        value={formData.emisorRegimenFiscal}
                        onChange={(e) => setFormData({...formData, emisorRegimenFiscal: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        placeholder="601"
                        maxLength={3}
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">CP de Expedición *</label>
                      <input
                        type="text"
                        value={formData.lugarExpedicion}
                        onChange={(e) => setFormData({...formData, lugarExpedicion: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        placeholder="12345"
                        maxLength={5}
                      />
                      {errors.lugarExpedicion && <span className="text-red-500 text-sm">{errors.lugarExpedicion}</span>}
                    </div>
                  </div>
                </div>
                
                <div className="mb-6">
                  <h3 className="font-semibold mb-2 text-lg">👥 Datos del Receptor</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium mb-1">RFC *</label>
                      <input
                        type="text"
                        value={formData.receptorRfc}
                        onChange={(e) => setFormData({...formData, receptorRfc: e.target.value.toUpperCase()})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        maxLength={13}
                      />
                      {errors.receptorRfc && <span className="text-red-500 text-sm">{errors.receptorRfc}</span>}
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">Nombre/Razón Social *</label>
                      <input
                        type="text"
                        value={formData.receptorNombre}
                        onChange={(e) => setFormData({...formData, receptorNombre: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">Domicilio Fiscal (CP) *</label>
                      <input
                        type="text"
                        value={formData.receptorDomicilioFiscal}
                        onChange={(e) => setFormData({...formData, receptorDomicilioFiscal: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        placeholder="67890"
                        maxLength={5}
                      />
                      {errors.receptorDomicilioFiscal && <span className="text-red-500 text-sm">{errors.receptorDomicilioFiscal}</span>}
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">Régimen Fiscal *</label>
                      <input
                        type="text"
                        value={formData.receptorRegimenFiscal}
                        onChange={(e) => setFormData({...formData, receptorRegimenFiscal: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        placeholder="616"
                        maxLength={3}
                      />
                    </div>
                    
                    <div className="col-span-2">
                      <label className="block text-sm font-medium mb-1">Uso CFDI</label>
                      <input
                        type="text"
                        value={formData.receptorUsoCfdi}
                        disabled={true}
                        className="w-full px-3 py-2 border rounded bg-gray-100 text-gray-600 cursor-not-allowed"
                      />
                      <span className="text-xs text-gray-500">Fijo en CP01 - Pagos para complementos de pago</span>
                    </div>
                  </div>
                </div>
                
                <div className="mb-6">
                  <h3 className="font-semibold mb-2 text-lg">🏦 Datos Bancarios</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium mb-1">Banco Destino</label>
                      <input
                        type="text"
                        value={formData.bankDestination}
                        onChange={(e) => setFormData({...formData, bankDestination: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        placeholder="BBVA"
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">Cuenta Destino</label>
                      <input
                        type="text"
                        value={formData.accountDestination}
                        onChange={(e) => setFormData({...formData, accountDestination: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        placeholder="1234567890"
                      />
                    </div>
                  </div>
                </div>
                
                <div className="mb-6">
                  <h3 className="font-semibold mb-2 text-lg">📝 Otros Datos</h3>
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium mb-1">Referencia</label>
                      <input
                        type="text"
                        value={formData.reference}
                        onChange={(e) => setFormData({...formData, reference: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        placeholder="Número de transferencia, cheque, etc."
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-1">Notas</label>
                      <textarea
                        value={formData.notes}
                        onChange={(e) => setFormData({...formData, notes: e.target.value})}
                        disabled={isTimbrado}
                        className={`w-full px-3 py-2 border rounded ${
                          isTimbrado 
                            ? 'bg-gray-100 text-gray-600 cursor-not-allowed' 
                            : 'border-gray-300 focus:border-blue-500'
                        }`}
                        rows={3}
                        placeholder="Notas adicionales..."
                      />
                    </div>
                  </div>
                </div>
                
                {/* Información de Timbrado (si aplica) */}
                {isTimbrado && (
                  <div className="mb-6 bg-green-50 border border-green-200 rounded p-4">
                    <h3 className="font-semibold mb-2 text-lg">📜 Información del Timbrado</h3>
                    <div className="space-y-2">
                      <div><strong>UUID:</strong> {payment.uuid}</div>
                      <div><strong>Fecha:</strong> {new Date(payment.timbradoAt).toLocaleString()}</div>
                    </div>
                  </div>
                )}
                
                {/* Botones */}
                <div className="flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={onClose}
                    className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
                  >
                    {isTimbrado ? 'Cerrar' : 'Cancelar'}
                  </button>
                  {!isTimbrado && (
                    <button
                      type="submit"
                      disabled={loading}
                      className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-400"
                    >
                      {loading ? 'Actualizando...' : 'Actualizar Pago'}
                    </button>
                  )}
                </div>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
```

---

## INTEGRACIÓN

En tu tabla de pagos (AccountsReceivableView o similar), agrega un botón "Editar" por cada fila:

```jsx
const [editModalOpen, setEditModalOpen] = useState(false);
const [selectedPaymentId, setSelectedPaymentId] = useState(null);

// En la tabla, columna de acciones:
<button 
  onClick={() => {
    setSelectedPaymentId(payment.id);
    setEditModalOpen(true);
  }}
  className="text-blue-600 hover:text-blue-800"
>
  ✏️ Editar
</button>

// Fuera de la tabla:
<EditPaymentModal
  isOpen={editModalOpen}
  onClose={() => {
    setEditModalOpen(false);
    setSelectedPaymentId(null);
  }}
  paymentId={selectedPaymentId}
  onSuccess={(updatedPayment) => {
    toast.success('Pago actualizado exitosamente');
    fetchPayments(); // Recargar lista de pagos
  }}
/>
```

---

## CASOS DE PRUEBA

1. ✅ Abrir modal con pago NO timbrado → debe permitir editar
2. ✅ Abrir modal con pago timbrado → todo en solo lectura
3. ✅ Editar RFC inválido → mostrar error
4. ✅ Editar CP inválido (no 5 dígitos) → mostrar error
5. ✅ Guardar cambios exitosamente → toast de éxito + cerrar modal
6. ✅ Error del backend → mostrar mensaje de error
7. ✅ Intentar editar pago timbrado → backend rechaza con error 400

---

## NOTAS IMPORTANTES

- El backend acepta **el mismo JSON** para POST y PUT (excepto campos inmutables como invoices, customerId)
- Los campos fiscales son **opcionales en POST**: si no los envías, el backend los toma automáticamente
- En **PUT solo se actualizan los campos fiscales/bancarios**, NO se pueden cambiar las facturas aplicadas
- El componente ya incluye TODO el código necesario
- Solo necesitas copiarlo a tu proyecto
- Asegúrate de tener `react-toastify` instalado
- El token JWT se obtiene de `localStorage.getItem('token')`
- Ajusta la URL base de la API según tu configuración

---

¿Genera este componente completo ahora?
