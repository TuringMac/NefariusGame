using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NefariusCore
{
    public class GameStateCycle : Game
    {
        public static GameStateCycle _Game = null;
        public static GameStateCycle Instance
        {
            get
            {
                if (_Game == null) _Game = new GameStateCycle();
                return _Game;
            }
        }

        GameState _State = GameState.Init;

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
                    if (StateChanged != null)
                        StateChanged(this, new StateEventArgs() { State = State });
                }
            }
        }

        public GameStateCycle()
            : base(new List<Player>())
        {

        }

        public void AddPlayer(Player pPlayer)
        {
            if (State != GameState.Init)
                throw new Exception("Game already starts");
            pPlayer.Color = ColorDeck.Pop();
            PlayerList.Add(pPlayer);
        }

        public override void StartGame()
        {
            if (State != GameState.Init)
                throw new Exception("Start after Init");

            base.StartGame();

            State++;
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

            if (CheckEverybodyDoAction())
                State++;
        }

        public override void Spying()
        {
            if (State != GameState.Spying)
                throw new Exception("Spying after Scoring");

            base.Spying();

            State++;
            if (CheckEverybodyDoSpy())
            {
                State++;
                if (CheckEverybodyDoInvent())
                {
                    State++;
                }
            }
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
                    //TODO взымать плату
                    break;
                }
            }
            pPlayer.Action = GameAction.None;
            if (CheckEverybodyDoSpy())
                State++;
            if (CheckEverybodyDoInvent()) //TODO Move this to Ticker event listener
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
            if (pPlayer.Coins < pInvention.Cost)
                throw new Exception("You haven't got enought coins");

            pPlayer.CurrentInvention = pInvention;
            pPlayer.Inventions.Remove(pInvention);
            pPlayer.PlayedInventions.Add(pInvention);
            pPlayer.Coins -= pPlayer.CurrentInvention.Cost;
            foreach (var effect in pPlayer.CurrentInvention.SelfEffectList) // Эффект на себя
            {
                pPlayer.EffectQueue.Enqueue(effect);
            }

            pPlayer.Action = GameAction.None;

            if (CheckEverybodyDoInvent())
                State++;
            return true;
        }

        public override void Inventing()
        {
            if (State != GameState.Inventing)
                throw new Exception("Inventing after Invent");

            base.Inventing();

            State++;
        }

        public override void Researching()
        {
            if (State != GameState.Research)
                throw new Exception("Researching after Inventing");

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

        public override bool Scoring()
        {
            if (State != GameState.Scoring)
                throw new Exception("Scoring after Working");

            bool isWin = base.Scoring();
            if (isWin)
                State++;
            else
                State = GameState.Turning;
            return isWin;
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
            return !PlayerList.Where(player => player.Action == GameAction.Spy).Any();
        }

        bool CheckEverybodyDoInvent()
        {
            return !PlayerList.Where(player => player.Action == GameAction.Invent).Any();
        }

        #endregion Methods
    }
}
