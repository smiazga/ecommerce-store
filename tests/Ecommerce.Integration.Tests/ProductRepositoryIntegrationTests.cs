namespace Ecommerce.Integration.Tests;

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
    [TestMethod]
    public async Task AddProduct_PersistsAndCanBeQueried()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddInfrastructurePersistence(options => options.UseInMemoryDatabase("test_db_add_1"));

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
        services.AddInfrastructurePersistence(options => options.UseInMemoryDatabase("test_db_images_1"));

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
