using Microsoft.VisualBasic.FileIO;
using System;

namespace MethaWebsite.Services
{
    public class WorldClockService
    {
        private readonly ILogger<WorldClockService> logger;
        private readonly Dictionary<string, string> WindowsTimeZoneIds;
        private readonly HashSet<string> cities;
        public WorldClockService(ILogger<WorldClockService> _logger)
        {
            logger = _logger;
            WindowsTimeZoneIds = LoadCityWindowsTimeZones("Models/CityTimeZones.csv");
            cities = GetCities();
        }
        public HashSet<string> GetCities()
        {
            HashSet<string> cities = new HashSet<string>();
            if (!cities.Any())
            {
                foreach (var kvp in WindowsTimeZoneIds)
                {
                    cities!.Add(kvp.Key);
                }
            }
            return cities ?? new HashSet<string>();
        }
        public static Dictionary<string, string> LoadCityWindowsTimeZones(string path)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            if (parser.EndOfData)
                throw new InvalidDataException("CSV is empty.");

            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields == null) continue;
                if (fields.Length <= Math.Max(0, 2)) continue;

                var city = (fields[0] ?? "").Trim();
                var windowsId = (fields[2] ?? "").Trim();

                if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(windowsId))
                    continue;

                map[city] = windowsId; // last wins
            }

            return map;
        }
        public (string, string) GetFullTimeZoneForCity(string input)
        {
            string city = "";
            foreach (var pair in WindowsTimeZoneIds)
            {
                if (input.ToLowerInvariant().Contains(pair.Key.ToLowerInvariant()))
                {
                    city = pair.Key;
                }
            }
            if (string.IsNullOrWhiteSpace(city))
            {
                return (null, null);
            }
            else
            {
                if (!WindowsTimeZoneIds.TryGetValue(city, out string fullTimeZone))
                {
                    return (null, null);
                }
                return (city, fullTimeZone);
            }
        }

        public (string, DateTime?) ProcessCity(string input)
        {
            string city, fullTimeZone;
            (city, fullTimeZone) = GetFullTimeZoneForCity(input);
            DateTime? localTime = null;
            if (!string.IsNullOrWhiteSpace(fullTimeZone))
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(fullTimeZone);
                    localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                }
                catch (TimeZoneNotFoundException)
                {
                    logger.LogDebug($"❌ Time zone ID '{fullTimeZone}' not found for city '{city}'");
                }
            }
            return (fullTimeZone, localTime);
        }
    }
}
