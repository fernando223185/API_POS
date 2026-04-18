using Application.Abstractions.Reports;
using Application.Core.Reports.Commands;
using Application.DTOs.Reports;
using Domain.Entities;
using MediatR;
using System.Text.Json;

namespace Application.Core.Reports.CommandHandlers
{
    public class CreateReportTemplateCommandHandler : IRequestHandler<CreateReportTemplateCommand, ReportTemplateResponseDto>
    {
        private readonly IReportTemplateRepository _repo;

        public CreateReportTemplateCommandHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<ReportTemplateResponseDto> Handle(CreateReportTemplateCommand request, CancellationToken cancellationToken)
        {
            // Validar que el tipo de reporte sea conocido
            _ = Engine.ReportFieldCatalog.GetCatalog(request.Data.ReportType);

            if (request.Data.IsDefault)
                await _repo.ClearDefaultForTypeAsync(request.Data.ReportType, request.CompanyId);

            var template = new ReportTemplate
            {
                Name = request.Data.Name,
                ReportType = request.Data.ReportType,
                Description = request.Data.Description,
                IsDefault = request.Data.IsDefault,
                IsActive = true,
                SectionsJson = JsonSerializer.Serialize(request.Data.Sections),
                CompanyId = request.CompanyId,
                CreatedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
            };

            var created = await _repo.CreateAsync(template);
            return MapToDto(created, request.Data.Sections);
        }

        private static ReportTemplateResponseDto MapToDto(ReportTemplate t, List<ReportSectionDefinition> sections) => new()
        {
            Id = t.Id,
            Name = t.Name,
            ReportType = t.ReportType,
            Description = t.Description,
            IsDefault = t.IsDefault,
            IsActive = t.IsActive,
            Sections = sections,
            CompanyId = t.CompanyId,
            CreatedByUserId = t.CreatedByUserId,
            CreatedAt = t.CreatedAt,
        };
    }

    public class UpdateReportTemplateCommandHandler : IRequestHandler<UpdateReportTemplateCommand, ReportTemplateResponseDto>
    {
        private readonly IReportTemplateRepository _repo;

        public UpdateReportTemplateCommandHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<ReportTemplateResponseDto> Handle(UpdateReportTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            if (request.Data.IsDefault && !template.IsDefault)
                await _repo.ClearDefaultForTypeAsync(template.ReportType, template.CompanyId);

            template.Name = request.Data.Name;
            template.Description = request.Data.Description;
            template.IsDefault = request.Data.IsDefault;
            template.SectionsJson = JsonSerializer.Serialize(request.Data.Sections);
            template.UpdatedAt = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(template);
            return new ReportTemplateResponseDto
            {
                Id = updated.Id,
                Name = updated.Name,
                ReportType = updated.ReportType,
                Description = updated.Description,
                IsDefault = updated.IsDefault,
                IsActive = updated.IsActive,
                Sections = request.Data.Sections,
                CompanyId = updated.CompanyId,
                CreatedByUserId = updated.CreatedByUserId,
                CreatedAt = updated.CreatedAt,
                UpdatedAt = updated.UpdatedAt,
            };
        }
    }

    public class DeleteReportTemplateCommandHandler : IRequestHandler<DeleteReportTemplateCommand, bool>
    {
        private readonly IReportTemplateRepository _repo;

        public DeleteReportTemplateCommandHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<bool> Handle(DeleteReportTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            if (template.IsDefault)
                throw new InvalidOperationException("No se puede eliminar la plantilla predeterminada");

            await _repo.DeleteAsync(request.TemplateId);
            return true;
        }
    }

    public class SetDefaultTemplateCommandHandler : IRequestHandler<SetDefaultTemplateCommand, bool>
    {
        private readonly IReportTemplateRepository _repo;

        public SetDefaultTemplateCommandHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<bool> Handle(SetDefaultTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            await _repo.ClearDefaultForTypeAsync(template.ReportType, template.CompanyId);

            template.IsDefault = true;
            template.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(template);
            return true;
        }
    }
}
