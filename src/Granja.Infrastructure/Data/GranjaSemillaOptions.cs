using Granja.Domain.Enums;

namespace Granja.Infrastructure.Data;

/// <summary>Valores iniciales de la granja, leídos de appsettings (sección "GranjaSemilla").</summary>
public class GranjaSemillaOptions
{
    public const string Seccion = "GranjaSemilla";

    public string Nombre { get; set; } = "Granja";
    public int NumeroVacas { get; set; }
    public decimal LitrosPorVaca { get; set; }
    public List<ProductoSemilla> Productos { get; set; } = new();

    public class ProductoSemilla
    {
        public TipoProducto Tipo { get; set; }
        public decimal Rendimiento { get; set; }
        public int VidaUtilDias { get; set; }
        public decimal Precio { get; set; }
        public string Unidad { get; set; } = string.Empty;
    }
}
