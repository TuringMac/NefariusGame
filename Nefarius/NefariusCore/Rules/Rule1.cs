using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NefariusCore.Rules
{
    /// <summary>
    /// Grants
    /// </summary>
    public class Rule1 : Game
    {
        public Rule1(List<Player> pPlayers) :
            base(pPlayers)
        {

        }

        /// <summary>
        /// Add 2 coint to players without invented inventions
        /// </summary>
        /// <returns></returns>
        protected override bool MoveBeginning()
        {
            foreach (var player in PlayerList)
            {
                if (player.PlayedInventions.Count == 0)
                    player.Coins += 2;
            }
            return base.MoveBeginning();
        }
    }
}
