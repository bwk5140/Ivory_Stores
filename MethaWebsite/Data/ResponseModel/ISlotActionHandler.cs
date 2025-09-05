namespace MethaWebsite.Data.ResponseModel
{
    public interface ISlotActionHandler
    {
        Task<bool> ExecuteAsync(SlotValue slotValue);

    }
}
