using Application.DTOs.Supplier;
using MediatR;

namespace Application.Core.Supplier.Commands
{
    /// <summary>
    /// Comando para crear proveedor
    /// </summary>
    public class CreateSupplierCommand : IRequest<SupplierResponseDto>
    {
        public CreateSupplierDto SupplierData { get; set; }

        public CreateSupplierCommand(CreateSupplierDto supplierData)
        {
            SupplierData = supplierData;
        }
    }

    /// <summary>
    /// Comando para actualizar proveedor
    /// </summary>
    public class UpdateSupplierCommand : IRequest<SupplierResponseDto>
    {
        public int SupplierId { get; set; }
        public UpdateSupplierDto SupplierData { get; set; }

        public UpdateSupplierCommand(int supplierId, UpdateSupplierDto supplierData)
        {
            SupplierId = supplierId;
            SupplierData = supplierData;
        }
    }

    /// <summary>
    /// Comando para eliminar proveedor (desactivar)
    /// </summary>
    public class DeleteSupplierCommand : IRequest<bool>
    {
        public int SupplierId { get; set; }

        public DeleteSupplierCommand(int supplierId)
        {
            SupplierId = supplierId;
        }
    }
}
