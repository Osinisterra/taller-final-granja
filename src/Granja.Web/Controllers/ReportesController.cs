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
    private readonly IReporteExportService _export;

    public ReportesController(IReporteService service, IReporteExportService export)
    {
        _service = service;
        _export = export;
    }

    /// <summary>
    /// Exporta un reporte a CSV. Valores de {reporte}: ventas, leche-diaria, producto-diario,
    /// producto-total, producto-danado, vacas. Acepta los mismos filtros que el reporte en JSON.
    /// </summary>
    [HttpGet("{reporte}/csv")]
    [Produces("text/csv", "application/problem+json")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "text/csv")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportarCsv(string reporte, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] TipoProducto? tipo, CancellationToken ct)
    {
        var archivo = await _export.ExportarCsvAsync(reporte, desde, hasta, tipo, ct);
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

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
