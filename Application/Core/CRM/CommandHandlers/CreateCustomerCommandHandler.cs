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
                var generatedCode = await _codeGenerator.GenerateNextCustomerCodeAsync();
                
                Console.WriteLine($"Generated customer code: {generatedCode}");

                var existingCustomer = await _repository.GetByCodeAsync(generatedCode);
                if (existingCustomer != null)
                {
                    generatedCode = await _codeGenerator.GetNextAvailableCodeAsync();
                    Console.WriteLine($"Code collision detected, using: {generatedCode}");
                }

                if (request.CustomerData.PriceListId.HasValue)
                {
                    if (request.CustomerData.PriceListId.Value <= 0)
                    {
                        throw new InvalidOperationException("El PriceListId debe ser mayor a 0 o NULL");
                    }
                }

                var customer = new Customer
                {
                    Code = generatedCode,
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
                    CompanyName = request.CustomerData.CompanyName,
                    SatTaxRegime = request.CustomerData.SatTaxRegime,
                    SatCfdiUse = request.CustomerData.SatCfdiUse ?? "G03",
                    PriceListId = request.CustomerData.PriceListId,
                    DiscountPercentage = request.CustomerData.DiscountPercentage,
                    CreditLimit = request.CustomerData.CreditLimit,
                    PaymentTermsDays = request.CustomerData.PaymentTermsDays,
                    IsActive = request.CustomerData.IsActive,
                    CreatedAtOriginal = DateTime.UtcNow,  
                    CreatedAt = DateTime.UtcNow,          
                    CreatedByUserId = request.CreatedByUserId
                };

                var createdCustomer = await _repository.CreateAsync(customer);

                Console.WriteLine($"Customer created successfully with ID: {createdCustomer.ID} and Code: {createdCustomer.Code}");

                var response = new CustomerResponseDto
                {
                    Id = createdCustomer.ID,
                    Code = createdCustomer.Code, 
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
                throw;
            }
            catch (Exception ex)
            {
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

