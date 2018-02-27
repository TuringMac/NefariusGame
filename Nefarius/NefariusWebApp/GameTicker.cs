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
                    BroadcastGame();
                    break;
                case GameState.Spying:
                    BroadcastGame();
                    _Game.Spying();
                    break;
                case GameState.Spy:
                    BroadcastGame();
                    break;
                case GameState.Invent:
                    BroadcastGame();
                    break;
                case GameState.Inventing:
                    BroadcastGame();
                    _Game.Inventing();
                    break;
                case GameState.Research:
                    BroadcastGame();
                    _Game.Researching();
                    break;
                case GameState.Work:
                    BroadcastGame();
                    _Game.Working();
                    break;
                case GameState.Scoring:
                    BroadcastGame();
                    _Game.Scoring();
                    break;
                case GameState.Win:
                    BroadcastGame();
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

        public void BroadcastGame()
        {
            Clients.All.InvokeAsync("StateChanged", new { players = _Game.PlayerList.Select(player => player.GetPlayerShort()), state = _Game.State });
            foreach (var player in _Game.PlayerList)
            {
                Clients.Client(player.ID).InvokeAsync("PlayerData", player); //TODO may be exception if player disconnected
            }
        }
    }
}
