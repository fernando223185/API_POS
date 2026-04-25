using Domain.Entities;
using QRCoder;
using PurchaseOrderEntity     = Domain.Entities.PurchaseOrder;
using PurchaseReceivingEntity = Domain.Entities.PurchaseOrderReceiving;
using WarehouseTransferEntity  = Domain.Entities.WarehouseTransfer;
using WarehouseTransferReceivingEntity = Domain.Entities.WarehouseTransferReceiving;

namespace Application.Core.Reports.Engine
{
    /// <summary>
    /// Convierte entidades de dominio a Dictionary[field_key, value] 
    /// para que el ReportPdfEngine pueda renderizarlos con cualquier plantilla.
    /// </summary>
    public static class ReportDataProvider
    {
        // ─────────────────────────────────────────────
        // VENTAS (POS y Delivery)
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromSale(Sale sale) => new()
        {
            ["saleCode"]          = sale.Code,
            ["saleDate"]          = sale.SaleDate,
            ["saleType"]          = sale.SaleType == "Delivery" ? "Entrega a domicilio" : "Punto de venta",
            ["status"]            = TranslateSaleStatus(sale.Status),
            ["customerName"]      = sale.Customer?.Name ?? sale.CustomerName ?? "Público general",
            ["customerCode"]      = sale.Customer?.Code ?? "",
            ["customerTaxId"]     = sale.Customer?.TaxId ?? "",
            ["sellerName"]        = sale.User?.Name ?? "",
            ["warehouseName"]     = sale.Warehouse?.Name ?? "",
            ["branchName"]        = sale.Warehouse?.Branch?.Name ?? sale.Branch?.Name ?? "",
            ["companyName"]       = sale.Company?.LegalName ?? "",
            ["deliveryAddress"]   = sale.DeliveryAddress ?? "",
            ["scheduledDate"]     = sale.ScheduledDeliveryDate,
            ["deliveredAt"]       = sale.DeliveredAt,
            ["notes"]             = sale.Notes ?? "",
            ["totalSubtotal"]     = sale.SubTotal,
            ["totalDiscount"]     = sale.DiscountAmount,
            ["totalTax"]          = sale.TaxAmount,
            ["totalAmount"]       = sale.Total,
            ["amountPaid"]        = sale.AmountPaid,
            ["changeAmount"]      = sale.ChangeAmount,
            ["paymentMethods"]    = string.Join(", ", sale.Payments.Select(p => p.PaymentMethod).Distinct()),
        };

        public static List<Dictionary<string, object?>> FromSaleDetails(Sale sale) =>
            sale.Details.Select(d => new Dictionary<string, object?>
            {
                ["productCode"]        = d.ProductCode,
                ["productName"]        = d.ProductName,
                ["quantity"]           = d.Quantity,
                ["unitPrice"]          = d.UnitPrice,
                ["discountPercentage"] = d.DiscountPercentage,
                ["discountAmount"]     = d.DiscountAmount,
                ["taxPercentage"]      = d.TaxPercentage,
                ["taxAmount"]          = d.TaxAmount,
                ["subtotal"]           = d.SubTotal,
                ["lineTotal"]          = d.Total,
            }).ToList();

        // ─────────────────────────────────────────────
        // COTIZACIONES
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromQuotation(Quotation q) => new()
        {
            ["quotationCode"]      = q.Code,
            ["quotationDate"]      = q.QuotationDate,
            ["validUntil"]         = q.ValidUntil,
            ["status"]             = TranslateQuotationStatus(q.Status),
            ["customerName"]       = q.Customer?.Name ?? q.CustomerName ?? "Público general",
            ["customerTaxId"]      = q.Customer?.TaxId ?? "",
            ["sellerName"]         = q.User?.Name ?? "",
            ["warehouseName"]      = q.Warehouse?.Name ?? "",
            ["branchName"]         = q.Branch?.Name ?? "",
            ["companyName"]        = q.Company?.LegalName ?? "",
            ["notes"]              = q.Notes ?? "",
            ["convertedSaleCode"]  = q.Sale?.Code ?? "",
            ["totalSubtotal"]      = q.SubTotal,
            ["totalDiscount"]      = q.DiscountAmount,
            ["totalTax"]           = q.TaxAmount,
            ["totalAmount"]        = q.Total,
        };

