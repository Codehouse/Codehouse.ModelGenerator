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
        private const string ValidOutputProvider = "Ids";
        private const string ValidInputProvider = "Tds";

        [Test]
        public void GivenSingleOutputProvider_ReturnsSingleCorrectProvider()
        {
            var settings = CreateSettings(null, ValidOutputProvider);

            settings.Output.ShouldHaveSingleItem().ShouldBe(ValidOutputProvider);
        }

        [Test]
        public void GivenMultipleOutputProvider_ReturnsProviders()
        {
            var settings = CreateSettings(null, new[] {ValidOutputProvider, ValidOutputProvider});

            settings.Output.Count.ShouldBe(2);
            settings.Output.ShouldContain(ValidOutputProvider);
        }

        [Test]
        public void GivenSingleInputProvider_ReturnsCorrectProvider()
        {
            var settings = CreateSettings(ValidInputProvider, null);

            settings.Input.ShouldBe(InputProviderNames.Tds.ToString());
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