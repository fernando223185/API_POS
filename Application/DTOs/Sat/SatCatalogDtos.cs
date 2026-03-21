namespace Application.DTOs.Sat
{
    public class SatUsoCfdiDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool AplicaPersonaFisica { get; set; }
        public bool AplicaPersonaMoral { get; set; }
    }

    public class SatRegimenFiscalDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool AplicaPersonaFisica { get; set; }
        public bool AplicaPersonaMoral { get; set; }
    }

    public class SatFormaPagoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Bancarizado { get; set; }
    }

    public class SatMetodoPagoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class SatTipoComprobanteDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class SatUnidadMedidaDto
    {
        public string ClaveUnidad { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Simbolo { get; set; }
        public string? Descripcion { get; set; }
    }

    public class SatProductoServicioDto
    {
        public string ClaveProdServ { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? IncluyeIva { get; set; }
        public string? IncluyeIeps { get; set; }
    }

    public class SatCatalogResponseDto<T>
    {
        public string Message { get; set; } = string.Empty;
        public int Error { get; set; }
        public List<T> Data { get; set; } = new();
        public int Total { get; set; }
    }
}
