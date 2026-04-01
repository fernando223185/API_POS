using Application.Abstractions.CashierShifts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para manejo de turnos de cajero
    /// </summary>
    public class CashierShiftRepository : ICashierShiftRepository
    {
        private readonly POSDbContext _context;

        public CashierShiftRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<CashierShift> CreateAsync(CashierShift shift)
        {
            _context.CashierShifts.Add(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<CashierShift> UpdateAsync(CashierShift shift)
        {
            _context.CashierShifts.Update(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<CashierShift?> GetByIdAsync(int id)
        {
            return await _context.CashierShifts
                .Include(cs => cs.Cashier)
                .Include(cs => cs.Warehouse)
                .Include(cs => cs.Branch)
                .Include(cs => cs.Company)
                .Include(cs => cs.ClosedBy)
                .Include(cs => cs.CancelledBy)
                .FirstOrDefaultAsync(cs => cs.Id == id);
        }

        public async Task<CashierShift?> GetActiveShiftAsync(int cashierId, int? branchId = null)
        {
            var query = _context.CashierShifts
                .Include(cs => cs.Cashier)
                .Include(cs => cs.Warehouse)
                .Include(cs => cs.Branch)
                .Include(cs => cs.Company)
                .Where(cs => cs.CashierId == cashierId && cs.Status == "Open");

            if (branchId.HasValue)
            {
                query = query.Where(cs => cs.BranchId == branchId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> HasActiveShiftAsync(int cashierId, int? branchId = null)
        {
            var query = _context.CashierShifts
                .Where(cs => cs.CashierId == cashierId && cs.Status == "Open");

            if (branchId.HasValue)
            {
                query = query.Where(cs => cs.BranchId == branchId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<(IEnumerable<CashierShift> Shifts, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? cashierId = null,
            int? warehouseId = null,
            int? branchId = null,
            int? companyId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.CashierShifts
                .Include(cs => cs.Cashier)
                .Include(cs => cs.Warehouse)
                .Include(cs => cs.Branch)
                .Include(cs => cs.Company)
                .Include(cs => cs.ClosedBy)
                .Include(cs => cs.CancelledBy)
                .AsQueryable();

            // Aplicar filtros
            if (cashierId.HasValue)
            {
                query = query.Where(cs => cs.CashierId == cashierId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(cs => cs.WarehouseId == warehouseId.Value);
            }

            if (branchId.HasValue)
            {
                query = query.Where(cs => cs.BranchId == branchId.Value);
            }

            if (companyId.HasValue)
            {
                query = query.Where(cs => cs.CompanyId == companyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(cs => cs.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(cs => cs.OpeningDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(cs => cs.OpeningDate <= toDate.Value);
            }

            // Contar total de registros
            var totalCount = await query.CountAsync();

            // Aplicar paginación y ordenamiento (más recientes primero)
            var shifts = await query
                .OrderByDescending(cs => cs.OpeningDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (shifts, totalCount);
        }

        public async Task<string> GetNextCodeAsync()
        {
            // Obtener el último código
            var lastCode = await _context.CashierShifts
                .OrderByDescending(cs => cs.Id)
                .Select(cs => cs.Code)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastCode))
            {
                // Primer turno
                return "TURNO-000001";
            }

            // Extraer el número del código (formato: TURNO-000001)
            var parts = lastCode.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[1], out var lastNumber))
            {
                // Si el formato no es válido, empezar desde 1
                return "TURNO-000001";
            }

            // Incrementar y formatear
            var newNumber = lastNumber + 1;
            return $"TURNO-{newNumber:D6}";
        }
    }
}
