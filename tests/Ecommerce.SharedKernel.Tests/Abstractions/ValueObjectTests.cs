namespace Ecommerce.SharedKernel.Tests.Abstractions;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.SharedKernel.Abstractions;

[TestClass]
public sealed class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    private sealed class Address : ValueObject
    {
        public string Line1 { get; }
        public string? Line2 { get; }
        public string City { get; }

        public Address(string line1, string? line2, string city)
        {
            Line1 = line1;
            Line2 = line2;
            City = city;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Line1;
            yield return Line2;
            yield return City;
        }
    }

    [TestMethod]
    public void Equals_SameComponents_ReturnsTrue()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(10m, "USD");

        m1.Equals(m2).Should().BeTrue();
    }

    [TestMethod]
    public void Equals_DifferentComponents_ReturnsFalse()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(20m, "USD");

        m1.Equals(m2).Should().BeFalse();
    }

    [TestMethod]
    public void NullableComponent_ComparisonWorks()
    {
        var a1 = new Address("1 St", null, "City");
        var a2 = new Address("1 St", null, "City");
        var a3 = new Address("1 St", "Apt", "City");

        a1.Equals(a2).Should().BeTrue();
        a1.Equals(a3).Should().BeFalse();
    }
}
