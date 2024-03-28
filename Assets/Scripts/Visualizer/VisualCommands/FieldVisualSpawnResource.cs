using Assets.Scripts.Engine;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.Utils;
using DG.Tweening;
using RSG;
using UnityEngine;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualSpawnResource : FieldVisualCommand
    {
        private (Match3Token, float)[,] valueMask;
        private float animTime;
        private Match3Player player;

        public FieldVisualSpawnResource(Match3Player player, (Match3Token, float)[,] valueMask, float animTime = 1f) : base()
        {
            this.valueMask = valueMask.Clone() as (Match3Token, float)[,];
            this.animTime = animTime;
            this.player = player;
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            ResolveFinishPromise();

            for (int x = 0; x < valueMask.GetLength(0); x++)
            {
                for (int y = 0; y < valueMask.GetLength(1); y++)
                {
                    if (valueMask[x, y].Item1 == Match3Token.Empty)
                        continue;

                    var token = valueMask[x, y].Item1;
                    var amount = valueMask[x, y].Item2;

                    var endPos = visualizer.PlayerPanel.GetPlayerResourcePosition(player, valueMask[x, y].Item1);
                    var startPos = visualizer.GetCellPosition(x, y);


                    var c1 = new Vector3(startPos.x, endPos.y, (startPos.z + endPos.z)/2);

                    var tokenParticle = visualizer.SpawnResourceToken(valueMask[x, y].Item1, x, y);

                    var flyTime = AnimationTimings.RESOURCE_FLOAT_TIME * animTime;

                    tokenParticle.DOScale(0, flyTime).SetEase(Ease.InQuad).SetLink(tokenParticle.gameObject);
                    tokenParticle.Bezier2Tween(c1, endPos, flyTime, true, Ease.OutCubic)
                        .SetLink(tokenParticle.gameObject).OnComplete(() =>
                    {
                        Object.Destroy(tokenParticle.gameObject);
                        player.Collect(token, 0, amount);
                    });

                }
            }
        }
    }
}