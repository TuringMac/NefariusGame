using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    public class Rule
    {
        public string Name { get; set; }

        public Rule(string pName)
        {
            Name = pName;
        }
    }
}
