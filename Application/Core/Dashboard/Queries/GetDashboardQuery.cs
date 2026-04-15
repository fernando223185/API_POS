using Application.DTOs.Dashboard;
using MediatR;

namespace Application.Core.Dashboard.Queries
{
    /// <summary>
    /// Query para obtener el dashboard adaptado al rol/permisos del usuario autenticado
    /// </summary>
    public class GetDashboardQuery : IRequest<DashboardResponseDto>
    {
        /// <summary>ID del usuario autenticado (obtenido del JWT)</summary>
        public int UserId { get; set; }

        /// <summary>Filtrar datos por empresa (opcional)</summary>
        public int? CompanyId { get; set; }
    }
}
