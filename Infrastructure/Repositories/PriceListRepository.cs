using Application.Abstractions.Catalogue;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class PriceListRepository : IPriceListRepository
    {
        private readonly POSDbContext _context;
        private readonly ILogger<PriceListRepository> _logger;

        public PriceListRepository(POSDbContext context, ILogger<PriceListRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PriceList>> GetAllAsync(bool? isActive = null)
        {
            try
            {
                var query = _context.PriceLists.AsQueryable();

                if (isActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == isActive.Value);
                }

                var priceLists = await query
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {priceLists.Count} price lists");
                return priceLists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving price lists");
                throw;
            }
        }

        public async Task<PriceList?> GetByIdAsync(int id)
        {
            try
            {
                var priceList = await _context.PriceLists
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (priceList == null)
                {
                    _logger.LogWarning($"PriceList with ID {id} not found");
                }

                return priceList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving price list with ID {id}");
                throw;
            }
        }

        public async Task<PriceList> CreateAsync(PriceList priceList)
        {
            try
            {
                priceList.CreatedAt = DateTime.UtcNow;
                priceList.IsActive = true;

                _context.PriceLists.Add(priceList);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"PriceList created: {priceList.Name} (ID: {priceList.Id})");
                return priceList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating price list");
                throw;
            }
        }

        public async Task<PriceList> UpdateAsync(PriceList priceList)
        {
            try
            {
                _context.PriceLists.Update(priceList);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"PriceList updated: {priceList.Name} (ID: {priceList.Id})");
                return priceList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating price list with ID {priceList.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var priceList = await _context.PriceLists.FindAsync(id);
                
                if (priceList == null)
                {
                    _logger.LogWarning($"PriceList with ID {id} not found for deletion");
                    return false;
                }

                // Eliminación lógica: marcar como inactivo
                priceList.IsActive = false;
                _context.PriceLists.Update(priceList);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"PriceList soft deleted: {priceList.Name} (ID: {id})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting price list with ID {id}");
                throw;
            }
        }
    }
}
