using Lab10_RodrigoApaza.Application.Interfaces;
using Lab10_RodrigoApaza.Infrastructure.Persistence;
using Lab10_RodrigoApaza.Infrastructure.Persistence.DataContext;
using Lab10_RodrigoApaza.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Lab10_RodrigoApaza.Infrastructure.Configuration;

public static class InfrastructureServicesExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registro del DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<TicketeraDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Registro de Repositorios y UnitOfWork
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}