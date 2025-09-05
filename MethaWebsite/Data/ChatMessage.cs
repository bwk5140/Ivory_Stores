namespace MethaWebsite.Data
{
    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "bot"
        public string Text { get; set; }
        public string Intent { get; set; }
        public string Context { get; set; }
        public DateTime Timestamp { get; set; }


        public ChatMessage(string role, string text, string intent, string context, DateTime timeStamp)
        {
            Role = role;
            Text = text;
            Intent = intent;
            Context = context;
            Timestamp = timeStamp;
        }
    }
}
