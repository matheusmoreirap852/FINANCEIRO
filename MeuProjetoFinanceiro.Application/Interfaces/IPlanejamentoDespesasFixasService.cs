using MeuProjetoFinanceiro.Application.Dtos;

namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface IPlanejamentoDespesasFixasService
{
    Task<PlanejamentoDespesasFixasDto> ObterAsync(int ano, CancellationToken cancellationToken = default);
    Task<int> CriarRecorrenteAsync(LancamentoDespesaFixaDto dto, CancellationToken cancellationToken = default);
}
