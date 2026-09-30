namespace Ecommerce.Web.Extensions;

using Ecommerce.Web.Clients;
using Ecommerce.Web.Services.Products;

/// <summary>
/// Extension methods for registering Ecommerce.Web services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers product API client and service for dependency injection.
    /// </summary>
    /// <param name="services">The service collection to register services in.</param>
    /// <param name="apiBaseUrl">The base URL for the product API (e.g., https://api.example.com/api/products).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProductServices(
        this IServiceCollection services,
        string apiBaseUrl)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(apiBaseUrl, nameof(apiBaseUrl));

        // Register the typed HttpClient
        services
            .AddHttpClient<IProductApiClient, ProductApiClient>(client =>
            {
                // Set base address for product API endpoints
                client.BaseAddress = new Uri(apiBaseUrl);

                // Add default headers
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Ecommerce.Web/1.0");

                // Set reasonable timeouts
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Register the business service layer
        services.AddScoped<IProductService, ProductService>();

        return services;
    }

    /// <summary>
    /// Registers product API client and service with custom HttpClient configuration.
    /// Allows full control over HttpClient setup (for logging, retry policies, etc.).
    /// </summary>
    /// <param name="services">The service collection to register services in.</param>
    /// <param name="apiBaseUrl">The base URL for the product API.</param>
    /// <param name="configureClient">Action to configure the HttpClient instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProductServices(
        this IServiceCollection services,
        string apiBaseUrl,
        Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(apiBaseUrl, nameof(apiBaseUrl));
        ArgumentNullException.ThrowIfNull(configureClient, nameof(configureClient));

        // Register the typed HttpClient with custom configuration
        services
            .AddHttpClient<IProductApiClient, ProductApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                configureClient(client);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Register the business service layer
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
