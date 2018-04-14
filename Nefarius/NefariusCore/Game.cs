using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NefariusCore
{
    public class Game
    {
        public Stack<Invention> InventDeck { get; private set; } = new Stack<Invention>();
        public Stack<PlayerColor> ColorDeck { get; private set; } = new Stack<PlayerColor>();
        protected List<Player> PlayerList { get; private set; }
        public decimal Move { get; set; } = 0;
        public GameState State { get; private set; } = GameState.Init;

        AutoResetEvent turnEvt = new AutoResetEvent(false);
        AutoResetEvent spyEvt = new AutoResetEvent(false);
        AutoResetEvent inventEvt = new AutoResetEvent(false);
        AutoResetEvent effectEvt = new AutoResetEvent(false);

        public Game(List<Player> pPlayers)
        {
            if (pPlayers.Count < 2 || pPlayers.Count > 6)
                throw new Exception("Wrong Player count. Game for 2 - 6 players. Invite more players"); //TODO

            PlayerList = pPlayers;
            DeckFiller.Fill(InventDeck);
            DeckFiller.FillColorDeck(ColorDeck);
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
            } while (!HasWinner());

            State = GameState.Win;
            GameOver();
        }

        protected virtual bool InitGame()
        {
            foreach (var player in PlayerList)
            {
                player.Color = ColorDeck.Pop();
                player.Coins = 10;
                for (int i = 0; i < 3; i++) // Каждому по 3 карты
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
            pPlayer.Action = pAction;

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
                        PlayerList[i].Coins++;
                    if (spy == PlayerList[next].Action)
                        PlayerList[i].Coins++;
                }
            }
            return true;
        }

        public bool Spy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            var result = pPlayer.SetSpy(pDestSpyPosition);
            if (result)
            {
                pPlayer.Action = GameAction.None;
                pPlayer.CurrentSetSpy = GameAction.None;
            }

            if (CheckEverybodyDoSpy())
                spyEvt.Set();

            return result;
        }

        public bool Invent(Player pPlayer, Invention pInvention)
        {
            var result = pPlayer.PlayInvention(pInvention);

            if (CheckEverybodyDoInvent())
                inventEvt.Set();

            return result;
        }

        /// <summary>
        /// Собирает эффекты разыгранных изобретений и применяет их на игроках
        /// </summary>
        protected virtual bool Inventing()
        {
            EffectManager.Assign(PlayerList);
            while (!ApplyEffects())
            {
                effectEvt.WaitOne();
            }
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
            }
            return true;
        }

        protected virtual bool Scoring() // True to win, false to continue game
        {
            Player winner = null;
            foreach (var player in PlayerList)
            {
                if (player.Score >= 20)
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
            return !PlayerList.Where(player => player.EffectQueue.Any()).Any(); // Есть ли игроки у которых остались неразыгранные эффекты
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
                while (player.EffectQueue.Any() && EffectManager.Apply(player, this)) ;
            }
            return !PlayerList.Where(pl => pl.EffectQueue.Any()).Any(); // Есть ли у кого ещё эффекты
        }

        #endregion Methods
    }
}
