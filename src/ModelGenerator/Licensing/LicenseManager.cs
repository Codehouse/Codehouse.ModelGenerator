using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using ModelGenerator.Framework.Configuration;

namespace ModelGenerator.Licensing
{
    public sealed class LicenseManager
    {
        private readonly ILogger<LicenseManager> _logger;
        private readonly IOptions<Settings> _settings;
        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, string> _publicKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"ff2bc7022c474b29a86d477ceac4cc5f", "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEDsrvNTTGXLomDLPZgcfTJulPjW+Vh3q4aJ/HvmdhAFvYR7WzDdELrnB8Q1Nfq1HB1FqG7Stp+PPf05oS29rEDA=="}
        };
        private readonly IEnumerable<string> _validAlgorithms = new[]
        {
            SecurityAlgorithms.EcdsaSha256,
            SecurityAlgorithms.EcdsaSha384,
            SecurityAlgorithms.EcdsaSha512,
        };

        public LicenseManager(ILogger<LicenseManager> logger, IOptions<Settings> settings, IConfiguration configuration)
        {
            _logger = logger;
            _settings = settings;
            _configuration = configuration;
        }

        public LicenseCheckResult CheckLicense()
        {
            _logger.LogInformation("Checking license...");
            var key = GetCurrentKey();
            if (string.IsNullOrWhiteSpace(key))
            {
                return LicenseCheckResult.Missing();
            }

            _logger.LogDebug("Effective license key: " + key);
            return ValidateKey(key);
        }

        private LicenseCheckResult ValidateKey(string key)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = "Codehouse",
                ValidAudience = "Codehouse.ModelGenerator",
                ValidAlgorithms = _validAlgorithms,
                IssuerSigningKeyResolver = ResolveKey
            };

            try
            {
                var principal = tokenHandler.ValidateToken(key, validationParams, out SecurityToken token);
                var name = principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                           ?? throw new Exception("License contained no licensee.");
                var entitlement = principal.Claims.SingleOrDefault(c => c.Type == "entitlement")?.Value
                                  ?? throw new Exception("License contained no entitlement.");

                _logger.LogInformation($"The license was valid (licensee: {name}, entitlement: {entitlement}).");
                return new LicenseCheckResult(name, entitlement, token.ValidTo, LicenseStatuses.Valid);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogError(ex, "The license has expired.");
                return LicenseCheckResult.Expired(ex.Expires);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The license is invalid.");
                return LicenseCheckResult.Invalid();
            }
        }

        /// <summary>
        /// Gets the key directly from the configuration (allowing it to be passed in), or from
        /// the file indicated by the configuration.
        /// </summary>
        /// <returns>The license key if found, or null.</returns>
        private string? GetCurrentKey()
        {
            var configurationKey = _configuration["LicenseKey"];
            if (!string.IsNullOrWhiteSpace(configurationKey))
            {
                _logger.LogInformation("Retrieved license key from configuration, command line, or environment.");
                return configurationKey;
            }

            if (string.IsNullOrWhiteSpace(_settings.Value.License))
            {
                _logger.LogError("License location was not specified in configuration (Settings:License).");
                return null;
            }

            if (!File.Exists(_settings.Value.License))
            {
                _logger.LogError($"The license file specified by the configuration ({_settings.Value.License}) does not exist.");
                return null;
            }

            try
            {
                _logger.LogInformation($"Reading text from license file at {_settings.Value.License}.");
                return File.ReadAllText(_settings.Value.License);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not read configured license file.");
                return null;
            }
        }

        private SecurityKey GetPublicKey(string publicKeyValue)
        {
            var ecDsaAlgorithm = ECDsa.Create();
            var publicKeyBytes = Convert.FromBase64String(publicKeyValue);
            ecDsaAlgorithm.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return new ECDsaSecurityKey(ecDsaAlgorithm);
        }

        private IEnumerable<SecurityKey> ResolveKey(string token, SecurityToken securitytoken, string kid, TokenValidationParameters validationparameters)
        {
            if (_publicKeys.ContainsKey(kid))
            {
                yield return GetPublicKey(_publicKeys[kid]);
            }
        }
    }
}