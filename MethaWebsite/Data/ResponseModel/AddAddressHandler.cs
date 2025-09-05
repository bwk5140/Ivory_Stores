using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;

namespace MethaWebsite.Data.ResponseModel
{
    public class AddAddressHandler : ISlotActionHandler
    {
        private readonly IDbContextFactory<ApplicationDbContext> _DbFactory;
        private readonly ApplicationUserService _userService;
        private readonly ApplicationUser applicationUser;
        public AddAddressHandler(IDbContextFactory<ApplicationDbContext> DbFactory, ApplicationUserService userService)
        {
            _DbFactory = DbFactory;
            _userService = userService;
            applicationUser = _userService.GetApplicationUser().Result;
        }
        async Task<bool> ISlotActionHandler.ExecuteAsync(SlotValue slotValue)
        {
            return await AddAddress(slotValue);
        }
        private async Task<bool> AddAddress(SlotValue slotValue)
        {
            string[] addressParts = slotValue.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();
            if (applicationUser is not null)
            {
                using var context = await _DbFactory.CreateDbContextAsync();
                Address address = null;
                if (addressParts.Length == 4)
                {
                    address = new Address
                    {
                        AddressLine1 = addressParts[0],
                        AddressLine2 = addressParts[1],
                        City = addressParts[2],
                        Country = addressParts[3]
                    };
                }
                if (addressParts.Length == 3)
                {
                    address = new Address
                    {
                        AddressLine1 = addressParts[0],
                        City = addressParts[1],
                        Country = addressParts[2]
                    };
                }
                if (addressParts.Length == 2)
                {
                    address = new Address
                    {
                        AddressLine1 = addressParts[0],
                        City = addressParts[1]
                    };
                }
                if (address is not null)
                {
                    address.UserId = applicationUser.Id;
                    context.Add(address);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}