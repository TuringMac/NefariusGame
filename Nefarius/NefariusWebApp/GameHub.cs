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
            var player = _table.GetPlayer(Context.ConnectionId);
            _table.Leave(player);
        }

        public void Begin()
        {
            var player = _table.GetPlayer(Context.ConnectionId);
            if (player != null)
                _table.Begin();
        }

        public void End()
        {
            var player = _table.GetPlayer(Context.ConnectionId);
            if (player != null)
                _table.End();
        }

        public void Turn(GameAction? pAction)
        {
            var action = GameAction.None;
            if (pAction.HasValue)
                action = pAction.Value;
            else
            {
                Console.WriteLine("Wrong Action from client");
                return;
            }

            var player = _table.GetPlayer(Context.ConnectionId);
            _table.Turn(player, action);
        }

        public void Spy(decimal pTo, decimal pFrom = 0)
        {
            var player = _table.GetPlayer(Context.ConnectionId);
            _table.SetSpy(player, (GameAction)pTo, (GameAction)pFrom);
        }

        public void Invent(decimal pInventID)
        {
            var player = _table.GetPlayer(Context.ConnectionId);
            if (player.Inventions.Select(inv => inv.ID == pInventID).Any())
                _table.Invent(player, player.Inventions.Single(inv => inv.ID == pInventID));
            else
                Console.WriteLine($"Player: {player.Name} tried to play invention: {pInventID} that doesn't have!");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Player p = _table.GetPlayer(Context.ConnectionId);
            if (p != null)
            {
                _table.Leave(p);
                Console.Write("Player " + p.Name + " ");
            }
            Console.WriteLine(Context.ConnectionId + " Lived");
            return base.OnDisconnectedAsync(exception);
        }
    }
}