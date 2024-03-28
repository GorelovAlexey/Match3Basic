using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

namespace Assets.Scripts.Engine.Commands
{
    public abstract class Match3CommandMove : Match3GameCommand, ISavableCommand
    {
        public abstract (bool valid, bool possible) IsValidAndPossible(Match3Matcher matcher, Match3Token[,] field);
        public abstract string GetTypeName { get; }
        public abstract JToken Serialize();
        public abstract void LoadData(JToken obj);
        public abstract MoveEvaluation EvaluateMove(Match3Game g);

        /*

    public (Dictionary<Match3Token, float>, int[]) GetAMoveValue(Match3CommandMove m)
    {
        var clone = field.Clone() as Match3Token[,];

        m.Apply();
        m.ApplyMove(clone, out _);

        return ClearAfterMove(clone, matcher, out _);
    }
         */
    }
}