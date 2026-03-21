using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Catálogo SAT - Forma de Pago (c_FormaPago)
    /// Efectivo, Tarjeta, Transferencia, etc.
    /// </summary>
    [Table("SatFormaPago")]
    public class SatFormaPago
    {
        [Key]
        [MaxLength(2)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Indica si es dinero en efectivo o bancarizado
        /// </summary>
        [MaxLength(10)]
        public string? Bancarizado { get; set; }

        /// <summary>
        /// Número de operación aplicable
        /// </summary>
        [MaxLength(100)]
        public string? NumeroOperacion { get; set; }

        /// <summary>
        /// RFC de la entidad bancaria
        /// </summary>
        [MaxLength(100)]
        public string? RfcEmisorCtaOrdenante { get; set; }

        /// <summary>
        /// Cuenta bancaria aplicable
        /// </summary>
        [MaxLength(100)]
        public string? CtaOrdenante { get; set; }

        /// <summary>
        /// RFC de la entidad bancaria beneficiaria
        /// </summary>
        [MaxLength(100)]
        public string? RfcEmisorCtaBeneficiario { get; set; }

        /// <summary>
        /// Cuenta bancaria beneficiaria
        /// </summary>
        [MaxLength(100)]
        public string? CtaBeneficiario { get; set; }

        /// <summary>
        /// Tipo de cadena de pago
        /// </summary>
        [MaxLength(100)]
        public string? TipoCadenaPago { get; set; }

        /// <summary>
        /// Fecha de inicio de vigencia
        /// </summary>
        public DateTime? FechaInicioVigencia { get; set; }

        /// <summary>
        /// Fecha de fin de vigencia (null si está vigente)
        /// </summary>
        public DateTime? FechaFinVigencia { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
