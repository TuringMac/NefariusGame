using System;
using System.Collections.Generic;
using System.Text;

namespace NefariusCore
{
    class DeckFiller
    {
        public static void Fill(Stack<Invention> pDeck)
        {
            List<Invention> inventions = new List<Invention>();
            inventions.Add(new Invention("Палка тыкалка") { Score = 2, Cost = 5, ID = 1 });
            inventions.Add(new Invention("Ботинки прыгуны") { Score = 1, Cost = 2, ID = 2 });
            inventions.Add(new Invention("Яд со вкусом бессмертия") { Score = 3, Cost = 7, ID = 3 });
            inventions.Add(new Invention("Монета счастливчика") { Score = 8, Cost = 20, ID = 4 });
            inventions.Add(new Invention("Луч сытости") { Score = 4, Cost = 10, ID = 5 });
            inventions.Add(new Invention("Беспроводной кабель") { Score = 5, Cost = 12, ID = 6 });
            inventions.Add(new Invention("Сжатый вакуум") { Score = 3, Cost = 7, ID = 7 });
            inventions.Add(new Invention("Удобрение") { Score = 2, Cost = 5, ID = 8 });
            inventions.Add(new Invention("Плюмбус") { Score = 1, Cost = 2, ID = 9 });
            inventions.Add(new Invention("Портальная пушка") { Score = 6, Cost = 15, ID = 10 });
            inventions.Add(new Invention("Чернила для карандаша") { Score = 4, Cost = 10, ID = 11 });
            inventions.Add(new Invention("Троллейбус из хлеба") { Score = 0, Cost = 0, ID = 12 });

            pDeck.Clear();
            do
            {
                pDeck.Push(PickRandom(inventions));
            } while (inventions.Count > 0);
        }

        public static void FillColorDeck(Stack<PlayerColor> pDeck)
        {
            List<PlayerColor> colors = new List<PlayerColor>();
            colors.Add(PlayerColor.Blue);
            colors.Add(PlayerColor.Brown);
            colors.Add(PlayerColor.Green);
            colors.Add(PlayerColor.Purple);
            colors.Add(PlayerColor.Red);
            colors.Add(PlayerColor.Yellow);

            pDeck.Clear();
            do
            {
                pDeck.Push(PickRandom(colors));
            } while (colors.Count > 0);
        }

        static T PickRandom<T>(List<T> pItemsList)
        {
            Random rnd = new Random();
            int i = rnd.Next(pItemsList.Count - 1); // Pick random color
            T result = pItemsList[i];
            pItemsList.RemoveAt(i);
            return result;
        }
    }
}
