using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Infrastructure.Persistence.DataContext;
using Lab10_RodrigoApaza.Infrastructure.Persistence.Repositories;

namespace Lab10_RodrigoApaza.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly TicketeraDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(TicketeraDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (_repositories.ContainsKey(typeof(T)))
        {
            return (IGenericRepository<T>)_repositories[typeof(T)];
        }

        var repoInstance = new GenericRepository<T>(_context);
        _repositories.Add(typeof(T), repoInstance);
        return repoInstance;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}