namespace Ecommerce.Infrastructure.DependencyInjection;

using Ecommerce.Application.Common.Persistence;
using Ecommerce.Domain.Repositories;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service collection extensions for Infrastructure registrations.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure persistence services to the service collection.
    /// The caller must provide a DbContext configuration action (e.g., options => options.UseSqlServer(conn)).
    /// </summary>
    public static IServiceCollection AddInfrastructurePersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (configureDb is null) throw new ArgumentNullException(nameof(configureDb));

        services.AddDbContext<EcommerceDbContext>(configureDb);

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
