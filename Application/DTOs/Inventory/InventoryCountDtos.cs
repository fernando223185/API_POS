namespace Application.DTOs.Inventory
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DTOs PARA CREACIÓN Y ACTUALIZACIÓN
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO para crear una nueva sesión de conteo
    /// </summary>
    public class CreateInventoryCountDto
    {
        public int WarehouseId { get; set; }
        public string CountType { get; set; } = string.Empty; // CYCLE, FULL, CATEGORY, LOCATION
        public DateTime ScheduledDate { get; set; }
        public int AssignedToUserId { get; set; }
        public int? CategoryId { get; set; }
        public string? Notes { get; set; }

        /// <summary>
        /// Lista manual de IDs de productos a contar (opcional)
        /// Si no se proporciona, el sistema selecciona automáticamente según el tipo
        /// </summary>
        public List<int>? ProductIds { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una sesión de conteo existente
    /// </summary>
    public class UpdateInventoryCountDto
    {
        public DateTime? ScheduledDate { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para iniciar un conteo (cambiar estado a InProgress)
    /// </summary>
    public class StartInventoryCountDto
    {
        // Este DTO puede estar vacío o incluir notas al inicio
        public string? StartNotes { get; set; }
    }

    /// <summary>
    /// DTO para actualizar la cantidad física de un producto durante el conteo
    /// </summary>
    public class UpdateCountDetailDto
    {
        public decimal PhysicalQuantity { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para solicitar reconteo de un producto
    /// </summary>
    public class RequestRecountDto
    {
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO para completar y aprobar un conteo (genera ajustes automáticamente)
    /// </summary>
    public class CompleteInventoryCountDto
    {
        public string? ApprovalNotes { get; set; }

        /// <summary>
        /// IDs de detalles que deben excluirse del ajuste automático
        /// (por ejemplo, si se va a investigar más)
        /// </summary>
        public List<int>? ExcludeDetailIds { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DTOs DE RESPUESTA
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO de respuesta completa de un conteo con todos sus detalles
    /// </summary>
    public class InventoryCountResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string CountType { get; set; } = string.Empty;
        public string CountTypeLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;
        public int? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Notes { get; set; }
        public int TotalProducts { get; set; }
        public int CountedProducts { get; set; }
        public int PendingProducts { get; set; }
        public int ProductsWithVariance { get; set; }
        public decimal ProgressPercentage { get; set; }
        public decimal TotalVarianceCost { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CompanyId { get; set; }

        public List<InventoryCountDetailResponseDto> Details { get; set; } = new();
    }

    /// <summary>
    /// DTO de detalle de un producto en el conteo
    /// </summary>
    public class InventoryCountDetailResponseDto
    {
        public int Id { get; set; }
        public int InventoryCountId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal SystemQuantity { get; set; }
        public decimal? PhysicalQuantity { get; set; }
        public decimal? Variance { get; set; }
        public decimal? VariancePercentage { get; set; }
        public decimal? VarianceCost { get; set; }
        public decimal? UnitCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? CountedByUserId { get; set; }
        public string? CountedByUserName { get; set; }
        public DateTime? CountedAt { get; set; }
        public bool RecountRequested { get; set; }
        public int? RecountedByUserId { get; set; }
        public string? RecountedByUserName { get; set; }
        public DateTime? RecountedAt { get; set; }
    }

    /// <summary>
    /// DTO resumido para listado de conteos
    /// </summary>
    public class InventoryCountSummaryDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string CountType { get; set; } = string.Empty;
        public string CountTypeLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int CountedProducts { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int ProductsWithVariance { get; set; }
        public decimal TotalVarianceCost { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO para respuesta paginada de conteos
    /// </summary>
    public class PagedInventoryCountsResponseDto
    {
        public List<InventoryCountSummaryDto> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CLASES DE UTILIDAD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tipos de conteo con sus etiquetas en español
    /// </summary>
    public static class CountType
    {
        public const string CYCLE = "CYCLE";
        public const string FULL = "FULL";
        public const string CATEGORY = "CATEGORY";
        public const string LOCATION = "LOCATION";

        public static readonly Dictionary<string, string> Labels = new()
        {
            { CYCLE, "Conteo Cíclico" },
            { FULL, "Inventario Completo" },
            { CATEGORY, "Por Categoría" },
            { LOCATION, "Por Ubicación" }
        };

        public static string GetLabel(string type)
        {
            return Labels.TryGetValue(type, out var label) ? label : type;
        }
    }

    /// <summary>
    /// Estados de conteo con etiquetas en español
    /// </summary>
    public static class CountStatus
    {
        public const string DRAFT = "Draft";
        public const string IN_PROGRESS = "InProgress";
        public const string COMPLETED = "Completed";
        public const string CANCELLED = "Cancelled";

        public static readonly Dictionary<string, string> Labels = new()
        {
            { DRAFT, "Borrador" },
            { IN_PROGRESS, "En Progreso" },
            { COMPLETED, "Completado" },
            { CANCELLED, "Cancelado" }
        };

        public static string GetLabel(string status)
        {
            return Labels.TryGetValue(status, out var label) ? label : status;
        }
    }

    /// <summary>
    /// Estados de detalle de conteo
    /// </summary>
    public static class CountDetailStatus
    {
        public const string PENDING = "Pending";
        public const string COUNTED = "Counted";
        public const string RECOUNTED = "Recounted";

        public static readonly Dictionary<string, string> Labels = new()
        {
            { PENDING, "Pendiente" },
            { COUNTED, "Contado" },
            { RECOUNTED, "Recontado" }
        };

        public static string GetLabel(string status)
        {
            return Labels.TryGetValue(status, out var label) ? label : status;
        }
    }
}
