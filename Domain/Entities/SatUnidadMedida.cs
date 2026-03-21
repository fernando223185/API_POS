using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Catálogo SAT - Unidad de Medida (c_ClaveUnidad)
    /// Pieza, Kilogramo, Litro, Metro, etc.
    /// </summary>
    [Table("SatUnidadMedida")]
    public class SatUnidadMedida
    {
        [Key]
        [MaxLength(3)]
        public string ClaveUnidad { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada de la unidad
        /// </summary>
        [MaxLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Nota adicional sobre el uso de la unidad
        /// </summary>
        [MaxLength(1000)]
        public string? Nota { get; set; }

        /// <summary>
        /// Símbolo de la unidad (kg, m, l, etc.)
        /// </summary>
        [MaxLength(20)]
        public string? Simbolo { get; set; }

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
