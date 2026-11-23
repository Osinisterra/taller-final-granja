using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>Litros producidos por una vaca en una fecha. Único por (VacaId, Fecha) — RN-01.</summary>
public class ProduccionLeche
{
    public int Id { get; private set; }
    public int VacaId { get; private set; }
    public Vaca? Vaca { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal Litros { get; private set; }

    private ProduccionLeche() { }

    public ProduccionLeche(int vacaId, DateOnly fecha, decimal litros)
    {
        if (litros < 0) throw new DomainException("Los litros producidos no pueden ser negativos.");
        VacaId = vacaId;
        Fecha = fecha;
        Litros = litros;
    }
}
