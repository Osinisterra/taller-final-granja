using System.ComponentModel.DataAnnotations;
using Granja.Domain.Enums;

namespace Granja.Services.Dtos;

public class RegistrarProduccionRequest
{
    /// <summary>Fecha de la producción (yyyy-MM-dd). Si se omite, se usa la fecha actual.</summary>
    public DateOnly? Fecha { get; set; }
}

public class ProduccionDiaResponse
{
    public DateOnly Fecha { get; set; }
    public int NumeroVacas { get; set; }
    public decimal LitrosProducidos { get; set; }
    public decimal LitrosTransformados { get; set; }
    /// <summary>Litros aún no transformados de ese día.</summary>
    public decimal LitrosDisponibles { get; set; }
}

public class TransformacionRequest
{
    /// <summary>Fecha de la leche a transformar. Si se omite, se usa la fecha actual.</summary>
    public DateOnly? Fecha { get; set; }

    [Required, EnumDataType(typeof(TipoProducto))]
    public TipoProducto TipoProducto { get; set; }

    [Range(0.001, 10_000_000)]
    public decimal Litros { get; set; }
}
