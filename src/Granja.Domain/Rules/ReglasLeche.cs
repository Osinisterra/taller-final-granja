using Granja.Domain.Exceptions;

namespace Granja.Domain.Rules;

/// <summary>Reglas sobre la leche producida en un día.</summary>
public static class ReglasLeche
{
    /// <summary>RN-02: la suma de litros transformados no puede superar los producidos ese día.</summary>
    public static void ValidarTransformacion(DateOnly fecha, decimal producidos, decimal yaTransformados, decimal solicitados)
    {
        if (solicitados <= 0) throw new DomainException("Los litros a transformar deben ser mayores que cero.");

        var disponibles = producidos - yaTransformados;
        if (solicitados > disponibles)
            throw new LecheInsuficienteException(fecha, solicitados, disponibles);
    }

    /// <summary>Leche no transformada del día: se reporta como no aprovechada.</summary>
    public static decimal NoAprovechada(decimal producidos, decimal transformados) =>
        Math.Max(0, producidos - transformados);
}
