using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

/// <summary>Configuración de la granja: n, l y parámetros por producto.</summary>
[ApiController]
[Route("api/granja")]
[Produces("application/json")]
public class GranjaController : ControllerBase
{
    private readonly IGranjaService _service;

    public GranjaController(IGranjaService service) => _service = service;

    /// <summary>Consulta la configuración (n, l) y los parámetros por producto.</summary>
    [HttpGet]
    [ProducesResponseType<GranjaResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<GranjaResponse>> Obtener(CancellationToken ct) =>
        Ok(await _service.ObtenerAsync(ct));

    /// <summary>Actualiza n, l y los parámetros de rendimiento, vida útil y precio.</summary>
    [HttpPut]
    [ProducesResponseType<GranjaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GranjaResponse>> Actualizar([FromBody] ActualizarGranjaRequest request, CancellationToken ct) =>
        Ok(await _service.ActualizarAsync(request, ct));
}
