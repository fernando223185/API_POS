namespace Application.DTOs.Product
{
    /// <summary>
    /// Respuesta al subir una imagen de producto
    /// </summary>
    public class UploadProductImageResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public ProductImageDto? Data { get; set; }
    }

    /// <summary>
    /// DTO de imagen de producto
    /// </summary>
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string S3Key { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Respuesta con múltiples imágenes de producto
    /// </summary>
    public class ProductImagesResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<ProductImageDto> Data { get; set; } = new();
        public int TotalImages { get; set; }
    }
}
