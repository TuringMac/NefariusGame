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

        [DataMember]
        public string ID { get; set; } //TODO Model should not know about signalr ids
        [DataMember]
        public string Name { get; set; } = "";
        [DataMember]
        public decimal Coins { get; set; } = 10;
        [DataMember]
        public GameAction[] Spies { get; protected set; }
        [DataMember]
        public decimal InventionCount { get { return Inventions.Count; } }
        [DataMember]
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
                Name,
                Coins,
                Spies,
                InventionCount,
                PlayedInventions,
                EffectQueue
            };
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
