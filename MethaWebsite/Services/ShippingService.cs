using MethaWebsite.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class ShippingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ShippingService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private async Task<double[]> GetShippingCoordinates(Data.Address address)
        {
            var addr = address.AddressLine1 + " " + address.AddressLine2 + "," + address.ZipCode + " " + address.City + "," + address.Country;
            var ApiKey = _config["OpenCage:ApiKey"];
            string requestURI = $"https://api.opencagedata.com/geocode/v1/json?q={addr}&key={ApiKey}";
            var response = await _httpClient.GetStringAsync(requestURI);
            using JsonDocument json = JsonDocument.Parse(response);
            var root = json.RootElement;
            var location = root
            .GetProperty("results")[0]
            .GetProperty("geometry");

            double lat = location.GetProperty("lat").GetDouble();
            double lng = location.GetProperty("lng").GetDouble();
            double[] coordinates = { lng, lat };
            return coordinates;
        }
        private async Task<double[]> GetShippingCoordinates(string address)
        {
            var ApiKey = _config["OpenCage:ApiKey"];
            string requestURI = $"https://api.opencagedata.com/geocode/v1/json?q={address}&key={ApiKey}";
            var response = await _httpClient.GetStringAsync(requestURI);
            using JsonDocument json = JsonDocument.Parse(response);
            var root = json.RootElement;
            var location = root
            .GetProperty("results")[0]
            .GetProperty("geometry");

            double lat = location.GetProperty("lat").GetDouble();
            double lng = location.GetProperty("lng").GetDouble();
            double[] coordinates = { lng, lat };
            return coordinates;
        }
        public async Task<double> GetDistance(Data.Address address)
        {
            double[] coordinates1 = await GetShippingCoordinates(address);
            double[] coordinates2 = await GetShippingCoordinates("The Hub Karen, Dagoretti Rd, Nairobi, Kenya");


            var apiKey = _config["Heigit:ApiKey"];
            string url = "https://api.openrouteservice.org/v2/directions/driving-car";

            var coordinates = new
            {
                coordinates = new[]
                {
                coordinates1,
                coordinates2
            }
            };

            var json = JsonSerializer.Serialize(coordinates);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

            var response = await httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            var summary = doc.RootElement
                             .GetProperty("routes")[0]
                             .GetProperty("summary");

            double distanceMeters = summary.GetProperty("distance").GetDouble();
            return distanceMeters / 1000;

        }
    }
}
