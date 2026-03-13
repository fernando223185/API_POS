using Application.Abstractions.Documents;
using Application.Abstractions.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio para generar documentos PDF de ventas
    /// Usa QuestPDF para generación de PDFs profesionales
    /// </summary>
    public class SaleDocumentService : ISaleDocumentService
    {
        private readonly ISaleRepository _saleRepository;

        public SaleDocumentService(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        /// <summary>
        /// Genera un ticket de venta en formato PDF
        /// </summary>
        public async Task<byte[]> GenerateSaleTicketPdfAsync(int saleId, bool includeCompanyLogo = true)
        {
            var sale = await _saleRepository.GetByIdAsync(saleId);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Venta con ID {saleId} no encontrada");
            }

            // Configurar licencia de QuestPDF (Community license)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, sale));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Genera una factura de venta en formato PDF
        /// </summary>
        public async Task<byte[]> GenerateSaleInvoicePdfAsync(int saleId)
        {
            var sale = await _saleRepository.GetByIdAsync(saleId);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Venta con ID {saleId} no encontrada");
            }

            if (!sale.RequiresInvoice)
            {
                throw new InvalidOperationException("Esta venta no requiere factura");
            }

            // Similar al ticket pero con formato de factura fiscal
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(header => ComposeInvoiceHeader(header, sale));
                    page.Content().Element(content => ComposeInvoiceContent(content, sale));
                    page.Footer().Element(footer => ComposeInvoiceFooter(footer, sale));
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Genera un reporte de ventas en formato PDF
        /// </summary>
        public async Task<byte[]> GenerateSalesReportPdfAsync(DateTime fromDate, DateTime toDate, int? warehouseId = null)
        {
            // TODO: Implementar reporte de ventas
            throw new NotImplementedException("Reporte de ventas en desarrollo");
        }

        #region Compose Methods - Ticket

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("EXPANDA ERP")
                    .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                
                column.Item().AlignCenter().Text("Sistema de Punto de Venta")
                    .FontSize(10).FontColor(Colors.Grey.Darken2);
                
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            });
        }

        private void ComposeContent(IContainer container, Domain.Entities.Sale sale)
        {
            container.Column(column =>
            {
                // Información de la venta
                column.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Folio: {sale.Code}").Bold();
                        col.Item().Text($"Fecha: {sale.SaleDate:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Almacén: {sale.Warehouse?.Name ?? "N/A"}");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignRight().Text($"Vendedor: {sale.User?.Name ?? "N/A"}");
                        if (!string.IsNullOrEmpty(sale.CustomerName))
                        {
                            col.Item().AlignRight().Text($"Cliente: {sale.CustomerName}");
                        }
                        col.Item().AlignRight().Text($"Estado: {FormatStatus(sale.Status)}");
                    });
                });

                // Tabla de productos
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); // Producto
                        columns.RelativeColumn(1); // Cantidad
                        columns.RelativeColumn(2); // Precio
                        columns.RelativeColumn(1); // Desc%
                        columns.RelativeColumn(2); // Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Producto").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Cant").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Precio U.").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Desc%").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Total").FontColor(Colors.White).Bold();
                    });

                    // Rows
                    foreach (var detail in sale.Details)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"{detail.ProductCode} - {detail.ProductName}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detail.Quantity.ToString("F2"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"${detail.UnitPrice:F2}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(detail.DiscountPercentage > 0 ? $"{detail.DiscountPercentage:F2}%" : "-");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"${detail.Total:F2}").Bold();
                    }
                });

                // Totales
                column.Item().PaddingTop(15).AlignRight().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Subtotal:");
                        row.RelativeItem().AlignRight().Text($"${sale.SubTotal:F2}");
                    });

                    if (sale.DiscountAmount > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Descuento ({sale.DiscountPercentage:F2}%):");
                            row.RelativeItem().AlignRight().Text($"-${sale.DiscountAmount:F2}").FontColor(Colors.Red.Medium);
                        });
                    }

                    if (sale.TaxAmount > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("IVA (16%):");
                            row.RelativeItem().AlignRight().Text($"${sale.TaxAmount:F2}");
                        });
                    }

                    col.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken3);

                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL:").FontSize(14).Bold();
                        row.RelativeItem().AlignRight().Text($"${sale.Total:F2}").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                    });
                });

                // Formas de pago
                if (sale.Payments.Any())
                {
                    column.Item().PaddingTop(15).Column(col =>
                    {
                        col.Item().Text("Formas de Pago:").Bold().FontSize(12);
                        col.Item().PaddingTop(5).PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        foreach (var payment in sale.Payments)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text(FormatPaymentMethod(payment.PaymentMethod));
                                row.RelativeItem().AlignRight().Text($"${payment.Amount:F2}").Bold();
                            });

                            if (!string.IsNullOrEmpty(payment.AuthorizationCode))
                            {
                                col.Item().Text($"  Autorización: {payment.AuthorizationCode}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            }
                        }

                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("Total Pagado:").Bold();
                            row.RelativeItem().AlignRight().Text($"${sale.AmountPaid:F2}").Bold();
                        });

                        if (sale.ChangeAmount > 0)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Cambio:").Bold().FontColor(Colors.Green.Darken2);
                                row.RelativeItem().AlignRight().Text($"${sale.ChangeAmount:F2}").Bold().FontColor(Colors.Green.Darken2);
                            });
                        }
                    });
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                
                column.Item().AlignCenter().Text("¡Gracias por su compra!")
                    .FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                
                column.Item().AlignCenter().Text("Conserve este ticket para cualquier aclaración")
                    .FontSize(8).FontColor(Colors.Grey.Darken1);
                
                column.Item().AlignCenter().PaddingTop(10).Text(text =>
                {
                    text.Span("Generado: ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        }

        #endregion

        #region Compose Methods - Invoice

        private void ComposeInvoiceHeader(IContainer container, Domain.Entities.Sale sale)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("EXPANDA ERP").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().Text("RFC: XAXX010101000").FontSize(10);
                        col.Item().Text("Régimen Fiscal: 601 - General").FontSize(9);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("FACTURA").FontSize(16).Bold();
                        col.Item().Text($"Folio: {sale.Code}").FontSize(12);
                        col.Item().Text($"Fecha: {sale.SaleDate:dd/MM/yyyy}").FontSize(10);
                    });
                });

                column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Darken3);
            });
        }

        private void ComposeInvoiceContent(IContainer container, Domain.Entities.Sale sale)
        {
            // Similar a ComposeContent pero con más detalles fiscales
            ComposeContent(container, sale);
        }

        private void ComposeInvoiceFooter(IContainer container, Domain.Entities.Sale sale)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                
                if (!string.IsNullOrEmpty(sale.InvoiceUuid))
                {
                    column.Item().PaddingTop(5).Text($"UUID: {sale.InvoiceUuid}").FontSize(8).FontFamily("Courier New");
                    column.Item().Text("Sello Digital del CFDI").FontSize(7).FontColor(Colors.Grey.Darken1);
                }
                else
                {
                    column.Item().PaddingTop(5).AlignCenter().Text("PENDIENTE DE TIMBRADO")
                        .FontSize(10).Bold().FontColor(Colors.Orange.Darken2);
                }
            });
        }

        #endregion

        #region Helper Methods

        private string FormatStatus(string status)
        {
            return status switch
            {
                "Draft" => "Borrador",
                "Completed" => "Completada",
                "Cancelled" => "Cancelada",
                "Invoiced" => "Facturada",
                _ => status
            };
        }

        private string FormatPaymentMethod(string method)
        {
            return method switch
            {
                "Cash" => "Efectivo",
                "CreditCard" => "Tarjeta de Crédito",
                "DebitCard" => "Tarjeta de Débito",
                "Transfer" => "Transferencia Bancaria",
                "Check" => "Cheque",
                _ => method
            };
        }

        #endregion
    }
}