        public static List<Dictionary<string, object?>> FromQuotationDetails(Quotation q) =>
            q.Details.Select(d => new Dictionary<string, object?>
            {
                ["productCode"]        = d.ProductCode,
                ["productName"]        = d.ProductName,
                ["quantity"]           = d.Quantity,
                ["unitPrice"]          = d.UnitPrice,
                ["discountPercentage"] = d.DiscountPercentage,
                ["discountAmount"]     = d.DiscountAmount,
                ["taxPercentage"]      = d.TaxPercentage,
                ["taxAmount"]          = d.TaxAmount,
                ["lineTotal"]          = d.Total,
            }).ToList();

        // ─────────────────────────────────────────────
        // ÓRDENES DE COMPRA
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromPurchaseOrder(PurchaseOrderEntity po) => new()
        {
            ["poCode"]          = po.Code,
            ["poDate"]          = po.OrderDate,
            ["status"]          = po.Status,
            ["supplierName"]    = po.Supplier?.Name ?? "",
            ["supplierTaxId"]   = po.Supplier?.TaxId ?? "",
            ["warehouseName"]   = po.Warehouse?.Name ?? "",
            ["branchName"]      = po.Warehouse?.Branch?.Name ?? "",
            ["notes"]           = po.Notes ?? "",
            ["totalAmount"]     = po.Total,
        };

        public static List<Dictionary<string, object?>> FromPurchaseOrderDetails(PurchaseOrderEntity po) =>
            po.Details.Select(d => new Dictionary<string, object?>
            {
                ["productCode"]       = d.Product?.code ?? "",
                ["productName"]       = d.Product?.name ?? "",
                ["quantityOrdered"]   = d.QuantityOrdered,
                ["quantityReceived"]  = d.QuantityReceived,
                ["unitCost"]          = d.UnitPrice,
                ["lineTotal"]         = d.Total,
            }).ToList();

        // ─────────────────────────────────────────────
        // INVENTARIO (Stock por almacén)
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> InventoryHeader(string warehouseName, string companyName) => new()
        {
            ["reportDate"]        = DateTime.UtcNow,
            ["warehouseName"]     = warehouseName,
            ["companyName"]       = companyName,
        };

        public static Dictionary<string, object?> FromProductStock(ProductStock ps) => new()
        {
            ["productCode"]     = ps.Product?.code ?? "",
            ["productName"]     = ps.Product?.name ?? "",
            ["category"]        = ps.Product?.Category?.Name ?? "",
            ["currentStock"]    = ps.Quantity,
            ["minimumStock"]    = ps.Product?.MinimumStock ?? 0m,
            ["unitCost"]        = ps.Product?.BaseCost ?? 0m,
            ["totalValue"]      = ps.Quantity * (ps.Product?.BaseCost ?? 0m),
            ["lastMovement"]    = ps.LastMovementDate,
        };

        // ─────────────────────────────────────────────
        // TURNO DE CAJERO
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromCashierShift(CashierShift shift, List<Sale> sales) => new()
        {
            ["shiftCode"]          = shift.Code,
            ["cashierName"]        = shift.Cashier?.Name ?? "",
            ["openedAt"]           = shift.OpeningDate,
            ["closedAt"]           = shift.ClosingDate,
            ["warehouseName"]      = shift.Warehouse?.Name ?? "",
            ["openingCash"]        = shift.InitialCash,
            ["closingCash"]        = shift.FinalCash ?? 0m,
            ["difference"]         = shift.Difference ?? 0m,
            ["cashTotal"]          = shift.CashSales,
            ["cardTotal"]          = shift.CardSales,
            ["transferTotal"]      = shift.TransferSales,
            ["totalSales"]         = shift.TotalSalesAmount,
            ["salesCount"]         = sales.Count,
        };

        public static List<Dictionary<string, object?>> FromCashierShiftSales(List<Sale> sales) =>
            sales.Select(s => new Dictionary<string, object?>
            {
                ["saleCode"]       = s.Code,
                ["saleTime"]       = s.SaleDate,
                ["customerName"]   = s.Customer?.Name ?? s.CustomerName ?? "Público general",
                ["paymentMethod"]  = string.Join(", ", s.Payments.Select(p => p.PaymentMethod).Distinct()),
                ["saleTotal"]      = s.Total,
            }).ToList();

