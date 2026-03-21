using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Catálogo SAT - Método de Pago (c_MetodoPago)
    /// PUE = Pago en una sola exhibición
    /// PPD = Pago en parcialidades o diferido
    /// </summary>
    [Table("SatMetodoPago")]
    public class SatMetodoPago
    {
        [Key]
        [MaxLength(3)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

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
