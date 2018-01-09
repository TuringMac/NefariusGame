using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusWebApp
{
    public class GameHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.InvokeAsync("Send", $"{Context.ConnectionId} joined");
        }

        public async Task Send(string message)
        {
            await Clients.All.InvokeAsync("Send", message);
        }
    }
}
