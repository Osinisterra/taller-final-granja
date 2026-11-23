namespace Granja.Domain.Interfaces;

/// <summary>Reloj abstracto para poder simular días en las pruebas (RN-10).</summary>
public interface IClock
{
    DateOnly Today { get; }
}
