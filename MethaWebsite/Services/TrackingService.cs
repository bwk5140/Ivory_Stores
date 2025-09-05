namespace MethaWebsite.Services
{
    public class TrackingService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TrackingService(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        public string GetDeviceId()
        {
            return _httpContextAccessor.HttpContext?.Items["DeviceId"]?.ToString();
        }

    }
}
