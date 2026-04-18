using Application.DTOs.Sales;
using Domain.Entities;

namespace Application.Core.Sales
{
    /// <summary>
    /// Mapper compartido para convertir Sale entity a SaleResponseDto
    /// </summary>
    internal static class SaleMapper
    {
        internal static SaleResponseDto ToResponseDto(Sale s)
        {
            var totalCost = s.Details.Sum(d => d.TotalCost ?? 0);
            var grossProfit = s.Total - totalCost;
            var profitMargin = s.Total > 0 ? (grossProfit / s.Total) * 100 : 0;

            return new SaleResponseDto
            {
                Id = s.Id,
                Code = s.Code,
                SaleDate = s.SaleDate,
                CustomerId = s.CustomerId,
                CustomerName = s.CustomerName,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse?.Name ?? string.Empty,
                BranchId = s.BranchId,
                BranchName = s.Branch?.Name,
                CompanyId = s.CompanyId,
                CompanyName = s.Company?.LegalName,
                UserId = s.UserId,
                UserName = s.User?.Name ?? string.Empty,
                PriceListId = s.PriceListId,
                PriceListName = s.PriceList?.Name,
                SubTotal = s.SubTotal,
                DiscountAmount = s.DiscountAmount,
                DiscountPercentage = s.DiscountPercentage,
                TaxAmount = s.TaxAmount,
                Total = s.Total,
                AmountPaid = s.AmountPaid,
                ChangeAmount = s.ChangeAmount,
                IsPaid = s.IsPaid,
                Status = s.Status,
                IsPostedToInventory = s.IsPostedToInventory,
                PostedToInventoryDate = s.PostedToInventoryDate,
                RequiresInvoice = s.RequiresInvoice,
                InvoiceUuid = s.InvoiceUuid,
                SaleType = s.SaleType,
                DeliveryAddress = s.DeliveryAddress,
                ScheduledDeliveryDate = s.ScheduledDeliveryDate,
                DeliveredAt = s.DeliveredAt,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                CreatedByName = s.CreatedBy?.Name,
                CancelledAt = s.CancelledAt,
                CancellationReason = s.CancellationReason,
                Details = s.Details.Select(d => new SaleDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    ProductName = d.ProductName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    DiscountPercentage = d.DiscountPercentage,
                    DiscountAmount = d.DiscountAmount,
                    TaxPercentage = d.TaxPercentage,
                    TaxAmount = d.TaxAmount,
                    SubTotal = d.SubTotal,
                    Total = d.Total,
                    UnitCost = d.UnitCost,
                    TotalCost = d.TotalCost,
                    LineProfit = d.Total - (d.TotalCost ?? 0),
                    Notes = d.Notes,
                    SerialNumber = d.SerialNumber,
                    LotNumber = d.LotNumber
                }).ToList(),
                Payments = s.Payments.Select(p => new SalePaymentResponseDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    CardNumber = p.CardNumber,
                    CardType = p.CardType,
                    AuthorizationCode = p.AuthorizationCode,
                    TransactionReference = p.TransactionReference,
                    TerminalId = p.TerminalId,
                    BankName = p.BankName,
                    TransferReference = p.TransferReference,
                    CheckNumber = p.CheckNumber,
                    CheckBank = p.CheckBank,
                    Status = p.Status,
                    Notes = p.Notes
                }).ToList(),
                TotalItems = s.Details.Count,
                TotalQuantity = s.Details.Sum(d => d.Quantity),
                TotalCost = totalCost,
                GrossProfit = grossProfit,
                ProfitMarginPercentage = profitMargin
            };
        }
    }
}
