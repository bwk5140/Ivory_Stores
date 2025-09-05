using Google.Apis.Drive.v3.Data;
using MethaWebsite.Data;
using MethaWebsite.Data.ResponseModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;

namespace MethaWebsite.Services
{
    public class TemplateResponseProvider : IResponseProvider
    {
        private bool Question = false;
        private bool Gratitude = false;
        private bool ActionSentence = false;
        private string? lastBotMessage { get; set; }
        [Inject] private WorldClockService? WorldClockService { get; set; }
        private readonly HttpClient _httpClient;
        public record IntentRoute(List<string> Response, Action<string>? Action);
        public readonly Dictionary<string, IntentRoute> _responses = new();
        public TemplateResponseProvider(WorldClockService worldClockService, HttpClient httpClient)
        {
            _httpClient = httpClient;
            WorldClockService = worldClockService;
            _responses["greet"] = new(new List<string> { "Hey there! 😊 How can I help you today?", "Hello! How can I help you today?", "Hey 👋 How can I help you today?" }, null);
            _responses["general_greet"] = new(new List<string> { "Hey there! 😊 How can I help you today?", "Hello! How can I help you today?", "Hey 👋 How can I help you today?" }, null);
            _responses["goodbye"] = new(new List<string> { "Goodbye!", "Catch you later!", "Take care!" }, null);
            _responses["gratitude"] = new(new List<string> { "You're welcome!", "No problem 😊", "Glad to help!", "My pleasure" }, null);
            _responses["Smalltalk"] = new(new List<string> { "Good. How are you doing?", "Great 😊", "Not too bad. How about yourself?" }, null);
            _responses["ask_weather"] = new(new List<string> { "I'm not a weather bot, but it looks sunny in here!" }, null);
            _responses["ask_faq_data"] = new(new List<string> { "We respect your privacy concerns." +
                " We ensure that your data is handled locally within our servers and limit sharing any data with 3rd party services unless absolutely necessary." }, null);
            _responses["bill"] = new(new List<string> { "Is this related to an order?" }, null);
            _responses["billing"] = new(new List<string> { "Is this related to an order?" }, null);
            _responses["billing_not_received"] = new(new List<string> { "I can help you track down your invoice. Please provide me with the order number." }, null);
            _responses["blocked_account"] = new(new List<string> { "Accounts can be locked for various security reasons. I can help you get your account back up and running." }, null);
            _responses["cancel_payment"] = new(new List<string> { "Please take a look at our refund policy. Unfortunately, all orders are currently final." }, null);
            _responses["change_credentials"] = new(new List<string> { "Looks like you want to change some contact details." }, null);
            _responses["complaint"] = new(new List<string> { "I can certainly help with filing complaints." }, null);
            _responses["trouble"] = new(new List<string> { "What seems to be the issue?" }, null);
            _responses["trouble_account"] = new(new List<string> { "What seems to be the issue with your account?" }, null);
            _responses["trouble_delivery"] = new(new List<string> { "How can I help with your delivery?" }, null);
            _responses["trouble_payment"] = new(new List<string> { "Let's sort out your payment issue." }, null);
            _responses["deliver_cost"] = new(new List<string> { "Our delivery fees vary based on the distance from the warehouse. These are usually calculated at checkout. Delivery costs within Nairobi usually do not exceed Ksh 400." }, null);
            _responses["return_communication"] = new(new List<string> { "Could you provide a little more detail about the reason for the initial call?" }, null);
            _responses["payment"] = new(new List<string> { "It seems you need some information about payments?" }, null);
            _responses["location_store"] = new(new List<string> { "We are currently an online only store. We will update our information with store locations when that changes." }, null);
            _responses["inquire_payment_options"] = new(new List<string> { "We accept cards from most major financial institutions as well as Mpesa" }, null);
            _responses["inquire_account"] = new(new List<string> { "How can I help with your account?" }, null);
            _responses["followup_repeat"] = new(new List<string> { $"Of course. Here's a simpler explanation of what I said earlier..." }, null);
            _responses["followup_clarify"] = new(new List<string> { "Sure, let me clarify what I mean" }, null);
            _responses["followup_continue"] = new(new List<string> { $"Continuing from where we left off..." }, null);
            _responses["followup_trouble_delivery"] = new(new List<string> { $"Let's track your delivery and get this issue sorted." }, null);
            _responses["music_dislikeness"] = new(new List<string> { $"Many people share the same opinion. I have no inclination either way." }, null);
            _responses["music_likeness"] = new(new List<string> { $"Great music choices. I like your style." }, null);
            _responses["iot_coffee"] = new(new List<string> { $"I'm an Ivory Stores bot agent. I can help with questions related to products and services we offer." }, null);
            _responses["play_music"] = new(new List<string> { $"I'm an Ivory Stores bot agent. I can help with questions related to products and services we offer." }, null);
            _responses["takeaway_query"] = new(new List<string> { "There are plenty of take away options around. However, I am an Ivory Stores bot agent." }, null);
            _responses["datetime_query"] = new(new List<string> { "The time there is ." }, GetTimeInCity);
            _responses["general_quirky"] = new(new List<string> { "The time there is ." }, GetTimeInCity);
            _responses["general_joke"] = new(new List<string> { "Why do programmers prefer dark mode?\r\nBecause light attracts bugs.\r\n.",
                    "Parallel lines have so much in common.\r\nIt’s a shame they’ll never meet.\r\n", "I told my wife she was drawing her eyebrows too high.\r\nShe looked surprised.\r\n",
                    "Why can’t you trust atoms?\r\nBecause they make up everything.\r\n", "I tried to catch some fog yesterday.\r\nI mist.\r\n"}, null);
            _responses["Unknown"] = new(new List<string> { "Hmm, I’m not sure how to respond to that yet." }, null);
        }

