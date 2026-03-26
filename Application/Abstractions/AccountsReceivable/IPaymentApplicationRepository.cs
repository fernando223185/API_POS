using Domain.Entities;

namespace Application.Abstractions.AccountsReceivable;

/// <summary>
/// Interfaz del repositorio de Aplicaciones de Pago
/// </summary>
public interface IPaymentApplicationRepository
{
    Task<PaymentApplication> CreateAsync(PaymentApplication application);
    Task<PaymentApplication?> GetByIdAsync(int id);
    Task<List<PaymentApplication>> GetByPaymentIdAsync(int paymentId);
    Task<List<PaymentApplication>> GetByInvoiceIdAsync(int invoicePPDId);
    Task<PaymentApplication> UpdateAsync(PaymentApplication application);
    Task<int> CountPendingComplementsAsync(int paymentId);
}
