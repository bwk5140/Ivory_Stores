using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace MethaWebsite.Data.ResponseModel
{
    public sealed class CityCanonicalizer : ICityCanonicalizer
    {
        private readonly CityCanonicalizerOptions _opts;
        private readonly Dictionary<string, CityRecord> _canonicalIndex;
        private readonly Dictionary<string, List<AliasEntry>> _aliasIndex;
        private readonly ConcurrentDictionary<(string, string?), (string, string?, double)?> _cache;

        public CityCanonicalizer(IOptions<CityCanonicalizerOptions> options)
        {
            _opts = options.Value ?? throw new ArgumentNullException(nameof(options));
            _cache = new();
            _canonicalIndex = new(StringComparer.OrdinalIgnoreCase);
            _aliasIndex = new(StringComparer.OrdinalIgnoreCase);

            foreach (var c in _opts.Cities)
            {
                if (string.IsNullOrWhiteSpace(c.Canonical)) continue;
                _canonicalIndex[c.Canonical] = c;

                // Canonical is an implicit alias (helps exact matches)
                IndexAlias(c, c.Canonical, locale: null, isCanonical: true);

                // Global aliases
                if (c.Aliases != null)
                {
                    foreach (var a in c.Aliases)
                        IndexAlias(c, a, locale: null);
                }

                // Locale-specific aliases
                if (c.LocaleAliases != null)
                {
                    foreach (var kvp in c.LocaleAliases)
                    {
                        foreach (var a in kvp.Value)
                            IndexAlias(c, a, kvp.Key);
                    }
                }
            }

            // Optional user overrides (alias -> canonical)
            if (_opts.Overrides != null)
            {
                foreach (var kv in _opts.Overrides)
                {
                    if (_canonicalIndex.TryGetValue(kv.Value, out var city))
                        IndexAlias(city, kv.Key, locale: null, isOverride: true);
                }
            }
        }

        public (string CanonicalCity, string? WindowsTimeZoneId, double Confidence)? TryResolve(string input, string? locale)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var key = (Normalize(input), NormalizeLocale(locale));
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var (normalized, original) = (key.Item1, input.Trim());

            // 1) Exact alias/canonical match
            var exact = FindExact(normalized, key.Item2);
            if (exact is not null)
                return _cache[key] = exact;

            // 2) Fuzzy search against alias keys (guarded, to reduce cost)
            var fuzzy = _opts.EnableFuzzy
                ? FindFuzzy(normalized, key.Item2)
                : null;

            // 3) If fuzzy produced a candidate and it meets the threshold, accept
            if (fuzzy is not null && fuzzy.Value.Confidence >= _opts.MinConfidence)
                return _cache[key] = fuzzy;

            // 4) Nothing acceptable
            return _cache[key] = null;
        }

        private (string CanonicalCity, string? WindowsTimeZoneId, double Confidence)? FindExact(string normalizedInput, string? locale)
        {
            if (_aliasIndex.TryGetValue(normalizedInput, out var matches) && matches.Count > 0)
            {
                // Rank by: isCanonical > override > locale boost > priority
                var best = matches
                    .Select(m => new
                    {
                        City = m.City,
                        Confidence = ScoreExact(m, locale)
                    })
                    .OrderByDescending(x => x.Confidence)
                    .ThenByDescending(x => x.City.Priority)
                    .First();

                if (best.Confidence >= _opts.MinConfidence)
                    return (best.City.Canonical, best.City.WindowsTimeZoneId, Clamp(best.Confidence));
            }
            return null;
        }

        private (string CanonicalCity, string? WindowsTimeZoneId, double Confidence)? FindFuzzy(string normalizedInput, string? locale)
        {
            if (normalizedInput.Length < _opts.MinFuzzyLength)
                return null;

            double bestScore = double.MinValue;
            CityRecord? bestCity = null;

            // Compare against unique alias keys to keep it bounded
            foreach (var kv in _aliasIndex)
            {
                var aliasKey = kv.Key;
                var sim = Similarity(normalizedInput, aliasKey);
                if (sim < _opts.MinFuzzySimilarity) continue;

                // pick the best alias entry for this alias key considering locale/priority
                var entry = BestAliasForLocale(kv.Value, locale);
                var score = ScoreFuzzy(sim, entry, locale);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCity = entry.City;
                }
            }

            if (bestCity is null) return null;
            var finalScore = Clamp(bestScore);
            return finalScore >= _opts.MinConfidence
                ? (bestCity.Canonical, bestCity.WindowsTimeZoneId, finalScore)
                : null;
        }

        private AliasEntry BestAliasForLocale(List<AliasEntry> entries, string? locale)
        {
            // Prefer exact locale match, then canonical/override flags, then priority
            return entries
                .OrderByDescending(e => LocaleMatchScore(e.Locale, locale))
                .ThenByDescending(e => e.IsCanonical)
                .ThenByDescending(e => e.IsOverride)
                .ThenByDescending(e => e.City.Priority)
                .First();
        }

        private double ScoreExact(AliasEntry match, string? locale)
        {
            double baseScore =
                match.IsCanonical ? _opts.Scores.ExactCanonical
              : match.IsOverride ? _opts.Scores.ExactOverride
              : match.Locale is not null ? _opts.Scores.ExactLocaleAlias
              : _opts.Scores.ExactAlias;

            baseScore += LocaleBoost(match.Locale, locale);
            baseScore += PriorityBoost(match.City.Priority);
            return Clamp(baseScore);
        }

        private double ScoreFuzzy(double similarity, AliasEntry entry, string? locale)
        {
            // Map similarity [0..1] into a confidence band
            var conf = _opts.Scores.FuzzyBase + similarity * _opts.Scores.FuzzySpan;
            conf += LocaleBoost(entry.Locale, locale);
            conf += PriorityBoost(entry.City.Priority);
            return Clamp(conf);
        }

        private static double PriorityBoost(int priority) => Math.Min(0.04, 0.01 * Math.Max(0, priority));

        private static double LocaleBoost(string? aliasLocale, string? requestLocale)
            => LocaleMatchScore(aliasLocale, requestLocale) * 0.03;

        private static double LocaleMatchScore(string? aliasLocale, string? requestLocale)
        {
            if (string.IsNullOrWhiteSpace(aliasLocale) || string.IsNullOrWhiteSpace(requestLocale)) return 0;
            var a = NormalizeLocale(aliasLocale);
            var r = NormalizeLocale(requestLocale);
            if (a == r) return 1;
            // Match by region if available (e.g., en-KE vs sw-KE -> region KE matches)
            var aRegion = RegionPart(a);
            var rRegion = RegionPart(r);
            return (!string.IsNullOrEmpty(aRegion) && aRegion == rRegion) ? 0.6 : 0;
        }

        private static string? RegionPart(string? locale)
        {
            if (string.IsNullOrWhiteSpace(locale)) return null;
            var parts = locale.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length >= 2 ? parts[1].ToUpperInvariant() : null;
        }

        private void IndexAlias(CityRecord city, string alias, string? locale, bool isCanonical = false, bool isOverride = false)
        {
            if (string.IsNullOrWhiteSpace(alias)) return;
            var key = Normalize(alias);
            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(_aliasIndex, key, out var exists);
            if (!exists || list is null) list = new List<AliasEntry>(1);
            list.Add(new AliasEntry(city, locale is null ? null : NormalizeLocale(locale), isCanonical, isOverride));
        }

        private static string Normalize(string text)
        {
            text = text.Trim().ToLowerInvariant();
            text = text.Normalize(NormalizationForm.FormD);
            Span<char> buf = stackalloc char[text.Length];
            int j = 0;
            foreach (var ch in text)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat == UnicodeCategory.NonSpacingMark) continue; // strip diacritics
                if (char.IsPunctuation(ch)) continue;
                if (char.IsWhiteSpace(ch)) { if (j > 0 && buf[j - 1] != ' ') buf[j++] = ' '; continue; }
                buf[j++] = ch;
            }
            return new string(buf[..j]).Trim();
        }

        private static string? NormalizeLocale(string? locale)
            => string.IsNullOrWhiteSpace(locale) ? null : locale.Trim().Replace('_', '-').ToLowerInvariant();

        private static double Similarity(string a, string b)
        {
            // Levenshtein-based similarity in [0..1]
            int dist = Levenshtein(a, b);
            int maxLen = Math.Max(a.Length, b.Length);
            return maxLen == 0 ? 1 : 1.0 - (double)dist / maxLen;
        }

        private static int Levenshtein(string s, string t)
        {
            int n = s.Length, m = t.Length;
            if (n == 0) return m;
            if (m == 0) return n;

            var dp = new int[n + 1, m + 1];
            for (int i = 0; i <= n; i++) dp[i, 0] = i;
            for (int j = 0; j <= m; j++) dp[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }
            return dp[n, m];
        }

        private static double Clamp(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        private readonly record struct AliasEntry(CityRecord City, string? Locale, bool IsCanonical, bool IsOverride);
    }

    public sealed class CityCanonicalizerOptions
    {
        public List<CityRecord> Cities { get; init; } = new();
        public Dictionary<string, string>? Overrides { get; init; } // alias -> canonical
        public bool EnableFuzzy { get; init; } = true;
        public int MinFuzzyLength { get; init; } = 3;
        public double MinFuzzySimilarity { get; init; } = 0.72;
        public double MinConfidence { get; init; } = 0.6;
        public ScoreOptions Scores { get; init; } = new();

        public sealed class ScoreOptions
        {
            public double ExactCanonical { get; init; } = 0.99;
            public double ExactOverride { get; init; } = 0.98;
            public double ExactLocaleAlias { get; init; } = 0.97;
            public double ExactAlias { get; init; } = 0.95;
            public double FuzzyBase { get; init; } = 0.55;   // baseline for fuzzy
            public double FuzzySpan { get; init; } = 0.35;   // fuzzy score = base + similarity * span
        }
    }

    public sealed class CityRecord
    {
        public string Canonical { get; init; } = "";
        public string? WindowsTimeZoneId { get; init; } // e.g., "E. Africa Standard Time"
        public string CountryIso2 { get; init; } = "";  // e.g., "KE"
        public int Priority { get; init; } = 0;         // higher wins on ties
        public string[]? Aliases { get; init; }
        public Dictionary<string, string[]>? LocaleAliases { get; init; } // "en-KE": ["nai", ...]
    }

}