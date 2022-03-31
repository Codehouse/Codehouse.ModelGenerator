using System;

namespace ModelGenerator.Licensing
{
    public record LicenseCheckResult(
        string? Licensee,
        string? Entitlement,
        DateTime? Expires,
        LicenseStatuses Status)
    {
        public static LicenseCheckResult Expired(DateTime expiry)
        {
            return new (null, null, expiry, LicenseStatuses.Expired); 
        }
        
        public static LicenseCheckResult Invalid()
        {
            return new(null, null, null, LicenseStatuses.Invalid);
        }

        public static LicenseCheckResult Missing()
        {
            return new(null, null, null, LicenseStatuses.Missing);
        }
    }
}