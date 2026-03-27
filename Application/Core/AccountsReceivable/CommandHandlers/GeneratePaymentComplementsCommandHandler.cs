using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Abstractions.Config;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using Domain.Entities;
using MediatR;
using System.Globalization;
using CompanyEntity = Domain.Entities.Company;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para generar y timbrar complementos de pago CFDI 4.0 (tipo "P")
/// Se ejecuta cuando el usuario solicita timbrar los complementos de un pago aplicado
/// </summary>
public class GeneratePaymentComplementsCommandHandler
    : IRequestHandler<GeneratePaymentComplementsCommand, GenerateComplementsResultDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentApplicationRepository _paymentApplicationRepository;
    private readonly IInvoicePPDRepository _invoicePPDRepository;
    private readonly IPaymentComplementLogRepository _logRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ISapiensService _sapiensService;

    public GeneratePaymentComplementsCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentApplicationRepository paymentApplicationRepository,
        IInvoicePPDRepository invoicePPDRepository,
        IPaymentComplementLogRepository logRepository,
        ICompanyRepository companyRepository,
        ISapiensService sapiensService)
    {
        _paymentRepository = paymentRepository;
        _paymentApplicationRepository = paymentApplicationRepository;
        _invoicePPDRepository = invoicePPDRepository;
        _logRepository = logRepository;
        _companyRepository = companyRepository;
        _sapiensService = sapiensService;
    }

    public async Task<GenerateComplementsResultDto> Handle(
        GeneratePaymentComplementsCommand command,
        CancellationToken cancellationToken)
    {
        var ic = CultureInfo.InvariantCulture;

        // 1. Cargar pago
        var payment = await _paymentRepository.GetByIdAsync(command.PaymentId)
            ?? throw new InvalidOperationException($"Pago {command.PaymentId} no encontrado.");

        // 2. Cargar empresa emisora
        var company = await _companyRepository.GetByIdAsync(payment.CompanyId)
            ?? throw new InvalidOperationException($"Empresa {payment.CompanyId} no encontrada.");

        // 3. Cargar aplicaciones de pago pendientes de timbrar
        var applications = await _paymentApplicationRepository.GetByPaymentIdAsync(command.PaymentId);
        var pending = applications
            .Where(a => a.ComplementStatus != "Generated" && a.ComplementStatus != "Cancelled")
            .ToList();

        var result = new GenerateComplementsResultDto
        {
            TotalProcessed = pending.Count,
            SuccessCount = 0,
            ErrorCount = 0
        };

        foreach (var application in pending)
        {
            var startTime = DateTime.UtcNow;

            // Marcar en proceso
            application.ComplementStatus = "Generating";
            application.RetryCount++;
            application.LastRetryAt = DateTime.UtcNow;
            await _paymentApplicationRepository.UpdateAsync(application);

            try
            {
                // 4. Cargar factura PPD relacionada
                var invoicePPD = await _invoicePPDRepository.GetByIdAsync(application.InvoicePPDId)
                    ?? throw new InvalidOperationException(
                        $"Factura PPD {application.InvoicePPDId} no encontrada para la aplicación {application.Id}.");

                // 5. Construir CFDI tipo "P"
                var cfdiData = BuildComplementeCFDI(payment, application, invoicePPD, company, ic);

                // 6. Timbrar con Sapiens
                var timbradoResponse = await _sapiensService.TimbrarFacturaAsync(cfdiData, "v4");

                if (timbradoResponse?.data == null || string.IsNullOrEmpty(timbradoResponse.data.uuid))
                    throw new InvalidOperationException(
                        $"Sapiens no devolvió UUID para la aplicación {application.Id}. Estado: {timbradoResponse?.status}");

                var elapsedMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                // 7. Actualizar aplicación → Generated
                application.ComplementUUID = timbradoResponse.data.uuid;
                application.ComplementStatus = "Generated";
                application.GeneratedAt = DateTime.UtcNow;
                application.XmlContent = timbradoResponse.data.cfdi;
                application.ComplementError = null;

                if (DateTime.TryParse(timbradoResponse.data.fechaTimbrado, out var fechaTimbrado))
                    application.SATCertificationDate = fechaTimbrado;

                application.SATSerialNumber = timbradoResponse.data.noCertificadoSAT;

                await _paymentApplicationRepository.UpdateAsync(application);

                // 8. Registrar log de éxito
                await _logRepository.CreateAsync(new PaymentComplementLog
                {
                    PaymentApplicationId = application.Id,
                    PaymentId = command.PaymentId,
                    AttemptNumber = application.RetryCount,
                    AttemptDate = startTime,
                    Action = "Stamp",
                    Status = "Success",
                    ExecutionTimeMs = elapsedMs,
                    PACResponse = timbradoResponse.data.cfdi,
                    PACTransactionId = timbradoResponse.data.uuid,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = command.ExecutedBy
                });

                // 9. Actualizar saldo de la factura PPD
                invoicePPD.PaidAmount += application.AmountApplied;
                invoicePPD.BalanceAmount -= application.AmountApplied;
                invoicePPD.TotalPartialities++;
                invoicePPD.NextPartialityNumber++;
                invoicePPD.LastPaymentDate = payment.PaymentDate;
                invoicePPD.Status = invoicePPD.BalanceAmount <= 0 ? "Paid" : "PartiallyPaid";
                invoicePPD.UpdatedAt = DateTime.UtcNow;
                await _invoicePPDRepository.UpdateAsync(invoicePPD);

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                var elapsedMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                application.ComplementStatus = "Error";
                application.ComplementError = ex.Message;
                await _paymentApplicationRepository.UpdateAsync(application);

                await _logRepository.CreateAsync(new PaymentComplementLog
                {
                    PaymentApplicationId = application.Id,
                    PaymentId = command.PaymentId,
                    AttemptNumber = application.RetryCount,
                    AttemptDate = startTime,
                    Action = "Stamp",
                    Status = "Failed",
                    ExecutionTimeMs = elapsedMs,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = command.ExecutedBy
                });

                result.ErrorCount++;
                result.Errors.Add($"Aplicación {application.Id}: {ex.Message}");
            }
        }

        // 10. Actualizar contadores del pago
        payment.ComplementsGenerated += result.SuccessCount;
        payment.ComplementsWithError += result.ErrorCount;
        if (result.SuccessCount > 0 && result.ErrorCount == 0)
            payment.Status = "Complemented";
        await _paymentRepository.UpdateAsync(payment);

        return result;
    }

    // ─────────────────────────────────────────────────────────────
    //  Construcción del CFDI 4.0 tipo "P" (Complemento de Pago)
    // ─────────────────────────────────────────────────────────────
    private static object BuildComplementeCFDI(
        Payment payment,
        PaymentApplication application,
        InvoicePPD invoicePPD,
        CompanyEntity company,
        IFormatProvider ic)
    {
        // Importes
        decimal baseIVA16    = Math.Round(application.AmountApplied / 1.16m, 2);
        decimal importeIVA16 = Math.Round(application.AmountApplied - baseIVA16, 2);

        decimal saldoAnterior  = application.PreviousBalance;
        decimal impPagado      = application.AmountApplied;
        decimal saldoInsoluto  = Math.Max(0, Math.Round(saldoAnterior - impPagado, 2));

        // Fecha del pago en formato SAT
        string fechaPago = payment.PaymentDate.ToString("yyyy-MM-ddTHH:mm:ss", ic);

        return new
        {
            Version             = "4.0",
            TipoDeComprobante   = "P",
            Fecha               = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss", ic),
            Sello               = "",
            NoCertificado       = "",
            Certificado         = "",
            SubTotal            = "0",
            Moneda              = "XXX",
            Total               = "0",
            Exportacion         = "01",
            LugarExpedicion     = company.FiscalZipCode,

            Emisor = new
            {
                Rfc            = company.TaxId,
                Nombre         = company.LegalName,
                RegimenFiscal  = company.SatTaxRegime
            },

            Receptor = new
            {
                Rfc                  = invoicePPD.CustomerRFC,
                Nombre               = invoicePPD.CustomerName,
                DomicilioFiscalReceptor = "00000",  // Se sobreescribe por Sapiens desde el PAC
                RegimenFiscalReceptor = "616",       // Valor genérico para complementos de pago
                UsoCFDI              = "CP01"        // CP01 = Pagos
            },

            Conceptos = new[]
            {
                new
                {
                    ClaveProdServ  = "84111506",
                    ClaveUnidad    = "ACT",
                    Cantidad       = "1",
                    Descripcion    = "Pago",
                    ValorUnitario  = "0",
                    Importe        = "0",
                    ObjetoImp      = "01"
                }
            },

            Complemento = new
            {
                Any = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["Pago20:Pagos"] = new
                        {
                            Version = "2.0",

                            Totales = new
                            {
                                TotalTrasladosBaseIVA16      = baseIVA16.ToString("0.##", ic),
                                TotalTrasladosImpuestoIVA16  = importeIVA16.ToString("0.##", ic),
                                MontoTotalPagos              = impPagado.ToString("0.##", ic)
                            },

                            Pago = new[]
                            {
                                new
                                {
                                    FechaPago      = fechaPago,
                                    FormaDePagoP   = payment.PaymentFormSAT,
                                    MonedaP        = payment.Currency,
                                    Monto          = impPagado.ToString("0.##", ic),

                                    DoctoRelacionado = new[]
                                    {
                                        new
                                        {
                                            IdDocumento     = invoicePPD.FolioUUID,
                                            Serie           = invoicePPD.Serie ?? "",
                                            Folio           = invoicePPD.Folio ?? "",
                                            MonedaDR        = invoicePPD.Currency,
                                            EquivalenciaDR  = "1",
                                            NumParcialidad  = application.PartialityNumber.ToString(),
                                            ImpSaldoAnt     = saldoAnterior.ToString("0.##", ic),
                                            ImpPagado       = impPagado.ToString("0.##", ic),
                                            ImpSaldoInsoluto = saldoInsoluto.ToString("0.##", ic),
                                            ObjetoImpDR     = "02",
                                            ImpuestosDR = new
                                            {
                                                TrasladosDR = new[]
                                                {
                                                    new
                                                    {
                                                        BaseDR         = baseIVA16.ToString("0.######", ic),
                                                        ImpuestoDR     = "002",
                                                        TipoFactorDR   = "Tasa",
                                                        TasaOCuotaDR   = "0.160000",
                                                        ImporteDR      = importeIVA16.ToString("0.######", ic)
                                                    }
                                                }
                                            }
                                        }
                                    },

                                    ImpuestosP = new
                                    {
                                        TrasladosP = new[]
                                        {
                                            new
                                            {
                                                BaseP        = baseIVA16.ToString("0.######", ic),
                                                ImpuestoP    = "002",
                                                TipoFactorP  = "Tasa",
                                                TasaOCuotaP  = "0.160000",
                                                ImporteP     = importeIVA16.ToString("0.######", ic)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
