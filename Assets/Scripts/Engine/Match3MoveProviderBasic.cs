using RSG;

namespace Assets.Scripts.Engine
{
    public class Match3MoveProviderBasic : Match3MoveProvider
    {
        private PlayersManager playerManager => Game.PlayersManager;
        public Match3MoveProviderBasic(Match3Game game) : base(game)
        {
        }

        protected override void ProgressTurnInner()
        {
            var movePromise = playerManager.GetAMove(Game);

            var p = new Promise();
            movePromise.Then(result =>
            {
                var (player, move) = result;
                Game.MakeMove(player, move).Then(OnVisualsDone);
            });

            void OnVisualsDone()
            {
                Game.PlayersManager.AdvanceTurn();
                IsTurnInProgress = false;
            }
        }
    }
}