using Granja.Domain.Enums;

namespace Granja.Domain.Interfaces;

/// <summary>Campo por el que se pueden ordenar las ventas.</summary>
public enum OrdenVentas
{
    Fecha = 1,
    Cantidad = 2,
    Total = 3
}

/// <summary>Criterios de búsqueda de ventas que el repositorio traduce a SQL.</summary>
public record FiltroVentas(
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    TipoProducto? Tipo = null,
    decimal? CantidadMinima = null,
    decimal? CantidadMaxima = null,
    OrdenVentas OrdenarPor = OrdenVentas.Fecha,
    bool Descendente = false);