        // ─────────────────────────────────────────────
        // RECIBO DE MERCANCÍA
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromReceiving(PurchaseReceivingEntity rec) => new()
        {
            ["receivingCode"]         = rec.Code,
            ["receivingDate"]         = rec.ReceivingDate,
            ["status"]                = rec.Status,
            ["poCode"]                = rec.PurchaseOrder?.Code ?? "",
            ["supplierName"]          = rec.PurchaseOrder?.Supplier?.Name ?? "",
            ["supplierTaxId"]         = rec.PurchaseOrder?.Supplier?.TaxId ?? "",
            ["warehouseName"]         = rec.Warehouse?.Name ?? "",
            ["receivedBy"]            = rec.ReceivedBy ?? "",
            ["supplierInvoiceNumber"] = rec.SupplierInvoiceNumber ?? "",
            ["carrierName"]           = rec.CarrierName ?? "",
            ["trackingNumber"]        = rec.TrackingNumber ?? "",
            ["notes"]                 = rec.Notes ?? "",
        };

        public static List<Dictionary<string, object?>> FromReceivingDetails(PurchaseReceivingEntity rec) =>
            rec.Details.Select(d => new Dictionary<string, object?>
            {
                ["productCode"]        = d.Product?.code ?? "",
                ["productName"]        = d.Product?.name ?? "",
                ["quantityReceived"]   = d.QuantityReceived,
                ["quantityApproved"]   = d.QuantityApproved ?? 0m,
                ["quantityRejected"]   = d.QuantityRejected ?? 0m,
                ["lotNumber"]          = d.LotNumber ?? "",
                ["storageLocation"]    = d.StorageLocation ?? "",
                ["notes"]              = d.Notes ?? "",
            }).ToList();

        // ─────────────────────────────────────────────
        // KARDEX DE INVENTARIO
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> KardexHeader(
            string warehouseName, string productName, DateTime? fromDate, DateTime? toDate) => new()
        {
            ["reportDate"]    = DateTime.UtcNow,
            ["warehouseName"] = warehouseName,
            ["productName"]   = productName,
            ["fromDate"]      = fromDate,
            ["toDate"]        = toDate,
        };

        public static Dictionary<string, object?> FromKardexMovement(InventoryMovement m) => new()
        {
            ["movementDate"]   = m.CreatedAt,
            ["movementCode"]   = m.Code,
            ["movementType"]   = m.MovementType,
            ["productCode"]    = m.Product?.code ?? "",
            ["productName"]    = m.Product?.name ?? "",
            ["warehouseName"]  = m.Warehouse?.Name ?? "",
            ["quantity"]       = m.Quantity,
            ["stockBefore"]    = m.StockBefore,
            ["stockAfter"]     = m.StockAfter,
            ["unitCost"]       = m.UnitCost ?? 0m,
            ["totalCost"]      = m.TotalCost ?? 0m,
            ["reference"]      = m.PurchaseOrderReceiving?.Code ?? m.Sale?.Code ?? "",
            ["notes"]          = m.Notes ?? "",
            ["createdBy"]      = m.CreatedBy?.Name ?? "",
        };

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        private static string TranslateSaleStatus(string status) => status switch
        {
            "Draft"     => "Borrador",
            "Completed" => "Completada",
            "Cancelled" => "Cancelada",
            "Invoiced"  => "Facturada",
            _           => status
        };

        private static string TranslateQuotationStatus(string status) => status switch
        {
            "Draft"     => "Borrador",
            "Sent"      => "Enviada",
            "Converted" => "Convertida",
            "Cancelled" => "Cancelada",
            "Expired"   => "Vencida",
            _           => status
        };

