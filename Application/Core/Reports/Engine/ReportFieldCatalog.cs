using Application.DTOs.Reports;

namespace Application.Core.Reports.Engine
{
    /// <summary>
    /// Catálogo estático de campos disponibles por tipo de reporte.
    /// Define qué datos puede mostrar el usuario en cada sección.
    /// </summary>
    public static class ReportFieldCatalog
    {
        private static readonly List<string> AllSections = new()
        {
            SectionType.Header, SectionType.Table, SectionType.Summary, SectionType.Footer
        };

        private static readonly List<string> HeaderSummaryFooter = new()
        {
            SectionType.Header, SectionType.Summary, SectionType.Footer
        };

        private static readonly List<string> TableOnly = new()
        {
            SectionType.Table
        };

        public static ReportFieldCatalogDto GetCatalog(string reportType)
        {
            return reportType switch
            {
                "Sales" or "Delivery" => GetSalesCatalog(reportType),
                "Quotation"           => GetQuotationCatalog(),
                "Purchase"            => GetPurchaseCatalog(),
                "Inventory"           => GetInventoryCatalog(),
                "CashierShift"        => GetCashierShiftCatalog(),
                "Invoice"             => GetInvoiceCatalog(),
                _                     => throw new ArgumentException($"Tipo de reporte desconocido: {reportType}")
            };
        }

        // ─────────────────────────────────────────────
        // VENTAS (POS y Delivery)
        // ─────────────────────────────────────────────
        private static ReportFieldCatalogDto GetSalesCatalog(string reportType) => new()
        {
            ReportType = reportType,
            AvailableSectionTypes = new() { SectionType.Header, SectionType.Table, SectionType.Summary, SectionType.Footer },
            Fields = new()
            {
                // Encabezado del documento
                F("saleCode",          "Código de venta",         FieldFormat.Text,       HeaderSummaryFooter),
                F("saleDate",          "Fecha de venta",          FieldFormat.DateTime,   HeaderSummaryFooter),
                F("saleType",          "Tipo de venta",           FieldFormat.Text,       HeaderSummaryFooter),
                F("status",            "Estado",                  FieldFormat.Text,       HeaderSummaryFooter),
                // Cliente
                F("customerName",      "Nombre del cliente",      FieldFormat.Text,       HeaderSummaryFooter),
                F("customerCode",      "Código del cliente",      FieldFormat.Text,       HeaderSummaryFooter),
                F("customerTaxId",     "RFC del cliente",         FieldFormat.Text,       HeaderSummaryFooter),
                // Vendedor / Empresa
                F("sellerName",        "Vendedor",                FieldFormat.Text,       HeaderSummaryFooter),
                F("warehouseName",     "Almacén",                 FieldFormat.Text,       HeaderSummaryFooter),
                F("branchName",        "Sucursal",                FieldFormat.Text,       HeaderSummaryFooter),
                F("companyName",       "Empresa",                 FieldFormat.Text,       HeaderSummaryFooter),
                // Solo Delivery
                F("deliveryAddress",   "Dirección de entrega",    FieldFormat.Text,       HeaderSummaryFooter, "Solo aplica a Delivery"),
                F("scheduledDate",     "Fecha programada",        FieldFormat.Date,       HeaderSummaryFooter, "Solo aplica a Delivery"),
                F("deliveredAt",       "Fecha de entrega real",   FieldFormat.DateTime,   HeaderSummaryFooter, "Solo aplica a Delivery"),
                // Notas
                F("notes",             "Notas",                   FieldFormat.Text,       HeaderSummaryFooter),
                // Columnas de tabla
                F("productCode",       "Código producto",         FieldFormat.Text,       TableOnly),
                F("productName",       "Descripción",             FieldFormat.Text,       TableOnly),
                F("quantity",          "Cantidad",                FieldFormat.Number,     TableOnly),
                F("unitPrice",         "Precio unitario",         FieldFormat.Currency,   TableOnly),
                F("discountPercentage","% Descuento",             FieldFormat.Percentage, TableOnly),
                F("discountAmount",    "Descuento $",             FieldFormat.Currency,   TableOnly),
                F("taxPercentage",     "% IVA",                   FieldFormat.Percentage, TableOnly),
                F("taxAmount",         "IVA $",                   FieldFormat.Currency,   TableOnly),
                F("subtotal",          "Subtotal línea",          FieldFormat.Currency,   TableOnly),
                F("lineTotal",         "Total línea",             FieldFormat.Currency,   TableOnly),
                // Totales (Summary)
                F("totalSubtotal",     "Subtotal",                FieldFormat.Currency,   HeaderSummaryFooter),
                F("totalDiscount",     "Descuento total",         FieldFormat.Currency,   HeaderSummaryFooter),
                F("totalTax",          "IVA total",               FieldFormat.Currency,   HeaderSummaryFooter),
                F("totalAmount",       "Total",                   FieldFormat.Currency,   HeaderSummaryFooter),
                F("amountPaid",        "Pagado",                  FieldFormat.Currency,   HeaderSummaryFooter),
                F("changeAmount",      "Cambio",                  FieldFormat.Currency,   HeaderSummaryFooter),
                F("paymentMethods",    "Forma(s) de pago",        FieldFormat.Text,       HeaderSummaryFooter),
            }
        };

