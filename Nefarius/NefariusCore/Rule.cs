using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class Rule
    {
        [DataMember]
        public decimal ID { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }

        public Rule(string pName)
        {
            Title = pName;
        }
    }
}
