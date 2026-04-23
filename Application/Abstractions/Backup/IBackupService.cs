using Application.DTOs.Backup;

namespace Application.Abstractions.Backup
{
    public interface IBackupService
    {
        /// <summary>
        /// Crea un backup de la BD, lo sube a S3 y guarda el registro.
        /// </summary>
        Task<BackupRecordDto> CreateBackupAsync(string? notes, int? userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lista todos los backups registrados.
        /// </summary>
        Task<List<BackupRecordDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un backup por ID.
        /// </summary>
        Task<BackupRecordDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera una URL pre-firmada de S3 válida por 5 minutos para descargar el backup.
        /// </summary>
        Task<BackupDownloadDto> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina el registro y el archivo del bucket S3.
        /// </summary>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
