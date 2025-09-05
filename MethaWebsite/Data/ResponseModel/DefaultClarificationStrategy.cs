namespace MethaWebsite.Data.ResponseModel
{
    public sealed class DefaultClarificationStrategy : IClarificationStrategy
    {
        public string BuildClarification(ResponseRequest request, AnchorDefinition anchor, IReadOnlyList<string> issues)
        {
            // Tailor by anchor
            if (anchor.AnchorId == "datetime_query")
            {
                if (issues.Any(i => i.Contains("city", StringComparison.OrdinalIgnoreCase)))
                    return "Which city should I check the time for?";
                if (issues.Any(i => i.Contains("time zone", StringComparison.OrdinalIgnoreCase)))
                    return "Do you have a time zone I should use?";
            }
            if (anchor.AnchorId == "status_order")
            {
                if (issues.Any(i => i.Contains("orderid", StringComparison.OrdinalIgnoreCase)))
                    return "If you provide an order number, I can help with that.";
                if (issues.Any(i => i.Contains("validordernumber", StringComparison.OrdinalIgnoreCase)))
                    return "I need a valid order number to help";
            }
            if (anchor.AnchorId == "inquire_delivery")
            {
                if (issues.Any(i => i.Contains("validordernumber", StringComparison.OrdinalIgnoreCase)))
                    return "I need a valid order number to help";
            }
            if (anchor.AnchorId == "inquire_payment_options")
            {
                if (issues.Any(i => i.Contains("validordernumber", StringComparison.OrdinalIgnoreCase)))
                    return "I need a valid order number to help";
            }
            if (anchor.AnchorId == "update_address")
            {
                if (issues.Any(i => i.Contains("defaultaddress", StringComparison.OrdinalIgnoreCase)))
                    return "I'm sorry. I can't update a default address. You can add a new address and set it as a default first.";
            }

            // Fallback generic
            return "Could you clarify a bit so I can help better?";
        }
    }
}
