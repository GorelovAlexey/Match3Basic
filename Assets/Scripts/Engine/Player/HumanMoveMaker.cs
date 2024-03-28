using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using RSG;

namespace Assets.Scripts.Engine.Player
{
    public class HumanMoveMaker : Match3PlayerMoveMaker
    {
        public override IPromise<Match3CommandMove> GetMovePromise(Match3Game field)
        {
            if (!CanMakeMove)
                lastMove = new Promise<Match3CommandMove>();

            return lastMove;
        }

        public void MakeAMove(Match3CommandMove m)
        {
            lastMove.Resolve(m);
        }
    }
}