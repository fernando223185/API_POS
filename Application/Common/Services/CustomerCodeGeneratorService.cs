using Application.Abstractions.CRM;

namespace Application.Common.Services
{
    /// <summary>
    /// Servicio para generar códigos automáticos de clientes incrementales únicos
    /// Formato: CLI001, CLI002, CLI003, CLI004...
    /// </summary>
    public interface ICustomerCodeGeneratorService
    {
        Task<string> GenerateNextCustomerCodeAsync();
        Task<bool> IsCodeAvailableAsync(string code);
        Task<string> GetNextAvailableCodeAsync();
    }

    public class CustomerCodeGeneratorService : ICustomerCodeGeneratorService
    {
        private readonly ICustomerRepository _customerRepository;
        private const string DEFAULT_PREFIX = "CLI";
        private const int CODE_LENGTH = 3; // 3 dígitos: 001, 002, 003...

        public CustomerCodeGeneratorService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// Genera el siguiente código incremental disponible
        /// Ejemplos: CLI001, CLI002, CLI003, CLI004...
        /// </summary>
        public async Task<string> GenerateNextCustomerCodeAsync()
        {
            try
            {
                // Obtener el siguiente número secuencial
                var nextNumber = await _customerRepository.GetNextSequentialNumberAsync();
                
                // Formatear código: CLI + número con ceros a la izquierda
                var code = $"{DEFAULT_PREFIX}{nextNumber:D3}";
                
                // Verificar disponibilidad por seguridad
                var isAvailable = await IsCodeAvailableAsync(code);
                
                if (!isAvailable)
                {
                    // Si por alguna razón ya existe, buscar el siguiente disponible
                    return await GetNextAvailableCodeAsync();
                }
                
                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating customer code: {ex.Message}");
                // Fallback: intentar con método alternativo
                return await GetNextAvailableCodeAsync();
            }
        }

        /// <summary>
        /// Verifica si un código está disponible
        /// </summary>
        public async Task<bool> IsCodeAvailableAsync(string code)
        {
            try
            {
                var existingCustomer = await _customerRepository.GetByCodeAsync(code);
                return existingCustomer == null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking code availability: {ex.Message}");
                return false; // Asumir no disponible en caso de error
            }
        }

        /// <summary>
        /// Busca el siguiente código disponible iterando desde el último
        /// Método de seguridad en caso de inconsistencias en la base de datos
        /// </summary>
        public async Task<string> GetNextAvailableCodeAsync()
        {
            try
            {
                // Obtener el último número usado
                var lastNumber = await _customerRepository.GetNextSequentialNumberAsync() - 1;
                
                // Buscar el siguiente disponible
                int candidate = Math.Max(1, lastNumber + 1);
                string candidateCode;
                
                do
                {
                    candidateCode = $"{DEFAULT_PREFIX}{candidate:D3}";
                    
                    if (await IsCodeAvailableAsync(candidateCode))
                    {
                        return candidateCode;
                    }
                    
                    candidate++;
                    
                    // Límite de seguridad para evitar loops infinitos
                    if (candidate > 999999) // CLI999999 es el máximo
                    {
                        throw new InvalidOperationException("Se alcanzó el límite máximo de códigos de cliente");
                    }
                    
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding next available code: {ex.Message}");
                
                // Último recurso: código basado en timestamp
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var fallbackCode = $"CLI{timestamp.ToString()[^3..]}"; // Últimos 3 dígitos del timestamp
                
                return fallbackCode;
            }
        }
    }
}