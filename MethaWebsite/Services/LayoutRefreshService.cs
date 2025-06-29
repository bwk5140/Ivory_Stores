namespace MethaWebsite.Services
{
    public class LayoutRefreshService
    {
        public event Action? OnRefreshRequested;

        public void RequestRefresh() => OnRefreshRequested?.Invoke();
    }
}
