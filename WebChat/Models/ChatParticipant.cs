namespace ChatAppApi.Models
{
    public class ChatParticipant
    {
        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        // Adicione outras propriedades como LastReadMessageId se quiser controlar mensagens lidas via DB
    }
}