        // ─────────────────────────────────────────────
        // COTIZACIONES
        // ─────────────────────────────────────────────
        private static ReportFieldCatalogDto GetQuotationCatalog() => new()
        {
            ReportType = "Quotation",
            AvailableSectionTypes = new() { SectionType.Header, SectionType.Table, SectionType.Summary, SectionType.Footer },
            Fields = new()
            {
                F("quotationCode",     "Código de cotización",    FieldFormat.Text,       HeaderSummaryFooter),
                F("quotationDate",     "Fecha",                   FieldFormat.DateTime,   HeaderSummaryFooter),
                F("validUntil",        "Válida hasta",            FieldFormat.Date,       HeaderSummaryFooter),
                F("status",            "Estado",                  FieldFormat.Text,       HeaderSummaryFooter),
                F("customerName",      "Nombre del cliente",      FieldFormat.Text,       HeaderSummaryFooter),
                F("customerTaxId",     "RFC del cliente",         FieldFormat.Text,       HeaderSummaryFooter),
                F("sellerName",        "Vendedor",                FieldFormat.Text,       HeaderSummaryFooter),
                F("warehouseName",     "Almacén",                 FieldFormat.Text,       HeaderSummaryFooter),
                F("branchName",        "Sucursal",                FieldFormat.Text,       HeaderSummaryFooter),
                F("companyName",       "Empresa",                 FieldFormat.Text,       HeaderSummaryFooter),
                F("notes",             "Notas",                   FieldFormat.Text,       HeaderSummaryFooter),
                F("convertedSaleCode", "Venta generada",          FieldFormat.Text,       HeaderSummaryFooter),
                // Tabla
                F("productCode",       "Código producto",         FieldFormat.Text,       TableOnly),
                F("productName",       "Descripción",             FieldFormat.Text,       TableOnly),
                F("quantity",          "Cantidad",                FieldFormat.Number,     TableOnly),
                F("unitPrice",         "Precio unitario",         FieldFormat.Currency,   TableOnly),
                F("discountPercentage","% Descuento",             FieldFormat.Percentage, TableOnly),
                F("discountAmount",    "Descuento $",             FieldFormat.Currency,   TableOnly),
                F("taxPercentage",     "% IVA",                   FieldFormat.Percentage, TableOnly),
                F("taxAmount",         "IVA $",                   FieldFormat.Currency,   TableOnly),
                F("lineTotal",         "Total línea",             FieldFormat.Currency,   TableOnly),
                // Totales
                F("totalSubtotal",     "Subtotal",                FieldFormat.Currency,   HeaderSummaryFooter),
                F("totalDiscount",     "Descuento total",         FieldFormat.Currency,   HeaderSummaryFooter),
                F("totalTax",          "IVA total",               FieldFormat.Currency,   HeaderSummaryFooter),
                F("totalAmount",       "Total",                   FieldFormat.Currency,   HeaderSummaryFooter),
            }
        };

        // ─────────────────────────────────────────────
        // COMPRAS
        // ─────────────────────────────────────────────
        private static ReportFieldCatalogDto GetPurchaseCatalog() => new()
        {
            ReportType = "Purchase",
            AvailableSectionTypes = new() { SectionType.Header, SectionType.Table, SectionType.Summary, SectionType.Footer },
            Fields = new()
            {
                F("poCode",            "Código de orden",         FieldFormat.Text,       HeaderSummaryFooter),
                F("poDate",            "Fecha",                   FieldFormat.DateTime,   HeaderSummaryFooter),
                F("status",            "Estado",                  FieldFormat.Text,       HeaderSummaryFooter),
                F("supplierName",      "Proveedor",               FieldFormat.Text,       HeaderSummaryFooter),
                F("supplierTaxId",     "RFC proveedor",           FieldFormat.Text,       HeaderSummaryFooter),
                F("warehouseName",     "Almacén destino",         FieldFormat.Text,       HeaderSummaryFooter),
                F("branchName",        "Sucursal",                FieldFormat.Text,       HeaderSummaryFooter),
                F("notes",             "Notas",                   FieldFormat.Text,       HeaderSummaryFooter),
                // Tabla
                F("productCode",       "Código producto",         FieldFormat.Text,       TableOnly),
                F("productName",       "Descripción",             FieldFormat.Text,       TableOnly),
                F("quantityOrdered",   "Cantidad pedida",         FieldFormat.Number,     TableOnly),
                F("quantityReceived",  "Cantidad recibida",       FieldFormat.Number,     TableOnly),
                F("unitCost",          "Costo unitario",          FieldFormat.Currency,   TableOnly),
                F("lineTotal",         "Total línea",             FieldFormat.Currency,   TableOnly),
                // Totales
                F("totalAmount",       "Total orden",             FieldFormat.Currency,   HeaderSummaryFooter),
            }
        };

