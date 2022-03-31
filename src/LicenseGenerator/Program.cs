using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LicenseGenerator;
using Microsoft.IdentityModel.Tokens;
using static LicenseGenerator.ConsoleHelpers;

var licensee = Prompt("Licensee name");
var entitlement = Prompt("Entitlement description");
var lifetime = Prompt("License lifetime (timespan)", TimeSpan.Parse);
var audience = Choose("Software tool (audience)", Constants.Audiences);

var notBefore = DateTime.UtcNow;
var expiry = notBefore.Add(lifetime);
var issuer = "Codehouse";

//Generating security key
var securityKey = KeyManager.GetPrivateKey();
var tokenHandler = new JwtSecurityTokenHandler();
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new List<Claim>
    {
        new ("sub",licensee),
        new ("entitlement", entitlement)
    }),
    Audience = audience,
    Expires = expiry,
    IssuedAt = DateTime.UtcNow,
    NotBefore = notBefore,
    Issuer = issuer,
    SigningCredentials = new SigningCredentials(securityKey, Constants.Signing.Algorithm)
};

var createdToken = tokenHandler.CreateToken(tokenDescriptor);
var tokenString = tokenHandler.WriteToken(createdToken);
Console.WriteLine(tokenString);
Console.WriteLine();

// Validating security key
Console.WriteLine("Verifying...");
var validationParameters = new TokenValidationParameters
{
    ValidIssuer = Constants.Issuer,
    ValidAudience = audience,
    IssuerSigningKey = KeyManager.GetPublicKey()
};
try
{
    var result = tokenHandler.ValidateToken(tokenString, validationParameters, out SecurityToken _);
    Console.WriteLine("Token is valid.");
}
catch (Exception ex)
{
    Console.WriteLine("Token is invalid");
    Console.WriteLine(ex);
}

Console.WriteLine("Writing token to license.dat");
File.WriteAllText("license.dat", tokenString);
