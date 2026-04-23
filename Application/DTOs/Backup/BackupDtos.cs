namespace Application.DTOs.Backup
{
    public class BackupRecordDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? S3Key { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? Notes { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class CreateBackupDto
    {
        public string? Notes { get; set; }
    }

    public class BackupDownloadDto
    {
        /// <summary>URL pre-firmada de S3 con expiración de 5 minutos</summary>
        public string DownloadUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
