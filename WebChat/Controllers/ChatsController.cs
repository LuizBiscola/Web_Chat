using ChatAppApi.Models;
using ChatAppApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Rota base: /api/chats
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Endpoint para criar um novo chat
        // POST: api/chats
        [HttpPost]
        public async Task<ActionResult<Chat>> CreateChat([FromBody] CreateChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || request.ParticipantUserIds == null || request.ParticipantUserIds.Count == 0)
            {
                return BadRequest("Nome do chat e IDs dos participantes são obrigatórios.");
            }

            var chat = await _chatService.CreateChatAsync(request.Name, request.ParticipantUserIds);
            return CreatedAtAction(nameof(GetChat), new { id = chat.Id }, chat);
        }

        // Endpoint para mostrar um chat específico por ID
        // GET: api/chats/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Chat>> GetChat(int id) // Mudado para int
        {
            var chat = await _chatService.GetChatByIdAsync(id);
            if (chat == null)
            {
                return NotFound();
            }
            return Ok(chat);
        }

        // Endpoint para listar os chats de um usuário
        // GET: api/chats/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Chat>>> GetUserChats(int userId) // Mudado para int
        {
            var chats = await _chatService.GetUserChatsAsync(userId);
            return Ok(chats);
        }
    }

    // DTO (Data Transfer Object) para a requisição de criação de chat
    public class CreateChatRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<int> ParticipantUserIds { get; set; } = new List<int>(); // Mudado para int
    }
}