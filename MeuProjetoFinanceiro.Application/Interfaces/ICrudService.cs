namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface ICrudService<TDto>
{
    Task<IReadOnlyList<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(int id);
    Task<TDto> CreateAsync(TDto dto);
    Task<TDto> UpdateAsync(TDto dto);
    Task<bool> DeleteAsync(int id);
}
