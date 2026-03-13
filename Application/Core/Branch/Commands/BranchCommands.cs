using Application.DTOs.Branch;
using MediatR;

namespace Application.Core.Branch.Commands
{
    /// <summary>
    /// Comando para crear una nueva sucursal
    /// </summary>
    public class CreateBranchCommand : IRequest<BranchResponseDto>
    {
        public CreateBranchDto BranchData { get; set; }
        public int CreatedByUserId { get; set; }

        public CreateBranchCommand(CreateBranchDto branchData, int createdByUserId)
        {
            BranchData = branchData;
            CreatedByUserId = createdByUserId;
        }
    }

    /// <summary>
    /// Comando para actualizar una sucursal existente
    /// </summary>
    public class UpdateBranchCommand : IRequest<BranchResponseDto>
    {
        public int BranchId { get; set; }
        public UpdateBranchDto BranchData { get; set; }
        public int UpdatedByUserId { get; set; }

        public UpdateBranchCommand(int branchId, UpdateBranchDto branchData, int updatedByUserId)
        {
            BranchId = branchId;
            BranchData = branchData;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para dar de baja lógica una sucursal
    /// </summary>
    public class DeactivateBranchCommand : IRequest<bool>
    {
        public int BranchId { get; set; }
        public int UpdatedByUserId { get; set; }

        public DeactivateBranchCommand(int branchId, int updatedByUserId)
        {
            BranchId = branchId;
            UpdatedByUserId = updatedByUserId;
        }
    }

    /// <summary>
    /// Comando para reactivar una sucursal
    /// </summary>
    public class ReactivateBranchCommand : IRequest<bool>
    {
        public int BranchId { get; set; }
        public int UpdatedByUserId { get; set; }

        public ReactivateBranchCommand(int branchId, int updatedByUserId)
        {
            BranchId = branchId;
            UpdatedByUserId = updatedByUserId;
        }
    }
}
