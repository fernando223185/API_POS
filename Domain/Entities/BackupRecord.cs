using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("BackupRecords")]
    public class BackupRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Nombre del archivo: ERP_20260423_143000.bak</summary>
        [Required]
        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>Key en S3: backups/ERP_20260423_143000.bak</summary>
        [MaxLength(500)]
        public string? S3Key { get; set; }

        /// <summary>Tamaño en bytes</summary>
        public long FileSizeBytes { get; set; }

        /// <summary>Estado: Pending, InProgress, Completed, Failed</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>Mensaje de error si falló</summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>Notas opcionales del usuario</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>Usuario que inició el backup</summary>
        public int? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
