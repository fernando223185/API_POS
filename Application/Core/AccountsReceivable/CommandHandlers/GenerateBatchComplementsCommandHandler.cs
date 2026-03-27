using Application.Abstractions.AccountsReceivable;
using Application.Abstractions.Billing;
using Application.Core.AccountsReceivable.Commands;
using Application.DTOs.AccountsReceivable;
using MediatR;
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
        // Calcular totales del complemento
        var totalPagado = payment.TotalAmount;
        var totalBaseIVA16 = payment.PaymentApplications.Sum(a => a.TaxBase);
        var totalImpuestoIVA16 = payment.PaymentApplications.Sum(a => a.TaxAmount);

        // Construir documentos relacionados
        var documentosRelacionados = payment.PaymentApplications.Select(app => new
        {
            IdDocumento = app.FolioUUID,
            Serie = app.SerieAndFolio?.Split('-')[0] ?? "",
            Folio = app.SerieAndFolio?.Split('-').Length > 1 ? app.SerieAndFolio.Split('-')[1] : app.SerieAndFolio,
            MonedaDR = app.DocumentCurrency ?? "MXN",
            EquivalenciaDR = app.DocumentExchangeRate.ToString("0.000000"),
            NumParcialidad = app.PartialityNumber.ToString(),
            ImpSaldoAnt = app.PreviousBalance.ToString("0.00"),
            ImpPagado = app.AmountApplied.ToString("0.00"),
            ImpSaldoInsoluto = app.NewBalance.ToString("0.00"),
            ObjetoImpDR = app.TaxObject ?? "02", // 02 = Sí objeto de impuestos
            ImpuestosDR = new
            {
                TrasladosDR = new[]
                {
                    new
                    {
                        BaseDR = app.TaxBase.ToString("0.000000"),
                        ImpuestoDR = app.TaxCode ?? "002", // 002 = IVA
                        TipoFactorDR = app.TaxFactorType ?? "Tasa",
                        TasaOCuotaDR = app.TaxRate.ToString("0.000000"),
                        ImporteDR = app.TaxAmount.ToString("0.000000")
                    }
                }
            }
        }).ToList();

        // Generar folio del complemento (puedes usar el PaymentNumber)
        var folio = payment.PaymentNumber?.Replace("PAG-", "") ?? DateTime.Now.ToString("yyyyMMddHHmmss");

        // Construir JSON completo
        return new
        {
            Version = "4.0",
            Serie = "CP", // Serie para complementos de pago
            Folio = folio,
            Fecha = payment.PaymentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
            SubTotal = "0",
            Moneda = "XXX", // Moneda XXX para complementos de pago (sin moneda)
            Total = "0",
            TipoDeComprobante = "P", // P = Pago
            Exportacion = "01", // 01 = No aplica
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
                UsoCFDI = payment.ReceptorUsoCfdi ?? "CP01" // CP01 = Pagos
            },
            Conceptos = new[]
            {
                new
                {
                    ClaveProdServ = "84111506", // Clave SAT para servicios de facturación
                    Cantidad = 1,
                    ClaveUnidad = "ACT", // Actividad
                    Descripcion = "Pago",
                    ValorUnitario = "0",
                    Importe = "0",
                    ObjetoImp = "01" // 01 = No objeto de impuestos (para el concepto genérico)
                }
            },
            Complemento = new
            {
                Any = new[]
                {
                    new
                    {
                        Pago20Pagos = new
                        {
                            Version = "2.0",
                            Totales = new
                            {
                                TotalTrasladosBaseIVA16 = totalBaseIVA16.ToString("0.00"),
                                TotalTrasladosImpuestoIVA16 = totalImpuestoIVA16.ToString("0.00"),
                                MontoTotalPagos = totalPagado.ToString("0.00")
                            },
                            Pago = new[]
                            {
                                new
                                {
                                    FechaPago = payment.PaymentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                    FormaDePagoP = payment.PaymentFormSAT ?? "03", // 03 = Transferencia
                                    MonedaP = payment.Currency ?? "MXN",
                                    TipoCambioP = (payment.ExchangeRate).ToString("0.000000"),
                                    Monto = totalPagado.ToString("0.00"),
                                    NumOperacion = payment.Reference, // Número de operación bancaria (opcional)
                                    CtaBeneficiario = payment.BankAccountDestination, // Cuenta destino (opcional, últimos 4 dígitos)
                                    DoctoRelacionado = documentosRelacionados.ToArray(),
                                    ImpuestosP = new
                                    {
                                        TrasladosP = new[]
                                        {
                                            new
                                            {
                                                BaseP = totalBaseIVA16.ToString("0.000000"),
                                                ImpuestoP = "002", // 002 = IVA
                                                TipoFactorP = "Tasa",
                                                TasaOCuotaP = "0.160000",
                                                ImporteP = totalImpuestoIVA16.ToString("0.000000")
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
