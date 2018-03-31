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

        public override void Spying()
        {
            if (State != GameState.Spying)
            {
                throw new Exception("Spying after Scoring");
            }

            base.Spying();

            State++;
        }

        public bool SetSpy(Player pPlayer, GameAction pDestSpyPosition, GameAction pSourceSpyPosition = GameAction.None)
        {
            if (State != GameState.Spy && State != GameState.Inventing)
            {
                Debug.WriteLine("Spy after Spying");
                return false;
            }
            if (pPlayer.Action != GameAction.Spy)
            {
                Debug.WriteLine("Шпионаж не в свой ход");
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
                // Is set
                // Has spy in source
                // Has money to dest
                if (isSet)
                {
                    for (int i = 0; i < pPlayer.Spies.Count(); i++)
                    {
                        if (pPlayer.Spies[i] == pSourceSpyPosition)
                        {
                            switch (pDestSpyPosition)
                            {
                                case GameAction.Spy: break;
                                case GameAction.Invent:
                                    if (pPlayer.Coins >= 2)
                                        pPlayer.DropCoins(2);
                                    else
                                    {
                                        Debug.WriteLine("Not enought coins to spy");
                                        return false;
                                    }
                                    break;
                                case GameAction.Research: break;
                                case GameAction.Work:
                                    if (pPlayer.Coins >= 1)
                                        pPlayer.DropCoins(1);
                                    else
                                    {
                                        Debug.WriteLine("Not enought coins to spy");
                                        return false;
                                    }
                                    break;
                            }
                            pPlayer.Spies[i] = pDestSpyPosition;
                            pPlayer.Action = GameAction.None;
                            if (CheckEverybodyDoSpy())
                                State++;
                            return true;
                        }
                    }
                    Debug.WriteLine("No spy in source location");
                    return false;
                }
                else
                {
                    Debug.WriteLine("In Spy state only set spy");
                    return false;
                }
            }
            else if (State == GameState.Inventing)
            {
                //TODO
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
                pPlayer.DropInvention(pInvention);

                if (CheckEverybodyApplyEffects())
                    State++;
            }

            return true;
        }

        public override void Inventing()
        {
            if (State != GameState.Inventing)
                throw new Exception("Inventing after Invent");

            base.Inventing();

            if (CheckEverybodyApplyEffects())
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
