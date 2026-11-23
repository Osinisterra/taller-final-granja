using Granja.Domain.Enums;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

/// <summary>Ventas con salida FIFO.</summary>
[ApiController]
[Route("api/ventas")]
[Produces("application/json")]
public class VentasController : ControllerBase
{
    private readonly IVentaService _service;

    public VentasController(IVentaService service) => _service = service;

    /// <summary>Registra una venta: despacha primero el producto más antiguo no vencido.</summary>
    [HttpPost]
    [ProducesResponseType<VentaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VentaResponse>> Registrar([FromBody] VentaRequest request, CancellationToken ct)
    {
        var venta = await _service.RegistrarVentaAsync(request, ct);
        return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, venta);
    }

    /// <summary>Lista las ventas filtradas por rango de fechas y tipo.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<VentaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VentaResponse>>> Listar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] TipoProducto? tipo, CancellationToken ct) =>
        Ok(await _service.ListarAsync(desde, hasta, tipo, ct));

    /// <summary>Consulta una venta con su detalle por lote.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<VentaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VentaResponse>> Obtener(int id, CancellationToken ct) =>
        Ok(await _service.ObtenerAsync(id, ct));
}
