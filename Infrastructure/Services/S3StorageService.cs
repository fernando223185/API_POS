using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Abstractions.Storage;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio para gestionar archivos en AWS S3
    /// </summary>
    public class S3StorageService : IS3StorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _region;

        public S3StorageService(IConfiguration configuration)
        {
            // Leer configuraci�n de AWS desde appsettings.json
            var awsAccessKey = configuration["AWS:AccessKey"];
            var awsSecretKey = configuration["AWS:SecretKey"];
            _region = configuration["AWS:Region"] ?? "us-east-1";
            _bucketName = configuration["AWS:S3:BucketName"] ?? throw new Exception("AWS S3 BucketName no configurado");

            // Crear cliente S3
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region)
            };

            if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
            {
                // Usar credenciales expl�citas
                _s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, config);
            }
            else
            {
                // Usar credenciales por defecto (IAM Role en EC2/ECS)
                _s3Client = new AmazonS3Client(config);
            }

            Console.WriteLine($"? S3StorageService initialized - Bucket: {_bucketName}, Region: {_region}");
        }

        /// <summary>
        /// Subir una imagen a S3
        /// </summary>
        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
        {
            try
            {
                // Generar key �nico con timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var extension = Path.GetExtension(fileName);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var uniqueFileName = $"{fileNameWithoutExt}_{timestamp}{extension}";

                // Construir key completo
                var key = string.IsNullOrEmpty(folder)
                    ? uniqueFileName
                    : $"{folder}/{uniqueFileName}";

                Console.WriteLine($"?? Uploading to S3: {key}");

                // ? CORREGIDO: Configurar request SIN ACL
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = key,
                    BucketName = _bucketName,
                    ContentType = contentType
                    // ? REMOVIDO: CannedACL = S3CannedACL.PublicRead
                    // Ahora se usa Bucket Policy en lugar de ACL
                };

                // Subir archivo
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                Console.WriteLine($"? Upload successful: {key}");

                return key;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"? S3 Error: {ex.Message}");
                Console.WriteLine($"   Error Code: {ex.ErrorCode}");
                Console.WriteLine($"   Status Code: {ex.StatusCode}");
                throw new Exception($"Error al subir archivo a S3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Upload Error: {ex.Message}");
                throw new Exception($"Error al subir archivo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Eliminar una imagen de S3
        /// </summary>
        public async Task<bool> DeleteImageAsync(string key)
        {
            try
            {
                Console.WriteLine($"??? Deleting from S3: {key}");

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);

                Console.WriteLine($"? Delete successful: {key}");
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"? S3 Delete Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Delete Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtener URL p�blica de una imagen
        /// </summary>
        public string GetPublicUrl(string key)
        {
            // URL p�blica est�ndar de S3
            return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}";
        }

        /// <summary>
        /// Obtener URL firmada temporal
        /// </summary>
        public async Task<string> GetPresignedUrlAsync(string key, int expirationMinutes = 60)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
                };

                var url = await _s3Client.GetPreSignedURLAsync(request);
                return url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error generating presigned URL: {ex.Message}");
                throw new Exception($"Error al generar URL firmada: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verificar si un archivo existe
        /// </summary>
        public async Task<bool> FileExistsAsync(string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error checking file existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Subir un archivo privado con cifrado AES256 en servidor (sin URL pública).
        /// Indicado para certificados SAT y llaves privadas.
        /// </summary>
        public async Task<string> UploadPrivateFileAsync(Stream fileStream, string key, string? bucketName = null)
        {
            var targetBucket = bucketName ?? _bucketName;
            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = key,
                    BucketName = targetBucket,
                    ContentType = "application/octet-stream",
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                Console.WriteLine($"🔒 Private file uploaded to [{targetBucket}]: {key}");
                return key;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"❌ S3 Error uploading private file: {ex.Message}");
                throw new Exception($"Error al subir archivo a S3: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Descargar un archivo de S3 como arreglo de bytes.
        /// </summary>
        public async Task<byte[]> DownloadFileAsync(string key, string? bucketName = null)
        {
            var sourceBucket = bucketName ?? _bucketName;
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = sourceBucket,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var memStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memStream);

                Console.WriteLine($"📥 File downloaded from [{sourceBucket}]: {key}");
                return memStream.ToArray();
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"❌ S3 Error downloading file: {ex.Message}");
                throw new Exception($"Error al descargar archivo de S3: {ex.Message}", ex);
            }
        }
    }
}
