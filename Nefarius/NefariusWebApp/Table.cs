using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NefariusWebApp
{
    public class Table //TODO Move to Core
    {
        public string TableName { get; private set; }

        public GameStateCycle Game { get; set; }

        internal Table(string pTableName)
        {
            TableName = pTableName;
            Init();
        }

        void Init()
        {
            if (Game != null)
                Game.StateChanged -= _Game_StateChanged;
            Game = new GameStateCycle();
            Game.StateChanged += _Game_StateChanged;
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
                    Game.Spying();
                    break;
                case GameState.Spy:
                    BroadcastGame();
                    if (Game.CheckEverybodyDoSpy())
                        Game.State++;
                    break;
                case GameState.Invent:
                    BroadcastGame();
                    if (Game.CheckEverybodyDoInvent())
                        Game.State++;
                    break;
                case GameState.Inventing:
                    BroadcastGame();
                    Game.Inventing();
                    break;
                case GameState.Research:
                    BroadcastGame();
                    Game.Researching();
                    break;
                case GameState.Work:
                    BroadcastGame();
                    Game.Working();
                    break;
                case GameState.Scoring:
                    BroadcastGame();
                    Game.Scoring();
                    break;
                case GameState.Win:
                    BroadcastGame();
                    break;
            }
        }

        public IHubCallerClients Clients
        {
            get;
            set;
        }

        public void Join(Player pPlayer)
        {
            Game.AddPlayer(pPlayer);
            BroadcastGame();
        }

        public void Begin()
        {
            Game.StartGame();
        }

        public void End()
        {
            Init();
            BroadcastGame();
        }

        public bool Turn(Player pPlayer, GameAction pAction)
        {
            var result = Game.Turning(pPlayer, pAction);
            BroadcastGame();
            return result;
        }

        public bool SetSpy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            var result = Game.SetSpy(pPlayer, pDestSpyPosition, pSourceSpyPosition);
            BroadcastGame();
            return result;
        }

        public bool Invent(Player pPlayer, Invention pInvention)
        {
            var result = Game.Invent(pPlayer, pInvention);
            BroadcastGame();
            return result;
        }

        void BroadcastGame()
        {
            Clients.All.SendAsync("StateChanged", new { players = Game.PlayerList.Select(p => p.GetPlayerShort(Game.State > GameState.Turning)), state = Game.State, move = Game.Move });
            foreach (var player in Game.PlayerList)
            {
                Clients.Client(player.ID).SendAsync("PlayerData", player); //TODO may be exception if player disconnected
            }
        }
    }
}
