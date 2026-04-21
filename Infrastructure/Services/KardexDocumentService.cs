using Application.Abstractions.Documents;
using Application.Abstractions.Reports;
using Application.Core.Reports.Engine;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data;
using ClosedXML.Excel;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio para generar documentos del kardex de inventario
    /// </summary>
    public class KardexDocumentService : IKardexDocumentService
    {
        private readonly POSDbContext _context;
        private readonly IReportTemplateRepository _templateRepo;

        public KardexDocumentService(
            POSDbContext context,
            IReportTemplateRepository templateRepo)
        {
            _context      = context;
            _templateRepo = templateRepo;
        }

        /// <summary>
        /// Genera un reporte de kardex en formato Excel
        /// </summary>
        public async Task<byte[]> GenerateKardexExcelAsync(
            int? productId,
            int? warehouseId,
            string? movementType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // Obtener movimientos
            var movements = await GetKardexMovements(productId, warehouseId, movementType, fromDate, toDate);

            // Crear DataTable
            var dt = new DataTable("Kardex");
            dt.Columns.Add("Fecha/Hora", typeof(string));
            dt.Columns.Add("C�digo Movimiento", typeof(string));
            dt.Columns.Add("Tipo", typeof(string));
            dt.Columns.Add("Producto", typeof(string));
            dt.Columns.Add("Almac�n", typeof(string));
            dt.Columns.Add("Cantidad", typeof(decimal));
            dt.Columns.Add("Saldo Anterior", typeof(decimal));
            dt.Columns.Add("Saldo Nuevo", typeof(decimal));
            dt.Columns.Add("Costo Unit.", typeof(decimal));
            dt.Columns.Add("Total", typeof(decimal));
            dt.Columns.Add("Referencia", typeof(string));
            dt.Columns.Add("Usuario", typeof(string));
            dt.Columns.Add("Notas", typeof(string));

            foreach (var m in movements)
            {
                dt.Rows.Add(
                    m.MovementDate.ToString("dd/MM/yyyy HH:mm"),
                    m.MovementCode,
                    FormatMovementType(m.MovementType),
                    $"{m.ProductCode} - {m.ProductName}",
                    m.WarehouseName ?? "N/A",
                    m.Quantity,
                    m.StockBefore,
                    m.StockAfter,
                    m.UnitCost ?? 0,
                    m.TotalCost ?? 0,
                    m.PurchaseOrderReceivingCode ?? m.SaleCode ?? "-",
                    m.CreatedByUserName ?? "Sistema",
                    m.Notes ?? ""
                );
            }

            // Crear Excel con ClosedXML
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(dt, "Kardex");

            // Formatear encabezados
            var headerRange = worksheet.Range(1, 1, 1, dt.Columns.Count);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Ajustar anchos
            worksheet.Columns().AdjustToContents();

            // Aplicar formato a n�meros
            worksheet.Column(6).Style.NumberFormat.Format = "#,##0.0000"; // Cantidad
            worksheet.Column(7).Style.NumberFormat.Format = "#,##0.0000"; // Saldo anterior
            worksheet.Column(8).Style.NumberFormat.Format = "#,##0.0000"; // Saldo nuevo
            worksheet.Column(9).Style.NumberFormat.Format = "$#,##0.0000"; // Costo unit
            worksheet.Column(10).Style.NumberFormat.Format = "$#,##0.00"; // Total

            // Guardar a memoria
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Genera un reporte de kardex en formato PDF usando el layout legacy por defecto.
        /// </summary>
        public async Task<byte[]> GenerateKardexPdfAsync(
            int? productId,
            int? warehouseId,
            string? movementType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var movements = await GetKardexMovements(productId, warehouseId, movementType, fromDate, toDate);

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, movements, fromDate, toDate));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        #region Helper Methods

        private async Task<List<KardexMovement>> GetKardexMovements(
            int? productId,
            int? warehouseId,
            string? movementType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.InventoryMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Include(m => m.PurchaseOrderReceiving)
                .Include(m => m.Sale)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(m => m.ProductId == productId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(m => m.WarehouseId == warehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(movementType))
            {
                query = query.Where(m => m.MovementType.StartsWith(movementType));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.MovementDate <= toDateEnd);
            }

            return await query
                .OrderByDescending(m => m.MovementDate)
                .Select(m => new KardexMovement
                {
                    MovementCode = m.Code, // ? Correcto: Code
                    MovementDate = m.MovementDate,
                    MovementType = m.MovementType,
                    ProductCode = m.Product.code,
                    ProductName = m.Product.name,
                    WarehouseName = m.Warehouse != null ? m.Warehouse.Name : null,
                    Quantity = m.Quantity,
                    StockBefore = m.StockBefore,
                    StockAfter = m.StockAfter,
                    UnitCost = m.UnitCost,
                    TotalCost = m.TotalCost,
                    PurchaseOrderReceivingCode = m.PurchaseOrderReceiving != null ? m.PurchaseOrderReceiving.Code : null,
                    SaleCode = m.Sale != null ? m.Sale.Code : null,
                    CreatedByUserName = "Sistema", // Campo CreatedBy existe pero no lo inclu�mos
                    Notes = m.Notes
                })
                .ToListAsync();
        }

        private string FormatMovementType(string type)
        {
            return type switch
            {
                "IN/PURCHASE" => "Entrada - Compra",
                "IN/ADJUSTMENT" => "Entrada - Ajuste",
                "IN/TRANSFER" => "Entrada - Traspaso",
                "OUT/SALE" => "Salida - Venta",
                "OUT/ADJUSTMENT" => "Salida - Ajuste",
                "OUT/TRANSFER" => "Salida - Traspaso",
                _ => type
            };
        }

        #endregion

        #region PDF Composition

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("KARDEX DE INVENTARIO")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().Text("Historial de Movimientos")
                            .FontSize(12).FontColor(Colors.Grey.Darken2);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9);
                    });
                });

                column.Item().PaddingVertical(8).LineHorizontal(2).LineColor(Colors.Blue.Darken3);
            });
        }

        private void ComposeContent(IContainer container, List<KardexMovement> movements, DateTime? fromDate, DateTime? toDate)
        {
            container.Column(column =>
            {
                // Informaci�n del per�odo
                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Text($"Per�odo: {fromDate?.ToString("dd/MM/yyyy") ?? "Todos"} - {toDate?.ToString("dd/MM/yyyy") ?? "Hoy"}")
                        .FontSize(10).Bold();
                    row.RelativeItem().AlignRight().Text($"Total de movimientos: {movements.Count}")
                        .FontSize(10).Bold();
                });

                // Tabla
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80); // Fecha
                        columns.ConstantColumn(60); // C�digo
                        columns.RelativeColumn(2); // Tipo
                        columns.RelativeColumn(3); // Producto
                        columns.RelativeColumn(2); // Almac�n
                        columns.ConstantColumn(50); // Cantidad
                        columns.ConstantColumn(50); // Saldo
                        columns.ConstantColumn(55); // Costo
                        columns.ConstantColumn(60); // Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Fecha").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("C�digo").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Tipo").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Producto").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Almac�n").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Cant.").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Saldo").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Costo U.").FontColor(Colors.White).Bold().FontSize(7);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(3)
                            .Text("Total").FontColor(Colors.White).Bold().FontSize(7);
                    });

                    // Rows
                    foreach (var m in movements)
                    {
                        var bgColor = m.MovementType.StartsWith("IN") ? Colors.Green.Lighten4 : Colors.Red.Lighten4;

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(m.MovementDate.ToString("dd/MM/yy HH:mm")).FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(m.MovementCode).FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(FormatMovementType(m.MovementType)).FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text($"{m.ProductCode} - {m.ProductName}").FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(m.WarehouseName ?? "N/A").FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(m.Quantity.ToString("F2")).FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(m.StockAfter.ToString("F2")).FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(m.UnitCost.HasValue ? $"${m.UnitCost:F2}" : "-").FontSize(7);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3)
                            .Text(m.TotalCost.HasValue ? $"${m.TotalCost:F2}" : "-").FontSize(7);
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("P�gina ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" de ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        }

        #endregion

        // Clase auxiliar para mapear datos
        private class KardexMovement
        {
            public string MovementCode { get; set; } = string.Empty;
            public DateTime MovementDate { get; set; }
            public string MovementType { get; set; } = string.Empty;
            public string ProductCode { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public string? WarehouseName { get; set; }
            public decimal Quantity { get; set; }
            public decimal StockBefore { get; set; }
            public decimal StockAfter { get; set; }
            public decimal? UnitCost { get; set; }
            public decimal? TotalCost { get; set; }
            public string? PurchaseOrderReceivingCode { get; set; }
            public string? SaleCode { get; set; }
            public string? CreatedByUserName { get; set; }
            public string? Notes { get; set; }
        }
    }
}
