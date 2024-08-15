using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Common;

public interface IBaseRepository<T, TKey> where T : class
{
    Task<T?> FindAsync(TKey id);
    IQueryable<T> FindAll();
    IQueryable<T> GetAll(string entityTypeName);
    IQueryable<T> Get(Expression<Func<T, bool>> where);
    IQueryable<T> Get(Expression<Func<T, bool>> where, params Expression<Func<T, object?>>[] includes);
    IQueryable<T> Get(string entityTypeName, Expression<Func<T, bool>> where);
    Task AddAsync(T entity);
    Task AddAsync(T entity, string entityTypeName);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}

public class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class
{
    protected ApplicationDbContext _applicationDbContext;
    protected DbSet<T> dbSet;
    //protected readonly ILogger _logger;

    public BaseRepository(ApplicationDbContext applicationDbContext
        //,ILoggerFactory logFactory
        )
    {
        _applicationDbContext = applicationDbContext;
        dbSet = _applicationDbContext.Set<T>();
        //_logger = logFactory.CreateLogger("logs");
    }

    public virtual async Task AddAsync(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await dbSet.AddRangeAsync(entities);
    }

    public virtual async Task AddAsync(T entity, string entityTypeName)
    {
        await _applicationDbContext.Set<T>(entityTypeName).AddAsync(entity);
    }

    public virtual IQueryable<T> FindAll()
    {
        return dbSet.AsNoTracking();
    }

    public virtual async Task<T?> FindAsync(TKey id)
    {
        return await dbSet.FindAsync(id);
    }

    public virtual IQueryable<T> Get(Expression<Func<T, bool>> where)
    {
        return dbSet.Where(where);
    }

    public virtual IQueryable<T> Get(Expression<Func<T, bool>> where, params Expression<Func<T, object?>>[] includes)
    {
        var result = dbSet.Where(where);
        foreach (var include in includes)
        {
            result = result.Include(include);
        }
        return result;
    }

    public virtual IQueryable<T> Get( string entityTypeName,Expression<Func<T, bool>> where)
    {     
        return _applicationDbContext.Set<T>(entityTypeName).Where(where);
    }

    public virtual void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        dbSet.RemoveRange(entities);
    }

    public virtual void Update(T entity)
    {
        _applicationDbContext.Entry<T>(entity).State = EntityState.Modified;
    }

    public virtual IQueryable<T> GetAll(string entityTypeName)
    {
        return _applicationDbContext.Set<T>(entityTypeName);
    }
}
