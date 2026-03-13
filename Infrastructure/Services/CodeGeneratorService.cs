using Application.Abstractions.Common;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio centralizado para generación de códigos únicos y secuenciales
    /// Previene duplicados usando transacciones y bloqueos
    /// </summary>
    public class CodeGeneratorService : ICodeGeneratorService
    {
        private readonly POSDbContext _context;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public CodeGeneratorService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateNextCodeAsync(
            string prefix, 
            string tableName, 
            string codeColumnName = "Code", 
            int length = 3)
        {
            // Usar semáforo para prevenir condiciones de carrera
            await _semaphore.WaitAsync();
            
            try
            {
                // Iniciar transacción para asegurar atomicidad
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Obtener todos los códigos que coinciden con el patrón
                    var pattern = $"{prefix}-%";
                    var sql = $"SELECT [{codeColumnName}] FROM [{tableName}] WHERE [{codeColumnName}] LIKE '{pattern}'";
                    
                    var existingCodes = await _context.Database
                        .SqlQueryRaw<string>(sql)
                        .ToListAsync();

                    int nextNumber = 1;

                    if (existingCodes.Any())
                    {
                        // Extraer todos los números y encontrar el máximo
                        var numbers = existingCodes
                            .Select(code =>
                            {
                                var match = Regex.Match(code, $@"{Regex.Escape(prefix)}-(\d+)");
                                if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                                {
                                    return num;
                                }
                                return 0;
                            })
                            .Where(n => n > 0);

                        if (numbers.Any())
                        {
                            nextNumber = numbers.Max() + 1;
                        }
                    }

                    // Generar el nuevo código con padding
                    var newCode = $"{prefix}-{nextNumber.ToString($"D{length}")}";

                    // Verificar que no exista (doble verificación)
                    while (existingCodes.Contains(newCode))
                    {
                        nextNumber++;
                        newCode = $"{prefix}-{nextNumber.ToString($"D{length}")}";
                    }

                    await transaction.CommitAsync();
                    return newCode;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
