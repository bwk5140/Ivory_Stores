namespace MethaWebsite.Data.ResponseModel
{
    public sealed class SlotResolver : ISlotResolver
    {
        private readonly ICityCanonicalizer _Resolver;
        private readonly ILogger<SlotResolver> _logger;

        public SlotResolver(ICityCanonicalizer Resolver, ILogger<SlotResolver> logger)
        {
            _Resolver = Resolver;
            _logger = logger;
        }

        public IReadOnlyDictionary<string, SlotValue> Resolve(ResponseRequest request,AnchorDefinition anchor, IReadOnlyDictionary<string, SlotValue> extracted)
        {
            var resolved = new Dictionary<string, SlotValue>(StringComparer.OrdinalIgnoreCase);

            foreach (var slotDef in anchor.Slots)
            {
                // Try to find matching extracted slot (case-insensitive)
                var match = extracted.FirstOrDefault(kvp =>
                    string.Equals(kvp.Key, slotDef.Name, StringComparison.OrdinalIgnoreCase));

                if (match.Key == null || string.IsNullOrWhiteSpace(match.Value.Value))
                {
                    // Log missing slot
                    _logger.LogWarning($"Missing or empty slot: {slotDef.Name}");
                    continue;
                }

                var sv = match.Value;

                // Handle City slot
                if (slotDef.Type?.Equals("City", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var res = _Resolver.TryResolve(sv.Value!, request.Locale);
                    if (res is { } ok)
                    {
                        resolved[slotDef.Name] = sv with
                        {
                            Value = ok.CanonicalCity,
                            Confidence = Math.Max(0.8, sv.Confidence),
                            IsResolved = true
                        };

                        // Inject derived timeZoneId if not already present
                        if (!resolved.ContainsKey("timeZoneId") && ok.WindowsTimeZoneId is { } tz)
                        {
                            resolved["timeZoneId"] = new SlotValue
                            {
                                Name = "timeZoneId",
                                Value = tz,
                                Confidence = ok.Confidence,
                                IsResolved = true
                            };
                        }
                        continue;
                    }

                    _logger.LogWarning($"City resolution failed for value: {sv.Value}");
                }

                // Default passthrough for other slot types
                resolved[slotDef.Name] = sv;
            }
            return resolved;
        }

    }

}
