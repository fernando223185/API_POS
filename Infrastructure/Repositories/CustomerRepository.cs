using Application.Abstractions.CRM;
using Application.Core.CRM.Queries;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<Customer>> GetByPageAsync(GetCustomerByPageQuery query)
        {
            try
            {
                var pageSize = 10;
                var pageNumber = query.Page;

                IQueryable<Customer> queryable = _dbcontext.Customer
                    .Include(c => c.PriceList)          // ✅ Incluir lista de precios
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(query.search))
                {
                    queryable = queryable.Where(c => 
                        c.Name.Contains(query.search) ||
                        c.Code.Contains(query.search) ||
                        (c.Email != null && c.Email.Contains(query.search)) ||
                        (c.Phone != null && c.Phone.Contains(query.search)) ||
                        (c.CompanyName != null && c.CompanyName.Contains(query.search)) ||
                        (c.TaxId != null && c.TaxId.Contains(query.search))
                    );
                }

                return await queryable
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customers by page: {ex.Message}");
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

        // ✅ NUEVOS MÉTODOS PARA GENERACIÓN DE CÓDIGOS

        /// <summary>
        /// Busca el último código que comience con el prefijo dado
        /// Ejemplo: prefix="JUAN" retorna "JUAN005" si es el último
        /// </summary>
        public async Task<string?> GetLastCodeByPrefixAsync(string prefix)
        {
            try
            {
                return await _dbcontext.Customer
                    .Where(c => c.Code.StartsWith(prefix))
                    .OrderByDescending(c => c.Code)
                    .Select(c => c.Code)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting last code by prefix: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene el siguiente número secuencial para códigos CLI001, CLI002, etc.
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
    }
}

