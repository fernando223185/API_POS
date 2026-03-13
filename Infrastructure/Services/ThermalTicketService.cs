using Application.Abstractions.Documents;
using Application.Abstractions.Sales;
using System.Text;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio para generar tickets de venta en formato térmico (ESC/POS)
    /// Compatible con impresoras de 58mm (32 chars) y 80mm (48 chars)
    /// </summary>
    public class ThermalTicketService : IThermalTicketService
    {
        private readonly ISaleRepository _saleRepository;

        public ThermalTicketService(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        /// <summary>
        /// Genera un ticket de venta en formato texto plano
        /// </summary>
        public async Task<string> GenerateSaleTicketAsync(int saleId, int width = 48)
        {
            var sale = await _saleRepository.GetByIdAsync(saleId);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Venta con ID {saleId} no encontrada");
            }

            var sb = new StringBuilder();

            // Encabezado
            sb.AppendLine(CenterText("*** TICKET DE VENTA ***", width));
            sb.AppendLine(CenterText("EXPANDA ERP", width));
            sb.AppendLine(CenterText("Sistema de Punto de Venta", width));
            sb.AppendLine(new string('=', width));
            sb.AppendLine();

            // Información de la venta
            sb.AppendLine($"Folio: {sale.Code}");
            sb.AppendLine($"Fecha: {sale.SaleDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Almacén: {sale.Warehouse?.Name ?? "N/A"}");
            sb.AppendLine($"Vendedor: {sale.User?.Name ?? "N/A"}");
            
            if (!string.IsNullOrEmpty(sale.CustomerName))
            {
                sb.AppendLine($"Cliente: {sale.CustomerName}");
            }
            
            sb.AppendLine(new string('-', width));
            sb.AppendLine();

            // Productos
            sb.AppendLine("PRODUCTOS:");
            sb.AppendLine(new string('-', width));
            
            foreach (var detail in sale.Details.OrderBy(d => d.Id))
            {
                // Nombre del producto
                sb.AppendLine($"{detail.ProductName}");
                
                // Cantidad, precio unitario y total
                var quantityText = $"{detail.Quantity:F2} x ${detail.UnitPrice:F2}";
                var totalText = $"${detail.Total:F2}";
                var spacesNeeded = width - quantityText.Length - totalText.Length;
                sb.AppendLine($"{quantityText}{new string(' ', spacesNeeded)}{totalText}");
                
                // Descuento si existe
                if (detail.DiscountAmount > 0)
                {
                    sb.AppendLine($"  Descuento ({detail.DiscountPercentage:F2}%): -${detail.DiscountAmount:F2}");
                }
            }
            
            sb.AppendLine(new string('-', width));
            sb.AppendLine();

            // Totales
            sb.AppendLine(FormatAmountLine("SUBTOTAL:", sale.SubTotal, width));
            
            if (sale.DiscountAmount > 0)
            {
                sb.AppendLine(FormatAmountLine($"Descuento ({sale.DiscountPercentage:F2}%):", -sale.DiscountAmount, width));
            }
            
            if (sale.TaxAmount > 0)
            {
                sb.AppendLine(FormatAmountLine("IVA (16%):", sale.TaxAmount, width));
            }
            
            sb.AppendLine(new string('=', width));
            sb.AppendLine(FormatAmountLine("TOTAL:", sale.Total, width, true));
            sb.AppendLine(new string('=', width));
            sb.AppendLine();

            // Pagos
            if (sale.Payments.Any())
            {
                sb.AppendLine("FORMA DE PAGO:");
                sb.AppendLine(new string('-', width));
                
                foreach (var payment in sale.Payments)
                {
                    var methodName = FormatPaymentMethod(payment.PaymentMethod);
                    sb.AppendLine(FormatAmountLine(methodName, payment.Amount, width));
                    
                    // Información adicional de tarjeta
                    if (!string.IsNullOrEmpty(payment.CardNumber))
                    {
                        sb.AppendLine($"  Tarjeta: ****{payment.CardNumber}");
                    }
                    if (!string.IsNullOrEmpty(payment.AuthorizationCode))
                    {
                        sb.AppendLine($"  Autorización: {payment.AuthorizationCode}");
                    }
                }
                
                sb.AppendLine(new string('-', width));
                sb.AppendLine(FormatAmountLine("Total Pagado:", sale.AmountPaid, width));
                
                if (sale.ChangeAmount > 0)
                {
                    sb.AppendLine(FormatAmountLine("CAMBIO:", sale.ChangeAmount, width, true));
                }
                
                sb.AppendLine();
            }

            // Pie de página
            sb.AppendLine(new string('=', width));
            sb.AppendLine(CenterText("¡GRACIAS POR SU COMPRA!", width));
            sb.AppendLine(CenterText("Conserve este ticket", width));
            
            if (sale.RequiresInvoice)
            {
                sb.AppendLine();
                sb.AppendLine(CenterText("** PENDIENTE DE FACTURA **", width));
            }
            
            sb.AppendLine(new string('=', width));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Genera un ticket de venta en formato binario con comandos ESC/POS
        /// </summary>
        public async Task<byte[]> GenerateSaleTicketBinaryAsync(int saleId, int width = 48)
        {
            var textContent = await GenerateSaleTicketAsync(saleId, width);
            
            var commands = new List<byte>();

            // Comandos ESC/POS
            byte ESC = 0x1B;
            byte GS = 0x1D;

            // Inicializar impresora
            commands.AddRange(new byte[] { ESC, 0x40 }); // ESC @

            // Establecer alineación al centro para el encabezado
            commands.AddRange(new byte[] { ESC, 0x61, 0x01 }); // ESC a 1

            // Texto en negrita para el título
            commands.AddRange(new byte[] { ESC, 0x45, 0x01 }); // ESC E 1
            
            // Agregar el contenido del ticket
            commands.AddRange(Encoding.UTF8.GetBytes(textContent));

            // Restablecer estilos
            commands.AddRange(new byte[] { ESC, 0x45, 0x00 }); // ESC E 0 (negrita off)
            commands.AddRange(new byte[] { ESC, 0x61, 0x00 }); // ESC a 0 (alineación izquierda)

            // Avanzar papel (6 líneas)
            commands.AddRange(new byte[] { ESC, 0x64, 0x06 }); // ESC d 6

            // Cortar papel (si la impresora lo soporta)
            commands.AddRange(new byte[] { GS, 0x56, 0x00 }); // GS V 0

            // Abrir cajón (si está conectado) - Pulso en pin 2
            commands.AddRange(new byte[] { ESC, 0x70, 0x00, 0x32, 0x32 }); // ESC p 0 50 50

            return commands.ToArray();
        }

        #region Helper Methods

        /// <summary>
        /// Centra un texto en una línea
        /// </summary>
        private string CenterText(string text, int width)
        {
            if (text.Length >= width) return text;
            
            var padding = (width - text.Length) / 2;
            return new string(' ', padding) + text;
        }

        /// <summary>
        /// Formatea una línea con descripción y monto alineado a la derecha
        /// </summary>
        private string FormatAmountLine(string label, decimal amount, int width, bool bold = false)
        {
            var amountText = $"${amount:F2}";
            var spacesNeeded = width - label.Length - amountText.Length;
            
            if (spacesNeeded < 1) spacesNeeded = 1;
            
            return $"{label}{new string(' ', spacesNeeded)}{amountText}";
        }

        /// <summary>
        /// Formatea el nombre del método de pago
        /// </summary>
        private string FormatPaymentMethod(string method)
        {
            return method switch
            {
                "Cash" => "Efectivo",
                "CreditCard" => "Tarjeta Crédito",
                "DebitCard" => "Tarjeta Débito",
                "Transfer" => "Transferencia",
                "Check" => "Cheque",
                _ => method
            };
        }

        #endregion
    }
}
