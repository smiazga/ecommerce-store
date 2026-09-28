namespace Ecommerce.SharedKernel.Tests.Exceptions;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ecommerce.SharedKernel.Exceptions;
using System.Collections.Generic;

[TestClass]
public sealed class DomainExceptionTests
{
    [TestMethod]
    public void DomainException_Message_IsSet()
    {
        var ex = new DomainException("msg");
        ex.Message.Should().Be("msg");
    }

    [TestMethod]
    public void DomainValidationException_SingleError_CreatesList()
    {
        var ex = new DomainValidationException("err");
        ex.Errors.Should().ContainSingle("err");
    }

    [TestMethod]
    public void DomainValidationException_Null_Throws()
    {
        Action act = () => new DomainValidationException((IEnumerable<string>)null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
