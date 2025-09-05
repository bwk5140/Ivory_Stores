using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;

namespace MethaWebsite.Data.ResponseModel
{
    public class UpdateAddressHandler : ISlotActionHandler
    {
        private readonly IDbContextFactory<ApplicationDbContext> _DbFactory;
        private readonly ApplicationUserService _userService;
        private readonly ApplicationUser applicationUser;
        public UpdateAddressHandler(IDbContextFactory<ApplicationDbContext> DbFactory, ApplicationUserService userService)
        {
            _DbFactory = DbFactory;
            _userService = userService;
            applicationUser = _userService.GetApplicationUser().Result;
        }
        async Task<bool> ISlotActionHandler.ExecuteAsync(SlotValue slotValue)
        {
            return await UpdateAddress(slotValue);
        }
        private async Task<bool> UpdateAddress(SlotValue slotValue)
        {
            string[] addressParts = slotValue.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();
            using var context = await _DbFactory.CreateDbContextAsync();
            if (applicationUser is not null)
            {
                var addresses = context.Address.Where(a => a.UserId == applicationUser.Id).ToList();
                var address = addresses.FirstOrDefault(a => a.AddressLine1.Contains(addressParts[0], StringComparison.OrdinalIgnoreCase));
                if (address != null)
                {
                    if (!address.DefaultAddress)
                    {
                        if (addressParts.Length == 5)
                        {
                            address.AddressLine1 = addressParts[0];
                            address.AddressLine2 = addressParts[1] + ", " + addressParts[2];
                            address.City = addressParts[3];
                            address.Country = addressParts[4];
                        }
                        if (addressParts.Length == 4)
                        {
                            address.AddressLine1 = addressParts[0];
                            address.AddressLine2 = addressParts[1];
                            address.City = addressParts[2];
                            address.Country = addressParts[3];
                        }
                        if (addressParts.Length == 3)
                        {
                            address.AddressLine1 = addressParts[0];
                            address.City = addressParts[1];
                            address.Country = addressParts[2];
                        }
                        if (addressParts.Length == 2)
                        {
                            address.AddressLine1 = addressParts[0];
                            address.AddressLine2 = addressParts[1];
                            address.City = addressParts[2];
                        }
                        if (address is not null)
                        {
                            context.Attach(address).State = EntityState.Modified;
                            await context.SaveChangesAsync();
                        }
                        await context.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (addressParts.Length == 5)
                    {
                        address = new Address
                        {
                            AddressLine1 = addressParts[0],
                            AddressLine2 = addressParts[1] + ", " + addressParts[2],
                            City = addressParts[3],
                            Country = addressParts[4]
                        };
                        
                    }
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
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}