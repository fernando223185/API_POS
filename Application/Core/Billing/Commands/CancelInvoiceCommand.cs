using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Commands
{
    /// <summary>
    /// Comando para cancelar una factura timbrada ante el SAT vía Sapiens
    /// </summary>
    public class CancelInvoiceCommand : IRequest<CancelInvoiceResultDto>
    {
        public int InvoiceId { get; set; }
        /// <summary>Motivo SAT: 01=Con sustitución, 02=Sin sustitución, 03=No se realizó, 04=Global</summary>
        public string Motivo { get; set; } = string.Empty;
        /// <summary>UUID del CFDI sustituto. Solo requerido cuando Motivo = "01"</summary>
        public string? FolioSustitucion { get; set; }
        /// <summary>Razón interna (texto libre para el usuario)</summary>
        public string? Reason { get; set; }
        public int UserId { get; set; }
    }
}
