namespace MethaWebsite.Data.ResponseModel
{
    public class SlotActionBinder
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SlotActionBinder> _logger;
        private readonly IDictionary<string, ISlotActionHandler> _handlerFactory;
        private readonly IDictionary<string, SlotDefinition> _slotDefinitions;

        public SlotActionBinder(IServiceProvider services, ILogger<SlotActionBinder> logger, IDictionary<string, SlotDefinition> slotDefinitions, IDictionary<string, ISlotActionHandler> handlerFactory)
        {
            _services = services;
            _logger = logger;
            _slotDefinitions = slotDefinitions;
            _handlerFactory = handlerFactory;
        }

        public async Task HandleSlotAsync(SlotValue slotValue)
        {
            if (!_slotDefinitions.TryGetValue(slotValue.Name, out var config))
            {
                _logger.LogWarning("No config found for slot '{slotValue}'", slotValue.Name);
                return;
            }

            var success = await TryExecute(config.OnFill, slotValue);
            if (!success && config.Fallback != null)
            {
                _logger.LogInformation("Executing fallback for slot '{SlotName}'", slotValue.Name);
                await TryExecute(config.Fallback, slotValue);
            }
        }

        private async Task<bool> TryExecute(string handlerName, SlotValue slotValue)
        {
            _handlerFactory.TryGetValue(handlerName, out var handler);
            if (handler == null)
            {
                _logger.LogError("Handler '{HandlerName}' not found for slot '{SlotName}'", handlerName, slotValue.Name);
                return false;
            }

            try
            {
                return await handler.ExecuteAsync(slotValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing handler '{HandlerName}' for slot '{SlotName}'", handlerName, slotValue.Name);
                return false;
            }
        }
    }
}
