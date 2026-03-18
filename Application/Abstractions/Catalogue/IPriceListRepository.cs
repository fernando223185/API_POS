namespace Application.Abstractions.Catalogue
{
    public interface IPriceListRepository
    {
        Task<List<Domain.Entities.PriceList>> GetAllAsync(bool? isActive = null);
        Task<Domain.Entities.PriceList?> GetByIdAsync(int id);
        Task<Domain.Entities.PriceList> CreateAsync(Domain.Entities.PriceList priceList);
        Task<Domain.Entities.PriceList> UpdateAsync(Domain.Entities.PriceList priceList);
        Task<bool> DeleteAsync(int id); // Eliminación lógica
    }
}
