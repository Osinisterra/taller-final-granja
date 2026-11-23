using Granja.Domain.Enums;
using Granja.Services.Dtos;

namespace Granja.Services.Interfaces;

public interface IGranjaService
{
    Task<GranjaResponse> ObtenerAsync(CancellationToken ct = default);
    Task<GranjaResponse> ActualizarAsync(ActualizarGranjaRequest request, CancellationToken ct = default);
}

public interface IVacaService
{
    Task<PaginaResponse<VacaResponse>> ListarAsync(bool incluirInactivas, PaginacionRequest paginacion, CancellationToken ct = default);
    Task<VacaResponse> ObtenerAsync(int id, CancellationToken ct = default);
}

public interface IProduccionService
{
    Task<ProduccionDiaResponse> RegistrarProduccionAsync(RegistrarProduccionRequest request, CancellationToken ct = default);
    Task<ProduccionDiaResponse> ObtenerDiaAsync(DateOnly fecha, CancellationToken ct = default);
    Task<PaginaResponse<ProduccionDiaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, PaginacionRequest paginacion, CancellationToken ct = default);
    Task<LoteResponse> TransformarAsync(TransformacionRequest request, CancellationToken ct = default);
}

public interface IInventarioService
{
    Task<PaginaResponse<LoteResponse>> ListarLotesAsync(FiltroLotesRequest filtro, CancellationToken ct = default);
    Task<LoteResponse> ObtenerLoteAsync(int id, DateOnly? fecha, CancellationToken ct = default);
    Task<InventarioResumenResponse> ResumenAsync(DateOnly? fecha, CancellationToken ct = default);
    Task<IReadOnlyList<LoteResponse>> ProximosAVencerAsync(int dias, DateOnly? fecha, CancellationToken ct = default);
    Task<ProcesarVencimientosResponse> ProcesarVencimientosAsync(ProcesarVencimientosRequest request, CancellationToken ct = default);
}

public interface IVentaService
{
    Task<VentaResponse> RegistrarVentaAsync(VentaRequest request, CancellationToken ct = default);
    Task<VentaResponse> ObtenerAsync(int id, CancellationToken ct = default);
    Task<PaginaResponse<VentaResponse>> ListarAsync(FiltroVentasRequest filtro, CancellationToken ct = default);
}

public interface IReporteService
{
    Task<ReporteVentasResponse> VentasAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
    Task<ReporteLecheDiariaResponse> LecheDiariaAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<IReadOnlyList<ProductoDiarioItem>> ProductoDiarioAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<IReadOnlyList<ProductoTotalItem>> ProductoTotalAsync(CancellationToken ct = default);
    Task<ReporteProductoDanadoResponse> ProductoDanadoAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<ReporteVacasResponse> VacasAsync(DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
}

public interface IReporteExportService
{
    /// <summary>Exporta un reporte (ventas, leche-diaria, producto-diario, producto-total, producto-danado, vacas) a CSV.</summary>
    Task<ArchivoExportado> ExportarCsvAsync(string reporte, DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default);
}
