using Granja.Domain.Enums;

namespace Granja.Domain.Exceptions;

/// <summary>RN-07: no hay existencias no vencidas suficientes para la venta.</summary>
public class InventarioInsuficienteException : ReglaNegocioException
{
    public TipoProducto Tipo { get; }
    public decimal Solicitado { get; }
    public decimal Disponible { get; }

    public InventarioInsuficienteException(TipoProducto tipo, decimal solicitado, decimal disponible)
        : base($"Inventario insuficiente de {tipo}: se solicitaron {solicitado} y solo hay {disponible} no vencido(s).")
    {
        Tipo = tipo;
        Solicitado = solicitado;
        Disponible = disponible;
    }
}
