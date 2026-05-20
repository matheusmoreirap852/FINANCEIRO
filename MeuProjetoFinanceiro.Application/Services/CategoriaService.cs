using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;

namespace MeuProjetoFinanceiro.Application.Services;

public class CategoriaService : ICrudService<CategoriaDto>
{
    private readonly IRepository<Categoria> _repository;

    public CategoriaService(IRepository<Categoria> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CategoriaDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(ToDto).ToList();

    public async Task<CategoriaDto?> GetByIdAsync(int id)
    {
        var categoria = await _repository.GetByIdAsync(id);
        return categoria is null ? null : ToDto(categoria);
    }

    public async Task<CategoriaDto> CreateAsync(CategoriaDto dto)
        => ToDto(await _repository.AddAsync(ToEntity(dto)));

    public async Task<CategoriaDto> UpdateAsync(CategoriaDto dto)
        => ToDto(await _repository.UpdateAsync(ToEntity(dto)));

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    private static CategoriaDto ToDto(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Tipo = categoria.Tipo,
        Cor = categoria.Cor,
        Ativa = categoria.Ativa
    };

    private static Categoria ToEntity(CategoriaDto dto) => new()
    {
        Id = dto.Id,
        Nome = dto.Nome,
        Tipo = dto.Tipo,
        Cor = dto.Cor,
        Ativa = dto.Ativa
    };
}
