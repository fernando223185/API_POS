using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Catálogo SAT - Régimen Fiscal (c_RegimenFiscal)
    /// </summary>
    [Table("SatRegimenFiscal")]
    public class SatRegimenFiscal
    {
        [Key]
        [MaxLength(3)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Aplica para personas físicas
        /// </summary>
        public bool AplicaPersonaFisica { get; set; }

        /// <summary>
        /// Aplica para personas morales
        /// </summary>
        public bool AplicaPersonaMoral { get; set; }

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
