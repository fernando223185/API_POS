using Application.Core.CashierShifts.Documents;
using Application.Core.CashierShifts.Queries;
using MediatR;

namespace Application.Core.CashierShifts.QueryHandlers
{
    /// <summary>
    /// Handler para generar el PDF del corte de caja usando el layout legacy por defecto.
    /// </summary>
    public class GenerateShiftPdfQueryHandler : IRequestHandler<GenerateShiftPdfQuery, byte[]>
    {
        private readonly IMediator _mediator;

        public GenerateShiftPdfQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<byte[]> Handle(GenerateShiftPdfQuery query, CancellationToken cancellationToken)
        {
            var report = await _mediator.Send(new GetShiftReportQuery(query.ShiftId), cancellationToken);
            return CashierShiftPdfDocument.Generate(report);
        }
    }
}
