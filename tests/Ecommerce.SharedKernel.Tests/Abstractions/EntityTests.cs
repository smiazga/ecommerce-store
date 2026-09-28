namespace Ecommerce.SharedKernel.Tests.Abstractions;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.SharedKernel.Abstractions;

[TestClass]
public sealed class EntityTests
{
    private sealed class TestEntity : Entity
    {
        public TestEntity() : base()
        {
        }

        public TestEntity(Guid id) : base(id)
        {
        }
    }

    [TestMethod]
    public void Constructor_NoArgs_GeneratesNewId()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [TestMethod]
    public void Constructor_WithId_AssignsId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestEntity(id);

        // Assert
        entity.Id.Should().Be(id);
    }

    [TestMethod]
    public void Constructor_WithEmptyGuid_ThrowsException()
    {
        // Act
        var action = () => new TestEntity(Guid.Empty);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Equals_SameId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_DifferentId_ReturnsFalse()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.Should().BeFalse();
    }
}
