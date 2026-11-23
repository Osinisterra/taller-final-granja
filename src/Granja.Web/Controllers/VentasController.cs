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

    /// <summary>
    /// Lista paginada de ventas con filtros avanzados: rango de fechas, tipo, cantidad mínima/máxima
    /// y orden por Fecha, Cantidad o Total (ascendente o descendente).
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PaginaResponse<VentaResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResponse<VentaResponse>>> Listar([FromQuery] FiltroVentasRequest filtro, CancellationToken ct) =>
        Ok(await _service.ListarAsync(filtro, ct));

    /// <summary>Consulta una venta con su detalle por lote.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<VentaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VentaResponse>> Obtener(int id, CancellationToken ct) =>
        Ok(await _service.ObtenerAsync(id, ct));
}
