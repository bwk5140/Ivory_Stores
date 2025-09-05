using Microsoft.AspNetCore.Mvc;

namespace MethaWebsite.Services
{
    public interface IResponseProvider
    {
        Task<string> GetResponse(string intent, string userMessage);
        string ModulateResponse(string reply, string sentiment, string intent);
    }
}
