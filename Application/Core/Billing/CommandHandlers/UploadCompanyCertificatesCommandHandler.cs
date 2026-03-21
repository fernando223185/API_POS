using Application.Abstractions.Billing;
using Application.Abstractions.Config;
using Application.Core.Billing.Commands;
using Application.DTOs.Billing;
using MediatR;
using System.Security.Cryptography.X509Certificates;

namespace Application.Core.Billing.CommandHandlers
{
    /// <summary>
    /// Sube los certificados SAT (.cer y .key) de una empresa a S3 en una
    /// carpeta privada y actualiza los paths + contraseña cifrada en Company.
    /// </summary>
    public class UploadCompanyCertificatesCommandHandler
        : IRequestHandler<UploadCompanyCertificatesCommand, UploadCertificatesResponseDto>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICertificateService _certificateService;

        public UploadCompanyCertificatesCommandHandler(
            ICompanyRepository companyRepository,
            ICertificateService certificateService)
        {
            _companyRepository = companyRepository;
            _certificateService = certificateService;
        }

        public async Task<UploadCertificatesResponseDto> Handle(
            UploadCompanyCertificatesCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"🔐 Cargando certificados SAT para empresa ID: {request.CompanyId}");

                // 1. Validar que la empresa existe
                var company = await _companyRepository.GetByIdAsync(request.CompanyId);
                if (company == null)
                    return Fail($"Empresa con ID {request.CompanyId} no encontrada.");

                // 2. Validar extensiones
                if (!HasExtension(request.CerFileName, ".cer"))
                    return Fail("El archivo de certificado debe tener extensión .cer");

                if (!HasExtension(request.KeyFileName, ".key"))
                    return Fail("El archivo de llave privada debe tener extensión .key");

                // 3. Validar el certificado .cer con X509Certificate2
                X509Certificate2 cert;
                try
                {
                    cert = new X509Certificate2(request.CerFileBytes);
                }
                catch (Exception ex)
                {
                    return Fail($"El archivo .cer no es un certificado X.509 válido: {ex.Message}");
                }

                var now = DateTime.UtcNow;
                if (now < cert.NotBefore || now > cert.NotAfter)
                    return Fail($"El certificado está expirado o aún no es válido. " +
                                $"Vigente: {cert.NotBefore:yyyy-MM-dd} – {cert.NotAfter:yyyy-MM-dd}");

                // 4. Subir al bucket privado de certificados
                var cerKey = $"certificates/company_{request.CompanyId}/cert.cer";
                var keyKey = $"certificates/company_{request.CompanyId}/key.key";

                using (var cerStream = new MemoryStream(request.CerFileBytes))
                    await _certificateService.UploadCertAsync(cerKey, cerStream);

                using (var keyStream = new MemoryStream(request.KeyFileBytes))
                    await _certificateService.UploadCertAsync(keyKey, keyStream);

                Console.WriteLine($"   ✅ Certificados subidos a S3: {cerKey}");

                // 5. Cifrar contraseña (la responsabilidad de la clave de cifrado
                //    vive en Infrastructure, no en Application)
                var encryptedPassword = _certificateService.EncryptKeyPassword(request.KeyPassword);

                // 6. Actualizar empresa
                company.SatCertificatePath = cerKey;
                company.SatKeyPath = keyKey;
                company.SatKeyPassword = encryptedPassword;
                await _companyRepository.UpdateAsync(company);

                Console.WriteLine($"   💾 Empresa actualizada con paths de certificados");

                var noCertificado = ExtractNoCertificado(cert);

                return new UploadCertificatesResponseDto
                {
                    Message = "Certificados cargados exitosamente",
                    Error = 0,
                    Data = new CertificateInfoDto
                    {
                        CompanyId = request.CompanyId,
                        SerialNumber = noCertificado,
                        ValidFrom = cert.NotBefore,
                        ValidTo = cert.NotAfter,
                        Subject = cert.Subject,
                        IsValid = true,
                        UploadedAt = now
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando certificados: {ex.Message}");
                return new UploadCertificatesResponseDto
                {
                    Message = $"Error al cargar certificados: {ex.Message}",
                    Error = 2
                };
            }
        }

        // ── helpers ─────────────────────────────────────────────────────────────

        private static UploadCertificatesResponseDto Fail(string msg) =>
            new() { Message = msg, Error = 1 };

        private static bool HasExtension(string fileName, string ext) =>
            Path.GetExtension(fileName).Equals(ext, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// El NoCertificado SAT es el número de serie del certificado expresado como
        /// cadena de dígitos ASCII. En .NET, GetSerialNumber() devuelve los bytes en
        /// orden little-endian, por lo que se invierten y se convierten a char.
        /// </summary>
        internal static string ExtractNoCertificado(X509Certificate2 cert)
        {
            var bytes = cert.GetSerialNumber(); // little-endian
            return new string(bytes.Reverse().Select(b => (char)b).ToArray());
        }
    }
}
