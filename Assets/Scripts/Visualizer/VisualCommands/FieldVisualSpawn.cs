using System;
using System.Collections.Generic;
using Assets.Scripts.Engine;
using RSG;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualSpawn : FieldVisualCommand
    {
        private Direction direction;
        private Match3Token[,] mask;
        private float animTime;

        public FieldVisualSpawn(Match3Token[,] spawnItems, Direction dir, float animTime = 1f) : base()
        {
            mask = spawnItems.Clone() as Match3Token[,];
            direction = dir;
            this.animTime = animTime;
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            Debug.Log("SpawnANdFall");

            var width = mask.GetLength(0);
            var height = mask.GetLength(1);

            var horizontal = direction == Direction.Top || direction == Direction.Bot;
            var offsets = new int[horizontal ? width : height];

            var xStart = 0;
            var yStart = 0;
            var xChange = 1;
            var yChange = 1;
            var xMax = width - 1;
            var yMax = height - 1;

            if (direction == Direction.Left)
            {
                xStart = xMax;
                xMax = 0;
                xChange = -1;
            }

            if (direction == Direction.Top)
            {
                yStart = yMax;
                yMax = 0;
                yChange = -1;
            }

            var promises = new List<IPromise>();

            for (var x = xStart; x != xMax + xChange; x += xChange)
            {
                for (var y = yStart; y != yMax + yChange; y += yChange)
                {
                    if (mask[x, y] == Match3Token.Empty) continue;

                    var xSpawn = x;
                    var ySpawn = y;
                    var dist = 0;

                    switch (direction)
                    {
                        case Direction.Top:
                            ySpawn = -1 - offsets[x];
                            dist = y - ySpawn;
                            break;
                        case Direction.Right:
                            xSpawn = width + offsets[y];
                            dist = x - xSpawn;
                            break;
                        case Direction.Bot:
                            ySpawn = height + offsets[x];
                            dist = y - ySpawn;
                            break;
                        case Direction.Left:
                            xSpawn = -1 - offsets[y];
                            dist = x - xSpawn;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }

                    dist = Math.Abs(dist);

                    var tokenSpawn = visualizer.SpawnToken(mask[x, y], xSpawn, ySpawn);
                    visualizer.SetToken(tokenSpawn, x, y);

                    var p = visualizer.MoveVisualToken(tokenSpawn, x, y, dist * AnimationTimings.TOKEN_MOVE_TIME_PER_SQUARE * animTime);
                    promises.Add(p);

                    if (horizontal) offsets[x] += 1;
                    else offsets[y] += 1;
                }
            }

            Promise.All(promises).Then(ResolveFinishPromise);
        }


    }

}