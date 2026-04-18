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

        /// <summary>Campos para Header/Summary/Footer</summary>
        [JsonPropertyName("fields")]
        public List<ReportSectionField> Fields { get; set; } = new();

        /// <summary>Columnas para Table</summary>
        [JsonPropertyName("columns")]
        public List<ReportTableColumn> Columns { get; set; } = new();

        /// <summary>Mostrar título de sección en el PDF</summary>
        [JsonPropertyName("showTitle")]
        public bool ShowTitle { get; set; } = true;
    }

    // ─────────────────────────────────────────────
    // DTOs DE PLANTILLA
    // ─────────────────────────────────────────────

    public class CreateReportTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        /// <summary>Sales | Delivery | Quotation | Purchase | Inventory | CashierShift</summary>
        public string ReportType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public List<ReportSectionDefinition> Sections { get; set; } = new();
    }

    public class UpdateReportTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public List<ReportSectionDefinition> Sections { get; set; } = new();
    }

    public class ReportTemplateResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public List<ReportSectionDefinition> Sections { get; set; } = new();
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
        /// <summary>Sales | Delivery | Quotation | Purchase | Inventory | CashierShift</summary>
        public string ReportType { get; set; } = string.Empty;
        /// <summary>IDs de los documentos a incluir (ventas, cotizaciones, etc.)</summary>
        public List<int> DocumentIds { get; set; } = new();
        /// <summary>Filtros opcionales de fecha</summary>
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? WarehouseId { get; set; }
        public int? CompanyId { get; set; }
    }
}
