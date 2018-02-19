using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NefariusCore
{
    public class Player
    {
        #region Public

        public string Name { get; set; } = "";
        public decimal Coins { get; set; } = 10;
        public GameAction[] Spies { get; protected set; }
        public decimal InventionCount { get { return Inventions.Count; } }
        public ICollection<Invention> PlayedInventions { get; protected set; }

        #endregion Public

        #region Private
        internal ICollection<Invention> Inventions { get; set; }
        internal GameAction Action { get; set; }
        public object ID { get; set; } //TODO Model should not know about signalr ids

        #endregion Private

        public Player(string pName)
        {
            Name = pName;
            Inventions = new List<Invention>();
            PlayedInventions = new List<Invention>();
            Spies = new GameAction[] { GameAction.None, GameAction.None, GameAction.None, GameAction.None, GameAction.None };
        }

        public Player Turn()
        {
            int delay = new Random().Next(500, 1000);
            Thread.Sleep(delay);
            Debug.WriteLine("Player: " + Name + " turned after: " + delay);
            return this;
        }
    }
}
