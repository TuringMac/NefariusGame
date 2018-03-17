using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusWebApp
{
    public class GameHub : Hub
    {
        private readonly Table _table;

        public GameHub() : this(Table.Instance) { }

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
            _table.Game.AddPlayer(player);
            if (Clients != null) _table.Clients = Clients;
            _table.BroadcastGame();
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

            var action = GameAction.None;
            switch (userAction)
            {
                case 0: action = GameAction.None; break;
                case 1: action = GameAction.Spy; break;
                case 2: action = GameAction.Invent; break;
                case 3: action = GameAction.Research; break;
                case 4: action = GameAction.Work; break;
                default: throw new Exception("Wrong action");
            }
            var player = GetPlayer(Context.ConnectionId);
            _table.Game.Turning(player, action);
            _table.BroadcastGame();
        }

        public void Spy(decimal pTo, decimal pFrom = 0)
        {
            var player = GetPlayer(Context.ConnectionId);
            if (player.SpyToDropCount > 0)
            {
                if (pTo != 0 || pFrom == 0) // Если шпион переставляется из нуля или на действие, то это ошибка
                {
                    Debug.WriteLine("Нужно отозвать шпиона, а не переставить");
                    return;
                }
                else if (_table.Game.SetSpy(player, GameAction.None, (GameAction)pFrom))
                {
                    player.SpyToDropCount--;
                    if (_table.Game.CheckEverybodyApplyEffects())
                        _table.Game.State++;
                }
            }
            else
            {
                if (_table.Game.SetSpy(player, (GameAction)pTo, (GameAction)pFrom))
                    player.Action = GameAction.None;
            }
            _table.BroadcastGame();
            if (_table.Game.CheckEverybodyDoSpy() && _table.Game.State == GameState.Spy)
            {
                _table.Game.State++;
            }
        }

        public void Invent(decimal pInventID)
        {
            var player = GetPlayer(Context.ConnectionId);
            if (_table.Game.Invent(player, player.Inventions.Single(inv => inv.ID == pInventID)))
                player.Action = GameAction.None;

            _table.BroadcastGame();
            if (_table.Game.CheckEverybodyDoInvent() && _table.Game.State == GameState.Invent)
                _table.Game.State++;
        }

        public void DropInvention(decimal InventionID)
        {

        }

        Player GetPlayer(string id)
        {
            foreach (var player in _table.Game.PlayerList)
            {
                if (string.Equals(player.ID, id))
                    return player;
            }
            return null;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_table.Game.PlayerList.Where(p => p.ID == Context.ConnectionId).Any())
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