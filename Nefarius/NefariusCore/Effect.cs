using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class Effect
    {
        [DataMember]
        public string direction { get; set; } // get/drop
        [DataMember]
        public string item { get; set; } // coin/spy/invention
        [DataMember]
        public string count { get; set; } // fixed("1","2","3")/"spy"/"invented"/"inventions"

        bool suspendUserActions = true;

        public void Apply(Player pPlayer) //TODO refactor! mb strategy
        {
            int spyCount = pPlayer.Spies.Where(s => s != 0).Count(); // Число выставленных шпионов
            int invCount = pPlayer.Inventions.Count();
            int playedinvCount = pPlayer.PlayedInventions.Count();

            if (string.Equals(direction, "get"))
            {
                if (string.Equals(item, "coin"))
                {
                    if (string.Equals(count, "spy"))
                    {
                        pPlayer.Coins += spyCount;
                    }
                    else if (string.Equals(count, "invented"))
                    {
                        pPlayer.Coins += playedinvCount;
                    }
                    else if (string.Equals(count, "inventions"))
                    {
                        pPlayer.Coins += invCount;
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(count, out n))
                            throw new Exception("Bad effect count");

                        pPlayer.Coins += n;
                    }
                }
                else if (string.Equals(item, "spy"))
                {
                    if (string.Equals(count, "spy"))
                    {
                        SetSpy(pPlayer, spyCount);
                    }
                    else if (string.Equals(count, "invented"))
                    {
                        SetSpy(pPlayer, playedinvCount);
                    }
                    else if (string.Equals(count, "inventions"))
                    {
                        SetSpy(pPlayer, invCount);
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(count, out n))
                            throw new Exception("Bad effect");

                        SetSpy(pPlayer, n);
                    }
                }
                else if (string.Equals(item, "invention"))
                {
                    if (string.Equals(count, "spy"))
                    {
                        for (int i = 0; i < spyCount; i++)
                            pPlayer.Inventions.Add(GameStateCycle.Instance.InventDeck.Pop());
                    }
                    else if (string.Equals(count, "invented"))
                    {
                        for (int i = 0; i < playedinvCount; i++)
                            pPlayer.Inventions.Add(GameStateCycle.Instance.InventDeck.Pop());
                    }
                    else if (string.Equals(count, "inventions"))
                    {
                        for (int i = 0; i < invCount; i++)
                            pPlayer.Inventions.Add(GameStateCycle.Instance.InventDeck.Pop());
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(count, out n))
                            throw new Exception("Bad effect");

                        for (int i = 0; i < n; i++)
                            pPlayer.Inventions.Add(GameStateCycle.Instance.InventDeck.Pop());
                    }
                }
                else
                    throw new Exception("Wrong effect item");
            }
            else if (string.Equals(direction, "drop"))
            {
                if (string.Equals(item, "coin"))
                {
                    if (string.Equals(count, "spy"))
                    {
                        DropCoins(pPlayer, spyCount);
                    }
                    else if (string.Equals(count, "invented"))
                    {
                        DropCoins(pPlayer, playedinvCount);
                    }
                    else if (string.Equals(count, "inventions"))
                    {
                        DropCoins(pPlayer, invCount);
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(count, out n))
                            throw new Exception("Bad effect");

                        DropCoins(pPlayer, n);
                    }
                }
                else if (string.Equals(item, "spy"))
                {
                    if (string.Equals(count, "spy"))
                    {
                        DropSpy(pPlayer, spyCount);
                    }
                    else if (string.Equals(count, "invented"))
                    {
                        DropSpy(pPlayer, playedinvCount);
                    }
                    else if (string.Equals(count, "inventions"))
                    {
                        DropSpy(pPlayer, invCount);
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(count, out n))
                            throw new Exception("Bad effect");

                        DropSpy(pPlayer, n);
                    }
                }
                else if (string.Equals(item, "invention"))
                {
                    if (string.Equals(count, "spy"))
                    {
                        DropInvention(pPlayer, spyCount);
                    }
                    else if (string.Equals(count, "invented"))
                    {
                        DropInvention(pPlayer, playedinvCount);
                    }
                    else if (string.Equals(count, "inventions"))
                    {
                        DropInvention(pPlayer, invCount);
                    }
                    else
                    {
                        decimal n = 0;
                        if (!decimal.TryParse(count, out n))
                            throw new Exception("Bad effect");

                        DropInvention(pPlayer, n);
                    }
                }
                else
                    throw new Exception("Wrong effect item");
            }
            else
                throw new Exception("Wrong effect direction");
        }

        void DropCoins(Player pPlayer, decimal dropCount)
        {
            if (pPlayer.Coins - dropCount < 0)
                pPlayer.Coins = 0;
            else
                pPlayer.Coins -= dropCount;
        }

        void DropInvention(Player pPlayer, decimal dropCount)
        {
            if (suspendUserActions) return;

            if (pPlayer.Inventions.Count < dropCount)
                dropCount = pPlayer.Inventions.Count;

            pPlayer.InventionToDropCount += dropCount;
        }

        void DropSpy(Player pPlayer, decimal dropCount)
        {
            if (suspendUserActions) return;

            var spyCount = pPlayer.Spies.Where(s => s > 0).Count();
            if (spyCount < dropCount)
                dropCount = spyCount;

            pPlayer.SpyToDropCount += dropCount;
        }

        void SetSpy(Player pPlayer, decimal setCount)
        {
            if (suspendUserActions) return;

            var spyCount = pPlayer.Spies.Where(s => s == 0).Count();
            if (setCount > spyCount)
                setCount = spyCount;

            pPlayer.SpyToSetCount += setCount;
        }
    }
}
/*
 * получить/отдать
 * монету/шпиона/карту
 * фиксированное количиство/по числу шпионов/по числу созданных изобретений/по числу изобретений в руке
 * 
 * 3 монеты за каждое изобретение в руке (46)
*/
