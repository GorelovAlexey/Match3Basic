using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualMove : FieldVisualCommand
    {
        private (int, int)[,] mask;
        private float animSpeed;

        public FieldVisualMove((int, int)[,] moveMask, float animSpeed = 1f) : base()
        {
            mask = moveMask.Clone() as (int, int)[,];
            this.animSpeed = animSpeed;
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            var maskMove = mask;
            var width = mask.GetLength(0);
            var height = mask.GetLength(1);

            var tokensRewriteLists = new List<FieldTokenVisual>[width, height];
            var tokensLeftMask = new bool[width, height];

            void SetNewToken(FieldTokenVisual t, int x, int y)
            {
                if (tokensRewriteLists[x, y] == null)
                    tokensRewriteLists[x, y] = new List<FieldTokenVisual>();

                tokensRewriteLists[x, y].Add(t);
            }

            var promises = new List<IPromise>();
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var (xMove, yMove) = maskMove[x, y];
                    if (xMove == 0 && yMove == 0) continue;

                    var yNew = y + yMove;
                    var xNew = x + xMove;

                    var moveToken = visualizer.GetToken(x, y);
                    SetNewToken(moveToken, xNew, yNew);
                    tokensLeftMask[x, y] = true;

                    var cellDistance = 0f;
                    if (xMove == 0) cellDistance = MathF.Abs(yMove);
                    else if (yMove == 0) cellDistance = MathF.Abs(xMove);
                    else cellDistance = MathF.Sqrt(xMove * xMove + yMove * yMove);

                    var moveTime = cellDistance * AnimationTimings.TOKEN_MOVE_TIME_PER_SQUARE * animSpeed;
                    promises.Add(visualizer.MoveVisualToken(moveToken, xNew, yNew, moveTime)); 
                }

            var lostTokens = new List<FieldTokenVisual>();
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var tokenLeft = tokensLeftMask[x, y];
                    var hasIncoming = tokensRewriteLists[x, y]?.Count > 0;

                    if (hasIncoming)
                    {
                        var incomingList = tokensRewriteLists[x, y];
                        for (var i = 0; i < incomingList.Count - 1; i++)
                            lostTokens.Add(incomingList[i]);

                        visualizer.SetToken(incomingList[^1], x, y); // last
                    }
                    else if (tokenLeft)
                    {
                        visualizer.SetToken(null, x, y);
                    }
                }

            Promise.All(promises).Then(() =>
            {
                ResolveFinishPromise();
                foreach (var match3VisualToken in lostTokens)
                    Object.Destroy(match3VisualToken.GameObject);

                Console.Write("!LOST_TOKENS: " + lostTokens.Count);
            });
        }
    }
}