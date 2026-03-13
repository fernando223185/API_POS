using Application.Abstractions.Documents;
using Application.Abstractions.Purchasing;
using Application.Abstractions.Storage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    public class PurchaseDocumentService : IPurchaseDocumentService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IPurchaseOrderReceivingRepository _receivingRepository;
        private readonly IS3StorageService _s3Service;

        public PurchaseDocumentService(
            IPurchaseOrderRepository purchaseOrderRepository,
            IPurchaseOrderReceivingRepository receivingRepository,
            IS3StorageService s3Service)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _receivingRepository = receivingRepository;
            _s3Service = s3Service;

            // Configurar licencia Community de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region ORDEN DE COMPRA PDF

        public async Task<byte[]> GeneratePurchaseOrderPdfAsync(int purchaseOrderId)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(purchaseOrderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Orden de compra con ID {purchaseOrderId} no encontrada");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Calibri"));

                    page.Header().Element(c => ComposeOrderHeader(c, order));
                    page.Content().Element(c => ComposeOrderContent(c, order));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeOrderHeader(IContainer container, Domain.Entities.PurchaseOrder order)
        {
            container.Row(row =>
            {
                // Logo y nombre de la empresa (izquierda)
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("MI EMPRESA S.A. DE C.V.")
                        .FontSize(18)
                        .Bold()
                        .FontColor("#1D336F"); // Color primario

                    column.Item().PaddingTop(5).Text("RFC: MIEA123456ABC")
                        .FontSize(9);

                    column.Item().Text("Av. Principal #123, Col. Centro")
                        .FontSize(9);

                    column.Item().Text("Ciudad, Estado, CP 12345")
                        .FontSize(9);

                    column.Item().Text("Tel: (55) 1234-5678")
                        .FontSize(9);
                });

                // Información del documento (derecha)
                row.ConstantItem(200).Column(column =>
                {
                    column.Item().Background("#E8EDF5") // Color primario claro
                        .Padding(10)
                        .Column(innerColumn =>
                        {
                            innerColumn.Item().Text("ORDEN DE COMPRA")
                                .FontSize(14)
                                .Bold()
                                .FontColor("#1D336F"); // Color primario

                            innerColumn.Item().PaddingTop(5).Row(infoRow =>
                            {
                                infoRow.AutoItem().Text("Folio: ").Bold();
                                infoRow.AutoItem().Text(order.Code).FontColor("#F29500").Bold(); // Color secundario
                            });

                            innerColumn.Item().Row(infoRow =>
                            {
                                infoRow.AutoItem().Text("Fecha: ").Bold();
                                infoRow.AutoItem().Text(order.OrderDate.ToString("dd/MM/yyyy"));
                            });

                            innerColumn.Item().Row(infoRow =>
                            {
                                infoRow.AutoItem().Text("Estado: ").Bold();
                                infoRow.AutoItem().Text(GetStatusText(order.Status))
                                    .FontColor(GetStatusColor(order.Status))
                                    .Bold();
                            });
                        });
                });
            });
        }

        private void ComposeOrderContent(IContainer container, Domain.Entities.PurchaseOrder order)
        {
            container.PaddingTop(20).Column(column =>
            {
                // Información del Proveedor
                column.Item().Element(c => ComposeOrderSupplierInfo(c, order));

                // Información de Entrega
                column.Item().PaddingTop(15).Element(c => ComposeOrderDeliveryInfo(c, order));

                // Tabla de Productos
                column.Item().PaddingTop(20).Element(c => ComposeOrderItemsTable(c, order));

                // Totales
                column.Item().PaddingTop(10).Element(c => ComposeOrderTotals(c, order));

                // Notas
                if (!string.IsNullOrEmpty(order.Notes))
                {
                    column.Item().PaddingTop(15).Element(c => ComposeOrderNotes(c, order));
                }

                // Términos y Condiciones
                column.Item().PaddingTop(15).Element(ComposeOrderTerms);
            });
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "Pending" => "Pendiente",
                "Approved" => "Aprobada",
                "InTransit" => "En Tránsito",
                "PartiallyReceived" => "Parcialmente Recibida",
                "Received" => "Recibida",
                "Cancelled" => "Cancelada",
                _ => status
            };
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => Colors.Orange.Medium,
                "Approved" => Colors.Green.Medium,
                "InTransit" => Colors.Blue.Medium,
                "PartiallyReceived" => Colors.Purple.Medium,
                "Received" => Colors.Green.Darken2,
                "Cancelled" => Colors.Red.Medium,
                _ => Colors.Grey.Medium
            };
        }

        private void ComposeOrderSupplierInfo(IContainer container, Domain.Entities.PurchaseOrder order)
        {
            container.Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().Text("PROVEEDOR").Bold().FontSize(11);

                    column.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Row(r =>
                            {
                                r.AutoItem().Text("Nombre: ").Bold();
                                r.AutoItem().Text(order.Supplier.Name);
                            });

                            col.Item().Row(r =>
                            {
                                r.AutoItem().Text("RFC: ").Bold();
                                r.AutoItem().Text(order.Supplier.TaxId ?? "N/A");
                            });
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Row(r =>
                            {
                                r.AutoItem().Text("Contacto: ").Bold();
                                r.AutoItem().Text(order.Supplier.ContactPerson ?? "N/A");
                            });

                            col.Item().Row(r =>
                            {
                                r.AutoItem().Text("Teléfono: ").Bold();
                                r.AutoItem().Text(order.Supplier.Phone ?? "N/A");
                            });
                        });
                    });
                });
        }

        private void ComposeOrderDeliveryInfo(IContainer container, Domain.Entities.PurchaseOrder order)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("INFORMACIÓN DE ENTREGA").Bold().FontSize(11);

                    column.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Text("Almacén: ").Bold();
                        r.AutoItem().Text(order.Warehouse.Name);
                    });

                    column.Item().Row(r =>
                    {
                        r.AutoItem().Text("Código: ").Bold();
                        r.AutoItem().Text(order.Warehouse.Code ?? "N/A");
                    });
                });

                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("FECHAS").Bold().FontSize(11);

                    column.Item().PaddingTop(5).Row(r =>
                    {
                        r.AutoItem().Text("Fecha Orden: ").Bold();
                        r.AutoItem().Text(order.OrderDate.ToString("dd/MM/yyyy"));
                    });

                    column.Item().Row(r =>
                    {
                        r.AutoItem().Text("Entrega Esperada: ").Bold();
                        r.AutoItem().Text(order.ExpectedDeliveryDate?.ToString("dd/MM/yyyy") ?? "No especificada");
                    });
                });
            });
        }

        private void ComposeOrderItemsTable(IContainer container, Domain.Entities.PurchaseOrder order)
        {
            container.Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // #
                    columns.ConstantColumn(100); // Código
                    columns.RelativeColumn(3);   // Descripción
                    columns.ConstantColumn(70);  // Cantidad
                    columns.ConstantColumn(90);  // P. Unitario
                    columns.ConstantColumn(60);  // Desc %
                    columns.ConstantColumn(90);  // Total
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").Bold();
                    header.Cell().Element(CellStyle).Text("Código").Bold();
                    header.Cell().Element(CellStyle).Text("Descripción").Bold();
                    header.Cell().Element(CellStyle).Text("Cantidad").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("P. Unitario").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Desc %").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Total").Bold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Background("#E8EDF5") // Color primario claro
                            .Padding(5)
                            .BorderBottom(1)
                            .BorderColor("#1D336F"); // Color primario
                    }
                });

                // Items
                int index = 1;
                foreach (var detail in order.Details)
                {
                    var bgColor = index % 2 == 0 ? "#FFFFFF" : "#F9FAFB"; // Alternar filas
                    
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).Text(index.ToString());
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).Text(detail.Product?.code ?? "N/A");
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).Text(detail.Product?.name ?? "Producto");
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).Text(detail.QuantityOrdered.ToString("N2"));
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).AlignRight().Text($"${detail.UnitPrice:N2}");
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).AlignRight().Text($"{detail.Discount:N2}%");
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).AlignRight().Text($"${detail.Total:N2}");

                    index++;
                }

                static IContainer RowCellStyle(IContainer container, string color)
                {
                    return container.Background(color)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(5);
                }
            });
        }

        private void ComposeOrderTotals(IContainer container, Domain.Entities.PurchaseOrder order)
        {
            container.AlignRight().Column(column =>
            {
                column.Spacing(3);

                column.Item().Row(row =>
                {
                    row.ConstantItem(120).Text("Subtotal:").Bold();
                    row.ConstantItem(100).AlignRight().Text($"${order.SubTotal:N2}");
                });

                column.Item().Row(row =>
                {
                    row.ConstantItem(120).Text("IVA (16%):").Bold();
                    row.ConstantItem(100).AlignRight().Text($"${order.Tax:N2}");
                });

                column.Item().Row(row =>
                {
                    row.ConstantItem(120).Background("#E8EDF5") // Color primario claro
                        .Padding(5)
                        .Text("TOTAL:").Bold().FontSize(12).FontColor("#1D336F");
                    row.ConstantItem(100).Background("#E8EDF5")
                        .Padding(5)
                        .AlignRight()
                        .Text($"${order.Total:N2}").Bold().FontSize(12)
                        .FontColor("#F29500"); // Color secundario para destacar el total
                });
            });
        }

        private void ComposeOrderNotes(IContainer container, Domain.Entities.PurchaseOrder order)
        {
            container.Background(Colors.Yellow.Lighten4)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().Text("NOTAS / OBSERVACIONES").Bold();
                    column.Item().PaddingTop(5).Text(order.Notes);
                });
        }

        private void ComposeOrderTerms(IContainer container)
        {
            container.Padding(10).Column(column =>
            {
                column.Item().Text("Términos y Condiciones").Bold().FontSize(9);
                column.Item().PaddingTop(5).Text(
                    "1. Los precios están sujetos a cambio sin previo aviso.\n" +
                    "2. El tiempo de entrega puede variar según disponibilidad.\n" +
                    "3. La mercancía debe ser inspeccionada al momento de la entrega.\n" +
                    "4. No se aceptan devoluciones después de 30 días."
                ).FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        }

        #endregion

        #region RECIBO DE MERCANCÍA PDF

        public async Task<byte[]> GenerateReceivingPdfAsync(int receivingId)
        {
            var receiving = await _receivingRepository.GetByIdAsync(receivingId);
            if (receiving == null)
            {
                throw new KeyNotFoundException($"Recibo con ID {receivingId} no encontrado");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Calibri"));

                    page.Header().Element(c => ComposeReceivingHeader(c, receiving));
                    page.Content().Element(c => ComposeReceivingContent(c, receiving));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeReceivingHeader(IContainer container, Domain.Entities.PurchaseOrderReceiving receiving)
        {
            container.Row(row =>
            {
                // Logo y nombre de la empresa (izquierda)
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("MI EMPRESA S.A. DE C.V.")
                        .FontSize(18)
                        .Bold()
                        .FontColor("#1D336F"); // Color primario

                    column.Item().PaddingTop(5).Text("RFC: MIEA123456ABC")
                        .FontSize(9);

                    column.Item().Text("Av. Principal #123, Col. Centro")
                        .FontSize(9);

                    column.Item().Text("Ciudad, Estado, CP 12345")
                        .FontSize(9);
                });

                // Información del documento (derecha)
                row.ConstantItem(200).Column(column =>
                {
                    column.Item().Background("#FEF3E5") // Color secundario claro
                        .Padding(10)
                        .Column(innerColumn =>
                        {
                            innerColumn.Item().Text("RECIBO DE MERCANCÍA")
                                .FontSize(14)
                                .Bold()
                                .FontColor("#1D336F"); // Color primario

                            innerColumn.Item().PaddingTop(5).Row(infoRow =>
                            {
                                infoRow.AutoItem().Text("Folio: ").Bold();
                                infoRow.AutoItem().Text(receiving.Code).FontColor("#F29500").Bold(); // Color secundario
                            });

                            innerColumn.Item().Row(infoRow =>
                            {
                                infoRow.AutoItem().Text("Fecha: ").Bold();
                                infoRow.AutoItem().Text(receiving.ReceivingDate.ToString("dd/MM/yyyy"));
                            });

                            innerColumn.Item().Row(infoRow =>
                            {
                                infoRow.AutoItem().Text("Estado: ").Bold();
                                infoRow.AutoItem().Text(GetReceivingStatusText(receiving.Status))
                                    .FontColor(GetReceivingStatusColor(receiving.Status))
                                    .Bold();
                            });

                            // Mostrar si ya se aplicó al inventario
                            if (receiving.IsPostedToInventory)
                            {
                                innerColumn.Item().PaddingTop(3).Row(infoRow =>
                                {
                                    infoRow.AutoItem().Text("? Aplicado a Inventario")
                                        .FontSize(8)
                                        .FontColor(Colors.Green.Darken2)
                                        .Bold();
                                });
                            }
                        });
                });
            });
        }

        private void ComposeReceivingContent(IContainer container, Domain.Entities.PurchaseOrderReceiving receiving)
        {
            container.PaddingTop(20).Column(column =>
            {
                // Información general
                column.Item().Element(c => ComposeReceivingInfo(c, receiving));

                // Tabla de productos recibidos
                column.Item().PaddingTop(20).Element(c => ComposeReceivingItemsTable(c, receiving));

                // Resumen
                column.Item().PaddingTop(15).Element(c => ComposeReceivingSummary(c, receiving));

                // Firmas
                column.Item().PaddingTop(30).Element(ComposeReceivingSignatures);
            });
        }

        private string GetReceivingStatusText(string status)
        {
            return status switch
            {
                "Draft" => "Borrador",
                "Received" => "Recibido",
                "QualityCheck" => "Control de Calidad",
                "Approved" => "Aprobado",
                "Rejected" => "Rechazado",
                "Applied" => "Aplicado",
                _ => status
            };
        }

        private string GetReceivingStatusColor(string status)
        {
            return status switch
            {
                "Draft" => Colors.Grey.Medium,
                "Received" => Colors.Blue.Medium,
                "QualityCheck" => Colors.Orange.Medium,
                "Approved" => Colors.Green.Medium,
                "Rejected" => Colors.Red.Medium,
                "Applied" => Colors.Green.Darken2,
                _ => Colors.Grey.Medium
            };
        }

        private void ComposeReceivingInfo(IContainer container, Domain.Entities.PurchaseOrderReceiving receiving)
        {
            container.Column(column =>
            {
                // Orden de Compra Relacionada
                column.Item().Background("#E8EDF5") // Color primario claro
                    .Padding(10)
                    .Row(row =>
                    {
                        row.AutoItem().Text("Orden de Compra: ").Bold();
                        row.AutoItem().Text(receiving.PurchaseOrder.Code)
                            .FontColor("#1D336F").Bold(); // Color primario
                    });

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PROVEEDOR").Bold().FontSize(11).FontColor("#1D336F");
                        col.Item().PaddingTop(5).Text(receiving.PurchaseOrder.Supplier.Name);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ALMACÉN").Bold().FontSize(11).FontColor("#1D336F");
                        col.Item().PaddingTop(5).Text(receiving.Warehouse.Name);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("RECIBIDO POR").Bold().FontSize(11).FontColor("#1D336F");
                        col.Item().PaddingTop(5).Text(receiving.ReceivedBy ?? receiving.CreatedBy?.Name ?? "N/A");
                    });
                });

                // Información de transporte
                if (!string.IsNullOrEmpty(receiving.CarrierName) || !string.IsNullOrEmpty(receiving.TrackingNumber))
                {
                    column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4)
                        .Padding(8)
                        .Row(row =>
                        {
                            if (!string.IsNullOrEmpty(receiving.CarrierName))
                            {
                                row.RelativeItem().Row(r =>
                                {
                                    r.AutoItem().Text("Transportista: ").Bold();
                                    r.AutoItem().Text(receiving.CarrierName);
                                });
                            }

                            if (!string.IsNullOrEmpty(receiving.TrackingNumber))
                            {
                                row.RelativeItem().Row(r =>
                                {
                                    r.AutoItem().Text("Guía: ").Bold();
                                    r.AutoItem().Text(receiving.TrackingNumber);
                                });
                            }
                        });
                }
            });
        }

        private void ComposeReceivingItemsTable(IContainer container, Domain.Entities.PurchaseOrderReceiving receiving)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // #
                    columns.ConstantColumn(90);  // Código
                    columns.RelativeColumn(3);   // Producto
                    columns.ConstantColumn(70);  // Ordenado
                    columns.ConstantColumn(70);  // Recibido
                    columns.ConstantColumn(70);  // Aceptado
                    columns.ConstantColumn(70);  // Rechazado
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").Bold();
                    header.Cell().Element(CellStyle).Text("Código").Bold();
                    header.Cell().Element(CellStyle).Text("Producto").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Ordenado").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Recibido").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Aceptado").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Rechazado").Bold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Background("#FEF3E5") // Color secundario claro
                            .Padding(5)
                            .BorderBottom(1)
                            .BorderColor("#F29500"); // Color secundario
                    }
                });

                // Items
                int index = 1;
                foreach (var detail in receiving.Details)
                {
                    var bgColor = detail.QuantityRejected > 0 ? "#FFE5E5" : 
                                  (index % 2 == 0 ? "#FFFFFF" : "#F9FAFB");

                    table.Cell().Element(c => RowCellStyle(c, bgColor)).Text(index.ToString());
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).Text(detail.Product?.code ?? "N/A");
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).Text(detail.Product?.name ?? "Producto");
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).AlignRight()
                        .Text(detail.PurchaseOrderDetail?.QuantityOrdered.ToString("N2") ?? "0");
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).AlignRight()
                        .Text(detail.QuantityReceived.ToString("N2"));
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).AlignRight()
                        .Text((detail.QuantityApproved ?? 0).ToString("N2"))
                        .FontColor(Colors.Green.Medium);
                    table.Cell().Element(c => RowCellStyle(c, bgColor)).AlignRight()
                        .Text((detail.QuantityRejected ?? 0).ToString("N2"))
                        .FontColor(Colors.Red.Medium);

                    index++;
                }

                static IContainer RowCellStyle(IContainer container, string color)
                {
                    return container.Background(color)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(5);
                }
            });
        }

        private void ComposeReceivingSummary(IContainer container, Domain.Entities.PurchaseOrderReceiving receiving)
        {
            var totalReceived = receiving.Details.Sum(d => d.QuantityReceived);
            var totalAccepted = receiving.Details.Sum(d => d.QuantityApproved ?? 0);
            var totalRejected = receiving.Details.Sum(d => d.QuantityRejected ?? 0);

            container.Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("RESUMEN DE RECEPCIÓN").Bold().FontSize(11);

                        column.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("Total Artículos: ").Bold();
                            r.AutoItem().Text(receiving.Details.Count.ToString());
                        });

                        column.Item().Row(r =>
                        {
                            r.AutoItem().Text("Total Recibido: ").Bold();
                            r.AutoItem().Text(totalReceived.ToString("N2"));
                        });
                    });

                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("CALIDAD").Bold().FontSize(11).FontColor(Colors.Green.Medium);

                        column.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("Aceptado: ").Bold();
                            r.AutoItem().Text(totalAccepted.ToString("N2")).FontColor(Colors.Green.Medium);
                        });

                        column.Item().Row(r =>
                        {
                            r.AutoItem().Text("Rechazado: ").Bold();
                            r.AutoItem().Text(totalRejected.ToString("N2")).FontColor(Colors.Red.Medium);
                        });
                    });
                });
        }

        private void ComposeReceivingSignatures(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(5).AlignCenter().Text("Recibió").FontSize(9).Bold();
                    column.Item().AlignCenter().Text("Almacén").FontSize(8);
                });

                row.ConstantItem(50);

                row.RelativeItem().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(5).AlignCenter().Text("Autorizó").FontSize(9).Bold();
                    column.Item().AlignCenter().Text("Compras").FontSize(8);
                });

                row.ConstantItem(50);

                row.RelativeItem().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(5).AlignCenter().Text("V°B°").FontSize(9).Bold();
                    column.Item().AlignCenter().Text("Gerencia").FontSize(8);
                });
            });
        }

        #endregion

        #region FOOTER COMÚN

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Documento generado automáticamente el ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(" | Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        }

        #endregion

        #region GUARDAR EN S3

        public async Task<string> SavePurchaseOrderPdfToS3Async(int purchaseOrderId)
        {
            var pdfBytes = await GeneratePurchaseOrderPdfAsync(purchaseOrderId);

            var order = await _purchaseOrderRepository.GetByIdAsync(purchaseOrderId);
            var fileName = $"OC-{order.Code}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            using var stream = new MemoryStream(pdfBytes);
            return await _s3Service.UploadImageAsync(stream, fileName, "application/pdf", "purchase-orders");
        }

        public async Task<string> SaveReceivingPdfToS3Async(int receivingId)
        {
            var pdfBytes = await GenerateReceivingPdfAsync(receivingId);

            var receiving = await _receivingRepository.GetByIdAsync(receivingId);
            var fileName = $"REC-{receiving.Code}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            using var stream = new MemoryStream(pdfBytes);
            return await _s3Service.UploadImageAsync(stream, fileName, "application/pdf", "receivings");
        }

        #endregion
    }
}
