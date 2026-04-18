using Domain.Entities;

namespace Application.Core.Reports.Engine
{
    /// <summary>
    /// Genera datos de ejemplo por tipo de reporte para renderizar el preview
    /// en el frontend sin necesidad de consultar documentos reales.
    /// Los valores están formateados como string listo para mostrar.
    /// </summary>
    public static class ReportMockDataProvider
    {
        public static Dictionary<string, string> GetMockDataRow(string reportType) =>
            reportType switch
            {
                "Sales" or "Delivery" => SalesMockRow(reportType),
                "Quotation"           => QuotationMockRow(),
                "Purchase"            => PurchaseMockRow(),
                "Inventory"           => InventoryMockRow(),
                "CashierShift"        => CashierShiftMockRow(),
                "Invoice"             => InvoiceMockRow(),
                _                     => new()
            };

        public static List<Dictionary<string, string>> GetMockTableRows(string reportType) =>
            reportType switch
            {
                "Sales" or "Delivery" => SalesMockTableRows(),
                "Quotation"           => QuotationMockTableRows(),
                "Purchase"            => PurchaseMockTableRows(),
                "Inventory"           => InventoryMockTableRows(),
                "CashierShift"        => CashierShiftMockTableRows(),
                "Invoice"             => InvoiceMockTableRows(),
                _                     => new()
            };

        // ─────────────────────────────────────────────
        // VENTAS / DELIVERY
        // ─────────────────────────────────────────────

        private static Dictionary<string, string> SalesMockRow(string type) => new()
        {
            ["saleCode"]           = "VTA-00042",
            ["saleDate"]           = "18/04/2026 10:35",
            ["saleType"]           = type == "Delivery" ? "Entrega a domicilio" : "Punto de venta",
            ["status"]             = "Completada",
            ["customerName"]       = "María García López",
            ["customerCode"]       = "CLI-0012",
            ["customerTaxId"]      = "GALM800101ABC",
            ["sellerName"]         = "Carlos Ramírez",
            ["warehouseName"]      = "Almacén Central",
            ["branchName"]         = "Sucursal Centro",
            ["companyName"]        = "Empresa Demo S.A. de C.V.",
            ["deliveryAddress"]    = "Av. Juárez 123, Col. Centro, Guadalajara, Jal.",
            ["scheduledDate"]      = "20/04/2026",
            ["deliveredAt"]        = "20/04/2026 14:20",
            ["notes"]              = "Entregar en horario matutino",
            ["totalSubtotal"]      = "$1,250.00",
            ["totalDiscount"]      = "$62.50",
            ["totalTax"]           = "$190.00",
            ["totalAmount"]        = "$1,377.50",
            ["amountPaid"]         = "$1,400.00",
            ["changeAmount"]       = "$22.50",
            ["paymentMethods"]     = "Efectivo",
        };

        private static List<Dictionary<string, string>> SalesMockTableRows() => new()
        {
            new()
            {
                ["productCode"]         = "PROD-001",
                ["productName"]         = "Laptop Lenovo IdeaPad 15\"",
                ["quantity"]            = "1",
                ["unitPrice"]           = "$950.00",
                ["discountPercentage"]  = "5%",
                ["discountAmount"]      = "$47.50",
                ["taxPercentage"]       = "16%",
                ["taxAmount"]           = "$144.40",
                ["subtotal"]            = "$902.50",
                ["lineTotal"]           = "$1,046.90",
            },
            new()
            {
                ["productCode"]         = "PROD-017",
                ["productName"]         = "Mouse inalámbrico Logitech",
                ["quantity"]            = "2",
                ["unitPrice"]           = "$175.00",
                ["discountPercentage"]  = "0%",
                ["discountAmount"]      = "$0.00",
                ["taxPercentage"]       = "16%",
                ["taxAmount"]           = "$56.00",
                ["subtotal"]            = "$350.00",
                ["lineTotal"]           = "$406.00",
            },
            new()
            {
                ["productCode"]         = "PROD-043",
                ["productName"]         = "Teclado USB compacto",
                ["quantity"]            = "1",
                ["unitPrice"]           = "$125.00",
                ["discountPercentage"]  = "0%",
                ["discountAmount"]      = "$0.00",
                ["taxPercentage"]       = "16%",
                ["taxAmount"]           = "$20.00",
                ["subtotal"]            = "$125.00",
                ["lineTotal"]           = "$145.00",
            },
        };

        // ─────────────────────────────────────────────
        // COTIZACIONES
        // ─────────────────────────────────────────────

