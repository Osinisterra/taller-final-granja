namespace Granja.Domain.Exceptions;

/// <summary>RN-01: la producción de una fecha ya fue registrada.</summary>
public class ProduccionDuplicadaException : ReglaNegocioException
{
    public DateOnly Fecha { get; }

    public ProduccionDuplicadaException(DateOnly fecha)
        : base($"La producción de leche del {fecha:yyyy-MM-dd} ya fue registrada.")
    {
        Fecha = fecha;
    }
}
