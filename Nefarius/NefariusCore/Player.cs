using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    public class Player
    {
        public string Name { get; set; } = "";
        public decimal Coins { get; set; } = 10;
        public IEnumerable<Invention> Inventions { get; protected set; }
        public GameAction[] Spies { get; protected set; }
        public GameAction Action { get; set; }
        public IEnumerable<Invention> PlayedInventions { get; protected set; }

        public Player(string pName)
        {
            Name = pName;
            Inventions = new List<Invention>();
            PlayedInventions = new List<Invention>();
            Spies = new GameAction[] { GameAction.None, GameAction.None, GameAction.None, GameAction.None, GameAction.None };
        }
    }
}
