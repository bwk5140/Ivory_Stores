namespace MethaWebsite.Data.ResponseModel
{
    public class ConversationIdProvider : IConversationIdProvider
    {
        private const string SessionKey = "ConversationId";
        private const string CookieKey = "ConversationId";

        public string GetConversationId(HttpContext context)
        {
            // 1. Try session
            if (context.Session?.IsAvailable == true)
            {
                var sessionId = context.Session.GetString(SessionKey);
                if (!string.IsNullOrEmpty(sessionId))
                    return sessionId;

                var newId = Guid.NewGuid().ToString();
                context.Session.SetString(SessionKey, newId);
                return newId;
            }

            // 2. Try cookie
            if (context.Request.Cookies.TryGetValue(CookieKey, out var cookieId) && !string.IsNullOrEmpty(cookieId))
            {
                return cookieId;
            }

            // 3. Fallback: generate and set cookie
            var fallbackId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append(CookieKey, fallbackId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return fallbackId;
        }
    }
}
