using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using static System.Net.Mime.MediaTypeNames;

namespace MethaWebsite.Data.ResponseModel
{
    public sealed class BasicResponseValidator(IConversationStateStore stateStore, 
                                                IHttpContextAccessor httpContextAccessor, 
                                                IDbContextFactory<ApplicationDbContext> DbFactory,
                                                ApplicationUserService userService) : IResponseValidator
    {
        private readonly IConversationStateStore _stateStore = stateStore;
        private readonly IDbContextFactory<ApplicationDbContext> _DbFactory = DbFactory;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ApplicationUserService _userService = userService;
        public IReadOnlyList<string> Validate(
            ResponseRequest request,
            AnchorDefinition anchor,
            IReadOnlyDictionary<string, SlotValue> slots)
        {
            var issues = new List<string>();
            var httpContext = _httpContextAccessor.HttpContext;
            var conversationId = httpContext.Session.GetString("ConversationId");
            var state = _stateStore.GetState(conversationId);

            foreach (var reqSlot in anchor.Slots.Where(s => s.Required))
            {
                if (!slots.TryGetValue(reqSlot.Name, out var v) || string.IsNullOrWhiteSpace(v.Value))
                    issues.Add($"Missing required slot '{reqSlot.Name}'.");
            }

            // Example: for TimeQuery, require either city or timeZoneId
            if (anchor.AnchorId == "datetime_query")
            {
                var hasCity = slots.TryGetValue("city", out var c) && !string.IsNullOrWhiteSpace(c.Value);
                var hasTz = slots.TryGetValue("timeZoneId", out var tz) && !string.IsNullOrWhiteSpace(tz.Value);
                var hasYes = slots.TryGetValue("Yes", out var yes) && !string.IsNullOrWhiteSpace(yes.Value);
                var hasNo = slots.TryGetValue("No", out var no) && !string.IsNullOrWhiteSpace(no.Value);
                var hasCorrectionCity = slots.TryGetValue("CorrectionCity", out var cc) && !string.IsNullOrWhiteSpace(cc.Value);
                if (!hasCity && !hasTz && !hasCorrectionCity && !hasYes) issues.Add("Need a city or time zone to answer.");
                if (hasCorrectionCity)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "CorrectionCity",
                        ProposedValue = cc.Value,
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance,
                    });
                    _stateStore.SaveState(conversationId, state);
                }
                if (hasYes)
                {
                    issues.Clear();
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "Yes",
                        ProposedValue = "Yes",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance,
                    });
                    _stateStore.SaveState(conversationId, state);
                }
                if (hasNo)
                {
                    issues.Clear();
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "No",
                        ProposedValue = "No",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance,
                    });
                    _stateStore.SaveState(conversationId, state);
                }
            }
            if (anchor.AnchorId == "status_order")
            {
                var hasOrderId = slots.TryGetValue("orderid", out var id) && !string.IsNullOrWhiteSpace(id.Value);
                var hasUserResponse = slots.TryGetValue("userresponse", out var user_response) && !string.IsNullOrWhiteSpace(user_response.Value);
                if (!hasOrderId && !hasUserResponse) issues.Add("Need an orderid to answer.");
                
                if (hasUserResponse || hasOrderId)
                {
                    if (hasOrderId)
                    {
                        using var context = _DbFactory.CreateDbContext();
                        var OrderId = context.Order.FirstOrDefault(o => o.Id == id.Value);
                        if (OrderId == null)
                        {
                            issues.Add("Need a validordernumber to answer.");
                        }
                        else
                        {
                            issues.Clear();
                            state.LastPromptedSlot = null;
                            if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
                        }
                    }
                    else
                    {
                        issues.Clear();
                        state.LastPromptedSlot = null;
                        if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
                    }
                }
                else
                {
                    if (!hasOrderId)
                    {
                        state.PendingConfirmations.Enqueue(new ConfirmationContext
                        {
                            SlotName = "UserResponse",
                            ProposedValue = "No order number",
                            Timestamp = DateTime.UtcNow,
                            SourceUtterance = request.Utterance
                        });
                    }     
                }
                _stateStore.SaveState(conversationId, state);
            }
            if (anchor.AnchorId == "confirm_contact_details" || anchor.AnchorId == "change_contact_details" )
            {
                var hasEmailMatch = slots.TryGetValue("emailmatch", out var email_match) && !string.IsNullOrWhiteSpace(email_match.Value);
                var hasPhoneMatch = slots.TryGetValue("phonematch", out var phone_match) && !string.IsNullOrWhiteSpace(phone_match.Value);
                //if (!hasEmailMatch && !hasPhoneMatch) issues.Add("Need an orderid to answer.");
                
                if (hasEmailMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "EmailMatch",
                        ProposedValue = "email matched",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (hasPhoneMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "PhoneMatch",
                        ProposedValue = "phone matched",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                _stateStore.SaveState(conversationId, state);
            }
            if (anchor.AnchorId == "update_address" )
            {
                var hasAddressMatch = slots.TryGetValue("address", out var address) && !string.IsNullOrWhiteSpace(address.Value);
                var hasNewAddressMatch = slots.TryGetValue("newaddress", out var newAddress) && !string.IsNullOrWhiteSpace(newAddress.Value);
                var hasAddressConfirmationNoMatch = slots.TryGetValue("address_confirmation_no", out var address_confirmation_no) && !string.IsNullOrWhiteSpace(address_confirmation_no.Value);
                var hasAddressConfirmationYesMatch = slots.TryGetValue("address_confirmation_yes", out var address_confirmation_yes) && !string.IsNullOrWhiteSpace(address_confirmation_yes.Value);
                var hasUpdateAddressConfirmationYesMatch = slots.TryGetValue("update_address_confirmation_no", out var update_address_confirmation_no) && !string.IsNullOrWhiteSpace(update_address_confirmation_no.Value);
                var hasUpdateAddressConfirmationNoMatch = slots.TryGetValue("update_address_confirmation_yes", out var update_address_confirmation_yes) && !string.IsNullOrWhiteSpace(update_address_confirmation_yes.Value);
                var hasAddAddressYesMatch = slots.TryGetValue("add_address_confirmation_yes", out var add_address_confirmation_yes) && !string.IsNullOrWhiteSpace(add_address_confirmation_yes.Value);
                var hasAddAddressNoMatch = slots.TryGetValue("add_address_confirmation_no", out var add_address_confirmation_no) && !string.IsNullOrWhiteSpace(add_address_confirmation_no.Value);

                //if (hasUpdateAddressConfirmationNoMatch)
                //{
                //    state.PendingConfirmations.Enqueue(new ConfirmationContext
                //    {
                //        SlotName = "update_address_confirmation_no",
                //        ProposedValue = "No",
                //        Timestamp = DateTime.UtcNow,
                //        SourceUtterance = request.Utterance
                //    });
                //}
                if (hasUpdateAddressConfirmationYesMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "newaddress",
                        ProposedValue = "newaddress",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                //if (hasAddAddressNoMatch)
                //{
                //    state.PendingConfirmations.Enqueue(new ConfirmationContext
                //    {
                //        SlotName = "add_address_confirmation_no",
                //        ProposedValue = "No",
                //        Timestamp = DateTime.UtcNow,
                //        SourceUtterance = request.Utterance
                //    });
                //}
                //if (hasAddAddressYesMatch)
                //{
                //    state.PendingConfirmations.Enqueue(new ConfirmationContext
                //    {
                //        SlotName = "add_address_confirmation_yes",
                //        ProposedValue = "Yes",
                //        Timestamp = DateTime.UtcNow,
                //        SourceUtterance = request.Utterance
                //    });
                //}
                if (hasAddressConfirmationNoMatch)
                {
                    //state.PendingConfirmations.Enqueue(new ConfirmationContext
                    //{
                    //    SlotName = "address_confirmation_no",
                    //    ProposedValue = "No",
                    //    Timestamp = DateTime.UtcNow,
                    //    SourceUtterance = request.Utterance
                    //});
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "update_address_confirmation_yes",
                        ProposedValue = "Yes",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "update_address_confirmation_no",
                        ProposedValue = "No",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (hasAddressConfirmationYesMatch)
                {
                    //state.PendingConfirmations.Enqueue(new ConfirmationContext
                    //{
                    //    SlotName = "address_confirmation_yes",
                    //    ProposedValue = "Yes",
                    //    Timestamp = DateTime.UtcNow,
                    //    SourceUtterance = request.Utterance
                    //});
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "update_address_confirmation_yes",
                        ProposedValue = "Yes",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "update_address_confirmation_no",
                        ProposedValue = "No",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (!hasAddressMatch && !state.FilledSlots.Any() && !hasNewAddressMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "Address",
                        ProposedValue = "address",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                else
                {
                    if (hasAddressMatch)
                    {
                        var user = _userService.GetApplicationUser().Result;
                        if (user is not null)
                        {
                            using var context = _DbFactory.CreateDbContext();
                            var fullAddress = address.Value.ToString();
                            var Address = context.Address.AsEnumerable().FirstOrDefault(a => fullAddress.Contains(a.AddressLine1, StringComparison.OrdinalIgnoreCase)
                                                                                && a.UserId == user.Id);
                            if (Address != null && Address.DefaultAddress)
                            {
                                issues.Add("Can't use a defaultaddress.");
                            }
                        }
                        if (state.FilledSlots.Any() && !hasNewAddressMatch)
                        {
                            state.CurrentStage = ConfirmationStage.ConfirmAddress;
                            state.PendingConfirmations.Enqueue(new ConfirmationContext
                            {
                                SlotName = "address_confirmation_yes",
                                ProposedValue = "Yes",
                                Timestamp = DateTime.UtcNow,
                                SourceUtterance = request.Utterance
                            });
                            state.PendingConfirmations.Enqueue(new ConfirmationContext
                            {
                                SlotName = "address_confirmation_no",
                                ProposedValue = "No",
                                Timestamp = DateTime.UtcNow,
                                SourceUtterance = request.Utterance
                            });
                        }
                        
                    }
                }
                _stateStore.SaveState(conversationId, state);
            }
            if (anchor.AnchorId == "deliver_cost" )
            {
                var hasAddressMatch = slots.TryGetValue("address", out var address) && !string.IsNullOrWhiteSpace(address.Value);
                var hasYesMatch = slots.TryGetValue("yes", out var yes) && !string.IsNullOrWhiteSpace(yes.Value);
                var hasNoMatch = slots.TryGetValue("no", out var no) && !string.IsNullOrWhiteSpace(no.Value);
                var hasUserResponse = slots.TryGetValue("userresponse", out var user_response) && !string.IsNullOrWhiteSpace(user_response.Value);
                if (hasYesMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "Yes",
                        ProposedValue = "Yes",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (!hasAddressMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "Address",
                        ProposedValue = "address",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (hasNoMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "No",
                        ProposedValue = "No",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                _stateStore.SaveState(conversationId, state);
            }
            if (anchor.AnchorId == "inquire_delivery" )
            {
                var hasOrderIdMatch = slots.TryGetValue("orderid", out var order_id) && !string.IsNullOrWhiteSpace(order_id.Value);
                var hasYesMatch = slots.TryGetValue("yes", out var yes) && !string.IsNullOrWhiteSpace(yes.Value);
                var hasNoMatch = slots.TryGetValue("no", out var no) && !string.IsNullOrWhiteSpace(no.Value);
                var hasUserResponse = slots.TryGetValue("userresponse", out var user_response) && !string.IsNullOrWhiteSpace(user_response.Value);
                if (hasYesMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "Yes",
                        ProposedValue = "Yes",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (!hasOrderIdMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "OrderId",
                        ProposedValue = "OrderId",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (hasNoMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "No",
                        ProposedValue = "No",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (hasOrderIdMatch)
                {
                    using var context = _DbFactory.CreateDbContext();
                    var OrderId = context.Order.FirstOrDefault(o => o.Id == order_id.Value);
                    if (OrderId == null)
                    {
                        issues.Add("Need a validordernumber to answer.");
                    }
                    else
                    {
                        issues.Clear();
                    }
                }
                _stateStore.SaveState(conversationId, state);
            }
            if (anchor.AnchorId == "inquire_payment_options" )
            {
                var hasOrderIdMatch = slots.TryGetValue("orderid", out var order_id) && !string.IsNullOrWhiteSpace(order_id.Value);
                var hasYesMatch = slots.TryGetValue("yes", out var yes) && !string.IsNullOrWhiteSpace(yes.Value);
                var hasNoMatch = slots.TryGetValue("no", out var no) && !string.IsNullOrWhiteSpace(no.Value);
                var hasUserResponse = slots.TryGetValue("userresponse", out var user_response) && !string.IsNullOrWhiteSpace(user_response.Value);
                if (hasYesMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "Yes",
                        ProposedValue = "Yes",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (!hasOrderIdMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "OrderId",
                        ProposedValue = "OrderId",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (hasNoMatch)
                {
                    state.PendingConfirmations.Enqueue(new ConfirmationContext
                    {
                        SlotName = "No",
                        ProposedValue = "No",
                        Timestamp = DateTime.UtcNow,
                        SourceUtterance = request.Utterance
                    });
                }
                if (hasOrderIdMatch)
                {
                    using var context = _DbFactory.CreateDbContext();
                    var OrderId = context.Order.FirstOrDefault(o => o.Id == order_id.Value);
                    if (OrderId == null)
                    {
                        issues.Add("Need a validordernumber to answer.");
                    }
                    else
                    {
                        issues.Clear();
                    }
                }
                _stateStore.SaveState(conversationId, state);
            }
            return issues;
        }
    }
}
