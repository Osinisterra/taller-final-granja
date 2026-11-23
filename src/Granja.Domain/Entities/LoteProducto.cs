using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>
/// Lote de producto terminado en el inventario (RN-04). Las reglas de vencimiento,
/// descuento y daño viven aquí y no en los servicios.
/// </summary>
public class LoteProducto
{
    public int Id { get; private set; }
    public TipoProducto Tipo { get; private set; }
    public DateOnly FechaProduccion { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public decimal LitrosUsados { get; private set; }
    public decimal CantidadInicial { get; private set; }
    public decimal CantidadDisponible { get; private set; }
    public decimal CantidadDanada { get; private set; }

    private LoteProducto() { }

    public LoteProducto(TipoProducto tipo, DateOnly fechaProduccion, int vidaUtilDias, decimal litrosUsados, decimal cantidad)
    {
        if (litrosUsados <= 0) throw new DomainException("Los litros a transformar deben ser mayores que cero.");
        if (cantidad <= 0) throw new DomainException("La cantidad del lote debe ser mayor que cero.");
        if (vidaUtilDias <= 0) throw new DomainException("La vida útil debe ser mayor que cero.");

        Tipo = tipo;
        FechaProduccion = fechaProduccion;
        FechaVencimiento = fechaProduccion.AddDays(vidaUtilDias);
        LitrosUsados = litrosUsados;
        CantidadInicial = cantidad;
        CantidadDisponible = cantidad;
    }

    /// <summary>Crea un lote a partir de litros de leche usando los parámetros del producto (RN-03, RN-04).</summary>
    public static LoteProducto Crear(DateOnly fechaProduccion, decimal litros, ParametroProducto parametro) =>
        new(parametro.Tipo, fechaProduccion, parametro.VidaUtilDias, litros, parametro.CalcularCantidad(litros));

    /// <summary>RN-08: vencido cuando la fecha es posterior a la de vencimiento.</summary>
    public bool EstaVencido(DateOnly fecha) => fecha > FechaVencimiento;

    public bool EsVendible(DateOnly fecha) => CantidadDisponible > 0 && !EstaVencido(fecha);

    /// <summary>Cantidad que pasó a dañada y la que se vendió.</summary>
    public decimal CantidadVendida => CantidadInicial - CantidadDisponible - CantidadDanada;

    public EstadoLote Estado(DateOnly fecha)
    {
        if (CantidadDisponible == 0 && CantidadDanada > 0) return EstadoLote.Vencido;
        if (CantidadDisponible == 0) return EstadoLote.Agotado;
        return EstaVencido(fecha) ? EstadoLote.Vencido : EstadoLote.Disponible;
    }

    public void Descontar(decimal cantidad)
    {
        if (cantidad <= 0 || cantidad > CantidadDisponible)
            throw new DomainException($"Cantidad inválida ({cantidad}) para el lote {Id}: disponible {CantidadDisponible}.");
        CantidadDisponible -= cantidad;
    }

    /// <summary>RN-08: la cantidad disponible pasa a dañada y deja de ser vendible.</summary>
    public decimal MarcarComoDanado()
    {
        var perdida = CantidadDisponible;
        CantidadDanada += perdida;
        CantidadDisponible = 0;
        return perdida;
    }
}
