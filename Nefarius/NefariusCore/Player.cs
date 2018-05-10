﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NefariusCore
{
    public class Player
    {
        #region Public

        public string ID { get; set; } //TODO Model should not know about signalr ids
        public PlayerColor Color { get; set; }
        public string Name { get; protected set; } = "";
        public decimal Coins { get; set; } = 0;
        public GameAction[] Spies { get; protected set; }
        public decimal InventionCount { get { return Inventions.Count; } }

        //Rule 36 if(GetSpyCount()==0) +3 score
        public decimal Score
        {
            get
            {
                //Rule 32 +5 points in >=5 inventions
                //Rule 25 each invention +1 score PlayedInventions.Sum(inv => inv.Score) + PlayedInvention.Count;
                //Rule 31 Score += (int)player.Coins / 5
                return PlayedInventions.Sum(inv => inv.Score);
            }
        }
        public bool IsMoved { get { return Action != GameAction.None; } }
        public ICollection<Invention> PlayedInventions { get; protected set; } = new List<Invention>();
        public Queue<EffectDescription> EffectQueue { get; protected set; } = new Queue<EffectDescription>();
        public Effect CurrentEffect { get; protected set; }
        public bool HasEffect { get { return EffectQueue.Count > 0 || CurrentEffect != null; } }

        #endregion Public

        #region Private

        public ICollection<Invention> Inventions { get; protected set; } = new List<Invention>(); // TODO internal + dataContract
        public GameAction Action { get; set; } // TODO internal + dataContract
        public Invention CurrentInvention { get; set; } // TODO internal + dataContract
        public GameAction CurrentSetSpy { get; set; }
        public GameAction CurrentDropSpy { get; set; }

        #endregion Private

        public Player(string pName)
        {
            Name = pName;
            Spies = new GameAction[] { GameAction.None, GameAction.None, GameAction.None, GameAction.None, GameAction.None };
        }

        public dynamic GetPlayerShort(bool pIsOpen = false)
        {
            return new
            {
                ID,
                Color,
                Name,
                Coins,
                Spies,
                Score,
                InventionCount,
                PlayedInventions,
                EffectQueue,
                IsMoved,
                Action = pIsOpen ? Action : GameAction.None,
            };
        }

        public void DropCoins(decimal dropCount)
        {
            if (Coins - dropCount < 0)
                Coins = 0;
            else
                Coins -= dropCount;
        }

        public bool SetSpy(GameAction pDestSpyPosition)
        {
            if (pDestSpyPosition == GameAction.None)
            {
                Debug.WriteLine("In Spy state only set spy");
                return false;
            }

            // Has spy in source if(GetSpyCount()<5)
            // Has money to dest
            for (int i = 0; i < Spies.Count(); i++)
            {
                if (Spies[i] == GameAction.None)
                {
                    switch (pDestSpyPosition)
                    {
                        case GameAction.Spy: break;
                        case GameAction.Invent:
                            if (Coins >= 2)
                                DropCoins(2);
                            else
                            {
                                Debug.WriteLine("Not enought coins to spy");
                                return false;
                            }
                            break;
                        case GameAction.Research: break;
                        case GameAction.Work:
                            if (Coins >= 1)
                                DropCoins(1);
                            else
                            {
                                Debug.WriteLine("Not enought coins to spy");
                                return false;
                            }
                            break;
                    }
                    Spies[i] = pDestSpyPosition;
                    CurrentSetSpy = pDestSpyPosition;
                    return true;
                }
            }
            Debug.WriteLine("No spy in source location");
            return false;
        }

        public bool DropSpy(GameAction pSourceSpyPosition)
        {
            for (int i = 0; i < Spies.Count(); i++)
            {
                if (Spies[i] == pSourceSpyPosition)
                {
                    Spies[i] = GameAction.None;
                    CurrentDropSpy = pSourceSpyPosition;
                    break;
                }
            }
            return true;
        }

        public bool DropInvention(Invention pInvention)
        {
            if (!Inventions.Contains(pInvention))
            {
                Debug.WriteLine("You haven't got this invention! Cheater?");
                return false;
            }

            CurrentInvention = pInvention;
            Inventions.Remove(pInvention);

            return true;
        }

        public bool PlayInvention(Invention pInvention)
        {
            if (Action != GameAction.Invent)
            {
                Debug.WriteLine("Изобретение не в свой ход");
                return false;
            }

            if (Coins < pInvention.Cost)
            {
                Debug.WriteLine("You haven't got enought coins");
                Action = GameAction.None;
                return true; //TODO true но карта не разыгрывается
            }

            if (!DropInvention(pInvention))
            {
                return false;
            }

            //Rule 34 +Effect drop spy/ if have not set spy to spy-zone and skip invention
            PlayedInventions.Add(pInvention);
            Coins -= CurrentInvention.Cost;
            //Rule 13 Coins = 0;

            Action = GameAction.None;

            return true;
        }

        public int GetSpyCount()
        {
            return Spies.Where(s => s != 0).Count();
        }

        public int GetInventionsCount()
        {
            return Inventions.Count();
        }

        public int GetPlayedInventionsCount()
        {
            return PlayedInventions.Count();
        }

        public bool PrepareEffect()
        {
            CurrentEffect = null;

            if (EffectQueue.Count > 0)
            {
                var effDesc = EffectQueue.Dequeue();
                var eff = new Effect(effDesc, this);
                CurrentEffect = eff;
                Debug.WriteLine($"{Name} должен {effDesc.direction} {eff.Count} {effDesc.item}");
            }

            return true;
        }
    }
}
