using FoundryBlazor.Extensions;
using FoundryBlazor.Message;

using Microsoft.AspNetCore.SignalR;

namespace Visio2023Foundry.Server
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task<string> Ping(string payload)
        {
            //"Sending a ping message to the server".WriteInfo();
            var data = $"DrawingHub: pong message {payload}";
            await Clients.All.SendAsync("Pong", data);
            return data;
        }

        public async Task Create(D2D_Create message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }

        public async Task Move(D2D_Move message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }

        public async Task Destroy(D2D_Destroy message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }

        public async Task Glue(D2D_Glue message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }
        public async Task Unglue(D2D_Unglue message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }
        public async Task ModelCreate(D2D_ModelCreate message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }
        public async Task ModelUpdate(D2D_ModelUpdate message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }
        public async Task ModelDestroy(D2D_ModelDestroy message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }
        public async Task UserMove(D2D_UserMove message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }
        public async Task UserToast(D2D_UserToast message)
        {
            await Clients.All.SendAsync(message.Topic(), message);
        }
    }
}
