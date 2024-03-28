using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using Assets.Scripts.Engine.Player;
using RSG;
using UniRx;

namespace Assets.Scripts.Engine
{
    public class PlayersManager
    {
        public List<Match3Player> players { get; private set; }
        public Match3Player CurrentPlayer => players[currentPlayer];

        public ReactiveProperty<Match3Player> CurrentPlayerReactive = new ReactiveProperty<Match3Player>(null);

        private int currentPlayerTurns;
        private int currentPlayer;
        private bool currentPlayerFreeTurn;

        private Match3GameSettings settings;

        public PlayersManager(Match3GameSettings settings,  IEnumerable<Match3Player> players)
        {
            this.players = players.ToList();
            currentPlayer = 0;
            this.settings = settings;
            CurrentPlayerReactive.Value = CurrentPlayer;

            ResetPlayerTurns();
        }

        private void ResetPlayerTurns()
        {
            currentPlayerTurns = settings.turnsPerPlayer;
            currentPlayerFreeTurn = false;
        }

        public IPromise<(Match3Player player, Match3CommandMove move)> GetAMove(Match3Game g)
        {
            var player = CurrentPlayer;
            var promise = new Promise<(Match3Player, Match3CommandMove)>();
            player.GetAMove(g).Then(x => promise.Resolve((player, x)));

            return promise;
        }

        public void AdvanceTurn()
        {
            if (currentPlayerFreeTurn)
            {
                currentPlayerFreeTurn = false;
                return;
            }

            currentPlayerTurns--;
            if (currentPlayerTurns <= 0)
            {
                currentPlayer++;
                if (currentPlayer >= players.Count)
                    currentPlayer = 0;

                ResetPlayerTurns();
                CurrentPlayerReactive.Value = CurrentPlayer;
            }
        }

        public void InspectReward(IEnumerable<int> tokensCounts)
        {
            if (settings.freeTurnsEnabled == false)
                return;

            foreach (var count in tokensCounts)
            {
                if (count >= settings.tokensForFreeTurn)
                    currentPlayerFreeTurn = true;
            }
        } 
    }
}