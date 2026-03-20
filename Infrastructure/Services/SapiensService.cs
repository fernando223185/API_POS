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
        private const string AUTH_ENDPOINT = "/v2/security/authenticate";
        
        // Caché del token
        private static SapiensTokenDto? _cachedToken;
        private static readonly object _lock = new object();

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
        /// Obtiene un token válido, reutilizando el caché si es posible
        /// </summary>
        public async Task<SapiensTokenDto> GetValidTokenAsync()
        {
            // Verificar si hay un token en caché válido
            lock (_lock)
            {
                if (_cachedToken != null && _cachedToken.IsValid)
                {
                    _logger.LogInformation("Usando token de Sapiens en caché. Expira: {ExpiresAt}", _cachedToken.ExpiresAt);
                    return _cachedToken;
                }
            }

            // Token expirado o no existe, renovar
            _logger.LogInformation("Token de Sapiens expirado o no existente. Solicitando nuevo token...");
            return await RefreshTokenAsync();
        }

        /// <summary>
        /// Fuerza la renovación del token
        /// </summary>
        public async Task<SapiensTokenDto> RefreshTokenAsync()
        {
            try
            {
                // Leer configuración
                var user = _configuration["Sapiens:User"];
                var password = _configuration["Sapiens:Password"];
                var useProd = bool.Parse(_configuration["Sapiens:UseProdEnvironment"] ?? "false");

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException("Credenciales de Sapiens no configuradas en appsettings.json");
                }

                // Determinar URL base
                var baseUrl = useProd ? PROD_URL : TEST_URL;
                var url = $"{baseUrl}{AUTH_ENDPOINT}";

                _logger.LogInformation("Autenticando con Sapiens: {Url} (Environment: {Env})", url, useProd ? "PRODUCCIÓN" : "PRUEBAS");

                // Crear request
                var authRequest = new SapiensAuthRequestDto
                {
                    user = user,
                    password = password
                };

                var jsonContent = JsonSerializer.Serialize(authRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Hacer petición HTTP
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error en autenticación de Sapiens. Status: {Status}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    throw new HttpRequestException($"Error al autenticar con Sapiens: {response.StatusCode} - {responseContent}");
                }

                // Parsear respuesta
                var authResponse = JsonSerializer.Deserialize<SapiensAuthResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (authResponse == null || authResponse.data == null || string.IsNullOrEmpty(authResponse.data.token))
                {
                    _logger.LogError("Respuesta inválida de Sapiens: {Response}", responseContent);
                    throw new InvalidOperationException("Respuesta de autenticación inválida de Sapiens");
                }

                if (authResponse.status != "success")
                {
                    _logger.LogError("Autenticación fallida. Status: {Status}", authResponse.status);
                    throw new InvalidOperationException($"Autenticación fallida: {authResponse.status}");
                }

                // Crear token DTO con información de expiración
                var expiresAt = DateTimeOffset.FromUnixTimeSeconds(authResponse.data.expires_in).UtcDateTime;
                var token = new SapiensTokenDto
                {
                    Token = authResponse.data.token,
                    ExpiresAt = expiresAt
                };

                // Guardar en caché
                lock (_lock)
                {
                    _cachedToken = token;
                }

                _logger.LogInformation("Token de Sapiens obtenido exitosamente. Expira: {ExpiresAt} (en {Minutes} minutos)", 
                    expiresAt, (expiresAt - DateTime.UtcNow).TotalMinutes);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener token de Sapiens");
                throw;
            }
        }

        /// <summary>
        /// Verifica si el token en caché es válido
        /// </summary>
        public bool IsTokenValid()
        {
            lock (_lock)
            {
                return _cachedToken != null && _cachedToken.IsValid;
            }
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

                var jsonContent = JsonSerializer.Serialize(requestDto.data);
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
