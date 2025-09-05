using Microsoft.Extensions.Caching.Memory;

namespace MethaWebsite.Data.ResponseModel
{
    public class CachedConversationStateStore : IConversationStateStore
    {
        private readonly IMemoryCache _cache;
        private readonly IConversationStateStore _fallbackStore;
        private readonly TimeSpan _cacheDuration;

        public CachedConversationStateStore(IMemoryCache cache,
                                            IConversationStateStore fallbackStore,
                                            TimeSpan cacheDuration)
        {
            _cache = cache;
            _fallbackStore = fallbackStore;
            _cacheDuration = cacheDuration;
        }

        public ConversationState? GetState(string conversationId)
        {
            if (_cache.TryGetValue(conversationId, out ConversationState? cachedState))
            {
                return cachedState;
            }

            var fallbackState = _fallbackStore.GetState(conversationId);
            if (fallbackState != null)
            {
                _cache.Set(conversationId, fallbackState, _cacheDuration);
            }

            return fallbackState;
        }

        public void SaveState(string conversationId, ConversationState state)
        {
            _fallbackStore.SaveState(conversationId, state);
            _cache.Set(conversationId, state, _cacheDuration);
        }
    }
}
