using Application.Abstractions.Billing;
using Application.Abstractions.Config;
using Application.Abstractions.Storage;
using Application.DTOs.Billing;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementación de ICertificateService.
    /// Lee los certificados SAT desde S3 (privado) y proporciona:
    ///   - NoCertificado (número de serie en decimal 20 dígitos)
    ///   - Certificado   (base64 DER del .cer)
    ///   - Sello         (firma RSA-SHA256 de la cadena original)
    /// </summary>
    public class CertificateService : ICertificateService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IS3StorageService _s3;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// Bucket exclusivo para certificados SAT.
        /// Configurado en AWS:S3:CertificatesBucketName.
        /// </summary>
        private readonly string _certsBucket;

        public CertificateService(
            ICompanyRepository companyRepository,
            IS3StorageService s3,
            IConfiguration configuration)
        {
            _companyRepository = companyRepository;
            _s3 = s3;
            _configuration = configuration;
            _certsBucket = configuration["AWS:S3:CertificatesBucketName"]
                ?? throw new InvalidOperationException("AWS:S3:CertificatesBucketName no configurado en appsettings.");
        }

        // ── GetCfdiCertificateDataAsync ──────────────────────────────────────────

        public async Task<(string NoCertificado, string Certificado)?> GetCfdiCertificateDataAsync(int companyId)
        {
            Console.WriteLine($"🔍 [CertService] GetCfdiCertificateDataAsync - companyId={companyId}");

            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                Console.WriteLine($"❌ [CertService] Empresa {companyId} no encontrada en BD");
                return null;
            }

            Console.WriteLine($"✅ [CertService] Empresa encontrada: {company.LegalName}");
            Console.WriteLine($"   SatCertificatePath = '{company.SatCertificatePath}'");
            Console.WriteLine($"   SatKeyPath         = '{company.SatKeyPath}'");
            Console.WriteLine($"   _certsBucket       = '{_certsBucket}'");

            if (string.IsNullOrEmpty(company.SatCertificatePath))
            {
                Console.WriteLine($"⚠️  [CertService] SatCertificatePath vacío — no se puede obtener certificado");
                return null;
            }

            Console.WriteLine($"📥 [CertService] Descargando .cer desde S3...");
            var cerBytes = await _s3.DownloadFileAsync(company.SatCertificatePath, _certsBucket);
            Console.WriteLine($"✅ [CertService] .cer descargado: {cerBytes.Length} bytes");

            var cert = new X509Certificate2(cerBytes);
            var noCertificado = ExtractNoCertificado(cert);
            var certificado = Convert.ToBase64String(cerBytes);

            Console.WriteLine($"✅ [CertService] NoCertificado={noCertificado}, Certificado(len)={certificado.Length}");
            return (noCertificado, certificado);
        }

        // ── SignAsync ────────────────────────────────────────────────────────────

        /// <summary>
        /// Firma la cadena original con la llave privada (.key) del SAT.
        ///
        /// Notas sobre el archivo .key del SAT:
        ///   El SAT emite llave privadas en formato PKCS#8 cifrado (DER).
        ///   Se usa RSA.ImportEncryptedPkcs8PrivateKey con la contraseña en Latin-1.
        ///   Si el algoritmo de cifrado del .key no es soportado directamente por .NET,
        ///   se recomienda pre-convertir la llave con OpenSSL:
        ///     openssl pkcs8 -inform DER -in key.key -out key.pem -passin pass:PASSWORD
        /// </summary>
        public async Task<string?> SignAsync(int companyId, string cadenaOriginal)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null
                || string.IsNullOrEmpty(company.SatKeyPath)
                || string.IsNullOrEmpty(company.SatKeyPassword))
                return null;

            var keyBytes = await _s3.DownloadFileAsync(company.SatKeyPath, _certsBucket);
            var password = DecryptPassword(company.SatKeyPassword);

            using var rsa = RSA.Create();

            // El SAT usa PKCS#8 cifrado; la contraseña se pasa en Latin-1 (ISO-8859-1)
            var passwordBytes = Encoding.Latin1.GetBytes(password);
            rsa.ImportEncryptedPkcs8PrivateKey(passwordBytes, keyBytes, out _);

            var dataBytes = Encoding.UTF8.GetBytes(cadenaOriginal);
            var signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        // ── GetStatusAsync ───────────────────────────────────────────────────────

        public async Task<CertificateStatusDto> GetStatusAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null || string.IsNullOrEmpty(company.SatCertificatePath))
                return new CertificateStatusDto { CompanyId = companyId, HasCertificates = false };

            try
            {
                var cerBytes = await _s3.DownloadFileAsync(company.SatCertificatePath, _certsBucket);
                var cert = new X509Certificate2(cerBytes);
                var now = DateTime.UtcNow;

                return new CertificateStatusDto
                {
                    CompanyId = companyId,
                    HasCertificates = true,
                    CertificateInfo = new CertificateInfoDto
                    {
                        CompanyId = companyId,
                        SerialNumber = ExtractNoCertificado(cert),
                        ValidFrom = cert.NotBefore,
                        ValidTo = cert.NotAfter,
                        Subject = cert.Subject,
                        IsValid = now >= cert.NotBefore && now <= cert.NotAfter,
                        UploadedAt = now
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error leyendo certificado de S3 para empresa {companyId}: {ex.Message}");
                return new CertificateStatusDto { CompanyId = companyId, HasCertificates = false };
            }
        }

        // ── UploadCertAsync ──────────────────────────────────────────────────────

        /// <summary>
        /// Sube un archivo al bucket exclusivo de certificados con SSE-AES256.
        /// </summary>
        public async Task UploadCertAsync(string key, Stream stream)
        {
            await _s3.UploadPrivateFileAsync(stream, key, _certsBucket);
        }

        // ── EncryptKeyPassword ───────────────────────────────────────────────────

        /// <summary>
        /// Cifra la contraseña con AES-256-CBC usando la clave de Certificate:EncryptionKey.
        /// Formato del resultado: "base64(IV):base64(ciphertext)"
        /// </summary>
        public string EncryptKeyPassword(string plainPassword)
        {
            var encKey = _configuration["Certificate:EncryptionKey"]
                         ?? throw new InvalidOperationException("Certificate:EncryptionKey no configurado en appsettings.");

            var keyBytes = DeriveKey(encKey);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plaintextBytes = Encoding.UTF8.GetBytes(plainPassword);
            var cipherBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

            return $"{Convert.ToBase64String(aes.IV)}:{Convert.ToBase64String(cipherBytes)}";
        }

        // ── helpers ─────────────────────────────────────────────────────────────

        /// <summary>
        /// El NoCertificado SAT es el número de serie del certificado expresado como
        /// cadena de dígitos ASCII. GetSerialNumber() devuelve bytes little-endian
        /// por lo que se invierten antes de convertir cada byte en su carácter ASCII.
        /// </summary>
        private static string ExtractNoCertificado(X509Certificate2 cert)
        {
            var bytes = cert.GetSerialNumber(); // little-endian
            return new string(bytes.Reverse().Select(b => (char)b).ToArray());
        }

        /// <summary>
        /// Descifra la contraseña almacenada en BD (formato "base64-iv:base64-cipher").
        /// </summary>
        private string DecryptPassword(string encryptedValue)
        {
            var parts = encryptedValue.Split(':');
            if (parts.Length != 2)
                throw new InvalidOperationException("Formato de contraseña cifrada inválido.");

            var iv = Convert.FromBase64String(parts[0]);
            var cipherBytes = Convert.FromBase64String(parts[1]);

            var encKey = _configuration["Certificate:EncryptionKey"]
                         ?? throw new InvalidOperationException("Certificate:EncryptionKey no configurado");

            var keyBytes = DeriveKey(encKey);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plaintextBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plaintextBytes);
        }

        private static byte[] DeriveKey(string key)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }
}
