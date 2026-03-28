using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Application.Core.Billing.Documents
{
    /// <summary>
    /// Genera el PDF de un Complemento de Pago CFDI 4.0 timbrado usando QuestPDF
    /// </summary>
    public static class PaymentPdfDocument
    {
        private static readonly string PrimaryColor = "#1a3c6e";
        private static readonly string LightGray = "#f5f5f5";
        private static readonly string BorderColor = "#cccccc";

        public static byte[] Generate(Payment payment)
        {
            QuestPDF.Settings.License = LicenseType.Community;

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
                                c.Item().Text(payment.EmisorNombre ?? "-").Bold().FontSize(13).FontColor(PrimaryColor);
                                c.Item().Text($"RFC: {payment.EmisorRfc ?? "-"}").FontSize(9);
                                c.Item().Text($"Régimen fiscal: {payment.EmisorRegimenFiscal} – {DescribeRegimenFiscal(payment.EmisorRegimenFiscal)}").FontSize(8);
                                c.Item().Text($"Lugar de expedición: {payment.LugarExpedicion ?? "-"}").FontSize(8);
                            });

                            // Datos del comprobante
                            row.RelativeItem(2).Border(1).BorderColor(PrimaryColor).Padding(8).Column(c =>
                            {
                                c.Item().Text("COMPLEMENTO DE PAGO").Bold().FontSize(12).FontColor(PrimaryColor).AlignCenter();
                                var serieYFolio = $"{payment.ComplementSerie}-{payment.ComplementFolio}".Trim('-');
                                if (!string.IsNullOrEmpty(serieYFolio.Trim('-')))
                                    c.Item().Text($"Serie-Folio: {serieYFolio}").Bold().FontSize(9).AlignCenter();
                                c.Item().Text($"No. Pago: {payment.PaymentNumber}").FontSize(9).AlignCenter();
                                c.Item().Text($"Fecha: {payment.PaymentDate:yyyy-MM-dd}").FontSize(8).AlignCenter();
                                c.Item().Text("CFDI 4.0 – Tipo P").FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();
                            });
                        });

                        col.Item().PaddingTop(10);

                        // ── RECEPTOR ────────────────────────────────────────────────
                        col.Item().Background(LightGray).Padding(6).Column(c =>
                        {
                            c.Item().Text("RECEPTOR").Bold().FontColor(PrimaryColor);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"RFC: {payment.ReceptorRfc ?? "-"}").FontSize(8);
                                r.RelativeItem().Text($"Régimen: {payment.ReceptorRegimenFiscal} – {DescribeRegimenFiscal(payment.ReceptorRegimenFiscal)}").FontSize(8);
                                r.RelativeItem().Text($"Uso CFDI: {payment.ReceptorUsoCfdi} – {DescribeUsoCfdi(payment.ReceptorUsoCfdi)}").FontSize(8);
                            });
                            c.Item().Text(payment.ReceptorNombre ?? "-").FontSize(9).Bold();
                            c.Item().Text($"Domicilio fiscal: {payment.ReceptorDomicilioFiscal ?? "-"}").FontSize(8);
                        });

                        col.Item().PaddingTop(8);

                        // ── DATOS DEL PAGO ───────────────────────────────────────────
                        col.Item().Border(1).BorderColor(BorderColor).Padding(6).Column(c =>
                        {
                            c.Item().Text("DATOS DEL PAGO").Bold().FontColor(PrimaryColor).FontSize(9);
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.RelativeItem().Column(inner =>
                                {
                                    inner.Item().Text($"Fecha de pago: {payment.PaymentDate:yyyy-MM-dd}").FontSize(8);
                                    inner.Item().Text($"Forma de pago: {payment.PaymentFormSAT} – {DescribeFormaPago(payment.PaymentFormSAT)}").FontSize(8);
                                    inner.Item().Text($"Moneda: {payment.Currency}").FontSize(8);
                                    if (!string.IsNullOrEmpty(payment.Reference))
                                        inner.Item().Text($"Referencia: {payment.Reference}").FontSize(8);
                                    if (!string.IsNullOrEmpty(payment.BankDestination))
                                        inner.Item().Text($"Banco destino: {payment.BankDestination}").FontSize(8);
                                    if (!string.IsNullOrEmpty(payment.BankAccountDestination))
                                        inner.Item().Text($"Cuenta destino: {payment.BankAccountDestination}").FontSize(8);
                                });
                                r.ConstantItem(120).Border(1).BorderColor(PrimaryColor).Padding(6).Column(inner =>
                                {
                                    inner.Item().Text("MONTO TOTAL").Bold().FontSize(8).FontColor(PrimaryColor).AlignCenter();
                                    inner.Item().Text($"${payment.TotalAmount:N2} {payment.Currency}").Bold().FontSize(13).FontColor(PrimaryColor).AlignCenter();
                                });
                            });
                        });

                        col.Item().PaddingTop(8);

                        // ── DOCUMENTOS RELACIONADOS ──────────────────────────────────
                        if (payment.PaymentApplications.Any())
                        {
                            col.Item().Text("DOCUMENTOS RELACIONADOS (FACTURAS APLICADAS)").Bold().FontColor(PrimaryColor).FontSize(9);
                            col.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2);  // Serie-Folio
                                    cols.RelativeColumn(3);  // UUID
                                    cols.ConstantColumn(25); // Parcialidad
                                    cols.ConstantColumn(65); // Saldo anterior
                                    cols.ConstantColumn(65); // Importe pagado
                                    cols.ConstantColumn(65); // Saldo insoluto
                                });

                                static IContainer HeaderCell(IContainer c) =>
                                    c.Background("#1a3c6e").PaddingHorizontal(3).PaddingVertical(4);

                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderCell).Text("Serie-Folio").FontColor(Colors.White).FontSize(7).Bold();
                                    h.Cell().Element(HeaderCell).Text("UUID").FontColor(Colors.White).FontSize(7).Bold();
                                    h.Cell().Element(HeaderCell).Text("Parc.").FontColor(Colors.White).FontSize(7).Bold().AlignCenter();
                                    h.Cell().Element(HeaderCell).Text("Saldo Ant.").FontColor(Colors.White).FontSize(7).Bold().AlignRight();
                                    h.Cell().Element(HeaderCell).Text("Imp. Pagado").FontColor(Colors.White).FontSize(7).Bold().AlignRight();
                                    h.Cell().Element(HeaderCell).Text("Saldo Insol.").FontColor(Colors.White).FontSize(7).Bold().AlignRight();
                                });

                                bool zebra = false;
                                foreach (var app in payment.PaymentApplications)
                                {
                                    string bg = zebra ? "#ffffff" : LightGray;
                                    zebra = !zebra;

                                    static IContainer DataCell(IContainer c, string bg) =>
                                        c.Background(bg).BorderBottom(1).BorderColor("#cccccc")
                                         .PaddingHorizontal(3).PaddingVertical(3);

                                    table.Cell().Element(c => DataCell(c, bg)).Text(app.SerieAndFolio).FontSize(7);
                                    table.Cell().Element(c => DataCell(c, bg)).Text(app.FolioUUID ?? "-").FontSize(6);
                                    table.Cell().Element(c => DataCell(c, bg)).Text(app.PartialityNumber.ToString()).FontSize(7).AlignCenter();
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"${app.PreviousBalance:N2}").FontSize(7).AlignRight();
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"${app.AmountApplied:N2}").FontSize(7).AlignRight();
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"${app.NewBalance:N2}").FontSize(7).AlignRight();
                                }
                            });
                        }

                        col.Item().PaddingTop(10);

                        // ── SELLO DIGITAL ────────────────────────────────────────────
                        col.Item().BorderTop(1).BorderColor(BorderColor).PaddingTop(6).Column(c =>
                        {
                            c.Item().Text("DATOS DE TIMBRADO").Bold().FontSize(7).FontColor(PrimaryColor);
                            c.Item().PaddingTop(2).Row(r =>
                            {
                                r.ConstantItem(55).Text("UUID:").FontSize(7).Bold();
                                r.RelativeItem().Text(payment.Uuid ?? "-").FontSize(7);
                            });
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(55).Text("Fecha timbrado:").FontSize(7).Bold();
                                r.RelativeItem().Text(payment.TimbradoAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-").FontSize(7);
                            });
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(55).Text("No. Cert. CFDI:").FontSize(7).Bold();
                                r.RelativeItem().Text(payment.NoCertificadoCfdi ?? "-").FontSize(7);
                            });
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(55).Text("No. Cert. SAT:").FontSize(7).Bold();
                                r.RelativeItem().Text(payment.NoCertificadoSat ?? "-").FontSize(7);
                            });

                            // QR
                            if (!string.IsNullOrEmpty(payment.QrCode))
                            {
                                c.Item().PaddingTop(5);
                                try
                                {
                                    var qrBytes = Convert.FromBase64String(payment.QrCode);
                                    c.Item().Width(70).Height(70).Image(qrBytes);
                                }
                                catch { /* QR inválido, se omite */ }
                            }

                            if (!string.IsNullOrEmpty(payment.SelloCfdi))
                            {
                                c.Item().PaddingTop(2).Text("Sello CFDI:").FontSize(6.5f).Bold().FontColor(PrimaryColor);
                                c.Item().Text(payment.SelloCfdi).FontSize(5.5f).FontColor(Colors.Grey.Darken2);
                            }
                            if (!string.IsNullOrEmpty(payment.SelloSat))
                            {
                                c.Item().PaddingTop(2).Text("Sello SAT:").FontSize(6.5f).Bold().FontColor(PrimaryColor);
                                c.Item().Text(payment.SelloSat).FontSize(5.5f).FontColor(Colors.Grey.Darken2);
                            }
                            if (!string.IsNullOrEmpty(payment.CadenaOriginalSat))
                            {
                                c.Item().PaddingTop(2).Text("Cadena original del complemento de certificación digital del SAT:").FontSize(6.5f).Bold().FontColor(PrimaryColor);
                                c.Item().Text(payment.CadenaOriginalSat).FontSize(5.5f).FontColor(Colors.Grey.Darken2);
                            }
                        });
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

        private static string DescribeFormaPago(string? clave) => clave switch
        {
            "01" => "Efectivo",
            "02" => "Cheque nominativo",
            "03" => "Transferencia electrónica",
            "04" => "Tarjeta de crédito",
            "05" => "Monedero electrónico",
            "06" => "Dinero electrónico",
            "08" => "Vales de despensa",
            "12" => "Dación en pago",
            "13" => "Pago por subrogación",
            "14" => "Pago por consignación",
            "15" => "Condonación",
            "17" => "Compensación",
            "23" => "Novación",
            "24" => "Confusión",
            "25" => "Remisión de deuda",
            "26" => "Prescripción o caducidad",
            "27" => "A satisfacción del acreedor",
            "28" => "Tarjeta de débito",
            "29" => "Tarjeta de servicios",
            "30" => "Aplicación de anticipos",
            "31" => "Intermediario pagos",
            "99" => "Por definir",
            _ => clave ?? "-"
        };

        private static string DescribeRegimenFiscal(string? clave) => clave switch
        {
            "601" => "General de Ley Personas Morales",
            "603" => "Personas Morales con Fines no Lucrativos",
            "605" => "Sueldos y Salarios e Ingresos Asimilados a Salarios",
            "606" => "Arrendamiento",
            "607" => "Régimen de Enajenación o Adquisición de Bienes",
            "608" => "Demás ingresos",
            "609" => "Consolidación",
            "610" => "Residentes en el Extranjero sin Establecimiento Permanente en México",
            "611" => "Ingresos por Dividendos (socios y accionistas)",
            "612" => "Personas Físicas con Actividades Empresariales y Profesionales",
            "614" => "Ingresos por intereses",
            "615" => "Régimen de los ingresos por obtención de premios",
            "616" => "Sin obligaciones fiscales",
            "620" => "Sociedades Cooperativas de Producción que optan por diferir sus ingresos",
            "621" => "Incorporación Fiscal",
            "622" => "Actividades Agrícolas, Ganaderas, Silvícolas y Pesqueras",
            "623" => "Opcional para Grupos de Sociedades",
            "624" => "Coordinados",
            "625" => "Régimen de las Actividades Empresariales con ingresos a través de Plataformas Tecnológicas",
            "626" => "Régimen Simplificado de Confianza",
            _ => clave ?? "-"
        };

        private static string DescribeUsoCfdi(string? clave) => clave switch
        {
            "G01" => "Adquisición de mercancias",
            "G02" => "Devoluciones, descuentos o bonificaciones",
            "G03" => "Gastos en general",
            "I01" => "Construcciones",
            "I02" => "Mobilario y equipo de oficina por inversiones",
            "I03" => "Equipo de transporte",
            "I04" => "Equipo de computo y accesorios",
            "I05" => "Dados, troqueles, moldes, matrices y herramental",
            "I06" => "Comunicaciones telefónicas",
            "I07" => "Comunicaciones satelitales",
            "I08" => "Otra maquinaria y equipo",
            "D01" => "Honorarios médicos, dentales y gastos hospitalarios",
            "D02" => "Gastos médicos por incapacidad o discapacidad",
            "D03" => "Gastos funerales",
            "D04" => "Donativos",
            "D05" => "Intereses reales efectivamente pagados por créditos hipotecarios (casa habitación)",
            "D06" => "Aportaciones voluntarias al SAR",
            "D07" => "Primas por seguros de gastos médicos",
            "D08" => "Gastos de transportación escolar obligatoria",
            "D09" => "Depósitos en cuentas para el ahorro, primas que tengan como base planes de pensiones",
            "D10" => "Pagos por servicios educativos (colegiaturas)",
            "S01" => "Sin efectos fiscales",
            "CP01" => "Pagos",
            "CN01" => "Nómina",
            _ => clave ?? "-"
        };
    }
}
