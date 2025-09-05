namespace MethaWebsite.Data.ResponseModel
{
    public class ConfirmationContext
    {
        public string SlotName { get; set; }
        public string ProposedValue { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SourceUtterance { get; set; }

    }
}
