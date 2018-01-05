using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NefariusCore
{
    public class Game
    {
        GameState _State = GameState.Init;

        Stack<Invention> InventDeck { get; set; } = new Stack<Invention>();
        LinkedList<Player> PlayerList { get; set; }
        GameState State
        {
            get
            {
                return _State;
            }
            set
            {
                if (_State != value)
                {
                    _State = value;
                    if (StateChanged != null)
                        StateChanged(this, new StateEventArgs() { State = State });
                }
            }
        }

        public Game(LinkedList<Player> pPlayers)
        {
            if (pPlayers.Count < 2 || pPlayers.Count > 6)
                throw new Exception("Wrong Player count. Game for 2 - 6 players. Invite more players");

            PlayerList = pPlayers;
            DeckFiller.Fill(InventDeck);
            State = GameState.Turning;
        }

        /// <summary>
        /// Выбирают действие на следующий ход
        /// </summary>
        /// <returns></returns>
        public bool Turning(Player pPlayer, GameAction pAction)
        {
            if (State != GameState.Turning)
                throw new Exception("Турнинг только после инит или скоринга");

            pPlayer.Action = pAction;
            if (IsEverybodyTurned())
                State++;
            return true;
        }

        public void Spying() //TODO
        {
            if (State != GameState.Spying)
                throw new Exception("Spying after Scoring");

            // Если справа или слева от игрока со шпионом разыграли действия
            var cur = PlayerList.First;
            Player prev = cur.Previous.Value;
            Player next = cur.Next.Value;
            foreach (var spy in cur.Value.Spies)
            {
                if (spy == prev.Action || spy == next.Action)
                    cur.Value.Coins += 1; // По монетке за шпиона
            }
            State++;
        }

        public bool SetSpy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            if (State != GameState.Spy)
                throw new Exception("Spy after Spying");
            if (pPlayer.Action != GameAction.Spy)
                return false;

            if (pDestSpyPosition == pSourceSpyPosition)
                return false;

            if (pPlayer.Spies.Count(s => s == pSourceSpyPosition) == 0) // Если нет шпионов в исходной позиции
                return false;

            for (int i = 0; i < pPlayer.Spies.Count(); i++)
            {
                if (pPlayer.Spies[i] == pSourceSpyPosition)
                {
                    pPlayer.Spies[i] = pDestSpyPosition;
                    break;
                }
            }
            pPlayer.Action = GameAction.None;
            if (IsEverybodyTurned())
                State++;
            return true;
        }

        public bool Invent(Player pPlayer, Invention pInvention)
        {
            if (State != GameState.Invent)
                throw new Exception("Invent after Spy");
            if (pPlayer.Action != GameAction.Invent)
                return false;
            if (!pPlayer.Inventions.Contains(pInvention))
                throw new Exception("You haven't got this invention! Cheater?");

            var cur = PlayerList.First;
            /// TODO Сначала эффект на себя, потом по часовой стрелке

            pPlayer.Action = GameAction.None;
            if (IsEverybodyTurned())
                State++;
            return true;
        }

        public void Researching()
        {
            if (State != GameState.Research)
                throw new Exception("Researching after Invent");
            foreach (var player in PlayerList)
            {
                if (player.Action != GameAction.Research)
                    continue;

                player.Coins += 5;
                player.Inventions.Append(InventDeck.Peek());
                player.Action = GameAction.None;
            }
            State++;
        }

        public void Working()
        {
            if (State != GameState.Work)
                throw new Exception("Working after Researching");
            foreach (var player in PlayerList)
            {
                if (player.Action != GameAction.Work)
                    continue;

                player.Coins += 10;
                player.Action = GameAction.None;
            }
            State++;
        }

        public void Scoring()
        {
            if (State != GameState.Scoring)
                throw new Exception("Scoring after Working");
            Player winner = null;
            foreach (var player in PlayerList)
            {
                if (IsWinner(player))
                {
                    if (winner == null)
                    {
                        winner = player;
                    }
                    else
                    {
                        // 2 или более игрока набрали больше 20
                        State = GameState.Turning;
                    }
                }
            }
            if (winner != null)
            {
                // Победитель!
                State = GameState.Win;
            }
            // Играем дальше
            State = GameState.Turning;
        }

        #region Events

        public event EventHandler<StateEventArgs> StateChanged;

        #endregion Events

        #region Methods

        bool IsEverybodyTurned()
        {
            switch (State)
            {
                case GameState.Spy: return CheckEverybodyDoSpy();
                case GameState.Invent: return CheckEverybodyDoInvent();
                case GameState.Turning: return CheckEverybodyDoAction();
                default: return true; // TODO может исключение?
            }
        }

        bool CheckEverybodyDoAction()
        {
            foreach (var player in PlayerList)
            {
                if (player.Action == GameAction.None)
                    return false;
            }
            return true;
        }

        bool CheckEverybodyDoSpy()
        {
            return true;
        }

        bool CheckEverybodyDoInvent()
        {
            return true;
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

        #endregion Methods
    }
}
