using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusWebApp
{
    public class GameHub : Hub
    {
        private readonly GameTicker _gameTicker;

        public GameHub() : this(GameTicker.Instance) { }

        GameHub(GameTicker pGameTicker)
        {
            _gameTicker = pGameTicker;
        }

        public async Task Join(string pName)
        {
            var player = new Player(pName)
            {
                ID = Context.ConnectionId
            };
            _gameTicker.Game.AddPlayer(player);
            if (Clients != null) _gameTicker.Clients = Clients;
            await Clients.All.InvokeAsync("PlayerJoined", player.Name);
        }

        public void Begin()
        {
            _gameTicker.Game.StartGame();
        }

        public void Turn(decimal pAction)
        {
            var action = GameAction.None;
            switch (pAction)
            {
                case 0: action = GameAction.None; break;
                case 1: action = GameAction.Spy; break;
                case 2: action = GameAction.Invent; break;
                case 3: action = GameAction.Research; break;
                case 4: action = GameAction.Work; break;
                default: throw new Exception("Wrong action");
            }
            var player = GetPlayer(Context.ConnectionId);
            _gameTicker.Game.Turning(player, action);
        }

        Player GetPlayer(string id)
        {
            foreach (var player in _gameTicker.Game.PlayerList)
            {
                if (string.Equals(player.ID, id))
                    return player;
            }
            throw new Exception("Player not found");
        }
    }
}
// await Clients.All.InvokeAsync("Send", message);
// Clients.Client(Context.ConnectionId).addContosoChatMessageToPage(name, message);