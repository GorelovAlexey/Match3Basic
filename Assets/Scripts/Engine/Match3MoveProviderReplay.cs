using System.Collections.Generic;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using RSG;
using UnityEngine.LowLevel;

namespace Assets.Scripts.Engine
{
    public class Match3MoveProviderReplay : Match3MoveProvider
    {
        private int currentTurn = 0;
        private Match3Replay activeReplay;

        public Match3MoveProviderReplay(Match3Replay replay, Match3Game game) : base(game)
        {
            activeReplay = replay;
        }

        public void Restart()
        {
            currentTurn = 0;
            //TODO RESET BOARD
        }

        protected override void ProgressTurnInner()
        {
            Match3ReplayTurn GetTurn()
            {
                if (currentTurn >= activeReplay.turns.Count)
                    return null;

                return activeReplay.turns[currentTurn++];
            }

            var turn = GetTurn();
            if (turn == null)
                return;

            var player = Game.PlayersManager.CurrentPlayer;

            turn.Reset();
            var move = turn.AdvanceTillMove();
            if (move == null)
            {
                OnVisualsDone();
                return;
            }

            Game.MakeMove(player, move, turn).Then(OnVisualsDone);

            void OnVisualsDone()
            {
                Game.PlayersManager.AdvanceTurn();
                IsTurnInProgress = false;
            }
        }
    }
}