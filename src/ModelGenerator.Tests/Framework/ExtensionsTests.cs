using System;
using ModelGenerator.Framework;
using NUnit.Framework;
using Shouldly;

namespace ModelGenerator.Tests.Framework
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void ToSitecoreId_GivenGuid_ReturnsExpectedString()
        {
            const string expected = "{E0B17449-638F-49A5-A571-63F79058E5A0}";
            var guid = Guid.Parse(expected);

            var result = guid.ToSitecoreId();

            result.ShouldBeEquivalentTo(expected);
        }
    }
}