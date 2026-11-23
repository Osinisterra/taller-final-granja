using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

/// <summary>Producción diaria de leche y transformación en producto.</summary>
[ApiController]
[Route("api/produccion")]
[Produces("application/json")]
public class ProduccionController : ControllerBase
{
    private readonly IProduccionService _service;

    public ProduccionController(IProduccionService service) => _service = service;

    /// <summary>Registra la producción del día (n × l). Solo una vez por fecha.</summary>
    [HttpPost]
    [ProducesResponseType<ProduccionDiaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProduccionDiaResponse>> Registrar([FromBody] RegistrarProduccionRequest? request, CancellationToken ct)
    {
        var resultado = await _service.RegistrarProduccionAsync(request ?? new RegistrarProduccionRequest(), ct);
        return CreatedAtAction(nameof(ObtenerDia), new { fecha = resultado.Fecha.ToString("yyyy-MM-dd") }, resultado);
    }

    /// <summary>Lista la producción por día en un rango de fechas.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProduccionDiaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProduccionDiaResponse>>> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        Ok(await _service.ListarAsync(desde, hasta, ct));

    /// <summary>Consulta la producción de una fecha (producidos, transformados y disponibles).</summary>
    [HttpGet("{fecha}")]
    [ProducesResponseType<ProduccionDiaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProduccionDiaResponse>> ObtenerDia(DateOnly fecha, CancellationToken ct) =>
        Ok(await _service.ObtenerDiaAsync(fecha, ct));

    /// <summary>Transforma litros de una fecha en queso o yogurt y crea un lote en el inventario.</summary>
    [HttpPost("transformaciones")]
    [ProducesResponseType<LoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoteResponse>> Transformar([FromBody] TransformacionRequest request, CancellationToken ct)
    {
        var lote = await _service.TransformarAsync(request, ct);
        return CreatedAtAction(nameof(InventarioController.ObtenerLote), "Inventario", new { id = lote.Id }, lote);
    }
}
