using Application.Abstractions.Quotations;
using Application.Core.Quotations.Queries;
using Application.DTOs.Quotations;
using Domain.Entities;
using MediatR;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Application.Core.Quotations.QueryHandlers
{
    public class GetQuotationByIdQueryHandler : IRequestHandler<GetQuotationByIdQuery, QuotationResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;

        public GetQuotationByIdQueryHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationResponseDto> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
        {
            var quotation = await _quotationRepository.GetByIdAsync(request.QuotationId)
                ?? throw new KeyNotFoundException($"Cotización con ID {request.QuotationId} no encontrada");

            return QuotationMapper.ToResponseDto(quotation);
        }
    }

    public class GetQuotationByCodeQueryHandler : IRequestHandler<GetQuotationByCodeQuery, QuotationResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;

        public GetQuotationByCodeQueryHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationResponseDto> Handle(GetQuotationByCodeQuery request, CancellationToken cancellationToken)
        {
            var quotation = await _quotationRepository.GetByCodeAsync(request.Code)
                ?? throw new KeyNotFoundException($"Cotización con código '{request.Code}' no encontrada");

            return QuotationMapper.ToResponseDto(quotation);
        }
    }

    public class GetQuotationsPagedQueryHandler : IRequestHandler<GetQuotationsPagedQuery, QuotationsPagedResponseDto>
    {
        private readonly IQuotationRepository _quotationRepository;

        public GetQuotationsPagedQueryHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<QuotationsPagedResponseDto> Handle(GetQuotationsPagedQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _quotationRepository.GetPagedAsync(
                page: request.Page,
                pageSize: request.PageSize,
                warehouseId: request.WarehouseId,
                customerId: request.CustomerId,
                userId: request.UserId,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                status: request.Status
            );

            var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

            return new QuotationsPagedResponseDto
            {
                Items = items.Select(QuotationMapper.ToSummaryDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PDF HANDLER
    // ─────────────────────────────────────────────────────────────────────────

    public class GetQuotationPdfQueryHandler : IRequestHandler<GetQuotationPdfQuery, byte[]>
    {
        private const string PrimaryColor   = "#1a3c6e";
        private const string PrimaryLight   = "#2a5298";
        private const string AccentColor    = "#e8f0fb";
        private const string SuccessColor   = "#2e7d32";
        private const string WarningColor   = "#e65100";
        private const string GrayLight      = "#f5f7fa";
        private const string GrayMid        = "#dde3ee";
        private const string TextDark       = "#1c2740";
        private const string TextMuted      = "#5b6472";

        private readonly IQuotationRepository _quotationRepository;

        public GetQuotationPdfQueryHandler(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<byte[]> Handle(GetQuotationPdfQuery request, CancellationToken cancellationToken)
        {
            var q = await _quotationRepository.GetByIdAsync(request.QuotationId)
                ?? throw new KeyNotFoundException($"Cotización con ID {request.QuotationId} no encontrada");

            var qrImageBytes = GenerateQrPng(q.Code);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0);
                    page.DefaultTextStyle(t => t.FontSize(9).FontFamily(Fonts.Arial).FontColor(TextDark));

                    page.Header().Element(c => RenderHeader(c, q));
                    page.Content().PaddingHorizontal(28).PaddingTop(16).Column(col =>
                    {
                        RenderInfoGrid(col, q);
                        col.Item().PaddingTop(14);
                        RenderProductsTable(col, q);
                        col.Item().PaddingTop(14);
                        RenderTotalsAndQr(col, q, qrImageBytes);
                        if (!string.IsNullOrWhiteSpace(q.Notes))
                        {
                            col.Item().PaddingTop(14);
                            RenderNotes(col, q.Notes);
                        }
                    });
                    page.Footer().Element(c => RenderFooter(c, q));
                });
            }).GeneratePdf();
        }

        // ─── Header ──────────────────────────────────────────────────────────

        private static void RenderHeader(IContainer container, Quotation q)
        {
            container.Background("#ffffff").BorderBottom(4).BorderColor(PrimaryColor).Padding(0).Column(col =>
            {
                col.Item().PaddingHorizontal(28).PaddingTop(22).PaddingBottom(18).Row(row =>
                {
                    // Company block
                    row.RelativeItem().Column(left =>
                    {
                        var companyName = q.Company?.LegalName ?? q.Company?.TradeName ?? "EasyPOS";
                        var tradeName   = q.Company?.TradeName ?? companyName;

                        var logoBytes = TryLoadLogo(q.Company?.LogoUrl);
                        if (logoBytes is not null)
                        {
                            left.Item()
                                .Width(130).Height(50)
                                .Background("#ffffff")
                                .Image(logoBytes).FitArea();
                            left.Item().PaddingTop(6).Text(tradeName)
                                .FontSize(8).FontColor(TextMuted).SemiBold();
                        }
                        else
                        {
                            left.Item().Text(companyName)
                                .FontSize(16).Bold().FontColor(PrimaryColor);
                            if (!string.IsNullOrWhiteSpace(q.Company?.TaxId))
                                left.Item().PaddingTop(3).Text($"RFC: {q.Company.TaxId}")
                                    .FontSize(8).FontColor(TextMuted);
                        }

                        if (!string.IsNullOrWhiteSpace(q.Company?.Phone))
                            left.Item().PaddingTop(4).Text($"Tel: {q.Company!.Phone}")
                                .FontSize(8).FontColor(TextMuted);
                        if (!string.IsNullOrWhiteSpace(q.Company?.Email))
                            left.Item().Text(q.Company!.Email)
                                .FontSize(8).FontColor(TextMuted);
                    });

                    row.Spacing(20);

                    // Quotation code block
                    row.ConstantItem(180).AlignRight().Column(right =>
                    {
                        right.Item().AlignRight().Text("COTIZACIÓN")
                            .FontSize(11).Bold().FontColor(TextMuted).LetterSpacing(0.1f);
                        right.Item().AlignRight().Text(q.Code)
                            .FontSize(28).Bold().FontColor(PrimaryColor);

                        right.Item().PaddingTop(6).AlignRight().Element(c =>
                            RenderStatusPill(c, q.Status));

                        right.Item().PaddingTop(8).AlignRight()
                            .Text($"Fecha: {q.QuotationDate:dd/MM/yyyy}")
                            .FontSize(8).FontColor(TextMuted);

                        if (q.ValidUntil.HasValue)
                        {
                            var expired = q.ValidUntil.Value < DateTime.UtcNow && q.Status == "Draft";
                            right.Item().AlignRight()
                                .Text($"Válida hasta: {q.ValidUntil.Value:dd/MM/yyyy}")
                                .FontSize(8)
                                .FontColor(expired ? "#e53935" : TextMuted);
                        }
                    });
                });
            });
        }

        private static void RenderStatusPill(IContainer container, string status)
        {
            var (label, bg) = status switch
            {
                "Draft"     => ("Vigente",    "#1565c0"),
                "Converted" => ("Convertida", SuccessColor),
                "Cancelled" => ("Cancelada",  "#c62828"),
                "Expired"   => ("Expirada",   WarningColor),
                _           => (status,       "#5b6472")
            };

            container.Background(bg)
                .PaddingHorizontal(10).PaddingVertical(4)
                .Text(label)
                .FontSize(9).Bold().FontColor(Colors.White);
        }

        // ─── Info Grid ───────────────────────────────────────────────────────

        private static void RenderInfoGrid(ColumnDescriptor col, Quotation q)
        {
            col.Item().Border(1).BorderColor(GrayMid).Background(GrayLight).Column(inner =>
            {
                inner.Item().Background(PrimaryColor)
                    .PaddingHorizontal(12).PaddingVertical(7)
                    .Text("INFORMACIÓN DE LA COTIZACIÓN")
                    .FontSize(8).Bold().FontColor(Colors.White).LetterSpacing(0.05f);

                inner.Item().Padding(14).Row(row =>
                {
                    // Left column
                    row.RelativeItem().Column(left =>
                    {
                        var fullName = q.Customer is not null
                            ? $"{q.Customer.Name} {q.Customer.LastName}".Trim()
                            : q.CustomerName ?? "Público general";

                        InfoRow(left, "Cliente", fullName);
                        if (!string.IsNullOrWhiteSpace(q.Customer?.TaxId))
                            InfoRow(left, "RFC", q.Customer.TaxId);
                        if (!string.IsNullOrWhiteSpace(q.Customer?.Email))
                            InfoRow(left, "Correo", q.Customer.Email);
                        if (!string.IsNullOrWhiteSpace(q.Customer?.Phone))
                            InfoRow(left, "Teléfono", q.Customer.Phone);
                    });

                    row.ConstantItem(1).Background(GrayMid);
                    row.Spacing(18);

                    // Right column
                    row.RelativeItem().Column(right =>
                    {
                        InfoRow(right, "Vendedor",  q.User?.Name ?? "—");
                        InfoRow(right, "Almacén",   q.Warehouse?.Name ?? "—");
                        InfoRow(right, "Sucursal",  q.Branch?.Name ?? q.Warehouse?.Branch?.Name ?? "—");
                        if (q.PriceList is not null)
                            InfoRow(right, "Lista de precios", q.PriceList.Name);
                        if (q.RequiresInvoice)
                            InfoRow(right, "Factura", "Sí");
                    });
                });
            });
        }

        private static void InfoRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().PaddingBottom(5).Row(r =>
            {
                r.ConstantItem(100).Text(label + ":").FontSize(8).FontColor(TextMuted).SemiBold();
                r.RelativeItem().Text(value).FontSize(8).FontColor(TextDark);
            });
        }

        // ─── Products Table ──────────────────────────────────────────────────

        private static void RenderProductsTable(ColumnDescriptor col, Quotation q)
        {
            col.Item().Border(1).BorderColor(GrayMid).Column(tbl =>
            {
                // Table header
                tbl.Item().Background(PrimaryColor)
                    .PaddingHorizontal(12).PaddingVertical(7)
                    .Text("DETALLE DE PRODUCTOS / SERVICIOS")
                    .FontSize(8).Bold().FontColor(Colors.White).LetterSpacing(0.05f);

                // Column headers
                tbl.Item().Background("#2a4a80").PaddingHorizontal(12).PaddingVertical(7).Row(h =>
                {
                    h.ConstantItem(56) .Text("Código")     .FontSize(8).Bold().FontColor(AccentColor);
                    h.RelativeItem()   .Text("Descripción").FontSize(8).Bold().FontColor(AccentColor);
                    h.ConstantItem(48) .AlignRight().Text("Cant.").FontSize(8).Bold().FontColor(AccentColor);
                    h.ConstantItem(72) .AlignRight().Text("P. Unitario").FontSize(8).Bold().FontColor(AccentColor);
                    h.ConstantItem(52) .AlignRight().Text("Desc. %").FontSize(8).Bold().FontColor(AccentColor);
                    h.ConstantItem(52) .AlignRight().Text("IVA").FontSize(8).Bold().FontColor(AccentColor);
                    h.ConstantItem(72) .AlignRight().Text("Total").FontSize(8).Bold().FontColor(AccentColor);
                });

                // Rows
                var details = q.Details.ToList();
                for (int i = 0; i < details.Count; i++)
                {
                    var d   = details[i];
                    var bg  = i % 2 == 0 ? "#ffffff" : "#f0f4fb";

                    tbl.Item().Background(bg).BorderBottom(1).BorderColor(GrayMid)
                        .PaddingHorizontal(12).PaddingVertical(6).Row(r =>
                        {
                            r.ConstantItem(56).Text(d.ProductCode).FontSize(8).FontColor(TextMuted);
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text(d.ProductName).FontSize(8);
                                if (!string.IsNullOrWhiteSpace(d.Notes))
                                    c.Item().Text(d.Notes).FontSize(7).FontColor(TextMuted).Italic();
                            });
                            r.ConstantItem(48).AlignRight()
                                .Text(d.Quantity.ToString("G29")).FontSize(8);
                            r.ConstantItem(72).AlignRight()
                                .Text(FormatCurrency(d.UnitPrice)).FontSize(8);
                            r.ConstantItem(52).AlignRight()
                                .Text(d.DiscountPercentage > 0
                                    ? $"{d.DiscountPercentage:0.##}%"
                                    : "—").FontSize(8).FontColor(TextMuted);
                            r.ConstantItem(52).AlignRight()
                                .Text(d.TaxAmount > 0
                                    ? FormatCurrency(d.TaxAmount)
                                    : "—").FontSize(8).FontColor(TextMuted);
                            r.ConstantItem(72).AlignRight()
                                .Text(FormatCurrency(d.Total)).FontSize(8).SemiBold();
                        });
                }

                if (!details.Any())
                {
                    tbl.Item().Padding(20).AlignCenter()
                        .Text("Sin productos registrados").FontSize(9).FontColor(TextMuted).Italic();
                }
            });
        }

        // ─── Totals + QR ─────────────────────────────────────────────────────

        private static void RenderTotalsAndQr(ColumnDescriptor col, Quotation q, byte[] qrImageBytes)
        {
            col.Item().Row(row =>
            {
                // QR side
                row.RelativeItem().Border(1).BorderColor(GrayMid)
                    .Background(AccentColor).Padding(16).Column(qrCol =>
                    {
                        qrCol.Item().AlignCenter().Text("ESCANEA PARA CONVERTIR EN VENTA")
                            .FontSize(7).Bold().FontColor(PrimaryColor).LetterSpacing(0.08f);

                        qrCol.Item().PaddingTop(10).AlignCenter()
                            .Border(3).BorderColor(PrimaryColor)
                            .Background(Colors.White)
                            .Padding(6)
                            .Width(130).Height(130)
                            .Image(qrImageBytes).FitArea();

                        qrCol.Item().PaddingTop(10).AlignCenter()
                            .Text(q.Code)
                            .FontSize(11).Bold().FontColor(PrimaryColor);

                        qrCol.Item().PaddingTop(4).AlignCenter()
                            .Text("Escanea el QR desde la app\npara procesar esta cotización")
                            .FontSize(7).FontColor(TextMuted).AlignCenter().Italic();
                    });

                row.Spacing(14);

                // Totals side
                row.RelativeItem().Border(1).BorderColor(GrayMid).Column(totals =>
                {
                    totals.Item().Background(PrimaryColor)
                        .PaddingHorizontal(12).PaddingVertical(7)
                        .Text("RESUMEN DE TOTALES")
                        .FontSize(8).Bold().FontColor(Colors.White).LetterSpacing(0.05f);

                    totals.Item().Padding(14).Column(inner =>
                    {
                        TotalRow(inner, "Subtotal",  FormatCurrency(q.SubTotal),  false);

                        if (q.DiscountAmount > 0)
                            TotalRow(inner, $"Descuento ({q.DiscountPercentage:0.##}%)",
                                $"- {FormatCurrency(q.DiscountAmount)}", false, "#c62828");

                        TotalRow(inner, "IVA (16%)", FormatCurrency(q.TaxAmount), false);

                        inner.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(PrimaryColor);

                        inner.Item().PaddingTop(8).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL")
                                .FontSize(13).Bold().FontColor(PrimaryColor);
                            r.RelativeItem().AlignRight()
                                .Text(FormatCurrency(q.Total))
                                .FontSize(13).Bold().FontColor(PrimaryColor);
                        });

                        if (q.RequiresInvoice)
                        {
                            inner.Item().PaddingTop(10)
                                .Background("#fff8e1")
                                .Border(1).BorderColor("#ffe082")
                                .Padding(8)
                                .Text("✓  Este pedido requiere factura (CFDI)")
                                .FontSize(8).FontColor("#e65100");
                        }
                    });
                });
            });
        }

        private static void TotalRow(ColumnDescriptor col, string label, string value,
            bool bold, string? color = null)
        {
            col.Item().PaddingBottom(6).Row(r =>
            {
                r.RelativeItem().Text(label)
                    .FontSize(9).FontColor(TextMuted);
                var txt = r.RelativeItem().AlignRight()
                    .Text(value).FontSize(9);
                if (bold) txt.Bold();
                if (color is not null) txt.FontColor(color);
            });
        }

        // ─── Notes ───────────────────────────────────────────────────────────

        private static void RenderNotes(ColumnDescriptor col, string notes)
        {
            col.Item().Border(1).BorderColor(GrayMid).Column(n =>
            {
                n.Item().Background(PrimaryColor)
                    .PaddingHorizontal(12).PaddingVertical(7)
                    .Text("NOTAS / OBSERVACIONES")
                    .FontSize(8).Bold().FontColor(Colors.White).LetterSpacing(0.05f);
                n.Item().Background(GrayLight).Padding(14)
                    .Text(notes).FontSize(9).FontColor(TextDark);
            });
        }

        // ─── Footer ──────────────────────────────────────────────────────────

        private static void RenderFooter(IContainer container, Quotation q)
        {
            container
                .BorderTop(1).BorderColor(GrayMid)
                .PaddingHorizontal(28).PaddingVertical(8)
                .Row(row =>
                {
                    row.RelativeItem().Text("EasyPOS — Documento generado automáticamente")
                        .FontSize(7).FontColor(TextMuted);
                    row.RelativeItem().AlignCenter().Text($"Cotización {q.Code}")
                        .FontSize(7).FontColor(TextMuted);
                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(7).FontColor(TextMuted));
                        x.Span($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static byte[] GenerateQrPng(string content)
        {
            var generator = new QRCodeGenerator();
            var data      = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
            var qr        = new PngByteQRCode(data);
            return qr.GetGraphic(10);
        }

        private static byte[]? TryLoadLogo(string? logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl)) return null;
            try
            {
                if (logoUrl.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    var idx = logoUrl.IndexOf(',');
                    return idx > -1 ? Convert.FromBase64String(logoUrl[(idx + 1)..]) : null;
                }
                if (System.IO.File.Exists(logoUrl))
                    return System.IO.File.ReadAllBytes(logoUrl);
                if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    return new System.Net.Http.HttpClient().GetByteArrayAsync(uri).GetAwaiter().GetResult();
            }
            catch { /* logo es opcional */ }
            return null;
        }

        private static string FormatCurrency(decimal value) =>
            value.ToString("C", new System.Globalization.CultureInfo("es-MX"));
    }
}
