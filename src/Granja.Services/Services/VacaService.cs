using AutoMapper;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
using Granja.Services.Interfaces;

namespace Granja.Services.Services;

public class VacaService : IVacaService
{
    private readonly IVacaRepository _vacas;
    private readonly IMapper _mapper;

    public VacaService(IVacaRepository vacas, IMapper mapper)
    {
        _vacas = vacas;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VacaResponse>> ListarAsync(bool incluirInactivas, CancellationToken ct = default) =>
        _mapper.Map<List<VacaResponse>>(await _vacas.ListarConProduccionAsync(!incluirInactivas, ct));

    public async Task<VacaResponse> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var vaca = await _vacas.ObtenerConProduccionAsync(id, ct) ?? throw new EntidadNoEncontradaException("la vaca", id);
        return _mapper.Map<VacaResponse>(vaca);
    }
}