        private static Dictionary<string, string> QuotationMockRow() => new()
        {
            ["quotationCode"]      = "COT-00015",
            ["quotationDate"]      = "18/04/2026 09:00",
            ["validUntil"]         = "25/04/2026",
            ["status"]             = "Pendiente",
            ["customerName"]       = "Empresa Constructora ABC",
            ["customerTaxId"]      = "ECA900101XYZ",
            ["sellerName"]         = "Laura Sánchez",
            ["warehouseName"]      = "Almacén Norte",
            ["branchName"]         = "Sucursal Norte",
            ["companyName"]        = "Empresa Demo S.A. de C.V.",
            ["notes"]              = "Cotización válida por 7 días hábiles",
            ["convertedSaleCode"]  = "",
            ["totalSubtotal"]      = "$4,500.00",
            ["totalDiscount"]      = "$225.00",
            ["totalTax"]           = "$675.00",
            ["totalAmount"]        = "$4,950.00",
        };

        private static List<Dictionary<string, string>> QuotationMockTableRows() => new()
        {
            new()
            {
                ["productCode"]         = "MAT-201",
                ["productName"]         = "Cemento Portland 50kg",
                ["quantity"]            = "100",
                ["unitPrice"]           = "$18.00",
                ["discountPercentage"]  = "5%",
                ["discountAmount"]      = "$90.00",
                ["taxPercentage"]       = "16%",
                ["taxAmount"]           = "$273.60",
                ["lineTotal"]           = "$1,983.60",
            },
            new()
            {
                ["productCode"]         = "MAT-312",
                ["productName"]         = "Varilla de acero 3/8\" × 12m",
                ["quantity"]            = "50",
                ["unitPrice"]           = "$58.00",
                ["discountPercentage"]  = "5%",
                ["discountAmount"]      = "$145.00",
                ["taxPercentage"]       = "16%",
                ["taxAmount"]           = "$437.60",
                ["lineTotal"]           = "$3,167.60",
            },
        };

        // ─────────────────────────────────────────────
        // ÓRDENES DE COMPRA
        // ─────────────────────────────────────────────

        private static Dictionary<string, string> PurchaseMockRow() => new()
        {
            ["poCode"]          = "OC-00008",
            ["poDate"]          = "18/04/2026 08:00",
            ["status"]          = "Enviada",
            ["supplierName"]    = "Distribuidora Nacional S.A.",
            ["supplierTaxId"]   = "DNA850601MNO",
            ["warehouseName"]   = "Almacén Central",
            ["branchName"]      = "Sucursal Centro",
            ["notes"]           = "Entrega en 3-5 días hábiles",
            ["totalAmount"]     = "$8,750.00",
        };

        private static List<Dictionary<string, string>> PurchaseMockTableRows() => new()
        {
            new()
            {
                ["productCode"]       = "COMP-10",
                ["productName"]       = "Pantalla LED 24\" Full HD",
                ["quantityOrdered"]   = "10",
                ["quantityReceived"]  = "0",
                ["unitCost"]          = "$420.00",
                ["lineTotal"]         = "$4,200.00",
            },
            new()
            {
                ["productCode"]       = "COMP-22",
                ["productName"]       = "Disco SSD 500GB",
                ["quantityOrdered"]   = "15",
                ["quantityReceived"]  = "0",
                ["unitCost"]          = "$302.00",
                ["lineTotal"]         = "$4,530.00",
            },
        };

        // ─────────────────────────────────────────────
        // INVENTARIO
        // ─────────────────────────────────────────────

        private static Dictionary<string, string> InventoryMockRow() => new()
        {
            ["reportDate"]       = "18/04/2026 12:00",
            ["warehouseName"]    = "Almacén Central",
            ["companyName"]      = "Empresa Demo S.A. de C.V.",
            ["totalProducts"]    = "248",
            ["belowMinimum"]     = "12",
            ["totalStockValue"]  = "$384,200.00",
        };

        private static List<Dictionary<string, string>> InventoryMockTableRows() => new()
        {
            new()
            {
                ["productCode"]    = "PROD-001",
                ["productName"]    = "Laptop Lenovo IdeaPad 15\"",
                ["category"]       = "Electrónica",
                ["currentStock"]   = "8",
                ["minimumStock"]   = "5",
                ["unitCost"]       = "$950.00",
                ["totalValue"]     = "$7,600.00",
                ["lastMovement"]   = "17/04/2026",
            },
            new()
            {
                ["productCode"]    = "PROD-017",
                ["productName"]    = "Mouse inalámbrico Logitech",
                ["category"]       = "Periféricos",
                ["currentStock"]   = "3",
                ["minimumStock"]   = "10",
                ["unitCost"]       = "$175.00",
                ["totalValue"]     = "$525.00",
                ["lastMovement"]   = "15/04/2026",
            },
            new()
            {
                ["productCode"]    = "PROD-043",
                ["productName"]    = "Teclado USB compacto",
                ["category"]       = "Periféricos",
                ["currentStock"]   = "22",
                ["minimumStock"]   = "10",
                ["unitCost"]       = "$125.00",
                ["totalValue"]     = "$2,750.00",
                ["lastMovement"]   = "10/04/2026",
            },
        };

