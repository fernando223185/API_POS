using System.Text.Json.Serialization;

namespace Application.DTOs.Reports
{
    // ─────────────────────────────────────────────
    // MODELOS DE SECCIÓN (JSON interno de la plantilla)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Tipos de sección disponibles en una plantilla
    /// </summary>
    public static class SectionType
    {
        public const string Header = "Header";
        public const string Table = "Table";
        public const string Summary = "Summary";
        public const string Footer = "Footer";
    }

    /// <summary>
    /// Alineación de texto
    /// </summary>
    public static class TextAlign
    {
        public const string Left = "left";
        public const string Center = "center";
        public const string Right = "right";
    }

    /// <summary>
    /// Formato de valor
    /// </summary>
    public static class FieldFormat
    {
        public const string Text = "text";
        public const string Image = "image";
        public const string Currency = "currency";
        public const string Date = "date";
        public const string DateTime = "datetime";
        public const string Number = "number";
        public const string Percentage = "percentage";
    }

    /// <summary>
    /// Campo para secciones Header/Summary/Footer (key-value)
    /// </summary>
    public class ReportSectionField
    {
        /// <summary>Key del campo según el catálogo (ej. "saleCode", "customerName")</summary>
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        /// <summary>Etiqueta que se muestra en el PDF</summary>
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("bold")]
        public bool Bold { get; set; }

        [JsonPropertyName("fontSize")]
        public float FontSize { get; set; } = 9;

        [JsonPropertyName("align")]
        public string Align { get; set; } = TextAlign.Left;

        /// <summary>text | currency | date | datetime | number | percentage</summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = FieldFormat.Text;

        /// <summary>Mostrar en la misma línea que el campo anterior (grid de 2 columnas)</summary>
        [JsonPropertyName("inline")]
        public bool Inline { get; set; }

