using MeuProjetoFinanceiro.Application.Dtos;

namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface IImportacaoPlanilhaFinanceiraService
{
    Task<ImportacaoPlanilhaResultadoDto> ImportarAsync(Stream arquivo, string nomeArquivo, CancellationToken cancellationToken = default);
}
