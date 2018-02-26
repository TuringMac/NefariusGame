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
            inventions.Add(new Invention("Палка тыкалка"));
            inventions.Add(new Invention("Ботинки прыгуны"));
            inventions.Add(new Invention("Яд со вкусом бессмертия"));
            inventions.Add(new Invention("Монета счастливчика"));
            inventions.Add(new Invention("Луч сытости"));
            inventions.Add(new Invention("Беспроводной кабель"));
            inventions.Add(new Invention("Сжатый вакуум"));
            inventions.Add(new Invention("Удобрение"));
            inventions.Add(new Invention("Плюмбус"));
            inventions.Add(new Invention("Портальная пушка"));
            inventions.Add(new Invention("Чернила для карандаша"));
            inventions.Add(new Invention("Троллейбус из хлеба"));

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
