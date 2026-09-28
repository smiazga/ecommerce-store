namespace Ecommerce.SharedKernel.Tests.Results;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.SharedKernel.Results;

[TestClass]
public sealed class ResultOfTTests
{
    [TestMethod]
    public void Success_WithValue_ReturnsResult()
    {
        var r = Result<string>.Success("v");
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be("v");
    }

    [TestMethod]
    public void Failure_WithError_ReturnsFailure()
    {
        var r = Result<string>.Failure("E","M");
        r.IsSuccess.Should().BeFalse();
        r.Value.Should().BeNull();
        r.Error.Should().NotBeNull();
    }

    [TestMethod]
    public void Map_OnSuccess_Transforms()
    {
        var r = Result<int>.Success(2);
        var mapped = r.Map(x => x * 3);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(6);
    }
}
