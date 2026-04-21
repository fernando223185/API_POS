using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// Inserta plantillas de reporte por defecto si aún no existen en la base de datos.
    /// Se llama una sola vez al arrancar la aplicación, después de aplicar migraciones.
    /// </summary>
    public static class ReportTemplateSeed
    {
        public static async Task SeedDefaultTemplatesAsync(POSDbContext context)
        {
            var types = new[]
            {
                ("Sales",        "Ticket / Factura de Venta", BuildSalesTemplate()),
                ("Purchase",     "Orden de Compra / Recibo",  BuildPurchaseTemplate()),
                ("CashierShift", "Corte de Caja",             BuildCashierShiftTemplate()),
                ("Inventory",    "Kardex de Inventario",      BuildInventoryTemplate()),
                ("Invoice",      "Factura CFDI",              BuildInvoiceTemplate()),
                ("Quotation",    "Cotización",                BuildQuotationTemplate()),
                ("Delivery",     "Nota de Entrega",           BuildDeliveryTemplate()),
            };

            bool changed = false;
            foreach (var (type, name, sectionsJson) in types)
            {
                var existing = await context.ReportTemplates
                    .FirstOrDefaultAsync(t => t.ReportType == type && t.IsDefault && t.CompanyId == null);

                if (existing == null)
                {
                    context.ReportTemplates.Add(new ReportTemplate
                    {
                        Name        = name,
                        ReportType  = type,
                        IsDefault   = true,
                        IsActive    = true,
                        SectionsJson = sectionsJson,
                        Description = $"Plantilla base editable inspirada en el layout legacy para reportes de tipo {type}.",
                        CompanyId   = null,
                    });
                    changed = true;
                    Console.WriteLine($"   ✅ Plantilla por defecto creada para tipo: {type}");
                }
                else if (existing.Name.StartsWith("Plantilla por defecto", StringComparison.OrdinalIgnoreCase)
                      || (existing.Description?.Contains("generada automáticamente por el sistema", StringComparison.OrdinalIgnoreCase) ?? false)
                      || (existing.Description?.Contains("layout legacy", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    existing.Name = name;
                    existing.SectionsJson = sectionsJson;
                    existing.Description = $"Plantilla base editable inspirada en el layout legacy para reportes de tipo {type}.";
                    existing.IsActive = true;
                    changed = true;
                    Console.WriteLine($"   ✅ Plantilla por defecto actualizada para tipo: {type}");
                }
            }

            if (changed)
                await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────
        // VENTAS
        // ─────────────────────────────────────────────

        private static string BuildSalesTemplate() => Json(new[]
        {
            Header("Documento", 1, new[]
            {
                F("companyName",    "Empresa",      bold: true),
                F("saleCode",       "Folio",        bold: true, inline: true),
                F("saleDate",       "Fecha",        fmt: "datetime"),
                F("warehouseName",  "Almacén",      inline: true),
                F("sellerName",     "Vendedor"),
                F("status",         "Estado",       inline: true),
            }, titleBg: "#1f4b99", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate"),
            Header("Cliente", 2, new[]
            {
                F("customerName",   "Cliente",      bold: true),
                F("customerTaxId",  "RFC",          inline: true),
                F("paymentMethods", "Forma de pago"),
                F("notes",          "Notas"),
            }, titleBg: "#2f66c2", bodyBg: "#f7faff", border: "#c8d9f6", variant: "corporate"),
            Table("Detalle de Productos", 3, new[]
            {
                C("productCode",  "Cód.",        60),
                C("productName",  "Descripción", 0),
                C("quantity",     "Cant.",        50,  fmt: "number",   align: "center"),
                C("unitPrice",    "Precio U.",    70,  fmt: "currency", align: "right"),
                C("discountPercentage","Desc %",   60,  fmt: "percentage", align: "right"),
                C("lineTotal",    "Total",        70,  fmt: "currency", align: "right"),
            }, titleBg: "#1f4b99", bodyBg: "#f7faff", border: "#c8d9f6", variant: "corporate"),
            Footer("Totales", 4, new[]
            {
                F("totalSubtotal", "Subtotal",  fmt: "currency", bold: true, inline: false),
                F("totalDiscount", "Descuento", fmt: "currency", inline: true),
                F("totalTax",      "IVA",       fmt: "currency", inline: false),
                F("totalAmount",   "Total",     fmt: "currency", bold: true, inline: true),
                F("amountPaid",    "Pagado",    fmt: "currency", inline: false),
                F("changeAmount",  "Cambio",    fmt: "currency", inline: true),
            }, titleBg: "#123a7a", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate"),
        });

        // ─────────────────────────────────────────────
        // COMPRAS (Orden de compra Y recibo de mercancía comparten la plantilla)
        // ─────────────────────────────────────────────

        private static string BuildPurchaseTemplate() => Json(new[]
        {
            Header("Documento de Compra", 1, new[]
            {
                F("poCode",         "Orden de Compra"),
                F("poDate",         "Fecha OC",        fmt: "date", inline: true),
                F("receivingCode",  "Folio Recibo"),
                F("receivingDate",  "Fecha Recibo",    fmt: "date", inline: true),
                F("status",         "Estado"),
                F("warehouseName",  "Almacén",         inline: true),
                F("supplierName",   "Proveedor",       bold: true),
                F("supplierTaxId",  "RFC Proveedor",   inline: true),
                F("warehouseName",  "Almacén"),
                F("supplierInvoiceNumber", "Factura Proveedor"),
                F("carrierName",    "Transportista",   inline: true),
                F("trackingNumber", "Guía",            inline: true),
                F("receivedBy",     "Recibió"),
                F("notes",          "Notas"),
            }, titleBg: "#8c4b16", bodyBg: "#fff7ef", border: "#f1cfaa", variant: "warm"),
            Table("Detalle de Partidas", 2, new[]
            {
                C("productCode",       "Cód.",           60),
                C("productName",       "Descripción",    0),
                C("quantityOrdered",   "Ord.",           50,  fmt: "number",   align: "center"),
                C("quantityReceived",  "Recibido",       60,  fmt: "number",   align: "center"),
                C("quantityApproved",  "Aprobado",       60,  fmt: "number",   align: "center"),
                C("quantityRejected",  "Rechazado",      60,  fmt: "number",   align: "center"),
                C("unitCost",          "Costo Unit.",    70,  fmt: "currency", align: "right"),
                C("lineTotal",         "Total",          70,  fmt: "currency", align: "right"),
            }, titleBg: "#b5631b", bodyBg: "#fffaf4", border: "#f1cfaa", variant: "warm"),
            Footer("Total de la Orden", 3, new[]
            {
                F("totalAmount", "Total", fmt: "currency", bold: true),
                F("notes",       "Notas"),
            }, titleBg: "#7c3d12", bodyBg: "#fff7ef", border: "#f1cfaa", variant: "warm"),
        });

        // ─────────────────────────────────────────────
        // CORTE DE CAJA
        // ─────────────────────────────────────────────

        private static string BuildCashierShiftTemplate() => Json(new[]
        {
            Header("Resumen del Turno", 1, new[]
            {
                F("shiftCode",    "Folio Turno"),
                F("cashierName",  "Cajero",        bold: true,  inline: true),
                F("warehouseName","Caja / Sucursal"),
                F("openedAt",     "Apertura",      fmt: "datetime", inline: false),
                F("closedAt",     "Cierre",        fmt: "datetime", inline: true),
            }, titleBg: "#1a6e5a", bodyBg: "#edf9f5", border: "#bde3d8", variant: "fresh"),
            Table("Ventas del Turno", 2, new[]
            {
                C("saleCode",     "Folio",       80),
                C("saleTime",     "Hora",        70,  fmt: "text", align: "center"),
                C("customerName", "Cliente",     0),
                C("paymentMethod","Pago",        60,  align: "center"),
                C("saleTotal",    "Total",       70,  fmt: "currency", align: "right"),
            }, titleBg: "#0f7b62", bodyBg: "#f5fcfa", border: "#bde3d8", variant: "fresh"),
            Footer("Resumen del Turno", 3, new[]
            {
                F("salesCount",    "N° Ventas",   fmt: "number",   bold: true),
                F("totalSales",    "Total Ventas",fmt: "currency", bold: true, inline: true),
                F("cashTotal",     "Efectivo",    fmt: "currency", inline: false),
                F("cardTotal",     "Tarjeta",     fmt: "currency", inline: true),
                F("transferTotal", "Transferencia",fmt: "currency"),
                F("openingCash",   "Fondo Inicial",fmt: "currency", inline: false),
                F("closingCash",   "Fondo Final",  fmt: "currency", inline: true),
                F("difference",    "Diferencia",   fmt: "currency", bold: true),
            }, titleBg: "#145347", bodyBg: "#edf9f5", border: "#bde3d8", variant: "fresh"),
        });

        // ─────────────────────────────────────────────
        // KARDEX / INVENTARIO
        // ─────────────────────────────────────────────

        private static string BuildInventoryTemplate() => Json(new[]
        {
            Header("Resumen del Kardex", 1, new[]
            {
                F("reportDate",    "Fecha Reporte", fmt: "datetime"),
                F("warehouseName", "Almacén",       bold: true,  inline: true),
                F("productName",   "Producto",      bold: true),
                F("fromDate",      "Del",           fmt: "date", inline: true),
                F("toDate",        "Al",            fmt: "date", inline: true),
            }, titleBg: "#5a3f9e", bodyBg: "#f3efff", border: "#d7cbff", variant: "technical"),
            Table("Movimientos de Inventario", 2, new[]
            {
                C("movementDate",  "Fecha",       80,  fmt: "datetime", align: "center"),
                C("movementCode",  "Folio",       70),
                C("movementType",  "Tipo",        90),
                C("productCode",   "Cód.",        55),
                C("productName",   "Producto",    0),
                C("quantity",      "Cantidad",    55,  fmt: "number",   align: "right"),
                C("stockBefore",   "Antes",       55,  fmt: "number",   align: "right"),
                C("stockAfter",    "Después",     55,  fmt: "number",   align: "right"),
                C("unitCost",      "Costo U.",    65,  fmt: "currency", align: "right"),
                C("totalCost",     "Costo Total", 70,  fmt: "currency", align: "right"),
                C("reference",     "Referencia",  80),
            }, titleBg: "#4d35a2", bodyBg: "#faf8ff", border: "#d7cbff", variant: "technical"),
        });

        // ─────────────────────────────────────────────
        // FACTURA CFDI
        // ─────────────────────────────────────────────

        private static string BuildInvoiceTemplate() => Json(new[]
        {
            Header("Comprobante", 1, new[]
            {
                F("emisorNombre",        "Emisor",            bold: true),
                F("invoiceSerie",        "Serie",             inline: true),
                F("emisorRfc",           "RFC",               inline: false),
                F("invoiceFolio",        "Folio",             bold: true, inline: true),
                F("emisorRegimenFiscal", "Régimen fiscal",    inline: false),
                F("invoiceDate",         "Fecha",             fmt: "datetime", inline: true),
                F("lugarExpedicion",     "Lugar expedición",  inline: false),
                F("tipoDeComprobante",   "Tipo",              inline: true),
                F("invoiceStatus",       "Estado"),
                F("moneda",              "Moneda",            inline: true),
                F("tipoCambio",          "Tipo de cambio",    inline: true),
            }, titleBg: "#204a87", bodyBg: "#f3f7fd", border: "#b8cce8", variant: "fiscal"),
            Header("Receptor", 2, new[]
            {
                F("receptorRfc",              "RFC"),
                F("receptorRegimenFiscal",    "Régimen", inline: true),
                F("receptorUsoCfdi",          "Uso CFDI", inline: true),
                F("receptorNombre",           "Razón social", bold: true),
                F("receptorDomicilioFiscal",  "Domicilio fiscal", inline: true),
            }, titleBg: "#204a87", bodyBg: "#f7f9fc", border: "#b8cce8", variant: "fiscal"),
            Table("Conceptos", 3, new[]
            {
                C("claveProdServ",  "Clave SAT",   70),
                C("noIdentificacion","No. Ident.",  70),
                C("descripcion",    "Descripción", 0),
                C("cantidad",       "Cant.",        45,  fmt: "number",   align: "center"),
                C("claveUnidad",    "Unidad",       45,  align: "center"),
                C("valorUnitario",  "V. Unit.",     70,  fmt: "currency", align: "right"),
                C("descuento",      "Desc.",        60,  fmt: "currency", align: "right"),
                C("importe",        "Importe",      70,  fmt: "currency", align: "right"),
                C("trasladoImporte","IVA",          65,  fmt: "currency", align: "right"),
            }, titleBg: "#264b7f", bodyBg: "#ffffff", border: "#b8cce8", variant: "fiscal"),
            Footer("Pago y Totales", 4, new[]
            {
                F("formaPago",      "Forma de pago"),
                F("metodoPago",     "Método de pago", inline: true),
                F("condicionesDePago","Condiciones de pago"),
                F("moneda",         "Moneda", inline: true),
                F("tipoCambio",     "Tipo de cambio", inline: true),
                F("subTotal",       "Subtotal",  fmt: "currency", bold: true, inline: false),
                F("discountAmount", "Descuento", fmt: "currency", inline: true),
                F("taxAmount",      "IVA",       fmt: "currency", inline: false),
                F("total",          "Total",     fmt: "currency", bold: true, inline: true),
            }, titleBg: "#163a68", bodyBg: "#f3f7fd", border: "#b8cce8", variant: "fiscal"),
            Footer("Datos de Timbrado", 5, new[]
            {
                F("uuid",                "UUID", bold: true),
                F("timbradoAt",          "Fecha timbrado", fmt: "datetime", inline: true),
                F("noCertificadoCfdi",   "No. Cert. CFDI"),
                F("noCertificadoSat",    "No. Cert. SAT", inline: true),
                F("selloCfdi",           "Sello CFDI"),
                F("selloSat",            "Sello SAT"),
                F("cadenaOriginalSat",   "Cadena original SAT"),
                F("qrCode",              "Código QR", fmt: "image"),
            }, titleBg: "#204a87", bodyBg: "#ffffff", border: "#b8cce8", variant: "fiscal"),
        });

        // ─────────────────────────────────────────────
        // COTIZACIÓN
        // ─────────────────────────────────────────────

        private static string BuildQuotationTemplate() => Json(new[]
        {
            Header("Encabezado de Cotización", 1, new[]
            {
                F("quotationCode", "Folio"),
                F("quotationDate", "Fecha",       fmt: "date",  inline: true),
                F("validUntil",    "Válida hasta",fmt: "date",  inline: true),
                F("companyName",   "Empresa",     bold: true),
                F("branchName",    "Sucursal",    inline: true),
                F("customerName",  "Cliente",     bold: true),
                F("customerTaxId", "RFC",         inline: true),
                F("sellerName",    "Vendedor"),
                F("status",        "Estatus",     inline: true),
                F("notes",         "Notas"),
            }, titleBg: "#8d2f5a", bodyBg: "#fff1f7", border: "#f0bfd3", variant: "proposal"),
            Table("Partidas Cotizadas", 2, new[]
            {
                C("productCode",  "Cód.",        60),
                C("productName",  "Descripción", 0),
                C("quantity",     "Cant.",        50,  fmt: "number",   align: "center"),
                C("unitPrice",    "P. Unit.",     70,  fmt: "currency", align: "right"),
                C("discountAmount","Desc.",        60,  fmt: "currency", align: "right"),
                C("taxAmount",    "IVA",          60,  fmt: "currency", align: "right"),
                C("lineTotal",    "Total",        70,  fmt: "currency", align: "right"),
            }, titleBg: "#aa3a6d", bodyBg: "#fff8fb", border: "#f0bfd3", variant: "proposal"),
            Footer("Totales", 3, new[]
            {
                F("totalSubtotal", "Subtotal",  fmt: "currency", bold: true, inline: false),
                F("totalDiscount", "Descuento", fmt: "currency", inline: true),
                F("totalTax",      "IVA",       fmt: "currency", inline: false),
                F("totalAmount",   "Total",     fmt: "currency", bold: true, inline: true),
            }, titleBg: "#702143", bodyBg: "#fff1f7", border: "#f0bfd3", variant: "proposal"),
        });

        // ─────────────────────────────────────────────
        // ENTREGA (Delivery)
        // ─────────────────────────────────────────────

        private static string BuildDeliveryTemplate() => Json(new[]
        {
            Header("Entrega / Remisión", 1, new[]
            {
                F("saleCode",        "Folio Venta"),
                F("saleDate",        "Fecha Venta",    fmt: "date",     inline: true),
                F("scheduledDate",   "Entrega Programada", fmt: "date", inline: false),
                F("deliveredAt",     "Entregado el",   fmt: "datetime", inline: true),
                F("companyName",     "Empresa",        bold: true),
                F("branchName",      "Sucursal",       inline: true),
                F("customerName",    "Cliente",        bold: true),
                F("customerTaxId",   "RFC",            inline: true),
                F("sellerName",      "Vendedor"),
                F("deliveryAddress", "Dirección de Entrega", bold: true),
                F("status",          "Estatus",        inline: true),
                F("notes",           "Notas"),
            }, titleBg: "#b04b2a", bodyBg: "#fff3ee", border: "#f2c8b9", variant: "delivery"),
            Table("Productos a Entregar", 2, new[]
            {
                C("productCode",  "Cód.",        60),
                C("productName",  "Descripción", 0),
                C("quantity",     "Cant.",        50,  fmt: "number",   align: "center"),
                C("unitPrice",    "P. Unit.",     70,  fmt: "currency", align: "right"),
                C("lineTotal",    "Total",        70,  fmt: "currency", align: "right"),
            }, titleBg: "#cb5c36", bodyBg: "#fffaf7", border: "#f2c8b9", variant: "delivery"),
            Footer("Totales", 3, new[]
            {
                F("totalSubtotal", "Subtotal",  fmt: "currency", bold: true, inline: false),
                F("totalDiscount", "Descuento", fmt: "currency", inline: true),
                F("totalTax",      "IVA",       fmt: "currency", inline: false),
                F("totalAmount",   "Total",     fmt: "currency", bold: true, inline: true),
            }, titleBg: "#8b3d23", bodyBg: "#fff3ee", border: "#f2c8b9", variant: "delivery"),
        });

        // ─────────────────────────────────────────────
        // Helpers JSON
        // ─────────────────────────────────────────────

        private static string Json(object sections) =>
            JsonSerializer.Serialize(sections, new JsonSerializerOptions { WriteIndented = false });

        private static object Header(string title, int order, object[] fields,
            string? titleBg = null, string? bodyBg = null, string? border = null, string? variant = null) =>
            new { type = "Header", title, order, showTitle = true, fields, columns = Array.Empty<object>(), titleBackground = titleBg, titleColor = "#ffffff", bodyBackground = bodyBg, borderColor = border, variant };

        private static object Table(string title, int order, object[] columns,
            string? titleBg = null, string? bodyBg = null, string? border = null, string? variant = null) =>
            new { type = "Table", title, order, showTitle = true, fields = Array.Empty<object>(), columns, titleBackground = titleBg, titleColor = "#ffffff", bodyBackground = bodyBg, borderColor = border, variant };

        private static object Footer(string title, int order, object[] fields,
            string? titleBg = null, string? bodyBg = null, string? border = null, string? variant = null) =>
            new { type = "Footer", title, order, showTitle = true, fields, columns = Array.Empty<object>(), titleBackground = titleBg, titleColor = "#ffffff", bodyBackground = bodyBg, borderColor = border, variant };

        private static object F(string field, string label, string fmt = "text", bool bold = false,
                                string align = "left", bool inline = false) =>
            new { field, label, bold, fontSize = 9, align, format = fmt, inline };

        private static object C(string field, string label, int width = 0, string fmt = "text",
                                string align = "left", bool bold = false) =>
            new { field, label, width, align, format = fmt, bold };
    }
}
