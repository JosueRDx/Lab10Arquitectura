namespace Lab10_RodrigoApaza.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Método genérico para obtener repositorios
    IGenericRepository<T> Repository<T>() where T : class;

    // Método para guardar cambios en la BD
    Task<int> SaveChangesAsync();
}