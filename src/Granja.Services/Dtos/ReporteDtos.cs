using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class ReporteVentaItem
{
    public int VentaId { get; set; }
    public DateOnly Fecha { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}

public class ReporteVentaTotalTipo
{
    public TipoProducto TipoProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Total { get; set; }
}

public class ReporteVentasResponse
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public TipoProducto? Tipo { get; set; }
    public int NumeroVentas { get; set; }
    public List<ReporteVentaItem> Ventas { get; set; } = new();
    public List<ReporteVentaTotalTipo> TotalesPorProducto { get; set; } = new();
    public decimal TotalGeneral { get; set; }
}

public class LecheDiariaItem
{
    public DateOnly Fecha { get; set; }
    public int NumeroVacas { get; set; }
    public decimal LitrosProducidos { get; set; }
    public decimal LitrosTransformados { get; set; }
    public decimal LitrosNoAprovechados { get; set; }
}

public class ReporteLecheDiariaResponse
{
    public List<LecheDiariaItem> Dias { get; set; } = new();
    public decimal TotalProducido { get; set; }
    public decimal TotalTransformado { get; set; }
    public decimal TotalNoAprovechado { get; set; }
}

public class ProductoDiarioItem
{
    public DateOnly Fecha { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public int Lotes { get; set; }
    public decimal LitrosUsados { get; set; }
    public decimal CantidadProducida { get; set; }
}

public class ProductoTotalItem
{
    public TipoProducto TipoProducto { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal Producido { get; set; }
    public decimal Vendido { get; set; }
    public decimal Disponible { get; set; }
    public decimal Danado { get; set; }
    public decimal Ingresos { get; set; }
}

public class ReporteProductoDanadoResponse
{
    public List<LoteDanadoItem> Lotes { get; set; } = new();
    public List<TotalPorTipo> TotalPorTipo { get; set; } = new();
}

public class VacaProduccionItem
{
    public int VacaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal LitrosTotales { get; set; }
    public int DiasProducidos { get; set; }
    public decimal PromedioDiario { get; set; }
}

public class ReporteVacasResponse
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public int NumeroVacas { get; set; }
    public decimal TotalLitros { get; set; }
    /// <summary>Promedio de litros totales por vaca en el período.</summary>
    public decimal PromedioPorVaca { get; set; }
    /// <summary>Promedio de litros por vaca por día.</summary>
    public decimal PromedioDiarioPorVaca { get; set; }
    public VacaProduccionItem? MayorProduccion { get; set; }
    public VacaProduccionItem? MenorProduccion { get; set; }
    public List<VacaProduccionItem> BajoPromedio { get; set; } = new();
    public List<VacaProduccionItem> Vacas { get; set; } = new();
}

/// <summary>Archivo generado por una exportación.</summary>
public record ArchivoExportado(string NombreArchivo, string ContentType, byte[] Contenido);
