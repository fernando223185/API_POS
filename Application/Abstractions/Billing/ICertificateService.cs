using Application.DTOs.Billing;

namespace Application.Abstractions.Billing
{
    /// <summary>
    /// Servicio para gestión de certificados SAT (.cer / .key) por empresa.
    /// Extrae NoCertificado, Certificado y genera el Sello para CFDI 4.0.
    /// </summary>
    public interface ICertificateService
    {
        /// <summary>
        /// Devuelve (NoCertificado, Certificado) extraídos del .cer de la empresa.
        /// NoCertificado = número de serie en decimal 20 dígitos (SAT).
        /// Certificado   = Base64 DER del archivo .cer.
        /// Retorna null si la empresa no tiene certificado cargado.
        /// </summary>
        Task<(string NoCertificado, string Certificado)?> GetCfdiCertificateDataAsync(int companyId);

        /// <summary>
        /// Firma el texto de la cadena original con la llave privada (.key) de la empresa.
        /// Retorna el Sello en Base64 (RSA-SHA256).
        /// Retorna null si la empresa no tiene llave cargada.
        /// </summary>
        Task<string?> SignAsync(int companyId, string cadenaOriginal);

        /// <summary>
        /// Estado actualizado de los certificados de una empresa.
        /// </summary>
        Task<CertificateStatusDto> GetStatusAsync(int companyId);

        /// <summary>
        /// Cifra la contraseña de la llave privada antes de almacenarla en BD.
        /// La clave de cifrado reside en Infrastructure (appsettings).
        /// </summary>
        string EncryptKeyPassword(string plainPassword);

        /// <summary>
        /// Sube un archivo de certificado al bucket privado de certificados.
        /// El bucket destino lo resuelve Infrastructure desde la configuración.
        /// </summary>
        Task UploadCertAsync(string key, Stream stream);
    }
}
