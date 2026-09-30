namespace Ecommerce.Api.Tests.Endpoints.Products;

using System.Net;
using System.Net.Http.Json;

using Ecommerce.Contracts.Catalog;
using Ecommerce.Infrastructure.Persistence;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class CreateProductEndpointTests
{
    private WebApplicationFactory<Program>? _factory;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    services.RemoveAll(typeof(DbContextOptions<EcommerceDbContext>));
                    services.RemoveAll(typeof(EcommerceDbContext));

                    // Register InMemory DbContext for testing using a unique database per test to avoid cross-test pollution
                    var dbName = $"TestDb_{Guid.NewGuid():N}";
                    services.AddDbContext<EcommerceDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });
                });
            });
    }

    [TestMethod]
    public async Task CreateProduct_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var client = _factory!.CreateClient();

        var request = new CreateProductRequest(
            Sku: "TEST-001",
            Name: "Test Product",
            Price: 12.34m,
            CategoryId: Guid.NewGuid(),
            Description: "Integration test product",
            Currency: "USD");

        // Act
        var response = await client.PostAsJsonAsync("/api/products", request);

        // Capture response body for diagnosis
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert - include response body in failure message to ease debugging
        response.StatusCode.Should().Be(HttpStatusCode.Created, "Response body: {0}", responseBody);

        var created = await response.Content.ReadFromJsonAsync<CreateProductResponse>();
        created.Should().NotBeNull();
        created!.Sku.Should().Be(request.Sku);
        created.Name.Should().Be(request.Name);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _factory?.Dispose();
    }
}
