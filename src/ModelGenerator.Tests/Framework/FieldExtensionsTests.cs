using System;
using ModelGenerator.Framework;
using ModelGenerator.Framework.FileParsing;
using NUnit.Framework;
using Shouldly;

namespace ModelGenerator.Tests.Framework
{
    [TestFixture]
    public class FieldExtensionsTests
    {
        [Test]
        public void GetMultiReferenceValue_GivenBlankValue_ReturnsEmptyCollection()
        {
            var field = new Field(Guid.Empty, string.Empty, string.Empty);

            var result = field.GetMultiReferenceValue();

            result.ShouldBeEmpty();
        }

        [Test]
        public void GetMultiReferenceValue_GivenInvalidValue_ThrowsException()
        {
            var field = new Field(Guid.Empty, string.Empty, "not-a-guid|also-not-a-guid");

            Should.Throw<Exception>(() => field.GetMultiReferenceValue());
        }

        [Test]
        public void GetMultiReferenceValue_GivenMultipleItems_ReturnsMultipleItems()
        {
            var ids = new[] {Guid.NewGuid(), Guid.NewGuid()};
            var field = new Field(Guid.Empty, string.Empty, $"{ids[0].ToSitecoreId()}|{ids[1].ToSitecoreId()}");

            var result = field.GetMultiReferenceValue();

            result.ShouldBeEquivalentTo(ids, "Output IDs should be identical to input IDs in value and order.");
        }

        [Test]
        public void GetMultiReferenceValue_GivenNull_ReturnsEmptyCollection()
        {
            var result = FieldExtensions.GetMultiReferenceValue(null);

            result.ShouldBeEmpty();
        }

        [Test]
        public void GetMultiReferenceValue_GivenSingleItem_ReturnsSingleItem()
        {
            var id = Guid.NewGuid();
            var field = new Field(Guid.Empty, string.Empty, id.ToSitecoreId());

            var result = field.GetMultiReferenceValue();

            result.ShouldHaveSingleItem()
                  .ShouldBeEquivalentTo(id);
        }
    }
}