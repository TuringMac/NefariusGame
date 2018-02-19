using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    public class Invention
    {
        public decimal Score { get; protected set; }
        public List<Effect> SelfEffectList { get; set; }
        public List<Effect> OtherEffectList { get; set; }
    }
}
