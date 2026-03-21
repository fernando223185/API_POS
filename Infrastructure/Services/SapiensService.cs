using Application.Abstractions.Billing;
using Application.DTOs.Billing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio completo de Sapiens (SW Smarterweb)
    /// Gestiona autenticación, timbrado CFDI y renovación automática de tokens
    /// </summary>
    public class SapiensService : ISapiensService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SapiensService> _logger;
        private readonly HttpClient _httpClient;
        
        // URLs de Sapiens
        private const string TEST_URL = "https://services.test.sw.com.mx";
        private const string PROD_URL = "https://services.sw.com.mx";

        public SapiensService(
            IConfiguration configuration, 
            ILogger<SapiensService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Obtiene un token válido (usa el token infinito de configuración)
        /// </summary>
        public async Task<SapiensTokenDto> GetValidTokenAsync()
        {
            // Leer token infinito de configuración
            var token = _configuration["Sapiens:Token"];
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token de Sapiens no configurado en appsettings.json");
                throw new InvalidOperationException("Token de Sapiens no configurado. Agrega 'Sapiens:Token' en appsettings.json");
            }

            // Token infinito, no expira
            var tokenDto = new SapiensTokenDto
            {
                Token = token,
                ExpiresAt = DateTime.MaxValue // Token infinito
            };

            _logger.LogInformation("Usando token infinito de Sapiens de configuración");
            return await Task.FromResult(tokenDto);
        }

        /// <summary>
        /// Renovación de token (no necesaria con token infinito, pero se mantiene por compatibilidad)
        /// </summary>
        public async Task<SapiensTokenDto> RefreshTokenAsync()
        {
            _logger.LogInformation("RefreshTokenAsync llamado - usando token infinito de configuración");
            return await GetValidTokenAsync();
        }

        /// <summary>
        /// Verifica si el token es válido (siempre true con token infinito)
        /// </summary>
        public bool IsTokenValid()
        {
            var token = _configuration["Sapiens:Token"];
            return !string.IsNullOrEmpty(token);
        }

        // ========================================
        // Métodos de Timbrado CFDI
        // ========================================

        /// <summary>
        /// Timbra un CFDI con el PAC Sapiens
        /// Endpoint: POST /v3/cfdi33/issue/json/{version}
        /// Nota: El endpoint cfdi33 acepta también CFDI 4.0
        /// </summary>
        /// <param name="cfdiData">Objeto con los datos del CFDI a timbrar</param>
        /// <param name="version">Versión de la respuesta (v1, v2, v3, v4)</param>
        /// <returns>Resultado del timbrado con UUID, QR, XML, etc.</returns>
        public async Task<SapiensTimbradoResponseDto> TimbrarFacturaAsync(object cfdiData, string version = "v4")
        {
            try
            {
                // Obtener token válido
                var tokenDto = await GetValidTokenAsync();

                // Leer BaseUrl de configuración
                var baseUrl = _configuration["Sapiens:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    var useProd = bool.Parse(_configuration["Sapiens:UseProdEnvironment"] ?? "false");
                    baseUrl = useProd ? PROD_URL : TEST_URL;
                    _logger.LogWarning("BaseUrl no configurado, usando: {BaseUrl}", baseUrl);
                }

                var url = $"{baseUrl}/v3/cfdi33/issue/json/{version}";

                _logger.LogInformation("Timbrando factura con Sapiens: {Url}", url);

                // Crear request
                var requestDto = new SapiensTimbradoRequestDto
                {
                    data = cfdiData
                };

                // Serializar con opciones específicas (ignorar nulls, formato indentado para logging)
                var jsonOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    PropertyNamingPolicy = null // Respetar nombres de propiedades tal cual
                };

                var jsonContent = JsonSerializer.Serialize(requestDto.data, jsonOptions);
                
                // Log del JSON enviado para debugging
                _logger.LogInformation("📤 JSON enviado a Sapiens:\n{JsonContent}", jsonContent);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/jsontoxml");

                // Configurar headers
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Bearer {tokenDto.Token}");
                request.Content = content;

                // Hacer petición HTTP
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al timbrar factura. Status: {Status}, Response: {Response}", 
                        response.StatusCode, responseContent);

                    // Intentar parsear como error de Sapiens
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<SapiensErrorResponseDto>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (errorResponse != null)
                        {
                            throw new HttpRequestException($"Error de Sapiens: {errorResponse.message} - {errorResponse.messageDetail}");
                        }
                    }
                    catch (JsonException)
                    {
                        // Si no se puede parsear como error, lanzar el contenido crudo
                    }

                    throw new HttpRequestException($"Error al timbrar factura: {response.StatusCode} - {responseContent}");
                }

                // Parsear respuesta exitosa
                var timbradoResponse = JsonSerializer.Deserialize<SapiensTimbradoResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (timbradoResponse == null || timbradoResponse.data == null)
                {
                    _logger.LogError("Respuesta inválida de Sapiens: {Response}", responseContent);
                    throw new InvalidOperationException("Respuesta de timbrado inválida de Sapiens");
                }

                if (timbradoResponse.status != "success")
                {
                    _logger.LogError("Timbrado fallido. Status: {Status}", timbradoResponse.status);
                    throw new InvalidOperationException($"Timbrado fallido: {timbradoResponse.status}");
                }

                _logger.LogInformation("Factura timbrada exitosamente. UUID: {UUID}, Fecha: {Fecha}", 
                    timbradoResponse.data.uuid, timbradoResponse.data.fechaTimbrado);

                return timbradoResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al timbrar factura");
                throw;
            }
        }
    }
}
