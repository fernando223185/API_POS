# Prompt — Botón PDF de Cotización (Electron + React)

## CONTEXTO

Ya tienes implementado el módulo de Cotizaciones con las siguientes pantallas:
- `/quotations` — Lista de cotizaciones
- `/quotations/new` — Nueva cotización
- `/quotations/:id` — Detalle de cotización
- `/quotations/scan` — Escanear QR

## NUEVO ENDPOINT DISPONIBLE

```
GET /api/quotations/{id}/pdf
Authorization: Bearer {token}
```

**Respuesta:** archivo binario `application/pdf` con nombre `cotizacion-{id}.pdf`

El PDF incluye:
- Header con datos de la empresa y logo
- Código de cotización grande (ej: `COT-000007`) con badge de estado
- Tabla de productos detallada
- Panel de totales (Subtotal, Descuento, IVA, Total)
- **Código QR** que al escanearse desde la app convierte la cotización en venta
- Notas si las hay

---

## LO QUE NECESITAS IMPLEMENTAR

### 1. Servicio `quotationService.js`

Agrega este método al servicio existente:

```javascript
async getPdf(id) {
  const response = await fetch(`${API_BASE_URL}/quotations/${id}/pdf`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${localStorage.getItem('token')}`
    }
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new Error(error.message || 'Error al obtener el PDF');
  }

  return response.blob(); // retorna Blob binario
}
```

### 2. Hook `useQuotationPdf.js`

Crea el hook `src/hooks/useQuotationPdf.js`:

```javascript
import { useState } from 'react';
import { quotationService } from '../services/api/quotationService';
import { toast } from 'react-toastify'; // o tu sistema de alertas

