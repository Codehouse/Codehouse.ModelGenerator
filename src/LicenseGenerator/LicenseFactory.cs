using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace LicenseGenerator;

public static class LicenseFactory
{
    private static class ClaimNames
    {
        public const string Audience = "aud";
        public const string Entitlement = "entitlement";
        public const string Subject = "sub";
    }
    
    public static string CreateLicense(LicenseRequest request)
    {
        var notBefore = DateTime.UtcNow.Date;
        var expiry = notBefore.Add(request.Lifetime);

        var securityKey = KeyManager.GetPrivateKey(request.Key);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new List<Claim>
            {
                new (ClaimNames.Subject,request.Licensee),
                new (ClaimNames.Entitlement, request.Entitlement)
            }),
            Expires = expiry,
            IssuedAt = DateTime.UtcNow,
            NotBefore = notBefore,
            Issuer = Constants.Issuer,
            SigningCredentials = new SigningCredentials(securityKey, request.Key.Algorithm)
        };
        
        if (request.Products.Length == 1)
        {
            tokenDescriptor.Audience = request.Products[0];
        }
        else
        {
            tokenDescriptor.Subject.AddClaims(request.Products.Select(a => new Claim(ClaimNames.Audience, a)));
        }
        
        var createdToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(createdToken);
    }

    public static bool ValidateLicense(LicenseRequest request, string license)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = Constants.Issuer,
            ValidAudiences = request.Products,
            IssuerSigningKey = KeyManager.GetPublicKey(request.Key)
        };
        
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var result = tokenHandler.ValidateToken(license, validationParameters, out SecurityToken token);
            Console.WriteLine($"Token is valid until {token.ValidTo:s}.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Token is invalid");
            Console.WriteLine(ex);
            return false;
        }
    }
}