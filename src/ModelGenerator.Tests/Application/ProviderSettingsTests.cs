using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;

namespace ModelGenerator.Tests.Application
{
    [TestFixture]
    public class ProviderSettingsTests
    {
        private readonly string _validOutputProvider = OutputProviderNames.Ids.ToString();
        private const string InvalidProvider = "Foo";
        private const string ValidInputProvider = "Tds";

        [Test]
        public void GivenInvalidInputProvider_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => CreateSettings(InvalidProvider, _validOutputProvider));
        }

        [Test]
        public void GivenSingleInvalidOutputProvider_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => CreateSettings(ValidInputProvider, InvalidProvider));
        }

        [Test]
        public void GivenMultipleInvalidOutputProvider_GivesNoEntries()
        {
            var settings = CreateSettings(ValidInputProvider, new[] {InvalidProvider, InvalidProvider});
            settings.Output.ShouldBeEmpty();
        }

        [Test]
        public void GivenMissingInputProvider_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => CreateSettings(null, _validOutputProvider));
        }

        [Test]
        public void GivenSingleOutputProvider_ReturnsSingleCorrectProvider()
        {
            var settings = CreateSettings(ValidInputProvider, _validOutputProvider);

            settings.Output.ShouldHaveSingleItem().ShouldBe(OutputProviderNames.Ids);
        }

        [Test]
        public void GivenMultipleOutputProvider_ReturnsSingleCorrectProvider()
        {
            var settings = CreateSettings(ValidInputProvider, new[] {_validOutputProvider, _validOutputProvider});

            settings.Output.Count.ShouldBe(2);
            settings.Output.ShouldContain(OutputProviderNames.Ids);
        }

        [Test]
        public void GivenSingleInputProvider_ReturnsCorrectProvider()
        {
            var settings = CreateSettings(ValidInputProvider, _validOutputProvider);

            settings.Input.ShouldBe(InputProviderNames.Tds);
        }

        [Test]
        public void GivenSingleMissingOutputProvider_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => CreateSettings(ValidInputProvider, null));
        }

        [Test]
        public void GivenMultipleMissingOutputProvider_GivesNoEntries()
        {
            var settings = CreateSettings(ValidInputProvider, new string?[] {null, null});
            settings.Output.ShouldBeEmpty();
        }

        private ProviderSettings CreateSettings(string? input, object? output)
        {
            var configString = JsonConvert.SerializeObject(new {Providers = new {Input = input, Output = output}});
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(configString));
            
            var config = new ConfigurationBuilder()
                        .AddJsonStream(memoryStream)
                        .Build();

            return new ProviderSettings(config);
        }
    }
}