        // ─────────────────────────────────────────────
        // FACTURA CFDI
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromInvoice(Invoice inv) => new()
        {
            ["invoiceFolio"]            = inv.Folio,
            ["invoiceSerie"]            = inv.Serie,
            ["invoiceDate"]             = inv.InvoiceDate,
            ["invoiceStatus"]           = inv.Status,
            ["uuid"]                    = inv.Uuid ?? "",
            ["timbradoAt"]              = inv.TimbradoAt,
            ["tipoDeComprobante"]       = inv.TipoDeComprobante,
            ["metodoPago"]              = inv.MetodoPago,
            ["formaPago"]               = inv.FormaPago,
            ["condicionesDePago"]       = inv.CondicionesDePago ?? "",
            ["moneda"]                  = inv.Moneda,
            ["tipoCambio"]              = inv.TipoCambio,
            ["lugarExpedicion"]         = inv.LugarExpedicion,
            // Emisor
            ["companyLogoUrl"]         = inv.Company?.LogoUrl ?? "",
            ["companyTradeName"]       = inv.Company?.TradeName ?? inv.EmisorNombre,
            ["emisorRfc"]               = inv.EmisorRfc,
            ["emisorNombre"]            = inv.EmisorNombre,
            ["emisorRegimenFiscal"]     = inv.EmisorRegimenFiscal,
            // Receptor
            ["receptorRfc"]             = inv.ReceptorRfc,
            ["receptorNombre"]          = inv.ReceptorNombre,
            ["receptorDomicilioFiscal"] = inv.ReceptorDomicilioFiscal,
            ["receptorRegimenFiscal"]   = inv.ReceptorRegimenFiscal ?? "",
            ["receptorUsoCfdi"]         = inv.ReceptorUsoCfdi,
            // Venta origen
            ["saleCode"]                = inv.Sale?.Code ?? "",
            // Timbrado
            ["noCertificadoCfdi"]       = inv.NoCertificadoCfdi ?? "",
            ["noCertificadoSat"]        = inv.NoCertificadoSat ?? "",
            ["selloCfdi"]               = inv.SelloCfdi ?? "",
            ["selloSat"]                = inv.SelloSat ?? "",
            ["cadenaOriginalSat"]       = inv.CadenaOriginalSat ?? "",
            ["qrCode"]                  = inv.QrCode ?? "",
            // Totales
            ["subTotal"]                = inv.SubTotal,
            ["discountAmount"]          = inv.DiscountAmount,
            ["taxAmount"]               = inv.TaxAmount,
            ["total"]                   = inv.Total,
        };

        public static List<Dictionary<string, object?>> FromInvoiceDetails(Invoice inv) =>
            inv.Details.Select(d => new Dictionary<string, object?>
            {
                ["claveProdServ"]    = d.ClaveProdServ,
                ["noIdentificacion"] = d.NoIdentificacion ?? "",
                ["descripcion"]      = d.Descripcion,
                ["cantidad"]         = d.Cantidad,
                ["claveUnidad"]      = d.ClaveUnidad,
                ["unidad"]           = d.Unidad ?? "",
                ["valorUnitario"]    = d.ValorUnitario,
                ["descuento"]        = d.Descuento,
                ["importe"]          = d.Importe,
                ["trasladoTasa"]     = d.TieneTraslados ? (d.TrasladoTasaOCuota ?? 0m) : 0m,
                ["trasladoImporte"]  = d.TieneTraslados ? (d.TrasladoImporte ?? 0m) : 0m,
            }).ToList();

