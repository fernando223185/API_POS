using Application.DTOs.Reports;
using System.Text;

namespace Application.Core.Reports.Engine
{
    /// <summary>
    /// Motor HTML que replica visualmente el layout de ReportPdfEngine.
    /// Genera un HTML autocontenido (CSS inline) idéntico al PDF, listo para WebView.
    /// </summary>
    public static class ReportHtmlEngine
    {
        private const string PrimaryColor = "#1a3c6e";
        private const string LightGray    = "#f5f5f5";
        private const string BorderColor  = "#cccccc";
        private const string HeaderBg     = "#1a3c6e";

        public static string Generate(
            List<ReportSectionDefinition> sections,
            List<Dictionary<string, object?>> dataRows,
            List<Dictionary<string, object?>> tableRows,
            string reportTitle = "Reporte")
        {
            var orderedSections = sections.OrderBy(s => s.Order).ToList();
            var sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: Arial, sans-serif; font-size: 9pt; color: #222; background: #d0d0d0; }
  .page { width: 794px; min-height: 1123px; background: #fff; margin: 0 auto; padding: 30px; }
  /* Header / Footer de página */
  .page-header { display: flex; align-items: baseline; border-bottom: 2px solid " + PrimaryColor + @"; padding-bottom: 4px; margin-bottom: 8px; }
  .page-header-title { flex: 1; font-size: 14pt; font-weight: bold; color: " + PrimaryColor + @"; }
  .page-header-date  { font-size: 7pt; color: #888; }
  .page-footer { border-top: 1px solid " + BorderColor + @"; padding-top: 4px; margin-top: 20px; display: flex; }
  .page-footer-left  { flex: 1; font-size: 7pt; color: #888; }
  .page-footer-right { font-size: 7pt; color: #888; }
  /* Secciones */
  .section { margin-top: 8px; }
  .section-title { background: " + PrimaryColor + @"; color: #fff; font-size: 9pt; font-weight: bold; padding: 4px; }
  .section-body  { background: " + LightGray + @"; padding: 6px; }
  /* Key-Value */
  .kv-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0; }
  .kv-row  { display: flex; padding: 2px 0; }
  .kv-label { width: 120px; font-size: inherit; color: #555; flex-shrink: 0; }
  .kv-value { flex: 1; font-size: inherit; }
  /* Table */
  .report-table { width: 100%; border-collapse: collapse; }
  .report-table th { background: " + HeaderBg + @"; color: #fff; font-size: 8pt; font-weight: bold;
                     padding: 5px 4px; text-align: left; }
  .report-table th.right  { text-align: right; }
  .report-table th.center { text-align: center; }
  .report-table td { font-size: 8pt; padding: 3px 4px; border-bottom: 1px solid " + BorderColor + @"; }
  .report-table td.right  { text-align: right; }
  .report-table td.center { text-align: center; }
  .report-table tr.zebra  { background: " + LightGray + @"; }
  .doc-separator { border: none; border-top: 1px solid " + BorderColor + @"; margin: 20px 0; }
</style>
</head>
<body>
<div class='page'>
");

            // Page header
            sb.Append("<div class='page-header'>");
            sb.Append($"<span class='page-header-title'>{HtmlEncode(reportTitle)}</span>");
            sb.Append($"<span class='page-header-date'>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</span>");
            sb.Append("</div>");

            // Render documents
            if (dataRows.Count > 1)
            {
                for (int i = 0; i < dataRows.Count; i++)
                {
                    if (i > 0)
                        sb.Append("<hr class='doc-separator'/>");
                    RenderDocument(sb, orderedSections, dataRows[i], tableRows);
                }
            }
            else
            {
                var row = dataRows.FirstOrDefault() ?? new();
                RenderDocument(sb, orderedSections, row, tableRows);
            }

            // Page footer
            sb.Append("<div class='page-footer'>");
            sb.Append("<span class='page-footer-left'>EasyPOS — Reporte generado automáticamente</span>");
            sb.Append("<span class='page-footer-right'>1 / 1</span>");
            sb.Append("</div>");

            sb.Append("</div></body></html>");
            return sb.ToString();
        }

        private static void RenderDocument(
            StringBuilder sb,
            List<ReportSectionDefinition> sections,
            Dictionary<string, object?> dataRow,
            List<Dictionary<string, object?>> tableRows)
        {
            foreach (var section in sections)
            {
                switch (section.Type)
                {
                    case SectionType.Header:
                    case SectionType.Summary:
                    case SectionType.Footer:
                        RenderKeyValueSection(sb, section, dataRow);
                        break;
                    case SectionType.Table:
                        RenderTableSection(sb, section, tableRows);
                        break;
                }
            }
        }

        private static void RenderKeyValueSection(
            StringBuilder sb,
            ReportSectionDefinition section,
            Dictionary<string, object?> data)
        {
            sb.Append("<div class='section'>");
            if (section.ShowTitle)
                sb.Append($"<div class='section-title'>{HtmlEncode(section.Title)}</div>");

            sb.Append("<div class='section-body'>");

            // Render in inline pairs (grid-like)
            var fields = section.Fields;
            int i = 0;
            while (i < fields.Count)
            {
                var f = fields[i];
                if (f.Inline && i + 1 < fields.Count)
                {
                    var f2 = fields[i + 1];
                    sb.Append("<div class='kv-grid'>");
                    RenderField(sb, f, data);
                    RenderField(sb, f2, data);
                    sb.Append("</div>");
                    i += 2;
                }
                else
                {
                    RenderField(sb, f, data);
                    i++;
                }
            }

            sb.Append("</div></div>"); // section-body + section
        }

        private static void RenderField(
            StringBuilder sb,
            ReportSectionField field,
            Dictionary<string, object?> data)
        {
            var rawValue = data.TryGetValue(field.Field, out var v) ? v : null;
            var display  = FormatValue(rawValue, field.Format);
            var label    = HtmlEncode(field.Label);
            var value    = HtmlEncode(display);

            var boldStyle  = field.Bold ? "font-weight:bold;" : "";
            var sizeStyle  = $"font-size:{field.FontSize}pt;";
            var alignStyle = field.Align switch
            {
                "right"  => "text-align:right;",
                "center" => "text-align:center;",
                _        => ""
            };

            sb.Append($"<div class='kv-row' style='{sizeStyle}{alignStyle}'>");
            sb.Append($"<span class='kv-label'>{label}:</span>");
            sb.Append($"<span class='kv-value' style='{boldStyle}{sizeStyle}'>{value}</span>");
            sb.Append("</div>");
        }

        private static void RenderTableSection(
            StringBuilder sb,
            ReportSectionDefinition section,
            List<Dictionary<string, object?>> rows)
        {
            if (!section.Columns.Any()) return;

            sb.Append("<div class='section'>");
            if (section.ShowTitle)
                sb.Append($"<div class='section-title'>{HtmlEncode(section.Title)}</div>");

            sb.Append("<table class='report-table'><thead><tr>");
            foreach (var col in section.Columns)
            {
                var alignCls = col.Align switch { "right" => "right", "center" => "center", _ => "" };
                var widthStyle = col.Width > 0 ? $" style='width:{col.Width}px'" : "";
                sb.Append($"<th class='{alignCls}'{widthStyle}>{HtmlEncode(col.Label)}</th>");
            }
            sb.Append("</tr></thead><tbody>");

            bool zebra = false;
            foreach (var row in rows)
            {
                var rowClass = zebra ? "zebra" : "";
                zebra = !zebra;
                sb.Append($"<tr class='{rowClass}'>");
                foreach (var col in section.Columns)
                {
                    var rawValue = row.TryGetValue(col.Field, out var v2) ? v2 : null;
                    var display  = HtmlEncode(FormatValue(rawValue, col.Format));
                    var boldStyle  = col.Bold ? "font-weight:bold;" : "";
                    var alignCls   = col.Align switch { "right" => "right", "center" => "center", _ => "" };
                    sb.Append($"<td class='{alignCls}' style='{boldStyle}'>{display}</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></div>");
        }

        private static string FormatValue(object? value, string format)
        {
            if (value is null) return string.Empty;
            return format switch
            {
                FieldFormat.Currency   => value is decimal d  ? $"${d:N2}"
                                        : decimal.TryParse(value.ToString(), out var dp) ? $"${dp:N2}"
                                        : value.ToString() ?? "",
                FieldFormat.Number     => value is decimal dn ? dn.ToString("0.##")
                                        : value is int ii     ? ii.ToString()
                                        : value.ToString() ?? "",
                FieldFormat.Percentage => value is decimal dp2 ? $"{dp2:0.##}%"
                                        : $"{value}%",
                FieldFormat.Date       => value is DateTime dt ? dt.ToString("dd/MM/yyyy")
                                        : DateTime.TryParse(value.ToString(), out var dtp) ? dtp.ToString("dd/MM/yyyy")
                                        : value.ToString() ?? "",
                FieldFormat.DateTime   => value is DateTime dtm ? dtm.ToString("dd/MM/yyyy HH:mm")
                                        : DateTime.TryParse(value.ToString(), out var dtmp) ? dtmp.ToString("dd/MM/yyyy HH:mm")
                                        : value.ToString() ?? "",
                _                      => value.ToString() ?? ""
            };
        }

        private static string HtmlEncode(string? s) =>
            string.IsNullOrEmpty(s) ? "" :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
