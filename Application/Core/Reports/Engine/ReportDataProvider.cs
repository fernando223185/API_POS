using Domain.Entities;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

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
    }
}
