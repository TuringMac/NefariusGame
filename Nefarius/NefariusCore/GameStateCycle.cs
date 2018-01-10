using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NefariusCore
{
    public class GameStateCycle : Game
    {
        GameState _State = GameState.Init;

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

        public GameStateCycle(LinkedList<Player> pPlayer)
            : base(pPlayer)
        {
            State = GameState.Turning;
        }

        /// <summary>
        /// Выбирают действие на следующий ход
        /// </summary>
        /// <returns></returns>
        public void Turning(Player pPlayer, GameAction pAction)
        {
            if (State != GameState.Turning)
                throw new Exception("Turning after Working");

            pPlayer.Action = pAction;

            if (IsEverybodyTurned())
                State++;
        }

        public override void Spying() //TODO
        {
            if (State != GameState.Spying)
                throw new Exception("Spying after Scoring");

            base.Spying();

            State++;
        }

        public void SetSpy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            if (State != GameState.Spy)
                throw new Exception("Spy after Spying");
            if (pPlayer.Action != GameAction.Spy)
                return;

            if (pDestSpyPosition == pSourceSpyPosition)
                return;

            if (pPlayer.Spies.Count(s => s == pSourceSpyPosition) == 0) // Если нет шпионов в исходной позиции
                return;

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
            return;
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

        public override void Researching()
        {
            if (State != GameState.Research)
                throw new Exception("Researching after Invent");

            base.Researching();

            State++;
        }

        public override void Working()
        {
            if (State != GameState.Work)
                throw new Exception("Working after Researching");

            base.Working();

            State++;
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

        #endregion Methods
    }
}
