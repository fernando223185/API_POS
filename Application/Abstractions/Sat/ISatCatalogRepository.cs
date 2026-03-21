using Domain.Entities;

namespace Application.Abstractions.Sat
{
    /// <summary>
    /// Repositorio para consultar catálogos oficiales del SAT
    /// </summary>
    public interface ISatCatalogRepository
    {
        // Uso del CFDI
        Task<List<SatUsoCfdi>> GetUsoCfdiAsync(bool? personaFisica = null, bool? personaMoral = null);
        Task<SatUsoCfdi?> GetUsoCfdiByCodigoAsync(string codigo);

        // Régimen Fiscal
        Task<List<SatRegimenFiscal>> GetRegimenFiscalAsync(bool? personaFisica = null, bool? personaMoral = null);
        Task<SatRegimenFiscal?> GetRegimenFiscalByCodigoAsync(string codigo);

        // Forma de Pago
        Task<List<SatFormaPago>> GetFormaPagoAsync();
        Task<SatFormaPago?> GetFormaPagoByCodigoAsync(string codigo);

        // Método de Pago
        Task<List<SatMetodoPago>> GetMetodoPagoAsync();
        Task<SatMetodoPago?> GetMetodoPagoByCodigoAsync(string codigo);

        // Tipo de Comprobante
        Task<List<SatTipoComprobante>> GetTipoComprobanteAsync();
        Task<SatTipoComprobante?> GetTipoComprobanteByCodigoAsync(string codigo);

        // Unidad de Medida
        Task<List<SatUnidadMedida>> GetUnidadMedidaAsync(string? search = null);
        Task<SatUnidadMedida?> GetUnidadMedidaByCodigoAsync(string codigo);

        // Producto/Servicio
        Task<List<SatProductoServicio>> GetProductoServicioAsync(string? search = null, int limit = 50);
        Task<SatProductoServicio?> GetProductoServicioByCodigoAsync(string codigo);
    }
}
