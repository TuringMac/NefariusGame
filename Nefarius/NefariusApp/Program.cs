using NefariusCore;
using System;

namespace NefariusApp
{
    class Program
    {
        static Game game;
        static void Main(string[] args)
        {
            Player[] players = new Player[6] {
                new Player(), // 0
                null,         // 1
                new Player(), // 2
                null,         // 3
                new Player(), // 4
                new Player()  // 5
            };

            game = new Game(players);
            game.ChooseAction(players[0], GameAction.Research);
            game.ChooseAction(players[2], GameAction.Spy);
            game.ChooseAction(players[4], GameAction.Work);
            game.ChooseAction(players[5], GameAction.Research);
            // Сервак начинает игру т.к. сделали выбор

        }
    }
}
