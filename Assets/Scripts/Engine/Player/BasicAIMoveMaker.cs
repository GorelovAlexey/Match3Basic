using System.Linq;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using RSG;

namespace Assets.Scripts.Engine.Player
{
    public class BasicAIMoveMaker : Match3PlayerMoveMaker
    {
        public override IPromise<Match3CommandMove> GetMovePromise(Match3Game game)
        {
            if (!CanMakeMove)
                lastMove = new Promise<Match3CommandMove>();

            var moves = game.Field.FindBasicMoves();
            // Оцениваем каждый ход
            // Присваем числовую оценку оценке - сумма собраных токенов + 100 если получим доп ход
            var best = moves.Select(x => (x, x.EvaluateMove(game)))
                .OrderByDescending(x => x.Item2.WillGetExtraTurns * 100f + x.Item2.Collected.Values.Sum())
                .First().x;

            lastMove.Resolve(best);
            return lastMove;
        }
    }
}