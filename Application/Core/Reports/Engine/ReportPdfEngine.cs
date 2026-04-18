using Application.DTOs.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Element(c => RenderPageHeader(c, reportTitle));
                    page.Footer().Element(RenderPageFooter);

                    page.Content().Column(col =>
                    {
                        // Si hay múltiples documentos, renderizar cada uno con sus secciones
                        if (dataRows.Count > 1)
                        {
                            foreach (var (dataRow, idx) in dataRows.Select((r, i) => (r, i)))
                            {
                                if (idx > 0)
                                    col.Item().PaddingTop(20).LineHorizontal(1).LineColor(BorderColor);

                                RenderDocument(col, orderedSections, dataRow, tableRows, isSingleDoc: false);
                            }
                        }
                        else
                        {
                            var dataRow = dataRows.FirstOrDefault() ?? new();
                            RenderDocument(col, orderedSections, dataRow, tableRows, isSingleDoc: true);
                        }
                    });
                });
            }).GeneratePdf();
        }

        // ─────────────────────────────────────────────
        // Cabecera y pie de página del PDF
        // ─────────────────────────────────────────────

        private static void RenderPageHeader(IContainer container, string reportTitle)
        {
            container
                .BorderBottom(2).BorderColor(PrimaryColor)
                .PaddingBottom(4)
                .Row(row =>
                {
                    row.RelativeItem().Text(reportTitle)
                        .Bold().FontSize(14).FontColor(PrimaryColor);
                    row.ConstantItem(120).Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(7).FontColor(Colors.Grey.Medium).AlignRight();
                });
        }

        private static void RenderPageFooter(IContainer container)
        {
            container
                .BorderTop(1).BorderColor(BorderColor)
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
                        col.Item().Element(c => RenderKeyValueSection(c, section, dataRow));
                        break;

                    case SectionType.Table:
                        col.Item().Element(c => RenderTableSection(c, section, tableRows));
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
            Dictionary<string, object?> data)
        {
            container.Column(col =>
            {
                if (section.ShowTitle)
                {
                    col.Item()
                        .Background(PrimaryColor)
                        .Padding(4)
                        .Text(section.Title)
                        .FontSize(9).Bold().FontColor(Colors.White);
                }

                col.Item().Background(LightGray).Padding(6).Column(inner =>
                {
                    // Renderizar en grid de 2 columnas (pares Inline)
                    var fields = section.Fields;
                    int i = 0;
                    while (i < fields.Count)
                    {
                        var f = fields[i];
                        if (f.Inline && i + 1 < fields.Count)
                        {
                            // Dos campos en la misma fila
                            var f2 = fields[i + 1];
                            inner.Item().Row(row =>
                            {
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

            container.PaddingVertical(2).Row(row =>
            {
                row.ConstantItem(120)
                    .Text($"{label}:")
                    .FontSize(field.FontSize).FontColor(Colors.Grey.Darken2);

                var valueContainer = row.RelativeItem();
                if (field.Bold)
                    valueContainer.Text(displayValue).FontSize(field.FontSize).Bold();
                else
                    valueContainer.Text(displayValue).FontSize(field.FontSize);
            });
        }

        // ─────────────────────────────────────────────
        // Sección Table
        // ─────────────────────────────────────────────

        private static void RenderTableSection(
            IContainer container,
            ReportSectionDefinition section,
            List<Dictionary<string, object?>> rows)
        {
            if (!section.Columns.Any()) return;

            container.Column(col =>
            {
                if (section.ShowTitle)
                {
                    col.Item()
                        .Background(PrimaryColor)
                        .Padding(4)
                        .Text(section.Title)
                        .FontSize(9).Bold().FontColor(Colors.White);
                }

                col.Item().Table(table =>
                {
                    // Definir columnas
                    table.ColumnsDefinition(cols =>
                    {
                        foreach (var c in section.Columns)
                        {
                            if (c.Width > 0)
                                cols.ConstantColumn(c.Width);
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
                                .Background(HeaderBg)
                                .PaddingHorizontal(4).PaddingVertical(5)
                                .Text(c.Label)
                                .FontSize(8).Bold().FontColor(Colors.White);
                        }
                    });

                    // Filas
                    bool zebra = false;
                    foreach (var row in rows)
                    {
                        string bg = zebra ? Colors.White : LightGray;
                        zebra = !zebra;

                        foreach (var col2 in section.Columns)
                        {
                            var rawValue = row.TryGetValue(col2.Field, out var v) ? v : null;
                            var display = FormatValue(rawValue, col2.Format);

                            var cell = table.Cell()
                                .Background(bg)
                                .BorderBottom(1).BorderColor(BorderColor)
                                .PaddingHorizontal(4).PaddingVertical(3);

                            if (col2.Bold)
                                cell.Text(display).FontSize(8).Bold();
                            else
                                cell.Text(display).FontSize(8);
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

        private static void ApplyAlign(TextSpanDescriptor text, string align)
        {
            // TextSpanDescriptor en QuestPDF no tiene métodos de alineación directos.
            // La alineación se controla desde el contenedor padre mediante el método
            // .AlignCenter() / .AlignRight() sobre el IContainer, no sobre el span.
            // Este método existe como extensión futura; la alineación real se aplica
            // en el IContainer que envuelve el texto (ver uso en RenderField / RenderTableSection).
        }
    }
}
