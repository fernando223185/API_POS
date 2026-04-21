namespace Application.Abstractions.Storage
{
    /// <summary>
    /// Interfaz para el servicio de almacenamiento en AWS S3
    /// </summary>
    public interface IS3StorageService
    {
        /// <summary>
        /// Subir una imagen a S3
        /// </summary>
        /// <param name="fileStream">Stream del archivo</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="contentType">Tipo de contenido (image/jpeg, image/png, etc.)</param>
        /// <param name="folder">Carpeta dentro del bucket (opcional)</param>
        /// <param name="bucketName">Bucket destino. Si es null usa el bucket por defecto.</param>
        /// <returns>Key del archivo en S3</returns>
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string? folder = null, string? bucketName = null);

        /// <summary>
        /// Eliminar una imagen de S3
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <param name="bucketName">Bucket origen. Si es null usa el bucket por defecto.</param>
        Task<bool> DeleteImageAsync(string key, string? bucketName = null);

        /// <summary>
        /// Obtener URL p�blica de una imagen
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <param name="bucketName">Bucket origen. Si es null usa el bucket por defecto.</param>
        /// <returns>URL p�blica del archivo</returns>
        string GetPublicUrl(string key, string? bucketName = null);

        /// <summary>
        /// Obtener URL firmada temporal de una imagen
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <param name="expirationMinutes">Minutos de expiraci�n (por defecto 60)</param>
        /// <returns>URL firmada temporal</returns>
        Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60);

        /// <summary>
        /// Verificar si un archivo existe en S3
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        Task<bool> FileExistsAsync(string key);

        /// <summary>
        /// Subir un archivo privado a S3 con cifrado en servidor (sin acceso público).
        /// Usar para archivos sensibles como certificados SAT (.cer, .key).
        /// </summary>
        /// <param name="fileStream">Stream del archivo</param>
        /// <param name="key">Key completa del archivo en S3 (ruta + nombre)</param>
        /// <param name="bucketName">Bucket destino. Si es null usa el bucket por defecto.</param>
        /// <returns>Key almacenada en S3</returns>
        Task<string> UploadPrivateFileAsync(Stream fileStream, string key, string? bucketName = null);

        /// <summary>
        /// Descargar un archivo de S3 como bytes.
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <param name="bucketName">Bucket origen. Si es null usa el bucket por defecto.</param>
        /// <returns>Contenido del archivo en bytes</returns>
        Task<byte[]> DownloadFileAsync(string key, string? bucketName = null);
    }
}
