using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class LoteResponse
{
    public int Id { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public DateOnly FechaProduccion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal LitrosUsados { get; set; }
    public decimal CantidadInicial { get; set; }
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadVendida { get; set; }
    public decimal CantidadDanada { get; set; }
    public EstadoLote Estado { get; set; }
}

public class InventarioResumenItem
{
    public TipoProducto TipoProducto { get; set; }
    public string Unidad { get; set; } = string.Empty;
    /// <summary>Existencias no vencidas que se pueden vender a la fecha.</summary>
    public decimal DisponibleVendible { get; set; }
    /// <summary>Existencias vencidas que aún no se han procesado como dañadas.</summary>
    public decimal VencidoSinProcesar { get; set; }
    public decimal Danado { get; set; }
    public int LotesVigentes { get; set; }
}

public class InventarioResumenResponse
{
    public DateOnly Fecha { get; set; }
    public List<InventarioResumenItem> Productos { get; set; } = new();
}

public class ProcesarVencimientosRequest
{
    /// <summary>Fecha de proceso. Si se omite, se usa la fecha actual.</summary>
    public DateOnly? Fecha { get; set; }
}

public class LoteDanadoItem
{
    public int LoteId { get; set; }
    public TipoProducto TipoProducto { get; set; }
    public DateOnly FechaProduccion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal CantidadDanada { get; set; }
}

public class ProcesarVencimientosResponse
{
    public DateOnly Fecha { get; set; }
    public int LotesAfectados { get; set; }
    public List<LoteDanadoItem> Lotes { get; set; } = new();
    public List<TotalPorTipo> TotalPorTipo { get; set; } = new();
}

public class TotalPorTipo
{
    public TipoProducto TipoProducto { get; set; }
    public decimal Cantidad { get; set; }
}
