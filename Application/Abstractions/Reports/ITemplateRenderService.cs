namespace Application.Abstractions.Reports
{
    /// <summary>
    /// Renderiza una plantilla HTML con sintaxis Liquid (Fluid) sustituyendo los placeholders con datos reales.
    /// Sintaxis: {{ variable }}, {% for item in items %}...{% endfor %}, {% if condition %}...{% endif %}
    /// </summary>
    public interface ITemplateRenderService
    {
        /// <summary>
        /// Renderiza la plantilla con datos de cabecera y filas de detalle.
        /// </summary>
        /// <param name="htmlTemplate">HTML con placeholders Liquid.</param>
        /// <param name="data">Datos de cabecera (p.ej. saleCode, customerName, totalAmount).</param>
        /// <param name="items">Filas de detalle accesibles en el template como {{ item.productName }}.</param>
        string Render(string htmlTemplate, Dictionary<string, object?> data, List<Dictionary<string, object?>> items);
    }
}
