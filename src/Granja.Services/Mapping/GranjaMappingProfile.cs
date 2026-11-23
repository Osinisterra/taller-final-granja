using AutoMapper;
using Granja.Domain.Entities;
using Granja.Services.Dtos;

namespace Granja.Services.Mapping;

/// <summary>Mapeos entidad → DTO. Las entidades de dominio nunca se exponen directamente.</summary>
public class GranjaMappingProfile : Profile
{
    public GranjaMappingProfile()
    {
        CreateMap<ParametroProducto, ParametroProductoDto>();
        CreateMap<GranjaLechera, GranjaResponse>();

        CreateMap<Vaca, VacaResponse>()
            .ForMember(d => d.LitrosProducidos, o => o.MapFrom(s => s.LitrosProducidos))
            .ForMember(d => d.DiasConProduccion, o => o.MapFrom(s => s.Producciones.Count));

        CreateMap<LoteProducto, LoteResponse>()
            .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo))
            .ForMember(d => d.CantidadVendida, o => o.MapFrom(s => s.CantidadVendida))
            .ForMember(d => d.Estado, o => o.MapFrom((s, _, _, ctx) =>
                s.Estado(ctx.Items.TryGetValue(FechaKey, out var f) ? (DateOnly)f : s.FechaProduccion)));

        CreateMap<LoteProducto, LoteDanadoItem>()
            .ForMember(d => d.LoteId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo));

        CreateMap<VentaDetalle, VentaDetalleResponse>()
            .ForMember(d => d.FechaProduccion, o => o.MapFrom(s => s.FechaProduccionLote));

        CreateMap<Venta, VentaResponse>()
            .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo))
            .ForMember(d => d.Detalle, o => o.MapFrom(s => s.Detalles));

        CreateMap<Venta, ReporteVentaItem>()
            .ForMember(d => d.VentaId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo));
    }

    /// <summary>Clave de contexto para indicar la fecha con la que se evalúa el estado de un lote.</summary>
    public const string FechaKey = "fecha";
}
