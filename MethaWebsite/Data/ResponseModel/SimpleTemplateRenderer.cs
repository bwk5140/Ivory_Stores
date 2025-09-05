namespace MethaWebsite.Data.ResponseModel
{
    public sealed class SimpleTemplateRenderer : ITemplateRenderer
    {
        public string Render(string templateText, IReadOnlyDictionary<string, SlotValue> slots)
        {
            var result = templateText;
            foreach (var (k, v) in slots)
            {
                // Replace {slot} with value
                result = result.Replace("{" + k + "}", v.Value ?? string.Empty, StringComparison.Ordinal);
            }
            // Remove any unreplaced tokens gracefully
            result = System.Text.RegularExpressions.Regex.Replace(result, "{[^}]+}", string.Empty);
            return result.Trim();
        }
    }
}
