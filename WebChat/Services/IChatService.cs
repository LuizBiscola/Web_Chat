using ChatAppApi.Models; // Para que a interface possa ver os modelos (Chat, Message, User)
using System.Collections.Generic; // Para IEnumerable
using System.Threading.Tasks; // Para Task

namespace ChatAppApi.Services
{
    public interface IChatService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<Chat> CreateChatAsync(string name, List<int> participantIds);
        Task<Chat?> GetChatByIdAsync(int chatId);
        Task<IEnumerable<Chat>> GetAllChatsAsync();
        // Adicione esta linha:
        Task<IEnumerable<Chat>> GetUserChatsAsync(int userId); // <<< ADICIONE ESTA LINHA

        Task<Message> AddMessageToChatAsync(int chatId, int senderId, string content);
        Task<IEnumerable<Message>> GetChatMessagesAsync(int chatId, int take = 100, int skip = 0);
    }
}