        // ─────────────────────────────────────────────
        // TURNO DE CAJERO
        // ─────────────────────────────────────────────

        private static Dictionary<string, string> CashierShiftMockRow() => new()
        {
            ["shiftCode"]        = "TRN-20260418-01",
            ["cashierName"]      = "Ana Martínez",
            ["openedAt"]         = "18/04/2026 08:00",
            ["closedAt"]         = "18/04/2026 16:00",
            ["warehouseName"]    = "Caja 1 - Planta Baja",
            ["openingCash"]      = "$500.00",
            ["closingCash"]      = "$2,830.00",
            ["difference"]       = "$0.00",
            ["cashTotal"]        = "$1,450.00",
            ["cardTotal"]        = "$780.00",
            ["transferTotal"]    = "$100.00",
            ["totalSales"]       = "$2,330.00",
            ["salesCount"]       = "18",
        };

        private static List<Dictionary<string, string>> CashierShiftMockTableRows() => new()
        {
            new()
            {
                ["saleCode"]       = "VTA-00040",
                ["saleTime"]       = "08:45",
                ["customerName"]   = "Público general",
                ["paymentMethod"]  = "Efectivo",
                ["saleTotal"]      = "$320.00",
            },
            new()
            {
                ["saleCode"]       = "VTA-00041",
                ["saleTime"]       = "09:12",
                ["customerName"]   = "Roberto Flores",
                ["paymentMethod"]  = "Tarjeta",
                ["saleTotal"]      = "$780.00",
            },
            new()
            {
                ["saleCode"]       = "VTA-00042",
                ["saleTime"]       = "10:35",
                ["customerName"]   = "María García López",
                ["paymentMethod"]  = "Efectivo",
                ["saleTotal"]      = "$1,377.50",
            },
        };

        // ─────────────────────────────────────────────
        // FACTURA CFDI
        // ─────────────────────────────────────────────

        private static Dictionary<string, string> InvoiceMockRow() => new()
        {
            ["invoiceFolio"]             = "1001",
            ["invoiceSerie"]             = "A",
            ["invoiceDate"]              = "18/04/2026 10:35",
            ["invoiceStatus"]            = "Timbrada",
            ["uuid"]                     = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
            ["timbradoAt"]               = "18/04/2026 10:36",
            ["tipoDeComprobante"]        = "I - Ingreso",
            ["metodoPago"]               = "PUE - Pago en una sola exhibición",
            ["formaPago"]                = "01 - Efectivo",
            ["condicionesDePago"]        = "Contado",
            ["moneda"]                   = "MXN",
            ["lugarExpedicion"]          = "44100",
            ["emisorRfc"]                = "EDE901231XX9",
            ["emisorNombre"]             = "Empresa Demo S.A. de C.V.",
            ["emisorRegimenFiscal"]      = "601 - General de Ley Personas Morales",
            ["receptorRfc"]              = "GALM800101ABC",
            ["receptorNombre"]           = "María García López",
            ["receptorDomicilioFiscal"]  = "44200",
            ["receptorRegimenFiscal"]    = "612 - Personas Físicas con Actividades Empresariales",
            ["receptorUsoCfdi"]          = "G03 - Gastos en general",
            ["saleCode"]                 = "VTA-00042",
            ["subTotal"]                 = "$1,250.00",
            ["discountAmount"]           = "$62.50",
            ["taxAmount"]                = "$190.00",
            ["total"]                    = "$1,377.50",
        };

        private static List<Dictionary<string, string>> InvoiceMockTableRows() => new()
        {
            new()
            {
                ["claveProdServ"]    = "43211503",
                ["noIdentificacion"] = "PROD-001",
                ["descripcion"]      = "Laptop Lenovo IdeaPad 15\"",
                ["cantidad"]         = "1",
                ["claveUnidad"]      = "H87",
                ["unidad"]           = "Pieza",
                ["valorUnitario"]    = "$950.00",
                ["descuento"]        = "$47.50",
                ["importe"]          = "$902.50",
                ["trasladoTasa"]     = "16%",
                ["trasladoImporte"]  = "$144.40",
            },
            new()
            {
                ["claveProdServ"]    = "43211706",
                ["noIdentificacion"] = "PROD-017",
                ["descripcion"]      = "Mouse inalámbrico Logitech",
                ["cantidad"]         = "2",
                ["claveUnidad"]      = "H87",
                ["unidad"]           = "Pieza",
                ["valorUnitario"]    = "$175.00",
                ["descuento"]        = "$0.00",
                ["importe"]          = "$350.00",
                ["trasladoTasa"]     = "16%",
                ["trasladoImporte"]  = "$56.00",
            },
            new()
            {
                ["claveProdServ"]    = "43211706",
                ["noIdentificacion"] = "PROD-043",
                ["descripcion"]      = "Teclado USB compacto",
                ["cantidad"]         = "1",
                ["claveUnidad"]      = "H87",
                ["unidad"]           = "Pieza",
                ["valorUnitario"]    = "$125.00",
                ["descuento"]        = "$0.00",
                ["importe"]          = "$125.00",
                ["trasladoTasa"]     = "16%",
                ["trasladoImporte"]  = "$20.00",
            },
        };

