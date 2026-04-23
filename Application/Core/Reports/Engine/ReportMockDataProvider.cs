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
        private const string SampleQrPngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAFIAAABSAQAAAADvV742AAABGElEQVR4Xu2UPQrCQBBGX0wsLCzsArewEbGwsrOxsLGwsrFIEewt7AQ72AksLHyA1WcQtnYhYJNNJrPJhQf3w8f/5s1nJCn5P4gzhf7TFfQuJ3lJ9s1jwoM6lgYaTf2ppviNQjpW5d9vPz1AKZwtomOzR6Pbi5JKz+QCQ6ybeVIC9tP63UOv7q6f8odQyl9LwD6G4i0n4IY3FXJvKH7g+9h9A6UfVfScCk9m3fAqdyTAVy2Ras5k+sBc2bYycA0hG9y8le4Ha3gRfR3F8db7l6mB2LeHcQw09sx9t9LJkz1PQ1q5JZs9tvV3aFjLPAW2k9FT2X4Q7Q2mnr7oGMxkqPeE6rRmp0rWjWQ3eGmNRL6n+5A3O0VnaD3X8UwAAAABJRU5ErkJggg==";

        public static Dictionary<string, string> GetMockDataRow(string reportType) =>
            reportType switch
            {
                "Sales" or "Delivery" => SalesMockRow(reportType),
                "Quotation"           => QuotationMockRow(),
                "Purchase"            => PurchaseMockRow(),
                "Inventory"           => InventoryMockRow(),
                "CashierShift"        => CashierShiftMockRow(),
                "Invoice"             => InvoiceMockRow(),
                "Payment"             => PaymentMockRow(),
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
                "Payment"             => PaymentMockTableRows(),
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
            ["receivingCode"]   = "REC-00008",
            ["receivingDate"]   = "19/04/2026 09:15",
            ["status"]          = "Enviada",
            ["supplierName"]    = "Distribuidora Nacional S.A.",
            ["supplierTaxId"]   = "DNA850601MNO",
            ["warehouseName"]   = "Almacén Central",
            ["branchName"]      = "Sucursal Centro",
            ["supplierInvoiceNumber"] = "FAC-PROV-8821",
            ["carrierName"]     = "Transportes del Bajío",
            ["trackingNumber"]  = "GUIA-928177",
            ["receivedBy"]      = "Jorge Hernández",
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
                ["quantityReceived"]  = "10",
                ["quantityApproved"]  = "10",
                ["quantityRejected"]  = "0",
                ["unitCost"]          = "$420.00",
                ["lineTotal"]         = "$4,200.00",
            },
            new()
            {
                ["productCode"]       = "COMP-22",
                ["productName"]       = "Disco SSD 500GB",
                ["quantityOrdered"]   = "15",
                ["quantityReceived"]  = "14",
                ["quantityApproved"]  = "13",
                ["quantityRejected"]  = "1",
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
            ["productName"]      = "Laptop Lenovo IdeaPad 15\"",
            ["fromDate"]         = "01/04/2026",
            ["toDate"]           = "18/04/2026",
        };

        private static List<Dictionary<string, string>> InventoryMockTableRows() => new()
        {
            new()
            {
                ["movementDate"]   = "03/04/2026 09:20",
                ["movementCode"]   = "MOV-0001",
                ["movementType"]   = "IN/PURCHASE",
                ["productCode"]    = "PROD-001",
                ["productName"]    = "Laptop Lenovo IdeaPad 15\"",
                ["quantity"]       = "5",
                ["stockBefore"]    = "3",
                ["stockAfter"]     = "8",
                ["unitCost"]       = "$950.00",
                ["totalCost"]      = "$4,750.00",
                ["reference"]      = "REC-0008",
            },
            new()
            {
                ["movementDate"]   = "10/04/2026 11:45",
                ["movementCode"]   = "MOV-0002",
                ["movementType"]   = "OUT/SALE",
                ["productCode"]    = "PROD-001",
                ["productName"]    = "Laptop Lenovo IdeaPad 15\"",
                ["quantity"]       = "2",
                ["stockBefore"]    = "8",
                ["stockAfter"]     = "6",
                ["unitCost"]       = "$950.00",
                ["totalCost"]      = "$1,900.00",
                ["reference"]      = "VTA-0042",
            },
            new()
            {
                ["movementDate"]   = "15/04/2026 16:10",
                ["movementCode"]   = "MOV-0003",
                ["movementType"]   = "IN/ADJUSTMENT",
                ["productCode"]    = "PROD-001",
                ["productName"]    = "Laptop Lenovo IdeaPad 15\"",
                ["quantity"]       = "1",
                ["stockBefore"]    = "6",
                ["stockAfter"]     = "7",
                ["unitCost"]       = "$950.00",
                ["totalCost"]      = "$950.00",
                ["reference"]      = "AJ-0003",
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
            ["invoiceFolio"]             = "A-2",
            ["invoiceSerie"]             = "A",
            ["invoiceDate"]              = "09/04/2026 06:00",
            ["invoiceStatus"]            = "Timbrada",
            ["uuid"]                     = "62be3311-f4ca-4def-bfb4-b36cfacc5a4b",
            ["timbradoAt"]               = "09/04/2026 23:33",
            ["tipoDeComprobante"]        = "Ingreso",
            ["metodoPago"]               = "PPD",
            ["formaPago"]                = "99",
            ["condicionesDePago"]        = "30 días",
            ["moneda"]                   = "MXN",
            ["tipoCambio"]               = "1",
            ["lugarExpedicion"]          = "36257",
            ["companyLogoUrl"]           = "",
            ["companyTradeName"]         = "XOCHILT CASAS CHAVEZ",
            ["emisorRfc"]                = "CACX7605101P8",
            ["emisorNombre"]             = "XOCHILT CASAS CHAVEZ",
            ["emisorRegimenFiscal"]      = "605",
            ["receptorRfc"]              = "XAXX010101000",
            ["receptorNombre"]           = "PUBLICO EN GENERAL",
            ["receptorDomicilioFiscal"]  = "36257",
            ["receptorRegimenFiscal"]    = "616",
            ["receptorUsoCfdi"]          = "S01",
            ["saleCode"]                 = "VTA-0042",
            ["noCertificadoCfdi"]        = "30001000000500003316",
            ["noCertificadoSat"]         = "30001000000500003456",
            ["selloCfdi"]                = "Sm3rEjcD5p0cPTHnUifWJgVpgqfz/Rz9VKIOO3I8PVDTYUZYPJG3RpegSvDmJm0IZw6q8SGIWAKyYR6uJk5PoRqFjieN1gLnh9g20iV...",
            ["selloSat"]                 = "jQjBaiwM5m3u0B0uaZ0v4uQv4e0jTd+FPzihTbGk9sDr0nI4mN8Rliq0vC0k1f0qA4Q+MyNh0vOVPgCpuazxXjvQW1TqGpHkCC58...",
            ["cadenaOriginalSat"]        = "||1.1|62be3311-f4ca-4def-bfb4-b36cfacc5a4b|2026-04-09T23:33:23|SPR190613I52|...||",
            ["qrCode"]                   = SampleQrPngBase64,
            ["subTotal"]                 = "$30,998.50",
            ["discountAmount"]           = "$0.00",
            ["taxAmount"]                = "$4,959.76",
            ["total"]                    = "$35,958.26",
        };

        private static List<Dictionary<string, string>> InvoiceMockTableRows() => new()
        {
            new()
            {
                ["claveProdServ"]    = "43191503",
                ["noIdentificacion"] = "PROD-4042-92",
                ["descripcion"]      = "Apple iPhone 17 Pro Max 256GB",
                ["cantidad"]         = "1",
                ["claveUnidad"]      = "H87",
                ["unidad"]           = "Pieza",
                ["valorUnitario"]    = "$30,998.50",
                ["descuento"]        = "$0.00",
                ["importe"]          = "$30,998.50",
                ["trasladoTasa"]     = "16%",
                ["trasladoImporte"]  = "$4,959.76",
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

        // ─────────────────────────────────────────────
        // COMPLEMENTO DE PAGO
        // ─────────────────────────────────────────────

        private static Dictionary<string, string> PaymentMockRow() => new()
        {
            ["paymentNumber"]           = "PAG-2026-0042",
            ["paymentDate"]             = "18/04/2026",
            ["totalAmount"]             = "$5,800.00",
            ["currency"]                = "MXN",
            ["exchangeRate"]            = "1.00",
            ["paymentFormSAT"]          = "03",
            ["reference"]               = "REF-TRF-88210",
            ["bankOrigin"]              = "BBVA Bancomer",
            ["bankAccountOrigin"]       = "012XXXXXXXX",
            ["bankDestination"]         = "Banamex",
            ["bankAccountDestination"]  = "002XXXXXXXX",
            ["complementSerie"]         = "CP",
            ["complementFolio"]         = "0042",
            ["uuid"]                    = "B2C3D4E5-F6A7-8901-BCDE-F12345678901",
            ["timbradoAt"]              = "18/04/2026 10:36",
            ["companyLogoUrl"]          = "",
            ["emisorRfc"]               = "EDE901231XX9",
            ["emisorNombre"]            = "Empresa Demo S.A. de C.V.",
            ["emisorRegimenFiscal"]     = "601",
            ["lugarExpedicion"]         = "44100",
            ["receptorRfc"]             = "GALM800101ABC",
            ["receptorNombre"]          = "María García López",
            ["receptorDomicilioFiscal"] = "44200",
            ["receptorRegimenFiscal"]   = "612",
            ["receptorUsoCfdi"]         = "CP01",
            ["noCertificadoCfdi"]       = "30001000000500003316",
            ["noCertificadoSat"]        = "30001000000500003456",
            ["selloCfdi"]               = "ANyYyM6yX1d9obB6bf8g4eM6xt4FRPqs6nItJ241V/tOELJWCTXui4aPW38JCmGEw==",
            ["selloSat"]                = "iWIbF+FCrJPlwysUfi9p5fyTKAy+KDd1nAfmPIAy2NS8FeHJVxdxNh2XAXIQ==",
            ["cadenaOriginalSat"]       = "||1.1|B2C3D4E5-F6A7-8901-BCDE-F12345678901|2026-04-18T10:36:00|SPR190613I52|ANyYyM6yX1d9==|30001000000500003456||",
            ["qrCode"]                  = SampleQrPngBase64,
            ["notes"]                   = "",
        };

        private static List<Dictionary<string, string>> PaymentMockTableRows() => new()
        {
            new()
            {
                ["serieAndFolio"]             = "A-00121",
                ["folioUUID"]                 = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
                ["originalInvoiceAmount"]     = "$3,480.00",
                ["partialityNumber"]          = "1",
                ["previousBalance"]           = "$3,480.00",
                ["amountApplied"]             = "$3,480.00",
                ["newBalance"]                = "$0.00",
                ["paymentType"]               = "FullPayment",
            },
            new()
            {
                ["serieAndFolio"]             = "A-00118",
                ["folioUUID"]                 = "C3D4E5F6-A7B8-9012-CDEF-123456789012",
                ["originalInvoiceAmount"]     = "$4,640.00",
                ["partialityNumber"]          = "2",
                ["previousBalance"]           = "$2,320.00",
                ["amountApplied"]             = "$2,320.00",
                ["newBalance"]                = "$0.00",
                ["paymentType"]               = "PartialPayment",
            },
        };
    }
}
