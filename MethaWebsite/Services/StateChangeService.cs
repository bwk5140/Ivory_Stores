namespace MethaWebsite.Services
{
    public class StateChangeService
    {
        private bool StateHasChanged { get; set; } = false;

        public bool HasStateChanged()
        {
            return StateHasChanged;
        }
        public void ChangeState()
        {
            StateHasChanged = true;
        }
        public void Dispose()
        {
            StateHasChanged = false;
        }
    }
}
