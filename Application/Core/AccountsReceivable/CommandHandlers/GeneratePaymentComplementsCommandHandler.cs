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
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentComplementLogRepository _logRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ISapiensService _sapiensService;

    public GeneratePaymentComplementsCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentApplicationRepository paymentApplicationRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentComplementLogRepository logRepository,
        ICompanyRepository companyRepository,
        ISapiensService sapiensService)
    {
        _paymentRepository = paymentRepository;
        _paymentApplicationRepository = paymentApplicationRepository;
        _invoiceRepository = invoiceRepository;
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

        // 3. Verificar que el pago no esté ya timbrado
        if (!string.IsNullOrEmpty(payment.Uuid))
        {
            throw new InvalidOperationException($"El pago {payment.PaymentNumber} ya tiene un complemento timbrado (UUID: {payment.Uuid}).");
        }

        // 4. Cargar TODAS las aplicaciones del pago
        var applications = await _paymentApplicationRepository.GetByPaymentIdAsync(command.PaymentId);
        
        if (!applications.Any())
        {
            throw new InvalidOperationException($"El pago {payment.PaymentNumber} no tiene facturas aplicadas.");
        }

        var result = new GenerateComplementsResultDto
        {
            TotalProcessed = applications.Count,
            SuccessCount = 0,
            ErrorCount = 0
        };

        var startTime = DateTime.UtcNow;

        try
        {
            // 5. Cargar todas las facturas relacionadas
            var invoiceIds = applications.Select(a => a.InvoiceId).Distinct().ToList();
            var invoices = new List<Invoice>();
            foreach (var id in invoiceIds)
            {
                var inv = await _invoiceRepository.GetByIdAsync(id);
                if (inv != null) invoices.Add(inv);
            }

            if (invoices.Count != invoiceIds.Count)
            {
                throw new InvalidOperationException("No se encontraron todas las facturas relacionadas al pago.");
            }

            // 6. Construir CFDI tipo "P" con TODAS las aplicaciones como DoctoRelacionado
            var cfdiData = BuildComplementoCFDI(payment, applications.ToList(), invoices, company, ic);

            // 7. Timbrar con Sapiens
            var timbradoResponse = await _sapiensService.TimbrarFacturaAsync(cfdiData, "v4");

            if (timbradoResponse?.data == null || string.IsNullOrEmpty(timbradoResponse.data.uuid))
                throw new InvalidOperationException($"Sapiens no devolvió UUID. Estado: {timbradoResponse?.status}");

            var elapsedMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // 8. Actualizar PAYMENT con los datos del complemento timbrado
            payment.Uuid = timbradoResponse.data.uuid;
            payment.XmlCfdi = timbradoResponse.data.cfdi;
            payment.ComplementError = null;
            payment.Status = "Complemented";
            payment.ComplementsGenerated = 1;

            if (DateTime.TryParse(timbradoResponse.data.fechaTimbrado, out var fechaTimbrado))
                payment.TimbradoAt = fechaTimbrado;
            else
                payment.TimbradoAt = DateTime.UtcNow;

            payment.NoCertificadoSat = timbradoResponse.data.noCertificadoSAT;
            payment.SelloCfdi = timbradoResponse.data.selloCFDI;
            payment.SelloSat = timbradoResponse.data.selloSAT;
            payment.NoCertificadoCfdi = timbradoResponse.data.noCertificadoCFDI;
            payment.CadenaOriginalSat = timbradoResponse.data.cadenaOriginalSAT;
            payment.QrCode = timbradoResponse.data.qrCode;

            await _paymentRepository.UpdateAsync(payment);

            // 9. Actualizar saldos de TODAS las facturas
            foreach (var app in applications)
            {
                var invoice = invoices.FirstOrDefault(i => i.Id == app.InvoiceId);
                if (invoice == null) continue;

                invoice.PaidAmount += app.AmountApplied;
                invoice.BalanceAmount = (invoice.BalanceAmount ?? invoice.Total) - app.AmountApplied;
                invoice.TotalPartialities += 1;
                invoice.NextPartialityNumber = (invoice.NextPartialityNumber ?? 1) + 1;
                invoice.LastPaymentDate = payment.PaymentDate;
                invoice.PaymentStatus = invoice.BalanceAmount <= 0 ? "Paid" : "PartiallyPaid";
                invoice.UpdatedAt = DateTime.UtcNow;
                await _invoiceRepository.UpdateAsync(invoice);
            }

            // 10. Log de éxito
            await _logRepository.CreateAsync(new PaymentComplementLog
            {
                PaymentId = command.PaymentId,
                AttemptNumber = payment.RetryCount + 1,
                AttemptDate = startTime,
                Action = "Stamp",
                Status = "Success",
                ExecutionTimeMs = elapsedMs,
                PACResponse = timbradoResponse.data.cfdi,
                PACTransactionId = timbradoResponse.data.uuid,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.ExecutedBy
            });

            result.SuccessCount = 1;
            result.TotalProcessed = 1;
        }
        catch (Exception ex)
        {
            var elapsedMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            payment.ComplementError = ex.Message;
            payment.ComplementsWithError = 1;
            payment.RetryCount++;
            payment.LastRetryAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            await _logRepository.CreateAsync(new PaymentComplementLog
            {
                PaymentId = command.PaymentId,
                AttemptNumber = payment.RetryCount,
                AttemptDate = startTime,
                Action = "Stamp",
                Status = "Failed",
                ExecutionTimeMs = elapsedMs,
                ErrorMessage = ex.Message,
                ErrorStackTrace = ex.StackTrace,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = command.ExecutedBy
            });

            result.ErrorCount = 1;
            result.Errors.Add($"Pago {payment.PaymentNumber}: {ex.Message}");
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────
    //  Hora local de México (zona fiscal) para el campo Fecha del CFDI
    // ─────────────────────────────────────────────────────────────
    private static string ObtenerFechaCfdi()
    {
        TimeZoneInfo tz;
        try { tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"); }
        catch { tz = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"); }
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz)
            .ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }

    // ─────────────────────────────────────────────────────────────
    //  Construcción del CFDI 4.0 tipo "P" (Complemento de Pago)
    //  Genera UN complemento con TODOS los DoctoRelacionado
    // ─────────────────────────────────────────────────────────────
    private static object BuildComplementoCFDI(
        Payment payment,
        List<PaymentApplication> applications,
        List<Invoice> invoices,
        CompanyEntity company,
        IFormatProvider ic)
    {
        // Acumuladores a 6 decimales: se usan en BaseP/ImporteP (ImpuestosP)
        // Los Totales (Pago20:Totales) se redondean a 2 decimales al final
        decimal totalBaseIVA16_6 = 0;
        decimal totalImpuestoIVA16_6 = 0;
        decimal montoTotalPagos = applications.Sum(a => a.AmountApplied);

        // Construir array de DoctoRelacionado
        var documentosRelacionados = new List<DoctoRelacionadoDto>();

        foreach (var app in applications)
        {
            var invoice = invoices.FirstOrDefault(i => i.Id == app.InvoiceId);
            if (invoice == null) continue;

            // Cálculo a 6 decimales: BaseDR × TasaOCuotaDR = ImporteDR exacto (regla SAT CRP20261)
            decimal baseIVA16 = Math.Round(app.AmountApplied / 1.16m, 6);
            decimal importeIVA16 = Math.Round(baseIVA16 * 0.16m, 6);

            // Acumular a 6 decimales para que BaseP/ImporteP = suma de BaseDR/ImporteDR
            totalBaseIVA16_6 += baseIVA16;
            totalImpuestoIVA16_6 += importeIVA16;

            decimal saldoAnterior = app.PreviousBalance;
            decimal impPagado = app.AmountApplied;
            decimal saldoInsoluto = Math.Max(0, Math.Round(saldoAnterior - impPagado, 2));

            documentosRelacionados.Add(new DoctoRelacionadoDto
            {
                IdDocumento = invoice.Uuid,
                Serie = invoice.Serie ?? "",
                Folio = invoice.Folio ?? "",
                MonedaDR = invoice.Moneda,
                EquivalenciaDR = "1",
                NumParcialidad = app.PartialityNumber.ToString(),
                ImpSaldoAnt = saldoAnterior.ToString("0.00", ic),
                ImpPagado = impPagado.ToString("0.00", ic),
                ImpSaldoInsoluto = saldoInsoluto.ToString("0.00", ic),
                ObjetoImpDR = "02",
                ImpuestosDR = new ImpuestosDRDto
                {
                    TrasladosDR = new[]
                    {
                        new TrasladoDRDto
                        {
                            BaseDR = baseIVA16.ToString("0.000000", ic),
                            ImpuestoDR = "002",
                            TipoFactorDR = "Tasa",
                            TasaOCuotaDR = "0.160000",
                            ImporteDR = importeIVA16.ToString("0.000000", ic)
                        }
                    }
                }
            });
        }

        // Obtener datos del receptor desde la primera factura (todas deberían tener el mismo receptor)
        var primeraFactura = invoices.First();
        string fechaPago = payment.PaymentDate.ToString("yyyy-MM-ddTHH:mm:ss", ic);
        string folio = payment.PaymentNumber?.Replace("PAG-", "").Replace("-", "") ?? DateTime.Now.ToString("yyyyMMddHHmmss");

        // Construir el complemento con [JsonProperty] para forzar "Pago20:Pagos"
        var complemento = new ComplementoDto
        {
            Pago20Pagos = new Pago20PagosDto
            {
                Version = "2.0",
                Totales = new TotalesDto
                {
                    TotalTrasladosBaseIVA16 = Math.Round(totalBaseIVA16_6, 2).ToString("0.00", ic),
                    TotalTrasladosImpuestoIVA16 = Math.Round(totalImpuestoIVA16_6, 2).ToString("0.00", ic),
                    MontoTotalPagos = montoTotalPagos.ToString("0.00", ic)
                },
                Pago = new[]
                {
                    new PagoDto
                    {
                        FechaPago = fechaPago,
                        FormaDePagoP = payment.PaymentFormSAT,
                        MonedaP = payment.Currency,
                        TipoCambioP = "1",
                        Monto = montoTotalPagos.ToString("0.00", ic),
                        DoctoRelacionado = documentosRelacionados.ToArray(),
                        ImpuestosP = new ImpuestosPDto
                        {
                            TrasladosP = new[]
                            {
                                new TrasladoPDto
                                {
                                    BaseP = totalBaseIVA16_6.ToString("0.000000", ic),
                                    ImpuestoP = "002",
                                    TipoFactorP = "Tasa",
                                    TasaOCuotaP = "0.160000",
                                    ImporteP = totalImpuestoIVA16_6.ToString("0.000000", ic)
                                }
                            }
                        }
                    }
                }
            }
        };

        return new
        {
            Version = "4.0",
            Serie = "CP",
            Folio = folio,
            TipoDeComprobante = "P",
            Fecha = ObtenerFechaCfdi(),
            Sello = "",
            NoCertificado = "",
            Certificado = "",
            SubTotal = "0",
            Moneda = "XXX",
            Total = "0",
            Exportacion = "01",
            LugarExpedicion = company.FiscalZipCode,

            Emisor = new
            {
                Rfc = company.TaxId,
                Nombre = company.LegalName,
                RegimenFiscal = company.SatTaxRegime
            },

            Receptor = new
            {
                Rfc = primeraFactura.ReceptorRfc,
                Nombre = primeraFactura.ReceptorNombre,
                DomicilioFiscalReceptor = primeraFactura.ReceptorDomicilioFiscal ?? "00000",
                RegimenFiscalReceptor = primeraFactura.ReceptorRegimenFiscal ?? "616",
                UsoCFDI = "CP01"
            },

            Conceptos = new[]
            {
                new
                {
                    ClaveProdServ = "84111506",
                    ClaveUnidad = "ACT",
                    Cantidad = 1,
                    Descripcion = "Pago",
                    ValorUnitario = "0",
                    Importe = "0",
                    ObjetoImp = "01"
                }
            },

            Complemento = new
            {
                Any = new[] { complemento }
            }
        };
    }
}
