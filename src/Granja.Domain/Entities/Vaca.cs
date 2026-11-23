namespace Granja.Domain.Entities;

/// <summary>Vaca de la granja. Cada día produce l litros (modelo n × l).</summary>
public class Vaca
{
    private readonly List<ProduccionLeche> _producciones = new();

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activa { get; private set; }
    public DateOnly FechaRegistro { get; private set; }
    public IReadOnlyCollection<ProduccionLeche> Producciones => _producciones.AsReadOnly();

    private Vaca() { }

    public Vaca(string nombre, DateOnly fechaRegistro)
    {
        Nombre = string.IsNullOrWhiteSpace(nombre) ? "Vaca" : nombre.Trim();
        FechaRegistro = fechaRegistro;
        Activa = true;
    }

    /// <summary>Total de litros producidos por la vaca. Una vaca recién registrada produce 0.</summary>
    public decimal LitrosProducidos => _producciones.Sum(p => p.Litros);

    public void Activar() => Activa = true;
    public void Desactivar() => Activa = false;
}
