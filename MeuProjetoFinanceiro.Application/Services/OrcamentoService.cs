using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Application.Services;

public class OrcamentoService : ICrudService<OrcamentoDto>
{
    private readonly IRepository<Orcamento> _repository;
    private readonly IRepository<Transacao> _transacoes;

    public OrcamentoService(IRepository<Orcamento> repository, IRepository<Transacao> transacoes)
    {
        _repository = repository;
        _transacoes = transacoes;
    }

    public async Task<IReadOnlyList<OrcamentoDto>> GetAllAsync()
    {
        var transacoes = await _transacoes.GetAllAsync();
        return (await _repository.GetAllAsync())
            .OrderBy(o => o.Ano)
            .ThenBy(o => o.Mes)
            .Select(o => ToDto(o, transacoes))
            .ToList();
    }

    public async Task<OrcamentoDto?> GetByIdAsync(int id)
    {
        var orcamento = await _repository.GetByIdAsync(id);
        return orcamento is null ? null : ToDto(orcamento, await _transacoes.GetAllAsync());
    }

    public async Task<OrcamentoDto> CreateAsync(OrcamentoDto dto)
        => ToDto(await _repository.AddAsync(ToEntity(dto)), await _transacoes.GetAllAsync());

    public async Task<OrcamentoDto> UpdateAsync(OrcamentoDto dto)
        => ToDto(await _repository.UpdateAsync(ToEntity(dto)), await _transacoes.GetAllAsync());

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    private static OrcamentoDto ToDto(Orcamento orcamento, IReadOnlyList<Transacao> transacoes)
    {
        var realizado = transacoes
            .Where(t => t.CategoriaId == orcamento.CategoriaId
                        && t.Tipo == TipoTransacao.Despesa
                        && t.Data.Month == orcamento.Mes
                        && t.Data.Year == orcamento.Ano
                        && t.Efetivada)
            .Sum(t => t.Valor);

        return new OrcamentoDto
        {
            Id = orcamento.Id,
            Nome = orcamento.Nome,
            Mes = orcamento.Mes,
            Ano = orcamento.Ano,
            ValorPlanejado = orcamento.ValorPlanejado,
            ValorRealizado = realizado,
            CategoriaId = orcamento.CategoriaId,
            CategoriaNome = orcamento.Categoria?.Nome ?? string.Empty
        };
    }

    private static Orcamento ToEntity(OrcamentoDto dto) => new()
    {
        Id = dto.Id,
        Nome = dto.Nome,
        Mes = dto.Mes,
        Ano = dto.Ano,
        ValorPlanejado = dto.ValorPlanejado,
        CategoriaId = dto.CategoriaId
    };
}
