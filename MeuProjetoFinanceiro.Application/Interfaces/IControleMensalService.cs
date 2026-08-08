using MeuProjetoFinanceiro.Application.Dtos;

namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface IControleMensalService
{
    Task<ControleMensalDto> ObterAsync(int ano, CancellationToken cancellationToken = default);
    Task AtualizarValoresAsync(int ano, IEnumerable<ControleMensalValorDto> valores, CancellationToken cancellationToken = default);
    Task RemoverEntradaAsync(int ano, string nome, CancellationToken cancellationToken = default);
    Task RemoverLinhaAsync(int ano, string grupo, string nome, CancellationToken cancellationToken = default);
}