        private void GetTimeInCity(string _)
        {
            string city;
            DateTime? date;
            (city, date) = WorldClockService.ProcessCity(_);
            if (city == null || date == null)
                _responses["datetime_query"] = new(new List<string> { "Sorry, I could not find the time zone." }, GetTimeInCity);
            else
            {
                _responses["datetime_query"] = new(new List<string> { $"The time in {city} is {date.Value.Hour}:{date.Value.Minute}" }, GetTimeInCity);
            }
        }
        public async Task<string> GetResponse(string intent, string userMessage)
        {
            var result = await _httpClient.PostAsJsonAsync("chat/respond", new ChatMessage("user", userMessage, intent, "default", DateTime.Now));
            return await result?.Content?.ReadAsStringAsync();
        }
        public string ModulateResponse(string reply, string sentiment, string intent)
        {
            if (intent.Equals("general_quirky") || intent.Equals("greet") || intent.Equals("gratitude") || 
                intent.Equals("wellness_check") || intent.Equals("goodbye") || intent.Equals("datetime_query") || 
                intent.Equals("manage_account") || intent.Equals("open_account") || intent.Equals("manage_account") || 
                intent.Equals("status_order") || intent.Equals("confirm_contact_details") || intent.Equals("confirm_address") || 
                intent.Equals("change_contact_details") || intent.Equals("update_address")|| intent.Equals("deliver_cost")|| 
                intent.Equals("inquire_delivery") ||intent.Equals("inquire_payment_options") || Question || ActionSentence)
                return reply;  // Don't modulate these responses

            return sentiment.ToLowerInvariant() switch
            {
                "0" or "negative" => $"I understand that might not be ideal. {reply}",
                "1" or "neutral" => $"Thanks for sharing your thoughts. {reply}",
                "2" or "positive" => $"Glad to hear that! {reply}",
                _ => reply
            };
        }
        bool IsFallback(string reply) =>
                reply.Contains("not sure how to respond", StringComparison.OrdinalIgnoreCase);
        bool IsGratitude(string reply) =>
                reply.Contains("thank", StringComparison.OrdinalIgnoreCase);
        bool IsGreeting(string reply) =>
            reply.Contains("Hey there", StringComparison.OrdinalIgnoreCase) || reply.Contains("Hello", StringComparison.OrdinalIgnoreCase)
            || reply.Contains("Hey", StringComparison.OrdinalIgnoreCase);
        private static bool IsQuestion(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var questionWords = new[] { "what", "what's", "why", "how", "when", "where", "who", "is", "are", "can", "do", "does", "did", "will", "would", "should", "could" };

            var trimmed = text.Trim().ToLowerInvariant();
            return trimmed.EndsWith("?") || questionWords.Any(q => trimmed.StartsWith(q + " "));
        }
        private static bool StartsWithActionVerb(string text)
        {
            var actionVerbs = new[] {
                "please", "try", "check", "look", "send", "show", "tell", "give",
                "open", "close", "run", "stop", "start", "go", "make", "build",
                "create", "delete", "update", "install", "remove", "help", "fix", "i want"
            };
            var trimmed = text.Trim().ToLowerInvariant();
            return actionVerbs.Any(v => trimmed.StartsWith(v + " "));
        }
    }

}
