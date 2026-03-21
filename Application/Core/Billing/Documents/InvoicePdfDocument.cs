using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Application.Core.Billing.Documents
{
    /// <summary>
    /// Genera el PDF de una factura CFDI 4.0 timbrada usando QuestPDF
    /// </summary>
    public static class InvoicePdfDocument
    {
        private static readonly string PrimaryColor = "#1a3c6e";
        private static readonly string LightGray = "#f5f5f5";
        private static readonly string BorderColor = "#cccccc";

        public static byte[] Generate(Invoice invoice)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily(Fonts.Arial));

                    page.Content().Column(col =>
                    {
                        // ── ENCABEZADO ──────────────────────────────────────────────
                        col.Item().Row(row =>
                        {
                            // Emisor
                            row.RelativeItem(3).Column(c =>
                            {
                                c.Item().Text(invoice.EmisorNombre).Bold().FontSize(13).FontColor(PrimaryColor);
                                c.Item().Text($"RFC: {invoice.EmisorRfc}").FontSize(9);
                                c.Item().Text($"Régimen fiscal: {invoice.EmisorRegimenFiscal}").FontSize(8);
                                c.Item().Text($"Lugar de expedición: {invoice.LugarExpedicion}").FontSize(8);
                            });

                            // Datos del comprobante
                            row.RelativeItem(2).Border(1).BorderColor(PrimaryColor).Padding(8).Column(c =>
                            {
                                c.Item().Text("FACTURA").Bold().FontSize(14).FontColor(PrimaryColor).AlignCenter();
                                c.Item().Text($"Serie-Folio: {invoice.Serie}-{invoice.Folio}").Bold().FontSize(9).AlignCenter();
                                c.Item().Text($"Fecha: {invoice.InvoiceDate:yyyy-MM-dd HH:mm:ss}").FontSize(8).AlignCenter();
                                c.Item().Text($"Tipo: {DescribeTipoComprobante(invoice.TipoDeComprobante)}").FontSize(8).AlignCenter();
                                c.Item().Text("CFDI 4.0").FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();
                            });
                        });

                        col.Item().PaddingTop(10);

                        // ── RECEPTOR ────────────────────────────────────────────────
                        col.Item().Background(LightGray).Padding(6).Column(c =>
                        {
                            c.Item().Text("RECEPTOR").Bold().FontColor(PrimaryColor);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"RFC: {invoice.ReceptorRfc}").FontSize(8);
                                r.RelativeItem().Text($"Régimen: {invoice.ReceptorRegimenFiscal}").FontSize(8);
                                r.RelativeItem().Text($"Uso CFDI: {invoice.ReceptorUsoCfdi}").FontSize(8);
                            });
                            c.Item().Text(invoice.ReceptorNombre).FontSize(9).Bold();
                            c.Item().Text($"Domicilio fiscal: {invoice.ReceptorDomicilioFiscal}").FontSize(8);
                        });

                        col.Item().PaddingTop(8);

                        // ── CONCEPTOS ────────────────────────────────────────────────
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(55);  // Clave SAT
                                cols.ConstantColumn(70);  // No. Identificación
                                cols.ConstantColumn(40);  // Cantidad
                                cols.ConstantColumn(30);  // ClaveUnidad
                                cols.RelativeColumn();    // Descripción
                                cols.ConstantColumn(55);  // ValorUnitario
                                cols.ConstantColumn(55);  // Importe
                                cols.ConstantColumn(35);  // IVA
                            });

                            // Header
                            static IContainer HeaderCell(IContainer c) =>
                                c.Background(PrimaryColor).PaddingHorizontal(3).PaddingVertical(4);

                            table.Header(h =>
                            {
                                h.Cell().Element(HeaderCell).Text("Clave SAT").FontColor(Colors.White).FontSize(7).Bold();
                                h.Cell().Element(HeaderCell).Text("No. Ident.").FontColor(Colors.White).FontSize(7).Bold();
                                h.Cell().Element(HeaderCell).Text("Cant.").FontColor(Colors.White).FontSize(7).Bold();
                                h.Cell().Element(HeaderCell).Text("UM").FontColor(Colors.White).FontSize(7).Bold();
                                h.Cell().Element(HeaderCell).Text("Descripción").FontColor(Colors.White).FontSize(7).Bold();
                                h.Cell().Element(HeaderCell).Text("Precio Unit.").FontColor(Colors.White).FontSize(7).Bold().AlignRight();
                                h.Cell().Element(HeaderCell).Text("Importe").FontColor(Colors.White).FontSize(7).Bold().AlignRight();
                                h.Cell().Element(HeaderCell).Text("IVA").FontColor(Colors.White).FontSize(7).Bold().AlignRight();
                            });

                            bool zebra = false;
                            foreach (var d in invoice.Details)
                            {
                                string bg = zebra ? "#ffffff" : LightGray;
                                zebra = !zebra;

                                static IContainer DataCell(IContainer c, string bg) =>
                                    c.Background(bg).BorderBottom(1).BorderColor(BorderColor)
                                     .PaddingHorizontal(3).PaddingVertical(3);

                                var iva = d.TieneTraslados ? d.TrasladoImporte?.ToString("N2") ?? "-" : "-";

                                table.Cell().Element(c => DataCell(c, bg)).Text(d.ClaveProdServ).FontSize(7);
                                table.Cell().Element(c => DataCell(c, bg)).Text(d.NoIdentificacion ?? "-").FontSize(7);
                                table.Cell().Element(c => DataCell(c, bg)).Text(d.Cantidad.ToString("0.##")).FontSize(7);
                                table.Cell().Element(c => DataCell(c, bg)).Text($"{d.ClaveUnidad}\n{d.Unidad}").FontSize(6.5f);
                                table.Cell().Element(c => DataCell(c, bg)).Text(d.Descripcion).FontSize(7);
                                table.Cell().Element(c => DataCell(c, bg)).Text($"${d.ValorUnitario:N2}").FontSize(7).AlignRight();
                                table.Cell().Element(c => DataCell(c, bg)).Text($"${d.Importe:N2}").FontSize(7).AlignRight();
                                table.Cell().Element(c => DataCell(c, bg)).Text($"${iva}").FontSize(7).AlignRight();
                            }
                        });

                        col.Item().PaddingTop(6);

                        // ── TOTALES + QR ─────────────────────────────────────────────
                        col.Item().Row(row =>
                        {
                            // Datos de pago
                            row.RelativeItem(3).Column(c =>
                            {
                                c.Item().Text($"Forma de pago: {invoice.FormaPago}").FontSize(8);
                                c.Item().Text($"Método de pago: {invoice.MetodoPago}").FontSize(8);
                                c.Item().Text($"Moneda: {invoice.Moneda}  |  Tipo de cambio: {invoice.TipoCambio:0.######}").FontSize(8);
                                if (!string.IsNullOrEmpty(invoice.CondicionesDePago))
                                    c.Item().Text($"Condiciones de pago: {invoice.CondicionesDePago}").FontSize(8);

                                // QR code
                                if (!string.IsNullOrEmpty(invoice.QrCode))
                                {
                                    c.Item().PaddingTop(5);
                                    try
                                    {
                                        var qrBytes = Convert.FromBase64String(invoice.QrCode);
                                        c.Item().Width(70).Height(70).Image(qrBytes);
                                    }
                                    catch { /* QR inválido, se omite */ }
                                }
                            });

                            // Totales
                            row.RelativeItem(2).Border(1).BorderColor(PrimaryColor).Column(c =>
                            {
                                static IContainer TotalRow(IContainer x) =>
                                    x.BorderBottom(1).BorderColor(BorderColor).PaddingHorizontal(8).PaddingVertical(4);

                                c.Item().Element(TotalRow).Row(r =>
                                {
                                    r.RelativeItem().Text("Subtotal").FontSize(8);
                                    r.ConstantItem(70).Text($"${invoice.SubTotal:N2}").FontSize(8).AlignRight();
                                });
                                if (invoice.DiscountAmount > 0)
                                {
                                    c.Item().Element(TotalRow).Row(r =>
                                    {
                                        r.RelativeItem().Text("Descuento").FontSize(8);
                                        r.ConstantItem(70).Text($"-${invoice.DiscountAmount:N2}").FontSize(8).AlignRight();
                                    });
                                }
                                c.Item().Element(TotalRow).Row(r =>
                                {
                                    r.RelativeItem().Text("IVA (16%)").FontSize(8);
                                    r.ConstantItem(70).Text($"${invoice.TaxAmount:N2}").FontSize(8).AlignRight();
                                });
                                c.Item().Background(PrimaryColor).PaddingHorizontal(8).PaddingVertical(5).Row(r =>
                                {
                                    r.RelativeItem().Text("TOTAL").FontColor(Colors.White).Bold().FontSize(10);
                                    r.ConstantItem(70).Text($"${invoice.Total:N2} {invoice.Moneda}").FontColor(Colors.White).Bold().FontSize(10).AlignRight();
                                });
                            });
                        });

                        col.Item().PaddingTop(10);

                        // ── SELLO DIGITAL ────────────────────────────────────────────
                        if (invoice.Status == "Timbrada")
                        {
                            col.Item().BorderTop(1).BorderColor(BorderColor).PaddingTop(6).Column(c =>
                            {
                                c.Item().Text("DATOS DE TIMBRADO").Bold().FontSize(7).FontColor(PrimaryColor);
                                c.Item().PaddingTop(2).Row(r =>
                                {
                                    r.ConstantItem(45).Text("UUID:").FontSize(7).Bold();
                                    r.RelativeItem().Text(invoice.Uuid ?? "-").FontSize(7);
                                });
                                c.Item().Row(r =>
                                {
                                    r.ConstantItem(45).Text("Fecha timbrado:").FontSize(7).Bold();
                                    r.RelativeItem().Text(invoice.TimbradoAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-").FontSize(7);
                                });
                                c.Item().Row(r =>
                                {
                                    r.ConstantItem(45).Text("No. Cert. CFDI:").FontSize(7).Bold();
                                    r.RelativeItem().Text(invoice.NoCertificadoCfdi ?? "-").FontSize(7);
                                });
                                c.Item().Row(r =>
                                {
                                    r.ConstantItem(45).Text("No. Cert. SAT:").FontSize(7).Bold();
                                    r.RelativeItem().Text(invoice.NoCertificadoSat ?? "-").FontSize(7);
                                });

                                if (!string.IsNullOrEmpty(invoice.SelloCfdi))
                                {
                                    c.Item().PaddingTop(2).Text("Sello CFDI:").FontSize(6.5f).Bold().FontColor(PrimaryColor);
                                    c.Item().Text(invoice.SelloCfdi).FontSize(5.5f).FontColor(Colors.Grey.Darken2);
                                }
                                if (!string.IsNullOrEmpty(invoice.SelloSat))
                                {
                                    c.Item().PaddingTop(2).Text("Sello SAT:").FontSize(6.5f).Bold().FontColor(PrimaryColor);
                                    c.Item().Text(invoice.SelloSat).FontSize(5.5f).FontColor(Colors.Grey.Darken2);
                                }
                                if (!string.IsNullOrEmpty(invoice.CadenaOriginalSat))
                                {
                                    c.Item().PaddingTop(2).Text("Cadena original del complemento de certificación digital del SAT:").FontSize(6.5f).Bold().FontColor(PrimaryColor);
                                    c.Item().Text(invoice.CadenaOriginalSat).FontSize(5.5f).FontColor(Colors.Grey.Darken2);
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Este documento es una representación impresa de un CFDI  |  Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                        t.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                        t.Span(" de ").FontSize(7).FontColor(Colors.Grey.Medium);
                        t.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }

        private static string DescribeTipoComprobante(string tipo) => tipo switch
        {
            "I" => "Ingreso",
            "E" => "Egreso",
            "T" => "Traslado",
            "N" => "Nómina",
            "P" => "Pago",
            _ => tipo
        };
    }
}
