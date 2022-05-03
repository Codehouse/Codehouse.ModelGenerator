using System.IO;
using LicenseGenerator;
using Microsoft.Extensions.Configuration;
using static System.Console;
using static LicenseGenerator.ConsoleHelpers;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .AddCommandLine(args)
    .Build();

var helpCommands = new[] {"help", "-help", "--help", "-?", "--?", "/?"};
if (args.Length == 1 && helpCommands.Contains(args[0], StringComparer.OrdinalIgnoreCase))
{
    WriteLine("Usage instructions:");
    WriteLine($"  LicenseGenerator.exe [--{ConfigManager.KeyNames.Licensee} string]");
    WriteLine($"                       [--{ConfigManager.KeyNames.Entitlement} string]");
    WriteLine($"                       [--{ConfigManager.KeyNames.Lifetime} duration/timespan]");
    WriteLine($"                       [--{ConfigManager.KeyNames.Products} comma-separated-string]");
    WriteLine($"                       [--{ConfigManager.KeyNames.Key} id-or-name]");
    WriteLine($"                       [--{ConfigManager.KeyNames.Output} path]");
    
    WriteLine();
    WriteLine("Valid product names:");
    foreach (var product in ConfigManager.GetAvailableProducts(configuration))
    {
        WriteLine("\t" + product);
    }
    
    WriteLine();
    WriteLine("Valid keys:");
    var keys = ConfigManager.GetKeys(configuration);
    foreach (var key in keys)
    {
        WriteLine($"\t{key.KeyId} - {key.Name}");
    }
    
    return 0;
}

try
{
    var licenseRequest = LicenseRequest.Create(configuration);
    licenseRequest.Print();

    var license = LicenseFactory.CreateLicense(licenseRequest);
    WriteLine("Generated license:");
    WrapAndIndent(license);
    
    WriteLine();
    WriteLine("Verifying...");
    if (!LicenseFactory.ValidateLicense(licenseRequest, license))
    {
        WriteLine("Token was invalid for unspecified reason.");
        return 1;
    }

    WriteLine();
    WriteLine("Outputting...");
    var output = Path.GetFullPath(configuration[ConfigManager.KeyNames.Output]);
    if (File.Exists(output))
    {
        var attributes = File.GetAttributes(output);
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            output = Path.Combine(output, "license.dat");
        }
    }

    WriteLine($"Writing token to {output}");
    try
    {
        File.WriteAllText(output, license);
        WriteLine("Done.");
    }
    catch (Exception ex)
    {
        WriteLine("Could not write file.");
        WriteLine(ex);
        return 1;
    }

    return 0;
}
catch (Exception e)
{
    WriteLine(e);
    return 1;
}