using Domain.Entities;

namespace Application.Abstractions.Shipping
{
    public interface IShippingCarrierRepository
    {
        Task<ShippingCarrier?> GetByIdAsync(int id);
        Task<ShippingCarrier?> GetByCodeAsync(string code);
        Task<List<ShippingCarrier>> GetAllAsync(int? companyId, bool includeInactive = false);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<ShippingCarrier> CreateAsync(ShippingCarrier carrier);
        Task<ShippingCarrier> UpdateAsync(ShippingCarrier carrier);
        Task<bool> DeleteAsync(int id);
    }
}
