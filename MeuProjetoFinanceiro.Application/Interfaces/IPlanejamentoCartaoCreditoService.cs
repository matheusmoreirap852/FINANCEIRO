using MeuProjetoFinanceiro.Application.Dtos;

namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface IPlanejamentoCartaoCreditoService
{
    Task<PlanejamentoCartaoCreditoDto> ObterAsync(int ano, CancellationToken cancellationToken = default);
    Task CriarLancamentoAsync(LancamentoCartaoCreditoDto dto, CancellationToken cancellationToken = default);
    Task CriarReceitaAsync(ReceitaMensalDto dto, CancellationToken cancellationToken = default);
    Task CriarOuAtualizarFaturaAsync(FaturaCartaoCreditoDto dto, CancellationToken cancellationToken = default);
    Task RemoverLancamentoAsync(int id, CancellationToken cancellationToken = default);
}
