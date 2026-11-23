namespace Granja.Domain.Entities;

/// <summary>Cantidad que salió de un lote específico en una venta (trazabilidad).</summary>
public class VentaDetalle
{
    public int Id { get; private set; }
    public int VentaId { get; private set; }
    public int LoteId { get; private set; }
    public LoteProducto? Lote { get; private set; }
    public DateOnly FechaProduccionLote { get; private set; }
    public decimal Cantidad { get; private set; }

    private VentaDetalle() { }

    internal VentaDetalle(LoteProducto lote, decimal cantidad)
    {
        Lote = lote;
        LoteId = lote.Id;
        FechaProduccionLote = lote.FechaProduccion;
        Cantidad = cantidad;
    }
}
