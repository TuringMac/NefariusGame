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
        GameStateCycle _Game = null;

        public GameHub()
        {
            _Game = GameStateCycle.GetInstance();
            _Game.StateChanged += Game_StateChanged;
        }

        private void Game_StateChanged(object sender, StateEventArgs e)
        {
            switch (e.State)
            {
                case GameState.Turning:
                    Clients.All.InvokeAsync("StateChanged", "Господа, делайте ход"); //TODO передавать состояние, а не сообщение
                    break;
                case GameState.Spying:
                    _Game.Spying();
                    Clients.All.InvokeAsync("StateChanged", "Расчитываем доход от шпионства");
                    break;
                case GameState.Spy:
                    Clients.All.InvokeAsync("StateChanged", "Господа, выставляйте шпионов"); //TODO разные сообщения в зависимости от хода пользователя
                    break;
                case GameState.Invent:
                    Clients.All.InvokeAsync("StateChanged", "Господа, показывайте изобретения"); //TODO разные сообщения в зависимости от хода пользователя
                    break;
                case GameState.Research:
                    _Game.Researching();
                    Clients.All.InvokeAsync("StateChanged", "Раздаем изобретения и монеты");
                    break;
                case GameState.Work:
                    _Game.Working();
                    Clients.All.InvokeAsync("StateChanged", "Выдаем зарплату");
                    break;
                case GameState.Win:
                    Clients.All.InvokeAsync("StateChanged", "Господа, у нас Победитель!");
                    break;
            }
        }

        public async Task Join(string pName)
        {
            var player = new Player(pName)
            {
                ID = Context.ConnectionId
            };
            _Game.AddPlayer(player);
            await Clients.All.InvokeAsync("PlayerJoined", player.Name);
        }

        public void Begin()
        {
            _Game.StartGame();
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
            _Game.Turning(player, action);
        }

        Player GetPlayer(string id)
        {
            foreach (var player in _Game.PlayerList)
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