        // ─────────────────────────────────────────────
        // MOCK INVOICE ENTITY (para InvoicePdfDocument)
        // ─────────────────────────────────────────────

        public static Invoice GetMockInvoice() => new()
        {
            Id               = 0,
            CompanyId        = 0,
            Serie            = "A",
            Folio            = "1001",
            InvoiceDate      = new DateTime(2026, 4, 18, 10, 35, 0),
            FormaPago        = "01",
            MetodoPago       = "PUE",
            CondicionesDePago = "Contado",
            TipoDeComprobante = "I",
            LugarExpedicion  = "44100",
            Moneda           = "MXN",
            TipoCambio       = 1m,
            Status           = "Timbrada",
            // Emisor
            EmisorRfc            = "EDE901231XX9",
            EmisorNombre         = "Empresa Demo S.A. de C.V.",
            EmisorRegimenFiscal  = "601",
            // Receptor
            ReceptorRfc             = "GALM800101ABC",
            ReceptorNombre          = "María García López",
            ReceptorDomicilioFiscal = "44200",
            ReceptorRegimenFiscal   = "612",
            ReceptorUsoCfdi         = "G03",
            // Montos
            SubTotal       = 1250.00m,
            DiscountAmount = 62.50m,
            TaxAmount      = 190.00m,
            Total          = 1377.50m,
            // Timbrado
            Uuid              = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
            TimbradoAt        = new DateTime(2026, 4, 18, 10, 36, 0),
            NoCertificadoCfdi = "30001000000500003316",
            NoCertificadoSat  = "30001000000500003456",
            SelloCfdi         = "ANyYyM6yX1d9obB6bf8g4eM6xt4FRPqs6nItJ241V/tOELJWCTXui4aPW38JCmGEwKxFcADUgvf2Y5vSADRB8bImRVfJPJc3EGQgkXVvYJYW6MKl1Em8oiN1pyS9CMtpKMpOCewNRWbH5mpk46S+ELm0lhLY9NoQBneKK17PQU==",
            SelloSat          = "iWIbF+FCrJPlwysUfi9p5fyTKAy+KDd1nAfmPIAy2NS8FeHJVxdxNh2XAXIQGplTOGmeBuQOOmxaUqt9lEJzQoqzxVk/wBEFD5vaZe/pB3RuofVGyqBzImaqQ9/JWhWFmIwt6g8sjfOqTunyhZ+Sv==",
            CadenaOriginalSat = "||1.1|A1B2C3D4-E5F6-7890-ABCD-EF1234567890|2026-04-18T10:36:00|SPR190613I52|ANyYyM6yX1d9obB6bf8g4eM6xt4FRPqs6nItJ241V/tOELJWCTXui4aPW38JCmGEw==|30001000000500003456||",
            QrCode            = null,
            Details = new()
            {
                new() { ClaveProdServ = "43211503", NoIdentificacion = "PROD-001", Descripcion = "Laptop Lenovo IdeaPad 15\"", Cantidad = 1m, ClaveUnidad = "H87", Unidad = "Pieza", ValorUnitario = 950.00m, Descuento = 47.50m, Importe = 902.50m, TieneTraslados = true, TrasladoImporte = 144.40m },
                new() { ClaveProdServ = "43211706", NoIdentificacion = "PROD-017", Descripcion = "Mouse inalámbrico Logitech",    Cantidad = 2m, ClaveUnidad = "H87", Unidad = "Pieza", ValorUnitario = 175.00m, Descuento = 0m,    Importe = 350.00m, TieneTraslados = true, TrasladoImporte = 56.00m  },
                new() { ClaveProdServ = "43211706", NoIdentificacion = "PROD-043", Descripcion = "Teclado USB compacto",         Cantidad = 1m, ClaveUnidad = "H87", Unidad = "Pieza", ValorUnitario = 125.00m, Descuento = 0m,    Importe = 125.00m, TieneTraslados = true, TrasladoImporte = 20.00m  },
            },
        };
    }
}
