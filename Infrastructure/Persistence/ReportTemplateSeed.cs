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
            // Mapa: tipo â†’ (nombre, htmlTemplate)
            var types = new[]
            {
                ("Sales",        "Ticket / Factura de Venta",    HtmlReportTemplates.Sales,        BuildSalesTemplate()),
                ("Purchase",     "Orden de Compra / Recibo",      HtmlReportTemplates.Purchase,     BuildPurchaseTemplate()),
                ("CashierShift", "Corte de Caja",                 HtmlReportTemplates.CashierShift, BuildCashierShiftTemplate()),
                ("Inventory",    "Kardex de Inventario",          HtmlReportTemplates.Inventory,    BuildInventoryTemplate()),
                ("Invoice",      "Factura CFDI",                  HtmlReportTemplates.Invoice,      BuildInvoiceTemplate()),
                ("Payment",      "Complemento de Pago CFDI",      HtmlReportTemplates.Payment,      BuildPaymentTemplate()),
                ("Quotation",    "Cotización",                    HtmlReportTemplates.Quotation,    BuildQuotationTemplate()),
                ("Delivery",     "Nota de Entrega",               HtmlReportTemplates.Delivery,     BuildDeliveryTemplate()),
                ("WarehouseTransferDispatch",   "Traspaso de Almacén — Despacho/Salida",   HtmlReportTemplates.WarehouseTransferDispatch,   BuildWarehouseTransferDispatchTemplate()),
                ("WarehouseTransferReceiving",  "Traspaso de Almacén — Recepción/Entrada", HtmlReportTemplates.WarehouseTransferReceiving,  BuildWarehouseTransferReceivingTemplate()),
            };

            bool changed = false;
            foreach (var (type, name, htmlTemplate, sectionsJson) in types)
            {
                // Obtener TODOS los registros de este tipo (todas las compañías)
                var allOfType = await context.ReportTemplates
                    .Where(t => t.ReportType == type)
                    .ToListAsync();

                var defaultRecord = allOfType.FirstOrDefault(t => t.IsDefault && t.CompanyId == null);

                // Crear el registro global default si no existe
                if (defaultRecord == null)
                {
                    context.ReportTemplates.Add(new ReportTemplate
                    {
                        Name         = name,
                        ReportType   = type,
                        IsDefault    = true,
                        IsActive     = true,
                        SectionsJson = sectionsJson,
                        HtmlTemplate = htmlTemplate,
                        Description  = $"Plantilla HTML editable con motor Playwright para reportes de tipo {type}.",
                        CompanyId    = null,
                    });
                    changed = true;
                    Console.WriteLine($"   ✅ Plantilla HTML creada para tipo: {type}");
                }

                // Actualizar TODOS los registros que no tienen los nuevos selectores CSS
                foreach (var record in allOfType)
                {
                    if (record.HtmlTemplate == null)
                    {
                        record.HtmlTemplate = htmlTemplate;
                        record.SectionsJson = sectionsJson;
                        record.Name         = name;
                        record.Description  = $"Plantilla HTML editable con motor Playwright para reportes de tipo {type}.";
                        changed = true;
                        Console.WriteLine($"   ✅ Plantilla migrada a HTML: tipo={type} id={record.Id}");
                    }
                    else if (record.SectionsJson == "[]" || !record.HtmlTemplate.Contains("id=\"\"sec-"))
                    {
                        record.HtmlTemplate = htmlTemplate;
                        record.SectionsJson = sectionsJson;
                        changed = true;
                        Console.WriteLine($"   ✅ HTML y SectionsJson actualizados: tipo={type} id={record.Id}");
                    }
                }
            }

            if (changed)
                await context.SaveChangesAsync();
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // VENTAS
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
                // Campos disponibles (ocultos por defecto)
                F("saleType",       "Tipo de venta",                visible: false),
                F("branchName",     "Sucursal",                     visible: false, inline: true),
            }, titleBg: "#1f4b99", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate", sectionId: "doc"),
            Header("Cliente", 2, new[]
            {
                F("customerName",   "Cliente",      bold: true),
                F("customerTaxId",  "RFC",          inline: true),
                F("paymentMethods", "Forma de pago"),
                F("notes",          "Notas"),
                // Campos disponibles (ocultos por defecto)
                F("customerCode",   "Código cliente",               visible: false),
            }, titleBg: "#2f66c2", bodyBg: "#f7faff", border: "#c8d9f6", variant: "corporate", sectionId: "cliente"),
            Table("Detalle de Productos", 3, new[]
            {
                C("productCode",  "Cód.",        60),
                C("productName",  "Descripción", 0),
                C("quantity",     "Cant.",        50,  fmt: "number",   align: "center"),
                C("unitPrice",    "Precio U.",    70,  fmt: "currency", align: "right"),
                C("discountPercentage","Desc %",   60,  fmt: "percentage", align: "right"),
                C("lineTotal",    "Total",        70,  fmt: "currency", align: "right"),
                // Columnas disponibles (ocultas por defecto)
                C("discountAmount",   "Descuento $",    65,  fmt: "currency",    align: "right",  visible: false),
                C("taxPercentage",    "% IVA",          55,  fmt: "percentage",  align: "right",  visible: false),
                C("taxAmount",        "IVA $",          65,  fmt: "currency",    align: "right",  visible: false),
                C("subtotal",         "Subtotal",       70,  fmt: "currency",    align: "right",  visible: false),
            }, titleBg: "#1f4b99", bodyBg: "#f7faff", border: "#c8d9f6", variant: "corporate", sectionId: "productos"),
            Footer("Totales", 4, new[]
            {
                F("totalSubtotal", "Subtotal",  fmt: "currency", bold: true, inline: false),
                F("totalDiscount", "Descuento", fmt: "currency", inline: true),
                F("totalTax",      "IVA",       fmt: "currency", inline: false),
                F("totalAmount",   "Total",     fmt: "currency", bold: true, inline: true),
                F("amountPaid",    "Pagado",    fmt: "currency", inline: false),
                F("changeAmount",  "Cambio",    fmt: "currency", inline: true),
            }, titleBg: "#123a7a", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate", sectionId: "totales"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // COMPRAS (Orden de compra Y recibo de mercancía comparten la plantilla)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
                // Campos disponibles (ocultos por defecto)
                F("branchName",     "Sucursal",                     visible: false),
            }, titleBg: "#8c4b16", bodyBg: "#fff7ef", border: "#f1cfaa", variant: "warm", sectionId: "doc"),
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
            }, titleBg: "#b5631b", bodyBg: "#fffaf4", border: "#f1cfaa", variant: "warm", sectionId: "partidas"),
            Footer("Total de la Orden", 3, new[]
            {
                F("totalAmount", "Total", fmt: "currency", bold: true),
                F("notes",       "Notas"),
            }, titleBg: "#7c3d12", bodyBg: "#fff7ef", border: "#f1cfaa", variant: "warm", sectionId: "totales"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // CORTE DE CAJA
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static string BuildCashierShiftTemplate() => Json(new[]
        {
            Header("Resumen del Turno", 1, new[]
            {
                F("shiftCode",    "Folio Turno"),
                F("cashierName",  "Cajero",        bold: true,  inline: true),
                F("warehouseName","Caja / Sucursal"),
                F("openedAt",     "Apertura",      fmt: "datetime", inline: false),
                F("closedAt",     "Cierre",        fmt: "datetime", inline: true),
                // Campos disponibles (ocultos por defecto)
                F("branchName",   "Sucursal",                     visible: false, inline: true),
            }, titleBg: "#1a6e5a", bodyBg: "#edf9f5", border: "#bde3d8", variant: "fresh", sectionId: "encabezado"),
            Table("Ventas del Turno", 2, new[]
            {
                C("saleCode",     "Folio",       80),
                C("saleTime",     "Hora",        70,  fmt: "text", align: "center"),
                C("customerName", "Cliente",     0),
                C("paymentMethod","Pago",        60,  align: "center"),
                C("saleTotal",    "Total",       70,  fmt: "currency", align: "right"),
            }, titleBg: "#0f7b62", bodyBg: "#f5fcfa", border: "#bde3d8", variant: "fresh", sectionId: "ventas"),
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
            }, titleBg: "#145347", bodyBg: "#edf9f5", border: "#bde3d8", variant: "fresh", sectionId: "totales"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // KARDEX / INVENTARIO
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static string BuildInventoryTemplate() => Json(new[]
        {
            Header("Resumen del Kardex", 1, new[]
            {
                F("reportDate",    "Fecha Reporte", fmt: "datetime"),
                F("warehouseName", "Almacén",       bold: true,  inline: true),
                F("productName",   "Producto",      bold: true),
                F("fromDate",      "Del",           fmt: "date", inline: true),
                F("toDate",        "Al",            fmt: "date", inline: true),
                // Campos disponibles (ocultos por defecto)
                F("companyName",   "Empresa",                      visible: false),
            }, titleBg: "#5a3f9e", bodyBg: "#f3efff", border: "#d7cbff", variant: "technical", sectionId: "kardex"),
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
            }, titleBg: "#4d35a2", bodyBg: "#faf8ff", border: "#d7cbff", variant: "technical", sectionId: "movimientos"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // FACTURA CFDI
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
                // Campos disponibles (ocultos por defecto)
                F("saleCode",            "N° venta origen",              visible: false),
                F("qrCode",              "Código QR SAT (encabezado)",   fmt: "image", visible: false),
            }, titleBg: "#204a87", bodyBg: "#f3f7fd", border: "#b8cce8", variant: "fiscal", sectionId: "comprobante"),
            Header("Receptor", 2, new[]
            {
                F("receptorRfc",              "RFC"),
                F("receptorRegimenFiscal",    "Régimen", inline: true),
                F("receptorUsoCfdi",          "Uso CFDI", inline: true),
                F("receptorNombre",           "Razón social", bold: true),
                F("receptorDomicilioFiscal",  "Domicilio fiscal", inline: true),
            }, titleBg: "#204a87", bodyBg: "#f7f9fc", border: "#b8cce8", variant: "fiscal", sectionId: "receptor"),
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
            }, titleBg: "#264b7f", bodyBg: "#ffffff", border: "#b8cce8", variant: "fiscal", sectionId: "conceptos"),
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
            }, titleBg: "#163a68", bodyBg: "#f3f7fd", border: "#b8cce8", variant: "fiscal", sectionId: "pago"),
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
            }, titleBg: "#204a87", bodyBg: "#ffffff", border: "#b8cce8", variant: "fiscal", sectionId: "emisor"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // COTIZACIÓN
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
                // Campos disponibles (ocultos por defecto)
                F("convertedSaleCode", "Venta generada",               visible: false),
                F("customerCode",      "Código cliente",               visible: false, inline: true),
            }, titleBg: "#8d2f5a", bodyBg: "#fff1f7", border: "#f0bfd3", variant: "proposal", sectionId: "encabezado"),
            Table("Partidas Cotizadas", 2, new[]
            {
                C("productCode",  "Cód.",        60),
                C("productName",  "Descripción", 0),
                C("quantity",     "Cant.",        50,  fmt: "number",   align: "center"),
                C("unitPrice",    "P. Unit.",     70,  fmt: "currency", align: "right"),
                C("discountAmount","Desc.",        60,  fmt: "currency", align: "right"),
                C("taxAmount",    "IVA",          60,  fmt: "currency", align: "right"),
                C("lineTotal",    "Total",        70,  fmt: "currency", align: "right"),
                // Columnas disponibles (ocultas por defecto)
                C("discountPercentage", "% Desc.",    50,  fmt: "percentage", align: "right",  visible: false),
                C("taxPercentage",      "% IVA",      50,  fmt: "percentage", align: "right",  visible: false),
            }, titleBg: "#aa3a6d", bodyBg: "#fff8fb", border: "#f0bfd3", variant: "proposal", sectionId: "partidas"),
            Footer("Totales", 3, new[]
            {
                F("totalSubtotal", "Subtotal",  fmt: "currency", bold: true, inline: false),
                F("totalDiscount", "Descuento", fmt: "currency", inline: true),
                F("totalTax",      "IVA",       fmt: "currency", inline: false),
                F("totalAmount",   "Total",     fmt: "currency", bold: true, inline: true),
            }, titleBg: "#702143", bodyBg: "#fff1f7", border: "#f0bfd3", variant: "proposal", sectionId: "totales"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // ENTREGA (Delivery)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
                // Campos disponibles (ocultos por defecto)
                F("customerCode",    "Código cliente",               visible: false),
                F("paymentMethods",  "Forma de pago",               visible: false),
            }, titleBg: "#b04b2a", bodyBg: "#fff3ee", border: "#f2c8b9", variant: "delivery", sectionId: "entrega"),
            Table("Productos a Entregar", 2, new[]
            {
                C("productCode",  "Cód.",        60),
                C("productName",  "Descripción", 0),
                C("quantity",     "Cant.",        50,  fmt: "number",   align: "center"),
                C("unitPrice",    "P. Unit.",     70,  fmt: "currency", align: "right"),
                C("lineTotal",    "Total",        70,  fmt: "currency", align: "right"),
                // Columnas disponibles (ocultas por defecto)
                C("discountPercentage", "% Desc.",    55,  fmt: "percentage", align: "right",  visible: false),
                C("discountAmount",     "Descuento $", 65,  fmt: "currency",   align: "right",  visible: false),
                C("taxAmount",          "IVA $",       65,  fmt: "currency",   align: "right",  visible: false),
            }, titleBg: "#cb5c36", bodyBg: "#fffaf7", border: "#f2c8b9", variant: "delivery", sectionId: "productos"),
            Footer("Totales", 3, new[]
            {
                F("totalSubtotal", "Subtotal",  fmt: "currency", bold: true, inline: false),
                F("totalDiscount", "Descuento", fmt: "currency", inline: true),
                F("totalTax",      "IVA",       fmt: "currency", inline: false),
                F("totalAmount",   "Total",     fmt: "currency", bold: true, inline: true),
            }, titleBg: "#8b3d23", bodyBg: "#fff3ee", border: "#f2c8b9", variant: "delivery", sectionId: "totales"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // COMPLEMENTO DE PAGO CFDI
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static string BuildWarehouseTransferDispatchTemplate() => Json(new[]
        {
            Header("Información del Traspaso", 1, new[]
            {
                F("transferCode",            "Orden de Traspaso",     bold: true),
                F("transferDate",            "Fecha Traspaso",        fmt: "date",     inline: true),
                F("dispatchedAt",            "Despachado el",         fmt: "datetime", inline: false),
                F("sourceWarehouseName",     "Almacén Origen (Sale)", bold: true,     inline: true),
                F("destinationWarehouseName","Almacén Destino (Recibe)", bold: true),
                F("dispatchedByName",        "Despachado por",        inline: true),
                F("createdByName",           "Creado por"),
                F("totalProducts",           "Total Productos",       fmt: "number",   inline: true),
                F("status",                  "Estado"),
                F("notes",                   "Notas"),
                F("companyName",             "Empresa",                               visible: false),
                F("receivingUrl",            "URL de recepción móvil",               visible: false),
            }, titleBg: "#1a3c6e", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate", sectionId: "traspaso"),
            Table("Productos Despachados", 2, new[]
            {
                C("productCode",       "Código",         80),
                C("productName",       "Producto",       0),
                C("quantityRequested", "Solicitado",     70,  fmt: "number",   align: "center"),
                C("quantityDispatched","Despachado",     70,  fmt: "number",   align: "center", bold: true),
                C("unitCost",          "Costo Unit.",    70,  fmt: "currency", align: "right"),
                C("lineTotal",         "Total Línea",    80,  fmt: "currency", align: "right"),
                C("notes",             "Notas",          100),
            }, titleBg: "#1a3c6e", bodyBg: "#f7faff", border: "#c8d9f6", variant: "corporate", sectionId: "productos"),
            Footer("Totales y QR de Recepción", 3, new[]
            {
                F("totalQuantityDispatched", "Total Unidades Despachadas", fmt: "number", bold: true),
                F("receivingQrCode",         "Código QR Recepción Móvil",  fmt: "image"),
                F("receivingUrl",            "URL de recepción"),
            }, titleBg: "#1a3c6e", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate", sectionId: "totales"),
        });

        private static string BuildWarehouseTransferReceivingTemplate() => Json(new[]
        {
            Header("Información de la Recepción", 1, new[]
            {
                F("receivingCode",           "Entrada de Mercancía",  bold: true),
                F("receivingDate",           "Fecha Recepción",       fmt: "datetime", inline: true),
                F("receivingType",           "Tipo de Entrada",       inline: false),
                F("transferCode",            "Ref. Orden Traspaso",   bold: true,     inline: true),
                F("sourceWarehouseName",     "Almacén Origen",        inline: false),
                F("destinationWarehouseName","Almacén Destino (Recibe)", bold: true, inline: true),
                F("receivedByName",          "Recibido por"),
                F("totalProducts",           "Productos Recibidos",   fmt: "number",   inline: true),
                F("notes",                   "Notas"),
                F("companyName",             "Empresa",                               visible: false),
            }, titleBg: "#1a3c6e", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate", sectionId: "recepcion"),
            Table("Detalle de Productos Recibidos", 2, new[]
            {
                C("productCode",       "Código",         80),
                C("productName",       "Producto",       0),
                C("quantityDispatched","Despachado",     70,  fmt: "number",   align: "center"),
                C("quantityReceived",  "Recibido",       70,  fmt: "number",   align: "center", bold: true),
                C("pendingQuantity",   "Pendiente",      70,  fmt: "number",   align: "center"),
                C("unitCost",          "Costo Unit.",    70,  fmt: "currency", align: "right"),
                C("notes",             "Notas",          100),
            }, titleBg: "#1a3c6e", bodyBg: "#f7faff", border: "#c8d9f6", variant: "corporate", sectionId: "productos"),
            Footer("Resumen de Recepción", 3, new[]
            {
                F("totalQuantityDispatched", "Total Despachado",      fmt: "number"),
                F("totalQuantityReceived",   "Recibido en esta Entrada", fmt: "number", bold: true, inline: true),
                F("totalQuantityPending",    "Pendiente por Recibir",  fmt: "number",  bold: true),
            }, titleBg: "#1a3c6e", bodyBg: "#eef4ff", border: "#c8d9f6", variant: "corporate", sectionId: "totales"),
        });

        private static string BuildPaymentTemplate() => Json(new[]
        {
            Header("Emisor", 1, new[]
            {
                F("emisorNombre",        "Emisor",             bold: true),
                F("emisorRfc",           "RFC Emisor",         inline: true),
                F("emisorRegimenFiscal", "Régimen Fiscal",     inline: false),
                F("lugarExpedicion",     "Lugar Expedición",   inline: true),
            }, titleBg: "#1a3c6e", bodyBg: "#f3f7fd", border: "#b8cce8", variant: "fiscal", sectionId: "emisor"),
            Header("Receptor", 2, new[]
            {
                F("receptorNombre",          "Receptor",           bold: true),
                F("receptorRfc",             "RFC Receptor",       inline: true),
                F("receptorRegimenFiscal",   "Régimen Fiscal",     inline: false),
                F("receptorUsoCfdi",         "Uso CFDI",           inline: true),
                F("receptorDomicilioFiscal", "Domicilio Fiscal",   inline: false),
            }, titleBg: "#1a3c6e", bodyBg: "#f7f9fc", border: "#b8cce8", variant: "fiscal", sectionId: "receptor"),
            Header("Datos del Pago", 3, new[]
            {
                F("complementSerie",  "Serie",              inline: false),
                F("complementFolio",  "Folio",              bold: true,  inline: true),
                F("paymentDate",      "Fecha de Pago",      fmt: "datetime", inline: false),
                F("totalAmount",      "Monto Total",        fmt: "currency", bold: true, inline: true),
                F("currency",         "Moneda",             inline: false),
                F("paymentFormSAT",   "Forma de Pago SAT",  inline: true),
                // Campos disponibles (ocultos por defecto)
                F("exchangeRate",         "Tipo de cambio",             fmt: "number",   visible: false, inline: true),
                F("reference",            "Referencia bancaria",        visible: false),
                F("bankOrigin",           "Banco origen",               visible: false, inline: true),
                F("bankAccountOrigin",    "Cuenta origen",              visible: false, inline: true),
                F("bankDestination",      "Banco destino",              visible: false),
                F("bankAccountDestination","Cuenta destino",            visible: false, inline: true),
                F("notes",                "Notas",                      visible: false),
            }, titleBg: "#1a3c6e", bodyBg: "#eef4ff", border: "#b8cce8", variant: "fiscal", sectionId: "pago"),
            Table("Facturas Aplicadas", 4, new[]
            {
                C("serieAndFolio",     "Serie/Folio",     80),
                C("amountApplied",     "Importe Pagado",  85,  fmt: "currency", align: "right"),
                C("previousBalance",   "Saldo Anterior",  85,  fmt: "currency", align: "right"),
                C("newBalance",        "Saldo Insoluto",  85,  fmt: "currency", align: "right"),
                C("partialityNumber",  "Parcialidad",     65,  fmt: "number",   align: "center"),
                // Columnas disponibles (ocultas por defecto)
                C("folioUUID",             "UUID Factura",       0,  fmt: "text",     visible: false),
                C("paymentType",           "Tipo de pago",      70,  fmt: "text",     visible: false),
                C("originalInvoiceAmount", "Importe original",  85,  fmt: "currency", align: "right", visible: false),
            }, titleBg: "#1a3c6e", bodyBg: "#ffffff", border: "#b8cce8", variant: "fiscal", sectionId: "facturas"),
            Footer("Datos de Timbrado", 5, new[]
            {
                F("uuid",                "UUID / Folio Fiscal", bold: true),
                F("timbradoAt",          "Fecha Timbrado",      fmt: "datetime", inline: true),
                F("noCertificadoCfdi",   "No. Cert. CFDI",      inline: false),
                F("noCertificadoSat",    "No. Cert. SAT",       inline: true),
                F("qrCode",              "Código QR SAT",       fmt: "image"),
                // Campos disponibles (ocultos por defecto)
                F("selloCfdi",           "Sello CFDI",          visible: false),
                F("selloSat",            "Sello SAT",           visible: false),
                F("cadenaOriginalSat",   "Cadena original SAT", visible: false),
            }, titleBg: "#1a3c6e", bodyBg: "#f3f7fd", border: "#b8cce8", variant: "fiscal", sectionId: "timbrado"),
        });

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Helpers JSON
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static string Json(object sections) =>
            JsonSerializer.Serialize(sections, new JsonSerializerOptions { WriteIndented = false });

        private static object Header(string title, int order, object[] fields,
            string? titleBg = null, string? bodyBg = null, string? border = null, string? variant = null, string? sectionId = null) =>
            new { type = "Header", title, order, sectionId, showTitle = true, fields, columns = Array.Empty<object>(), titleBackground = titleBg, titleColor = "#ffffff", bodyBackground = bodyBg, borderColor = border, variant };

        private static object Table(string title, int order, object[] columns,
            string? titleBg = null, string? bodyBg = null, string? border = null, string? variant = null, string? sectionId = null) =>
            new { type = "Table", title, order, sectionId, showTitle = true, fields = Array.Empty<object>(), columns, titleBackground = titleBg, titleColor = "#ffffff", bodyBackground = bodyBg, borderColor = border, variant };

        private static object Footer(string title, int order, object[] fields,
            string? titleBg = null, string? bodyBg = null, string? border = null, string? variant = null, string? sectionId = null) =>
            new { type = "Footer", title, order, sectionId, showTitle = true, fields, columns = Array.Empty<object>(), titleBackground = titleBg, titleColor = "#ffffff", bodyBackground = bodyBg, borderColor = border, variant };

        private static object F(string field, string label, string fmt = "text", bool bold = false,
                                string align = "left", bool inline = false, bool visible = true) =>
            new { field, label, bold, fontSize = 9, align, format = fmt, inline, visible };

        private static object C(string field, string label, int width = 0, string fmt = "text",
                                string align = "left", bool bold = false, bool visible = true) =>
            new { field, label, width, align, format = fmt, bold, visible };
    }
}
