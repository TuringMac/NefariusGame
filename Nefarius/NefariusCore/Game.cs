using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NefariusCore
{
    public class Game
    {
        public Stack<Invention> InventDeck { get; private set; } = new Stack<Invention>();
        public List<Player> PlayerList { get; private set; }

        public Game(List<Player> pPlayers)
        {
            if (pPlayers.Count < 2 || pPlayers.Count > 6)
                ;// throw new Exception("Wrong Player count. Game for 2 - 6 players. Invite more players"); //TODO

            PlayerList = pPlayers;
            DeckFiller.Fill(InventDeck);
        }

        public void Run()
        {
            while (!HasWinner())
            {
                var TurnedPlayers = TurningAsync();

                Spying();

                foreach (var player in PlayerList.Where(p => p.Action == GameAction.Spy))
                {
                    //SetSpy(player, GameAction.None, GameAction.Spy); //TODO
                    Thread.Sleep(2000);
                }

                foreach (var player in PlayerList.Where(p => p.Action == GameAction.Invent))
                {
                    //Invent(player, null); //TODO
                    Thread.Sleep(2000);
                }

                Inventing();

                Researching();

                Working();
            }
        }

        /// <summary>
        /// Выбирают действие на следующий ход
        /// </summary>
        /// <returns></returns>
        public async Task TurningAsync()
        {
            var getUserTasks = new List<Task>();

            foreach (var player in PlayerList)
            {
                getUserTasks.Add(Task.Run(() => player.Turn()));
                //Turning(player, GameAction.None); //TODO
                //Thread.Sleep(2000);
            }

            await Task.WhenAll(getUserTasks);
            return;
        }

        public virtual void Spying()
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
        }

        /// <summary>
        /// Собирает эффекты разыгранных изобретений и применяет их на игроках
        /// </summary>
        public virtual void Inventing()
        {
            foreach (var player in PlayerList) // Эффекты по часовой стрелке
            {
                foreach (var inventor in PlayerList) //TODO Reverse
                {
                    if (inventor.CurrentInvention == null) continue;

                    foreach (var effect in inventor.CurrentInvention.OtherEffectList)
                    {
                        player.EffectQueue.Enqueue(effect);
                    }
                }
                while (player.EffectQueue.Count > 0)
                {
                    var eff = player.EffectQueue.Dequeue();
                    eff.Apply(player);
                }
            }
        }

        public virtual void Researching()
        {
            foreach (var player in PlayerList)
            {
                if (player.Action != GameAction.Research)
                    continue;

                player.Coins += 5;
                player.Inventions.Append(InventDeck.Peek());
                player.Action = GameAction.None;
            }
        }

        public virtual void Working()
        {
            foreach (var player in PlayerList)
            {
                if (player.Action != GameAction.Work)
                    continue;

                player.Coins += 10;
                player.Action = GameAction.None;
            }
        }

        #region Methods

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

        #endregion Methods
    }
}
