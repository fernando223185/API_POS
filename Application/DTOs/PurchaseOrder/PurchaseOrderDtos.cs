using System;
using System.Collections.Generic;

namespace Application.DTOs.PurchaseOrder
{
    /// <summary>
    /// DTO para crear una nueva orden de compra
    /// </summary>
    public class CreatePurchaseOrderDto
    {
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string? SupplierReference { get; set; }
        public string? PaymentTerms { get; set; }
        public string? DeliveryTerms { get; set; }
        public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO para detalle de orden de compra
    /// </summary>
    public class CreatePurchaseOrderDetailDto
    {
        public int ProductId { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal TaxPercentage { get; set; } = 16;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para actualizar orden de compra
    /// </summary>
    public class UpdatePurchaseOrderDto
    {
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public string? SupplierReference { get; set; }
        public string? PaymentTerms { get; set; }
        public string? DeliveryTerms { get; set; }
        public List<UpdatePurchaseOrderDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO para actualizar detalle
    /// </summary>
    public class UpdatePurchaseOrderDetailDto
    {
        public int? Id { get; set; } // Null si es nuevo
        public int ProductId { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal TaxPercentage { get; set; } = 16;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para detalle
    /// </summary>
    public class PurchaseOrderDetailResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantityOrdered { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal QuantityPending { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para orden de compra
    /// </summary>
    public class PurchaseOrderResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public string? SupplierReference { get; set; }
        public string? PaymentTerms { get; set; }
        public string? DeliveryTerms { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? UpdatedByUserName { get; set; }
        public List<PurchaseOrderDetailResponseDto> Details { get; set; } = new();
        
        // Información adicional
        public int TotalItems { get; set; }
        public decimal TotalQuantityOrdered { get; set; }
        public decimal TotalQuantityReceived { get; set; }
        public decimal CompletionPercentage { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para lista de órdenes
    /// </summary>
    public class PurchaseOrdersListResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<PurchaseOrderResponseDto> PurchaseOrders { get; set; } = new();
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ApprovedOrders { get; set; }
        public int ReceivedOrders { get; set; }
    }

    /// <summary>
    /// DTO de respuesta paginada
    /// </summary>
    public class PurchaseOrdersPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<PurchaseOrderResponseDto> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// DTO para cambiar estado
    /// </summary>
    public class ChangeStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
