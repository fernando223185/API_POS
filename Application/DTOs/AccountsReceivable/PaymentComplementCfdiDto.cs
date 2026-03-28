using Newtonsoft.Json;

namespace Application.DTOs.AccountsReceivable;

/// <summary>
/// Estructura del nodo Complemento para CFDI de Pago 2.0
/// Usa [JsonProperty] para forzar el nombre "Pago20:Pagos" con dos puntos
/// </summary>
public class ComplementoDto
{
    [JsonProperty("Pago20:Pagos")]
    public Pago20PagosDto Pago20Pagos { get; set; } = null!;
}

public class Pago20PagosDto
{
    public string Version { get; set; } = null!;
    public TotalesDto Totales { get; set; } = null!;
    public PagoDto[] Pago { get; set; } = null!;
}

public class TotalesDto
{
    public string TotalTrasladosBaseIVA16 { get; set; } = null!;
    public string TotalTrasladosImpuestoIVA16 { get; set; } = null!;
    public string MontoTotalPagos { get; set; } = null!;
}

public class PagoDto
{
    public string FechaPago { get; set; } = null!;
    public string FormaDePagoP { get; set; } = null!;
    public string MonedaP { get; set; } = null!;
    public string TipoCambioP { get; set; } = null!;
    public string Monto { get; set; } = null!;
    public DoctoRelacionadoDto[] DoctoRelacionado { get; set; } = null!;
    public ImpuestosPDto? ImpuestosP { get; set; }
}

public class DoctoRelacionadoDto
{
    public string IdDocumento { get; set; } = null!;
    public string Serie { get; set; } = null!;
    public string Folio { get; set; } = null!;
    public string MonedaDR { get; set; } = null!;
    public string EquivalenciaDR { get; set; } = null!;
    public string NumParcialidad { get; set; } = null!;
    public string ImpSaldoAnt { get; set; } = null!;
    public string ImpPagado { get; set; } = null!;
    public string ImpSaldoInsoluto { get; set; } = null!;
    public string ObjetoImpDR { get; set; } = null!;
    public ImpuestosDRDto? ImpuestosDR { get; set; }
}

public class ImpuestosDRDto
{
    public TrasladoDRDto[] TrasladosDR { get; set; } = null!;
}

public class TrasladoDRDto
{
    public string BaseDR { get; set; } = null!;
    public string ImpuestoDR { get; set; } = null!;
    public string TipoFactorDR { get; set; } = null!;
    public string TasaOCuotaDR { get; set; } = null!;
    public string ImporteDR { get; set; } = null!;
}

public class ImpuestosPDto
{
    public TrasladoPDto[] TrasladosP { get; set; } = null!;
}

public class TrasladoPDto
{
    public string BaseP { get; set; } = null!;
    public string ImpuestoP { get; set; } = null!;
    public string TipoFactorP { get; set; } = null!;
    public string TasaOCuotaP { get; set; } = null!;
    public string ImporteP { get; set; } = null!;
}
