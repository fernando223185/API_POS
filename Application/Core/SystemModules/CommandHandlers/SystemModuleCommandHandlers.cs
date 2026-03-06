using Application.Abstractions.Config;
using Application.Core.SystemModules.Commands;
using Application.DTOs.SystemModules;
using Domain.Entities;
using MediatR;

namespace Application.Core.SystemModules.CommandHandlers
{
    // ===================================================================
    // COMMAND HANDLERS PARA MÓDULOS
    // ===================================================================

    public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, ModuleCommandResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public CreateModuleCommandHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModuleCommandResponseDto> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var module = new SystemModule
                {
                    Name = request.ModuleData.Name,
                    Description = request.ModuleData.Description,
                    Path = request.ModuleData.Path,
                    Icon = request.ModuleData.Icon,
                    Order = request.ModuleData.Order,
                    IsActive = request.ModuleData.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _repository.CreateModuleAsync(module);

                Console.WriteLine($"? Módulo creado: {created.Name} (ID: {created.Id})");

                return new ModuleCommandResponseDto
                {
                    Message = "Módulo creado exitosamente",
                    Error = 0,
                    Data = new ModuleResponseDto
                    {
                        Id = created.Id,
                        Name = created.Name,
                        Description = created.Description,
                        Path = created.Path,
                        Icon = created.Icon,
                        Order = created.Order,
                        IsActive = created.IsActive,
                        CreatedAt = created.CreatedAt,
                        Submodules = new List<SubmoduleResponseDto>()
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear módulo: {ex.Message}");
                throw;
            }
        }
    }

    public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, ModuleCommandResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public UpdateModuleCommandHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModuleCommandResponseDto> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var module = new SystemModule
                {
                    Name = request.ModuleData.Name,
                    Description = request.ModuleData.Description,
                    Path = request.ModuleData.Path,
                    Icon = request.ModuleData.Icon,
                    Order = request.ModuleData.Order,
                    IsActive = request.ModuleData.IsActive
                };

                var updated = await _repository.UpdateModuleAsync(request.ModuleId, module);

                if (updated == null)
                {
                    return new ModuleCommandResponseDto
                    {
                        Message = "Módulo no encontrado",
                        Error = 1,
                        Data = null
                    };
                }

                Console.WriteLine($"? Módulo actualizado: {updated.Name} (ID: {updated.Id})");

