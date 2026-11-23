using Granja.Domain.Enums;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Controllers;

/// <summary>Reportes de ventas, leche, producto y vacas.</summary>
[ApiController]
[Route("api/reportes")]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _service;

    public ReportesController(IReporteService service) => _service = service;

    /// <summary>Ventas del período: fecha, producto, cantidad, precio, total y total general.</summary>
    [HttpGet("ventas")]
    public async Task<ActionResult<ReporteVentasResponse>> Ventas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] TipoProducto? tipo, CancellationToken ct) =>
        Ok(await _service.VentasAsync(desde, hasta, tipo, ct));

    /// <summary>Leche total producida por día, litros transformados y no aprovechados.</summary>
    [HttpGet("leche-diaria")]
    public async Task<ActionResult<ReporteLecheDiariaResponse>> LecheDiaria([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        Ok(await _service.LecheDiariaAsync(desde, hasta, ct));

    /// <summary>Producto elaborado por día y por tipo.</summary>
    [HttpGet("producto-diario")]
    public async Task<ActionResult<IReadOnlyList<ProductoDiarioItem>>> ProductoDiario([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        Ok(await _service.ProductoDiarioAsync(desde, hasta, ct));

    /// <summary>Por producto: total producido, vendido, disponible y dañado.</summary>
    [HttpGet("producto-total")]
    public async Task<ActionResult<IReadOnlyList<ProductoTotalItem>>> ProductoTotal(CancellationToken ct) =>
        Ok(await _service.ProductoTotalAsync(ct));

    /// <summary>Producto dañado por lote y por tipo, con fecha de vencimiento.</summary>
    [HttpGet("producto-danado")]
    public async Task<ActionResult<ReporteProductoDanadoResponse>> ProductoDanado([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        Ok(await _service.ProductoDanadoAsync(desde, hasta, ct));

    /// <summary>Vacas: producción promedio, la que más produce, la que menos y las que están por debajo del promedio.</summary>
    [HttpGet("vacas")]
    public async Task<ActionResult<ReporteVacasResponse>> Vacas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken ct) =>
        Ok(await _service.VacasAsync(desde, hasta, ct));
}
