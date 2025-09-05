using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;

namespace MethaWebsite.Data.ResponseModel
{
    public class SlotFillerRegistry
    {
        private readonly Dictionary<string, ISlotFiller> _fillers;

        public SlotFillerRegistry(WorldClockService clock, ApplicationUserService userService, 
                                    IDbContextFactory<ApplicationDbContext> dbFactory, IConversationStateStore conversationStore, ShippingCalculator shippingCalculator,
                                    SlotActionBinder slotActionBinder)
        {
            _fillers = new Dictionary<string, ISlotFiller>
            {
                ["datetime_query"] = new DateTimeSlotFiller(clock, conversationStore),
                ["greet"] = new GreetSlotFiller(userService, conversationStore),
                ["wellness_check"] = new WellnessCheckSlotFiller(userService),
                ["wellness_check_response"] = new WellnessCheckSlotFiller(userService),
                ["general_quirky"] = new FillerTextSlotFiller(userService),
                ["open_account"] = new CreateAccountHelpSlotFiller(),
                ["manage_account"] = new ManageAccountHelpSlotFiller(),
                ["trouble_account"] = new TroubleWithAccountHelpSlotFiller(userService),
                ["blocked_account"] = new BlockedAccountHelpSlotFiller(userService),
                ["confirm_address"] = new ConfirmAddressHelpSlotFiller(dbFactory, conversationStore),
                ["update_address"] = new UpdateAddressHelpSlotFiller(dbFactory, conversationStore, slotActionBinder, userService),
                ["confirm_contact_details"] = new ConfirmContactDetailsHelpSlotFiller(userService, conversationStore),
                ["change_contact_details"] = new ChangeContactDetailsHelpSlotFiller(userService, conversationStore),
                ["status_order"] = new OrderHelpSlotFiller(dbFactory, conversationStore),
                ["gratitude"] = new FillerTextSlotFiller(userService),
                ["goodbye"] = new FillerTextSlotFiller(userService),
                ["deliver_cost"] = new DeliverySlotFiller(dbFactory, conversationStore, shippingCalculator),
                ["inquire_delivery"] = new DeliverySlotFiller(dbFactory, conversationStore, shippingCalculator),
                ["inquire_payment_options"] = new PaymentsSlotFiller(dbFactory, conversationStore),
            };
        }
        public ISlotFiller GetFiller(string anchorId) => _fillers[anchorId];
    }
}
