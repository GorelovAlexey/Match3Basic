using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Engine.Commands
{
    public class MoveEvaluation
    {
        public int WillGetExtraTurns { get; private set; }
        public Dictionary<Match3Token, float> Collected { get; private set; }

        public MoveEvaluation((Dictionary<Match3Token, float> total, int[] matches) collected, Match3Game g)
        {
            var hasFreeTurn = g.Settings.freeTurnsEnabled && collected.Item2.Any(x => x >= g.Settings.tokensForFreeTurn);
            WillGetExtraTurns = hasFreeTurn ? 1 : 0;
            Collected = collected.total;
        }
    }
}