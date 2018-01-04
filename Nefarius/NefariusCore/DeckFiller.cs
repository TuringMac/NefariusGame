using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    class DeckFiller
    {
        public static void Fill(Stack<Invention> pDeck)
        {
            pDeck.Clear();
            pDeck.Push(new Invention());
            pDeck.Push(new Invention());
            pDeck.Push(new Invention());
            pDeck.Push(new Invention());
            pDeck.Push(new Invention());
            pDeck.Push(new Invention());
            pDeck.Push(new Invention());
        }
    }
}
