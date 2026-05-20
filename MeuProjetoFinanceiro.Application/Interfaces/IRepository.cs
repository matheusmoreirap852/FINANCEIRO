using MeuProjetoFinanceiro.Core.Common;

namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(int id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(int id);
}
