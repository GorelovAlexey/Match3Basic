using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.Controls
{
    public abstract class BasicProgressBar : MonoBehaviour
    {
        public float progressUpdateTime = .33f;

        protected ReactiveProperty<float> ProgressDisplay { get; } = new ReactiveProperty<float>();
        protected ReactiveProperty<float> ProgressTarget { get; } = new ReactiveProperty<float>();

        private float _progress;
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = Mathf.Clamp(value, 0f, 1f);
                ProgressTarget.Value = _progress;
            }
        }

        public void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            Init();
        }


        private IDisposable updateDisplayDisposable;
        private IDisposable onProgressUpdateDisposable;
        private Tween updateDisplayTween;
        protected void Init()
        {
            ProgressTarget.Value = _progress;
            ProgressDisplay.Value = ProgressTarget.Value;

            updateDisplayDisposable?.Dispose();
            updateDisplayDisposable = ProgressTarget.Subscribe(UpdateDisplay);
            updateDisplayDisposable.AddTo(gameObject);

            onProgressUpdateDisposable?.Dispose();
            onProgressUpdateDisposable = ProgressDisplay.StartWith(ProgressDisplay.Value).Subscribe(OnProgressUpdate).AddTo(gameObject);
        }

        private void UpdateDisplay(float f)
        {
            if (updateDisplayTween != null && updateDisplayTween.active)
            {
                var duration = updateDisplayTween.Duration();
                duration /= .8f;
                if (updateDisplayTween.fullPosition < duration)
                {
                    // TODO, в данном методе постоянно скачет скорость, нужна более плавная 
                    updateDisplayTween.target = f;
                    return;
                }
            }

            updateDisplayTween?.Kill();
            updateDisplayTween =
                DOTween.To(() => ProgressDisplay.Value, x => ProgressDisplay.Value = x, f, progressUpdateTime)
                    .SetLink(gameObject);
        }

        public IDisposable SubscribeToProgress(IObserver<float> onProgress)
        {
            return ProgressDisplay.Subscribe(onProgress);
        }

        public abstract void OnProgressUpdate(float f);
    }
}