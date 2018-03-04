using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace NefariusCore
{
    class DeckFiller
    {
        public static void Fill(Stack<Invention> pDeck)
        {
            string filename = "Data/inventions.json";
            string json = File.ReadAllText(filename);
            List<Invention> inventions = JsonConvert.DeserializeObject<List<Invention>>(json);
            ValidateInventions(inventions);

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

        static bool ValidateInventions(ICollection<Invention> pInventions)
        {
            bool result = true;
            foreach (var inv in pInventions)
            {
                if (string.IsNullOrWhiteSpace(inv.Description))
                    Debug.WriteLine("Карта без описания ID:" + inv.ID);
                foreach (var eff in inv.SelfEffectList)
                {
                    if (string.IsNullOrWhiteSpace(eff.direction))
                    {
                        Debug.WriteLine("Bad effect direction in card ID:" + inv.ID);
                        result = false;
                    }
                    if (string.IsNullOrWhiteSpace(eff.item))
                    {
                        Debug.WriteLine("Bad effect item in card ID:" + inv.ID);
                        result = false;
                    }
                    if (string.IsNullOrWhiteSpace(eff.count))
                    {
                        Debug.WriteLine("Bad effect count in card ID:" + inv.ID);
                        result = false;
                    }
                }
            }
            return result;
        }
    }
}
