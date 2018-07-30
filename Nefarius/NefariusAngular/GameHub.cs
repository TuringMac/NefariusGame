using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusAngular
{
    public class GameHub : Hub
    {
        Table Table
        {
            get
            {
                foreach (var table in TableManager.GetTableList())
                {
                    foreach (var player in table.PlayerList)
                    {
                        if (player.ID == Context.ConnectionId)
                            return table;
                    }
                }
                return null;
            }
        }

        public GameHub()
        {

        }

        public void GetTableList()
        {
            // TODO create router. to check TableListChange and broadcast. layer over Table class
            Clients.All.SendAsync("TableList", new
            {
                tableList = TableManager.GetTableList()
            });
        }

        public void Join(string pTableName, string pName)
        {
            Player player = Table?.GetPlayer(Context.ConnectionId);
            if (player == null)
            {
                player = new Player(pName)
                {
                    ID = Context.ConnectionId
                };
            }
            var table = TableManager.GetTable(pTableName);
            if (Clients != null) table.Clients = Clients;
            table.Join(player);

            GetTableList();
        }

        public void Leave()
        {
            var player = Table.GetPlayer(Context.ConnectionId);
            Table.Leave(player);
        }

        public void Begin()
        {
            var player = Table.GetPlayer(Context.ConnectionId);
            if (player != null)
                Table.Begin();
        }

        public void End()
        {
            var player = Table.GetPlayer(Context.ConnectionId);
            if (player != null)
                Table.End();
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

            var player = Table.GetPlayer(Context.ConnectionId);
            Table.Turn(player, action);
        }

        public void Spy(decimal pTo, decimal pFrom = 0)
        {
            var player = Table.GetPlayer(Context.ConnectionId);
            Table.SetSpy(player, (GameAction)pTo, (GameAction)pFrom);
        }

        public void Invent(decimal pInventID)
        {
            var player = Table.GetPlayer(Context.ConnectionId);
            if (player.Inventions.Select(inv => inv.ID == pInventID).Any())
                Table.Invent(player, player.Inventions.Single(inv => inv.ID == pInventID));
            else
                Console.WriteLine($"Player: {player.Name} tried to play invention: {pInventID} that doesn't have!");
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (Table != null)
            {
                Player p = Table.GetPlayer(Context.ConnectionId);
                if (p != null)
                {
                    Table.Leave(p);
                    Console.Write("Player " + p.Name + " ");
                }
            }
            Console.WriteLine(Context.ConnectionId + " Lived");
            return base.OnDisconnectedAsync(exception);
        }
    }
}