using Granja.Domain.Enums;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

/// <summary>Inventario de lotes de queso y yogurt.</summary>
[ApiController]
[Route("api/inventario")]
[Produces("application/json")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _service;

    public InventarioController(IInventarioService service) => _service = service;

    /// <summary>Lista los lotes del inventario (opcionalmente por tipo y solo los vendibles a la fecha).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LoteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LoteResponse>>> Listar([FromQuery] TipoProducto? tipo, [FromQuery] bool soloDisponibles = false,
        [FromQuery] DateOnly? fecha = null, CancellationToken ct = default) =>
        Ok(await _service.ListarLotesAsync(tipo, soloDisponibles, fecha, ct));

    /// <summary>Consulta un lote por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<LoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoteResponse>> ObtenerLote(int id, [FromQuery] DateOnly? fecha, CancellationToken ct) =>
        Ok(await _service.ObtenerLoteAsync(id, fecha, ct));

    /// <summary>Resumen de existencias por producto a una fecha.</summary>
    [HttpGet("resumen")]
    [ProducesResponseType<InventarioResumenResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<InventarioResumenResponse>> Resumen([FromQuery] DateOnly? fecha, CancellationToken ct) =>
        Ok(await _service.ResumenAsync(fecha, ct));

    /// <summary>Alerta de lotes próximos a vencer (por defecto, 3 días).</summary>
    [HttpGet("proximos-a-vencer")]
    [ProducesResponseType<IReadOnlyList<LoteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LoteResponse>>> ProximosAVencer([FromQuery] int dias = 3, [FromQuery] DateOnly? fecha = null,
        CancellationToken ct = default) =>
        Ok(await _service.ProximosAVencerAsync(dias, fecha, ct));

    /// <summary>Procesa vencimientos: el disponible de los lotes vencidos pasa a dañado.</summary>
    [HttpPost("procesar-vencimientos")]
    [ProducesResponseType<ProcesarVencimientosResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcesarVencimientosResponse>> ProcesarVencimientos([FromBody] ProcesarVencimientosRequest? request, CancellationToken ct) =>
        Ok(await _service.ProcesarVencimientosAsync(request ?? new ProcesarVencimientosRequest(), ct));
}
