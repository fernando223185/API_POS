using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using MediatR;
using System.Globalization;
using System.Text.Json;

namespace Application.Core.AccountsReceivable.CommandHandlers;

/// <summary>
/// Handler para generar complementos de pago de un lote completo
/// </summary>
public class GenerateBatchComplementsCommandHandler : IRequestHandler<GenerateBatchComplementsCommand, GenerateComplementsResultDto>
{
    private readonly IPaymentBatchRepository _batchRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentComplementLogRepository _logRepository;
    private readonly ISapiensService _sapiensService;

    public GenerateBatchComplementsCommandHandler(
        IPaymentBatchRepository batchRepository,
        IPaymentRepository paymentRepository,
        IPaymentComplementLogRepository logRepository,
        ISapiensService sapiensService)
    {
        _batchRepository = batchRepository;
        _paymentRepository = paymentRepository;
        _logRepository = logRepository;
        _sapiensService = sapiensService;
    }

    public async Task<GenerateComplementsResultDto> Handle(GenerateBatchComplementsCommand command, CancellationToken cancellationToken)
    {
        // 1. Obtener lote con pagos y sus aplicaciones
        var batch = await _batchRepository.GetByIdAsync(command.BatchId)
            ?? throw new InvalidOperationException($"Lote {command.BatchId} no encontrado");

        if (batch.Status == "Completed")
            throw new InvalidOperationException("Este lote ya fue procesado completamente");

        // 2. Actualizar estado del lote
        batch.Status = "Processing";
        await _batchRepository.UpdateAsync(batch);

        // 3. Procesar cada pago del lote
        int totalProcessed = 0;
        int totalSuccess = 0;
        int totalErrors = 0;
        var errors = new List<string>();

        foreach (var payment in batch.Payments)
        {
            try
            {
                // Verificar que el pago tenga todos los datos necesarios
                if (string.IsNullOrEmpty(payment.EmisorRfc) || string.IsNullOrEmpty(payment.ReceptorRfc))
                {
                    throw new InvalidOperationException("El pago no tiene datos completos de emisor/receptor");
                }

                if (payment.PaymentApplications == null || !payment.PaymentApplications.Any())
                {
                    throw new InvalidOperationException("El pago no tiene facturas aplicadas");
                }

                // Construir JSON del complemento de pago
                var complementoJson = BuildComplementoPagoJson(payment);

                // IMPRIMIR JSON que se enviará a Sapiens
                var jsonString = JsonSerializer.Serialize(complementoJson, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                Console.WriteLine("==========================================");
                Console.WriteLine($"COMPLEMENTO DE PAGO - Payment ID: {payment.Id}");
                Console.WriteLine("==========================================");
                Console.WriteLine(jsonString);
                Console.WriteLine("==========================================");

                // Timbrar con SAPIENS
                var timbradoResponse = await _sapiensService.TimbrarFacturaAsync(complementoJson, "v4");

                // Verificar respuesta exitosa
                if (timbradoResponse.status != "success" || timbradoResponse.data == null)
                {
                    throw new InvalidOperationException(
                        $"Error al timbrar: {timbradoResponse.status}");
                }

                // Guardar datos del timbrado en el pago
                payment.Uuid = timbradoResponse.data.uuid;
                payment.XmlCfdi = timbradoResponse.data.cfdi; // XML timbrado completo
                payment.CadenaOriginalSat = timbradoResponse.data.cadenaOriginalSAT;
                payment.SelloCfdi = timbradoResponse.data.selloCFDI;
                payment.SelloSat = timbradoResponse.data.selloSAT;
                payment.NoCertificadoCfdi = timbradoResponse.data.noCertificadoCFDI;
                payment.NoCertificadoSat = timbradoResponse.data.noCertificadoSAT;
                payment.QrCode = timbradoResponse.data.qrCode;
                payment.TimbradoAt = DateTime.UtcNow;
                payment.Status = "Complemented";
                payment.CompletedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment);

                totalSuccess++;
            }
            catch (Exception ex)
            {
                totalErrors++;
                errors.Add($"Pago {payment.PaymentNumber}: {ex.Message}");

                // Marcar pago con error
                payment.Status = "Error";
                payment.ComplementsWithError = 1;
                await _paymentRepository.UpdateAsync(payment);

                // Registrar error en log
                var log = new Domain.Entities.PaymentComplementLog
                {
                    PaymentId = payment.Id,
                    BatchId = batch.Id,
                    PaymentApplicationId = null,
                    AttemptNumber = 1,
                    Status = "Error",
                    Action = "GenerateBatch",
                    ErrorMessage = ex.Message,
                    AttemptDate = DateTime.UtcNow
                };
                await _logRepository.CreateAsync(log);
            }

            totalProcessed++;
            
            // Actualizar progreso del lote
            batch.ProcessingProgress = (int)((decimal)totalProcessed / batch.TotalPayments * 100);
            batch.ComplementsGenerated = totalSuccess;
            batch.ComplementsWithError = totalErrors;
            await _batchRepository.UpdateAsync(batch);
        }

        // 4. Actualizar estado final del lote
        batch.Status = totalErrors > 0 ? "PartialError" : "Completed";
        batch.CompletedAt = DateTime.UtcNow;
        batch.ProcessedBy = command.ExecutedBy;
        await _batchRepository.UpdateAsync(batch);

        // 5. Retornar resultado
        return new GenerateComplementsResultDto
        {
            TotalProcessed = totalProcessed,
            SuccessCount = totalSuccess,
            ErrorCount = totalErrors,
            Errors = errors,
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber
        };
    }

