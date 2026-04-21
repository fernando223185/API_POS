using Application.DTOs.Reports;
using System.Text;

namespace Application.Core.Reports.Engine
{
    public static class ReportHtmlEngine
    {
        private const string PrimaryColor = "#1a3c6e";
        private const string LightGray = "#f5f5f5";
        private const string BorderColor = "#cccccc";

        private sealed record ThemePalette(string Primary, string Surface, string Border, string TextOnPrimary);

        public static string Generate(
            List<ReportSectionDefinition> sections,
            List<Dictionary<string, object?>> dataRows,
            List<Dictionary<string, object?>> tableRows,
            string reportTitle = "Reporte")
        {
            var orderedSections = sections.OrderBy(s => s.Order).ToList();
            var palette = ResolvePagePalette(orderedSections);
            var sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: Arial, sans-serif; font-size: 9pt; color: #222; background: #dde3ec; padding: 24px 0; }
    .page { width: 794px; min-height: 1123px; background: #fff; margin: 0 auto; padding: 30px; }
  .page-header { display: flex; align-items: baseline; border-bottom: 2px solid " + palette.Primary + @"; padding-bottom: 8px; margin-bottom: 14px; }
  .page-header-title { flex: 1; font-size: 14pt; font-weight: bold; color: " + palette.Primary + @"; letter-spacing: .03em; }
  .page-header-date  { font-size: 7pt; color: #888; }
  .page-footer { border-top: 1px solid " + palette.Border + @"; padding-top: 6px; margin-top: 24px; display: flex; }
  .page-footer-left  { flex: 1; font-size: 7pt; color: #888; }
  .page-footer-right { font-size: 7pt; color: #888; }
    .section { margin-top: 14px; }
    .section-title { font-size: 10pt; font-weight: bold; padding: 10px 14px; letter-spacing: .02em; }
    .invoice-hero { margin-bottom: 16px; padding: 14px; background: #eef3fb; border: 1px solid #d7e1ef; }
    .invoice-hero-top { display: flex; justify-content: space-between; align-items: flex-start; gap: 18px; }
    .invoice-logo-wrap { width: 240px; height: 108px; background: #ffffff; border: 1px solid #d7e1ef; padding: 4px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; overflow: hidden; }
    .invoice-logo { width: 100%; height: 100%; object-fit: contain; }
    .invoice-meta-card { min-width: 230px; max-width: 250px; background: #ffffff; border: 1px solid #d7e1ef; padding: 12px 14px; margin-left: auto; }
    .invoice-meta-line { display: flex; justify-content: space-between; gap: 12px; padding: 4px 0; font-size: 8pt; }
    .invoice-meta-label { color: #6b7280; font-weight: 600; }
    .invoice-meta-value { color: #1f2937; font-weight: 700; text-align: right; }
    .invoice-hero-bottom { margin-top: 16px; display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .invoice-party { background: #ffffff; padding: 12px 14px; border: 1px solid #d7e1ef; }
    .invoice-party-label { font-size: 7pt; letter-spacing: .12em; text-transform: uppercase; color: #5273a7; margin-bottom: 6px; }
    .invoice-party-name { font-size: 10pt; font-weight: 700; color: #17345f; margin-bottom: 4px; }
    .invoice-party-meta { font-size: 8pt; color: #5b6472; line-height: 1.45; }
    .section-body  { padding: 16px; }
    .variant-fiscal .section-body, .variant-technical .section-body { padding: 16px; }
    .variant-proposal .section-body, .variant-delivery .section-body { box-shadow: inset 0 1px 0 rgba(255,255,255,.55); }
    .kv-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px 24px; }
    .variant-fiscal .kv-grid { grid-template-columns: 1fr 1fr 1fr; }
    .variant-fiscal .kv-grid.has-image { grid-template-columns: minmax(150px, 180px) 1fr 1fr; align-items: start; }
    .kv-row  { display: flex; padding: 6px 0; gap: 12px; align-items: flex-start; }
    .kv-row.block { display: block; }
    .kv-row.image { display: block; }
    .kv-label { width: 160px; color: #5e6572; flex-shrink: 0; font-weight: 600; }
    .kv-row.block .kv-label { width: auto; display: block; margin-bottom: 4px; }
    .kv-row.image .kv-label { width: auto; display: block; margin-bottom: 8px; }
    .kv-value { flex: 1; line-height: 1.45; color: #1f2430; }
    .kv-row.block .kv-value { display: block; padding: 8px 10px; background: #ffffff; border: 1px solid rgba(0,0,0,.08); white-space: normal; word-break: break-word; }
    .kv-row.image .kv-value { display: block; }
    .image-tail { display: flex; justify-content: flex-end; padding-top: 22px; margin-top: 8px; border-top: 1px solid rgba(32,74,135,.16); }
    .qr-box { width: 182px; padding: 12px; background: #ffffff; border: 1px solid rgba(32,74,135,.18); }
    .qr-title { font-size: 7pt; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: #204a87; margin-bottom: 8px; text-align: center; }
    .qr-img { width: 100%; aspect-ratio: 1 / 1; object-fit: contain; display: block; background: #fff; border: 1px solid rgba(0,0,0,.08); }
    .qr-caption { margin-top: 8px; font-size: 6.8pt; color: #6b7280; text-align: center; line-height: 1.35; }
  .report-table { width: 100%; border-collapse: separate; border-spacing: 0; margin-top: 2px; }
    .report-table th { font-size: 9.5pt; font-weight: bold; padding: 10px 10px; text-align: left; }
  .report-table th.right { text-align: right; }
  .report-table th.center { text-align: center; }
    .report-table td { font-size: 9pt; padding: 10px 10px; vertical-align: top; }
  .report-table td.right { text-align: right; }
  .report-table td.center { text-align: center; }
  .doc-separator { border: none; border-top: 1px solid " + palette.Border + @"; margin: 24px 0; }
</style>
</head>
<body>
<div class='page'>");

            var headerRow = dataRows.FirstOrDefault() ?? new Dictionary<string, object?>();
            if (IsInvoiceDocument(headerRow))
                RenderInvoiceHero(sb, headerRow, reportTitle);
            else
            {
                sb.Append("<div class='page-header'>");
                sb.Append($"<span class='page-header-title'>{HtmlEncode(reportTitle)}</span>");
                sb.Append($"<span class='page-header-date'>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</span>");
                sb.Append("</div>");
            }

            if (dataRows.Count > 1)
            {
                for (int i = 0; i < dataRows.Count; i++)
                {
                    if (i > 0)
                        sb.Append("<hr class='doc-separator'/>");
                    RenderDocument(sb, orderedSections, dataRows[i], tableRows, palette);
                }
            }
            else
            {
                var row = dataRows.FirstOrDefault() ?? new();
                RenderDocument(sb, orderedSections, row, tableRows, palette);
            }

            if (dataRows.Count == 1)
            {
                var row = dataRows.FirstOrDefault() ?? new();
                RenderTrailingQr(sb, row);
            }

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
            List<Dictionary<string, object?>> tableRows,
            ThemePalette palette)
        {
            for (int sIdx = 0; sIdx < sections.Count; sIdx++)
            {
                var section = sections[sIdx];
                switch (section.Type)
                {
                    case SectionType.Header:
                    case SectionType.Summary:
                    case SectionType.Footer:
                        RenderKeyValueSection(sb, section, dataRow, sIdx, palette);
                        break;
                    case SectionType.Table:
                        RenderTableSection(sb, section, tableRows, sIdx, palette);
                        break;
                }
            }
        }

        private static void RenderKeyValueSection(
            StringBuilder sb,
            ReportSectionDefinition section,
            Dictionary<string, object?> data,
            int sIdx,
            ThemePalette palette)
        {
            var sectionTitleBg = section.TitleBackground ?? palette.Primary;
            var sectionTitleColor = section.TitleColor ?? palette.TextOnPrimary;
            var sectionBodyBg = section.BodyBackground ?? palette.Surface;
            var sectionBorder = section.BorderColor ?? palette.Border;

            sb.Append($"<div class='section variant-{HtmlEncode(section.Variant ?? "default") }'>");
            if (section.ShowTitle)
                sb.Append($"<div class='section-title' style='background:{sectionTitleBg};color:{sectionTitleColor};'>{HtmlEncode(section.Title)}</div>");

            sb.Append($"<div class='section-body' style='background:{sectionBodyBg};border:1px solid {sectionBorder};border-top:none;'>");

            var regularFields = section.Fields.Where(f => !IsTrailingQrField(f) && !IsImageField(f)).ToList();
            var imageFields = section.Fields.Where(f => IsImageField(f) && !IsTrailingQrField(f)).ToList();

            int i = 0;
            while (i < regularFields.Count)
            {
                var field = regularFields[i];
                
                // Si este campo NO es inline pero el siguiente SÍ es inline → emparejarlos
                if (!field.Inline && i + 1 < regularFields.Count && regularFields[i + 1].Inline)
                {
                    var nextField = regularFields[i + 1];
                    sb.Append("<div class='kv-grid'>");
                    RenderField(sb, field, data, sIdx, i);
                    RenderField(sb, nextField, data, sIdx, i + 1);
                    sb.Append("</div>");
                    i += 2;
                }
                else
                {
                    RenderField(sb, field, data, sIdx, i);
                    i++;
                }
            }

            if (imageFields.Count > 0)
            {
                sb.Append("<div class='image-tail'>");
                for (int imageIdx = 0; imageIdx < imageFields.Count; imageIdx++)
                    RenderField(sb, imageFields[imageIdx], data, sIdx, regularFields.Count + imageIdx);
                sb.Append("</div>");
            }

            sb.Append("</div></div>");
        }

        private static void RenderField(
            StringBuilder sb,
            ReportSectionField field,
            Dictionary<string, object?> data,
            int sIdx,
            int iIdx)
        {
            var rawValue = data.TryGetValue(field.Field, out var value) ? value : null;
            var display = HtmlEncode(FormatValue(rawValue, field.Format));
            var label = HtmlEncode(field.Label);

            if (IsImageField(field))
            {
                if (string.IsNullOrWhiteSpace(display))
                    return;

                sb.Append($"<div class='kv-row image' data-s-idx='{sIdx}' data-i-idx='{iIdx}' data-kind='field'>");
                sb.Append($"<span class='kv-label'>{label}</span>");
                sb.Append("<span class='kv-value'><div class='qr-box'>");
                sb.Append($"<img class='qr-img' src='data:image/png;base64,{display}' alt='{label}'/>");
                sb.Append("<div class='qr-caption'>Escanea para validar CFDI</div>");
                sb.Append("</div></span></div>");
                return;
            }

            var boldStyle = field.Bold ? "font-weight:bold;" : string.Empty;
            var sizeStyle = $"font-size:{field.FontSize}pt;";
            var isLongValue = display.Length > 70
                || field.Field.Contains("sello", StringComparison.OrdinalIgnoreCase)
                || field.Field.Contains("cadena", StringComparison.OrdinalIgnoreCase);
            var alignStyle = field.Align switch
            {
                "right" => "text-align:right;",
                "center" => "text-align:center;",
                _ => string.Empty
            };

            sb.Append($"<div class='kv-row{(isLongValue ? " block" : string.Empty)}' data-s-idx='{sIdx}' data-i-idx='{iIdx}' data-kind='field' style='{sizeStyle}{alignStyle}'>");
            sb.Append($"<span class='kv-label'>{label}:</span>");
            sb.Append($"<span class='kv-value' style='{boldStyle}{sizeStyle}'>{display}</span>");
            sb.Append("</div>");
        }

        private static void RenderTableSection(
            StringBuilder sb,
            ReportSectionDefinition section,
            List<Dictionary<string, object?>> rows,
            int sIdx,
            ThemePalette palette)
        {
            if (!section.Columns.Any()) return;

            var sectionTitleBg = section.TitleBackground ?? palette.Primary;
            var sectionTitleColor = section.TitleColor ?? palette.TextOnPrimary;
            var sectionBodyBg = section.BodyBackground ?? palette.Surface;
            var sectionBorder = section.BorderColor ?? palette.Border;

            sb.Append($"<div class='section variant-{HtmlEncode(section.Variant ?? "default") }'>");
            if (section.ShowTitle)
                sb.Append($"<div class='section-title' style='background:{sectionTitleBg};color:{sectionTitleColor};'>{HtmlEncode(section.Title)}</div>");

            sb.Append($"<div class='section-body' style='background:{sectionBodyBg};border:1px solid {sectionBorder};border-top:none;padding:12px;'>");
            sb.Append("<table class='report-table'><thead><tr>");

            for (int colIdx = 0; colIdx < section.Columns.Count; colIdx++)
            {
                var col = section.Columns[colIdx];
                var alignClass = col.Align switch { "right" => "right", "center" => "center", _ => string.Empty };
                var widthStyle = col.Width > 0 ? $"width:{col.Width}px;" : string.Empty;
                sb.Append($"<th class='{alignClass}' data-s-idx='{sIdx}' data-i-idx='{colIdx}' data-kind='column' style='background:{sectionTitleBg};color:{sectionTitleColor};{widthStyle}'>{HtmlEncode(col.Label)}</th>");
            }

            sb.Append("</tr></thead><tbody>");

            bool zebra = false;
            foreach (var row in rows)
            {
                var rowBackground = zebra ? sectionBodyBg : "#ffffff";
                zebra = !zebra;
                sb.Append($"<tr style='background:{rowBackground};'>");

                for (int colIdx = 0; colIdx < section.Columns.Count; colIdx++)
                {
                    var col = section.Columns[colIdx];
                    var rawValue = row.TryGetValue(col.Field, out var value) ? value : null;
                    var display = HtmlEncode(FormatValue(rawValue, col.Format));
                    var alignClass = col.Align switch { "right" => "right", "center" => "center", _ => string.Empty };
                    var boldStyle = col.Bold ? "font-weight:bold;" : string.Empty;
                    sb.Append($"<td class='{alignClass}' data-s-idx='{sIdx}' data-i-idx='{colIdx}' data-kind='column' style='border-bottom:1px solid {sectionBorder};{boldStyle}'>{display}</td>");
                }

                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></div></div>");
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
                firstStyled?.TitleColor ?? "#ffffff");
        }

        private static string FormatValue(object? value, string format)
        {
            if (value is null) return string.Empty;
            return format switch
            {
                FieldFormat.Currency => value is decimal d ? $"${d:N2}" : decimal.TryParse(value.ToString(), out var dp) ? $"${dp:N2}" : value.ToString() ?? string.Empty,
                FieldFormat.Number => value is decimal dn ? dn.ToString("0.##") : value is int ii ? ii.ToString() : value.ToString() ?? string.Empty,
                FieldFormat.Percentage => value is decimal dp2 ? $"{dp2:0.##}%" : $"{value}%",
                FieldFormat.Date => value is DateTime dt ? dt.ToString("dd/MM/yyyy") : DateTime.TryParse(value.ToString(), out var dtp) ? dtp.ToString("dd/MM/yyyy") : value.ToString() ?? string.Empty,
                FieldFormat.DateTime => value is DateTime dtm ? dtm.ToString("dd/MM/yyyy HH:mm") : DateTime.TryParse(value.ToString(), out var dtmp) ? dtmp.ToString("dd/MM/yyyy HH:mm") : value.ToString() ?? string.Empty,
                _ => value.ToString() ?? string.Empty
            };
        }

        private static string HtmlEncode(string? s) =>
            string.IsNullOrEmpty(s) ? string.Empty :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        private static void RenderTrailingQr(StringBuilder sb, Dictionary<string, object?> data)
        {
            var qrValue = data.TryGetValue("qrCode", out var rawQr) ? rawQr?.ToString() : null;
            if (string.IsNullOrWhiteSpace(qrValue))
                return;

            sb.Append("<div class='section'><div class='image-tail'>");
            sb.Append("<div class='qr-box'>");
            sb.Append("<div class='qr-title'>Validacion SAT</div>");
            sb.Append($"<img class='qr-img' src='data:image/png;base64,{HtmlEncode(qrValue)}' alt='Código QR'/>");
            sb.Append("<div class='qr-caption'>Escanea este codigo para validar el CFDI en el SAT</div>");
            sb.Append("</div></div></div>");
        }

        private static void RenderInvoiceHero(StringBuilder sb, Dictionary<string, object?> data, string reportTitle)
        {
            var serie = HtmlEncode(data.TryGetValue("invoiceSerie", out var serieValue) ? serieValue?.ToString() : string.Empty);
            var folio = HtmlEncode(data.TryGetValue("invoiceFolio", out var folioValue) ? folioValue?.ToString() : string.Empty);
            var emisor = HtmlEncode(data.TryGetValue("emisorNombre", out var emisorValue) ? emisorValue?.ToString() : string.Empty);
            var logoUrl = HtmlEncode(data.TryGetValue("companyLogoUrl", out var logoValue) ? logoValue?.ToString() : string.Empty);
            var tradeName = HtmlEncode(data.TryGetValue("companyTradeName", out var tradeValue) ? tradeValue?.ToString() : emisor);
            var emisorRfc = HtmlEncode(data.TryGetValue("emisorRfc", out var emisorRfcValue) ? emisorRfcValue?.ToString() : string.Empty);
            var receptor = HtmlEncode(data.TryGetValue("receptorNombre", out var receptorValue) ? receptorValue?.ToString() : string.Empty);
            var receptorRfc = HtmlEncode(data.TryGetValue("receptorRfc", out var receptorRfcValue) ? receptorRfcValue?.ToString() : string.Empty);
            var usoCfdi = HtmlEncode(data.TryGetValue("receptorUsoCfdi", out var usoCfdiValue) ? usoCfdiValue?.ToString() : string.Empty);
            var fecha = HtmlEncode(FormatValue(data.TryGetValue("invoiceDate", out var dateValue) ? dateValue : null, FieldFormat.DateTime));
            var total = HtmlEncode(FormatValue(data.TryGetValue("total", out var totalValue) ? totalValue : null, FieldFormat.Currency));

            sb.Append("<div class='invoice-hero'>");
            sb.Append("<div class='invoice-hero-top'>");
            sb.Append("<div class='invoice-logo-wrap'>");
            if (!string.IsNullOrWhiteSpace(logoUrl))
                sb.Append($"<img class='invoice-logo' src='{logoUrl}' alt='Logo empresa'/>");
            else
                sb.Append($"<div style='font-size:10pt;color:#5b6472;font-weight:600;text-align:center;'>{tradeName}</div>");
            sb.Append("</div>");
            sb.Append("<div class='invoice-meta-card'>");
            HeroLine(sb, "Folio", $"{serie}-{folio}");
            HeroLine(sb, "Fecha", fecha);
            HeroLine(sb, "Total", total);
            sb.Append("</div></div>");
            sb.Append("<div class='invoice-hero-bottom'>");
            HeroParty(sb, "Emisor", emisor, $"RFC: {emisorRfc}");
            HeroParty(sb, "Receptor", receptor, $"RFC: {receptorRfc}<br/>Uso CFDI: {usoCfdi}");
            sb.Append("</div></div>");
        }

        private static void HeroLine(StringBuilder sb, string label, string value)
        {
            sb.Append("<div class='invoice-meta-line'>");
            sb.Append($"<span class='invoice-meta-label'>{HtmlEncode(label)}</span>");
            sb.Append($"<span class='invoice-meta-value'>{value}</span>");
            sb.Append("</div>");
        }

        private static void HeroParty(StringBuilder sb, string label, string name, string meta)
        {
            sb.Append("<div class='invoice-party'>");
            sb.Append($"<div class='invoice-party-label'>{HtmlEncode(label)}</div>");
            sb.Append($"<div class='invoice-party-name'>{name}</div>");
            sb.Append($"<div class='invoice-party-meta'>{meta}</div>");
            sb.Append("</div>");
        }

        private static bool IsImageField(ReportSectionField field) =>
            field.Format.Equals("image", StringComparison.OrdinalIgnoreCase)
            || field.Field.Equals("qrCode", StringComparison.OrdinalIgnoreCase);

        private static bool IsTrailingQrField(ReportSectionField field) =>
            field.Field.Equals("qrCode", StringComparison.OrdinalIgnoreCase);

        private static bool IsInvoiceDocument(Dictionary<string, object?> data) =>
            data.ContainsKey("invoiceFolio") || data.ContainsKey("uuid") || data.ContainsKey("emisorNombre");
    }
}
