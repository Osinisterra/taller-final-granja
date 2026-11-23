using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;

namespace Granja.Services.Export;

/// <summary>Exporta los reportes a CSV reutilizando exactamente los datos de <see cref="IReporteService"/>.</summary>
public class ReporteExportService : IReporteExportService
{
    /// <summary>Reportes exportables, identificados por el mismo segmento de ruta que usan en /api/reportes.</summary>
    public static readonly IReadOnlyList<string> Reportes =
        ["ventas", "leche-diaria", "producto-diario", "producto-total", "producto-danado", "vacas"];

    private readonly IReporteService _reportes;

    public ReporteExportService(IReporteService reportes) => _reportes = reportes;

    public async Task<ArchivoExportado> ExportarCsvAsync(string reporte, DateOnly? desde, DateOnly? hasta, TipoProducto? tipo,
        CancellationToken ct = default)
    {
        var nombre = reporte.Trim().ToLowerInvariant();
        var csv = nombre switch
        {
            "ventas" => Ventas(await _reportes.VentasAsync(desde, hasta, tipo, ct)),
            "leche-diaria" => LecheDiaria(await _reportes.LecheDiariaAsync(desde, hasta, ct)),
            "producto-diario" => ProductoDiario(await _reportes.ProductoDiarioAsync(desde, hasta, ct)),
            "producto-total" => ProductoTotal(await _reportes.ProductoTotalAsync(ct)),
            "producto-danado" => ProductoDanado(await _reportes.ProductoDanadoAsync(desde, hasta, ct)),
            "vacas" => Vacas(await _reportes.VacasAsync(desde, hasta, ct)),
            _ => throw new DomainException($"Reporte '{reporte}' no válido. Valores permitidos: {string.Join(", ", Reportes)}.")
        };

        var sufijo = desde.HasValue || hasta.HasValue ? $"_{desde:yyyyMMdd}-{hasta:yyyyMMdd}" : string.Empty;
        return new ArchivoExportado($"reporte-{nombre}{sufijo}.csv", "text/csv", csv.ToBytes());
    }

    private static CsvBuilder Ventas(ReporteVentasResponse r)
    {
        var csv = new CsvBuilder().Fila("VentaId", "Fecha", "Producto", "Cantidad", "PrecioUnitario", "Total");
        foreach (var v in r.Ventas) csv.Fila(v.VentaId, v.Fecha, v.TipoProducto, v.Cantidad, v.PrecioUnitario, v.Total);
        csv.FilaVacia().Fila("Producto", "CantidadTotal", "Total");
        foreach (var t in r.TotalesPorProducto) csv.Fila(t.TipoProducto, t.Cantidad, t.Total);
        return csv.Fila("TOTAL GENERAL", null, r.TotalGeneral);
    }

    private static CsvBuilder LecheDiaria(ReporteLecheDiariaResponse r)
    {
        var csv = new CsvBuilder().Fila("Fecha", "NumeroVacas", "LitrosProducidos", "LitrosTransformados", "LitrosNoAprovechados");
        foreach (var d in r.Dias) csv.Fila(d.Fecha, d.NumeroVacas, d.LitrosProducidos, d.LitrosTransformados, d.LitrosNoAprovechados);
        return csv.Fila("TOTAL", null, r.TotalProducido, r.TotalTransformado, r.TotalNoAprovechado);
    }

    private static CsvBuilder ProductoDiario(IReadOnlyList<ProductoDiarioItem> r)
    {
        var csv = new CsvBuilder().Fila("Fecha", "Producto", "Unidad", "Lotes", "LitrosUsados", "CantidadProducida");
        foreach (var d in r) csv.Fila(d.Fecha, d.TipoProducto, d.Unidad, d.Lotes, d.LitrosUsados, d.CantidadProducida);
        return csv;
    }

    private static CsvBuilder ProductoTotal(IReadOnlyList<ProductoTotalItem> r)
    {
        var csv = new CsvBuilder().Fila("Producto", "Unidad", "Producido", "Vendido", "Disponible", "Danado", "Ingresos");
        foreach (var p in r) csv.Fila(p.TipoProducto, p.Unidad, p.Producido, p.Vendido, p.Disponible, p.Danado, p.Ingresos);
        return csv;
    }

    private static CsvBuilder ProductoDanado(ReporteProductoDanadoResponse r)
    {
        var csv = new CsvBuilder().Fila("LoteId", "Producto", "FechaProduccion", "FechaVencimiento", "CantidadDanada");
        foreach (var l in r.Lotes) csv.Fila(l.LoteId, l.TipoProducto, l.FechaProduccion, l.FechaVencimiento, l.CantidadDanada);
        csv.FilaVacia().Fila("Producto", "TotalDanado");
        foreach (var t in r.TotalPorTipo) csv.Fila(t.TipoProducto, t.Cantidad);
        return csv;
    }

    private static CsvBuilder Vacas(ReporteVacasResponse r)
    {
        var bajo = r.BajoPromedio.Select(v => v.VacaId).ToHashSet();
        var csv = new CsvBuilder().Fila("VacaId", "Nombre", "LitrosTotales", "DiasProducidos", "PromedioDiario", "BajoPromedio");
        foreach (var v in r.Vacas) csv.Fila(v.VacaId, v.Nombre, v.LitrosTotales, v.DiasProducidos, v.PromedioDiario, bajo.Contains(v.VacaId));
        return csv.FilaVacia()
            .Fila("TotalLitros", r.TotalLitros)
            .Fila("PromedioPorVaca", r.PromedioPorVaca)
            .Fila("PromedioDiarioPorVaca", r.PromedioDiarioPorVaca)
            .Fila("MayorProduccion", r.MayorProduccion?.Nombre, r.MayorProduccion?.LitrosTotales)
            .Fila("MenorProduccion", r.MenorProduccion?.Nombre, r.MenorProduccion?.LitrosTotales);
    }
}