        // ─────────────────────────────────────────────
        // INVENTARIO / KARDEX
        // ─────────────────────────────────────────────
        private static ReportFieldCatalogDto GetInventoryCatalog() => new()
        {
            ReportType = "Inventory",
            AvailableSectionTypes = new() { SectionType.Header, SectionType.Table, SectionType.Summary },
            Fields = new()
            {
                F("reportDate",        "Fecha del reporte",       FieldFormat.DateTime,   HeaderSummaryFooter),
                F("warehouseName",     "Almacén",                 FieldFormat.Text,       HeaderSummaryFooter),
                F("companyName",       "Empresa",                 FieldFormat.Text,       HeaderSummaryFooter),
                // Tabla
                F("productCode",       "Código",                  FieldFormat.Text,       TableOnly),
                F("productName",       "Producto",                FieldFormat.Text,       TableOnly),
                F("category",          "Categoría",               FieldFormat.Text,       TableOnly),
                F("currentStock",      "Stock actual",            FieldFormat.Number,     TableOnly),
                F("minimumStock",      "Stock mínimo",            FieldFormat.Number,     TableOnly),
                F("unitCost",          "Costo unitario",          FieldFormat.Currency,   TableOnly),
                F("totalValue",        "Valor total",             FieldFormat.Currency,   TableOnly),
                F("lastMovement",      "Último movimiento",       FieldFormat.DateTime,   TableOnly),
                // Resumen
                F("totalProducts",     "Total productos",         FieldFormat.Number,     HeaderSummaryFooter),
                F("totalStockValue",   "Valor total inventario",  FieldFormat.Currency,   HeaderSummaryFooter),
                F("belowMinimum",      "Productos bajo mínimo",   FieldFormat.Number,     HeaderSummaryFooter),
            }
        };

        // ─────────────────────────────────────────────
        // TURNO DE CAJERO
        // ─────────────────────────────────────────────
        private static ReportFieldCatalogDto GetCashierShiftCatalog() => new()
        {
            ReportType = "CashierShift",
            AvailableSectionTypes = new() { SectionType.Header, SectionType.Table, SectionType.Summary },
            Fields = new()
            {
                F("shiftCode",         "Código de turno",         FieldFormat.Text,       HeaderSummaryFooter),
                F("cashierName",       "Cajero",                  FieldFormat.Text,       HeaderSummaryFooter),
                F("openedAt",          "Apertura",                FieldFormat.DateTime,   HeaderSummaryFooter),
                F("closedAt",          "Cierre",                  FieldFormat.DateTime,   HeaderSummaryFooter),
                F("warehouseName",     "Almacén",                 FieldFormat.Text,       HeaderSummaryFooter),
                F("openingCash",       "Fondo inicial",           FieldFormat.Currency,   HeaderSummaryFooter),
                F("closingCash",       "Efectivo final",          FieldFormat.Currency,   HeaderSummaryFooter),
                F("difference",        "Diferencia",              FieldFormat.Currency,   HeaderSummaryFooter),
                // Tabla de ventas del turno
                F("saleCode",          "Código venta",            FieldFormat.Text,       TableOnly),
                F("saleTime",          "Hora",                    FieldFormat.DateTime,   TableOnly),
                F("customerName",      "Cliente",                 FieldFormat.Text,       TableOnly),
                F("paymentMethod",     "Forma de pago",           FieldFormat.Text,       TableOnly),
                F("saleTotal",         "Total",                   FieldFormat.Currency,   TableOnly),
                // Resumen por método de pago
                F("cashTotal",         "Total efectivo",          FieldFormat.Currency,   HeaderSummaryFooter),
                F("cardTotal",         "Total tarjeta",           FieldFormat.Currency,   HeaderSummaryFooter),
                F("transferTotal",     "Total transferencia",     FieldFormat.Currency,   HeaderSummaryFooter),
                F("totalSales",        "Total ventas",            FieldFormat.Currency,   HeaderSummaryFooter),
                F("salesCount",        "Número de ventas",        FieldFormat.Number,     HeaderSummaryFooter),
            }
        };

