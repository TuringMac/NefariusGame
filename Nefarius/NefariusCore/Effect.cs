﻿using System;
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
    }
}
/*
 * получить/отдать
 * монету/шпиона/карту
 * фиксированное количиство/по числу шпионов/по числу созданных изобретений/по числу изобретений в руке
 * 
 * 3 монеты за каждое изобретение в руке (46)
*/
