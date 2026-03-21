using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Catálogo SAT - Producto o Servicio (c_ClaveProdServ)
    /// Catálogo muy extenso con más de 50,000 registros
    /// </summary>
    [Table("SatProductoServicio")]
    public class SatProductoServicio
    {
        [Key]
        [MaxLength(8)]
        public string ClaveProdServ { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Incluye IVA
        /// </summary>
        [MaxLength(10)]
        public string? IncluyeIva { get; set; }

        /// <summary>
        /// Incluye IEPS
        /// </summary>
        [MaxLength(10)]
        public string? IncluyeIeps { get; set; }

        /// <summary>
        /// Complemento que debe usarse
        /// </summary>
        [MaxLength(200)]
        public string? Complemento { get; set; }

        /// <summary>
        /// Palabras clave para búsquedas
        /// </summary>
        [MaxLength(2000)]
        public string? PalabrasSimilares { get; set; }

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
