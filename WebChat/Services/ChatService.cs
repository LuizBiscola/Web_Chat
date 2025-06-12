using ChatAppApi.Data;
using ChatAppApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Certifique-se que está presente
using System.Threading.Tasks; // Certifique-se que está presente

namespace ChatAppApi.Services
{
    // ... (definição da interface IChatService, se você a mantém no mesmo arquivo,
    // embora seja recomendado em arquivo separado)

    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- User Methods ---
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        // <<< ADICIONE ESTE BLOCO ABAIXO (ou verifique se ele já está e se a assinatura está correta)
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
        // >>> FIM DO BLOCO A SER ADICIONADO

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // --- Chat Methods ---
        public async Task<Chat> CreateChatAsync(string name, List<int> participantIds)
        {
            var newChat = new Chat
            {
                Name = name,
                Type = participantIds.Count == 2 ? "direct" : "group",
                CreatedAt = DateTime.UtcNow
            };

            foreach (var userId in participantIds)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    newChat.Participants.Add(new ChatParticipant { User = user });
                }
            }

            _context.Chats.Add(newChat);
            await _context.SaveChangesAsync();
            return newChat;
        }

        public async Task<Chat?> GetChatByIdAsync(int chatId)
        {
            return await _context.Chats
                                 .Include(c => c.Participants)
                                     .ThenInclude(cp => cp.User)
                                 .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<IEnumerable<Chat>> GetAllChatsAsync()
        {
            return await _context.Chats
                                 .Include(c => c.Participants)
                                     .ThenInclude(cp => cp.User)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetUserChatsAsync(int userId)
        {
            return await _context.ChatParticipants
                                 .Where(cp => cp.UserId == userId)
                                 .Select(cp => cp.Chat)
                                 .Include(c => c.Participants)
                                     .ThenInclude(cp => cp.User)
                                 .ToListAsync();
        }

        // --- Message Methods ---
        public async Task<Message> AddMessageToChatAsync(int chatId, int senderId, string content)
        {
            var newMessage = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Content = content,
                Timestamp = DateTime.UtcNow,
                Status = "sent"
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();
            return newMessage;
        }

        public async Task<IEnumerable<Message>> GetChatMessagesAsync(int chatId, int take = 100, int skip = 0)
        {
            return await _context.Messages
                                 .Where(m => m.ChatId == chatId)
                                 .OrderBy(m => m.Timestamp)
                                 .Skip(skip)
                                 .Take(take)
                                 .Include(m => m.Sender)
                                 .ToListAsync();
        }
    }
}