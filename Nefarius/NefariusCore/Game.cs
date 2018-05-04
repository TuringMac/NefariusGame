using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace NefariusCore
{
    public class Game : INotifyPropertyChanged
    {
        GameState _State = GameState.Init;

        public Stack<Rule> RuleDeck { get; private set; } = new Stack<Rule>();
        public Stack<Invention> InventDeck { get; private set; } = new Stack<Invention>();
        public Stack<PlayerColor> ColorDeck { get; private set; } = new Stack<PlayerColor>();
        internal List<Player> PlayerList { get; private set; }
        internal DefaultEffectManager EM { get; set; } //TODO Create Custom replace with rules
        public Rule FirstRule { get; set; }
        public Rule SecondRule { get; set; }
        public decimal Move { get; set; } = 0;
        public GameState State
        {
            get
            {
                return _State;
            }
            private set
            {
                if (_State != value)
                {
                    _State = value;
                    Debug.WriteLine($"State: {_State}");
                    NotifyPropertyChanged();
                }
            }
        }

        AutoResetEvent turnEvt = new AutoResetEvent(false);
        AutoResetEvent spyEvt = new AutoResetEvent(false);
        AutoResetEvent inventEvt = new AutoResetEvent(false);
        AutoResetEvent effectEvt = new AutoResetEvent(false);

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public delegate void EffectQueueChangedH();
        public event EffectQueueChangedH EffectQueueChanged;

        public Game(List<Player> pPlayers)
        {
            if (pPlayers.Count < 2 || pPlayers.Count > 6)
                throw new Exception("Wrong Player count. Game for 2 - 6 players. Invite more players"); //TODO

            PlayerList = pPlayers;
            DeckFiller.Fill(InventDeck);
            DeckFiller.FillColorDeck(ColorDeck);
            DeckFiller.FillRuleDeck(RuleDeck);
            FirstRule = RuleDeck.Pop();
            SecondRule = RuleDeck.Pop();
            EM = new DefaultEffectManager(this);
        }

        public bool Start()
        {
            new Thread(Run).Start();
            return true;
        }

        public bool Stop()
        {
            //TODO
            return false;
        }

        void Run()
        {
            InitGame();

            do
            {
                State = GameState.Turn;
                if (!CheckEverybodyDoAction())
                    turnEvt.WaitOne(); // Wait for CheckEverybodyDoAction() == true and Set

                State = GameState.Spying;
                Spying();

                State = GameState.Spy;
                if (!CheckEverybodyDoSpy())
                    spyEvt.WaitOne(); // 

                State = GameState.Invent;
                if (!CheckEverybodyDoInvent())
                    inventEvt.WaitOne();

                State = GameState.Inventing;
                Inventing();

                State = GameState.Researching;
                Researching();

                State = GameState.Working;
                Working();

                State = GameState.Scoring;
            } while (!HasWinner() && PlayerList.Count >= 2);

            State = GameState.Win;
            GameOver();
        }

        protected virtual bool InitGame()
        {
            foreach (var player in PlayerList)
            {
                player.Color = ColorDeck.Pop();

#if DEBUG
                player.Coins = 1000;
                for (int i = 0; i < 10; i++)
#else
                player.Coins = 10;
                for (int i = 0; i < 3; i++) // Каждому по 3 карты
#endif

                {
                    player.Inventions.Add(InventDeck.Pop());
                }
            }
            Move = 1;
            return true;
        }

        protected virtual bool GameOver()
        {
            return true;
        }

        /// <summary>
        /// Выбирают действие на следующий ход
        /// </summary>
        /// <returns></returns>
        public bool Turn(Player pPlayer, GameAction pAction)
        {
            if (State != GameState.Turn)
            {
                Debug.WriteLine("Turning after Working");
                return false;
            }

            pPlayer.Action = pAction;
            Debug.WriteLine($"{pPlayer.Name} выбрал(а) карту действия");
            if (CheckEverybodyDoAction())
                turnEvt.Set();

            return true;
        }

        protected virtual bool Spying()
        {
            // Если справа или слева от игрока со шпионом разыграли действия
            for (int i = 0; i < PlayerList.Count; i++)
            {
                int prev = i - 1;
                int next = i + 1;

                if (prev < 0) prev = PlayerList.Count - 1;
                if (next > PlayerList.Count - 1) next = 0;

                foreach (var spy in PlayerList[i].Spies)
                {
                    if (spy == PlayerList[prev].Action)
                    {
                        PlayerList[i].Coins++;
                        Debug.WriteLine($"{PlayerList[i].Name} получил монету за игрока {PlayerList[prev].Name}");
                    }
                    if (spy == PlayerList[next].Action)
                    {
                        PlayerList[i].Coins++;
                        Debug.WriteLine($"{PlayerList[i].Name} получил монету за игрока {PlayerList[next].Name}");
                    }
                }
            }
            return true;
        }

        public bool Spy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            if (pDestSpyPosition == pSourceSpyPosition)
            {
                Debug.WriteLine("Исходная позиция шпиона совпадает с конечной");
                return false;
            }
            if (pPlayer.Spies.Count(s => s == pSourceSpyPosition) == 0) // Если нет шпионов в исходной позиции
            {
                Debug.WriteLine("Нет шпиона в исходной позиции");
                return false;
            }

            bool isDrop = pSourceSpyPosition != GameAction.None && pDestSpyPosition == GameAction.None;
            bool isSet = pSourceSpyPosition == GameAction.None && pDestSpyPosition != GameAction.None;

            if (State == GameState.Spy)
            {
                if (pPlayer.Action != GameAction.Spy)
                {
                    Debug.WriteLine($"{pPlayer.Name} Шпионаж не в свой ход");
                    return false;
                }

                var result = pPlayer.SetSpy(pDestSpyPosition);
                if (result)
                {
                    pPlayer.Action = GameAction.None;
                    pPlayer.CurrentSetSpy = GameAction.None;
                    Debug.WriteLine($"{pPlayer.Name} шпионит за {pDestSpyPosition}");
                    if (CheckEverybodyDoSpy())
                        spyEvt.Set();
                }

                return result;
            }
            else if (State == GameState.Inventing)
            {
                //TODO Check top effect for drop spy requiring. Continue if not
                if (isSet && pPlayer.CurrentEffect != null && pPlayer.CurrentEffect.It == EffectItem.Spy && pPlayer.CurrentEffect.Dir == EffectDirection.Get)
                {
                    pPlayer.SetSpy(pDestSpyPosition);
                    Debug.WriteLine($"{pPlayer.Name} шпионит за {pDestSpyPosition}");
                }
                else if (isDrop && pPlayer.CurrentEffect != null && pPlayer.CurrentEffect.It == EffectItem.Spy && pPlayer.CurrentEffect.Dir == EffectDirection.Drop)
                {
                    pPlayer.DropSpy(pSourceSpyPosition);
                    Debug.WriteLine($"{pPlayer.Name} убрал шпиона с {pSourceSpyPosition}");
                }
                else // Move (not set, not drop)
                {
                    Debug.WriteLine("Moving Spies not allowed");
                    return false;
                }

                effectEvt.Set();

                return true;
            }
            else
            {
                Debug.WriteLine($"{pPlayer.Name} Spy не в свой ход");
                return false;
            }
        }

        public bool Invent(Player pPlayer, Invention pInvention)
        {
            if (State == GameState.Invent)
            {
                pPlayer.PlayInvention(pInvention);
                Debug.WriteLine($"{pPlayer.Name} изобрел {pInvention.Name}");
                if (CheckEverybodyDoInvent())
                    inventEvt.Set();
            }
            else if (State == GameState.Inventing)
            {
                //TODO Check top effect for drop card requiring. Continue if not
                pPlayer.DropInvention(pInvention);
                Debug.WriteLine($"{pPlayer.Name} отказался от изобретения {pInvention.Name}");

                effectEvt.Set();
            }
            else
            {
                Debug.WriteLine($"{pPlayer.Name} Invent не в свой ход");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Собирает эффекты разыгранных изобретений и применяет их на игроках
        /// </summary>
        protected virtual bool Inventing()
        {
            EM.Assign();
            PrintEffects();
            while (!ApplyEffects())
            {
                if (EffectQueueChanged != null)
                    EffectQueueChanged();
                effectEvt.WaitOne();
            }
            if (EffectQueueChanged != null)
                EffectQueueChanged();
            return true;
        }

        protected virtual bool Researching()
        {
            foreach (var player in PlayerList)
            {
                if (player.Action != GameAction.Research)
                    continue;

                player.Coins += 2;
                player.Inventions.Add(InventDeck.Pop());
                player.Action = GameAction.None;
                Debug.WriteLine($"{player.Name} провёл исследование");
            }
            return true;
        }

        protected virtual bool Working()
        {
            foreach (var player in PlayerList)
            {
                if (player.Action != GameAction.Work)
                    continue;

                player.Coins += 4;
                player.Action = GameAction.None;
                Debug.WriteLine($"{player.Name} отработал смену");
            }
            return true;
        }

        #region Methods

        public bool CheckEverybodyDoAction()
        {
            foreach (var player in PlayerList)
            {
                if (player.Action == GameAction.None)
                    return false;
            }
            return true;
        }

        public bool CheckEverybodyDoSpy()
        {
            return !PlayerList.Where(player => player.Action == GameAction.Spy).Any();
        }

        public bool CheckEverybodyDoInvent()
        {
            return !PlayerList.Where(player => player.Action == GameAction.Invent).Any();
        }

        public bool CheckEverybodyApplyEffects()
        {
            return !PlayerList.Where(player => player.CurrentEffect != null).Any(); // Есть ли игроки у которых остались неразыгранные эффекты
        }

        bool IsWinner(Player pPlayer)
        {
            decimal score = 0;
            foreach (var invent in pPlayer.PlayedInventions)
            {
                score += invent.Score;
            }
            return score >= 20;
        }

        bool HasWinner()
        {
            var maxScore = PlayerList.Max(p => p.Score);
            if (maxScore > 20) // Any player has more than 20
            {
                var playersWithMax = PlayerList.Where(p => p.Score == maxScore);
                if (playersWithMax.Count() == 1) // Only one player with max score
                {
                    //var winner = playersWithMax.First();
                    //Debug.WriteLine(winner.Name + " is Winner!!");
                    return true;
                }
            }
            // Have not players with win score OR more than one winner
            Move++;
            return false;
        }

        bool ApplyEffects()
        {
            foreach (var player in PlayerList)
            {
                while (player.HasEffect && EM.Apply(player)) ;
            }
            return !PlayerList.Where(pl => pl.HasEffect).Any(); // false - эффекты не погашены
        }

        void PrintEffects()
        {
            Debug.WriteLine("--- Effect snapshot ---");
            foreach (var player in PlayerList)
            {
                Debug.WriteLine(player.Name);
                foreach (var eff in player.EffectQueue)
                {
                    Debug.WriteLine($"{eff.direction} {eff.count} {eff.item}");
                }
                Debug.WriteLine("----------------");
            }
        }

        #endregion Methods
    }
}
