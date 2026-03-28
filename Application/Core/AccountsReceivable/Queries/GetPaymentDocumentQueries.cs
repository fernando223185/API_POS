using MediatR;

namespace Application.Core.AccountsReceivable.Queries;

/// <summary>
/// Query para descargar el XML timbrado de un complemento de pago
/// </summary>
public class GetPaymentXmlQuery : IRequest<(byte[] Bytes, string FileName)>
{
    public int PaymentId { get; set; }
}

/// <summary>
/// Query para generar el PDF de un complemento de pago timbrado
/// </summary>
public class GetPaymentPdfQuery : IRequest<(byte[] Bytes, string FileName)>
{
    public int PaymentId { get; set; }
}
