namespace Application.DTOs.Inventory
{
    // ─── Request DTOs ─────────────────────────────────────────────────────────

    public class CreateWarehouseTransferDto
    {
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int? CompanyId { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public List<CreateWarehouseTransferDetailDto> Details { get; set; } = new();
    }

    public class CreateWarehouseTransferDetailDto
    {
        public int ProductId { get; set; }
        public decimal QuantityRequested { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateWarehouseTransferDto
    {
        public DateTime TransferDate { get; set; }
        public string? Notes { get; set; }
        public List<CreateWarehouseTransferDetailDto> Details { get; set; } = new();
    }

    public class DispatchWarehouseTransferDto
    {
        /// <summary>Nota adicional al momento del despacho.</summary>
        public string? Notes { get; set; }
    }

    public class CreateWarehouseTransferReceivingDto
    {
        public DateTime ReceivingDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public List<CreateWarehouseTransferReceivingDetailDto> Details { get; set; } = new();
    }

    public class CreateWarehouseTransferReceivingDetailDto
    {
        /// <summary>ID del detalle de la orden de traspaso (WarehouseTransferDetail.Id)</summary>
        public int WarehouseTransferDetailId { get; set; }
        public decimal QuantityReceived { get; set; }
        public string? Notes { get; set; }
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    public class WarehouseTransferResponseDto
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

        public DateTime? DispatchedAt { get; set; }
        public string? DispatchedByUserName { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }

        public List<WarehouseTransferDetailResponseDto> Details { get; set; } = new();
        public List<WarehouseTransferReceivingListItemDto> Receivings { get; set; } = new();

        public int TotalProducts { get; set; }
        public decimal TotalQuantityRequested { get; set; }
        public decimal TotalQuantityDispatched { get; set; }
        public decimal TotalQuantityReceived { get; set; }
        public decimal TotalPendingQuantity { get; set; }
    }

    public class WarehouseTransferDetailResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityRequested { get; set; }
        public decimal QuantityDispatched { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal PendingQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? Notes { get; set; }
    }

    public class WarehouseTransferReceivingListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime ReceivingDate { get; set; }
        public string ReceivingType { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public decimal TotalQuantityReceived { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
    }

    public class WarehouseTransferReceivingResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;

        public int WarehouseTransferId { get; set; }
        public string WarehouseTransferCode { get; set; } = string.Empty;

        public int DestinationWarehouseId { get; set; }
        public string DestinationWarehouseName { get; set; } = string.Empty;

        public DateTime ReceivingDate { get; set; }
        public string ReceivingType { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }

        public List<WarehouseTransferReceivingDetailResponseDto> Details { get; set; } = new();
        public List<WarehouseTransferReceivingMovementDto> Movements { get; set; } = new();

        public int TotalProducts { get; set; }
        public decimal TotalQuantityReceived { get; set; }
    }

    public class WarehouseTransferReceivingDetailResponseDto
    {
        public int Id { get; set; }
        public int WarehouseTransferDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityReceived { get; set; }
        public string? Notes { get; set; }
    }

    public class WarehouseTransferReceivingMovementDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityReceived { get; set; }
        public string MovementCode { get; set; } = string.Empty;
        public decimal StockBefore { get; set; }
        public decimal StockAfter { get; set; }
    }

    public class DispatchWarehouseTransferResponseDto
    {
        public int TransferId { get; set; }
        public string TransferCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DispatchedAt { get; set; }
        public int TotalMovementsCreated { get; set; }
        public List<DispatchMovementSummaryDto> Movements { get; set; } = new();
    }

    public class DispatchMovementSummaryDto
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityDispatched { get; set; }
        public string MovementCode { get; set; } = string.Empty;
        public decimal StockBefore { get; set; }
        public decimal StockAfter { get; set; }
    }

    public class WarehouseTransferListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SourceWarehouseName { get; set; } = string.Empty;
        public string DestinationWarehouseName { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalQuantityRequested { get; set; }
        public decimal TotalQuantityReceived { get; set; }
    }

    public class PagedWarehouseTransferResponseDto
    {
        public List<WarehouseTransferListItemDto> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
