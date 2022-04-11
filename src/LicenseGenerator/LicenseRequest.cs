using Microsoft.Extensions.Configuration;
using static System.Console;
using static LicenseGenerator.ConsoleHelpers;

namespace LicenseGenerator;

public record LicenseRequest(string Licensee, string Entitlement, TimeSpan Lifetime, string[] Products, Key Key)
{
    public static LicenseRequest Create(IConfiguration configuration)
    {
        var licensee = configuration[ConfigManager.KeyNames.Licensee];
        if (string.IsNullOrWhiteSpace(licensee))
        {
            licensee = Prompt("Licensee name");
        }

        var entitlement = configuration[ConfigManager.KeyNames.Entitlement];
        if (string.IsNullOrWhiteSpace(entitlement))
        {
            entitlement = Prompt("Entitlement description");
        }

        if (!TimeSpan.TryParse(configuration[ConfigManager.KeyNames.Lifetime], out var lifetime))
        {
            lifetime = Prompt("License lifetime (timespan)", TimeSpan.Parse);
        }

        var availableProducts = ConfigManager.GetAvailableProducts(configuration);
        var products = configuration[ConfigManager.KeyNames.Products]?.Split(",");
        if (products != null && products.All(p => availableProducts.Contains(p, StringComparer.OrdinalIgnoreCase)))
        {
            // While the license tool should be case insensitive, the tokens may not be.  Ensure we select
            // the appropriately cased values from the constants.
            products = availableProducts
                .Intersect(products, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        else
        {
            products = SelectProducts(availableProducts).ToArray();
        }

        var keys = ConfigManager.GetKeys(configuration);
        var key = keys.SingleOrDefault(k => KeySelector(configuration[ConfigManager.KeyNames.Key], k));
        if (key == null)
        {
            var options = keys.Select((k, i) => new Option<Key>((i + 1).ToString(), k.Name, k)).ToArray();
            key = Choose("Choose a signing key", options).Value;
        }

        return new LicenseRequest(licensee, entitlement, lifetime, products, key);
    }

    public void Print()
    {
        WriteLine("Generating license for:");
        WriteLine($"  Licensee:    {Licensee}");
        WriteLine($"  Entitlement: {Entitlement}");
        WriteLine($"  Lifetime:    {Lifetime}");
        WriteLine($"  Key:         {Key.Name}");
        WriteLine("  Products:");
        foreach (var product in Products)
        {
            WriteLine($"    {product}");
        }
    }

    private static bool KeySelector(string? value, Key k)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }
        
        return string.Equals(k.KeyId, value, StringComparison.OrdinalIgnoreCase)
               || string.Equals(k.Name, value, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SelectProducts(string[] availableProducts)
    {
        do
        {
            var choice = Choose("Licensed product(s)", availableProducts);
            
            // Don't allow selection of the same product twice, and don't prompt for more
            // if there aren't any more to select.
            availableProducts = availableProducts.Where(x => x != choice).ToArray();
            yield return choice;
        } while (availableProducts.Any() && Confirm("Add another product?"));
    }
}