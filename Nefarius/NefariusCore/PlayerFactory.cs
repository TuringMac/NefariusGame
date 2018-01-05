using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    class PlayerFactory
    {
        public static Player CreatePlayer(string pName)
        {
            return new Player(pName);
        }
    }
}
