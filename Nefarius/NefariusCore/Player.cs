using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NefariusCore
{
    public class Player
    {
        #region Public

        public string ID { get; set; } //TODO Model should not know about signalr ids
        public PlayerColor Color { get; set; }
        public string Name { get; set; } = "";
        public decimal Coins { get; set; } = 0;
        public GameAction[] Spies { get; protected set; }
        public decimal InventionCount { get { return Inventions.Count; } }
        public decimal Score { get { return PlayedInventions.Sum(inv => inv.Score); } }
        public bool IsMoved { get { return Action != GameAction.None; } }
        public ICollection<Invention> PlayedInventions { get; protected set; } = new List<Invention>();
        public Queue<Effect> EffectQueue { get; protected set; } = new Queue<Effect>();
        public decimal InventionToDropCount { get; set; } = 0;
        public decimal SpyToDropCount { get; set; } = 0;
        public decimal SpyToSetCount { get; set; } = 0;

        #endregion Public

        #region Private

        public ICollection<Invention> Inventions { get; set; } = new List<Invention>(); // TODO internal + dataContract
        public GameAction Action { get; set; } // TODO internal + dataContract
        public Invention CurrentInvention { get; set; } // TODO internal + dataContract

        #endregion Private

        public Player(string pName)
        {
            Name = pName;
            Spies = new GameAction[] { GameAction.None, GameAction.None, GameAction.None, GameAction.None, GameAction.None };
        }

        public dynamic GetPlayerShort(bool pIsOpen = false)
        {
            return new
            {
                ID,
                Color,
                Name,
                Coins,
                Spies,
                Score,
                InventionCount,
                PlayedInventions,
                EffectQueue,
                IsMoved,
                Action = pIsOpen ? Action : GameAction.None,
                InventionToDropCount,
                SpyToDropCount,
                SpyToSetCount
            };
        }
    }
}
