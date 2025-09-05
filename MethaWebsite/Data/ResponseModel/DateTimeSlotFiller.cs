using MethaWebsite.Services;
using static System.Net.Mime.MediaTypeNames;

namespace MethaWebsite.Data.ResponseModel
{
    public class DateTimeSlotFiller : ISlotFiller
    {
        private readonly WorldClockService _clock;
        private readonly IConversationStateStore _conversationStore;

        public DateTimeSlotFiller(WorldClockService clock, IConversationStateStore conversationStore)
        {
            _clock = clock;
            _conversationStore = conversationStore;
        }

        public Dictionary<string, string> FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            var state = _conversationStore.GetState(conversationId);

            extractedSlots.TryGetValue("CorrectionCity", out var correctionCity);
            extractedSlots.TryGetValue("Yes", out var yesResponse);
            if (yesResponse is not null)
            {
                if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                if (correctionCity is not null)
                {
                    var (timeZoneId, localTime) = _clock.ProcessCity(correctionCity.Value);
                    if (localTime is not null)
                    {
                        return new Dictionary<string, string>
                        {
                            ["CorrectionCity"] = correctionCity.Value,
                            ["Time"] = localTime.Value.ToString("hh:mm tt")
                        };
                    }
                }
            }
            extractedSlots.TryGetValue("No", out var noResponse);
            if (noResponse is not null)
            {
                if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                return new Dictionary<string, string>
                {
                    ["No"] = noResponse.Value
                };
            }
            extractedSlots.TryGetValue("City", out var city);
            if (correctionCity is not null)
            {
                return new Dictionary<string, string>
                {
                    ["CorrectionCity"] = correctionCity.Value
                };
            }
            if (city is not null)
            {
                var (timeZoneId, localTime) = _clock.ProcessCity(city.Value);
                if (localTime is not null)
                {
                    return new Dictionary<string, string>
                    {
                        ["City"] = city.Value,
                        ["Time"] = localTime.Value.ToString("hh:mm tt")
                    };
                }
            }
            return new Dictionary<string, string>();
        }
    }
}
