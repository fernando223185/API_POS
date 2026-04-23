using Application.Abstractions.Reports;
using Fluid;
using Fluid.Values;

namespace Application.Core.Reports.Engine
{
    /// <summary>
    /// Implementación de ITemplateRenderService usando Fluid (Liquid syntax).
    /// Fluid está diseñado para templates de usuario: sandboxed, sin acceso a reflexión .NET.
    /// </summary>
    public class FluidTemplateRenderService : ITemplateRenderService
    {
        private static readonly FluidParser _parser = new();

        public string Render(string htmlTemplate, Dictionary<string, object?> data, List<Dictionary<string, object?>> items)
        {
            if (!_parser.TryParse(htmlTemplate, out var template, out var error))
                throw new InvalidOperationException($"Error al parsear la plantilla HTML: {error}");

            var context = new TemplateContext();

            // Inyectar variables de cabecera
            foreach (var (key, value) in data)
                context.SetValue(key, value is null ? NilValue.Empty : FluidValue.Create(value, context.Options));

            // Inyectar items como lista de objetos Liquid
            var liquidItems = items.Select(row =>
            {
                var dict = new Dictionary<string, FluidValue>(StringComparer.OrdinalIgnoreCase);
                foreach (var (k, v) in row)
                    dict[k] = v is null ? NilValue.Empty : FluidValue.Create(v, context.Options);
                return (FluidValue)new DictionaryValue(dict);
            }).ToList();

            context.SetValue("items", liquidItems);
            context.SetValue("now", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            return template.Render(context);
        }
    }

    /// <summary>
    /// FluidValue que envuelve un Dictionary para acceso por clave en templates Liquid.
    /// Permite {{ item.productName }} cuando item es un Dictionary.
    /// </summary>
    internal class DictionaryValue : FluidValue
    {
        private readonly Dictionary<string, FluidValue> _dict;

        public DictionaryValue(Dictionary<string, FluidValue> dict)
            => _dict = dict;

        public override FluidValues Type => FluidValues.Object;

        public override bool Equals(FluidValue other)
            => other is DictionaryValue d && ReferenceEquals(_dict, d._dict);

        public override bool ToBooleanValue() => true;

        public override decimal ToNumberValue() => 0;

        public override object ToObjectValue() => _dict;

        public override string ToStringValue() => string.Empty;

        protected override FluidValue GetValue(string name, TemplateContext context)
        {
            if (_dict.TryGetValue(name, out var val))
                return val;
            return NilValue.Empty;
        }

        public override void WriteTo(System.IO.TextWriter writer, System.Text.Encodings.Web.TextEncoder encoder, System.Globalization.CultureInfo cultureInfo)
            => writer.Write(string.Empty);
    }
}
