using System;
using Domain.Entities;

namespace Application.Abstractions.Catalogue
{
	public interface IUserRepository
	{
        Task<User> CreateAsync(User user);
		Task<User?> GetByIdAsync(int userId);
		Task<List<User>> GetAllAsync(bool includeInactive = false);
		Task<User?> UpdateAsync(User user);
		
		// ✅ NUEVO: Validación de almacén
		Task<bool> WarehouseExistsAsync(int warehouseId);
    }
}

