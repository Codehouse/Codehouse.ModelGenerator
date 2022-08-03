using NUnit.Framework;
using Shouldly;

namespace ModelGenerator.Tests.Application
{
    [TestFixture]
    public class ProgramTests
    {
        private readonly string _invalidProviderName = "Foo";
        private readonly string _validProviderName = InputProviderNames.Scs.ToString();
        private readonly InputProviderNames _validProvider = InputProviderNames.Scs;
        
        [Test]
        public void ParseProviderName_GivenInvalidItem_ThrowsException()
        {
            Assert.Throws<ProviderNameException>(() => Program.ParseProviderName<InputProviderNames>(_invalidProviderName))
                  .ShouldNotBeNull()
                  .OriginalName.ShouldBe(_invalidProviderName);
        }
        
        [Test]
        public void ParseProviderName_GivenNullItem_ThrowsException()
        {
            Assert.Throws<ProviderNameException>(() => Program.ParseProviderName<InputProviderNames>(null))
                  .ShouldNotBeNull()
                  .OriginalName.ShouldBeNull();
        }
        
        [Test]
        public void ParseProviderName_GivenValidItem_ReturnsItem()
        {
            var result = Program.ParseProviderName<InputProviderNames>(_validProviderName);
            result.ShouldBe(_validProvider);
        }
    }
}