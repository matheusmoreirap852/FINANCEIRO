using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Common;
using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoFinanceiro.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        var query = _dbSet.AsQueryable();

        if (typeof(TEntity) == typeof(Transacao))
        {
            query = query.Include(nameof(Transacao.ContaFinanceira)).Include(nameof(Transacao.Categoria));
        }

        if (typeof(TEntity) == typeof(Orcamento))
        {
            query = query.Include(nameof(Orcamento.Categoria));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null)
        {
            return false;
        }

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
