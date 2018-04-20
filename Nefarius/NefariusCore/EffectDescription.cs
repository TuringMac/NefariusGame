using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class EffectDescription
    {
        [DataMember]
        public string direction { get; set; } // get/drop
        [DataMember]
        public string item { get; set; } // coin/spy/invention
        [DataMember]
        public string count { get; set; } // fixed("1","2","3")/"spy"/"invented"/"inventions"
    }
}
