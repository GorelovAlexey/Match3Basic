using System;
using System.Diagnostics.CodeAnalysis;
using Assets.Scripts.Engine.Moves;
using Assets.Scripts.Visualizer;
using System.Text.Json;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.Visualizer.VisualCommands;
using Newtonsoft.Json;

namespace Assets.Scripts.Engine
{
    [Serializable]
    public class Match3MoveBasicSwap : Match3Move
    {
        [JsonProperty]
        private int x0, y0, x1, y1;

        public Match3MoveBasicSwap(int x0, int y0, int x1, int y1)
        {
            this.x0 = x0; this.y0 = y0; 
            this.x1 = x1; this.y1 = y1;
        }

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
        }

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
    }
}