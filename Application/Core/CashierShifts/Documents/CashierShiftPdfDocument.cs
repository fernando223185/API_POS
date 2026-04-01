using Application.DTOs.CashierShift;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Application.Core.CashierShifts.Documents
{
    /// <summary>
    /// Genera el PDF del corte de caja (cierre de turno) usando QuestPDF
    /// </summary>
    public static class CashierShiftPdfDocument
    {
        private static readonly string PrimaryColor = "#1a3c6e";
        private static readonly string SuccessColor = "#28a745";
        private static readonly string WarningColor = "#ffc107";
        private static readonly string DangerColor = "#dc3545";
        private static readonly string LightGray = "#f5f5f5";
        private static readonly string BorderColor = "#cccccc";

        public static byte[] Generate(ShiftReportDto report)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("CORTE DE CAJA")
                            .Bold()
                            .FontSize(18)
                            .FontColor(PrimaryColor)
                            .AlignCenter();
                        
                        col.Item().PaddingTop(5).Text($"Turno: {report.Shift.Code}")
                            .FontSize(12)
                            .Bold()
                            .AlignCenter();
                        
                        col.Item().LineHorizontal(1).LineColor(PrimaryColor);
                    });

                    page.Content().Column(col =>
                    {
                        // ── INFORMACIÓN GENERAL ──────────────────────────────────────
                        col.Item().PaddingTop(10).Background(LightGray).Padding(8).Column(c =>
                        {
                            c.Item().Text("INFORMACIÓN DEL TURNO").Bold().FontColor(PrimaryColor).FontSize(11);
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.RelativeItem().Column(left =>
                                {
                                    left.Item().Text(t =>
                                    {
                                        t.Span("Cajero: ").SemiBold();
                                        t.Span(report.Shift.CashierName);
                                    });
                                    left.Item().Text(t =>
                                    {
                                        t.Span("Sucursal: ").SemiBold();
                                        t.Span(report.Shift.BranchName);
                                    });
                                    left.Item().Text(t =>
                                    {
                                        t.Span("Almacén: ").SemiBold();
                                        t.Span(report.Shift.WarehouseName);
                                    });
                                });

                                r.RelativeItem().Column(right =>
                                {
                                    right.Item().Text(t =>
                                    {
                                        t.Span("Apertura: ").SemiBold();
                                        t.Span(report.Shift.OpeningDate.ToString("dd/MM/yyyy HH:mm"));
                                    });
                                    right.Item().Text(t =>
                                    {
                                        t.Span("Cierre: ").SemiBold();
                                        t.Span(report.Shift.ClosingDate?.ToString("dd/MM/yyyy HH:mm") ?? "N/A");
                                    });
                                    right.Item().Text(t =>
                                    {
                                        t.Span("Duración: ").SemiBold();
                                        var duration = report.Shift.Duration;
                                        if (duration.HasValue)
                                        {
                                            t.Span($"{(int)duration.Value.TotalHours}h {duration.Value.Minutes}m");
                                        }
                                        else
                                        {
                                            t.Span("N/A");
                                        }
                                    });
                                });
                            });
                        });

                        col.Item().PaddingTop(12);

                        // ── RESUMEN DE VENTAS ────────────────────────────────────────
                        col.Item().Text("RESUMEN DE VENTAS").Bold().FontColor(PrimaryColor).FontSize(11);
                        col.Item().PaddingTop(4).Border(1).BorderColor(BorderColor).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.ConstantColumn(80);
                            });

                            static IContainer CellStyle(IContainer c) => c.Padding(6);

                            table.Cell().Element(CellStyle).Text("Total de ventas").SemiBold();
                            table.Cell().Element(CellStyle).AlignRight().Text(report.Shift.TotalSales.ToString());

                            table.Cell().Element(CellStyle).Background(LightGray).Text("Ventas canceladas").SemiBold();
                            table.Cell().Element(CellStyle).Background(LightGray).AlignRight().Text(report.Shift.CancelledSales.ToString());

                            table.Cell().Element(CellStyle).Text("Monto total de ventas").SemiBold();
                            table.Cell().Element(CellStyle).AlignRight().Text($"${report.Shift.TotalSalesAmount:N2}");

                            table.Cell().Element(CellStyle).Background(LightGray).Text("Monto cancelado").SemiBold();
                            table.Cell().Element(CellStyle).Background(LightGray).AlignRight().Text($"${report.Shift.CancelledSalesAmount:N2}");
                        });

                        col.Item().PaddingTop(12);

                        // ── DESGLOSE POR FORMA DE PAGO ──────────────────────────────
                        col.Item().Text("DESGLOSE POR FORMA DE PAGO").Bold().FontColor(PrimaryColor).FontSize(11);
                        col.Item().PaddingTop(4).Border(1).BorderColor(BorderColor).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.ConstantColumn(60);
                                cols.ConstantColumn(80);
                                cols.ConstantColumn(60);
                            });

                            // Header
                            static IContainer HeaderCell(IContainer c) =>
                                c.Background(PrimaryColor).Padding(6);

                            table.Header(h =>
                            {
                                h.Cell().Element(HeaderCell).Text("Forma de Pago").FontColor(Colors.White).Bold();
                                h.Cell().Element(HeaderCell).AlignCenter().Text("Cantidad").FontColor(Colors.White).Bold();
                                h.Cell().Element(HeaderCell).AlignRight().Text("Monto").FontColor(Colors.White).Bold();
                                h.Cell().Element(HeaderCell).AlignRight().Text("% del Total").FontColor(Colors.White).Bold();
                            });

                            // Data
                            bool zebra = false;
                            foreach (var pm in report.PaymentMethodSummary)
                            {
                                string bg = zebra ? "#ffffff" : LightGray;
                                zebra = !zebra;

                                static IContainer DataCell(IContainer c, string bg) =>
                                    c.Background(bg).Padding(6);

                                table.Cell().Element(c => DataCell(c, bg)).Text(pm.PaymentMethod).SemiBold();
                                table.Cell().Element(c => DataCell(c, bg)).AlignCenter().Text(pm.Count.ToString());
                                table.Cell().Element(c => DataCell(c, bg)).AlignRight().Text($"${pm.Amount:N2}");
                                table.Cell().Element(c => DataCell(c, bg)).AlignRight().Text($"{pm.Percentage:N1}%");
                            }
                        });

                        col.Item().PaddingTop(12);

                        // ── FLUJO DE EFECTIVO ────────────────────────────────────────
                        col.Item().Text("FLUJO DE EFECTIVO").Bold().FontColor(PrimaryColor).FontSize(11);
                        col.Item().PaddingTop(4).Border(1).BorderColor(BorderColor).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.ConstantColumn(100);
                            });

                            static IContainer CellStyle(IContainer c) => c.Padding(6);

                            table.Cell().Element(CellStyle).Text("Efectivo inicial").SemiBold();
                            table.Cell().Element(CellStyle).AlignRight().Text($"${report.CashFlow.InitialCash:N2}");

                            table.Cell().Element(CellStyle).Background(LightGray).Text("Ventas en efectivo (+)").SemiBold();
                            table.Cell().Element(CellStyle).Background(LightGray).AlignRight().Text($"${report.CashFlow.CashSalesIn:N2}");

                            table.Cell().Element(CellStyle).Text("Depósitos de efectivo (+)").SemiBold();
                            table.Cell().Element(CellStyle).AlignRight().Text($"${report.CashFlow.CashDepositsIn:N2}");

                            table.Cell().Element(CellStyle).Background(LightGray).Text("Retiros de efectivo (-)").SemiBold().FontColor(DangerColor);
                            table.Cell().Element(CellStyle).Background(LightGray).AlignRight().Text($"${report.CashFlow.CashWithdrawalsOut:N2}").FontColor(DangerColor);

                            table.Cell().Element(CellStyle).Text("Efectivo esperado").SemiBold().FontSize(10);
                            table.Cell().Element(CellStyle).AlignRight().Text($"${report.CashFlow.ExpectedCash:N2}").SemiBold().FontSize(10);

                            table.Cell().Element(CellStyle).Background(LightGray).Text("Efectivo final declarado").SemiBold();
                            table.Cell().Element(CellStyle).Background(LightGray).AlignRight().Text($"${report.CashFlow.FinalCash:N2}");

                            // Diferencia con color
                            string diffColor = report.CashFlow.Difference switch
                            {
                                > 0 => SuccessColor,
                                < 0 => DangerColor,
                                _ => "#6c757d"
                            };

                            table.Cell().Element(CellStyle).Column(c =>
                            {
                                c.Item().Text(t =>
                                {
                                    t.Span("Diferencia (").SemiBold().FontSize(10);
                                    t.Span(report.CashFlow.DifferenceStatus).SemiBold().FontSize(10).FontColor(diffColor);
                                    t.Span(")").SemiBold().FontSize(10);
                                });
                            });
                            table.Cell().Element(CellStyle).AlignRight().Text($"${report.CashFlow.Difference:N2}").SemiBold().FontSize(10).FontColor(diffColor);
                        });

                        col.Item().PaddingTop(12);

                        // ── NOTAS ────────────────────────────────────────────────────
                        if (!string.IsNullOrEmpty(report.Shift.OpeningNotes) || !string.IsNullOrEmpty(report.Shift.ClosingNotes))
                        {
                            col.Item().Text("NOTAS").Bold().FontColor(PrimaryColor).FontSize(11);
                            col.Item().PaddingTop(4).Border(1).BorderColor(BorderColor).Padding(8).Column(c =>
                            {
                                if (!string.IsNullOrEmpty(report.Shift.OpeningNotes))
                                {
                                    c.Item().Text(t =>
                                    {
                                        t.Span("Apertura: ").SemiBold();
                                        t.Span(report.Shift.OpeningNotes);
                                    });
                                }

                                if (!string.IsNullOrEmpty(report.Shift.ClosingNotes))
                                {
                                    c.Item().PaddingTop(4).Text(t =>
                                    {
                                        t.Span("Cierre: ").SemiBold();
                                        t.Span(report.Shift.ClosingNotes);
                                    });
                                }
                            });

                            col.Item().PaddingTop(8);
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(BorderColor);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Impreso por: ").FontSize(7);
                                t.Span(report.Shift.ClosedByName ?? "Sistema").FontSize(7).SemiBold();
                            });

                            row.RelativeItem().AlignRight().Text($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7);
                        });

                        col.Item().PaddingTop(10).AlignCenter().Column(c =>
                        {
                            c.Item().LineHorizontal(1).LineColor(BorderColor);
                            c.Item().PaddingTop(5).Text("Firma del Cajero").FontSize(8);
                            c.Item().Text(report.Shift.CashierName).FontSize(8).Italic();
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
