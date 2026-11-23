using Granja.Domain.Exceptions;

namespace Granja.Services.Services;

internal static class Validaciones
{
    public static void Rango(DateOnly? desde, DateOnly? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new DomainException("La fecha 'desde' no puede ser posterior a 'hasta'.");
    }
}
