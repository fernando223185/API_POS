using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstractions.Backup;
using Application.DTOs.Backup;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private readonly POSDbContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly string _backupsBucket;
        private readonly string _connectionString;
        private readonly string _localTempPath;

        public BackupService(POSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("ConnectionString no configurada");
            _backupsBucket = configuration["AWS:S3:BackupsBucketName"]
                ?? throw new Exception("AWS:S3:BackupsBucketName no configurado");
            _localTempPath = configuration["Backups:LocalTempPath"] ?? "C:\\Backups\\ERP";

            var awsAccessKey = configuration["AWS:AccessKey"];
            var awsSecretKey = configuration["AWS:SecretKey"];
            var region = configuration["AWS:Region"] ?? "us-east-1";
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            };

            _s3Client = (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
                ? new AmazonS3Client(awsAccessKey, awsSecretKey, s3Config)
                : new AmazonS3Client(s3Config);
        }

        public async Task<BackupRecordDto> CreateBackupAsync(string? notes, int? userId, CancellationToken cancellationToken = default)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"ERP_{timestamp}.bak";
            var s3Key = $"backups/{fileName}";

            // Carpeta compartida con permisos para SQL Server y la API
            Directory.CreateDirectory(_localTempPath);
            var localPath = Path.Combine(_localTempPath, fileName);

            // Registrar intento en BD
            var record = new BackupRecord
            {
                FileName = fileName,
                S3Key = s3Key,
                Status = "InProgress",
                Notes = notes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.BackupRecords.Add(record);
            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                // 1) Generar .bak en disco temporal via T-SQL
                await ExecuteSqlBackupAsync(localPath, cancellationToken);

                var fileInfo = new FileInfo(localPath);
                long fileSize = fileInfo.Length;

                // 2) Subir a S3 con SSE-AES256
                await UploadToS3Async(localPath, s3Key, cancellationToken);

                // 3) Actualizar registro como completado
                record.Status = "Completed";
                record.FileSizeBytes = fileSize;
                record.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                return MapToDto(record);
            }
            catch (Exception ex)
            {
                record.Status = "Failed";
                record.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync(cancellationToken);
                throw new Exception($"Error al crear backup: {ex.Message}", ex);
            }
            finally
            {
                // Limpiar archivo temporal
                if (File.Exists(localPath))
                    File.Delete(localPath);
            }
        }

        public async Task<List<BackupRecordDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var records = await _context.BackupRecords
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);
            return records.Select(MapToDto).ToList();
        }

        public async Task<BackupRecordDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var record = await _context.BackupRecords.FindAsync(new object[] { id }, cancellationToken);
            return record is null ? null : MapToDto(record);
        }

        public async Task<BackupDownloadDto> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default)
        {
            var record = await _context.BackupRecords.FindAsync(new object[] { id }, cancellationToken)
                ?? throw new Exception($"Backup {id} no encontrado");

            if (record.Status != "Completed" || string.IsNullOrEmpty(record.S3Key))
                throw new Exception("El backup no está disponible para descarga");

            var expiresAt = DateTime.UtcNow.AddMinutes(5);
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _backupsBucket,
                Key = record.S3Key,
                Expires = expiresAt
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return new BackupDownloadDto
            {
                DownloadUrl = url,
                FileName = record.FileName,
                ExpiresAt = expiresAt
            };
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var record = await _context.BackupRecords.FindAsync(new object[] { id }, cancellationToken)
                ?? throw new Exception($"Backup {id} no encontrado");

            // Eliminar de S3 si existe
            if (!string.IsNullOrEmpty(record.S3Key))
            {
                try
                {
                    await _s3Client.DeleteObjectAsync(_backupsBucket, record.S3Key, cancellationToken);
                }
                catch
                {
                    // Si no está en S3 (backup fallido), continuar con el borrado del registro
                }
            }

            _context.BackupRecords.Remove(record);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // -------------------------------------------------------
        // Privados
        // -------------------------------------------------------

        private async Task ExecuteSqlBackupAsync(string localPath, CancellationToken cancellationToken)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var dbName = builder.InitialCatalog;

            var sql = $"BACKUP DATABASE [{dbName}] TO DISK = N'{localPath}' WITH FORMAT";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);
            using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 600 };
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }



        private async Task UploadToS3Async(string localPath, string s3Key, CancellationToken cancellationToken)
        {
            using var fileStream = File.OpenRead(localPath);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = s3Key,
                BucketName = _backupsBucket,
                ContentType = "application/octet-stream",
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest, cancellationToken);
        }

        private static BackupRecordDto MapToDto(BackupRecord r) => new()
        {
            Id = r.Id,
            FileName = r.FileName,
            S3Key = r.S3Key,
            FileSizeBytes = r.FileSizeBytes,
            FileSizeFormatted = FormatBytes(r.FileSizeBytes),
            Status = r.Status,
            ErrorMessage = r.ErrorMessage,
            Notes = r.Notes,
            CreatedByUserId = r.CreatedByUserId,
            CreatedAt = r.CreatedAt,
            CompletedAt = r.CompletedAt
        };

        private static string FormatBytes(long bytes)
        {
            if (bytes == 0) return "—";
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
    }
}
