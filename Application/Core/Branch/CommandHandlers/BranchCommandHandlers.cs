using Application.Abstractions.Config;
using Application.Core.Branch.Commands;
using Application.DTOs.Branch;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Core.Branch.CommandHandlers
{
    /// <summary>
    /// Handler para crear sucursal
    /// </summary>
    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, BranchResponseDto>
    {
        private readonly IBranchRepository _branchRepository;

        public CreateBranchCommandHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<BranchResponseDto> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            // Generar código automático
            var code = await _branchRepository.GenerateNextCodeAsync();

            var branch = new Domain.Entities.Branch
            {
                Code = code,
                Name = request.BranchData.Name,
                Description = request.BranchData.Description,
                Address = request.BranchData.Address,
                City = request.BranchData.City,
                State = request.BranchData.State,
                ZipCode = request.BranchData.ZipCode,
                Country = request.BranchData.Country,
                Phone = request.BranchData.Phone,
                Email = request.BranchData.Email,
                ManagerName = request.BranchData.ManagerName,
                IsMainBranch = request.BranchData.IsMainBranch,
                OpeningDate = request.BranchData.OpeningDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            var createdBranch = await _branchRepository.CreateAsync(branch);

            return new BranchResponseDto
            {
                Id = createdBranch.Id,
                Code = createdBranch.Code,
                Name = createdBranch.Name,
                Description = createdBranch.Description,
                Address = createdBranch.Address,
                City = createdBranch.City,
                State = createdBranch.State,
                ZipCode = createdBranch.ZipCode,
                Country = createdBranch.Country,
                Phone = createdBranch.Phone,
                Email = createdBranch.Email,
                ManagerName = createdBranch.ManagerName,
                IsMainBranch = createdBranch.IsMainBranch,
                IsActive = createdBranch.IsActive,
                OpeningDate = createdBranch.OpeningDate,
                CreatedAt = createdBranch.CreatedAt
            };
        }
    }

    /// <summary>
    /// Handler para actualizar sucursal
    /// </summary>
    public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, BranchResponseDto>
    {
        private readonly IBranchRepository _branchRepository;

        public UpdateBranchCommandHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<BranchResponseDto> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
            {
                throw new KeyNotFoundException($"Sucursal con ID {request.BranchId} no encontrada");
            }

            // Actualizar campos
            branch.Name = request.BranchData.Name;
            branch.Description = request.BranchData.Description;
            branch.Address = request.BranchData.Address;
            branch.City = request.BranchData.City;
            branch.State = request.BranchData.State;
            branch.ZipCode = request.BranchData.ZipCode;
            branch.Country = request.BranchData.Country;
            branch.Phone = request.BranchData.Phone;
            branch.Email = request.BranchData.Email;
            branch.ManagerName = request.BranchData.ManagerName;
            branch.IsMainBranch = request.BranchData.IsMainBranch;
            branch.OpeningDate = request.BranchData.OpeningDate;
            branch.IsActive = request.BranchData.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;
            branch.UpdatedByUserId = request.UpdatedByUserId;

            await _branchRepository.UpdateAsync(branch);

            return new BranchResponseDto
            {
                Id = branch.Id,
                Code = branch.Code,
                Name = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                City = branch.City,
                State = branch.State,
                ZipCode = branch.ZipCode,
                Country = branch.Country,
                Phone = branch.Phone,
                Email = branch.Email,
                ManagerName = branch.ManagerName,
                IsMainBranch = branch.IsMainBranch,
                IsActive = branch.IsActive,
                OpeningDate = branch.OpeningDate,
                CreatedAt = branch.CreatedAt,
                UpdatedAt = branch.UpdatedAt
            };
        }
    }

    /// <summary>
    /// Handler para desactivar sucursal
    /// </summary>
    public class DeactivateBranchCommandHandler : IRequestHandler<DeactivateBranchCommand, bool>
    {
        private readonly IBranchRepository _branchRepository;

        public DeactivateBranchCommandHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<bool> Handle(DeactivateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
            {
                return false;
            }

            branch.IsActive = false;
            branch.UpdatedAt = DateTime.UtcNow;
            branch.UpdatedByUserId = request.UpdatedByUserId;

            await _branchRepository.UpdateAsync(branch);
            return true;
        }
    }

    /// <summary>
    /// Handler para reactivar sucursal
    /// </summary>
    public class ReactivateBranchCommandHandler : IRequestHandler<ReactivateBranchCommand, bool>
    {
        private readonly IBranchRepository _branchRepository;

        public ReactivateBranchCommandHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<bool> Handle(ReactivateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
            {
                return false;
            }

            branch.IsActive = true;
            branch.UpdatedAt = DateTime.UtcNow;
            branch.UpdatedByUserId = request.UpdatedByUserId;

            await _branchRepository.UpdateAsync(branch);
            return true;
        }
    }
}
