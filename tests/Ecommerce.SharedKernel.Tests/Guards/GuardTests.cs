namespace Ecommerce.SharedKernel.Tests.Guards;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.SharedKernel.Guards;

[TestClass]
public sealed class GuardTests
{
    [TestMethod]
    public void NullOrEmpty_Valid_DoesNotThrow()
    {
        Action act = () => Guard.NullOrEmpty("ok","p");
        act.Should().NotThrow();
    }

    [TestMethod]
    public void NullOrEmpty_Null_Throws()
    {
        Action act = () => Guard.NullOrEmpty(null!,"p");
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void Null_Null_Throws()
    {
        Action act = () => Guard.Null<object>(null!,"p");
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void NegativeOrZero_Decimal_Zero_Throws()
    {
        Action act = () => Guard.NegativeOrZero(0m,"p");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void OutOfRange_Below_Throws()
    {
        Action act = () => Guard.OutOfRange(-1m,0m,10m,"p");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