        // ─────────────────────────────────────────────
        // COMPLEMENTO DE PAGO CFDI
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromPayment(Payment p) => new()
        {
            // Comprobante
            ["paymentNumber"]           = p.PaymentNumber,
            ["paymentDate"]             = p.PaymentDate,
            ["totalAmount"]             = p.TotalAmount,
            ["currency"]                = p.Currency,
            ["exchangeRate"]            = p.ExchangeRate,
            ["paymentFormSAT"]          = p.PaymentFormSAT,
            ["reference"]               = p.Reference ?? "",
            ["bankOrigin"]              = p.BankOrigin ?? "",
            ["bankAccountOrigin"]       = p.BankAccountOrigin ?? "",
            ["bankDestination"]         = p.BankDestination ?? "",
            ["bankAccountDestination"]  = p.BankAccountDestination ?? "",
            // Serie/Folio del complemento
            ["complementSerie"]         = p.ComplementSerie ?? "",
            ["complementFolio"]         = p.ComplementFolio ?? "",
            ["uuid"]                    = p.Uuid ?? "",
            ["timbradoAt"]              = p.TimbradoAt,
            // Emisor
            ["companyLogoUrl"]          = p.Company?.LogoUrl ?? "",
            ["emisorRfc"]               = p.EmisorRfc ?? "",
            ["emisorNombre"]            = p.EmisorNombre ?? "",
            ["emisorRegimenFiscal"]     = p.EmisorRegimenFiscal ?? "",
            ["lugarExpedicion"]         = p.LugarExpedicion ?? "",
            // Receptor
            ["receptorRfc"]             = p.ReceptorRfc ?? "",
            ["receptorNombre"]          = p.ReceptorNombre ?? "",
            ["receptorDomicilioFiscal"] = p.ReceptorDomicilioFiscal ?? "",
            ["receptorRegimenFiscal"]   = p.ReceptorRegimenFiscal ?? "",
            ["receptorUsoCfdi"]         = p.ReceptorUsoCfdi ?? "",
            // Timbrado
            ["noCertificadoCfdi"]       = p.NoCertificadoCfdi ?? "",
            ["noCertificadoSat"]        = p.NoCertificadoSat ?? "",
            ["selloCfdi"]               = p.SelloCfdi ?? "",
            ["selloSat"]                = p.SelloSat ?? "",
            ["cadenaOriginalSat"]       = p.CadenaOriginalSat ?? "",
            ["qrCode"]                  = p.QrCode ?? "",
            ["notes"]                   = p.Notes ?? "",
        };

        public static List<Dictionary<string, object?>> FromPaymentApplications(Payment p) =>
            p.PaymentApplications.Select(a => new Dictionary<string, object?>
            {
                ["serieAndFolio"]             = a.SerieAndFolio,
                ["folioUUID"]                 = a.FolioUUID,
                ["originalInvoiceAmount"]     = a.OriginalInvoiceAmount,
                ["partialityNumber"]          = a.PartialityNumber,
                ["previousBalance"]           = a.PreviousBalance,
                ["amountApplied"]             = a.AmountApplied,
                ["newBalance"]                = a.NewBalance,
                ["paymentType"]               = a.PaymentType,
            }).ToList();

        // ─────────────────────────────────────────────
        // TRASPASO DE ALMACÉN — DOCUMENTO DE SALIDA
        // ─────────────────────────────────────────────

        /// <summary>
        /// Genera el diccionario de cabecera para el documento de despacho (salida).
        /// <param name="receivingUrl">URL que se codifica en el QR para que el destino registre la entrada desde móvil.</param>
        /// </summary>
        public static Dictionary<string, object?> FromWarehouseTransferDispatch(
            WarehouseTransferEntity transfer, string receivingUrl)
        {
            var qrBytes  = GenerateQrPng(receivingUrl);
            var qrBase64 = Convert.ToBase64String(qrBytes);

            return new Dictionary<string, object?>
            {
                ["transferCode"]              = transfer.Code,
                ["transferDate"]              = transfer.TransferDate,
                ["dispatchedAt"]              = transfer.DispatchedAt,
                ["status"]                    = TranslateTransferStatus(transfer.Status),
                ["sourceWarehouseName"]       = transfer.SourceWarehouse?.Name ?? "",
                ["sourceWarehouseCode"]       = transfer.SourceWarehouse?.Code ?? "",
                ["destinationWarehouseName"]  = transfer.DestinationWarehouse?.Name ?? "",
                ["destinationWarehouseCode"]  = transfer.DestinationWarehouse?.Code ?? "",
                ["companyName"]               = transfer.SourceWarehouse?.Branch?.Company?.LegalName ?? "",
                ["dispatchedByName"]          = transfer.DispatchedBy?.Name ?? "",
                ["createdByName"]             = transfer.CreatedBy?.Name ?? "",
                ["notes"]                     = transfer.Notes ?? "",
                ["totalProducts"]             = transfer.Details.Count,
                ["totalQuantityDispatched"]   = transfer.Details.Sum(d => d.QuantityDispatched),
                // QR para escanear desde móvil y dar entrada
                ["receivingUrl"]              = receivingUrl,
                ["receivingQrCode"]           = qrBase64,
            };
        }

