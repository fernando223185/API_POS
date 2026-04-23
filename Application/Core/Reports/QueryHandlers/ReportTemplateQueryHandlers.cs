using Application.Abstractions.Billing;
using Application.Abstractions.CashierShifts;
using Application.Abstractions.Config;
using Application.Abstractions.Purchasing;
using Application.Abstractions.Quotations;
using Application.Abstractions.Reports;
using Application.Abstractions.Sales;
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
            HtmlTemplate = t.HtmlTemplate,
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

        internal static List<ReportSectionDefinition> EnsureInvoiceVisualSections(List<ReportSectionDefinition> sections)
        {
            if (!sections.Any())
                return sections;

            var timbreSection = sections.FirstOrDefault(s => s.Fields.Any(f => f.Field.Equals("uuid", StringComparison.OrdinalIgnoreCase)))
                ?? sections.LastOrDefault(s => s.Type == SectionType.Footer || s.Type == SectionType.Summary);

            if (timbreSection is null)
                return sections;

            var qrField = timbreSection.Fields.FirstOrDefault(f => f.Field.Equals("qrCode", StringComparison.OrdinalIgnoreCase));
            if (qrField is null)
            {
                timbreSection.Fields.Add(new ReportSectionField
                {
                    Field = "qrCode",
                    Label = "Código QR",
                    Format = FieldFormat.Image
                });
            }

            if (!timbreSection.Fields.Any(f => f.Field.Equals("uuid", StringComparison.OrdinalIgnoreCase)))
                timbreSection.Fields.Add(new ReportSectionField { Field = "uuid", Label = "UUID", Bold = true });

            if (!timbreSection.Fields.Any(f => f.Field.Equals("timbradoAt", StringComparison.OrdinalIgnoreCase)))
                timbreSection.Fields.Add(new ReportSectionField { Field = "timbradoAt", Label = "Fecha timbrado", Format = FieldFormat.DateTime, Inline = true });

            if (!timbreSection.Fields.Any(f => f.Field.Equals("noCertificadoCfdi", StringComparison.OrdinalIgnoreCase)))
                timbreSection.Fields.Add(new ReportSectionField { Field = "noCertificadoCfdi", Label = "No. Cert. CFDI" });

            if (!timbreSection.Fields.Any(f => f.Field.Equals("noCertificadoSat", StringComparison.OrdinalIgnoreCase)))
                timbreSection.Fields.Add(new ReportSectionField { Field = "noCertificadoSat", Label = "No. Cert. SAT", Inline = true });

            var legacySatSection = sections.FirstOrDefault(s => s.Title.Equals("Validación SAT", StringComparison.OrdinalIgnoreCase));
            if (legacySatSection is not null && !ReferenceEquals(legacySatSection, timbreSection))
            {
                foreach (var field in legacySatSection.Fields)
                {
                    if (!timbreSection.Fields.Any(existing => existing.Field.Equals(field.Field, StringComparison.OrdinalIgnoreCase)))
                        timbreSection.Fields.Add(field);
                }

                sections.Remove(legacySatSection);
            }

            qrField = timbreSection.Fields.FirstOrDefault(f => f.Field.Equals("qrCode", StringComparison.OrdinalIgnoreCase));
            if (qrField is not null)
            {
                timbreSection.Fields.Remove(qrField);
                timbreSection.Fields.Add(qrField);
            }

            return sections.OrderBy(s => s.Order).ToList();
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
    // GET ACTIVE TEMPLATE BY TYPE
    // ─────────────────────────────────────────────

    public class GetActiveTemplateByTypeQueryHandler : IRequestHandler<GetActiveTemplateByTypeQuery, ReportTemplateResponseDto?>
    {
        private readonly IReportTemplateRepository _repo;

        public GetActiveTemplateByTypeQueryHandler(IReportTemplateRepository repo)
            => _repo = repo;

        public async Task<ReportTemplateResponseDto?> Handle(GetActiveTemplateByTypeQuery request, CancellationToken cancellationToken)
        {
            var t = await _repo.GetDefaultByTypeAsync(request.ReportType, request.CompanyId);
            return t is null ? null : GetReportTemplateByIdQueryHandler.MapToDto(t);
        }
    }

    // ─────────────────────────────────────────────
    // GET PREVIEW DATA (schema + mock data)
    // ─────────────────────────────────────────────

    public class GetReportPreviewDataQueryHandler : IRequestHandler<GetReportPreviewDataQuery, ReportPreviewDataDto>
    {
        private readonly IReportTemplateRepository _repo;
        private readonly ICompanyRepository _companyRepo;

        public GetReportPreviewDataQueryHandler(IReportTemplateRepository repo, ICompanyRepository companyRepo)
        {
            _repo = repo;
            _companyRepo = companyRepo;
        }

        public async Task<ReportPreviewDataDto> Handle(GetReportPreviewDataQuery request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            var sections = GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson);

            var mockDataRow = await EnrichMockDataRowAsync(template.ReportType, ReportMockDataProvider.GetMockDataRow(template.ReportType));

            return new ReportPreviewDataDto
            {
                TemplateName  = template.Name,
                ReportType    = template.ReportType,
                Sections      = sections,
                MockDataRow   = mockDataRow,
                MockTableRows = ReportMockDataProvider.GetMockTableRows(template.ReportType),
            };
        }

        private async Task<Dictionary<string, string>> EnrichMockDataRowAsync(string reportType, Dictionary<string, string> row)
        {
            if (!reportType.Equals("Invoice", StringComparison.OrdinalIgnoreCase))
                return row;

            var company = await _companyRepo.GetMainCompanyAsync();
            if (company is null)
                return row;

            if (!string.IsNullOrWhiteSpace(company.LogoUrl))
                row["companyLogoUrl"] = company.LogoUrl;

            if (!string.IsNullOrWhiteSpace(company.TradeName))
                row["companyTradeName"] = company.TradeName;

            if (!string.IsNullOrWhiteSpace(company.LegalName))
                row["emisorNombre"] = company.LegalName;

            if (!string.IsNullOrWhiteSpace(company.TaxId))
                row["emisorRfc"] = company.TaxId;

            return row;
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
        private readonly ICompanyRepository _companyRepo;
        private readonly ITemplateRenderService _templateRender;
        private readonly IPdfRenderService _pdfRender;

        public GetReportTemplatePdfPreviewQueryHandler(
            IReportTemplateRepository repo,
            ICompanyRepository companyRepo,
            ITemplateRenderService templateRender,
            IPdfRenderService pdfRender)
        {
            _repo = repo;
            _companyRepo = companyRepo;
            _templateRender = templateRender;
            _pdfRender = pdfRender;
        }

        public async Task<byte[]> Handle(GetReportTemplatePdfPreviewQuery request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            var dataRow   = (await BuildPreviewDataRowAsync(template.ReportType))
                                .ToDictionary(k => k.Key, v => (object?)v.Value);
            var tableRows = ReportMockDataProvider.GetMockTableRows(template.ReportType)
                                .Select(r => r.ToDictionary(k => k.Key, v => (object?)v.Value))
                                .ToList();

            // Motor HTML/Playwright (nuevo)
            if (!string.IsNullOrEmpty(template.HtmlTemplate))
            {
                var html = _templateRender.Render(template.HtmlTemplate, dataRow, tableRows);
                return await _pdfRender.RenderHtmlToPdfAsync(html);
            }

            // Motor QuestPDF (legacy)
            var sections = GetReportTemplateByIdQueryHandler.EnsureInvoiceVisualSections(
                GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson));
            var title = $"{template.Name} — VISTA PREVIA ({DateTime.Now:dd/MM/yyyy})";
            return ReportPdfEngine.Generate(sections, new() { dataRow }, tableRows, title);
        }

        private async Task<Dictionary<string, string>> BuildPreviewDataRowAsync(string reportType)
        {
            var row = ReportMockDataProvider.GetMockDataRow(reportType);
            if (!reportType.Equals("Invoice", StringComparison.OrdinalIgnoreCase))
                return row;

            var company = await _companyRepo.GetMainCompanyAsync();
            if (company is null)
                return row;

            if (!string.IsNullOrWhiteSpace(company.LogoUrl))
                row["companyLogoUrl"] = company.LogoUrl;

            if (!string.IsNullOrWhiteSpace(company.TradeName))
                row["companyTradeName"] = company.TradeName;

            if (!string.IsNullOrWhiteSpace(company.LegalName))
                row["emisorNombre"] = company.LegalName;

            if (!string.IsNullOrWhiteSpace(company.TaxId))
                row["emisorRfc"] = company.TaxId;

            return row;
        }
    }

    // ─────────────────────────────────────────────
    // LIVE PREVIEW (sin guardar)
    // ─────────────────────────────────────────────

    public class GetLivePreviewHtmlQueryHandler : IRequestHandler<GetLivePreviewHtmlQuery, string>
    {
        private readonly ITemplateRenderService _templateRender;

        public GetLivePreviewHtmlQueryHandler(ITemplateRenderService templateRender)
            => _templateRender = templateRender;

        public Task<string> Handle(GetLivePreviewHtmlQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.HtmlTemplate))
                throw new ArgumentException("El HTML de la plantilla no puede estar vacío");

            var dataRow   = ReportMockDataProvider.GetMockDataRow(request.ReportType)
                                .ToDictionary(k => k.Key, v => (object?)v.Value);
            var tableRows = ReportMockDataProvider.GetMockTableRows(request.ReportType)
                                .Select(r => r.ToDictionary(k => k.Key, v => (object?)v.Value))
                                .ToList();

            var html = _templateRender.Render(request.HtmlTemplate, dataRow, tableRows);
            return Task.FromResult(html);
        }
    }

    // ─────────────────────────────────────────────
    // GET TEMPLATE HTML PREVIEW
    // ─────────────────────────────────────────────

    public class GetReportTemplateHtmlPreviewQueryHandler : IRequestHandler<GetReportTemplateHtmlPreviewQuery, string>
    {
        private readonly IReportTemplateRepository _repo;
        private readonly ICompanyRepository _companyRepo;
        private readonly ITemplateRenderService _templateRender;

        public GetReportTemplateHtmlPreviewQueryHandler(
            IReportTemplateRepository repo,
            ICompanyRepository companyRepo,
            ITemplateRenderService templateRender)
        {
            _repo = repo;
            _companyRepo = companyRepo;
            _templateRender = templateRender;
        }

        public async Task<string> Handle(GetReportTemplateHtmlPreviewQuery request, CancellationToken cancellationToken)
        {
            var template = await _repo.GetByIdAsync(request.TemplateId)
                ?? throw new KeyNotFoundException($"Plantilla {request.TemplateId} no encontrada");

            var dataRow   = (await BuildPreviewDataRowAsync(template.ReportType))
                                .ToDictionary(k => k.Key, v => (object?)v.Value);
            var tableRows = ReportMockDataProvider.GetMockTableRows(template.ReportType)
                                .Select(r => r.ToDictionary(k => k.Key, v => (object?)v.Value))
                                .ToList();

            // Motor HTML (nuevo) — devuelve el HTML con datos mock inyectados
            if (!string.IsNullOrEmpty(template.HtmlTemplate))
                return _templateRender.Render(template.HtmlTemplate, dataRow, tableRows);

            // Motor QuestPDF (legacy) — genera HTML estilo legacy para preview
            var sections = GetReportTemplateByIdQueryHandler.EnsureInvoiceVisualSections(
                GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson));
            var title = $"{template.Name} — VISTA PREVIA ({DateTime.Now:dd/MM/yyyy})";
            return ReportHtmlEngine.Generate(sections, new() { dataRow }, tableRows, title);
        }

        private async Task<Dictionary<string, string>> BuildPreviewDataRowAsync(string reportType)
        {
            var row = ReportMockDataProvider.GetMockDataRow(reportType);
            if (!reportType.Equals("Invoice", StringComparison.OrdinalIgnoreCase))
                return row;

            var company = await _companyRepo.GetMainCompanyAsync();
            if (company is null)
                return row;

            if (!string.IsNullOrWhiteSpace(company.LogoUrl))
                row["companyLogoUrl"] = company.LogoUrl;

            if (!string.IsNullOrWhiteSpace(company.TradeName))
                row["companyTradeName"] = company.TradeName;

            if (!string.IsNullOrWhiteSpace(company.LegalName))
                row["emisorNombre"] = company.LegalName;

            if (!string.IsNullOrWhiteSpace(company.TaxId))
                row["emisorRfc"] = company.TaxId;

            return row;
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
        private readonly Application.Abstractions.AccountsReceivable.IPaymentRepository _paymentRepo;
        private readonly ITemplateRenderService _templateRender;
        private readonly IPdfRenderService _pdfRender;

        public GenerateReportPdfQueryHandler(
            IReportTemplateRepository templateRepo,
            ISaleRepository saleRepo,
            IQuotationRepository quotationRepo,
            IPurchaseOrderRepository purchaseRepo,
            ICashierShiftRepository shiftRepo,
            IInvoiceRepository invoiceRepo,
            Application.Abstractions.AccountsReceivable.IPaymentRepository paymentRepo,
            ITemplateRenderService templateRender,
            IPdfRenderService pdfRender)
        {
            _templateRepo = templateRepo;
            _saleRepo = saleRepo;
            _quotationRepo = quotationRepo;
            _purchaseRepo = purchaseRepo;
            _shiftRepo = shiftRepo;
            _invoiceRepo = invoiceRepo;
            _paymentRepo = paymentRepo;
            _templateRender = templateRender;
            _pdfRender = pdfRender;
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

            // Determinar motor: HTML/Playwright o legacy QuestPDF
            bool useHtml = !string.IsNullOrEmpty(template.HtmlTemplate);

            if (useHtml)
            {
                var (dataRows, tableRows) = await BuildDataAsync(data);
                // Para múltiples documentos, renderizar cada uno y concatenar (una página por doc)
                // Simplificación: renderizar el primer doc (caso más común con 1 documento)
                var dataRow = dataRows.FirstOrDefault() ?? new();
                var html = _templateRender.Render(template.HtmlTemplate!, dataRow, tableRows);
                return await _pdfRender.RenderHtmlToPdfAsync(html);
            }

            // Motor legacy QuestPDF
            var sections = GetReportTemplateByIdQueryHandler.EnsureInvoiceVisualSections(
                GetReportTemplateByIdQueryHandler.DeserializeSections(template.SectionsJson));
            var reportTitle = $"{template.Name} — {DateTime.Now:dd/MM/yyyy}";

            return data.ReportType switch
            {
                "Sales" or "Delivery" => await GenerateSalesReportAsync(sections, data, reportTitle),
                "Quotation"           => await GenerateQuotationsReportAsync(sections, data, reportTitle),
                "Purchase"            => await GeneratePurchaseReportAsync(sections, data, reportTitle),
                "CashierShift"        => await GenerateCashierShiftReportAsync(sections, data, reportTitle),
                "Invoice"             => await GenerateInvoiceReportAsync(sections, data, reportTitle),
                "Payment"             => await GeneratePaymentReportAsync(sections, data, reportTitle),
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

            var inv = await _invoiceRepo.GetByIdAsync(data.DocumentIds[0])
                ?? throw new KeyNotFoundException($"Factura {data.DocumentIds[0]} no encontrada");

            var dataRow  = ReportDataProvider.FromInvoice(inv);
            var tableRows = ReportDataProvider.FromInvoiceDetails(inv);

            return ReportPdfEngine.Generate(sections, new() { dataRow }, tableRows, title);
        }

        /// <summary>
        /// Obtiene los datos (cabecera + filas) para el tipo de reporte solicitado.
        /// Usado por el motor HTML/Playwright.
        /// </summary>
        private async Task<(List<Dictionary<string, object?>> DataRows, List<Dictionary<string, object?>> TableRows)> BuildDataAsync(GenerateReportDto data)
        {
            return data.ReportType switch
            {
                "Sales" or "Delivery" => await BuildSalesDataAsync(data),
                "Quotation"           => await BuildQuotationDataAsync(data),
                "Purchase"            => await BuildPurchaseDataAsync(data),
                "CashierShift"        => await BuildCashierShiftDataAsync(data),
                "Invoice"             => await BuildInvoiceDataAsync(data),
                "Payment"             => await BuildPaymentDataAsync(data),
                _                     => throw new ArgumentException($"Tipo de reporte no soportado: {data.ReportType}")
            };
        }

        private async Task<(List<Dictionary<string, object?>>, List<Dictionary<string, object?>>)> BuildSalesDataAsync(GenerateReportDto data)
        {
            var dataRows  = new List<Dictionary<string, object?>>();
            var tableRows = new List<Dictionary<string, object?>>();
            foreach (var id in data.DocumentIds)
            {
                var sale = await _saleRepo.GetByIdAsync(id);
                if (sale is null) continue;
                dataRows.Add(ReportDataProvider.FromSale(sale));
                tableRows.AddRange(ReportDataProvider.FromSaleDetails(sale));
            }
            if (!dataRows.Any()) throw new InvalidOperationException("No se encontraron ventas");
            return (dataRows, tableRows);
        }

        private async Task<(List<Dictionary<string, object?>>, List<Dictionary<string, object?>>)> BuildQuotationDataAsync(GenerateReportDto data)
        {
            var dataRows  = new List<Dictionary<string, object?>>();
            var tableRows = new List<Dictionary<string, object?>>();
            foreach (var id in data.DocumentIds)
            {
                var q = await _quotationRepo.GetByIdAsync(id);
                if (q is null) continue;
                dataRows.Add(ReportDataProvider.FromQuotation(q));
                tableRows.AddRange(ReportDataProvider.FromQuotationDetails(q));
            }
            if (!dataRows.Any()) throw new InvalidOperationException("No se encontraron cotizaciones");
            return (dataRows, tableRows);
        }

        private async Task<(List<Dictionary<string, object?>>, List<Dictionary<string, object?>>)> BuildPurchaseDataAsync(GenerateReportDto data)
        {
            var dataRows  = new List<Dictionary<string, object?>>();
            var tableRows = new List<Dictionary<string, object?>>();
            foreach (var id in data.DocumentIds)
            {
                var po = await _purchaseRepo.GetByIdAsync(id);
                if (po is null) continue;
                dataRows.Add(ReportDataProvider.FromPurchaseOrder(po));
                tableRows.AddRange(ReportDataProvider.FromPurchaseOrderDetails(po));
            }
            if (!dataRows.Any()) throw new InvalidOperationException("No se encontraron órdenes de compra");
            return (dataRows, tableRows);
        }

        private async Task<(List<Dictionary<string, object?>>, List<Dictionary<string, object?>>)> BuildCashierShiftDataAsync(GenerateReportDto data)
        {
            if (!data.DocumentIds.Any()) throw new InvalidOperationException("Debe especificar el ID del turno");
            var shift = await _shiftRepo.GetByIdAsync(data.DocumentIds[0])
                ?? throw new KeyNotFoundException($"Turno {data.DocumentIds[0]} no encontrado");
            var sales    = new List<Sale>();
            var dataRow  = ReportDataProvider.FromCashierShift(shift, sales);
            var tableRows = ReportDataProvider.FromCashierShiftSales(sales);
            return (new() { dataRow }, tableRows);
        }

        private async Task<(List<Dictionary<string, object?>>, List<Dictionary<string, object?>>)> BuildInvoiceDataAsync(GenerateReportDto data)
        {
            if (!data.DocumentIds.Any()) throw new InvalidOperationException("Debe especificar el ID de la factura");
            var inv = await _invoiceRepo.GetByIdAsync(data.DocumentIds[0])
                ?? throw new KeyNotFoundException($"Factura {data.DocumentIds[0]} no encontrada");
            return (new() { ReportDataProvider.FromInvoice(inv) }, ReportDataProvider.FromInvoiceDetails(inv));
        }

        private async Task<(List<Dictionary<string, object?>>, List<Dictionary<string, object?>>)> BuildPaymentDataAsync(GenerateReportDto data)
        {
            if (!data.DocumentIds.Any()) throw new InvalidOperationException("Debe especificar el ID del complemento de pago");
            var payment = await _paymentRepo.GetByIdAsync(data.DocumentIds[0])
                ?? throw new KeyNotFoundException($"Complemento de pago {data.DocumentIds[0]} no encontrado");
            return (new() { ReportDataProvider.FromPayment(payment) }, ReportDataProvider.FromPaymentApplications(payment));
        }

        private async Task<byte[]> GeneratePaymentReportAsync(
            List<ReportSectionDefinition> sections, GenerateReportDto data, string title)
        {
            if (!data.DocumentIds.Any()) throw new InvalidOperationException("Debe especificar el ID del complemento de pago");
            var payment = await _paymentRepo.GetByIdAsync(data.DocumentIds[0])
                ?? throw new KeyNotFoundException($"Complemento de pago {data.DocumentIds[0]} no encontrado");
            var dataRow   = ReportDataProvider.FromPayment(payment);
            var tableRows = ReportDataProvider.FromPaymentApplications(payment);
            return ReportPdfEngine.Generate(sections, new() { dataRow }, tableRows, title);
        }
    }
}
