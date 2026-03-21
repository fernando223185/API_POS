using Application.DTOs.Billing;
using MediatR;

namespace Application.Core.Billing.Commands
{
    /// <summary>
    /// Sube los certificados SAT (.cer y .key) de una empresa a S3 (privado)
    /// y actualiza los paths en la entidad Company.
    /// El controller es responsable de leer los bytes del IFormFile antes de crear este comando.
    /// </summary>
    public class UploadCompanyCertificatesCommand : IRequest<UploadCertificatesResponseDto>
    {
        public int CompanyId { get; set; }

        /// <summary>Bytes del archivo .cer del SAT</summary>
        public byte[] CerFileBytes { get; set; } = Array.Empty<byte>();

        /// <summary>Nombre original del archivo .cer (para validar extensión)</summary>
        public string CerFileName { get; set; } = string.Empty;

        /// <summary>Bytes del archivo .key del SAT</summary>
        public byte[] KeyFileBytes { get; set; } = Array.Empty<byte>();

        /// <summary>Nombre original del archivo .key (para validar extensión)</summary>
        public string KeyFileName { get; set; } = string.Empty;

        /// <summary>Contraseña de la llave privada (en texto plano)</summary>
        public string KeyPassword { get; set; } = string.Empty;
    }
}
