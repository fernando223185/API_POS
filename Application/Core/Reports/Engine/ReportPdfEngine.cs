using Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace Application.Core.Reports.Engine
{
    /// <summary>
    /// Motor de generación de PDF dinámico.
    /// Lee la configuración de secciones (ReportSectionDefinition[]) y 
    /// genera el documento QuestPDF en base a los datos provistos.
    /// </summary>
    public static class ReportPdfEngine
    {
        private static readonly string PrimaryColor = "#1a3c6e";
        private static readonly string LightGray = "#f5f5f5";
        private static readonly string BorderColor = "#cccccc";
        private static readonly string HeaderBg = "#1a3c6e";
        private sealed record ThemePalette(string Primary, string Surface, string Border, string TextOnPrimary);

        /// <summary>
        /// Genera un PDF en base a una plantilla y un conjunto de datos.
        /// </summary>
        /// <param name="sections">Secciones configuradas de la plantilla</param>
        /// <param name="dataRows">
        ///   Lista de filas de datos. Cada fila es un Dictionary[string, object?]
        ///   donde la clave es el field key del catálogo (ej. "saleCode", "customerName").
        ///   Para reportes de un solo documento → lista con un elemento.
        ///   Para reportes de múltiples → cada elemento es un documento.
        /// </param>
        /// <summary>
        /// Overload que acepta SectionsJson (string) en lugar de la lista deserializada.
        /// Conveniente para servicios que no tienen acceso al query handler.
        /// </summary>
        public static byte[] Generate(
            string sectionsJson,
            List<Dictionary<string, object?>> dataRows,
            List<Dictionary<string, object?>> tableRows,
            string reportTitle = "Reporte")
        {
            var sections = JsonSerializer.Deserialize<List<ReportSectionDefinition>>(sectionsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            return Generate(sections, dataRows, tableRows, reportTitle);
        }

        /// <param name="tableRows">
        ///   Filas de detalle para la sección Table. Cada fila es Dictionary[string, object?].
        /// </param>
        /// <param name="reportTitle">Título que aparece en el encabezado del PDF</param>
        public static byte[] Generate(
            List<ReportSectionDefinition> sections,
            List<Dictionary<string, object?>> dataRows,
            List<Dictionary<string, object?>> tableRows,
            string reportTitle = "Reporte")
        {
            var orderedSections = sections.OrderBy(s => s.Order).ToList();
            var palette = ResolvePagePalette(orderedSections);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    var headerRow = dataRows.FirstOrDefault() ?? new Dictionary<string, object?>();
                    if (IsInvoiceDocument(headerRow))
                        page.Header().ShowOnce().Element(c => RenderPageHeader(c, reportTitle, palette, headerRow));
                    else
                        page.Header().Element(c => RenderPageHeader(c, reportTitle, palette, headerRow));
                    page.Footer().Element(c => RenderPageFooter(c, palette));

                    page.Content().Column(col =>
                    {
                        // Si hay múltiples documentos, renderizar cada uno con sus secciones
                        if (dataRows.Count > 1)
                        {
                            foreach (var (dataRow, idx) in dataRows.Select((r, i) => (r, i)))
                            {
                                if (idx > 0)
                                    col.Item().PaddingTop(20).LineHorizontal(1).LineColor(palette.Border);

                                RenderDocument(col, orderedSections, dataRow, tableRows, palette, isSingleDoc: false);
                            }
                        }
                        else
                        {
                            var dataRow = dataRows.FirstOrDefault() ?? new();
                            RenderDocument(col, orderedSections, dataRow, tableRows, palette, isSingleDoc: true);
                            RenderTrailingQr(col, dataRow);
                        }
                    });
                });
            }).GeneratePdf();
        }

        // ─────────────────────────────────────────────
        // Cabecera y pie de página del PDF
        // ─────────────────────────────────────────────

        private static void RenderPageHeader(IContainer container, string reportTitle, ThemePalette palette, Dictionary<string, object?> data)
        {
            if (IsInvoiceDocument(data))
            {
                RenderInvoiceHeader(container, reportTitle, data);
                return;
            }

            container
                .BorderBottom(2).BorderColor(palette.Primary)
                .PaddingBottom(4)
                .Row(row =>
                {
                    row.RelativeItem().Text(reportTitle)
                        .Bold().FontSize(14).FontColor(palette.Primary);
                    row.ConstantItem(120).Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(7).FontColor(Colors.Grey.Medium).AlignRight();
                });
        }

        private static void RenderPageFooter(IContainer container, ThemePalette palette)
        {
            container
                .BorderTop(1).BorderColor(palette.Border)
                .PaddingTop(4)
                .Row(row =>
                {
                    row.RelativeItem().Text("EasyPOS — Reporte generado automáticamente")
                        .FontSize(7).FontColor(Colors.Grey.Medium);
                    row.ConstantItem(60).Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(7));
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
        }

        // ─────────────────────────────────────────────
        // Renderizado de un documento completo
        // ─────────────────────────────────────────────

        private static void RenderDocument(
            ColumnDescriptor col,
            List<ReportSectionDefinition> sections,
            Dictionary<string, object?> dataRow,
            List<Dictionary<string, object?>> tableRows,
            ThemePalette palette,
            bool isSingleDoc)
        {
            foreach (var section in sections)
            {
                col.Item().PaddingTop(8);

                switch (section.Type)
                {
                    case SectionType.Header:
                    case SectionType.Summary:
                    case SectionType.Footer:
                        if (ShouldKeepSectionTogether(section))
                            col.Item().ShowEntire().Element(c => RenderKeyValueSection(c, section, dataRow, palette));
                        else
                            col.Item().Element(c => RenderKeyValueSection(c, section, dataRow, palette));
                        break;

                    case SectionType.Table:
                        col.Item().Element(c => RenderTableSection(c, section, tableRows, palette));
                        break;
                }
            }
        }

        // ─────────────────────────────────────────────
        // Sección Key-Value (Header / Summary / Footer)
        // ─────────────────────────────────────────────

        private static void RenderKeyValueSection(
            IContainer container,
            ReportSectionDefinition section,
            Dictionary<string, object?> data,
            ThemePalette palette)
        {
            var sectionTitleBg = section.TitleBackground ?? palette.Primary;
            var sectionTitleColor = section.TitleColor ?? palette.TextOnPrimary;
            var sectionBodyBg = section.BodyBackground ?? palette.Surface;
            var sectionBorder = section.BorderColor ?? palette.Border;

            container.Border(1).BorderColor(sectionBorder).Background(sectionBodyBg).Column(col =>
            {
                if (section.ShowTitle)
                {
                    col.Item()
                        .Background(sectionTitleBg)
                        .PaddingHorizontal(12).PaddingVertical(9)
                        .Text(section.Title)
                        .FontSize(9).Bold().FontColor(sectionTitleColor);
                }

                col.Item().Padding(16).Column(inner =>
                {
                    // Renderizar en grid de 2 columnas (pares Inline)
                    var fields = section.Fields.Where(f => !IsTrailingQrField(f) && !IsImageField(f)).ToList();
                    var imageFields = section.Fields.Where(f => IsImageField(f) && !IsTrailingQrField(f)).ToList();
                    int i = 0;
                    while (i < fields.Count)
                    {
                        var f = fields[i];
                        
                        // Si este campo NO es inline pero el siguiente SÍ es inline → emparejarlos
                        if (!f.Inline && i + 1 < fields.Count && fields[i + 1].Inline)
                        {
                            var f2 = fields[i + 1];
                            inner.Item().Row(row =>
                            {
                                row.Spacing(24);
                                row.RelativeItem().Element(c => RenderField(c, f, data));
                                row.RelativeItem().Element(c => RenderField(c, f2, data));
                            });
                            i += 2;
                        }
                        else
                        {
                            inner.Item().Element(c => RenderField(c, f, data));
                            i++;
                        }
                    }

                    foreach (var imageField in imageFields)
                    {
                        inner.Item().PaddingTop(14).AlignRight().Element(c => RenderField(c, imageField, data));
                    }
                });
            });
        }

        private static void RenderField(
            IContainer container,
            ReportSectionField field,
            Dictionary<string, object?> data)
        {
            var rawValue = data.TryGetValue(field.Field, out var v) ? v : null;
            var displayValue = FormatValue(rawValue, field.Format);
            var label = field.Label;

            if (IsImageField(field))
            {
                if (string.IsNullOrWhiteSpace(displayValue))
                    return;

                container.PaddingVertical(6).Column(col =>
                {
                    col.Item().Text(label)
                        .FontSize(field.FontSize).Bold().FontColor(Colors.Grey.Darken2);

                    try
                    {
                        var qrBytes = Convert.FromBase64String(displayValue);
                        col.Item()
                            .PaddingTop(6)
                            .Border(1).BorderColor("#dbe4f0")
                            .Background("#f8fbff")
                            .Padding(8)
                            .Width(120)
                            .Column(qr =>
                            {
                                qr.Item().Width(100).Height(100).AlignCenter().Image(qrBytes);
                                qr.Item().PaddingTop(6).AlignCenter().Text("Escanea para validar CFDI").FontSize(7).FontColor(Colors.Grey.Darken2);
                            });
                    }
                    catch
                    {
                    }
                });
                return;
            }

            var isLongValue = displayValue.Length > 70
                || field.Field.Contains("sello", StringComparison.OrdinalIgnoreCase)
                || field.Field.Contains("cadena", StringComparison.OrdinalIgnoreCase);

            if (isLongValue)
            {
                container.PaddingVertical(6).Column(col =>
                {
                    col.Item().Text($"{label}:")
                        .FontSize(field.FontSize).Bold().FontColor(Colors.Grey.Darken2);

                    var block = col.Item()
                        .PaddingTop(4)
                        .Background(Colors.White)
                        .Border(1).BorderColor("#e5e7eb")
                        .Padding(8);

                    if (field.Bold)
                        block.Text(displayValue).FontSize(field.FontSize).Bold();
                    else
                        block.Text(displayValue).FontSize(field.FontSize);
                });
                return;
            }

            container.PaddingVertical(6).Row(row =>
            {
                row.ConstantItem(160)
                    .Text($"{label}:")
                    .FontSize(field.FontSize).Bold().FontColor("#5b6472");

                var valueContainer = row.RelativeItem();
                if (field.Bold)
                    valueContainer.Text(displayValue).FontSize(field.FontSize).Bold().FontColor("#17345f");
                else
                    valueContainer.Text(displayValue).FontSize(field.FontSize).FontColor("#1f2937");
            });
        }

        // ─────────────────────────────────────────────
        // Sección Table
        // ─────────────────────────────────────────────

        private static void RenderTableSection(
            IContainer container,
            ReportSectionDefinition section,
            List<Dictionary<string, object?>> rows,
            ThemePalette palette)
        {
            if (!section.Columns.Any()) return;

            var sectionTitleBg = section.TitleBackground ?? palette.Primary;
            var sectionTitleColor = section.TitleColor ?? palette.TextOnPrimary;
            var sectionBodyBg = section.BodyBackground ?? palette.Surface;
            var sectionBorder = section.BorderColor ?? palette.Border;
            var normalizedWidths = NormalizeTableColumnWidths(section.Columns);
            var isDenseTable = section.Columns.Count >= 8;
            var headerHorizontalPadding = isDenseTable ? 6 : 10;
            var cellHorizontalPadding = isDenseTable ? 6 : 10;
            var tableFontSize = isDenseTable ? 8f : 9f;
            var headerFontSize = isDenseTable ? 8.5f : 9.5f;

            container.Border(1).BorderColor(sectionBorder).Background(sectionBodyBg).Column(col =>
            {
                if (section.ShowTitle)
                {
                    col.Item()
                        .Background(sectionTitleBg)
                        .PaddingHorizontal(14).PaddingVertical(10)
                        .Text(section.Title)
                        .FontSize(10).Bold().FontColor(sectionTitleColor);
                }

                col.Item().Padding(12).Table(table =>
                {
                    // Definir columnas
                    table.ColumnsDefinition(cols =>
                    {
                        for (var index = 0; index < section.Columns.Count; index++)
                        {
                            var c = section.Columns[index];
                            if (c.Width > 0)
                                cols.ConstantColumn(normalizedWidths[index]);
                            else
                                cols.RelativeColumn();
                        }
                    });

                    // Header
                    table.Header(h =>
                    {
                        foreach (var c in section.Columns)
                        {
                            h.Cell()
                                .Background(sectionTitleBg)
                                .PaddingHorizontal(headerHorizontalPadding).PaddingVertical(10)
                                .Text(c.Label)
                                .FontSize(headerFontSize).Bold().FontColor(sectionTitleColor);
                        }
                    });

                    // Filas
                    bool zebra = false;
                    foreach (var row in rows)
                    {
                        string bg = zebra ? Colors.White : sectionBodyBg;
                        zebra = !zebra;

                        foreach (var col2 in section.Columns)
                        {
                            var rawValue = row.TryGetValue(col2.Field, out var v) ? v : null;
                            var display = FormatValue(rawValue, col2.Format);

                            var cell = table.Cell()
                                .Background(bg)
                                .BorderBottom(1).BorderColor(sectionBorder)
                                .PaddingHorizontal(cellHorizontalPadding).PaddingVertical(10);

                            if (col2.Bold)
                                cell.Text(display).FontSize(tableFontSize).Bold();
                            else
                                cell.Text(display).FontSize(tableFontSize);
                        }
                    }
                });
            });
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        private static string FormatValue(object? value, string format)
        {
            if (value is null) return string.Empty;

            return format switch
            {
                FieldFormat.Currency   => value is decimal d ? $"${d:N2}" :
                                         decimal.TryParse(value.ToString(), out var dp) ? $"${dp:N2}" : value.ToString() ?? "",
                FieldFormat.Number     => value is decimal dn ? dn.ToString("0.##") :
                                         value is int i ? i.ToString() : value.ToString() ?? "",
                FieldFormat.Percentage => value is decimal dp2 ? $"{dp2:0.##}%" : $"{value}%",
                FieldFormat.Date       => value is DateTime dt ? dt.ToString("dd/MM/yyyy") :
                                         DateTime.TryParse(value.ToString(), out var dtp) ? dtp.ToString("dd/MM/yyyy") : value.ToString() ?? "",
                FieldFormat.DateTime   => value is DateTime dtm ? dtm.ToString("dd/MM/yyyy HH:mm") :
                                         DateTime.TryParse(value.ToString(), out var dtmp) ? dtmp.ToString("dd/MM/yyyy HH:mm") : value.ToString() ?? "",
                _                      => value.ToString() ?? ""
            };
        }

        private static List<float> NormalizeTableColumnWidths(List<ReportTableColumn> columns)
        {
            const float estimatedContentWidth = 515f;
            const float minimumRelativeWidth = 84f;

            var relativeCount = columns.Count(c => c.Width <= 0);
            var fixedTotal = columns.Where(c => c.Width > 0).Sum(c => c.Width);
            var maxFixedWidth = Math.Max(estimatedContentWidth - (relativeCount * minimumRelativeWidth), estimatedContentWidth * 0.45f);

            if (fixedTotal <= 0 || fixedTotal <= maxFixedWidth)
                return columns.Select(c => (float)c.Width).ToList();

            var scale = maxFixedWidth / fixedTotal;

            return columns
                .Select(c => c.Width > 0 ? Math.Max(24f, c.Width * scale) : 0f)
                .ToList();
        }

        private static void ApplyAlign(TextSpanDescriptor text, string align)
        {
            // TextSpanDescriptor en QuestPDF no tiene métodos de alineación directos.
            // La alineación se controla desde el contenedor padre mediante el método
            // .AlignCenter() / .AlignRight() sobre el IContainer, no sobre el span.
            // Este método existe como extensión futura; la alineación real se aplica
            // en el IContainer que envuelve el texto (ver uso en RenderField / RenderTableSection).
        }

        private static ThemePalette ResolvePagePalette(List<ReportSectionDefinition> sections)
        {
            var firstStyled = sections.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.TitleBackground)
                                                        || !string.IsNullOrWhiteSpace(s.BodyBackground)
                                                        || !string.IsNullOrWhiteSpace(s.BorderColor));

            return new ThemePalette(
                firstStyled?.TitleBackground ?? PrimaryColor,
                firstStyled?.BodyBackground ?? LightGray,
                firstStyled?.BorderColor ?? BorderColor,
                firstStyled?.TitleColor ?? Colors.White);
        }

        private static void RenderTrailingQr(ColumnDescriptor col, Dictionary<string, object?> data)
        {
            var qrValue = data.TryGetValue("qrCode", out var rawQr) ? rawQr?.ToString() : null;
            if (string.IsNullOrWhiteSpace(qrValue))
                return;

            col.Item().PaddingTop(22).BorderTop(1).BorderColor("#dbe4f0").PaddingTop(14).AlignRight().Column(qr =>
            {
                try
                {
                    var qrBytes = Convert.FromBase64String(qrValue);
                    qr.Item()
                        .Border(1).BorderColor("#dbe4f0")
                        .Background("#f8fbff")
                        .Padding(10)
                        .Width(148)
                        .Column(content =>
                        {
                            content.Item().AlignCenter().Text("VALIDACION SAT").FontSize(7).SemiBold().FontColor("#204a87");
                            content.Item().PaddingTop(8).Width(120).Height(120).AlignCenter().Image(qrBytes);
                            content.Item().PaddingTop(8).AlignCenter().Text("Escanea este codigo para validar el CFDI en el SAT").FontSize(6.8f).FontColor(Colors.Grey.Darken2);
                        });
                }
                catch
                {
                }
            });
        }

        private static void RenderInvoiceHeader(IContainer container, string reportTitle, Dictionary<string, object?> data)
        {
            var serie = data.TryGetValue("invoiceSerie", out var serieValue) ? serieValue?.ToString() ?? string.Empty : string.Empty;
            var folio = data.TryGetValue("invoiceFolio", out var folioValue) ? folioValue?.ToString() ?? string.Empty : string.Empty;
            var emisor = data.TryGetValue("emisorNombre", out var emisorValue) ? emisorValue?.ToString() ?? string.Empty : string.Empty;
            var logoUrl = data.TryGetValue("companyLogoUrl", out var logoValue) ? logoValue?.ToString() ?? string.Empty : string.Empty;
            var tradeName = data.TryGetValue("companyTradeName", out var tradeValue) ? tradeValue?.ToString() ?? emisor : emisor;
            var emisorRfc = data.TryGetValue("emisorRfc", out var emisorRfcValue) ? emisorRfcValue?.ToString() ?? string.Empty : string.Empty;
            var receptor = data.TryGetValue("receptorNombre", out var receptorValue) ? receptorValue?.ToString() ?? string.Empty : string.Empty;
            var receptorRfc = data.TryGetValue("receptorRfc", out var receptorRfcValue) ? receptorRfcValue?.ToString() ?? string.Empty : string.Empty;
            var fecha = FormatValue(data.TryGetValue("invoiceDate", out var dateValue) ? dateValue : null, FieldFormat.DateTime);
            var total = FormatValue(data.TryGetValue("total", out var totalValue) ? totalValue : null, FieldFormat.Currency);

            container.Background("#eef3fb").Border(1).BorderColor("#d7e1ef").Padding(14).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Element(left =>
                    {
                        left.Width(240).Height(108).Element(box => RenderCompanyMark(box, logoUrl, tradeName));
                    });

                    row.Spacing(12);

                    row.RelativeItem().AlignRight().Column(right =>
                    {
                        right.Item().Border(1).BorderColor("#dbe4f0").Background(Colors.White).Padding(12).Column(meta =>
                        {
                            MetaRow(meta, "Folio", $"{serie}-{folio}");
                            MetaRow(meta, "Fecha", fecha);
                            MetaRow(meta, "Total", total, true);
                        });
                    });
                });

                col.Item().PaddingTop(12).Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor("#dbe4f0").Background(Colors.White).Padding(12).Column(x =>
                    {
                        x.Item().Text("EMISOR").FontSize(7).FontColor("#6b85b3").SemiBold();
                        x.Item().PaddingTop(4).Text(emisor).FontSize(10).Bold().FontColor("#17345f");
                        x.Item().PaddingTop(2).Text($"RFC: {emisorRfc}").FontSize(8).FontColor("#5b6472");
                    });

                    row.Spacing(10);

                    row.RelativeItem().Border(1).BorderColor("#dbe4f0").Background(Colors.White).Padding(12).Column(x =>
                    {
                        x.Item().Text("RECEPTOR").FontSize(7).FontColor("#6b85b3").SemiBold();
                        x.Item().PaddingTop(4).Text(receptor).FontSize(10).Bold().FontColor("#17345f");
                        x.Item().PaddingTop(2).Text($"RFC: {receptorRfc}").FontSize(8).FontColor("#5b6472");
                    });
                });
            });
        }

        private static void MetaRow(ColumnDescriptor container, string label, string value, bool highlight = false)
        {
            container.Item().PaddingVertical(2).Row(row =>
            {
                row.RelativeItem().Text(label).FontSize(8).FontColor("#6b7280");
                var text = row.RelativeItem().AlignRight().Text(value).FontSize(8).FontColor("#1f2937");
                if (highlight)
                    text.SemiBold();
            });
        }

        private static void RenderCompanyMark(IContainer container, string logoUrl, string tradeName)
        {
            container.Border(1).BorderColor("#d7e1ef").Background(Colors.White).Padding(4).AlignCenter().AlignMiddle().Element(inner =>
            {
                var imageBytes = TryGetImageBytes(logoUrl);
                if (imageBytes is not null)
                {
                    inner.Image(imageBytes).FitArea();
                    return;
                }

                inner.Text(tradeName)
                    .FontSize(10)
                    .FontColor("#5b6472")
                    .SemiBold()
                    .AlignCenter();
            });
        }

        private static byte[]? TryGetImageBytes(string? logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                return null;

            try
            {
                if (logoUrl.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    var commaIndex = logoUrl.IndexOf(',');
                    if (commaIndex > -1)
                        return Convert.FromBase64String(logoUrl[(commaIndex + 1)..]);
                }

                if (File.Exists(logoUrl))
                    return File.ReadAllBytes(logoUrl);

                if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    return new HttpClient().GetByteArrayAsync(uri).GetAwaiter().GetResult();

                return Convert.FromBase64String(logoUrl);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsImageField(ReportSectionField field) =>
            field.Format.Equals("image", StringComparison.OrdinalIgnoreCase)
            || field.Field.Equals("qrCode", StringComparison.OrdinalIgnoreCase);

        private static bool IsTrailingQrField(ReportSectionField field) =>
            field.Field.Equals("qrCode", StringComparison.OrdinalIgnoreCase);

        private static bool ShouldKeepSectionTogether(ReportSectionDefinition section) =>
            section.Type is SectionType.Header or SectionType.Summary or SectionType.Footer
            && !section.Fields.Any(IsLargeField)
            && !section.Fields.Any(IsImageField);

        private static bool IsLargeField(ReportSectionField field) =>
            field.Field.Contains("sello", StringComparison.OrdinalIgnoreCase)
            || field.Field.Contains("cadena", StringComparison.OrdinalIgnoreCase)
            || field.Field.Contains("original", StringComparison.OrdinalIgnoreCase);

        private static bool IsInvoiceDocument(Dictionary<string, object?> data) =>
            data.ContainsKey("invoiceFolio") || data.ContainsKey("uuid") || data.ContainsKey("emisorNombre");
    }
}
