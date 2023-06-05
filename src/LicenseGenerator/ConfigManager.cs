using Microsoft.Extensions.Configuration;

namespace LicenseGenerator;

public static class ConfigManager
{
    internal static class KeyNames
    {
        public const string AvailableProducts = nameof(AvailableProducts);
        public const string Entitlement = nameof(Entitlement);
        public const string Key = nameof(Key);
        public const string Keys = nameof(Keys);
        public const string Licensee = nameof(Licensee);
        public const string Lifetime = nameof(Lifetime);
        public const string Output = nameof(Output);
        public const string Products = nameof(Products);
    }
    
    public static string[] GetAvailableProducts(IConfiguration configuration)
    {
        return configuration.GetSection(KeyNames.AvailableProducts).Get<string[]>();
    }
    
    public static Key[] GetKeys(IConfiguration configuration)
    {
        return configuration.GetSection(KeyNames.Keys).Get<Key[]>();
    } 
}