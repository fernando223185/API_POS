using Application.Abstractions.Sat;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SatCatalogRepository : ISatCatalogRepository
    {
        private readonly POSDbContext _context;

        public SatCatalogRepository(POSDbContext context)
        {
            _context = context;
        }

        // ========================================
        // USO DEL CFDI
        // ========================================
        public async Task<List<SatUsoCfdi>> GetUsoCfdiAsync(bool? personaFisica = null, bool? personaMoral = null)
        {
            var query = _context.SatUsoCfdi.Where(x => x.IsActive);

            if (personaFisica.HasValue)
                query = query.Where(x => x.AplicaPersonaFisica == personaFisica.Value);

            if (personaMoral.HasValue)
                query = query.Where(x => x.AplicaPersonaMoral == personaMoral.Value);

            return await query.OrderBy(x => x.Codigo).ToListAsync();
        }

        public async Task<SatUsoCfdi?> GetUsoCfdiByCodigoAsync(string codigo)
        {
            return await _context.SatUsoCfdi.FirstOrDefaultAsync(x => x.Codigo == codigo);
        }

        // ========================================
        // RÉGIMEN FISCAL
        // ========================================
        public async Task<List<SatRegimenFiscal>> GetRegimenFiscalAsync(bool? personaFisica = null, bool? personaMoral = null)
        {
            var query = _context.SatRegimenFiscal.Where(x => x.IsActive);

            if (personaFisica.HasValue)
                query = query.Where(x => x.AplicaPersonaFisica == personaFisica.Value);

            if (personaMoral.HasValue)
                query = query.Where(x => x.AplicaPersonaMoral == personaMoral.Value);

            return await query.OrderBy(x => x.Codigo).ToListAsync();
        }

        public async Task<SatRegimenFiscal?> GetRegimenFiscalByCodigoAsync(string codigo)
        {
            return await _context.SatRegimenFiscal.FirstOrDefaultAsync(x => x.Codigo == codigo);
        }

        // ========================================
        // FORMA DE PAGO
        // ========================================
        public async Task<List<SatFormaPago>> GetFormaPagoAsync()
        {
            return await _context.SatFormaPago
                .Where(x => x.IsActive)
                .OrderBy(x => x.Codigo)
                .ToListAsync();
        }

        public async Task<SatFormaPago?> GetFormaPagoByCodigoAsync(string codigo)
        {
            return await _context.SatFormaPago.FirstOrDefaultAsync(x => x.Codigo == codigo);
        }

        // ========================================
        // MÉTODO DE PAGO
        // ========================================
        public async Task<List<SatMetodoPago>> GetMetodoPagoAsync()
        {
            return await _context.SatMetodoPago
                .Where(x => x.IsActive)
                .OrderBy(x => x.Codigo)
                .ToListAsync();
        }

        public async Task<SatMetodoPago?> GetMetodoPagoByCodigoAsync(string codigo)
        {
            return await _context.SatMetodoPago.FirstOrDefaultAsync(x => x.Codigo == codigo);
        }

        // ========================================
        // TIPO DE COMPROBANTE
        // ========================================
        public async Task<List<SatTipoComprobante>> GetTipoComprobanteAsync()
        {
            return await _context.SatTipoComprobante
                .Where(x => x.IsActive)
                .OrderBy(x => x.Codigo)
                .ToListAsync();
        }

        public async Task<SatTipoComprobante?> GetTipoComprobanteByCodigoAsync(string codigo)
        {
            return await _context.SatTipoComprobante.FirstOrDefaultAsync(x => x.Codigo == codigo);
        }

        // ========================================
        // UNIDAD DE MEDIDA
        // ========================================
        public async Task<List<SatUnidadMedida>> GetUnidadMedidaAsync(string? search = null)
        {
            var query = _context.SatUnidadMedida.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.ClaveUnidad.Contains(search) ||
                    x.Nombre.Contains(search) ||
                    (x.Descripcion != null && x.Descripcion.Contains(search)));
            }

            return await query
                .OrderBy(x => x.ClaveUnidad)
                .Take(50)
                .ToListAsync();
        }

        public async Task<SatUnidadMedida?> GetUnidadMedidaByCodigoAsync(string codigo)
        {
            return await _context.SatUnidadMedida.FirstOrDefaultAsync(x => x.ClaveUnidad == codigo);
        }

        // ========================================
        // PRODUCTO/SERVICIO
        // ========================================
        public async Task<List<SatProductoServicio>> GetProductoServicioAsync(string? search = null, int limit = 50)
        {
            var query = _context.SatProductoServicio.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.ClaveProdServ.Contains(search) ||
                    x.Descripcion.Contains(search) ||
                    (x.PalabrasSimilares != null && x.PalabrasSimilares.Contains(search)));
            }

            return await query
                .OrderBy(x => x.ClaveProdServ)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<SatProductoServicio?> GetProductoServicioByCodigoAsync(string codigo)
        {
            return await _context.SatProductoServicio.FirstOrDefaultAsync(x => x.ClaveProdServ == codigo);
        }
    }
}
