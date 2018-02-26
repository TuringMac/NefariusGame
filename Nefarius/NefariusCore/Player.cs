using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NefariusCore
{
    [DataContract]
    public class Player
    {
        #region Public

        public string ID { get; set; } //TODO Model should not know about signalr ids
        public PlayerColor Color { get; set; }
        public string Name { get; set; } = "";
        public decimal Coins { get; set; } = 10;
        public GameAction[] Spies { get; protected set; }
        public decimal InventionCount { get { return Inventions.Count; } }
        public ICollection<Invention> PlayedInventions { get; protected set; }
        public Queue<Effect> EffectQueue { get; protected set; }

        #endregion Public

        #region Private

        internal ICollection<Invention> Inventions { get; set; }
        internal GameAction Action { get; set; }
        internal Invention CurrentInvention { get; set; }

        #endregion Private

        public Player(string pName)
        {
            Name = pName;
            Inventions = new List<Invention>();
            PlayedInventions = new List<Invention>();
            Spies = new GameAction[] { GameAction.None, GameAction.None, GameAction.None, GameAction.None, GameAction.None };
        }

        public dynamic GetPlayerShort()
        {
            return new
            {
                ID,
                Color,
                Name,
                Coins,
                Spies,
                InventionCount,
                PlayedInventions,
                EffectQueue
            };
        }
    }
}