        public static List<Dictionary<string, object?>> FromWarehouseTransferDispatchDetails(
            WarehouseTransferEntity transfer) =>
            transfer.Details.Select(d => new Dictionary<string, object?>
            {
                ["productCode"]         = d.Product?.code ?? "",
                ["productName"]         = d.Product?.name ?? "",
                ["quantityRequested"]   = d.QuantityRequested,
                ["quantityDispatched"]  = d.QuantityDispatched,
                ["unitCost"]            = d.UnitCost ?? 0m,
                ["lineTotal"]           = d.QuantityDispatched * (d.UnitCost ?? 0m),
                ["notes"]               = d.Notes ?? "",
            }).ToList();

        // ─────────────────────────────────────────────
        // TRASPASO DE ALMACÉN — DOCUMENTO DE ENTRADA
        // ─────────────────────────────────────────────

        public static Dictionary<string, object?> FromWarehouseTransferReceivingDoc(
            WarehouseTransferReceivingEntity receiving)
        {
            var transfer = receiving.WarehouseTransfer;
            return new Dictionary<string, object?>
            {
                ["receivingCode"]             = receiving.Code,
                ["receivingDate"]             = receiving.ReceivingDate,
                ["receivingType"]             = receiving.ReceivingType == "Complete" ? "Completa" : "Parcial",
                ["transferCode"]              = transfer?.Code ?? "",
                ["sourceWarehouseName"]       = transfer?.SourceWarehouse?.Name ?? "",
                ["sourceWarehouseCode"]       = transfer?.SourceWarehouse?.Code ?? "",
                ["destinationWarehouseName"]  = receiving.DestinationWarehouse?.Name ?? "",
                ["destinationWarehouseCode"]  = receiving.DestinationWarehouse?.Code ?? "",
                ["companyName"]               = receiving.DestinationWarehouse?.Branch?.Company?.LegalName ?? "",
                ["receivedByName"]            = receiving.CreatedBy?.Name ?? "",
                ["notes"]                     = receiving.Notes ?? "",
                ["totalProducts"]             = receiving.Details.Count,
                ["totalQuantityReceived"]     = receiving.Details.Sum(d => d.QuantityReceived),
                ["totalQuantityDispatched"]   = transfer?.Details.Sum(d => d.QuantityDispatched) ?? 0m,
                ["totalQuantityPending"]      = (transfer?.Details.Sum(d => d.QuantityDispatched) ?? 0m)
                                                - (transfer?.Details.Sum(d => d.QuantityReceived) ?? 0m),
            };
        }

        public static List<Dictionary<string, object?>> FromWarehouseTransferReceivingDetails(
            WarehouseTransferReceivingEntity receiving) =>
            receiving.Details.Select(d => new Dictionary<string, object?>
            {
                ["productCode"]         = d.Product?.code ?? "",
                ["productName"]         = d.Product?.name ?? "",
                ["quantityReceived"]    = d.QuantityReceived,
                ["quantityDispatched"]  = d.WarehouseTransferDetail?.QuantityDispatched ?? 0m,
                ["pendingQuantity"]     = (d.WarehouseTransferDetail?.QuantityDispatched ?? 0m)
                                          - (d.WarehouseTransferDetail?.QuantityReceived ?? 0m),
                ["unitCost"]            = d.WarehouseTransferDetail?.UnitCost ?? 0m,
                ["notes"]               = d.Notes ?? "",
            }).ToList();

        // ─────────────────────────────────────────────
        // Helpers privados
        // ─────────────────────────────────────────────

        private static string TranslateTransferStatus(string status) => status switch
        {
            "Draft"             => "Borrador",
            "Dispatched"        => "Despachado",
            "PartiallyReceived" => "Recibido Parcial",
            "Received"          => "Recibido",
            "Cancelled"         => "Cancelado",
            _                   => status
        };

        private static byte[] GenerateQrPng(string content)
        {
            var generator = new QRCodeGenerator();
            var data      = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
            var qr        = new PngByteQRCode(data);
            return qr.GetGraphic(10);
        }
    }
}
