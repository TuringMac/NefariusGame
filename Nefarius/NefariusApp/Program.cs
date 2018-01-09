﻿using NefariusCore;
using System;
using System.Collections.Generic;

namespace NefariusApp
{
    class Program
    {
        static Game game;
        static void Main(string[] args)
        {
            LinkedList<Player> players = new LinkedList<Player>();
            players.AddLast(new Player("Игорь"));
            players.AddLast(new Player("Димас"));
            players.AddLast(new Player("Серый"));
            players.AddLast(new Player("Дыча"));

            game = new Game(players);
            game.StateChanged += Game_StateChanged;
            game.Run();
            // Сервак начинает игру т.к. сделали выбор

        }

        private static void Game_StateChanged(object sender, StateEventArgs e)
        {
            //Clients.All.broadcastState(e.State);
            switch (e.State)
            {
                case GameState.Spying: game.Spying(); break;
                case GameState.Research: game.Researching(); break;
                case GameState.Work: game.Working(); break;
            }
        }
    }
}
