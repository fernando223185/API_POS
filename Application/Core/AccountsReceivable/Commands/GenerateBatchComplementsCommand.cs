using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para generar todos los complementos de un batch asíncronamente
/// </summary>
public class GenerateBatchComplementsCommand : IRequest<GenerateComplementsResultDto>
{
    public int BatchId { get; set; }
    public bool SendEmailsAutomatically { get; set; } = false;
    public int ExecutedBy { get; set; }
}
