using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                //Rule 7 inv.Cost = Math.Ceiling((inv.Cost/2.));
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

        public static void FillRuleDeck(Stack<Rule> pDeck)
        {
            string filename = "Data/rules.json";
            string json = File.ReadAllText(filename);
            List<Rule> rules = JsonConvert.DeserializeObject<List<Rule>>(json);
            ValidateRules(rules);

            pDeck.Clear();
            do
            {
                pDeck.Push(PickRandom(rules));
            } while (rules.Count > 0);
        }

        static T PickRandom<T>(List<T> pItemsList)
        {
            Random rnd = new Random();
            int i = rnd.Next(pItemsList.Count); // Pick random color
            T result = pItemsList[i];
            pItemsList.RemoveAt(i);
            return result;
        }

        static bool ValidateInventions(ICollection<Invention> pInventions)
        {
            Debug.WriteLine("Start validating Inventions deck");
            bool result = true;
            for (int i = 1; i < pInventions.Count + 1; i++)
            {
                try
                {
                    pInventions.Single(inv => inv.ID == i);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Card ID:{i} {ex.Message}");
                }
            }
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
            Debug.WriteLine("Finish validating deck: Result:" + (result ? "OK" : "Fail"));
            return result;
        }

        static bool ValidateRules(ICollection<Rule> pRules)
        {
            Debug.WriteLine("Start validating Rules deck");
            bool result = true;
            for (int i = 1; i < pRules.Count + 1; i++)
            {
                try
                {
                    pRules.Single(rule => rule.ID == i);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Rule ID:{i} {ex.Message}");
                }
            }
            foreach (var rule in pRules)
            {
                if (string.IsNullOrWhiteSpace(rule.Description))
                    Debug.WriteLine("Карта без описания ID:" + rule.ID);
                if (string.IsNullOrWhiteSpace(rule.Title))
                    Debug.WriteLine("Карта без названия ID:" + rule.ID);

            }
            Debug.WriteLine("Finish validating deck: Result:" + (result ? "OK" : "Fail"));
            return result;
        }
    }
}
