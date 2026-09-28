namespace Ecommerce.SharedKernel.Tests.Abstractions;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.SharedKernel.Abstractions;

[TestClass]
public sealed class AggregateRootTests
{
    private sealed class TestDomainEvent : DomainEvent
    {
        public string Message { get; }

        public TestDomainEvent(string message) : base()
        {
            Message = message;
        }
    }

    private sealed class TestAggregate : AggregateRoot
    {
        public TestAggregate() : base()
        {
        }

        public TestAggregate(Guid id) : base(id)
        {
        }

        public void RaiseEvent(string message)
        {
            RaiseDomainEvent(new TestDomainEvent(message));
        }
    }

    [TestMethod]
    public void Constructor_NoArgs_GeneratesNewId()
    {
        // Act
        var aggregate = new TestAggregate();

        // Assert
        aggregate.Id.Should().NotBe(Guid.Empty);
    }

    [TestMethod]
    public void DomainEvents_Empty_ReturnsEmptyCollection()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act
        var events = aggregate.DomainEvents;

        // Assert
        events.Should().BeEmpty();
    }

    [TestMethod]
    public void RaiseDomainEvent_AddsEvent()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act
        aggregate.RaiseEvent("test");

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
    }

    [TestMethod]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.RaiseEvent("e1");
        aggregate.RaiseEvent("e2");

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }
}
