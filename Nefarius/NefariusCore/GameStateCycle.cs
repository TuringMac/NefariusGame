﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            {
                Debug.WriteLine("Game already starts");
                return;
            }

            pPlayer.Color = ColorDeck.Pop();
            PlayerList.Add(pPlayer);
        }

        public override void StartGame()
        {
            if (State != GameState.Init)
            {
                Debug.WriteLine("Start after Init");
                return;
            }

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
            {
                Debug.WriteLine("Turning after Working");
                return;
            }

            pPlayer.Action = pAction;

            if (CheckEverybodyDoAction())
                State++;
        }

        public override void Spying()
        {
            if (State != GameState.Spying)
            {
                throw new Exception("Spying after Scoring");
            }

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
            {
                Debug.WriteLine("Spy after Spying");
                return;
            }
            if (pPlayer.Action != GameAction.Spy)
            {
                Debug.WriteLine("Шпионаж не в свой ход");
                return;
            }

            if (pDestSpyPosition == pSourceSpyPosition)
            {
                Debug.WriteLine("Исходная позиция шпиона совпадает с конечной");
                return;
            }

            if (pPlayer.Spies.Count(s => s == pSourceSpyPosition) == 0) // Если нет шпионов в исходной позиции
            {
                Debug.WriteLine("Нет шпиона в исходной позиции");
                return;
            }

            for (int i = 0; i < pPlayer.Spies.Count(); i++)
            {
                if (pPlayer.Spies[i] == pSourceSpyPosition)
                {
                    //TODO взымать плату
                    pPlayer.Spies[i] = pDestSpyPosition;
                    break;
                }
            }

            pPlayer.Action = GameAction.None;
            if (CheckEverybodyDoSpy())
            {
                State++;
                if (CheckEverybodyDoInvent()) //TODO Move this to Ticker event listener
                    State++;
            }

            return;
        }

        public void Invent(Player pPlayer, Invention pInvention)
        {
            if (State != GameState.Invent)
            {
                Debug.WriteLine("Invent after Spy");
                return;
            }
            if (pPlayer.Action != GameAction.Invent)
            {
                Debug.WriteLine("Изобретение не в свой ход");
                return;
            }
            if (!pPlayer.Inventions.Contains(pInvention))
            {
                Debug.WriteLine("You haven't got this invention! Cheater?");
                return;
            }
            if (pPlayer.Coins < pInvention.Cost)
            {
                Debug.WriteLine("You haven't got enought coins");
                return;
            }

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
