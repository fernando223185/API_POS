using Domain.Entities;

namespace Application.Abstractions.CashierShifts
{
    public interface ICashierShiftRepository
    {
        /// <summary>
        /// Crear un nuevo turno
        /// </summary>
        Task<CashierShift> CreateAsync(CashierShift shift);

        /// <summary>
        /// Actualizar un turno existente
        /// </summary>
        Task<CashierShift> UpdateAsync(CashierShift shift);

        /// <summary>
        /// Obtener turno por ID con todas las relaciones
        /// </summary>
        Task<CashierShift?> GetByIdAsync(int id);

        /// <summary>
        /// Obtener turno activo de un cajero en una sucursal específica
        /// </summary>
        Task<CashierShift?> GetActiveShiftAsync(int cashierId, int? branchId = null);

        /// <summary>
        /// Verificar si existe un turno activo para un cajero en una sucursal
        /// </summary>
        Task<bool> HasActiveShiftAsync(int cashierId, int? branchId = null);

        /// <summary>
        /// Obtener turnos paginados con filtros
        /// </summary>
        Task<(IEnumerable<CashierShift> Shifts, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            int? cashierId = null,
            int? warehouseId = null,
            int? branchId = null,
            int? companyId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null
        );

        /// <summary>
        /// Obtener el siguiente número de código de turno
        /// </summary>
        Task<string> GetNextCodeAsync();
    }
}