    /// <summary>
    /// Construye el JSON del complemento de pago según el formato de SAT/Sapiens
    /// </summary>
    private object BuildComplementoPagoJson(Domain.Entities.Payment payment)
    {
        var ic = CultureInfo.InvariantCulture; // CRÍTICO: Usar punto decimal

        // Calcular totales del complemento
        var totalPagado = payment.TotalAmount;
        var totalBaseIVA16 = payment.PaymentApplications.Sum(a => a.TaxBase);
        var totalImpuestoIVA16 = payment.PaymentApplications.Sum(a => a.TaxAmount);

        // Construir documentos relacionados usando DTOs tipados
        var documentosRelacionados = payment.PaymentApplications.Select(app => new DoctoRelacionadoDto
        {
            IdDocumento = app.FolioUUID,
            Serie = app.SerieAndFolio?.Split('-')[0] ?? "",
            Folio = app.SerieAndFolio?.Split('-').Length > 1 ? app.SerieAndFolio.Split('-')[1] : app.SerieAndFolio,
            MonedaDR = app.DocumentCurrency ?? "MXN",
            EquivalenciaDR = "1",
            NumParcialidad = app.PartialityNumber.ToString(),
            ImpSaldoAnt = app.PreviousBalance.ToString("0.00", ic),
            ImpPagado = app.AmountApplied.ToString("0.00", ic),
            ImpSaldoInsoluto = app.NewBalance.ToString("0.00", ic),
            ObjetoImpDR = app.TaxObject ?? "02",
            ImpuestosDR = new ImpuestosDRDto
            {
                TrasladosDR = new[]
                {
                    new TrasladoDRDto
                    {
                        BaseDR = app.TaxBase.ToString("0.000000", ic),
                        ImpuestoDR = app.TaxCode ?? "002",
                        TipoFactorDR = app.TaxFactorType ?? "Tasa",
                        TasaOCuotaDR = app.TaxRate.ToString("0.000000", ic),
                        ImporteDR = app.TaxAmount.ToString("0.000000", ic)
                    }
                }
            }
        }).ToList();

        // Generar folio del complemento
        var folio = payment.PaymentNumber?.Replace("PAG-", "").Replace("-", "") ?? DateTime.Now.ToString("yyyyMMddHHmmss");

        // Construir complemento con [JsonProperty] para forzar "Pago20:Pagos"
        var complemento = new ComplementoDto
        {
            Pago20Pagos = new Pago20PagosDto
            {
                Version = "2.0",
                Totales = new TotalesDto
                {
                    TotalTrasladosBaseIVA16 = totalBaseIVA16.ToString("0.00", ic),
                    TotalTrasladosImpuestoIVA16 = totalImpuestoIVA16.ToString("0.00", ic),
                    MontoTotalPagos = totalPagado.ToString("0.00", ic)
                },
                Pago = new[]
                {
                    new PagoDto
                    {
                        FechaPago = payment.PaymentDate.ToString("yyyy-MM-ddTHH:mm:ss", ic),
                        FormaDePagoP = payment.PaymentFormSAT ?? "03",
                        MonedaP = payment.Currency ?? "MXN",
                        TipoCambioP = "1",
                        Monto = totalPagado.ToString("0.00", ic),
                        DoctoRelacionado = documentosRelacionados.ToArray(),
                        ImpuestosP = new ImpuestosPDto
                        {
                            TrasladosP = new[]
                            {
                                new TrasladoPDto
                                {
                                    BaseP = totalBaseIVA16.ToString("0.000000", ic),
                                    ImpuestoP = "002",
                                    TipoFactorP = "Tasa",
                                    TasaOCuotaP = "0.160000",
                                    ImporteP = totalImpuestoIVA16.ToString("0.000000", ic)
                                }
                            }
                        }
                    }
                }
            }
        };

        // Construir JSON completo
        return new
        {
            Version = "4.0",
            Serie = "CP",
            Folio = folio,
            Fecha = payment.PaymentDate.ToString("yyyy-MM-ddTHH:mm:ss", ic),
            SubTotal = "0",
            Moneda = "XXX",
            Total = "0",
            TipoDeComprobante = "P",
            Exportacion = "01",
            LugarExpedicion = payment.LugarExpedicion ?? "00000",
            Emisor = new
            {
                Rfc = payment.EmisorRfc,
                Nombre = payment.EmisorNombre,
                RegimenFiscal = payment.EmisorRegimenFiscal
            },
            Receptor = new
            {
                Rfc = payment.ReceptorRfc,
                Nombre = payment.ReceptorNombre,
                DomicilioFiscalReceptor = payment.ReceptorDomicilioFiscal,
                RegimenFiscalReceptor = payment.ReceptorRegimenFiscal,
                UsoCFDI = payment.ReceptorUsoCfdi ?? "CP01"
            },
            Conceptos = new[]
            {
                new
                {
                    ClaveProdServ = "84111506",
                    Cantidad = 1,
                    ClaveUnidad = "ACT",
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
