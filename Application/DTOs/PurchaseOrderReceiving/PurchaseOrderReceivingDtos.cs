using System;
using System.Collections.Generic;

namespace Application.DTOs.PurchaseOrderReceiving
{
    /// <summary>
    /// DTO para crear una nueva recepción de mercancía
    /// </summary>
    public class CreatePurchaseOrderReceivingDto
    {
        public int PurchaseOrderId { get; set; }
        public DateTime ReceivingDate { get; set; } = DateTime.UtcNow;
        public string? SupplierInvoiceNumber { get; set; }
        public string? CarrierName { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderReceivingDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO para detalle de recepción
    /// </summary>
    public class CreatePurchaseOrderReceivingDetailDto
    {
        public int PurchaseOrderDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal QuantityAccepted { get; set; }
        public decimal QuantityRejected { get; set; }
        public string? StorageLocation { get; set; }
        public string? LotNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para recepción
    /// </summary>
    public class PurchaseOrderReceivingResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime ReceivingDate { get; set; }
        public string? SupplierInvoiceNumber { get; set; }
        public string? CarrierName { get; set; }
        public string? TrackingNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPostedToInventory { get; set; }
        public DateTime? PostedToInventoryDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        
        public List<PurchaseOrderReceivingDetailResponseDto> Details { get; set; } = new();
        
        // Totales calculados
        public int TotalItems { get; set; }
        public decimal TotalQuantityReceived { get; set; }
        public decimal TotalQuantityAccepted { get; set; }
        public decimal TotalQuantityRejected { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para detalle de recepción
    /// </summary>
    public class PurchaseOrderReceivingDetailResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityOrdered { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal QuantityAccepted { get; set; }
        public decimal QuantityRejected { get; set; }
        public decimal QuantityPending { get; set; }
        public string? StorageLocation { get; set; }
        public string? LotNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para lista de recepciones
    /// </summary>
    public class ReceivingsListResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<PurchaseOrderReceivingResponseDto> Receivings { get; set; } = new();
        public int TotalReceivings { get; set; }
        public int PendingToPost { get; set; }
        public int Posted { get; set; }
    }

    /// <summary>
    /// DTO de respuesta paginada
    /// </summary>
    public class ReceivingsPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<PurchaseOrderReceivingResponseDto> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// DTO para aplicar recepción a inventario
    /// </summary>
    public class PostToInventoryResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public PostToInventoryDataDto Data { get; set; } = new();
    }

    public class PostToInventoryDataDto
    {
        public int ReceivingId { get; set; }
        public string ReceivingCode { get; set; } = string.Empty;
        public int TotalMovementsCreated { get; set; }
        public List<InventoryMovementSummaryDto> Movements { get; set; } = new();
    }

    public class InventoryMovementSummaryDto
    {
        public string MovementCode { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal StockBefore { get; set; }
        public decimal StockAfter { get; set; }
    }
}
