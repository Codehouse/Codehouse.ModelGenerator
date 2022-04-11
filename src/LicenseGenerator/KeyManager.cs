using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace LicenseGenerator;

public static class KeyManager
{
    public static ECDsaSecurityKey GetPrivateKey(Key key)
    {
        var ecDsaAlgorithm = CreateAlgorithm();

        var privateKeyBytes = Convert.FromBase64String(key.PrivateKey);
        ecDsaAlgorithm.ImportECPrivateKey(privateKeyBytes, out _);
        return new ECDsaSecurityKey(ecDsaAlgorithm){KeyId = key.KeyId};
    }

    public static ECDsaSecurityKey GetPublicKey(Key key)
    {
        var ecDsaAlgorithm = CreateAlgorithm();

        var publicKeyBytes = Convert.FromBase64String(key.PublicKey);
        ecDsaAlgorithm.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
        return new ECDsaSecurityKey(ecDsaAlgorithm){KeyId = key.KeyId};
    }

    private static ECDsa CreateAlgorithm()
    {
        var ecDsaAlgorithm = ECDsa.Create();
        if (ecDsaAlgorithm == null)
        {
            throw new CryptographicException("Algorithm not created");
        }

        return ecDsaAlgorithm;
    }
}