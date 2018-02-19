using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusWebApp
{
    public class GameTicker
    {
        private readonly static Lazy<GameTicker> _instance = new Lazy<GameTicker>(() => new GameTicker());

        GameStateCycle _Game;

        public GameStateCycle Game { get { return _Game; } }

        private GameTicker()
        {
            _Game = GameStateCycle.Instance;
            _Game.StateChanged += _Game_StateChanged;
        }

        private void _Game_StateChanged(object sender, StateEventArgs e)
        {
            switch (e.State)
            {
                case GameState.Turning:
                    Clients.All.InvokeAsync("StateChanged", "Господа, делайте ход"); //TODO передавать состояние, а не сообщение
                    break;
                case GameState.Spying:
                    Clients.All.InvokeAsync("StateChanged", "Расчитываем доход от шпионства");
                    _Game.Spying();
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

        public static GameTicker Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public IHubClients Clients
        {
            get;
            set;
        }

        public IEnumerable<Player> GetAllStocks()
        {
            throw new NotImplementedException();
        }

        private void BroadcastState(Player stock)
        {
            throw new NotImplementedException();
        }
    }
}
