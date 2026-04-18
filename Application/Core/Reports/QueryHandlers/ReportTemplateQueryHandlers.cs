using Application.Abstractions.Billing;
using Application.Abstractions.CashierShifts;
using Application.Abstractions.Purchasing;
using Application.Abstractions.Quotations;
using Application.Abstractions.Reports;
using Application.Abstractions.Sales;
using Application.Core.Billing.Documents;
using Application.Core.Reports.Engine;
using Application.Core.Reports.Queries;
using Application.DTOs.Reports;
using Domain.Entities;
using MediatR;
using System.Text.Json;

namespace Application.Core.Reports.QueryHandlers
{
    // ─────────────────────────────────────────────
    // GET TEMPLATE BY ID
    // ─────────────────────────────────────────────

    public class GetReportTemplateByIdQueryHandler : IRequestHandler<GetReportTemplateByIdQuery, ReportTemplateResponseDto?>
    {
        private readonly IReportTemplateRepository _repo;

        public GetReportTemplateByIdQueryHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<ReportTemplateResponseDto?> Handle(GetReportTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var t = await _repo.GetByIdAsync(request.TemplateId);
            return t is null ? null : MapToDto(t);
        }

        internal static ReportTemplateResponseDto MapToDto(ReportTemplate t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            ReportType = t.ReportType,
            Description = t.Description,
            IsDefault = t.IsDefault,
            IsActive = t.IsActive,
            Sections = DeserializeSections(t.SectionsJson),
            CompanyId = t.CompanyId,
            CreatedByUserId = t.CreatedByUserId,
            CreatedByName = t.CreatedBy?.Name,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
        };

        internal static List<ReportSectionDefinition> DeserializeSections(string json)
        {
            try { return JsonSerializer.Deserialize<List<ReportSectionDefinition>>(json) ?? new(); }
            catch { return new(); }
        }
    }

    // ─────────────────────────────────────────────
    // GET TEMPLATES BY TYPE
    // ─────────────────────────────────────────────

    public class GetReportTemplatesByTypeQueryHandler : IRequestHandler<GetReportTemplatesByTypeQuery, List<ReportTemplateSummaryDto>>
    {
        private readonly IReportTemplateRepository _repo;

        public GetReportTemplatesByTypeQueryHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<List<ReportTemplateSummaryDto>> Handle(GetReportTemplatesByTypeQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetByTypeAsync(request.ReportType, request.CompanyId);
            return list.Select(t => new ReportTemplateSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                ReportType = t.ReportType,
                Description = t.Description,
                IsDefault = t.IsDefault,
                CreatedAt = t.CreatedAt,
            }).ToList();
        }
    }

    // ─────────────────────────────────────────────
    // GET ALL TEMPLATES (todos los tipos)
    // ─────────────────────────────────────────────

    public class GetAllReportTemplatesQueryHandler : IRequestHandler<GetAllReportTemplatesQuery, List<ReportTemplateSummaryDto>>
    {
        private readonly IReportTemplateRepository _repo;

        public GetAllReportTemplatesQueryHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<List<ReportTemplateSummaryDto>> Handle(GetAllReportTemplatesQuery request, CancellationToken cancellationToken)
        {
            var list = await _repo.GetAllAsync(request.CompanyId);
            return list.Select(t => new ReportTemplateSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                ReportType = t.ReportType,
                Description = t.Description,
                IsDefault = t.IsDefault,
                CreatedAt = t.CreatedAt,
            }).ToList();
        }
    }

    // ─────────────────────────────────────────────
    // GET PREVIEW DATA (schema + mock data)
    // ─────────────────────────────────────────────

    public class GetReportPreviewDataQueryHandler : IRequestHandler<GetReportPreviewDataQuery, ReportPreviewDataDto>
    {
        private readonly IReportTemplateRepository _repo;

        public GetReportPreviewDataQueryHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<ReportPreviewDataDto> Handle(GetReportPreviewDataQuery request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            var sections = GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson);

            return new ReportPreviewDataDto
            {
                TemplateName  = template.Name,
                ReportType    = template.ReportType,
                Sections      = sections,
                MockDataRow   = ReportMockDataProvider.GetMockDataRow(template.ReportType),
                MockTableRows = ReportMockDataProvider.GetMockTableRows(template.ReportType),
            };
        }
    }

    // ─────────────────────────────────────────────
    // GET FIELD CATALOG
    // ─────────────────────────────────────────────

    public class GetReportFieldCatalogQueryHandler : IRequestHandler<GetReportFieldCatalogQuery, ReportFieldCatalogDto>
    {
        public Task<ReportFieldCatalogDto> Handle(GetReportFieldCatalogQuery request, CancellationToken cancellationToken)
            => Task.FromResult(ReportFieldCatalog.GetCatalog(request.ReportType));
    }

    // ─────────────────────────────────────────────
    // GET PDF PREVIEW (real PDF with mock data)
    // ─────────────────────────────────────────────

    public class GetReportTemplatePdfPreviewQueryHandler : IRequestHandler<GetReportTemplatePdfPreviewQuery, byte[]>
    {
        private readonly IReportTemplateRepository _repo;

        public GetReportTemplatePdfPreviewQueryHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<byte[]> Handle(GetReportTemplatePdfPreviewQuery request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            // Las facturas usan su propio motor PDF (layout CFDI fijo), no el motor genérico de plantillas
            if (template.ReportType == "Invoice")
                return InvoicePdfDocument.Generate(ReportMockDataProvider.GetMockInvoice());

            var sections  = GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson);
            var dataRow   = ReportMockDataProvider.GetMockDataRow(template.ReportType)
                                .ToDictionary(k => k.Key, v => (object?)v.Value);
            var tableRows = ReportMockDataProvider.GetMockTableRows(template.ReportType)
                                .Select(r => r.ToDictionary(k => k.Key, v => (object?)v.Value))
                                .ToList();

            var title = $"{template.Name} — VISTA PREVIA ({DateTime.Now:dd/MM/yyyy})";
            return ReportPdfEngine.Generate(sections, new() { dataRow }, tableRows, title);
        }
    }

    // ─────────────────────────────────────────────
    // GET TEMPLATE HTML PREVIEW
    // ─────────────────────────────────────────────

    public class GetReportTemplateHtmlPreviewQueryHandler : IRequestHandler<GetReportTemplateHtmlPreviewQuery, string>
    {
        private readonly IReportTemplateRepository _repo;

        public GetReportTemplateHtmlPreviewQueryHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<string> Handle(GetReportTemplateHtmlPreviewQuery request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            // Las facturas usan el motor HTML de CFDI (layout fijo oficial)
            if (template.ReportType == "Invoice")
                return InvoiceHtmlDocument.GenerateHtml(ReportMockDataProvider.GetMockInvoice());

            var sections  = GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson);
            var dataRow   = ReportMockDataProvider.GetMockDataRow(template.ReportType)
                                .ToDictionary(k => k.Key, v => (object?)v.Value);
            var tableRows = ReportMockDataProvider.GetMockTableRows(template.ReportType)
                                .Select(r => r.ToDictionary(k => k.Key, v => (object?)v.Value))
                                .ToList();

            var title = $"{template.Name} — VISTA PREVIA ({DateTime.Now:dd/MM/yyyy})";
            return ReportHtmlEngine.Generate(sections, new() { dataRow }, tableRows, title);
        }
    }

    // ─────────────────────────────────────────────
    // GENERATE REPORT PDF
    // ─────────────────────────────────────────────

    public class GenerateReportPdfQueryHandler : IRequestHandler<GenerateReportPdfQuery, byte[]>
    {
        private readonly IReportTemplateRepository _templateRepo;
        private readonly ISaleRepository _saleRepo;
        private readonly IQuotationRepository _quotationRepo;
        private readonly IPurchaseOrderRepository _purchaseRepo;
        private readonly ICashierShiftRepository _shiftRepo;
        private readonly IInvoiceRepository _invoiceRepo;

        public GenerateReportPdfQueryHandler(
            IReportTemplateRepository templateRepo,
            ISaleRepository saleRepo,
            IQuotationRepository quotationRepo,
            IPurchaseOrderRepository purchaseRepo,
            ICashierShiftRepository shiftRepo,
            IInvoiceRepository invoiceRepo)
        {
            _templateRepo = templateRepo;
            _saleRepo = saleRepo;
            _quotationRepo = quotationRepo;
            _purchaseRepo = purchaseRepo;
            _shiftRepo = shiftRepo;
            _invoiceRepo = invoiceRepo;
        }

        public async Task<byte[]> Handle(GenerateReportPdfQuery request, CancellationToken cancellationToken)
        {
            var data = request.Data;

            // Obtener la plantilla (por ID o la default del tipo)
            ReportTemplate? template = data.TemplateId.HasValue
                ? await _templateRepo.GetByIdAsync(data.TemplateId.Value)
                : await _templateRepo.GetDefaultByTypeAsync(data.ReportType, data.CompanyId);

            if (template is null)
                throw new KeyNotFoundException(
                    $"No se encontró plantilla para el tipo '{data.ReportType}'. " +
                    "Cree una plantilla o marque una como predeterminada.");

            var sections = GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson);
            var reportTitle = $"{template.Name} — {DateTime.Now:dd/MM/yyyy}";

            return data.ReportType switch
            {
                "Sales" or "Delivery" => await GenerateSalesReportAsync(sections, data, reportTitle),
                "Quotation"           => await GenerateQuotationsReportAsync(sections, data, reportTitle),
                "Purchase"            => await GeneratePurchaseReportAsync(sections, data, reportTitle),
                "CashierShift"        => await GenerateCashierShiftReportAsync(sections, data, reportTitle),
                "Invoice"             => await GenerateInvoiceReportAsync(sections, data, reportTitle),
                _                     => throw new ArgumentException($"Tipo de reporte no soportado: {data.ReportType}")
            };
        }

        // ─────────────────────────────────────────────────
        // Generadores por tipo
        // ─────────────────────────────────────────────────

        private async Task<byte[]> GenerateSalesReportAsync(
            List<ReportSectionDefinition> sections, GenerateReportDto data, string title)
        {
            var dataRows = new List<Dictionary<string, object?>>();
            var tableRows = new List<Dictionary<string, object?>>();

            if (data.DocumentIds.Any())
            {
                foreach (var id in data.DocumentIds)
                {
                    var sale = await _saleRepo.GetByIdAsync(id);
                    if (sale is null) continue;
                    dataRows.Add(ReportDataProvider.FromSale(sale));
                    tableRows.AddRange(ReportDataProvider.FromSaleDetails(sale));
                }
            }

            if (!dataRows.Any())
                throw new InvalidOperationException("No se encontraron ventas para generar el reporte");

            return ReportPdfEngine.Generate(sections, dataRows, tableRows, title);
        }

        private async Task<byte[]> GenerateQuotationsReportAsync(
            List<ReportSectionDefinition> sections, GenerateReportDto data, string title)
        {
            var dataRows = new List<Dictionary<string, object?>>();
            var tableRows = new List<Dictionary<string, object?>>();

            foreach (var id in data.DocumentIds)
            {
                var q = await _quotationRepo.GetByIdAsync(id);
                if (q is null) continue;
                dataRows.Add(ReportDataProvider.FromQuotation(q));
                tableRows.AddRange(ReportDataProvider.FromQuotationDetails(q));
            }

            if (!dataRows.Any())
                throw new InvalidOperationException("No se encontraron cotizaciones para generar el reporte");

            return ReportPdfEngine.Generate(sections, dataRows, tableRows, title);
        }

        private async Task<byte[]> GeneratePurchaseReportAsync(
            List<ReportSectionDefinition> sections, GenerateReportDto data, string title)
        {
            var dataRows = new List<Dictionary<string, object?>>();
            var tableRows = new List<Dictionary<string, object?>>();

            foreach (var id in data.DocumentIds)
            {
                var po = await _purchaseRepo.GetByIdAsync(id);
                if (po is null) continue;
                dataRows.Add(ReportDataProvider.FromPurchaseOrder(po));
                tableRows.AddRange(ReportDataProvider.FromPurchaseOrderDetails(po));
            }

            if (!dataRows.Any())
                throw new InvalidOperationException("No se encontraron órdenes de compra para generar el reporte");

            return ReportPdfEngine.Generate(sections, dataRows, tableRows, title);
        }

        private async Task<byte[]> GenerateCashierShiftReportAsync(
            List<ReportSectionDefinition> sections, GenerateReportDto data, string title)
        {
            if (!data.DocumentIds.Any())
                throw new InvalidOperationException("Debe especificar el ID del turno de cajero");

            var shift = await _shiftRepo.GetByIdAsync(data.DocumentIds[0])
                ?? throw new KeyNotFoundException($"Turno {data.DocumentIds[0]} no encontrado");

            // Obtener las ventas del turno
            var sales = new List<Sale>();
            // El turno tiene ventas asociadas; obtener via SaleRepository si están disponibles
            // Como fallback usamos colección vacía y los totales del shift
            var dataRow = ReportDataProvider.FromCashierShift(shift, sales);
            var tableRows = ReportDataProvider.FromCashierShiftSales(sales);

            return ReportPdfEngine.Generate(sections, new() { dataRow }, tableRows, title);
        }

        private async Task<byte[]> GenerateInvoiceReportAsync(
            List<ReportSectionDefinition> sections, GenerateReportDto data, string title)
        {
            if (!data.DocumentIds.Any())
                throw new InvalidOperationException("Debe especificar el ID de la factura");

            // Las facturas usan InvoicePdfDocument para respetar el layout CFDI 4.0 oficial
            var inv = await _invoiceRepo.GetByIdAsync(data.DocumentIds[0])
                ?? throw new KeyNotFoundException($"Factura {data.DocumentIds[0]} no encontrada");

            return InvoicePdfDocument.Generate(inv);
        }
    }
}
