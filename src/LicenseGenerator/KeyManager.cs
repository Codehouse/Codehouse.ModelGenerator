using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace LicenseGenerator;

public static class KeyManager
{
    public static ECDsaSecurityKey GetPrivateKey()
    {
        var ecDsaAlgorithm = CreateAlgorithm();

        var privateKeyBytes = Convert.FromBase64String(Constants.Signing.PrivateKey);
        ecDsaAlgorithm.ImportECPrivateKey(privateKeyBytes, out _);
        return new ECDsaSecurityKey(ecDsaAlgorithm){KeyId = "ff2bc7022c474b29a86d477ceac4cc5f"};
    }

    public static ECDsaSecurityKey GetPublicKey()
    {
        var ecDsaAlgorithm = CreateAlgorithm();

        var publicKeyBytes = Convert.FromBase64String(Constants.Signing.PublicKey);
        ecDsaAlgorithm.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
        return new ECDsaSecurityKey(ecDsaAlgorithm){KeyId = "ff2bc7022c474b29a86d477ceac4cc5f"};
    }

    private static ECDsa? CreateAlgorithm()
    {
        var ecDsaAlgorithm = ECDsa.Create();
        if (ecDsaAlgorithm == null)
        {
            throw new CryptographicException("Algorithm not created");
        }

        return ecDsaAlgorithm;
    }
}