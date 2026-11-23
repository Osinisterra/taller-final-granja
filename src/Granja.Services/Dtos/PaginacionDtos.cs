using System.ComponentModel.DataAnnotations;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;

namespace Granja.Services.Dtos;

/// <summary>Parámetros de paginación (query string: ?pagina=1&amp;tamanoPagina=20).</summary>
public class PaginacionRequest
{
    public const int TamanoMaximo = 100;

    [Range(1, int.MaxValue)]
    public int Pagina { get; set; } = 1;

    [Range(1, TamanoMaximo)]
    public int TamanoPagina { get; set; } = 20;
}

/// <summary>Página de resultados con los metadatos de paginación.</summary>
public class PaginaResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public bool TieneAnterior => Pagina > 1;
    public bool TieneSiguiente => Pagina < TotalPaginas;
}

/// <summary>Filtros avanzados del listado de ventas.</summary>
public class FiltroVentasRequest : PaginacionRequest
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }

    [EnumDataType(typeof(TipoProducto))]
    public TipoProducto? Tipo { get; set; }

    [Range(0, 10_000_000)]
    public decimal? CantidadMinima { get; set; }

    [Range(0, 10_000_000)]
    public decimal? CantidadMaxima { get; set; }

    /// <summary>Fecha (por defecto), Cantidad o Total.</summary>
    [EnumDataType(typeof(OrdenVentas))]
    public OrdenVentas OrdenarPor { get; set; } = OrdenVentas.Fecha;

    public bool Descendente { get; set; }
}

/// <summary>Filtros avanzados del listado de lotes del inventario.</summary>
public class FiltroLotesRequest : PaginacionRequest
{
    [EnumDataType(typeof(TipoProducto))]
    public TipoProducto? Tipo { get; set; }

    /// <summary>Estado del lote evaluado a <see cref="Fecha"/>: Disponible, Agotado o Vencido.</summary>
    [EnumDataType(typeof(EstadoLote))]
    public EstadoLote? Estado { get; set; }

    /// <summary>Solo lotes vendibles (no vencidos y con existencias) a la fecha.</summary>
    public bool SoloDisponibles { get; set; }

    /// <summary>Rango de fecha de producción.</summary>
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }

    /// <summary>Fecha con la que se evalúa el estado (por defecto, hoy).</summary>
    public DateOnly? Fecha { get; set; }
}
