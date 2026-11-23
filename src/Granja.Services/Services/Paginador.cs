using Granja.Services.Dtos;

namespace Granja.Services.Services;

/// <summary>Arma páginas de resultados y valida los parámetros de paginación.</summary>
internal static class Paginador
{
    public static PaginaResponse<T> Paginar<T>(IReadOnlyList<T> todos, PaginacionRequest p) =>
        Crear(todos.Skip(Salto(p)).Take(p.TamanoPagina).ToList(), todos.Count, p);

    public static PaginaResponse<T> Crear<T>(List<T> items, int total, PaginacionRequest p) => new()
    {
        Items = items,
        Pagina = p.Pagina,
        TamanoPagina = p.TamanoPagina,
        TotalRegistros = total,
        TotalPaginas = (int)Math.Ceiling(total / (double)p.TamanoPagina)
    };

    public static int Salto(PaginacionRequest p)
    {
        if (p.Pagina < 1) throw new Domain.Exceptions.DomainException("La página debe ser 1 o mayor.");
        if (p.TamanoPagina is < 1 or > PaginacionRequest.TamanoMaximo)
            throw new Domain.Exceptions.DomainException($"El tamaño de página debe estar entre 1 y {PaginacionRequest.TamanoMaximo}.");
        return (p.Pagina - 1) * p.TamanoPagina;
    }
}
