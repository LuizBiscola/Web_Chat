using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatAppApi.Hubs
{
    public class ChatHub : Hub
    {
        // Este método será chamado pelo cliente para enviar uma mensagem
        // A mensagem será então enviada para todos os clientes conectados
        public async Task SendMessage(string user, string message)
        {
            // Clients.All.SendAsync envia a mensagem para todos os clientes conectados
            // O nome "ReceiveMessage" deve ser o mesmo que o cliente vai "ouvir"
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // Você pode adicionar mais métodos aqui para diferentes funcionalidades:
        // public async Task SendPrivateMessage(string recipientConnectionId, string user, string message)
        // {
        //     await Clients.Client(recipientConnectionId).SendAsync("ReceivePrivateMessage", user, message);
        // }

        // Sobrescritas para eventos de conexão/desconexão (opcional, mas útil para depuração)
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}