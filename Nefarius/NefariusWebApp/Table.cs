﻿using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NefariusWebApp
{
    public class Table
    {
        public string TableName { get; private set; }
        public List<Player> PlayerList { get; private set; } = new List<Player>();

        public Game Game { get; set; }

        public IHubCallerClients Clients { get; set; }

        internal Table(string pTableName)
        {
            TableName = pTableName;
        }

        public void Join(Player pPlayer)
        {
            if (Game == null)
                PlayerList.Add(pPlayer);
            BroadcastTable();
        }

        public void Leave(Player pPlayer)
        {
            PlayerList.Remove(pPlayer);
            BroadcastTable();
        }

        public void Begin()
        {
            Game = new Game(PlayerList);
            Game.PropertyChanged += Game_PropertyChanged;
            Game.Start();
            BroadcastGame();
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                BroadcastGame();
            }
        }

        public void End()
        {
            if (Game != null)
            {
                Game.Stop();
                Game = null;
            }
            BroadcastTable();
        }

        public bool Turn(Player pPlayer, GameAction pAction)
        {
            var result = Game.Turn(pPlayer, pAction);
            BroadcastGame();
            return result;
        }

        public bool SetSpy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            var result = Game.Spy(pPlayer, pDestSpyPosition, pSourceSpyPosition);
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
            Clients.All.SendAsync("StateChanged", new { players = PlayerList.Select(p => p.GetPlayerShort(Game.State > GameState.Turn)), state = Game.State, move = Game.Move });
            foreach (var player in PlayerList)
            {
                Clients.Client(player.ID).SendAsync("PlayerData", player); //TODO may be exception if player disconnected
            }
        }

        void BroadcastTable()
        {
            Clients.All.SendAsync("StateChanged", new { players = PlayerList.Select(p => p.GetPlayerShort(false)), state = 0, move = 0 });
            foreach (var player in PlayerList) //TODO remove! here for backward compability
            {
                Clients.Client(player.ID).SendAsync("PlayerData", player); //TODO may be exception if player disconnected
            }
        }
    }
}
