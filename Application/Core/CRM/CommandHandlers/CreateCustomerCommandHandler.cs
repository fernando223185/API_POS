using Application.Abstractions.CRM;
using Application.Common.Services;
using Application.Core.CRM.Commands;
using Application.DTOs.Customer;
using Domain.Entities;
using MediatR;

namespace Application.Core.CRM.CommandHandlers
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponseDto>
    {
        private readonly ICustomerRepository _repository;
        private readonly ICustomerCodeGeneratorService _codeGenerator;

        public CreateCustomerCommandHandler(
            ICustomerRepository repository, 
            ICustomerCodeGeneratorService codeGenerator)
        {
            _repository = repository;
            _codeGenerator = codeGenerator;
        }

        public async Task<CustomerResponseDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ GENERAR CÓDIGO INCREMENTAL ÚNICO
                var generatedCode = await _codeGenerator.GenerateNextCustomerCodeAsync();
                
                Console.WriteLine($"Generated customer code: {generatedCode}");

                // ✅ VALIDACIÓN ADICIONAL DE CÓDIGO ÚNICO (por seguridad)
                var existingCustomer = await _repository.GetByCodeAsync(generatedCode);
                if (existingCustomer != null)
                {
                    // Si existe, usar método de búsqueda avanzada
                    generatedCode = await _codeGenerator.GetNextAvailableCodeAsync();
                    Console.WriteLine($"Code collision detected, using: {generatedCode}");
                }

                // ✅ VALIDACIÓN DE PRICELIST (si se proporciona)
                if (request.CustomerData.PriceListId.HasValue)
                {
                    if (request.CustomerData.PriceListId.Value <= 0)
                    {
                        throw new InvalidOperationException("El PriceListId debe ser mayor a 0 o NULL");
                    }
                }

                // ✅ Crear entidad Customer con código incremental
                var customer = new Customer
                {
                    // ✅ CÓDIGO INCREMENTAL ÚNICO: CLI001, CLI002, CLI003...
                    Code = generatedCode,
                    
                    // Información básica
                    Name = request.CustomerData.Name,
                    LastName = request.CustomerData.LastName,
                    Phone = request.CustomerData.Phone,
                    Email = request.CustomerData.Email,
                    Address = request.CustomerData.Address,
                    TaxId = request.CustomerData.TaxId,
                    ZipCode = request.CustomerData.ZipCode,
                    Commentary = request.CustomerData.Commentary,
                    CountryId = request.CustomerData.CountryId,
                    StateId = request.CustomerData.StateId,
                    InteriorNumber = request.CustomerData.InteriorNumber,
                    ExteriorNumber = request.CustomerData.ExteriorNumber,
                    StatusId = request.CustomerData.StatusId,
                    
                    // ✅ Campos ERP avanzados
                    CompanyName = request.CustomerData.CompanyName,
                    SatTaxRegime = request.CustomerData.SatTaxRegime,
                    SatCfdiUse = request.CustomerData.SatCfdiUse ?? "G03",
                    PriceListId = request.CustomerData.PriceListId,
                    DiscountPercentage = request.CustomerData.DiscountPercentage,
                    CreditLimit = request.CustomerData.CreditLimit,
                    PaymentTermsDays = request.CustomerData.PaymentTermsDays,
                    IsActive = request.CustomerData.IsActive,
                    
                    // Auditoría
                    CreatedAtOriginal = DateTime.UtcNow,  // Campo original
                    CreatedAt = DateTime.UtcNow,          // Campo nuevo
                    CreatedByUserId = request.CreatedByUserId
                };

                // ✅ Guardar en la base de datos
                var createdCustomer = await _repository.CreateAsync(customer);

                Console.WriteLine($"Customer created successfully with ID: {createdCustomer.ID} and Code: {createdCustomer.Code}");

                // Mapear a DTO de respuesta con TODOS los campos
                var response = new CustomerResponseDto
                {
                    Id = createdCustomer.ID,
                    Code = createdCustomer.Code, // ✅ Código incremental generado
                    Name = createdCustomer.Name,
                    LastName = createdCustomer.LastName,
                    Phone = createdCustomer.Phone,
                    Email = createdCustomer.Email,
                    Address = createdCustomer.Address,
                    TaxId = createdCustomer.TaxId,
                    ZipCode = createdCustomer.ZipCode,
                    Commentary = createdCustomer.Commentary,
                    CountryId = createdCustomer.CountryId,
                    StateId = createdCustomer.StateId,
                    InteriorNumber = createdCustomer.InteriorNumber,
                    ExteriorNumber = createdCustomer.ExteriorNumber,
                    StatusId = createdCustomer.StatusId,
                    
                    // ✅ CAMPOS ERP AVANZADOS
                    CompanyName = createdCustomer.CompanyName,
                    SatTaxRegime = createdCustomer.SatTaxRegime,
                    SatCfdiUse = createdCustomer.SatCfdiUse,
                    PriceListId = createdCustomer.PriceListId,
                    PriceListName = createdCustomer.PriceList?.Name,
                    DiscountPercentage = createdCustomer.DiscountPercentage,
                    CreditLimit = createdCustomer.CreditLimit,
                    PaymentTermsDays = createdCustomer.PaymentTermsDays,
                    IsActive = createdCustomer.IsActive,
                    CreatedAt = createdCustomer.CreatedAt ?? createdCustomer.CreatedAtOriginal,
                    UpdatedAt = createdCustomer.UpdatedAt
                };

                return response;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar errores de negocio
                throw;
            }
            catch (Exception ex)
            {
                // Log y re-lanzar otros errores con más detalle
                var innerMessage = ex.InnerException?.Message ?? "";
                var fullMessage = $"Error al crear el cliente: {ex.Message}";
                
                if (innerMessage.Contains("FOREIGN KEY") && innerMessage.Contains("PriceListId"))
                {
                    throw new InvalidOperationException($"El PriceListId '{request.CustomerData.PriceListId}' no existe. Use NULL o un ID válido de la tabla PriceLists.", ex);
                }
                
                Console.WriteLine($"Error in CreateCustomerCommandHandler: {ex.Message}");
                Console.WriteLine($"Inner Exception: {innerMessage}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                throw new InvalidOperationException(fullMessage, ex);
            }
        }
    }
}

