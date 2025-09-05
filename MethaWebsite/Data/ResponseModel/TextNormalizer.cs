namespace MethaWebsite.Data.ResponseModel
{
    public static class TextNormalizer
    {
        public static string Normalize(string text, string locale)
        {
            // Basic cleanup
            text = text.Trim().ToLowerInvariant();

            // Locale-specific tweaks
            return locale switch
            {
                "sw-KE" => text.Replace("toka ", "from "),
                "en-KE" => text, // you might add regional idioms here
                _ => text
            };
        }
    }
}
