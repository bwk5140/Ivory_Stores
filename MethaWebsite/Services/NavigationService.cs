using Microsoft.JSInterop;

namespace MethaWebsite.Services
{
    public class NavigationService
    {
        private readonly IJSRuntime _js;

        public NavigationService(IJSRuntime js) => _js = js;

        public async Task OpenInNewTab(string url)
        {
            await _js.InvokeVoidAsync("openInNewTab", url);
        }
    }
}