        /// <summary>Mostrar este campo en el PDF. false = disponible para agregar, pero actualmente oculto.</summary>
        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;
    }

    /// <summary>
    /// Columna para secciones de tipo Table
    /// </summary>
    public class ReportTableColumn
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>Ancho en puntos (0 = relativo / auto)</summary>
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("align")]
        public string Align { get; set; } = TextAlign.Left;

        [JsonPropertyName("format")]
        public string Format { get; set; } = FieldFormat.Text;

        [JsonPropertyName("bold")]
        public bool Bold { get; set; }

        /// <summary>Mostrar esta columna en el PDF. false = disponible para agregar, pero actualmente oculta.</summary>
        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;
    }

    /// <summary>
    /// Definición de una sección dentro de la plantilla
    /// </summary>
    public class ReportSectionDefinition
    {
        /// <summary>Header | Table | Summary | Footer</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("order")]
        public int Order { get; set; }

        /// <summary>
        /// Identificador único de la sección. Coincide con el atributo id="sec-{sectionId}"
        /// del elemento HTML en la plantilla. Permite al frontend ocultar/mostrar la sección
        /// inyectando CSS: #sec-{sectionId} { display: none }
        /// </summary>
        [JsonPropertyName("sectionId")]
        public string? SectionId { get; set; }

        /// <summary>Campos para Header/Summary/Footer</summary>
        [JsonPropertyName("fields")]
        public List<ReportSectionField> Fields { get; set; } = new();

        /// <summary>Columnas para Table</summary>
        [JsonPropertyName("columns")]
        public List<ReportTableColumn> Columns { get; set; } = new();

        /// <summary>Mostrar título de sección en el PDF</summary>
        [JsonPropertyName("showTitle")]
        public bool ShowTitle { get; set; } = true;

        /// <summary>Mostrar esta sección en el PDF. false = disponible para agregar, pero actualmente oculta.</summary>
        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;

        [JsonPropertyName("titleBackground")]
        public string? TitleBackground { get; set; }

        [JsonPropertyName("titleColor")]
        public string? TitleColor { get; set; }

        [JsonPropertyName("bodyBackground")]
        public string? BodyBackground { get; set; }

        [JsonPropertyName("borderColor")]
        public string? BorderColor { get; set; }

        [JsonPropertyName("variant")]
        public string? Variant { get; set; }
    }

    // ─────────────────────────────────────────────
    // DTOs DE PLANTILLA
    // ─────────────────────────────────────────────

    public class CreateReportTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        /// <summary>Sales | Delivery | Quotation | Purchase | Inventory | CashierShift | Invoice</summary>
        public string ReportType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        /// <summary>Configuración de secciones y campos. Define qué información se muestra u oculta en el PDF.</summary>
        public List<ReportSectionDefinition> Sections { get; set; } = new();
        /// <summary>Plantilla HTML con sintaxis Liquid. Cuando está presente se usa Playwright para generar el PDF.</summary>
        public string? HtmlTemplate { get; set; }
    }

    public class UpdateReportTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        /// <summary>Configuración de secciones y campos. Define qué información se muestra u oculta en el PDF.</summary>
        public List<ReportSectionDefinition> Sections { get; set; } = new();
        /// <summary>Plantilla HTML con sintaxis Liquid. Cuando está presente se usa Playwright para generar el PDF.</summary>
        public string? HtmlTemplate { get; set; }
    }

    public class ReportTemplateResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        /// <summary>Configuración de secciones y campos. Visible=true = se muestra en el PDF; Visible=false = disponible para agregar.</summary>
        public List<ReportSectionDefinition> Sections { get; set; } = new();
        /// <summary>Plantilla HTML Liquid. Presente cuando se usa el motor Playwright.</summary>
        public string? HtmlTemplate { get; set; }
        /// <summary>html | sections</summary>
        public string Engine => HtmlTemplate != null ? "html" : "sections";
        public int? CompanyId { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ReportTemplateSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ─────────────────────────────────────────────
    // CATÁLOGO DE CAMPOS DISPONIBLES
    // ─────────────────────────────────────────────

    public class FieldDefinition
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        /// <summary>text | currency | date | datetime | number | percentage</summary>
        public string DefaultFormat { get; set; } = FieldFormat.Text;
        /// <summary>Secciones donde aplica: Header, Table, Summary, Footer</summary>
        public List<string> ApplicableSections { get; set; } = new();
        public string? Description { get; set; }
    }

    public class ReportFieldCatalogDto
    {
        public string ReportType { get; set; } = string.Empty;
        public List<FieldDefinition> Fields { get; set; } = new();
        public List<string> AvailableSectionTypes { get; set; } = new();
    }

    // ─────────────────────────────────────────────
    // DTO PARA GENERAR REPORTE
    // ─────────────────────────────────────────────

    public class GenerateReportDto
    {
        /// <summary>ID de la plantilla a usar (null = usar default del tipo)</summary>
        public int? TemplateId { get; set; }
        /// <summary>Sales | Delivery | Quotation | Purchase | Inventory | CashierShift | Invoice | Payment | WarehouseTransferDispatch | WarehouseTransferReceiving</summary>
        public string ReportType { get; set; } = string.Empty;
        /// <summary>IDs de los documentos a incluir (ventas, cotizaciones, etc.)</summary>
        public List<int> DocumentIds { get; set; } = new();
        /// <summary>Filtros opcionales de fecha</summary>
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? WarehouseId { get; set; }
        public int? CompanyId { get; set; }
        /// <summary>URL base de la aplicación (para generar QR codes que apunten a pantallas específicas)</summary>
        public string? AppBaseUrl { get; set; }
    }

    // ─────────────────────────────────────────────
    // DTO PARA LIVE PREVIEW (sin guardar)
    // ─────────────────────────────────────────────

    public class LivePreviewRequestDto
    {
        /// <summary>Sales | Delivery | Quotation | Purchase | Inventory | CashierShift | Invoice | Payment</summary>
        public string ReportType { get; set; } = string.Empty;
        /// <summary>HTML con sintaxis Liquid. Se renderizará con datos mock del tipo indicado.</summary>
        public string HtmlTemplate { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────
    // DTO PARA PREVIEW EN FRONTEND
    // ─────────────────────────────────────────────

    /// <summary>
    /// Devuelve el esquema exacto de secciones de la plantilla junto con
    /// datos de ejemplo por tipo, para que el frontend renderice un preview
    /// idéntico al PDF generado por QuestPDF sin necesidad de conocer la
    /// estructura interna de cada tipo de reporte.
    /// </summary>
    public class ReportPreviewDataDto
    {
        /// <summary>Secciones con su configuración completa (mismo objeto que usa QuestPDF)</summary>
        public List<ReportSectionDefinition> Sections { get; set; } = new();

        /// <summary>
        /// Valores de ejemplo para campos de tipo Header/Summary/Footer.
        /// Clave = field key (ej. "saleCode"), Valor = dato de ejemplo formateado como string.
        /// </summary>
        public Dictionary<string, string> MockDataRow { get; set; } = new();

        /// <summary>
        /// Filas de ejemplo para secciones de tipo Table (2-3 filas).
        /// Cada fila: clave = field key, valor = dato de ejemplo.
        /// </summary>
        public List<Dictionary<string, string>> MockTableRows { get; set; } = new();

        /// <summary>Nombre de la plantilla</summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>Tipo de reporte: Sales | Delivery | Quotation | Purchase | Inventory | CashierShift</summary>
        public string ReportType { get; set; } = string.Empty;
    }
}
