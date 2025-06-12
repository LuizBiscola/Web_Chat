using ChatAppApi.Models;
using ChatAppApi.Services;
using ChatAppApi.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppApi.Controllers
{
    [ApiController]
    // A rota base para messages deve ser dentro de um chat específico,
    // ou se for global para todas as mensagens, ela seria diferente.
    // Considerando o padrão anterior: api/chats/{chatId}/messages
    [Route("api/chats/{chatId}/messages")] // Rota para listar mensagens de um chat específico
    public class MessagesController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext; // Para enviar mensagens via SignalR

        public MessagesController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        // Endpoint para enviar uma mensagem via HTTP (persistir e notificar em tempo real)
        // POST: api/chats/{chatId}/messages
        [HttpPost] // Remova "send" da rota do método, pois já está na rota do controller
        public async Task<ActionResult<Message>> SendMessageHttp(int chatId, [FromBody] SendMessageRequest request) // Mude chatId para int
        {
            // Validação
            // request.ChatId e request.SenderId agora são int, então compare com 0 ou valide > 0
            if (request == null || request.SenderId <= 0 || string.IsNullOrWhiteSpace(request.Content))
            {
                // Se ChatId é parte da rota, ele não pode ser Guid.Empty. A validação seria no parâmetro chatId.
                return BadRequest("Dados da mensagem inválidos.");
            }

            // Você também precisa verificar se o chatId passado na rota corresponde ao request.ChatId, se ambos existirem.
            // Por enquanto, vamos usar o chatId da rota para simplificar.

            // 1. Persistir a mensagem no banco de dados
            // Certifique-se que o AddMessageToChatAsync no ChatService também aceita int
            var message = await _chatService.AddMessageToChatAsync(chatId, request.SenderId, request.Content);

            // Certifique-se de que o objeto message retornado pelo serviço tem o Sender populado
            // ou busque o nome do usuário aqui para a notificação SignalR
            var senderUser = await _chatService.GetUserByIdAsync(request.SenderId); // Ou use GetUserByIdAsync

            // 2. Notificar clientes em tempo real via SignalR (WebSocket)
            // Agora usa o ID do chat para o grupo e o nome do usuário para exibição
            if (senderUser != null)
            {
                await _hubContext.Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", senderUser.Username, message.Content);
            }
            else
            {
                // Fallback caso o usuário não seja encontrado (apenas para depuração)
                await _hubContext.Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", "Desconhecido", message.Content);
            }

            // Você pode retornar a mensagem persistida ou um status de sucesso
            return CreatedAtAction(nameof(GetMessages), new { chatId = chatId }, message); // Retorna 201 Created
        }

        // Endpoint para mostrar mensagens de um chat específico (histórico)
        // GET: api/chats/{chatId}/messages
        [HttpGet] // Remova "chat/{chatId}" da rota do método, pois já está na rota do controller
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(int chatId, [FromQuery] int take = 100, [FromQuery] int skip = 0) // Mude chatId para int
        {
            var messages = await _chatService.GetChatMessagesAsync(chatId, take, skip);

            // É uma boa prática verificar se o chat existe mesmo que não haja mensagens
            var chatExists = await _chatService.GetChatByIdAsync(chatId);
            if (chatExists == null)
            {
                return NotFound("Chat não encontrado.");
            }

            if (messages == null || !messages.Any())
            {
                return Ok(new List<Message>()); // Retorna lista vazia, não 204 No Content se o chat existe
            }
            return Ok(messages);
        }
    }

    // DTO para a requisição de envio de mensagem
    public class SendMessageRequest
    {
        // Remova ChatId daqui, pois ele virá da rota {chatId}
        public int SenderId { get; set; } // Mude para int
        public string Content { get; set; } = string.Empty; // Use inicialização nula para string
    }
}