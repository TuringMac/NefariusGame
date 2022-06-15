using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NefariusCore.Rules
{
    public class Rule2 : Game
    {
        Dictionary<Player, GameAction> PreviousMove = new Dictionary<Player, GameAction>();
        public Rule2(List<Player> pPlayers) :
            base(pPlayers)
        {

        }

        /// <summary>
        /// Deny same action as in previous move
        /// </summary>
        /// <param name="pPlayer"></param>
        /// <param name="pAction"></param>
        /// <returns></returns>
        public override bool Turn(Player pPlayer, GameAction pAction)
        {
            if (PreviousMove.ContainsKey(pPlayer) && PreviousMove[pPlayer] == pAction)
                return false;
            return base.Turn(pPlayer, pAction);
        }

        protected override bool MoveEnding()
        {
            foreach (Player player in PlayerList)
                PreviousMove[player] = player.Action;
            return base.MoveEnding();
        }
    }
}
