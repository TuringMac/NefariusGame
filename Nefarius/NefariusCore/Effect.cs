using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class Effect
    {
        [DataMember]
        public string Direction { get; set; }
        [DataMember]
        public string Item { get; set; }
        [DataMember]
        public string Inventor { get; set; }
        [DataMember]
        public EffectDirection Dir { get; set; } = EffectDirection.None;
        [DataMember]
        public EffectItem It { get; set; } = EffectItem.None;
        [DataMember]
        public int Count { get; set; } = 0;

        public Effect(EffectDescription pDescription, Player pPlayer)
        {
            Direction = pDescription.direction;
            Item = pDescription.item;
            Inventor = pDescription.Inventor;

            switch (pDescription.direction)
            {
                case "get": Dir = EffectDirection.Get; break;
                case "drop": Dir = EffectDirection.Drop; break;
                default: Console.WriteLine("Wrong effect direction"); break;
            }

            switch (pDescription.item)
            {
                case "coin": It = EffectItem.Coin; break;
                case "spy": It = EffectItem.Spy; break;
                case "invention": It = EffectItem.Invention; break;
                default: Console.WriteLine("Wrong effect item"); break;
            }

            switch (pDescription.count)
            {
                case "spy":
                    Count = pPlayer.GetSpyCount();
                    break;
                case "invented":
                    Count = pPlayer.GetPlayedInventionsCount();
                    break;
                case "inventions":
                    Count = pPlayer.GetInventionsCount();
                    break;
                default:
                    decimal n = 0;
                    if (!decimal.TryParse(pDescription.count, out n))
                        Console.WriteLine("Wrong effect count");

                    Count = (int)n;
                    break;
            }
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
