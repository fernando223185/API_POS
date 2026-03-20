namespace Application.DTOs.Billing
{
    /// <summary>
    /// Request para autenticación con Sapiens
    /// </summary>
    public class SapiensAuthRequestDto
    {
        public string user { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response de autenticación con Sapiens
    /// </summary>
    public class SapiensAuthResponseDto
    {
        public SapiensAuthDataDto? data { get; set; }
        public string status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Datos del token de autenticación
    /// </summary>
    public class SapiensAuthDataDto
    {
        public string token { get; set; } = string.Empty;
        public long expires_in { get; set; }
        public string token_type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Configuración de Sapiens desde appsettings.json
    /// </summary>
    public class SapiensConfigDto
    {
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public bool UseProdEnvironment { get; set; } = false;
    }

    /// <summary>
    /// Token de Sapiens con información de expiración
    /// </summary>
    public class SapiensTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !string.IsNullOrEmpty(Token) && !IsExpired;
    }

    // ========================================
    // DTOs para Timbrado CFDI
    // ========================================

    /// <summary>
    /// Request para timbrado de CFDI en formato JSON
    /// El objeto 'data' contiene la estructura completa del CFDI 4.0
    /// </summary>
    public class SapiensTimbradoRequestDto
    {
        /// <summary>
        /// Datos del CFDI a timbrar. Puede ser un objeto dinámico o un objeto serializado
        /// Nota: Los campos Sello, Certificado y NoCertificado deben enviarse vacíos
        /// </summary>
        public object data { get; set; } = new { };
    }

    /// <summary>
    /// Response del servicio de timbrado de Sapiens
    /// </summary>
    public class SapiensTimbradoResponseDto
    {
        public SapiensTimbradoDataDto? data { get; set; }
        public string status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Datos del CFDI timbrado exitosamente
    /// </summary>
    public class SapiensTimbradoDataDto
    {
        /// <summary>
        /// Cadena original del SAT
        /// </summary>
        public string cadenaOriginalSAT { get; set; } = string.Empty;

        /// <summary>
        /// Número de certificado del SAT
        /// </summary>
        public string noCertificadoSAT { get; set; } = string.Empty;

        /// <summary>
        /// Número de certificado del CFDI
        /// </summary>
        public string noCertificadoCFDI { get; set; } = string.Empty;

        /// <summary>
        /// UUID (Folio Fiscal) - Identificador único del comprobante
        /// </summary>
        public string uuid { get; set; } = string.Empty;

        /// <summary>
        /// Sello digital del SAT
        /// </summary>
        public string selloSAT { get; set; } = string.Empty;

        /// <summary>
        /// Sello digital del CFDI
        /// </summary>
        public string selloCFDI { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de timbrado
        /// </summary>
        public string fechaTimbrado { get; set; } = string.Empty;

        /// <summary>
        /// Código QR en formato base64
        /// </summary>
        public string qrCode { get; set; } = string.Empty;

        /// <summary>
        /// XML completo del CFDI timbrado
        /// </summary>
        public string cfdi { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response de error de Sapiens
    /// </summary>
    public class SapiensErrorResponseDto
    {
        public string? message { get; set; }
        public string? messageDetail { get; set; }
        public string status { get; set; } = string.Empty;
    }
}
