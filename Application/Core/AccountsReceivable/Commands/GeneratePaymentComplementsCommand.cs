using Application.DTOs.AccountsReceivable;
using MediatR;

namespace Application.Core.AccountsReceivable.Commands;

/// <summary>
/// Command para generar complementos de pago SAT para un pago específico
/// Genera un complemento por cada aplicación de pago (una por factura)
/// </summary>
public class GeneratePaymentComplementsCommand : IRequest<GenerateComplementsResultDto>
{
    public int PaymentId { get; set; }
    public bool SendEmailsAutomatically { get; set; } = false;
    public int ExecutedBy { get; set; }
}

/// <summary>
/// Resultado de la generación de complementos
/// </summary>
public class GenerateComplementsResultDto
{
    public int PaymentId { get; set; }
    public int TotalApplications { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ComplementGenerationResult> Results { get; set; } = new();
}

public class ComplementGenerationResult
{
    public int PaymentApplicationId { get; set; }
    public string InvoiceSerieAndFolio { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ComplementUUID { get; set; }
    public string? Error { get; set; }
}
