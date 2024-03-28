using System;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Visualizer.VisualCommands;

namespace Assets.Scripts.Engine.Commands
{
    public class Match3CommandTokensSpawnFall : Match3GameCommand
    {
        private Match3Token[,] FallDown(Match3Token[,] field, Direction dir, out (int, int)[,] moveMatrix)
        {
            var width = field.GetLength(0);
            var height = field.GetLength(1);

            moveMatrix = new (int, int)[width, height];

            var outter = 0;
            var verticalFall = dir == Direction.Bot || dir == Direction.Top;
            var maxOutter = verticalFall ? width : height;

            Func<int, (int x, int y)> coordsGetter;
            if (verticalFall) coordsGetter = v => (outter, v);
            else coordsGetter = v => (v, outter);

            Func<int, int> moveInnerUpstream;
            if (dir == Direction.Top || dir == Direction.Left) moveInnerUpstream = v => v - 1;
            else moveInnerUpstream = v => v + 1;

            var fallFrom = 0;
            var fallTo = 0;
            var fallLast = 0;
            void ResetFrom()
            {
                switch (dir)
                {
                    case Direction.Top:
                        fallLast = -1;
                        fallTo = height - 1;
                        break;
                    case Direction.Left:
                        fallLast = -1;
                        fallTo = width - 1;
                        break;
                    case Direction.Right:
                        fallLast = width;
                        fallTo = 0;
                        break;
                    case Direction.Bot:
                        fallLast = height;
                        fallTo = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
                }
                fallFrom = moveInnerUpstream.Invoke(fallTo);
            }

            for (outter = 0; outter < maxOutter; outter++)
            {
                ResetFrom();
                while (fallFrom != fallLast)
                {
                    var (x, y) = coordsGetter.Invoke(fallFrom);
                    var from = field[x, y];

                    var (x0, y0) = coordsGetter.Invoke(fallTo);
                    var to = field[x0, y0];

                    if (to != Match3Token.Empty)
                    {
                        fallTo = moveInnerUpstream.Invoke(fallTo);
                    }
                    else if (to == Match3Token.Empty && from != Match3Token.Empty)
                    {
                        field[x0, y0] = from;
                        field[x, y] = Match3Token.Empty;

                        moveMatrix[x, y] = (x0 - x, y0 - y);

                        fallTo = moveInnerUpstream.Invoke(fallTo);
                    }

                    fallFrom = moveInnerUpstream.Invoke(fallFrom);
                }
            }

            return field;
        }
        private Match3Token[,] SpawnNewTokens(Match3Token[,] field, Match3TokenGenerator generator, out Match3Token[,] tokensMask)
        {
            tokensMask = new Match3Token[field.GetLength(0), field.GetLength(1)];

            var needTokens = 0;
            for (var x = 0; x < field.GetLength(0); x++)
            for (var y = 0; y < field.GetLength(1); y++)
                if (field[x, y] == Match3Token.Empty)
                    needTokens++;

            var tokens = generator.GetTokens(needTokens);

            for (var x = 0; x < field.GetLength(0); x++)
            for (var y = 0; y < field.GetLength(1); y++)
            {
                if (field[x, y] != Match3Token.Empty || needTokens <= 0) continue;
                needTokens--;
                field[x, y] = tokens[needTokens];
                tokensMask[x, y] = tokens[needTokens];
            }

            return field;
        }

        private (int, int)[,] visualsMoveMatrix;
        private Match3Token[,] spawnTokensMask;
        private float animSpeed;

        public Match3CommandTokensSpawnFall(float animSpeed)
        {
            this.animSpeed = animSpeed;
        }

        public override void Apply(Match3Game g)
        {
            var field = g.Field.GetFieldClone;
            field = FallDown(field, g.TokensSpawnDirection, out visualsMoveMatrix);
            field = SpawnNewTokens(field, g.TokenGenerator, out spawnTokensMask);
            g.Field.field = field;
        }

        public override FieldVisualCommand GetVisuals(Match3Game g)
        {
            var fallDownAnim = new FieldVisualMove(visualsMoveMatrix, animSpeed);
            var spawnAndFall = new FieldVisualSpawn(spawnTokensMask, g.TokensSpawnDirection, animSpeed);

            return new FieldVisualCommandAll(fallDownAnim, spawnAndFall);
        }
    }
}