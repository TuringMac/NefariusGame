using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NefariusCore
{
    public class Game
    {
        GameState _State;

        Stack<Invention> InventDeck { get; set; } = new Stack<Invention>();
        LinkedList<Player> PlayerList { get; set; } = new LinkedList<Player>();
        GameState State
        {
            get
            {
                return _State;
            }
            set
            {
                if (value != _State)
                {
                    value = _State;
                    if (StateChanged != null)
                        StateChanged(this, new StateEventArgs() { State = State });
                }
            }
        }

        public Game(Player[] pPlayers)
        {
            if (pPlayers.Count() != 6)
                throw new Exception("Wrong Player positions. Complete 6 places with 'null's");
            if (pPlayers.Count(p => p != null) < 2)
                throw new Exception("Wrong Player count. Game for 2 - 6 players. Invite more players");

            DeckFiller.Fill(InventDeck);
            State = GameState.Init;
        }

        public void Scoring()
        {
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
                        State++;
                    }
                }
            }
            if (winner != null)
            {
                // Победитель!
                State = GameState.Win;
            }
            // Играем дальше
            State++;
        }

        public void Spying() //TODO
        {
            if (State != GameState.Scoring)
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

        /// <summary>
        /// Установка шпиона
        /// </summary>
        /// <param name="pPlayer">Игрок</param>
        /// <param name="pAction">Куда ставить шпиона</param>
        /// <returns>Удалось ли поставить шпиона или нет</returns>
        public bool SetSpy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
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
            if (IsEverybodyTurned())
                State++;
            return true;
        }

        /// <summary>
        /// Розыгрыш изобретения
        /// </summary>
        /// <param name="pPlayer">Игрок</param>
        /// <param name="pInvention">Разыгрываемое изобретение</param>
        /// <returns>Разыграно ли</returns>
        public bool Invent(Player pPlayer, Invention pInvention)
        {
            if (!pPlayer.Inventions.Contains(pInvention))
                throw new Exception("You haven't got this invention! Cheater?");

            var cur = PlayerList.First;
            /// TODO Сначала эффект на себя, потом по часовой стрелке

            return pPlayer.Inventions.Contains(pInvention); //TODO
        }

        /// <summary>
        /// Исследование
        /// </summary>
        /// <param name="pPlayer">Игрок</param>
        /// <returns>Удалось ли</returns>
        public bool Research(Player pPlayer)
        {
            pPlayer.Coins += 5;
            pPlayer.Inventions.Append(InventDeck.Peek());
            return true;
        }

        /// <summary>
        /// Работа
        /// </summary>
        /// <param name="pPlayer"></param>
        /// <returns>Удалось ли</returns>
        public bool Work(Player pPlayer)
        {
            pPlayer.Coins += 10;
            return true;
        }

        /// <summary>
        /// Выбирают действие на следующий ход
        /// </summary>
        /// <returns></returns>
        public bool ChooseAction(Player pPlayer, GameAction pAction)
        {
            pPlayer.Action = pAction;
            return true;
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
