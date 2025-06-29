using MethaWebsite.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MethaWebsite.Services
{
    public class ApplicationUserService
    {
        private System.Security.Claims.ClaimsPrincipal? claimsPrincipal { get; set; }
        private ApplicationUser? ApplicationUser { get; set; }
        private AuthenticationState? AuthState { get; set; }
        private UserManager<ApplicationUser>? UserManager { get; set; }
        
        private async Task InitializeUser()
        {
            claimsPrincipal = AuthState.User;
            ApplicationUser = await UserManager.GetUserAsync(claimsPrincipal);
        }
        public async Task<ApplicationUser> GetApplicationUser(AuthenticationState authState, UserManager<ApplicationUser> userManager)
        {
            if (ApplicationUser == null)
            {
                AuthState = authState;
                UserManager = userManager;
                await InitializeUser();
            }
            return ApplicationUser;
        }
        public void UpdateUser(ApplicationUser user)
        {
            this.ApplicationUser = user;
        }
    }
}
