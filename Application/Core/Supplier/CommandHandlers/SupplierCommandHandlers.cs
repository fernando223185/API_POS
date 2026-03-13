using Application.Abstractions.Common;
using Application.Abstractions.Purchasing;
using Application.Core.Supplier.Commands;
using Application.DTOs.Supplier;
using MediatR;

namespace Application.Core.Supplier.CommandHandlers
{
    /// <summary>
    /// Handler para crear proveedor
    /// </summary>
    public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierResponseDto>
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly ICodeGeneratorService _codeGenerator;

        public CreateSupplierCommandHandler(
            ISupplierRepository supplierRepository,
            ICodeGeneratorService codeGenerator)
        {
            _supplierRepository = supplierRepository;
            _codeGenerator = codeGenerator;
        }

        public async Task<SupplierResponseDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            // Validar RFC si existe
            if (!string.IsNullOrWhiteSpace(request.SupplierData.TaxId))
            {
                if (await _supplierRepository.TaxIdExistsAsync(request.SupplierData.TaxId))
                {
                    throw new InvalidOperationException($"Ya existe un proveedor con el RFC {request.SupplierData.TaxId}");
                }
            }

            // Generar código automático
            var code = await _codeGenerator.GenerateNextCodeAsync("PROV", "Suppliers");

            // Crear proveedor
            var supplier = new Domain.Entities.Supplier
            {
                Code = code,
                Name = request.SupplierData.Name,
                TaxId = request.SupplierData.TaxId,
                ContactPerson = request.SupplierData.ContactPerson,
                Email = request.SupplierData.Email,
                Phone = request.SupplierData.Phone,
                Address = request.SupplierData.Address,
                City = request.SupplierData.City,
                State = request.SupplierData.State,
                ZipCode = request.SupplierData.ZipCode,
                Country = request.SupplierData.Country ?? "México",
                PaymentTermsDays = request.SupplierData.PaymentTermsDays,
                CreditLimit = request.SupplierData.CreditLimit,
                DefaultDiscountPercentage = request.SupplierData.DefaultDiscountPercentage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdSupplier = await _supplierRepository.CreateAsync(supplier);

            // Obtener estadísticas
            var totalOrders = await _supplierRepository.GetPurchaseOrderCountAsync(createdSupplier.Id);
            var totalPurchased = await _supplierRepository.GetTotalPurchasedAsync(createdSupplier.Id);

            return MapToResponseDto(createdSupplier, totalOrders, totalPurchased);
        }

        private SupplierResponseDto MapToResponseDto(Domain.Entities.Supplier supplier, int totalOrders, decimal totalPurchased)
        {
            return new SupplierResponseDto
            {
                Id = supplier.Id,
                Code = supplier.Code,
                Name = supplier.Name,
                TaxId = supplier.TaxId,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                ZipCode = supplier.ZipCode,
                Country = supplier.Country,
                PaymentTermsDays = supplier.PaymentTermsDays,
                CreditLimit = supplier.CreditLimit,
                DefaultDiscountPercentage = supplier.DefaultDiscountPercentage,
                IsActive = supplier.IsActive,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt,
                TotalPurchaseOrders = totalOrders,
                TotalPurchased = totalPurchased
            };
        }
    }

    /// <summary>
    /// Handler para actualizar proveedor
    /// </summary>
    public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierResponseDto>
    {
        private readonly ISupplierRepository _supplierRepository;

        public UpdateSupplierCommandHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<SupplierResponseDto> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);
            if (supplier == null)
            {
                throw new KeyNotFoundException($"Proveedor con ID {request.SupplierId} no encontrado");
            }

            // Validar RFC si cambió
            if (!string.IsNullOrWhiteSpace(request.SupplierData.TaxId) && 
                request.SupplierData.TaxId != supplier.TaxId)
            {
                if (await _supplierRepository.TaxIdExistsAsync(request.SupplierData.TaxId, request.SupplierId))
                {
                    throw new InvalidOperationException($"Ya existe otro proveedor con el RFC {request.SupplierData.TaxId}");
                }
            }

            // Actualizar campos
            supplier.Name = request.SupplierData.Name;
            supplier.TaxId = request.SupplierData.TaxId;
            supplier.ContactPerson = request.SupplierData.ContactPerson;
            supplier.Email = request.SupplierData.Email;
            supplier.Phone = request.SupplierData.Phone;
            supplier.Address = request.SupplierData.Address;
            supplier.City = request.SupplierData.City;
            supplier.State = request.SupplierData.State;
            supplier.ZipCode = request.SupplierData.ZipCode;
            supplier.Country = request.SupplierData.Country;
            supplier.PaymentTermsDays = request.SupplierData.PaymentTermsDays;
            supplier.CreditLimit = request.SupplierData.CreditLimit;
            supplier.DefaultDiscountPercentage = request.SupplierData.DefaultDiscountPercentage;
            supplier.IsActive = request.SupplierData.IsActive;

            await _supplierRepository.UpdateAsync(supplier);

            // Obtener estadísticas
            var totalOrders = await _supplierRepository.GetPurchaseOrderCountAsync(supplier.Id);
            var totalPurchased = await _supplierRepository.GetTotalPurchasedAsync(supplier.Id);

            return MapToResponseDto(supplier, totalOrders, totalPurchased);
        }

        private SupplierResponseDto MapToResponseDto(Domain.Entities.Supplier supplier, int totalOrders, decimal totalPurchased)
        {
            return new SupplierResponseDto
            {
                Id = supplier.Id,
                Code = supplier.Code,
                Name = supplier.Name,
                TaxId = supplier.TaxId,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                ZipCode = supplier.ZipCode,
                Country = supplier.Country,
                PaymentTermsDays = supplier.PaymentTermsDays,
                CreditLimit = supplier.CreditLimit,
                DefaultDiscountPercentage = supplier.DefaultDiscountPercentage,
                IsActive = supplier.IsActive,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt,
                TotalPurchaseOrders = totalOrders,
                TotalPurchased = totalPurchased
            };
        }
    }

    /// <summary>
    /// Handler para eliminar proveedor (desactivar)
    /// </summary>
    public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, bool>
    {
        private readonly ISupplierRepository _supplierRepository;

        public DeleteSupplierCommandHandler(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<bool> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId);
            if (supplier == null)
            {
                return false;
            }

            supplier.IsActive = false;
            await _supplierRepository.UpdateAsync(supplier);

            return true;
        }
    }
}
