namespace Application.DTOs.Billing
{
    public class UploadCertificatesResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public CertificateInfoDto? Data { get; set; }
    }

    public class CertificateInfoDto
    {
        public int CompanyId { get; set; }
        /// <summary>Número de certificado SAT (serial en formato decimal de 20 dígitos)</summary>
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string Subject { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class CertificateStatusDto
    {
        public int CompanyId { get; set; }
        public bool HasCertificates { get; set; }
        public CertificateInfoDto? CertificateInfo { get; set; }
    }
}
