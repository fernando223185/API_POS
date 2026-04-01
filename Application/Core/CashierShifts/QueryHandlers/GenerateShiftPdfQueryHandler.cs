using Application.Core.CashierShifts.Documents;
using Application.Core.CashierShifts.Queries;
using MediatR;

namespace Application.Core.CashierShifts.QueryHandlers
{
    /// <summary>
    /// Handler para generar el PDF del corte de caja
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
            // 1. Obtener el reporte completo del turno
            var report = await _mediator.Send(new GetShiftReportQuery(query.ShiftId), cancellationToken);

            // 2. Generar el PDF usando el documento
            var pdfBytes = CashierShiftPdfDocument.Generate(report);

            return pdfBytes;
        }
    }
}
