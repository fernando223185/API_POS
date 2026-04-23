namespace Application.DTOs.ShippingCarrier
{
    public class CreateShippingCarrierDto
    {
        public int? CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>
        /// Código completamente manual. Si se provee, se usa tal cual (debe ser único).
        /// Ej: "FEDEX", "DHL-MX", "ESTAFETA"
        /// </summary>
        public string? CustomCode { get; set; }
        /// <summary>
        /// Prefijo para el código autogenerado. Si no se provee, usa "PKT".
        /// Ej: "FEDEX" genera "FEDEX-00001"
        /// </summary>
        public string? CodePrefix { get; set; }
        // Contacto
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? PhoneAlt { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        // Dirección
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; } = "México";
        // Cuenta
        public string? AccountNumber { get; set; }
        public string? AccountRepName { get; set; }
        public string? AccountRepPhone { get; set; }
        public string? AccountRepEmail { get; set; }
        // Servicio
        public string? ServiceTypes { get; set; }
        public int? EstimatedDeliveryDays { get; set; }
        public string? Coverage { get; set; } = "nacional";
        // Precios
        public decimal? BasePrice { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? ExpressPricePerKg { get; set; }
        public decimal? MaxWeightKg { get; set; }
        // Rastreo e integración
        public string? TrackingUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? LogoUrl { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateShippingCarrierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>Nuevo código para la paquetería. Si es null, no se modifica.</summary>
        public string? Code { get; set; }
        // Contacto
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? PhoneAlt { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        // Dirección
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        // Cuenta
        public string? AccountNumber { get; set; }
        public string? AccountRepName { get; set; }
        public string? AccountRepPhone { get; set; }
        public string? AccountRepEmail { get; set; }
        // Servicio
        public string? ServiceTypes { get; set; }
        public int? EstimatedDeliveryDays { get; set; }
        public string? Coverage { get; set; }
        // Precios
        public decimal? BasePrice { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? ExpressPricePerKg { get; set; }
        public decimal? MaxWeightKg { get; set; }
        // Rastreo e integración
        public string? TrackingUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class ShippingCarrierDto
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        // Contacto
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? PhoneAlt { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        // Dirección
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        // Cuenta
        public string? AccountNumber { get; set; }
        public string? AccountRepName { get; set; }
        public string? AccountRepPhone { get; set; }
        public string? AccountRepEmail { get; set; }
        // Servicio
        public string? ServiceTypes { get; set; }
        public int? EstimatedDeliveryDays { get; set; }
        public string? Coverage { get; set; }
        // Precios
        public decimal? BasePrice { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? ExpressPricePerKg { get; set; }
        public decimal? MaxWeightKg { get; set; }
        // Rastreo e integración
        public string? TrackingUrl { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? LogoUrl { get; set; }
        // Estado
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
