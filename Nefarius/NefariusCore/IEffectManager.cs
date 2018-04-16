using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    interface IEffectManager
    {
        void Assign();
        bool Apply(Player pPlayer);
    }
}
