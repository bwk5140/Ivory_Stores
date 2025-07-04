using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class MpesaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public MpesaService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> InitiateStkPush(string phoneNumber, decimal amount)
        {
            var token = await GetAccessTokenAsync();

            var payload = new
            {
                BusinessShortCode = _config["Mpesa:ShortCode"],
                Password = GeneratePassword(),
                Timestamp = GetTimestamp(),
                TransactionType = "CustomerPayBillOnline",
                Amount = amount,
                PartyA = phoneNumber,
                PartyB = _config["Mpesa:ShortCode"],
                PhoneNumber = phoneNumber,
                CallBackURL = _config["Mpesa:CallbackUrl"],
                AccountReference = "BlazorApp",
                TransactionDesc = "Payment"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config["Mpesa:ConsumerKey"]}:{_config["Mpesa:ConsumerSecret"]}"));
            var request = new HttpRequestMessage(HttpMethod.Get, "https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json).RootElement.GetProperty("access_token").GetString();
        }

        private string GetTimestamp() => DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        private string GeneratePassword()
        {
            var plain = $"{_config["Mpesa:ShortCode"]}{_config["Mpesa:PassKey"]}{GetTimestamp()}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));
        }
    }
}
