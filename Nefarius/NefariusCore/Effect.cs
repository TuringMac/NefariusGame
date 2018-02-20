using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public abstract class Effect
    {
        [DataMember]
        public string Name { get; set; }
        public abstract void Apply(Player pPlayer);
    }
}
