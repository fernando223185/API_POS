using Application.Abstractions.Backup;
using Application.DTOs.Backup;
using MediatR;

namespace Application.Core.Backup
{
    // ─── Commands ───────────────────────────────────────────────
    public record CreateBackupCommand(string? Notes, int? UserId) : IRequest<BackupRecordDto>;
    public record DeleteBackupCommand(int Id) : IRequest;

    // ─── Queries ─────────────────────────────────────────────────
    public record GetAllBackupsQuery() : IRequest<List<BackupRecordDto>>;
    public record GetBackupByIdQuery(int Id) : IRequest<BackupRecordDto?>;
    public record GetBackupDownloadUrlQuery(int Id) : IRequest<BackupDownloadDto>;

    // ─── Handlers ────────────────────────────────────────────────
    public class CreateBackupHandler : IRequestHandler<CreateBackupCommand, BackupRecordDto>
    {
        private readonly IBackupService _backupService;
        public CreateBackupHandler(IBackupService backupService) => _backupService = backupService;

        public Task<BackupRecordDto> Handle(CreateBackupCommand request, CancellationToken cancellationToken)
            => _backupService.CreateBackupAsync(request.Notes, request.UserId, cancellationToken);
    }

    public class DeleteBackupHandler : IRequestHandler<DeleteBackupCommand>
    {
        private readonly IBackupService _backupService;
        public DeleteBackupHandler(IBackupService backupService) => _backupService = backupService;

        public Task Handle(DeleteBackupCommand request, CancellationToken cancellationToken)
            => _backupService.DeleteAsync(request.Id, cancellationToken);
    }

    public class GetAllBackupsHandler : IRequestHandler<GetAllBackupsQuery, List<BackupRecordDto>>
    {
        private readonly IBackupService _backupService;
        public GetAllBackupsHandler(IBackupService backupService) => _backupService = backupService;

        public Task<List<BackupRecordDto>> Handle(GetAllBackupsQuery request, CancellationToken cancellationToken)
            => _backupService.GetAllAsync(cancellationToken);
    }

    public class GetBackupByIdHandler : IRequestHandler<GetBackupByIdQuery, BackupRecordDto?>
    {
        private readonly IBackupService _backupService;
        public GetBackupByIdHandler(IBackupService backupService) => _backupService = backupService;

        public Task<BackupRecordDto?> Handle(GetBackupByIdQuery request, CancellationToken cancellationToken)
            => _backupService.GetByIdAsync(request.Id, cancellationToken);
    }

    public class GetBackupDownloadUrlHandler : IRequestHandler<GetBackupDownloadUrlQuery, BackupDownloadDto>
    {
        private readonly IBackupService _backupService;
        public GetBackupDownloadUrlHandler(IBackupService backupService) => _backupService = backupService;

        public Task<BackupDownloadDto> Handle(GetBackupDownloadUrlQuery request, CancellationToken cancellationToken)
            => _backupService.GetDownloadUrlAsync(request.Id, cancellationToken);
    }
}
