using System;
using DG.Tweening;
using DG.Tweening.Core;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.UI.WindowsManagerSystem
{
    // TODO Дженерик не совсем так работает как планировалось, переделать?
    public class WindowsTweenAnimationCreator<T> where T: Window 
    {
        protected T window;
        public WindowsTweenAnimationCreator([NotNull] T window) 
        {
            this.window = window;
            if (window is null) 
                throw new Exception($"[{GetType().Name}] window is null");
        }

        protected Tween CreateAlphaWindowTween(float end, float duration, float? changeStartValue = null)
        {
            DOGetter<float> getter = () => window.Alpha.Value;
            DOSetter<float> setter = f => window.Alpha.Value = f;

            var tween = DOTween.To(getter, setter, end, duration).SetUpdate(true).SetLink(window.gameObject);

            if (changeStartValue.HasValue)
                tween.ChangeStartValue(changeStartValue.Value);

            return tween;
        }

        protected Tween CreateScaleWindowTween(float end, float duration, float? changeStartValue = null)
        {
            var tween = window.content.transform.DOScale(end, duration).SetUpdate(true).SetLink(window.gameObject);

            if (changeStartValue.HasValue)
            {
                var v = changeStartValue.Value;
                var scale = new Vector3(v, v, v);
                tween.ChangeStartValue(scale);
            }

            return tween;
        }

        public virtual Tween CreateAppearAnimation()
        {
            var seq = DOTween.Sequence();
            var scaleDur = .35f;
            var alphaDur = .25f;

            seq.SetUpdate(true);
            seq.Insert(0, CreateScaleWindowTween(1f, scaleDur, 0).SetEase(Ease.OutBack));
            seq.Insert(0, CreateAlphaWindowTween(1f, alphaDur, 0));
            seq.SetLink(window.gameObject);

            return seq;
        }
        public virtual Tween CreateCloseAnimation()
        {
            var seq = DOTween.Sequence();
            var duration = .35f;

            seq.SetUpdate(true);
            seq.Insert(0, CreateScaleWindowTween(0, duration));
            seq.Insert(0, CreateAlphaWindowTween(0, duration));
            seq.SetLink(window.gameObject);

            return seq;
        }
        public virtual Tween CreateMinimizeAnimation()
        {
            return CreateAlphaWindowTween(0, .35f);

        }
        public virtual Tween CreateMaximizeAnimation()
        {
            return CreateAlphaWindowTween(1f, .35f);
        }
    }
}