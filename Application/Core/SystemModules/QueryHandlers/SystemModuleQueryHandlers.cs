using Application.Abstractions.Config;
using Application.Core.SystemModules.Queries;
using Application.DTOs.SystemModules;
using MediatR;

namespace Application.Core.SystemModules.QueryHandlers
{
    // ===================================================================
    // QUERY HANDLERS PARA MÓDULOS
    // ===================================================================

    public class GetAllModulesQueryHandler : IRequestHandler<GetAllModulesQuery, ModulesListResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public GetAllModulesQueryHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModulesListResponseDto> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllModulesAsync(request.IncludeInactive);
        }
    }

    public class GetModuleByIdQueryHandler : IRequestHandler<GetModuleByIdQuery, ModuleResponseDto?>
    {
        private readonly ISystemModuleRepository _repository;

        public GetModuleByIdQueryHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModuleResponseDto?> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetModuleByIdAsync(request.ModuleId);
        }
    }

    // ===================================================================
    // QUERY HANDLERS PARA SUBMÓDULOS
    // ===================================================================

    public class GetSubmodulesByModuleQueryHandler : IRequestHandler<GetSubmodulesByModuleQuery, SubmodulesListResponseDto>
    {
        private readonly ISystemModuleRepository _repository;

        public GetSubmodulesByModuleQueryHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<SubmodulesListResponseDto> Handle(GetSubmodulesByModuleQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetSubmodulesByModuleAsync(request.ModuleId, request.IncludeInactive);
        }
    }

    public class GetSubmoduleByIdQueryHandler : IRequestHandler<GetSubmoduleByIdQuery, SubmoduleResponseDto?>
    {
        private readonly ISystemModuleRepository _repository;

        public GetSubmoduleByIdQueryHandler(ISystemModuleRepository repository)
        {
            _repository = repository;
        }

        public async Task<SubmoduleResponseDto?> Handle(GetSubmoduleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetSubmoduleByIdAsync(request.SubmoduleId);
        }
    }
}
