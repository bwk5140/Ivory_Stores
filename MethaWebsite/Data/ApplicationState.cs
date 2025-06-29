using System.Security.Claims;

namespace MethaWebsite.Data
{
    public class ApplicationState
    {
        private ApplicationUser? user;

        private ClaimsPrincipal? ClaimsPrincipal;

        public event Action? OnChange;

        public void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }

        public void SetApplicationUser(ApplicationUser user)
        {
            this.user = user;
        }
        public ApplicationUser GetApplicationUser()
        {
            return this.user;
        }
        public void SetClaimsPrincipal(ClaimsPrincipal claims_principal)
        {
            this.ClaimsPrincipal = claims_principal;
        }
        public ClaimsPrincipal GetClaimsPrincipal()
        {
            return this.ClaimsPrincipal;
        }
    }
}
