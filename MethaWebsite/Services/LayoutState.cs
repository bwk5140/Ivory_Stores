namespace MethaWebsite.Services
{
    public class LayoutState
    {
        public string BackgroundClass { get; set; } = "bg-default";
        public event Action? OnChange;

        public void SetBackground(string cssClass)
        {
            BackgroundClass = cssClass;
            OnChange?.Invoke();
        }
    }
}
