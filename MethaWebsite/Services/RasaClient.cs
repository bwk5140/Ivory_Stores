namespace MethaWebsite.Services
{
    public class RasaClient
    {
        private readonly HttpClient _http;

        public RasaClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> SendMessageAsync(string message)
        {
            var payload = new { sender = "user", message };
            var response = await _http.PostAsJsonAsync("http://localhost:5005/webhooks/rest/webhook", payload);
            var result = await response.Content.ReadFromJsonAsync<List<RasaResponse>>();
            return result?.FirstOrDefault()?.Text ?? "No response.";
        }
    }

    public class RasaResponse
    {
        public string? Text { get; set; }
    }
}
