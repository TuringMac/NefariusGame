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
        private readonly GameTicker _gameTicker;

        public GameHub() : this(GameTicker.Instance) { }

        GameHub(GameTicker pGameTicker)
        {
            _gameTicker = pGameTicker;
        }

        public void Join(string pName)
        {
            var player = new Player(pName)
            {
                ID = Context.ConnectionId
            };
            _gameTicker.Game.AddPlayer(player);
            if (Clients != null) _gameTicker.Clients = Clients;
            _gameTicker.BroadcastGame();
        }

        public void Begin()
        {
            _gameTicker.Game.StartGame();
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
            _gameTicker.Game.Turning(player, action);
            Clients.Client(Context.ConnectionId).InvokeAsync("PlayerData", player);
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
                else if (_gameTicker.Game.SetSpy(player, GameAction.None, (GameAction)pFrom))
                {
                    player.SpyToDropCount--;
                    if (_gameTicker.Game.CheckEverybodyApplyEffects())
                        _gameTicker.Game.State++;
                }
            }
            else
            {
                if (_gameTicker.Game.SetSpy(player, (GameAction)pTo, (GameAction)pFrom))
                    player.Action = GameAction.None;
            }
            Clients.Client(Context.ConnectionId).InvokeAsync("PlayerData", player);
            if (_gameTicker.Game.CheckEverybodyDoSpy())
            {
                _gameTicker.Game.State++;
            }
        }

        public void Invent(decimal pInventID)
        {
            var player = GetPlayer(Context.ConnectionId);
            if (_gameTicker.Game.Invent(player, player.Inventions.Single(inv => inv.ID == pInventID)))
                player.Action = GameAction.None;

            Clients.Client(Context.ConnectionId).InvokeAsync("PlayerData", player);
            if (_gameTicker.Game.CheckEverybodyDoInvent())
                _gameTicker.Game.State++;
        }

        public void DropInvention(decimal InventionID)
        {

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