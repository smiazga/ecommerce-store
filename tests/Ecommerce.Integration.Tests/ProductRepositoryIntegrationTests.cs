namespace Ecommerce.Integration.Tests;

using System;
using System.Threading.Tasks;

using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Infrastructure.DependencyInjection;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.SharedKernel.Results;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class ProductRepositoryIntegrationTests
{
    private static string? _connectionString;
    private static string? _databaseName;

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        // Build a unique test database name per test run to avoid collisions
        var server = Environment.GetEnvironmentVariable("INTEGRATION_TEST_SQL_SERVER") ?? "STEVEM4";
        _databaseName = $"EcommerceDb_Test_{Guid.NewGuid():N}";
        _connectionString = $"Data Source={server};Database={_databaseName};Integrated Security=True;Encrypt=False;MultipleActiveResultSets=True";

        var services = new ServiceCollection();
        services.AddInfrastructurePersistence(options => options.UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure().CommandTimeout(180)));
        var provider = services.BuildServiceProvider();

        // Ensure database is created and migrations applied before any tests run
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        db.Database.Migrate();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        if (string.IsNullOrWhiteSpace(_connectionString)) return;

        var services = new ServiceCollection();
        services.AddInfrastructurePersistence(options => options.UseSqlServer(_connectionString));
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        // Clean up test database after tests complete
        db.Database.EnsureDeleted();
    }
    [TestMethod]
    public async Task AddProduct_PersistsAndCanBeQueried()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = _connectionString ?? throw new InvalidOperationException("Test database connection string is not initialized.");
        services.AddInfrastructurePersistence(options => options.UseSqlServer(connectionString));

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();

        var sku = new Sku("INT-SKU-1");
        var price = new Money(9.99m, "USD");
        var categoryId = Guid.NewGuid();

        var createResult = Product.Create(sku, "Integration Product", price, categoryId);
        createResult.IsSuccess.Should().BeTrue();
        var product = createResult.Unwrap();

        // Act
        await repo.AddAsync(product);
        await db.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(product.Id);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(product.Id);
        fetched.Sku.Value.Should().Be(product.Sku.Value);
        fetched.Price.Amount.Should().Be(product.Price.Amount);
    }

    [TestMethod]
    public async Task AddProduct_WithImages_PersistsImages()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = _connectionString ?? throw new InvalidOperationException("Test database connection string is not initialized.");
        services.AddInfrastructurePersistence(options => options.UseSqlServer(connectionString));

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();

        var sku = new Sku("INT-SKU-IMG");
        var price = new Money(5.00m, "USD");
        var categoryId = Guid.NewGuid();

        var product = Product.Create(sku, "With Images", price, categoryId).Unwrap();
        product.AddImage("http://example.com/1.jpg", 0);
        product.AddImage("http://example.com/2.jpg", 1);

        // Act
        await repo.AddAsync(product);
        await db.SaveChangesAsync();

        var fetched = await repo.GetBySkuAsync(sku);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.Images.Should().HaveCount(2);
        fetched.Images.Should().Contain(i => i.ImageUrl == "http://example.com/1.jpg");
    }
}
