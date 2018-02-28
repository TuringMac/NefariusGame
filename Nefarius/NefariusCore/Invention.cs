﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class Invention
    {
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public decimal Cost { get; set; }
        [DataMember]
        public decimal Score { get; set; }
        [DataMember]
        public List<Effect> SelfEffectList { get; set; }
        [DataMember]
        public List<Effect> OtherEffectList { get; set; }

        public Invention(string pName = "")
        {
            Name = pName;
        }
    }
}
