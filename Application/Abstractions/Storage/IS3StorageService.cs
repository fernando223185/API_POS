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
        /// <returns>Key del archivo en S3</returns>
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string? folder = null);

        /// <summary>
        /// Eliminar una imagen de S3
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        Task<bool> DeleteImageAsync(string key);

        /// <summary>
        /// Obtener URL pública de una imagen
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <returns>URL pública del archivo</returns>
        string GetPublicUrl(string key);

        /// <summary>
        /// Obtener URL firmada temporal de una imagen
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <param name="expirationMinutes">Minutos de expiración (por defecto 60)</param>
        /// <returns>URL firmada temporal</returns>
        Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60);

        /// <summary>
        /// Verificar si un archivo existe en S3
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        Task<bool> FileExistsAsync(string key);
    }
}
