using Domain.Entities;
using System.Text;

namespace Application.Core.Billing.Documents
{
    /// <summary>
    /// Genera el HTML de una factura CFDI 4.0 — layout idéntico al PDF de InvoicePdfDocument.
    /// Devuelve una página HTML autocontenida (CSS inline) lista para WebView.
    /// </summary>
    public static class InvoiceHtmlDocument
    {
        private const string PrimaryColor = "#1a3c6e";
        private const string LightGray    = "#f5f5f5";
        private const string BorderColor  = "#cccccc";

        public static string GenerateHtml(Invoice invoice)
        {
            var sb = new StringBuilder();

            sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: Arial, sans-serif; font-size: 8pt; color: #222; background: #d0d0d0; }
  .page { width: 794px; min-height: 1123px; background: #fff; margin: 0 auto; padding: 30px; }
  /* ── Encabezado ── */
  .header-row { display: flex; gap: 12px; }
  .emisor-col { flex: 3; }
  .factura-box { flex: 2; border: 1.5px solid " + PrimaryColor + @"; padding: 8px; text-align: center; }
  .emisor-name { font-size: 13pt; font-weight: bold; color: " + PrimaryColor + @"; }
  .emisor-sub  { font-size: 8pt; margin-top: 2px; }
  .factura-title { font-size: 14pt; font-weight: bold; color: " + PrimaryColor + @"; }
  .factura-sub   { font-size: 8pt; margin-top: 3px; }
  .factura-light { font-size: 8pt; color: #888; margin-top: 2px; }
  /* ── Receptor ── */
  .receptor-box { background: " + LightGray + @"; padding: 6px; margin-top: 10px; }
  .receptor-label { font-weight: bold; color: " + PrimaryColor + @"; font-size: 8pt; margin-bottom: 3px; }
  .receptor-row { display: flex; gap: 8px; margin-top: 2px; }
  .receptor-row span { flex: 1; font-size: 8pt; }
  .receptor-name { font-weight: bold; font-size: 9pt; margin-top: 2px; }
  .receptor-domicilio { font-size: 8pt; margin-top: 1px; }
  /* ── Tabla conceptos ── */
  .conceptos { width: 100%; border-collapse: collapse; margin-top: 8px; }
  .conceptos th { background: " + PrimaryColor + @"; color: #fff; font-size: 7pt; font-weight: bold;
                  padding: 4px 3px; text-align: left; }
  .conceptos th.right { text-align: right; }
  .conceptos td { font-size: 7pt; padding: 3px; border-bottom: 1px solid " + BorderColor + @"; }
  .conceptos td.right { text-align: right; }
  .conceptos tr.zebra { background: " + LightGray + @"; }
  /* ── Totales + pago ── */
  .bottom-row { display: flex; gap: 12px; margin-top: 6px; }
  .pago-col { flex: 3; font-size: 8pt; }
  .pago-col p { margin-top: 2px; }
  .totales-box { flex: 2; border: 1.5px solid " + PrimaryColor + @"; }
  .total-line { display: flex; border-bottom: 1px solid " + BorderColor + @"; padding: 4px 8px; }
  .total-label { flex: 1; font-size: 8pt; }
  .total-value { font-size: 8pt; text-align: right; width: 80px; }
  .total-final { display: flex; background: " + PrimaryColor + @"; padding: 5px 8px; }
  .total-final-label { flex: 1; font-size: 10pt; font-weight: bold; color: #fff; }
  .total-final-value { font-size: 10pt; font-weight: bold; color: #fff; text-align: right; width: 80px; }
  /* ── Timbrado ── */
  .timbrado { border-top: 1px solid " + BorderColor + @"; padding-top: 6px; margin-top: 10px; }
  .timbrado-title { font-weight: bold; font-size: 7pt; color: " + PrimaryColor + @"; margin-bottom: 4px; }
  .timbrado-row { display: flex; margin-top: 2px; }
  .timbrado-key { width: 100px; font-weight: bold; font-size: 7pt; flex-shrink: 0; }
  .timbrado-val { font-size: 7pt; color: #444; word-break: break-all; }
  .sello-label { font-weight: bold; font-size: 6.5pt; color: " + PrimaryColor + @"; margin-top: 4px; }
  .sello-val   { font-size: 5.5pt; color: #555; word-break: break-all; margin-top: 1px; }
  /* ── Footer ── */
  .footer { text-align: center; font-size: 7pt; color: #888; margin-top: 20px; border-top: 1px solid " + BorderColor + @"; padding-top: 4px; }
  /* ── QR ── */
  .qr-img { width: 70px; height: 70px; margin-top: 5px; }
</style>
</head>
<body>
<div class='page'>
");

            // ── ENCABEZADO ──────────────────────────────────────────────
            sb.Append("<div class='header-row'>");

            // Emisor
            sb.Append("<div class='emisor-col'>");
            sb.Append($"<div class='emisor-name'>{HtmlEncode(invoice.EmisorNombre)}</div>");
            sb.Append($"<div class='emisor-sub'>RFC: {HtmlEncode(invoice.EmisorRfc)}</div>");
            sb.Append($"<div class='emisor-sub'>Régimen fiscal: {HtmlEncode(invoice.EmisorRegimenFiscal)}</div>");
            sb.Append($"<div class='emisor-sub'>Lugar de expedición: {HtmlEncode(invoice.LugarExpedicion)}</div>");
            sb.Append("</div>");

            // FACTURA box
            sb.Append("<div class='factura-box'>");
            sb.Append("<div class='factura-title'>FACTURA</div>");
            sb.Append($"<div class='factura-sub'><b>Serie-Folio: {HtmlEncode(invoice.Serie)}-{HtmlEncode(invoice.Folio)}</b></div>");
            sb.Append($"<div class='factura-sub'>Fecha: {invoice.InvoiceDate:yyyy-MM-dd HH:mm:ss}</div>");
            sb.Append($"<div class='factura-sub'>Tipo: {HtmlEncode(DescribeTipo(invoice.TipoDeComprobante))}</div>");
            sb.Append("<div class='factura-light'>CFDI 4.0</div>");
            sb.Append("</div>");

            sb.Append("</div>"); // header-row

            // ── RECEPTOR ────────────────────────────────────────────────
            sb.Append("<div class='receptor-box'>");
            sb.Append("<div class='receptor-label'>RECEPTOR</div>");
            sb.Append("<div class='receptor-row'>");
            sb.Append($"<span>RFC: {HtmlEncode(invoice.ReceptorRfc)}</span>");
            sb.Append($"<span>Régimen: {HtmlEncode(invoice.ReceptorRegimenFiscal ?? "-")}</span>");
            sb.Append($"<span>Uso CFDI: {HtmlEncode(invoice.ReceptorUsoCfdi)}</span>");
            sb.Append("</div>");
            sb.Append($"<div class='receptor-name'>{HtmlEncode(invoice.ReceptorNombre)}</div>");
            sb.Append($"<div class='receptor-domicilio'>Domicilio fiscal: {HtmlEncode(invoice.ReceptorDomicilioFiscal)}</div>");
            sb.Append("</div>"); // receptor-box

            // ── CONCEPTOS ────────────────────────────────────────────────
            sb.Append("<table class='conceptos'>");
            sb.Append("<thead><tr>");
            sb.Append("<th style='width:55px'>Clave SAT</th>");
            sb.Append("<th style='width:70px'>No. Ident.</th>");
            sb.Append("<th style='width:40px'>Cant.</th>");
            sb.Append("<th style='width:30px'>UM</th>");
            sb.Append("<th>Descripción</th>");
            sb.Append("<th class='right' style='width:55px'>Precio Unit.</th>");
            sb.Append("<th class='right' style='width:55px'>Importe</th>");
            sb.Append("<th class='right' style='width:35px'>IVA</th>");
            sb.Append("</tr></thead><tbody>");

            bool zebra = false;
            foreach (var d in invoice.Details)
            {
                var rowClass = zebra ? "zebra" : "";
                zebra = !zebra;
                var iva = d.TieneTraslados ? $"${d.TrasladoImporte?.ToString("N2") ?? "-"}" : "-";
                sb.Append($"<tr class='{rowClass}'>");
                sb.Append($"<td>{HtmlEncode(d.ClaveProdServ)}</td>");
                sb.Append($"<td>{HtmlEncode(d.NoIdentificacion ?? "-")}</td>");
                sb.Append($"<td>{d.Cantidad:0.##}</td>");
                sb.Append($"<td>{HtmlEncode(d.ClaveUnidad)}<br/><span style='font-size:6.5pt'>{HtmlEncode(d.Unidad ?? "")}</span></td>");
                sb.Append($"<td>{HtmlEncode(d.Descripcion)}</td>");
                sb.Append($"<td class='right'>${d.ValorUnitario:N2}</td>");
                sb.Append($"<td class='right'>${d.Importe:N2}</td>");
                sb.Append($"<td class='right'>{HtmlEncode(iva)}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");

            // ── TOTALES + PAGO ────────────────────────────────────────────
            sb.Append("<div class='bottom-row' style='margin-top:6px'>");

            // Pago info
            sb.Append("<div class='pago-col'>");
            sb.Append($"<p>Forma de pago: {HtmlEncode(invoice.FormaPago)}</p>");
            sb.Append($"<p>Método de pago: {HtmlEncode(invoice.MetodoPago)}</p>");
            sb.Append($"<p>Moneda: {HtmlEncode(invoice.Moneda)}  |  Tipo de cambio: {invoice.TipoCambio:0.######}</p>");
            if (!string.IsNullOrEmpty(invoice.CondicionesDePago))
                sb.Append($"<p>Condiciones de pago: {HtmlEncode(invoice.CondicionesDePago)}</p>");

            if (!string.IsNullOrEmpty(invoice.QrCode))
            {
                sb.Append($"<img class='qr-img' src='data:image/png;base64,{invoice.QrCode}' alt='QR'/>");
            }
            sb.Append("</div>"); // pago-col

            // Totales
            sb.Append("<div class='totales-box'>");
            sb.Append("<div class='total-line'>");
            sb.Append("<span class='total-label'>Subtotal</span>");
            sb.Append($"<span class='total-value'>${invoice.SubTotal:N2}</span>");
            sb.Append("</div>");
            if (invoice.DiscountAmount > 0)
            {
                sb.Append("<div class='total-line'>");
                sb.Append("<span class='total-label'>Descuento</span>");
                sb.Append($"<span class='total-value'>-${invoice.DiscountAmount:N2}</span>");
                sb.Append("</div>");
            }
            sb.Append("<div class='total-line'>");
            sb.Append("<span class='total-label'>IVA (16%)</span>");
            sb.Append($"<span class='total-value'>${invoice.TaxAmount:N2}</span>");
            sb.Append("</div>");
            sb.Append("<div class='total-final'>");
            sb.Append("<span class='total-final-label'>TOTAL</span>");
            sb.Append($"<span class='total-final-value'>${invoice.Total:N2} {HtmlEncode(invoice.Moneda)}</span>");
            sb.Append("</div>");
            sb.Append("</div>"); // totales-box

            sb.Append("</div>"); // bottom-row

            // ── DATOS DE TIMBRADO ─────────────────────────────────────────
            if (invoice.Status == "Timbrada")
            {
                sb.Append("<div class='timbrado'>");
                sb.Append("<div class='timbrado-title'>DATOS DE TIMBRADO</div>");

                TimbradoRow(sb, "UUID:", invoice.Uuid ?? "-");
                TimbradoRow(sb, "Fecha timbrado:", invoice.TimbradoAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-");
                TimbradoRow(sb, "No. Cert. CFDI:", invoice.NoCertificadoCfdi ?? "-");
                TimbradoRow(sb, "No. Cert. SAT:", invoice.NoCertificadoSat ?? "-");

                if (!string.IsNullOrEmpty(invoice.SelloCfdi))
                {
                    sb.Append("<div class='sello-label'>Sello CFDI:</div>");
                    sb.Append($"<div class='sello-val'>{HtmlEncode(invoice.SelloCfdi)}</div>");
                }
                if (!string.IsNullOrEmpty(invoice.SelloSat))
                {
                    sb.Append("<div class='sello-label'>Sello SAT:</div>");
                    sb.Append($"<div class='sello-val'>{HtmlEncode(invoice.SelloSat)}</div>");
                }
                if (!string.IsNullOrEmpty(invoice.CadenaOriginalSat))
                {
                    sb.Append("<div class='sello-label'>Cadena original del complemento de certificación digital del SAT:</div>");
                    sb.Append($"<div class='sello-val'>{HtmlEncode(invoice.CadenaOriginalSat)}</div>");
                }

                sb.Append("</div>"); // timbrado
            }

            // ── FOOTER ────────────────────────────────────────────────────
            sb.Append("<div class='footer'>Este documento es una representación impresa de un CFDI  |  Página 1 de 1</div>");

            sb.Append("</div></body></html>");

            return sb.ToString();
        }

        private static void TimbradoRow(StringBuilder sb, string key, string value)
        {
            sb.Append("<div class='timbrado-row'>");
            sb.Append($"<span class='timbrado-key'>{HtmlEncode(key)}</span>");
            sb.Append($"<span class='timbrado-val'>{HtmlEncode(value)}</span>");
            sb.Append("</div>");
        }

        private static string DescribeTipo(string tipo) => tipo switch
        {
            "I" => "Ingreso",
            "E" => "Egreso",
            "T" => "Traslado",
            "N" => "Nómina",
            "P" => "Pago",
            _ => tipo
        };

        private static string HtmlEncode(string? s) =>
            string.IsNullOrEmpty(s) ? "" :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
