using Domain.Entities;
using Infrastructure.Persistence;
using Application.Abstractions.Catalogue;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly POSDbContext _dbcontext;

		public UserRepository(POSDbContext dbContext)
		{
			_dbcontext = dbContext;
		}

		public async Task<User> CreateAsync(User user)
		{
			_dbcontext.User.Add(user);
            await _dbcontext.SaveChangesAsync();
			return user;
        }

		public async Task<User?> GetByIdAsync(int userId)
		{
			return await _dbcontext.User
				.Include(u => u.Role)
				.Include(u => u.DefaultWarehouse)
					.ThenInclude(w => w.Branch)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);
		}

		public async Task<List<User>> GetAllAsync(bool includeInactive = false)
		{
			var query = _dbcontext.User
				.Include(u => u.Role)
				.Include(u => u.DefaultWarehouse)
					.ThenInclude(w => w.Branch)
				.AsQueryable();

			if (!includeInactive)
			{
				query = query.Where(u => u.Active);
			}

			return await query
				.OrderBy(u => u.Name)
				.ToListAsync();
		}

		public async Task<User?> UpdateAsync(User user)
		{
			var existing = await _dbcontext.User.FindAsync(user.Id);
			if (existing == null) return null;

			existing.Name = user.Name;
			existing.Email = user.Email;
			existing.Phone = user.Phone;
			existing.RoleId = user.RoleId;
			existing.Active = user.Active;
			existing.DefaultWarehouseId = user.DefaultWarehouseId;
			existing.CanSellFromMultipleWarehouses = user.CanSellFromMultipleWarehouses;
			existing.UpdatedAt = DateTime.UtcNow;

			await _dbcontext.SaveChangesAsync();
			return existing;
		}

		// ✅ NUEVO: Validar existencia de almacén
		public async Task<bool> WarehouseExistsAsync(int warehouseId)
		{
			return await _dbcontext.Warehouses
				.AnyAsync(w => w.Id == warehouseId && w.IsActive);
		}
    }
}

