using Granja.Domain.Enums;
using Granja.Domain.Exceptions;

namespace Granja.Domain.Entities;

/// <summary>
/// Configuración de la granja: n vacas que producen l litros por día cada una,
/// y los parámetros de cada producto. Se llama GranjaLechera para no chocar con el namespace raíz "Granja".
/// </summary>
public class GranjaLechera
{
    private readonly List<ParametroProducto> _parametros = new();

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    /// <summary>n: número de vacas.</summary>
    public int NumeroVacas { get; private set; }
    /// <summary>l: litros por vaca por día.</summary>
    public decimal LitrosPorVaca { get; private set; }
    public IReadOnlyCollection<ParametroProducto> Parametros => _parametros.AsReadOnly();

    /// <summary>Producción diaria total esperada: n × l (RN-01).</summary>
    public decimal ProduccionDiariaEsperada => NumeroVacas * LitrosPorVaca;

    private GranjaLechera() { }

    public GranjaLechera(string nombre, int numeroVacas, decimal litrosPorVaca, IEnumerable<ParametroProducto> parametros)
    {
        Nombre = string.IsNullOrWhiteSpace(nombre) ? "Granja" : nombre.Trim();
        ActualizarProduccion(numeroVacas, litrosPorVaca);
        _parametros.AddRange(parametros);

        foreach (var tipo in Enum.GetValues<TipoProducto>())
        {
            if (_parametros.All(p => p.Tipo != tipo))
                throw new DomainException($"Falta la configuración del producto {tipo}.");
        }
    }

    public void ActualizarProduccion(int numeroVacas, decimal litrosPorVaca)
    {
        if (numeroVacas <= 0) throw new DomainException("El número de vacas (n) debe ser mayor que cero.");
        if (litrosPorVaca <= 0) throw new DomainException("Los litros por vaca (l) deben ser mayores que cero.");

        NumeroVacas = numeroVacas;
        LitrosPorVaca = litrosPorVaca;
    }

    public ParametroProducto ParametroDe(TipoProducto tipo) =>
        _parametros.FirstOrDefault(p => p.Tipo == tipo)
        ?? throw new DomainException($"No hay parámetros configurados para {tipo}.");
}
