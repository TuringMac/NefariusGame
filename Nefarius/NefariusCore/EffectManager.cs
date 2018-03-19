using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NefariusCore
{
    class EffectManager
    {
        public static void Assign(List<Player> pPlayerList)
        {
            // Self effects first
            foreach (var player in pPlayerList)
            {
                if (player.CurrentInvention == null) continue;

                foreach (var eff in player.CurrentInvention.SelfEffectList)
                    player.EffectQueue.Enqueue(eff);
            }

            // Other effects second
            foreach (var player in pPlayerList) // Эффекты по часовой стрелке
            {
                foreach (var inventor in pPlayerList) //TODO Reverse
                {
                    if (inventor.CurrentInvention == null) continue;

                    foreach (var effect in inventor.CurrentInvention.OtherEffectList)
                    {
                        player.EffectQueue.Enqueue(effect);
                    }
                }
            }

            // Clear current invention and work with queue
            foreach (var inventor in pPlayerList)
            {
                if (inventor.CurrentInvention != null)
                    inventor.CurrentInvention = null;
            }
        }

        public static bool Apply(Player pPlayer, Game pGame, bool suspendUserActions = true) //TODO refactor! mb strategy // TODO for debug
        {
            Effect eff = pPlayer.EffectQueue.Peek();
            bool success = false;

            int spyCount = pPlayer.Spies.Where(s => s != 0).Count(); // Число выставленных шпионов
            int invCount = pPlayer.Inventions.Count();
            int playedinvCount = pPlayer.PlayedInventions.Count();

            if (string.Equals(eff.direction, "get"))
            {
                if (string.Equals(eff.item, "coin"))
                {
                    if (string.Equals(eff.count, "spy"))
                    {
                        pPlayer.Coins += spyCount;
                    }
                    else if (string.Equals(eff.count, "invented"))
                    {
                        pPlayer.Coins += playedinvCount;
                    }
                    else if (string.Equals(eff.count, "inventions"))
                    {
                        pPlayer.Coins += invCount;
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(eff.count, out n))
                            throw new Exception("Bad effect count");

                        pPlayer.Coins += n;
                    }
                    success = true;
                }
                else if (string.Equals(eff.item, "spy"))
                {
                    if (suspendUserActions)
                        success = true;
                    else
                        success = false;
                }
                else if (string.Equals(eff.item, "invention"))
                {
                    if (string.Equals(eff.count, "spy"))
                    {
                        for (int i = 0; i < spyCount; i++)
                            pPlayer.Inventions.Add(pGame.InventDeck.Pop());
                    }
                    else if (string.Equals(eff.count, "invented"))
                    {
                        for (int i = 0; i < playedinvCount; i++)
                            pPlayer.Inventions.Add(pGame.InventDeck.Pop());
                    }
                    else if (string.Equals(eff.count, "inventions"))
                    {
                        for (int i = 0; i < invCount; i++)
                            pPlayer.Inventions.Add(pGame.InventDeck.Pop());
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(eff.count, out n))
                            throw new Exception("Bad effect");

                        for (int i = 0; i < n; i++)
                            pPlayer.Inventions.Add(pGame.InventDeck.Pop());
                    }
                    success = true;
                }
                else
                    throw new Exception("Wrong effect item");
            }
            else if (string.Equals(eff.direction, "drop"))
            {
                if (string.Equals(eff.item, "coin"))
                {
                    if (string.Equals(eff.count, "spy"))
                    {
                        DropCoins(pPlayer, spyCount);
                    }
                    else if (string.Equals(eff.count, "invented"))
                    {
                        DropCoins(pPlayer, playedinvCount);
                    }
                    else if (string.Equals(eff.count, "inventions"))
                    {
                        DropCoins(pPlayer, invCount);
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(eff.count, out n))
                            throw new Exception("Bad effect");

                        DropCoins(pPlayer, n);
                    }
                    success = true;
                }
                else if (string.Equals(eff.item, "spy"))
                {
                    if (spyCount == 0)
                        success = true;
                    else
                    {
                        if (suspendUserActions)
                            success = true;
                        else
                            success = false;
                    }
                }
                else if (string.Equals(eff.item, "invention"))
                {
                    if (invCount == 0)
                    {
                        success = true;
                    }
                    else
                    {
                        if (suspendUserActions)
                            success = true;
                        else
                            success = false;
                    }
                }
                else
                    throw new Exception("Wrong effect item");
            }
            else
                throw new Exception("Wrong effect direction");

            if (success)
            {
                pPlayer.EffectQueue.Dequeue();
                return true;
            }
            else
                return false;
        }

        static void DropCoins(Player pPlayer, decimal dropCount)
        {
            if (pPlayer.Coins - dropCount < 0)
                pPlayer.Coins = 0;
            else
                pPlayer.Coins -= dropCount;
        }
    }
}
