namespace Granja.Domain.Exceptions;

/// <summary>RN-02: se intentan transformar más litros de los disponibles en la fecha.</summary>
public class LecheInsuficienteException : ReglaNegocioException
{
    public DateOnly Fecha { get; }
    public decimal Solicitados { get; }
    public decimal Disponibles { get; }

    public LecheInsuficienteException(DateOnly fecha, decimal solicitados, decimal disponibles)
        : base($"Leche insuficiente el {fecha:yyyy-MM-dd}: se solicitaron {solicitados} L y solo quedan {disponibles} L sin transformar.")
    {
        Fecha = fecha;
        Solicitados = solicitados;
        Disponibles = disponibles;
    }
}