        // ─────────────────────────────────────────────
        // FACTURA CFDI
        // ─────────────────────────────────────────────
        private static ReportFieldCatalogDto GetInvoiceCatalog() => new()
        {
            ReportType = "Invoice",
            AvailableSectionTypes = new() { SectionType.Header, SectionType.Table, SectionType.Summary, SectionType.Footer },
            Fields = new()
            {
                // Comprobante
                F("invoiceFolio",       "Folio",                    FieldFormat.Text,       HeaderSummaryFooter),
                F("invoiceSerie",       "Serie",                    FieldFormat.Text,       HeaderSummaryFooter),
                F("invoiceDate",        "Fecha del comprobante",    FieldFormat.DateTime,   HeaderSummaryFooter),
                F("invoiceStatus",      "Estado",                   FieldFormat.Text,       HeaderSummaryFooter),
                F("qrCode",             "Código QR SAT",            FieldFormat.Image,      HeaderSummaryFooter),
                F("uuid",               "UUID / Folio Fiscal",      FieldFormat.Text,       HeaderSummaryFooter),
                F("timbradoAt",         "Fecha timbrado",           FieldFormat.DateTime,   HeaderSummaryFooter),
                F("tipoDeComprobante",  "Tipo comprobante",         FieldFormat.Text,       HeaderSummaryFooter),
                F("metodoPago",         "Método de pago (SAT)",     FieldFormat.Text,       HeaderSummaryFooter),
                F("formaPago",          "Forma de pago (SAT)",      FieldFormat.Text,       HeaderSummaryFooter),
                F("condicionesDePago",  "Condiciones de pago",      FieldFormat.Text,       HeaderSummaryFooter),
                F("moneda",             "Moneda",                   FieldFormat.Text,       HeaderSummaryFooter),
                F("lugarExpedicion",    "Lugar de expedición (CP)", FieldFormat.Text,       HeaderSummaryFooter),
                // Emisor
                F("emisorRfc",          "RFC emisor",               FieldFormat.Text,       HeaderSummaryFooter),
                F("emisorNombre",       "Razón social emisor",      FieldFormat.Text,       HeaderSummaryFooter),
                F("emisorRegimenFiscal","Régimen fiscal emisor",    FieldFormat.Text,       HeaderSummaryFooter),
                // Receptor
                F("receptorRfc",        "RFC receptor",             FieldFormat.Text,       HeaderSummaryFooter),
                F("receptorNombre",     "Razón social receptor",    FieldFormat.Text,       HeaderSummaryFooter),
                F("receptorDomicilioFiscal","CP domicilio fiscal",  FieldFormat.Text,       HeaderSummaryFooter),
                F("receptorRegimenFiscal",  "Régimen fiscal receptor", FieldFormat.Text,    HeaderSummaryFooter),
                F("receptorUsoCfdi",    "Uso CFDI",                 FieldFormat.Text,       HeaderSummaryFooter),
                // Venta origen
                F("saleCode",           "N° de venta origen",       FieldFormat.Text,       HeaderSummaryFooter),
                // Conceptos (tabla)
                F("claveProdServ",      "Clave SAT prod/serv",      FieldFormat.Text,       TableOnly),
                F("noIdentificacion",   "N° identificación",        FieldFormat.Text,       TableOnly),
                F("descripcion",        "Descripción",              FieldFormat.Text,       TableOnly),
                F("cantidad",           "Cantidad",                 FieldFormat.Number,     TableOnly),
                F("claveUnidad",        "Clave unidad",             FieldFormat.Text,       TableOnly),
                F("unidad",             "Unidad",                   FieldFormat.Text,       TableOnly),
                F("valorUnitario",      "Valor unitario",           FieldFormat.Currency,   TableOnly),
                F("descuento",          "Descuento",                FieldFormat.Currency,   TableOnly),
                F("importe",            "Importe",                  FieldFormat.Currency,   TableOnly),
                F("trasladoTasa",       "Tasa IVA",                 FieldFormat.Percentage, TableOnly),
                F("trasladoImporte",    "IVA $",                    FieldFormat.Currency,   TableOnly),
                // Totales
                F("subTotal",           "Subtotal",                 FieldFormat.Currency,   HeaderSummaryFooter),
                F("discountAmount",     "Descuento total",          FieldFormat.Currency,   HeaderSummaryFooter),
                F("taxAmount",          "Impuestos trasladados",    FieldFormat.Currency,   HeaderSummaryFooter),
                F("total",              "Total",                    FieldFormat.Currency,   HeaderSummaryFooter),
            }
        };

        // ─────────────────────────────────────────────
        // Helper
        // ─────────────────────────────────────────────
        private static FieldDefinition F(
            string key, string label, string format,
            List<string> sections, string? description = null) => new()
        {
            Key = key,
            Label = label,
            DefaultFormat = format,
            ApplicableSections = sections,
            Description = description
        };
    }
}
