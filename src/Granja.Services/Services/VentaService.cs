using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;

namespace Granja.Services.Services;

public class VentaService : IVentaService
{
    private readonly ILoteRepository _lotes;
    private readonly IVentaRepository _ventas;
    private readonly IGranjaRepository _granjas;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public VentaService(ILoteRepository lotes, IVentaRepository ventas, IGranjaRepository granjas,
        IUnitOfWork uow, IClock clock, IMapper mapper)
    {
        _lotes = lotes;
        _ventas = ventas;
        _granjas = granjas;
        _uow = uow;
        _clock = clock;
        _mapper = mapper;
    }

    /// <summary>
    /// Venta FIFO (PDF §7): toma primero del lote más antiguo no vencido; si las existencias
    /// no alcanzan se rechaza sin modificar nada (RN-07). Todo se guarda en una sola transacción.
    /// </summary>
    public async Task<VentaResponse> RegistrarVentaAsync(VentaRequest request, CancellationToken ct = default)
    {
        if (request.Cantidad <= 0) throw new DomainException("La cantidad a vender debe ser mayor que cero.");

        var fecha = request.Fecha ?? _clock.Today;
        var lotes = await _lotes.ObtenerVendiblesAsync(request.TipoProducto, fecha, ct); // FechaProduccion ASC

        var disponible = lotes.Sum(l => l.CantidadDisponible);
        if (disponible < request.Cantidad)
            throw new InventarioInsuficienteException(request.TipoProducto, request.Cantidad, disponible);

        var granja = await GranjaService.ObtenerGranjaAsync(_granjas, ct);
        var precio = granja.ParametroDe(request.TipoProducto).Precio;

        var venta = new Venta(fecha, request.TipoProducto, precio);
        var restante = request.Cantidad;
        foreach (var lote in lotes)
        {
            if (restante == 0) break;
            var tomar = Math.Min(lote.CantidadDisponible, restante);
            venta.AgregarDetalle(lote, tomar);
            restante -= tomar;
        }

        await _ventas.AgregarAsync(venta, ct);
        await _uow.SaveChangesAsync(ct); // una sola transacción: venta + detalles + lotes
        return _mapper.Map<VentaResponse>(venta);
    }

    public async Task<VentaResponse> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var venta = await _ventas.ObtenerConDetalleAsync(id, ct) ?? throw new EntidadNoEncontradaException("la venta", id);
        return _mapper.Map<VentaResponse>(venta);
    }

    public async Task<IReadOnlyList<VentaResponse>> ListarAsync(DateOnly? desde, DateOnly? hasta, TipoProducto? tipo, CancellationToken ct = default)
    {
        Validaciones.Rango(desde, hasta);
        return _mapper.Map<List<VentaResponse>>(await _ventas.BuscarAsync(desde, hasta, tipo, ct));
    }
}
