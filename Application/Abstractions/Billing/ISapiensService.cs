using Application.DTOs.Billing;

namespace Application.Abstractions.Billing
{
    /// <summary>
    /// Servicio completo de Sapiens (SW Smarterweb) para autenticación y timbrado CFDI
    /// </summary>
    public interface ISapiensService
    {
        // ========================================
        // Métodos de Autenticación
        // ========================================

        /// <summary>
        /// Obtiene un token de autenticación válido de Sapiens
        /// Reutiliza el token en caché si aún es válido, o solicita uno nuevo si expiró
        /// </summary>
        /// <returns>Token de autenticación válido</returns>
        Task<SapiensTokenDto> GetValidTokenAsync();

        /// <summary>
        /// Fuerza la renovación del token de autenticación
        /// </summary>
        /// <returns>Nuevo token de autenticación</returns>
        Task<SapiensTokenDto> RefreshTokenAsync();

        /// <summary>
        /// Verifica si el token actual es válido
        /// </summary>
        /// <returns>True si el token es válido, false si está expirado o no existe</returns>
        bool IsTokenValid();

        // ========================================
        // Métodos de Timbrado CFDI
        // ========================================

        /// <summary>
        /// Timbra un CFDI con el PAC Sapiens
        /// Los campos Sello, Certificado y NoCertificado se generan automáticamente
        /// </summary>
        /// <param name="cfdiData">Objeto con los datos del CFDI a timbrar</param>
        /// <param name="version">Versión de la respuesta (v1, v2, v3, v4). Por defecto: v4</param>
        /// <returns>Resultado del timbrado con UUID, QR, XML, etc.</returns>
        Task<SapiensTimbradoResponseDto> TimbrarFacturaAsync(object cfdiData, string version = "v4");
    }
}
