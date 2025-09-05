namespace MethaWebsite.Services
{
    public class CookieReaderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieReaderService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCookie(string name)
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request?.Cookies[name];
        }
    }

}
