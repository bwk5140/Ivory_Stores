using PhoneNumbers;
using System.Globalization;

namespace MethaWebsite.Services
{
    public class SMS_Service
    {
        //private string accountSid, authToken;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public SMS_Service(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }


        public async Task<string> SendSmsAsync(string message, string phoneNumber)
        {
            var username = _config["AfricasTalking:Username"];
            var apiKey = _config["AfricasTalking:ApiKey"];

            var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            string region = CultureInfo.CurrentCulture.Name.Substring(3);
            string cleanInput = phoneNumber.Trim();
            string formattedNumber;
            PhoneNumbers.PhoneNumber number;

            try
            {
                // Use null region for international numbers
                number = cleanInput.StartsWith("+")
                    ? phoneUtil.Parse(cleanInput, null)
                    : phoneUtil.Parse(cleanInput, region);

                if (!phoneUtil.IsValidNumber(number))
                    return null;

            }
            catch (NumberParseException)
            {
                return null;
            }
            formattedNumber = phoneUtil.Format(number, PhoneNumberFormat.E164);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sandbox.africastalking.com/version1/messaging")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "username", username },
                    { "from", "9165"},
                    { "to", formattedNumber },
                    { "message", message }
                })
            };
            request.Headers.Add("apiKey", apiKey);
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;


            //TwilioClient.Init(accountSid, authToken);
            //var msg = await MessageResource.CreateAsync(
            //        body: message,
            //        from: new Twilio.Types.PhoneNumber("+15732601144"),
            //        to: new Twilio.Types.PhoneNumber(formattedNumber));
        }
        public async Task<string> SendSmsCompleteNumberAsync(string message, string phoneNumber)
        {
            var username = _config["AfricasTalking:Username"];
            var apiKey = _config["AfricasTalking:ApiKey"];

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sandbox.africastalking.com/version1/messaging")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "username", username },
                    { "from", "9165"},
                    { "to", phoneNumber },
                    { "message", message }
                })
            };
            request.Headers.Add("apiKey", apiKey);
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;


            //TwilioClient.Init(accountSid, authToken);
            //var msg = await MessageResource.CreateAsync(
            //        body: message,
            //        from: new Twilio.Types.PhoneNumber("+15732601144"),
            //        to: new Twilio.Types.PhoneNumber(formattedNumber));
        }
    }
}
