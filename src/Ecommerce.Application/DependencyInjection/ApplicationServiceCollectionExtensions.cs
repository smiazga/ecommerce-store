namespace Ecommerce.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

/// <summary>
/// Extension methods for registering Application layer services into the dependency injection container.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container.
    /// Registers MediatR and FluentValidation with automatic handler and validator discovery.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        // Register MediatR with automatic handler registration from this assembly
        // MediatR will scan for all IRequestHandler<TRequest, TResponse> implementations
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);
        });

        // Register FluentValidation with automatic validator discovery from this assembly
        // ValidatorRegistry will scan for all IValidator<T> implementations
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);

        return services;
    }
}
