using System;
using DG.Tweening;
using RSG;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualPause : FieldVisualCommand
    {
        private float time;
        public FieldVisualPause(float timeSec)
        {
            time = timeSec;
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            IDisposable disposable = null;
            disposable = Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(_ =>
            {
                ResolveFinishPromise();
                disposable?.Dispose();
            });

           // DOVirtual.DelayedCall(time, ResolveFinishPromise);
        }
    }
}