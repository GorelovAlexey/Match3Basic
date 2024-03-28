using System;
using System.Collections.Generic;
using RSG;
using UnityEngine;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualHide : FieldVisualCommand
    {
        private bool[,] mask;
        private float animSpeed;

        public FieldVisualHide(bool[,] hideAnimMask, float animSpeed = 1f)
        {
            mask = hideAnimMask.Clone() as bool[,];
            this.animSpeed = animSpeed;
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            var promises = new List<IPromise>();
            for (var x = 0; x < mask.GetLength(0); x++)
                for (var y = 0; y < mask.GetLength(1); y++)
                    if (mask[x, y])
                        promises.Add(visualizer.RemoveTokenAnimated(x, y, animSpeed));
            FinishPromise.Then(() => Debug.Log("hide done"));
            
            Promise.All(promises).Then(ResolveFinishPromise);
        }
    }
}