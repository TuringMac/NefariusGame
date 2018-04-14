using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusWebApp
{
    public class GameHub : Hub
    {
        private readonly Table _table;

        public GameHub() : this(TableManager.GetTable("tbl1")) { }

        GameHub(Table pGameTicker)
        {
            _table = pGameTicker;
        }

        public void Join(string pName)
        {
            var player = new Player(pName)
            {
                ID = Context.ConnectionId
            };
            if (Clients != null) _table.Clients = Clients;
            _table.Join(player);
        }

        public void Leave()
        {
            var player = GetPlayer(Context.ConnectionId);
            _table.Leave(player);
        }

        public void Begin()
        {
            var player = GetPlayer(Context.ConnectionId);
            if (player != null)
                _table.Begin();
        }

        public void End()
        {
            var player = GetPlayer(Context.ConnectionId);
            if (player != null)
                _table.End();
        }

        public void Turn(decimal? pAction) //TODO use Nullable<GameAction>
        {
            decimal userAction = 0;
            if (pAction.HasValue)
                userAction = pAction.Value;
            else
            {
                Debug.WriteLine("Wrong Action from client");
                return;
            }

            var action = (GameAction)userAction;
            var player = GetPlayer(Context.ConnectionId);
            _table.Turn(player, action);
        }

        public void Spy(decimal pTo, decimal pFrom = 0)
        {
            var player = GetPlayer(Context.ConnectionId);
            _table.SetSpy(player, (GameAction)pTo, (GameAction)pFrom);
        }

        public void Invent(decimal pInventID)
        {
            var player = GetPlayer(Context.ConnectionId);
            _table.Invent(player, player.Inventions.Single(inv => inv.ID == pInventID));
        }

        Player GetPlayer(string id)
        {
            foreach (var player in _table.PlayerList)
            {
                if (string.Equals(player.ID, id))
                    return player;
            }
            return null;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_table.PlayerList.Where(p => p.ID == Context.ConnectionId).Any())
            {
                Player p = GetPlayer(Context.ConnectionId);
                Debug.Write("Player " + p.Name + " ");
            }
            Debug.WriteLine(Context.ConnectionId + " Lived");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
// await Clients.All.InvokeAsync("Send", message);
// Clients.Client(Context.ConnectionId).addContosoChatMessageToPage(name, message);