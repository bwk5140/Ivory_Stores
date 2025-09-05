using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data
{
    public class ConversationStateEntity
    {
        [Key]
        public string ConversationId { get; set; } = default!;
        public string SerializedState { get; set; } = default!;
    }
}
