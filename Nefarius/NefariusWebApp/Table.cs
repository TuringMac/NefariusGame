using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusWebApp
{
    public class Table
    {
        private readonly static Lazy<Table> _instance = new Lazy<Table>(() => new Table());

        GameStateCycle _Game;

        public GameStateCycle Game { get { return _Game; } }

        private Table()
        {
            Init();
        }

        void Init()
        {
            if (_Game != null)
                _Game.StateChanged -= _Game_StateChanged;
            _Game = new GameStateCycle();
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
                    if (_Game.CheckEverybodyDoSpy())
                        _Game.State++;
                    break;
                case GameState.Invent:
                    BroadcastGame();
                    if (_Game.CheckEverybodyDoInvent())
                        _Game.State++;
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

        public static Table Instance
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

        public void Begin()
        {
            _Game.StartGame();
        }

        public void End()
        {
            Init();
            BroadcastGame();
        }

        public void BroadcastGame()
        {
            Clients.All.InvokeAsync("StateChanged", new { players = _Game.PlayerList.Select(p => p.GetPlayerShort(_Game.State > GameState.Turning)), state = _Game.State, move = _Game.Move });
            foreach (var player in _Game.PlayerList)
            {
                Clients.Client(player.ID).InvokeAsync("PlayerData", player); //TODO may be exception if player disconnected
            }
        }
    }
}
