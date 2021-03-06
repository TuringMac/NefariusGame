﻿using Microsoft.AspNetCore.SignalR;
using NefariusCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NefariusWebApp
{
    public class Table
    {
        public string Name { get; private set; }
        public List<Player> PlayerList { get; private set; } = new List<Player>();

        protected Game Game { get; set; }

        public IHubCallerClients Clients { get; set; }

        internal Table(string pTableName)
        {
            Name = pTableName;
        }

        public void Join(Player pPlayer)
        {
            if (Game == null)
                PlayerList.Add(pPlayer);
            BroadcastGame();
        }

        public void Leave(Player pPlayer)
        {
            PlayerList.Remove(pPlayer);
            BroadcastGame();
        }

        public void Begin()
        {
            End();
            Game = new Game(PlayerList);
            Game.PropertyChanged += Game_PropertyChanged;
            Game.EffectQueueChanged += Game_EffectQueueChanged;
            Game.Start();
            BroadcastGame();
        }

        private void Game_EffectQueueChanged()
        {
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
                Game.PropertyChanged -= Game_PropertyChanged;
                Game.EffectQueueChanged -= Game_EffectQueueChanged;
                Game = null;
                PlayerList.Clear();
            }
            BroadcastGame();
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

        public Player GetPlayer(string id)
        {
            foreach (var player in PlayerList)
            {
                if (string.Equals(player.ID, id))
                    return player;
            }
            return null;
        }

        void BroadcastGame()
        {
            var opponents = new List<Player>();
            lock (PlayerList)
            {
                opponents = new List<Player>(PlayerList);
            }
            foreach (var player in opponents)
            {
                // Выводим список так, чтобы левый и правый оппонент были слева и справа
                var opponents_left = opponents.Take(opponents.IndexOf(player));
                var opponents_right = opponents.TakeLast(opponents.Count - opponents.IndexOf(player) - 1);
                var players = opponents_right.Concat(opponents_left).Select(p => p.GetPlayerShort(Game?.State > GameState.Turn));

                Clients.Client(player.ID).SendAsync("StateChanged", new { players, state = Game?.State, move = Game?.Move, table = Name });

                Clients.Client(player.ID).SendAsync("PlayerData", player); //TODO may be exception if player disconnected
            }
        }
    }
}
