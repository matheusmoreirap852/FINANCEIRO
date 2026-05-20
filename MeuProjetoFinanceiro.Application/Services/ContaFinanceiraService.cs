using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Application.Services;

public class ContaFinanceiraService : ICrudService<ContaFinanceiraDto>
{
    private readonly IRepository<ContaFinanceira> _repository;
    private readonly IRepository<Transacao> _transacoes;

    public ContaFinanceiraService(IRepository<ContaFinanceira> repository, IRepository<Transacao> transacoes)
    {
        _repository = repository;
        _transacoes = transacoes;
    }

    public async Task<IReadOnlyList<ContaFinanceiraDto>> GetAllAsync()
        => await Task.WhenAll((await _repository.GetAllAsync()).Select(ToDtoAsync));

    public async Task<ContaFinanceiraDto?> GetByIdAsync(int id)
    {
        var conta = await _repository.GetByIdAsync(id);
        return conta is null ? null : await ToDtoAsync(conta);
    }

    public async Task<ContaFinanceiraDto> CreateAsync(ContaFinanceiraDto dto)
        => await ToDtoAsync(await _repository.AddAsync(ToEntity(dto)));

    public async Task<ContaFinanceiraDto> UpdateAsync(ContaFinanceiraDto dto)
        => await ToDtoAsync(await _repository.UpdateAsync(ToEntity(dto)));

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    private async Task<ContaFinanceiraDto> ToDtoAsync(ContaFinanceira conta)
    {
        var transacoes = await _transacoes.GetAllAsync();
        var saldoMovimentado = transacoes
            .Where(t => t.ContaFinanceiraId == conta.Id && t.Efetivada)
            .Sum(t => t.Tipo == TipoTransacao.Despesa ? -t.Valor : t.Valor);

        return new ContaFinanceiraDto
        {
            Id = conta.Id,
            Nome = conta.Nome,
            Instituicao = conta.Instituicao,
            Tipo = conta.Tipo,
            SaldoInicial = conta.SaldoInicial,
            SaldoAtual = conta.SaldoInicial + saldoMovimentado,
            Ativa = conta.Ativa
        };
    }

    private static ContaFinanceira ToEntity(ContaFinanceiraDto dto) => new()
    {
        Id = dto.Id,
        Nome = dto.Nome,
        Instituicao = dto.Instituicao,
        Tipo = dto.Tipo,
        SaldoInicial = dto.SaldoInicial,
        Ativa = dto.Ativa
    };
}
