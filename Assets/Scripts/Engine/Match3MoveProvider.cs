using JetBrains.Annotations;

namespace Assets.Scripts.Engine
{
    public abstract class Match3MoveProvider
    {
        public Match3Game Game { get; protected set; }
        
        public bool IsTurnInProgress { get; protected set; }

        protected Match3MoveProvider(Match3Game game)
        {
            Game = game;
        }

        public void ProgressTurn()
        {
            if (!Game.IsActive)
                return;

            if (IsTurnInProgress)
                return;

            IsTurnInProgress = true;

            ProgressTurnInner();
        }

        protected abstract void ProgressTurnInner();
    }
}