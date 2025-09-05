using MethaWebsite.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MethaWebsite.Data.ResponseModel
{
    public class PersistentConversationStateStore : IConversationStateStore
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PersistentConversationStateStore(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public ConversationState? GetState(string conversationId)
        {
            using var context = _dbFactory.CreateDbContext();
            var entity = context.ConversationStates.Find(conversationId);
            if (entity == null) return null;

            return Deserialize(entity.SerializedState);
        }

        public void SaveState(string conversationId, ConversationState state)
        {
            var serialized = Serialize(state);
            using var context = _dbFactory.CreateDbContext();
            var entity = context.ConversationStates.Find(conversationId);
            var existing = context.ConversationStates.Find(conversationId);

            if (existing != null)
            {
                existing.SerializedState = serialized;
                context.Update(existing);
            }
            else
            {
                context.Add(new ConversationStateEntity
                {
                    ConversationId = conversationId,
                    SerializedState = serialized
                });
            }

            context.SaveChanges();
        }

        private string Serialize(ConversationState state)
        {
            return JsonSerializer.Serialize(state);
        }

        private ConversationState Deserialize(string json)
        {
            return JsonSerializer.Deserialize<ConversationState>(json)!;
        }

    }
}
