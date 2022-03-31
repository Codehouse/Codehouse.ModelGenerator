using Microsoft.IdentityModel.Tokens;

namespace LicenseGenerator;

public static class Constants
{
    public static string[] Audiences { get; } = {
        "Codehouse.ModelGenerator"
    };
    public static string Issuer => "Codehouse";

    public static class Signing
    {
        public static string Algorithm => SecurityAlgorithms.EcdsaSha256;
        public static string KeyId => "ff2bc7022c474b29a86d477ceac4cc5f";
        public static string PrivateKey => "MHcCAQEEIIrOKzZUpZF6IV04avkTngAPG2QmO/I1dRQtnuLpcKXdoAoGCCqGSM49AwEHoUQDQgAEDsrvNTTGXLomDLPZgcfTJulPjW+Vh3q4aJ/HvmdhAFvYR7WzDdELrnB8Q1Nfq1HB1FqG7Stp+PPf05oS29rEDA==";
        public static string PublicKey => "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEDsrvNTTGXLomDLPZgcfTJulPjW+Vh3q4aJ/HvmdhAFvYR7WzDdELrnB8Q1Nfq1HB1FqG7Stp+PPf05oS29rEDA==";
    }
}