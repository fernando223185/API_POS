using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Reports;
using Application.Core.AccountsReceivable.Queries;
using Application.Core.Billing.Documents;
using Application.Core.Reports.Engine;
using MediatR;

namespace Application.Core.AccountsReceivable.QueryHandlers;

// ============================================================
// XML
// ============================================================
/// <summary>
/// Handler para descargar el XML timbrado de un complemento de pago
/// </summary>
public class GetPaymentXmlQueryHandler : IRequestHandler<GetPaymentXmlQuery, (byte[] Bytes, string FileName)>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentXmlQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<(byte[] Bytes, string FileName)> Handle(
        GetPaymentXmlQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);

        if (payment == null)
            throw new KeyNotFoundException($"Pago con ID {request.PaymentId} no encontrado.");

        if (payment.Status != "Complemented" || string.IsNullOrEmpty(payment.XmlCfdi))
            throw new InvalidOperationException("El pago no tiene XML timbrado disponible.");

        var bytes = System.Text.Encoding.UTF8.GetBytes(payment.XmlCfdi);
        var fileName = $"CP-{payment.PaymentNumber}_{payment.Uuid}.xml";
        return (bytes, fileName);
    }
}

// ============================================================
// PDF
// ============================================================
/// <summary>
/// Handler para generar el PDF de un complemento de pago timbrado
/// </summary>
public class GetPaymentPdfQueryHandler : IRequestHandler<GetPaymentPdfQuery, (byte[] Bytes, string FileName)>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReportTemplateRepository _templateRepository;
    private readonly ITemplateRenderService _templateRender;
    private readonly IPdfRenderService _pdfRender;

    public GetPaymentPdfQueryHandler(
        IPaymentRepository paymentRepository,
        IReportTemplateRepository templateRepository,
        ITemplateRenderService templateRender,
        IPdfRenderService pdfRender)
    {
        _paymentRepository = paymentRepository;
        _templateRepository = templateRepository;
        _templateRender = templateRender;
        _pdfRender = pdfRender;
    }

    public async Task<(byte[] Bytes, string FileName)> Handle(
        GetPaymentPdfQuery request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId)
            ?? throw new KeyNotFoundException($"Pago con ID {request.PaymentId} no encontrado.");

        if (payment.Status != "Complemented")
            throw new InvalidOperationException("Solo se puede generar PDF de complementos de pago timbrados.");

        var template = await _templateRepository.GetDefaultByTypeAsync("Payment", payment.CompanyId)
            ?? await _templateRepository.GetDefaultByTypeAsync("Payment", null);

        byte[] bytes;
        if (template?.HtmlTemplate != null)
        {
            var header = ReportDataProvider.FromPayment(payment);
            var items  = ReportDataProvider.FromPaymentApplications(payment);
            var html   = _templateRender.Render(template.HtmlTemplate, header, items);
            bytes = await _pdfRender.RenderHtmlToPdfAsync(html);
        }
        else
        {
            // Fallback al motor legacy mientras no exista plantilla activa
            bytes = PaymentPdfDocument.Generate(payment);
        }

        var fileName = $"CP-{payment.PaymentNumber}_{payment.Uuid}.pdf";
        return (bytes, fileName);
    }
}
