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
                //Rule 16 Wait for invent from everybody (invent should be rewrited)
                //Rule 8 монеты кратные 5
                //Rule 1 if(player.PlayedInventions.Count == 0) player.Coins += 2;
                //Rule 18 player.Coins += 2
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

                //Rule 3 playerList.Where(GameAction==Invent){Drop all inventions, get same count inventions} mb in inventing effect
                //Rule 19 Drop 1 coin for each player with the same action
                //Rule 23 if(player.coin==0)player.Coin=2; if(player.Inventions.Count==0)player.Inventions.Add(InventionsDeck.Pop());

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
                //Rule 24 player.Coins += 10;
                //Rule 33 +7 inventions
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
            //Rule 2 if(pPlayer.PrevAction == pAction) return false;

            //Rule 22 pPlayer.Action1= ; pPlayer.Action2= ; two different actions per move
            pPlayer.Action = pAction;
            //Rule 20 reward for more spies in action +2 coins
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
                //Rule 35 if(player.PlayedInventions.Count==player.GetSpyCount())
                foreach (var spy in PlayerList[i].Spies)
                {
                    //Rule 14 if(spy == PlayerList[i].Action) PlayerList[i].Coins++;
                    if (spy == PlayerList[prev].Action)
                    {
                        //Rule 11 //Rule 21 if(player.Action ==Research) PlayerList[i].Coins++; Doubles profit
                        PlayerList[i].Coins++;
                        Debug.WriteLine($"{PlayerList[i].Name} получил монету за игрока {PlayerList[prev].Name}");
                    }
                    if (spy == PlayerList[next].Action)
                    {
                        //Rule 11 //Rule 21 if(player.Action ==Research) PlayerList[i].Coins++; Doubles profit
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
                    //Rule 29 player.Coin += GetSpyCount(player.Action); //TODO remember spy coint before anybody set spy
                    //Rule 17 player get invention
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
                //Rule 10 drop invention
                //Rule 27 get invention
                //Rule 5 if(pInvention.Score>player.PlayedInvention.Max(inv=>inv.Score))player.Coin+=2;
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
            //Rule 28 Disable effects
            EM.Assign();
            //Rule 26 //Rule 21 effects playes twise. double assign
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

                //Rule 4 no coin income
                player.Coins += 2;
                player.Inventions.Add(InventDeck.Pop());
                //Rule 10 player.Inventions.Add(InventDeck.Pop());
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

        protected virtual bool Scoring() // True to win, false to continue game
        {
            //Rule 12 win score == 30
            decimal winScore = 20;
            Player winner = null;
            foreach (var player in PlayerList)
            {
                if (player.Score >= winScore)
                {
                    if (winner != null) // Когда более одного 
                    {
                        if (winner.Score == player.Score)
                        {
                            Move++;
                            return false;
                        }
                        else if (winner.Score < player.Score)
                            winner = player;
                    }
                    else
                        winner = player;
                }
            }
            if (winner == null) // Когда ниодного
            {
                Move++;
                return false;
            }
            else
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
            bool winner = false;
            foreach (var player in PlayerList)
            {
                if (IsWinner(player))
                {
                    if (winner) // 2 or more winners
                    {
                        return false;
                    }
                    else
                    {
                        winner = true;
                    }
                }
            }
            return winner;
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

        decimal GetSpyCount(GameAction pArea)
        {
            decimal result = 0;
            foreach (var player in PlayerList)
            {
                foreach (var spy in player.Spies)
                {
                    if (spy == pArea)
                        result++;
                }
            }
            return result;
        }

        #endregion Methods
    }
}
