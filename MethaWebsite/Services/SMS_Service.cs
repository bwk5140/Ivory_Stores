using AfricasTalkingCS;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using PhoneNumbers;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            message = "Your Ivory Stores verification code is: " + message;
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
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.africastalking.com/version1/messaging")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "username", username },
                    { "to", formattedNumber },
                    { "message", message }
                })
            };
            request.Headers.Add("apiKey", apiKey);
            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();


            //TwilioClient.Init(accountSid, authToken);
            //var msg = await MessageResource.CreateAsync(
            //        body: message,
            //        from: new Twilio.Types.PhoneNumber("+15732601144"),
            //        to: new Twilio.Types.PhoneNumber(formattedNumber));
        }
    }
}
