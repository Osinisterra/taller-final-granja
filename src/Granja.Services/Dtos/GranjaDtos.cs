using System.ComponentModel.DataAnnotations;
using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class ParametroProductoDto
{
    [Required, EnumDataType(typeof(TipoProducto))]
    public TipoProducto Tipo { get; set; }

    /// <summary>Litros de leche por unidad de producto (queso: 10 L = 1 kg; yogurt: 1 L = 1 L).</summary>
    [Range(0.001, 1_000_000)]
    public decimal Rendimiento { get; set; }

    [Range(1, 3650)]
    public int VidaUtilDias { get; set; }

    [Range(0, 1_000_000_000)]
    public decimal Precio { get; set; }

    public string Unidad { get; set; } = string.Empty;
}

public class GranjaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int NumeroVacas { get; set; }
    public decimal LitrosPorVaca { get; set; }
    public decimal ProduccionDiariaEsperada { get; set; }
    public List<ParametroProductoDto> Parametros { get; set; } = new();
}

public class ActualizarGranjaRequest
{
    /// <summary>n: número de vacas.</summary>
    [Range(1, 100_000)]
    public int NumeroVacas { get; set; }

    /// <summary>l: litros por vaca por día.</summary>
    [Range(0.001, 1_000)]
    public decimal LitrosPorVaca { get; set; }

    /// <summary>Parámetros por producto. Solo se actualizan los productos incluidos.</summary>
    public List<ParametroProductoDto> Parametros { get; set; } = new();
}

public class VacaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public DateOnly FechaRegistro { get; set; }
    public decimal LitrosProducidos { get; set; }
    public int DiasConProduccion { get; set; }
}
