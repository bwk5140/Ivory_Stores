using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MethaWebsite.Data.ResponseModel
{
    public class EntityRecognizer(WorldClockService worldClockService, IConversationStateStore conversationStateStore, ApplicationUserService userService, IDbContextFactory<ApplicationDbContext> DbFactory)
    {
        private readonly WorldClockService worldClockService = worldClockService;
        private readonly ApplicationUserService _userService = userService;
        private readonly IConversationStateStore _conversationStateStore = conversationStateStore;
        private readonly HashSet<string> KnownCities = worldClockService.GetCities();
        private readonly Regex DateRegex = new(@"\b(?:\d{1,2}[/-])?\d{1,2}[/-]\d{2,4}\b", RegexOptions.IgnoreCase);
        private readonly Regex TimeRegex = new(@"\b\d{1,2}(:\d{2})?\s?(am|pm)?\b", RegexOptions.IgnoreCase);
        private readonly Regex EmailRegex = new(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase);
        private readonly Regex UsernameRegex = new Regex(@"^[a-zA-Z0-9_]{3,20}$");
        private readonly Regex PasswordRegex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
        private readonly Regex PhoneRegex = new Regex(@"(?:\+?\d{1,3}[\s-]?)?(?:\(?\d{2,4}\)?[\s-]?)?\d{3,4}[\s-]?\d{3,4}");
        private readonly Regex AddressRegex = new Regex(@"(?is)(?:(?<Building>[a-zA-Z0-9\s]+?),\s*(?<Floor>\d{1,2}(?:st|nd|rd|th)?\s+Floor)\s*)?(?:\s*(?<StreetNumber>\d{1,6}(?:-\d{1,6})?)\s+)?(?<StreetName>(?:[a-zA-Z]+\s+){1,4})(?<StreetType>street|strt|st\.?|avenue|ave\.?|road|rd\.?|boulevard|blvd\.?|lane|lne|ln\.?|drive|dr\.?|court|crt|ct\.?|way|place|pl\.?|plc|terrace|ter\.?|trc|square|sq\.?|close|cls\.?|clse\.?|bypass|estate|est\.?)\b(?:,\s*(?<Neighborhood>[a-zA-Z\s]+))?(?:,\s*(?<City>[a-zA-Z\s]+))?(?:,\s*(?<Country>[a-zA-Z\s]+))?");

        public IDictionary<string, string> ExtractEntities(string text, string locale, string conversationId)
        {
            var result = new Dictionary<string, string>();
            var new_text = TextNormalizer.Normalize(text, locale);
            int maxDistance = 2;
            // City matching
            foreach (var city in KnownCities)
            {
                if (new_text.Contains(city.ToLowerInvariant()))
                {
                    result["City"] = city;
                    break;
                }
            }
            // City fuzzy matching
            if (!result.TryGetValue("City", out var city_match))
            {
                foreach (var city in KnownCities)
                {
                    int distance = Levenshtein(new_text, city.ToLowerInvariant());
                    if (distance <= maxDistance)
                    {
                        result["CorrectionCity"] = city;
                        result["City"] = city;
                        break;
                    }
                }
            }

            // User Contact Details Matching
            var user = _userService.GetApplicationUser().Result;
            if (user is not null)
            {
                // Email Matching
                var email = user.Email;
                result.TryGetValue("Email", out var user_email);
                if (email is not null && user_email is not null && email == user_email)
                {
                    result["EmailMatch"] = "email matches";
                }
                // Phone Matching
                var phone = user.PhoneNumber;
                result.TryGetValue("Phone", out var user_phone);
                if (phone is not null && user_phone is not null && (phone == user_phone ||
                                                                     phone.Contains(user_phone) ||
                                                                     phone.Contains(user_phone.Substring(1))))
                {
                    result["PhoneMatch"] = "phone matches";
                }
            }

            // Address detection
            var addressMatch = AddressRegex.Match(new_text);
            if (addressMatch.Success)
            {
                result["Address"] = text;
                using var context = DbFactory.CreateDbContext();
                var addresses = context.Address.Where(a => a.UserId == user.Id).ToList();
                var matchedAddress = addresses.FirstOrDefault(a => new_text.Contains(a.AddressLine1, StringComparison.OrdinalIgnoreCase));
                if (matchedAddress is null)
                {
                    result["NewAddress"] = text;
                }
            }

            // Date detection
            var dateMatch = DateRegex.Match(new_text);
            if (dateMatch.Success)
                result["Date"] = dateMatch.Value;

            // Time detection
            var timeMatch = TimeRegex.Match(new_text);
            if (timeMatch.Success)
                result["Time"] = timeMatch.Value;

            // Email detection
            var emailMatch = EmailRegex.Match(new_text);
            if (emailMatch.Success)
                result["Email"] = emailMatch.Value;

            // Username detection
            var usernameMatch = UsernameRegex.Match(new_text);
            if (usernameMatch.Success)
                result["Username"] = usernameMatch.Value;

            // Password detection
            var passwordMatch = PasswordRegex.Match(new_text);
            if (passwordMatch.Success)
                result["Password"] = passwordMatch.Value;

            // Phone detection
            var phoneMatch = PhoneRegex.Match(new_text);
            if (phoneMatch.Success)
                result["Phone"] = phoneMatch.Value;

            
            // TimeOfDay Matching
            //var time = DateTime.Now;
            //if (time.Hour > 0 && time.Hour < 12)
            //{
            //    result["Morning"] = time.Hour.ToString() + ":" + time.Minute.ToString() + ":" + time.Second.ToString();
            //}
            //else if (time.Hour > 12 && time.Hour < 16)
            //{
            //    result["Afternoon"] = time.Hour.ToString() + ":" + time.Minute.ToString() + ":" + time.Second.ToString();
            //}
            //else
            //{
            //    result["Evening"] = time.Hour.ToString() + ":" + time.Minute.ToString() + ":" + time.Second.ToString();
            //}
            return result;
        }
        public static int Levenshtein(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b.Length;
            if (string.IsNullOrEmpty(b)) return a.Length;

            var dp = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost
                    );
                }

            return dp[a.Length, b.Length];
        }
    }
}
