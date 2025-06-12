using System.Collections.Generic;

namespace ChatAppApi.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Para chats em grupo
        public string Type { get; set; } = "direct"; // "direct" ou "group"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriedades de navegação
        public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}