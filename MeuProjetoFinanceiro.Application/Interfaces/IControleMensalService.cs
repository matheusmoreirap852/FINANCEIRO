using MeuProjetoFinanceiro.Application.Dtos;

namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface IControleMensalService
{
    Task<ControleMensalDto> ObterAsync(int ano, CancellationToken cancellationToken = default);
}
