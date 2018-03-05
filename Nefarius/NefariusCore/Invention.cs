using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class Invention
    {
        [DataMember]
        public decimal ID { get; set; }
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public decimal Cost { get; set; }
        [DataMember]
        public decimal Score { get; set; }
        [DataMember]
        public List<Effect> SelfEffectList { get; set; } = new List<Effect>();
        [DataMember]
        public List<Effect> OtherEffectList { get; set; } = new List<Effect>();

        public Invention(string pName = "")
        {
            Name = pName;
        }
    }
}
/*
{
    "ID": ,
    "Name": "",
    "Description": "",
    "Cost": ,
    "Score": ,
    "SelfEffectList": [
      {
        "direction": "",
        "item": "",
        "count": ""
      }
    ],
    "OtherEffectList": []
  },

    Score by invented (45)
*/
