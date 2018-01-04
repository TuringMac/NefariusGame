using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    public class StateEventArgs : EventArgs
    {
        public GameState State { get; set; }
    }
}
