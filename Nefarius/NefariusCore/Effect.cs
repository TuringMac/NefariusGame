using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class Effect : EffectDescription
    {
        [DataMember]
        public EffectDirection Dir { get; set; } = EffectDirection.None;
        [DataMember]
        public EffectItem It { get; set; } = EffectItem.None;
        [DataMember]
        public int Count { get; set; } = 0;

        public Effect(EffectDescription pDescription, Player pPlayer)
        {
            switch (pDescription.direction)
            {
                case "get": Dir = EffectDirection.Get; break;
                case "drop": Dir = EffectDirection.Drop; break;
                default: Debug.WriteLine("Wrong effect direction"); break;
            }

            switch (pDescription.item)
            {
                case "coin": It = EffectItem.Coin; break;
                case "spy": It = EffectItem.Spy; break;
                case "invention": It = EffectItem.Invention; break;
                default: Debug.WriteLine("Wrong effect item"); break;
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
                        Debug.WriteLine("Wrong effect count");

                    Count = (int)n;
                    break;
            }
        }

        public bool IsFormal
        {
            get
            {
                return true; // TODO remove this property
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
