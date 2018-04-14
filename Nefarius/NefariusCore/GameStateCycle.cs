using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NefariusCore
{
    public class GameStateCycle : Game
    {
        GameState _State = GameState.Init;

        public GameState State
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

        public GameStateCycle()
            : base(new List<Player>())
        {

        }

        public void AddPlayer(Player pPlayer)
        {
            if (State != GameState.Init)
            {
                Debug.WriteLine("Game already starts");
                return;
            }

            if (PlayerList.Count >= 6)
            {
                Debug.WriteLine("All seats are taken");
                return;
            }

            pPlayer.Color = ColorDeck.Pop();
            PlayerList.Add(pPlayer);
        }

        public override bool StartGame()
        {
            if (State != GameState.Init)
            {
                Debug.WriteLine("Start after Init");
                return false;
            }

            base.StartGame();

            State++;
            return true;
        }

        /// <summary>
        /// Выбирают действие на следующий ход
        /// </summary>
        /// <returns></returns>
        public bool Turning(Player pPlayer, GameAction pAction)
        {
            if (State != GameState.Turning)
            {
                Debug.WriteLine("Turning after Working");
                return false;
            }

            pPlayer.Action = pAction;

            if (CheckEverybodyDoAction())
                State++;
            return true;
        }

        public override bool Spying()
        {
            if (State != GameState.Spying)
            {
                Debug.WriteLine("Spying after Scoring");
                return false;
            }

            base.Spying();

            State++;
            return true;
        }

        public bool SetSpy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            if (State != GameState.Spy && State != GameState.Inventing)
            {
                Debug.WriteLine("Spy after Spying");
                return false;
            }

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
                    Debug.WriteLine("Шпионаж не в свой ход");
                    return false;
                }

                var result = pPlayer.SetSpy(pDestSpyPosition);
                if (result)
                {
                    pPlayer.Action = GameAction.None;
                    pPlayer.CurrentSetSpy = GameAction.None;
                }
                if (CheckEverybodyDoSpy())
                    State++;

                return result;
            }
            else if (State == GameState.Inventing)
            {
                //TODO Check top effect for drop spy requiring. Continue if not
                if (isSet && pPlayer.EffectQueue.Any() && pPlayer.EffectQueue.Peek().It == EffectItem.Spy && pPlayer.EffectQueue.Peek().Dir == EffectDirection.Get)
                    pPlayer.SetSpy(pDestSpyPosition);
                else if (isDrop && pPlayer.EffectQueue.Any() && pPlayer.EffectQueue.Peek().It == EffectItem.Spy && pPlayer.EffectQueue.Peek().Dir == EffectDirection.Drop)
                    pPlayer.DropSpy(pSourceSpyPosition);
                else // Move (not set, not drop)
                    return false;

                foreach (var player in PlayerList) // TODO Дублирует код из Inventing
                {
                    while (player.EffectQueue.Any() && EffectManager.Apply(player, this)) ;
                }

                if (CheckEverybodyApplyEffects())
                    State++;
                return true;
            }
            return false;
        }

        public bool Invent(Player pPlayer, Invention pInvention)
        {
            if (State != GameState.Invent && State != GameState.Inventing)
            {
                Debug.WriteLine("Invent after Spy");
                return false;
            }

            if (State == GameState.Invent)
            {
                pPlayer.PlayInvention(pInvention);

                if (CheckEverybodyDoInvent())
                    State++;
            }
            else if (State == GameState.Inventing)
            {
                //TODO Check top effect for drop card requiring. Continue if not
                pPlayer.DropInvention(pInvention);
                foreach (var player in PlayerList) // TODO Дублирует код из Inventing
                {
                    while (player.EffectQueue.Any() && EffectManager.Apply(player, this)) ;
                }

                if (CheckEverybodyApplyEffects())
                    State++;
            }

            return true;
        }

        public override bool Inventing()
        {
            if (State != GameState.Inventing)
                throw new Exception("Inventing after Invent");

            base.Inventing();

            if (CheckEverybodyApplyEffects())
            {
                State++;
                return true;
            }
            return false;
        }

        public override bool Researching()
        {
            if (State != GameState.Research)
                throw new Exception("Researching after Inventing");

            base.Researching();

            State++;
            return true;
        }

        public override bool Working()
        {
            if (State != GameState.Work)
                throw new Exception("Working after Researching");

            base.Working();

            State++;
            return true;
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

        #endregion Methods
    }
}
