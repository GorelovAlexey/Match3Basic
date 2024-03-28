using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using RSG;

namespace Assets.Scripts.Engine.Player
{
    public abstract class Match3PlayerMoveMaker
    {
        protected Promise<Match3CommandMove> lastMove;
        public bool CanMakeMove => lastMove != null && lastMove.CurState == PromiseState.Pending;
        public void FinishLastMove()
        {
            lastMove = new Promise<Match3CommandMove>();
        }

        public abstract IPromise<Match3CommandMove> GetMovePromise(Match3Game field);
    }
}