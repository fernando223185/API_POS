namespace Application.DTOs.Inventory
{
    // ─── Request DTOs ────────────────────────────────────────────────────────────

    public class CreateStockTransferDto
    {
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int? CompanyId { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public List<CreateStockTransferDetailDto> Details { get; set; } = new();
    }

    public class CreateStockTransferDetailDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateStockTransferDto
    {
        public DateTime TransferDate { get; set; }
        public string? Notes { get; set; }
        public List<CreateStockTransferDetailDto> Details { get; set; } = new();
    }

    // ─── Response DTOs ───────────────────────────────────────────────────────────

    public class StockTransferResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int SourceWarehouseId { get; set; }
        public string SourceWarehouseName { get; set; } = string.Empty;
        public string SourceWarehouseCode { get; set; } = string.Empty;

        public int DestinationWarehouseId { get; set; }
        public string DestinationWarehouseName { get; set; } = string.Empty;
        public string DestinationWarehouseCode { get; set; } = string.Empty;

        public int? CompanyId { get; set; }
        public DateTime TransferDate { get; set; }
        public string? Notes { get; set; }

        public bool IsApplied { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? AppliedByUserName { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }

        public List<StockTransferDetailResponseDto> Details { get; set; } = new();
        public int TotalProducts { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class StockTransferDetailResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? Notes { get; set; }
    }

    public class StockTransferListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SourceWarehouseName { get; set; } = string.Empty;
        public string DestinationWarehouseName { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public bool IsApplied { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class ApplyStockTransferResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public ApplyStockTransferDataDto? Data { get; set; }
    }

    public class ApplyStockTransferDataDto
    {
        public int TransferId { get; set; }
        public string TransferCode { get; set; } = string.Empty;
        public int TotalMovementsCreated { get; set; }
        public List<TransferMovementSummaryDto> Movements { get; set; } = new();
    }

    public class TransferMovementSummaryDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string OutMovementCode { get; set; } = string.Empty;
        public string InMovementCode { get; set; } = string.Empty;
        public decimal SourceStockBefore { get; set; }
        public decimal SourceStockAfter { get; set; }
        public decimal DestinationStockBefore { get; set; }
        public decimal DestinationStockAfter { get; set; }
    }

    public class PagedStockTransferResponseDto
    {
        public List<StockTransferListItemDto> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
