using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Catálogo SAT - Tipo de Comprobante (c_TipoDeComprobante)
    /// I = Ingreso, E = Egreso, T = Traslado, N = Nómina, P = Pago
    /// </summary>
    [Table("SatTipoComprobante")]
    public class SatTipoComprobante
    {
        [Key]
        [MaxLength(1)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Valor que se puede registrar en el atributo Moneda
        /// </summary>
        [MaxLength(50)]
        public string? ValorMaximo { get; set; }

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
