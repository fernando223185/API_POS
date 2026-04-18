using Application.DTOs.Quotations;
using Domain.Entities;

namespace Application.Core.Quotations
{
    public static class QuotationMapper
    {
        public static QuotationResponseDto ToResponseDto(Quotation q)
        {
            return new QuotationResponseDto
            {
                Id = q.Id,
                Code = q.Code,
                QuotationDate = q.QuotationDate,
                ValidUntil = q.ValidUntil,
                CustomerId = q.CustomerId,
                CustomerName = q.CustomerName,
                WarehouseId = q.WarehouseId,
                WarehouseName = q.Warehouse?.Name ?? string.Empty,
                BranchId = q.BranchId,
                BranchName = q.Branch?.Name,
                CompanyId = q.CompanyId,
                CompanyName = q.Company?.LegalName,
                UserId = q.UserId,
                UserName = q.User?.Name ?? string.Empty,
                PriceListId = q.PriceListId,
                PriceListName = q.PriceList?.Name,
                SubTotal = q.SubTotal,
                DiscountAmount = q.DiscountAmount,
                DiscountPercentage = q.DiscountPercentage,
                TaxAmount = q.TaxAmount,
                Total = q.Total,
                Status = q.Status,
                RequiresInvoice = q.RequiresInvoice,
                SaleId = q.SaleId,
                SaleCode = q.Sale?.Code,
                ConvertedAt = q.ConvertedAt,
                Notes = q.Notes,
                CreatedAt = q.CreatedAt,
                CancelledAt = q.CancelledAt,
                CancellationReason = q.CancellationReason,
                TotalItems = q.Details?.Count ?? 0,
                TotalQuantity = q.Details?.Sum(d => d.Quantity) ?? 0,
                Details = q.Details?.Select(d => new QuotationDetailResponseDto
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
                    Notes = d.Notes
                }).ToList() ?? new()
            };
        }

        public static QuotationSummaryDto ToSummaryDto(Quotation q)
        {
            return new QuotationSummaryDto
            {
                Id = q.Id,
                Code = q.Code,
                QuotationDate = q.QuotationDate,
                ValidUntil = q.ValidUntil,
                CustomerName = q.CustomerName,
                WarehouseName = q.Warehouse?.Name ?? string.Empty,
                BranchName = q.Branch?.Name,
                Total = q.Total,
                Status = q.Status,
                RequiresInvoice = q.RequiresInvoice,
                TotalItems = q.Details?.Count ?? 0,
                UserName = q.User?.Name ?? string.Empty,
                SaleId = q.SaleId,
                SaleCode = q.Sale?.Code
            };
        }
    }
}
