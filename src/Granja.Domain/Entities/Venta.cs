using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>Venta de un producto con su trazabilidad por lote (RN-09).</summary>
public class Venta
{
    private readonly List<VentaDetalle> _detalles = new();

    public int Id { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Total { get; private set; }
    public IReadOnlyCollection<VentaDetalle> Detalles => _detalles.AsReadOnly();

    private Venta() { }

    public Venta(DateOnly fecha, TipoProducto tipo, decimal precioUnitario)
    {
        if (precioUnitario < 0) throw new DomainException("El precio unitario no puede ser negativo.");
        Fecha = fecha;
        Tipo = tipo;
        PrecioUnitario = precioUnitario;
    }

    /// <summary>Descuenta del lote y deja registro del detalle; recalcula cantidad y total.</summary>
    public void AgregarDetalle(LoteProducto lote, decimal cantidad)
    {
        if (lote.Tipo != Tipo) throw new DomainException("El lote no corresponde al producto de la venta.");
        if (lote.EstaVencido(Fecha)) throw new DomainException($"El lote {lote.Id} está vencido y no se puede vender.");

        lote.Descontar(cantidad);
        _detalles.Add(new VentaDetalle(lote, cantidad));
        Cantidad += cantidad;
        Total = Cantidad * PrecioUnitario;
    }
}
