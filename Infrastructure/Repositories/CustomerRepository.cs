using Infrastructure.Persistence;
using Domain.Entities;
using Application.Abstractions.CRM;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly POSDbContext _dbcontext;

        public CustomerRepository(POSDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            try
            {
                _dbcontext.Customer.Add(customer);
                await _dbcontext.SaveChangesAsync();
                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating customer: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int customerId)
        {
            try
            {
                var existingCustomer = await _dbcontext.Customer
                    .FirstOrDefaultAsync(c => c.ID == customerId);
                
                if (existingCustomer != null)
                {
                    _dbcontext.Customer.Remove(existingCustomer);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting customer: {ex.Message}");
                throw;
            }
        }

        public async Task<Customer?> GetByIdAsync(int customerId)
        {
            try
            {
                return await _dbcontext.Customer
                    .Include(c => c.PriceList)          // ✅ Incluir lista de precios
                    .Include(c => c.CreatedBy)          // ✅ Incluir usuario creador
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ID == customerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer by ID: {ex.Message}");
                throw;
            }
        }

        public async Task<Customer?> GetByCodeAsync(string code)
        {
            try
            {
                return await _dbcontext.Customer
                    .Include(c => c.PriceList)          // ✅ Incluir lista de precios
                    .Include(c => c.CreatedBy)          // ✅ Incluir usuario creador
                    .FirstOrDefaultAsync(c => c.Code == code);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer by code: {ex.Message}");
                throw;
            }
        }

        public async Task<Customer?> UpdateAsync(Customer customer)
        {
            try
            {
                var existingCustomer = await _dbcontext.Customer
                    .FirstOrDefaultAsync(c => c.ID == customer.ID);
                
                if (existingCustomer != null)
                {
                    // ✅ Actualizar TODOS los campos (originales + ERP)
                    existingCustomer.Name = customer.Name;
                    existingCustomer.LastName = customer.LastName;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.Code = customer.Code;
                    existingCustomer.TaxId = customer.TaxId;
                    existingCustomer.ZipCode = customer.ZipCode;
                    existingCustomer.Commentary = customer.Commentary;
                    existingCustomer.CountryId = customer.CountryId;
                    existingCustomer.StateId = customer.StateId;
                    existingCustomer.InteriorNumber = customer.InteriorNumber;
                    existingCustomer.ExteriorNumber = customer.ExteriorNumber;
                    existingCustomer.StatusId = customer.StatusId;
                    
                    // ✅ Campos ERP avanzados (después de la migración)
                    existingCustomer.CompanyName = customer.CompanyName;
                    existingCustomer.SatTaxRegime = customer.SatTaxRegime;
                    existingCustomer.SatCfdiUse = customer.SatCfdiUse;
                    existingCustomer.PriceListId = customer.PriceListId;
                    existingCustomer.DiscountPercentage = customer.DiscountPercentage;
                    existingCustomer.CreditLimit = customer.CreditLimit;
                    existingCustomer.PaymentTermsDays = customer.PaymentTermsDays;
                    existingCustomer.IsActive = customer.IsActive;
                    existingCustomer.UpdatedAt = DateTime.UtcNow;
                    existingCustomer.UpdatedByUserId = customer.UpdatedByUserId;

                    await _dbcontext.SaveChangesAsync();
                    return existingCustomer;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating customer: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Customer customer)
        {
            try
            {
                _dbcontext.Customer.Remove(customer);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting customer: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                return await _dbcontext.Customer
                    .Include(c => c.PriceList)
                    .Include(c => c.CreatedBy)
                    .OrderBy(c => c.Code)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting paged customers: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _dbcontext.Customer.CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer count: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, int page, int pageSize)
        {
            try
            {
                return await _dbcontext.Customer
                    .Include(c => c.PriceList)
                    .Include(c => c.CreatedBy)
                    .Where(c => c.Name.Contains(searchTerm) || 
                               c.Code.Contains(searchTerm) || 
                               c.Email.Contains(searchTerm) ||
                               c.TaxId.Contains(searchTerm))
                    .OrderBy(c => c.Code)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching customers: {ex.Message}");
                throw;
            }
        }

        // ✅ MÉTODOS PARA GENERACIÓN DE CÓDIGOS INCREMENTALES

        /// <summary>
        /// Busca el último código que comience con el prefijo dado
        /// Ejemplo: prefix="CLI" retorna "CLI005" si es el último
        /// </summary>
        public async Task<string?> GetLastCodeByPrefixAsync(string prefix)
        {
            try
            {
                // Buscar códigos que coincidan con el patrón: CLI + exactamente 3 dígitos
                var codes = await _dbcontext.Customer
                    .Where(c => c.Code.StartsWith(prefix) && c.Code.Length == prefix.Length + 3)
                    .Select(c => c.Code)
                    .ToListAsync();

                // Ordenar por el número extraído del código
                var lastCode = codes
                    .Where(code => int.TryParse(code.Substring(prefix.Length), out _))
                    .OrderByDescending(code => int.Parse(code.Substring(prefix.Length)))
                    .FirstOrDefault();

                Console.WriteLine($"Last code for prefix '{prefix}': {lastCode ?? "None"}");
                return lastCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting last code by prefix: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el siguiente número secuencial para códigos CLI001, CLI002, etc.
        /// ✅ MEJORADO: Más robusto y preciso
        /// </summary>
        public async Task<int> GetNextSequentialNumberAsync()
        {
            try
            {
                Console.WriteLine("Getting next sequential number...");

                // Obtener todos los códigos CLI con exactamente 6 caracteres (CLI + 3 dígitos)
                var codes = await _dbcontext.Customer
                    .Where(c => c.Code.StartsWith("CLI") && c.Code.Length == 6)
                    .Select(c => c.Code)
                    .ToListAsync();

                Console.WriteLine($"Found {codes.Count} existing CLI codes");

                if (codes.Count == 0)
                {
                    Console.WriteLine("No existing codes found, starting with 1");
                    return 1; // Primer cliente
                }

                // Extraer números válidos y encontrar el máximo
                var numbers = codes
                    .Select(code => code.Substring(3)) // Quitar "CLI"
                    .Where(numberStr => int.TryParse(numberStr, out _))
                    .Select(numberStr => int.Parse(numberStr))
                    .ToList();

                if (numbers.Count == 0)
                {
                    Console.WriteLine("No valid numbers found, starting with 1");
                    return 1;
                }

                var maxNumber = numbers.Max();
                var nextNumber = maxNumber + 1;

                Console.WriteLine($"Max existing number: {maxNumber}, next number: {nextNumber}");
                return nextNumber;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting next sequential number: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1; // Fallback seguro
            }
        }

        // ✅ NUEVO: MÉTODO DE PAGINACIÓN AVANZADA CON FILTROS Y ORDENAMIENTO
        public async Task<(IEnumerable<Customer> customers, int totalCount)> GetPagedWithCountAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = "name",
            string? sortDirection = "asc",
            bool? isActive = null,
            int? statusId = null,
            int? priceListId = null)
        {
            try
            {
                Console.WriteLine($"GetPagedWithCountAsync - Page: {page}, PageSize: {pageSize}, Search: '{searchTerm}', Sort: {sortBy} {sortDirection}");

                // Construir query base con includes
                IQueryable<Customer> query = _dbcontext.Customer
                    .Include(c => c.PriceList)
                    .Include(c => c.CreatedBy)
                    .AsNoTracking();

                // ✅ APLICAR FILTROS

                // Filtro de búsqueda
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var search = searchTerm.Trim().ToLower();
                    query = query.Where(c => 
                        c.Name.ToLower().Contains(search) ||
                        c.LastName.ToLower().Contains(search) ||
                        c.Code.ToLower().Contains(search) ||
                        c.Email.ToLower().Contains(search) ||
                        c.Phone.Contains(search) ||
                        c.TaxId.ToLower().Contains(search) ||
                        (c.CompanyName != null && c.CompanyName.ToLower().Contains(search))
                    );
                }

                // Filtro por estado activo/inactivo
                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                // Filtro por StatusId
                if (statusId.HasValue)
                {
                    query = query.Where(c => c.StatusId == statusId.Value);
                }

                // Filtro por PriceListId
                if (priceListId.HasValue)
                {
                    query = query.Where(c => c.PriceListId == priceListId.Value);
                }

                // ✅ OBTENER TOTAL COUNT (antes de aplicar paginación)
                var totalCount = await query.CountAsync();
                Console.WriteLine($"Total count after filters: {totalCount}");

                // ✅ APLICAR ORDENAMIENTO
                query = ApplySorting(query, sortBy, sortDirection);

                // ✅ APLICAR PAGINACIÓN
                var customers = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                Console.WriteLine($"Retrieved {customers.Count} customers for page {page}");

                return (customers, totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPagedWithCountAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Aplica ordenamiento dinámico basado en el campo solicitado
        /// </summary>
        private IQueryable<Customer> ApplySorting(IQueryable<Customer> query, string? sortBy, string? sortDirection)
        {
            var isDescending = sortDirection?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "name" => isDescending 
                    ? query.OrderByDescending(c => c.Name).ThenByDescending(c => c.LastName)
                    : query.OrderBy(c => c.Name).ThenBy(c => c.LastName),
                
                "code" => isDescending 
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code),
                
                "email" => isDescending 
                    ? query.OrderByDescending(c => c.Email)
                    : query.OrderBy(c => c.Email),
                
                "company" => isDescending 
                    ? query.OrderByDescending(c => c.CompanyName ?? "")
                    : query.OrderBy(c => c.CompanyName ?? ""),
                
                "created_at" or "createdat" => isDescending 
                    ? query.OrderByDescending(c => c.CreatedAt ?? c.CreatedAtOriginal)
                    : query.OrderBy(c => c.CreatedAt ?? c.CreatedAtOriginal),
                
                "status" => isDescending 
                    ? query.OrderByDescending(c => c.IsActive).ThenByDescending(c => c.StatusId)
                    : query.OrderBy(c => c.IsActive).ThenBy(c => c.StatusId),
                
                "pricelist" => isDescending 
                    ? query.OrderByDescending(c => c.PriceList!.Name ?? "")
                    : query.OrderBy(c => c.PriceList!.Name ?? ""),
                
                _ => query.OrderBy(c => c.Name).ThenBy(c => c.LastName) // Default: ordenar por nombre
            };
        }
    }
}

