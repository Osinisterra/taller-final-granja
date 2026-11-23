using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>Parámetros configurables por producto: rendimiento, vida útil y precio.</summary>
public class ParametroProducto
{
    public int Id { get; private set; }
    public int GranjaId { get; private set; }
    public TipoProducto Tipo { get; private set; }
    /// <summary>Litros de leche necesarios para obtener una unidad de producto (RN-03).</summary>
    public decimal Rendimiento { get; private set; }
    public int VidaUtilDias { get; private set; }
    public decimal Precio { get; private set; }
    public string Unidad { get; private set; } = string.Empty;

    private ParametroProducto() { }

    public ParametroProducto(TipoProducto tipo, decimal rendimiento, int vidaUtilDias, decimal precio, string unidad)
    {
        Tipo = tipo;
        Unidad = string.IsNullOrWhiteSpace(unidad) ? "unidad" : unidad.Trim();
        Actualizar(rendimiento, vidaUtilDias, precio);
    }

    public void Actualizar(decimal rendimiento, int vidaUtilDias, decimal precio)
    {
        if (rendimiento <= 0) throw new DomainException($"El rendimiento de {Tipo} debe ser mayor que cero.");
        if (vidaUtilDias <= 0) throw new DomainException($"La vida útil de {Tipo} debe ser mayor que cero.");
        if (precio < 0) throw new DomainException($"El precio de {Tipo} no puede ser negativo.");

        Rendimiento = rendimiento;
        VidaUtilDias = vidaUtilDias;
        Precio = precio;
    }

    /// <summary>RN-03: cantidad de producto = litros ÷ rendimiento.</summary>
    public decimal CalcularCantidad(decimal litros) => Math.Round(litros / Rendimiento, 3);
}
