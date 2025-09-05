namespace MethaWebsite.Data.ResponseModel
{
    public interface ICityCanonicalizer
    {
        // e.g., "Nairobi" -> ("Nairobi", "E. Africa Standard Time")
        (string CanonicalCity, string? WindowsTimeZoneId, double Confidence)? TryResolve(string input, string? locale);
    }

}
