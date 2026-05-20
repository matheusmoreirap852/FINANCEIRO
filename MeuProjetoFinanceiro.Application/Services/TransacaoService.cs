using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;

namespace MeuProjetoFinanceiro.Application.Services;

public class TransacaoService : ICrudService<TransacaoDto>
{
    private readonly IRepository<Transacao> _repository;

    public TransacaoService(IRepository<Transacao> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TransacaoDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).OrderByDescending(t => t.Data).Select(ToDto).ToList();

    public async Task<TransacaoDto?> GetByIdAsync(int id)
    {
        var transacao = await _repository.GetByIdAsync(id);
        return transacao is null ? null : ToDto(transacao);
    }

    public async Task<TransacaoDto> CreateAsync(TransacaoDto dto)
        => ToDto(await _repository.AddAsync(ToEntity(dto)));

    public async Task<TransacaoDto> UpdateAsync(TransacaoDto dto)
        => ToDto(await _repository.UpdateAsync(ToEntity(dto)));

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    public static TransacaoDto ToDto(Transacao transacao) => new()
    {
        Id = transacao.Id,
        Descricao = transacao.Descricao,
        Valor = transacao.Valor,
        Data = transacao.Data,
        Tipo = transacao.Tipo,
        Efetivada = transacao.Efetivada,
        Observacao = transacao.Observacao,
        ContaFinanceiraId = transacao.ContaFinanceiraId,
        ContaNome = transacao.ContaFinanceira?.Nome ?? string.Empty,
        CategoriaId = transacao.CategoriaId,
        CategoriaNome = transacao.Categoria?.Nome ?? string.Empty
    };

    private static Transacao ToEntity(TransacaoDto dto) => new()
    {
        Id = dto.Id,
        Descricao = dto.Descricao,
        Valor = dto.Valor,
        Data = dto.Data,
        Tipo = dto.Tipo,
        Efetivada = dto.Efetivada,
        Observacao = dto.Observacao,
        ContaFinanceiraId = dto.ContaFinanceiraId,
        CategoriaId = dto.CategoriaId
    };
}
