namespace Ecommerce.SharedKernel.Tests.Results;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.SharedKernel.Results;

[TestClass]
public sealed class ResultTests
{
    [TestMethod]
    public void Success_ReturnsSuccess()
    {
        var r = Result.Success();
        r.IsSuccess.Should().BeTrue();
        r.Error.Should().BeNull();
    }

    [TestMethod]
    public void Failure_WithError_ReturnsFailure()
    {
        var error = new Error("CODE", "Msg");
        var r = Result.Failure(error);
        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Be(error);
    }

    [TestMethod]
    public void Failure_WithCodeMessage_ReturnsFailure()
    {
        var r = Result.Failure("C","M");
        r.IsSuccess.Should().BeFalse();
        r.Error.Should().NotBeNull();
        r.Error!.Code.Should().Be("C");
    }
}
