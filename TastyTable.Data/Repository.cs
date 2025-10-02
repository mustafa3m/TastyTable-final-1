using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TastyTable.Core.Interfaces;

namespace TastyTable.Data;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    internal DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();
    public void Remove(T entity) => _dbSet.Remove(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}