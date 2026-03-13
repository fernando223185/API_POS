using System;
using System.Collections.Generic;

namespace Application.DTOs.Supplier
{
    /// <summary>
    /// DTO para crear un nuevo proveedor
    /// </summary>
    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; } = "México";
        public int PaymentTermsDays { get; set; } = 30;
        public decimal CreditLimit { get; set; } = 0;
        public decimal DefaultDiscountPercentage { get; set; } = 0;
    }

    /// <summary>
    /// DTO para actualizar proveedor
    /// </summary>
    public class UpdateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public int PaymentTermsDays { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal DefaultDiscountPercentage { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para proveedor
    /// </summary>
    public class SupplierResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public int PaymentTermsDays { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal DefaultDiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Información adicional
        public int TotalPurchaseOrders { get; set; }
        public decimal TotalPurchased { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para lista de proveedores
    /// </summary>
    public class SuppliersListResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<SupplierResponseDto> Suppliers { get; set; } = new();
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }
        public int InactiveSuppliers { get; set; }
    }

    /// <summary>
    /// DTO de respuesta paginada
    /// </summary>
    public class SuppliersPagedResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<SupplierResponseDto> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
