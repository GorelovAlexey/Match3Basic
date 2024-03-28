using System;
using System.Linq;
using Assets.Scripts.Visualizer.VisualCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Engine.Commands
{
    public class Match3CommandMoveSwap : Match3CommandMove
    {
        public new const string TYPE_NAME = "MoveSwap";

        [JsonProperty]
        private int x0, y0, x1, y1;

        public Match3CommandMoveSwap(int x0, int y0, int x1, int y1)
        {
            this.x0 = x0; this.y0 = y0;
            this.x1 = x1; this.y1 = y1;
        }

        /*
        public override Match3Token[,] ApplyMove(Match3Token[,] field, out FieldVisualCommand command)
        {
            command = GetAnimation(field, x0, y0, x1, y1);
            field = field.Swap(x0, y0, x1, y1);
            return field;
        }

        public override Match3Token[,] RevertMove(Match3Token[,] field, out FieldVisualCommand command)
        {
            return ApplyMove(field, out command);
        }

        private FieldVisualCommand GetAnimation(Match3Token[,] field, int x0, int y0, int x1, int y1)
        {
            var moveX0 = x1 - x0;
            var moveY0 = y1 - y0;
            var moveMask = new (int, int)[field.GetLength(0), field.GetLength(1)];
            moveMask[x0, y0] = (moveX0, moveY0);
            moveMask[x1, y1] = (-moveX0, -moveY0);
            return new FieldVisualMove(moveMask);
        }*/

        public override (bool valid, bool possible) IsValidAndPossible(Match3Matcher matcher, Match3Token[,] field)
        {
            var width = field.GetLength(0);
            var height = field.GetLength(1);

            bool WithinWidth(int x) => x >= 0 && x < width;
            bool WithinHeight(int y) => y >= 0 && y < height;

            if (!WithinWidth(x0) || !WithinWidth(x1) || !WithinHeight(y0) || !WithinHeight(y1))
                return (false, false);

            var distX = Math.Abs(x0 - x1);
            var distY = Math.Abs(y0 - y1);

            if ((distX == 1 && distY == 0 || distY == 1 && distX == 0) == false)
                return (false, false);

            return (true, matcher.MatchesExistsNoClone(field, (x0, y0, field[x1, y1]), (x1, y1, field[x0, y0])));
        }

        public override string GetTypeName => TYPE_NAME;

        public override void Apply(Match3Game game)
        {
            game.Field.GetField.Swap(x0, y0, x1, y1);
        }

        public override FieldVisualCommand GetVisuals(Match3Game game)
        {
            var moveX0 = x1 - x0;
            var moveY0 = y1 - y0;
            var width = game.Field.Width;
            var height = game.Field.Height;
            var moveMask = new (int, int)[width, height];
            moveMask[x0, y0] = (moveX0, moveY0);
            moveMask[x1, y1] = (-moveX0, -moveY0);
            return new FieldVisualMove(moveMask);
        }

        public override JToken Serialize()
        {
            var obj = JObject.FromObject(this);
            obj[TYPE_TOKEN_NAME] = TYPE_NAME;

            return obj;
        }

        public override void LoadData(JToken obj)
        {
            x0 = obj.Value<int>("x0");
            x1 = obj.Value<int>("x1");
            y0 = obj.Value<int>("y0");
            y1 = obj.Value<int>("y1");
        }

        public override MoveEvaluation EvaluateMove(Match3Game g)
        {
            var clone = g.Field.GetFieldClone.Swap(x0, y0, x1, y1);
            var result = Match3Field.ClearAfterMove(clone, g.Matcher, out _);

            return new MoveEvaluation(result, g);
        }
    }
}