                return new ModuleCommandResponseDto
                {
                    Message = "Módulo actualizado exitosamente",
                    Error = 0,
                    Data = new ModuleResponseDto
                    {
                        Id = updated.Id,
                        Name = updated.Name,
                        Description = updated.Description,
                        Path = updated.Path,
                        Icon = updated.Icon,
                        Order = updated.Order,
                        IsActive = updated.IsActive,
                        CreatedAt = updated.CreatedAt,
                        UpdatedAt = updated.UpdatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar módulo: {ex.Message}");
                throw;
            }
        }
    }

    public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, DeleteResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public DeleteModuleCommandHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<DeleteResponseDto> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _repository.DeleteModuleAsync(request.ModuleId);

                if (!deleted)
                {
                    return new DeleteResponseDto
                    {
                        Message = "Módulo no encontrado",
                        Error = 1,
                        DeletedId = 0
                    };
                }

                Console.WriteLine($"??? Módulo eliminado (soft): ID {request.ModuleId}");

                return new DeleteResponseDto
                {
                    Message = "Módulo eliminado exitosamente",
                    Error = 0,
                    DeletedId = request.ModuleId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar módulo: {ex.Message}");
                throw;
            }
        }
    }

    // ===================================================================
    // COMMAND HANDLERS PARA SUBMÓDULOS
    // ===================================================================

    public class CreateSubmoduleCommandHandler : IRequestHandler<CreateSubmoduleCommand, SubmoduleCommandResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public CreateSubmoduleCommandHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<SubmoduleCommandResponseDto> Handle(CreateSubmoduleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar que el módulo padre existe
                var moduleExists = await _repository.ModuleExistsAsync(request.SubmoduleData.ModuleId);
                if (!moduleExists)
                {
                    return new SubmoduleCommandResponseDto
                    {
                        Message = $"El módulo padre con ID {request.SubmoduleData.ModuleId} no existe",
                        Error = 1,
                        Data = null
                    };
                }

                var submodule = new SystemSubmodule
                {
                    ModuleId = request.SubmoduleData.ModuleId,
                    Name = request.SubmoduleData.Name,
                    Description = request.SubmoduleData.Description,
                    Path = request.SubmoduleData.Path,
                    Icon = request.SubmoduleData.Icon,
                    Order = request.SubmoduleData.Order,
                    Color = request.SubmoduleData.Color,
                    IsActive = request.SubmoduleData.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _repository.CreateSubmoduleAsync(submodule);

                Console.WriteLine($"? Submódulo creado: {created.Name} (ID: {created.Id})");

                return new SubmoduleCommandResponseDto
                {
                    Message = "Submódulo creado exitosamente",
                    Error = 0,
                    Data = new SubmoduleResponseDto
                    {
                        Id = created.Id,
                        ModuleId = created.ModuleId,
                        Name = created.Name,
                        Description = created.Description,
                        Path = created.Path,
                        Icon = created.Icon,
                        Order = created.Order,
                        Color = created.Color,
                        IsActive = created.IsActive,
                        CreatedAt = created.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al crear submódulo: {ex.Message}");
                throw;
            }
        }
    }

    public class UpdateSubmoduleCommandHandler : IRequestHandler<UpdateSubmoduleCommand, SubmoduleCommandResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public UpdateSubmoduleCommandHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<SubmoduleCommandResponseDto> Handle(UpdateSubmoduleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var submodule = new SystemSubmodule
                {
                    Name = request.SubmoduleData.Name,
                    Description = request.SubmoduleData.Description,
                    Path = request.SubmoduleData.Path,
                    Icon = request.SubmoduleData.Icon,
                    Order = request.SubmoduleData.Order,
                    Color = request.SubmoduleData.Color,
                    IsActive = request.SubmoduleData.IsActive
                };

                var updated = await _repository.UpdateSubmoduleAsync(request.SubmoduleId, submodule);

                if (updated == null)
                {
                    return new SubmoduleCommandResponseDto
                    {
                        Message = "Submódulo no encontrado",
                        Error = 1,
                        Data = null
                    };
                }

                Console.WriteLine($"? Submódulo actualizado: {updated.Name} (ID: {updated.Id})");

                return new SubmoduleCommandResponseDto
                {
                    Message = "Submódulo actualizado exitosamente",
                    Error = 0,
                    Data = new SubmoduleResponseDto
                    {
                        Id = updated.Id,
                        ModuleId = updated.ModuleId,
                        Name = updated.Name,
                        Description = updated.Description,
                        Path = updated.Path,
                        Icon = updated.Icon,
                        Order = updated.Order,
                        Color = updated.Color,
                        IsActive = updated.IsActive,
                        CreatedAt = updated.CreatedAt,
                        UpdatedAt = updated.UpdatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al actualizar submódulo: {ex.Message}");
                throw;
            }
        }
    }

    public class DeleteSubmoduleCommandHandler : IRequestHandler<DeleteSubmoduleCommand, DeleteResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public DeleteSubmoduleCommandHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<DeleteResponseDto> Handle(DeleteSubmoduleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _repository.DeleteSubmoduleAsync(request.SubmoduleId);

                if (!deleted)
                {
                    return new DeleteResponseDto
                    {
                        Message = "Submódulo no encontrado",
                        Error = 1,
                        DeletedId = 0
                    };
                }

                Console.WriteLine($"??? Submódulo eliminado (soft): ID {request.SubmoduleId}");

                return new DeleteResponseDto
                {
                    Message = "Submódulo eliminado exitosamente",
                    Error = 0,
                    DeletedId = request.SubmoduleId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al eliminar submódulo: {ex.Message}");
                throw;
            }
        }
    }
}
