using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    class Player
    {
        public string Name { get; set; } = "";
        public decimal Coins { get; set; } = 10;
        public IEnumerable<Invent> Invents { get; protected set; }

        public Player()
        {
            Invents = new List<Invent>();
        }
    }
}
