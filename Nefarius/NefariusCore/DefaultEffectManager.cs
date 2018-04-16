using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NefariusCore
{
    class DefaultEffectManager : IEffectManager
    {
        Game _Game;

        public DefaultEffectManager(Game pGame)
        {
            _Game = pGame;
        }

        public void Assign()
        {
            // Self effects first
            foreach (var player in _Game.PlayerList)
            {
                if (player.CurrentInvention == null) continue;

                foreach (var eff in player.CurrentInvention.SelfEffectList)
                    player.EffectQueue.Enqueue(eff);
            }

            // Other effects second
            foreach (var player in _Game.PlayerList) // Эффекты по часовой стрелке
            {
                foreach (var inventor in _Game.PlayerList) //TODO Reverse?
                {
                    if (inventor.CurrentInvention == null) continue;
                    if (inventor == player) continue; // Self effect done before

                    foreach (var effect in inventor.CurrentInvention.OtherEffectList)
                    {
                        player.EffectQueue.Enqueue(effect);
                    }
                }
            }

            // Clear current invention and work with queue
            foreach (var inventor in _Game.PlayerList)
            {
                if (inventor.CurrentInvention != null)
                    inventor.CurrentInvention = null;
            }
        }

        public bool Apply(Player pPlayer) //TODO refactor! mb strategy // TODO for debug
        {
            if (pPlayer.EffectQueue.Count == 0) return true;

            Effect eff = pPlayer.EffectQueue.Peek();

            if (!eff.IsFormal)
                FormalizeTop(pPlayer);

            if (eff.It == EffectItem.Coin)
            {
                if (eff.Dir == EffectDirection.Get)
                {
                    pPlayer.Coins += eff.Count;
                }
                else if (eff.Dir == EffectDirection.Drop)
                {
                    pPlayer.DropCoins(eff.Count);
                }
                else
                {
                    Debug.WriteLine("Wrong direction");
                }
            }
            else if (eff.It == EffectItem.Spy)
            {
                if (eff.Dir == EffectDirection.Get)
                {
                    if (pPlayer.CurrentSetSpy != GameAction.None) // Уменьшаем эффект при скинутой карте
                    {
                        eff.Count--;
                        pPlayer.CurrentSetSpy = GameAction.None;
                    }
                    if (pPlayer.Spies.Where(s => s == GameAction.None).Any() && eff.Count != 0) // Нет шпионов чтобы выставить. Можно гасить эффект
                        return false;
                }
                else if (eff.Dir == EffectDirection.Drop)
                {
                    if (pPlayer.CurrentDropSpy != GameAction.None) // Уменьшаем эффект при скинутой карте
                    {
                        eff.Count--;
                        pPlayer.CurrentDropSpy = GameAction.None;
                    }
                    if (pPlayer.Spies.Where(s => s != GameAction.None).Any() && eff.Count != 0) // Нет шпионов чтобы сбросить. Можно гасить эффект
                        return false;
                }
                else
                {
                    Debug.WriteLine("Wrong direction");
                }
            }
            else if (eff.It == EffectItem.Invention)
            {
                if (eff.Dir == EffectDirection.Get)
                {
                    for (int i = 0; i < eff.Count; i++)
                        pPlayer.Inventions.Add(_Game.InventDeck.Pop());
                }
                else if (eff.Dir == EffectDirection.Drop)
                {
                    if (pPlayer.CurrentInvention != null) // Уменьшаем эффект при скинутой карте
                    {
                        eff.Count--;
                        pPlayer.CurrentInvention = null;
                    }
                    if (pPlayer.GetInventionsCount() != 0 && eff.Count != 0) // Пользователь скинул все требуемые карты. Можно гасить эффект
                        return false;
                }
                else
                {
                    Debug.WriteLine("Wrong direction");
                }
            }
            else
            {
                Debug.WriteLine("Wrong item");
            }

            pPlayer.EffectQueue.Dequeue();
            FormalizeTop(pPlayer);
            return true;
        }

        /// <summary>
        /// Очередной эффект заполняется конкретными значениями
        /// </summary>
        /// <param name="pPlayer">Игрок</param>
        /// <returns></returns>
        static bool FormalizeTop(Player pPlayer)
        {
            if (pPlayer.EffectQueue.Count == 0) return true;

            Effect eff = pPlayer.EffectQueue.Peek();
            switch (eff.direction)
            {
                case "get": eff.Dir = EffectDirection.Get; break;
                case "drop": eff.Dir = EffectDirection.Drop; break;
                default: Debug.WriteLine("Wrong effect direction"); break;
            }
            switch (eff.item)
            {
                case "coin": eff.It = EffectItem.Coin; break;
                case "spy": eff.It = EffectItem.Spy; break;
                case "invention": eff.It = EffectItem.Invention; break;
                default: Debug.WriteLine("Wrong effect item"); break;
            }
            if (string.Equals(eff.count, "spy"))
            {
                eff.Count = pPlayer.GetSpyCount();
            }
            else if (string.Equals(eff.count, "invented"))
            {
                eff.Count = pPlayer.GetPlayedInventionsCount();
            }
            else if (string.Equals(eff.count, "inventions"))
            {
                eff.Count = pPlayer.GetInventionsCount();
            }
            else
            {
                decimal n = 0;
                if (!decimal.TryParse(eff.count, out n))
                    Debug.WriteLine("Wrong effect count");

                eff.Count = (int)n;
            }
            return true;
        }
    }
}