export function useQuotationPdf() {
  const [loading, setLoading] = useState(false);

  const openPdf = async (id) => {
    setLoading(true);
    try {
      const blob = await quotationService.getPdf(id);
      const url  = URL.createObjectURL(blob);
      window.open(url, '_blank');        // abre en pestaña/ventana del OS
      setTimeout(() => URL.revokeObjectURL(url), 60000);
    } catch (err) {
      toast.error(err.message || 'No se pudo generar el PDF');
    } finally {
      setLoading(false);
    }
  };

  const downloadPdf = async (id, code) => {
    setLoading(true);
    try {
      const blob = await quotationService.getPdf(id);
      const url  = URL.createObjectURL(blob);
      const a    = document.createElement('a');
      a.href     = url;
      a.download = `${code || `cotizacion-${id}`}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      toast.error(err.message || 'No se pudo descargar el PDF');
    } finally {
      setLoading(false);
    }
  };

  return { openPdf, downloadPdf, loading };
}
```

### 3. Componente `QuotationPdfButtons.jsx`

Crea `src/components/quotations/QuotationPdfButtons.jsx`:

```jsx
import { Button, ButtonGroup, CircularProgress, Tooltip } from '@mui/material';
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf';
import DownloadIcon from '@mui/icons-material/Download';
import { useQuotationPdf } from '../../hooks/useQuotationPdf';

/**
 * @param {number} id    - ID de la cotización
 * @param {string} code  - Código (ej: COT-000007) para nombre del archivo
 * @param {string} [size]  - 'small' | 'medium' (default: 'small')
 */
export function QuotationPdfButtons({ id, code, size = 'small' }) {
  const { openPdf, downloadPdf, loading } = useQuotationPdf();

  return (
    <ButtonGroup variant="outlined" size={size} disabled={loading}>
      <Tooltip title="Ver PDF">
        <Button
          startIcon={loading ? <CircularProgress size={14} /> : <PictureAsPdfIcon />}
          onClick={() => openPdf(id)}
          color="primary"
        >
          PDF
        </Button>
      </Tooltip>
      <Tooltip title="Descargar PDF">
        <Button
          onClick={() => downloadPdf(id, code)}
          color="primary"
        >
          <DownloadIcon fontSize="small" />
        </Button>
      </Tooltip>
    </ButtonGroup>
  );
}
```

---

## DÓNDE AGREGAR LOS BOTONES

### A) En `QuotationDetailPage.jsx` — pantalla de detalle `/quotations/:id`

Agrega los botones junto a los botones existentes de "Cancelar" y "Convertir":

```jsx
import { QuotationPdfButtons } from '../../components/quotations/QuotationPdfButtons';

// Dentro del JSX, en la barra de acciones superior o inferior:
<Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mb: 2 }}>
  <QuotationPdfButtons id={quotation.id} code={quotation.code} />

  {quotation.status === 'Draft' && (
    <>
      <Button variant="outlined" color="error" onClick={handleCancel}>
        🗑️ Cancelar Cotización
      </Button>
      <Button variant="contained" color="primary" onClick={handleConvert}>
        ✅ Convertir a Venta
      </Button>
    </>
  )}
</Box>
```

### B) En `QuotationListPage.jsx` — tabla/cards de lista `/quotations`

Agrega un ícono PDF en cada fila/card:

```jsx
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useQuotationPdf } from '../../hooks/useQuotationPdf';

// Dentro del componente:
const { openPdf, loading: pdfLoading } = useQuotationPdf();

// En la columna de acciones de la tabla:
<TableCell align="right">
  <Tooltip title="Ver PDF">
    <IconButton
      size="small"
      onClick={() => openPdf(row.id)}
      disabled={pdfLoading}
      color="primary"
    >
      <PictureAsPdfIcon fontSize="small" />
    </IconButton>
  </Tooltip>
  <IconButton size="small" onClick={() => navigate(`/quotations/${row.id}`)}>
    <VisibilityIcon fontSize="small" />
  </IconButton>
</TableCell>
```

### C) Modal de éxito al crear cotización (`QuotationQRModal.jsx`)

Cuando se crea una cotización exitosamente y aparece el modal con el QR, agrega los botones PDF:

```jsx
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf';
import DownloadIcon from '@mui/icons-material/Download';
import { useQuotationPdf } from '../../hooks/useQuotationPdf';

// Dentro del componente:
const { openPdf, downloadPdf, loading: pdfLoading } = useQuotationPdf();

// En las acciones del modal:
<DialogActions sx={{ justifyContent: 'center', gap: 2, pb: 3 }}>
  <Button
    variant="outlined"
    startIcon={pdfLoading ? <CircularProgress size={16} /> : <PictureAsPdfIcon />}
    onClick={() => openPdf(createdQuotation.id)}
    disabled={pdfLoading}
  >
    Ver PDF
  </Button>
  <Button
    variant="contained"
    startIcon={<DownloadIcon />}
    onClick={() => downloadPdf(createdQuotation.id, createdQuotation.code)}
    disabled={pdfLoading}
  >
    Descargar PDF
  </Button>
  <Button variant="text" onClick={onClose}>
    Cerrar
  </Button>
</DialogActions>
```

---

## NOTAS ESPECIALES PARA ELECTRON

En Electron el `window.open` para PDFs puede no abrir el visor correcto según la configuración de la `BrowserWindow`. Usa esta alternativa nativa que escribe el PDF en un archivo temporal y lo abre con el visor del sistema operativo:

```javascript
// src/hooks/useQuotationPdf.js — versión Electron

import { useState } from 'react';
import { quotationService } from '../services/api/quotationService';
import { toast } from 'react-toastify';

// Asume que tienes expuesto window.electronAPI desde el preload con contextBridge
// Si no, usa require directamente si tienes nodeIntegration habilitado

export function useQuotationPdf() {
  const [loading, setLoading] = useState(false);

  const openPdf = async (id) => {
    setLoading(true);
    try {
      const blob        = await quotationService.getPdf(id);
      const arrayBuffer = await blob.arrayBuffer();
      const buffer      = Buffer.from(arrayBuffer);

      // Opción A — con contextBridge (recomendado)
      await window.electronAPI.openPdf(`cotizacion-${id}.pdf`, buffer);

      // Opción B — con nodeIntegration habilitado
      // const os   = require('os');
      // const fs   = require('fs');
      // const path = require('path');
      // const { shell } = require('electron');
      // const tempPath = path.join(os.tmpdir(), `cotizacion-${id}.pdf`);
      // fs.writeFileSync(tempPath, buffer);
      // shell.openPath(tempPath);
    } catch (err) {
      toast.error(err.message || 'No se pudo abrir el PDF');
    } finally {
      setLoading(false);
    }
  };

  const downloadPdf = async (id, code) => {
    setLoading(true);
    try {
      const blob        = await quotationService.getPdf(id);
      const arrayBuffer = await blob.arrayBuffer();
      const buffer      = Buffer.from(arrayBuffer);

      // Opción A — con contextBridge
      await window.electronAPI.savePdf(`${code || `cotizacion-${id}`}.pdf`, buffer);

      // Opción B — con nodeIntegration
      // const { dialog } = require('@electron/remote');
      // const savePath = await dialog.showSaveDialog({ defaultPath: `${code}.pdf` });
      // if (!savePath.canceled) fs.writeFileSync(savePath.filePath, buffer);
    } catch (err) {
      toast.error(err.message || 'No se pudo descargar el PDF');
    } finally {
      setLoading(false);
    }
  };

  return { openPdf, downloadPdf, loading };
}
```

### Configuración del `preload.js` (si usas contextBridge)

```javascript
// preload.js
const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electronAPI', {
  openPdf: (filename, buffer) => ipcRenderer.invoke('open-pdf', filename, buffer),
  savePdf: (filename, buffer) => ipcRenderer.invoke('save-pdf', filename, buffer),
});
```

### Handlers en el `main.js`

```javascript
const { ipcMain, shell, dialog } = require('electron');
const fs   = require('fs');
const path = require('path');
const os   = require('os');

// Abrir PDF con el visor del OS
ipcMain.handle('open-pdf', async (event, filename, buffer) => {
  const tempPath = path.join(os.tmpdir(), filename);
  fs.writeFileSync(tempPath, Buffer.from(buffer));
  await shell.openPath(tempPath);
});

// Guardar PDF con diálogo "Guardar como"
ipcMain.handle('save-pdf', async (event, filename, buffer) => {
  const result = await dialog.showSaveDialog({
    defaultPath: filename,
    filters: [{ name: 'PDF', extensions: ['pdf'] }]
  });
  if (!result.canceled && result.filePath) {
    fs.writeFileSync(result.filePath, Buffer.from(buffer));
  }
});
```

---

## RESUMEN DE ARCHIVOS A CREAR/MODIFICAR

| Acción    | Archivo                                              |
|-----------|------------------------------------------------------|
| Crear     | `src/hooks/useQuotationPdf.js`                       |
| Crear     | `src/components/quotations/QuotationPdfButtons.jsx`  |
| Modificar | `src/services/api/quotationService.js`               |
| Modificar | `src/pages/Quotations/QuotationDetailPage.jsx`       |
| Modificar | `src/pages/Quotations/QuotationListPage.jsx`         |
| Modificar | `src/components/quotations/QuotationQRModal.jsx`     |
| Modificar | `preload.js` *(solo si usas contextBridge)*          |
| Modificar | `main.js` *(solo si usas contextBridge)*             |
