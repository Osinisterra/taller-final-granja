using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

/// <summary>Vacas de la granja (se generan a partir de n).</summary>
[ApiController]
[Route("api/vacas")]
[Produces("application/json")]
public class VacasController : ControllerBase
{
    private readonly IVacaService _service;

    public VacasController(IVacaService service) => _service = service;

    /// <summary>Listado paginado de vacas con su producción acumulada.</summary>
    [HttpGet]
    [ProducesResponseType<PaginaResponse<VacaResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResponse<VacaResponse>>> Listar([FromQuery] PaginacionRequest paginacion,
        [FromQuery] bool incluirInactivas = false, CancellationToken ct = default) =>
        Ok(await _service.ListarAsync(incluirInactivas, paginacion, ct));

    /// <summary>Consulta una vaca por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<VacaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacaResponse>> Obtener(int id, CancellationToken ct) =>
        Ok(await _service.ObtenerAsync(id, ct));
}
