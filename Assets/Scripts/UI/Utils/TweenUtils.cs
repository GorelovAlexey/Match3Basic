using System;
using Assets.Scripts.UI.Utils;
using DG.Tweening;
using RSG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public static class TweenUtils
    {
        public static Sequence AddAppearAnimation(this CanvasGroup target, Vector3 endScale, float duration = .5f, Ease ease = Ease.OutCubic, bool playImmediately = true)
        {
            var seq = target.GetScaleFadeAnim(new Vector3(0f, 0f, 0f), endScale, 0f, 1f, duration).SetEase(ease);

            if (playImmediately) seq.Play();
            return seq;
        }

        private static Sequence GetScaleFadeAnim(CanvasGroup target, float duration, bool appear = true)
        {
            var start = appear ? 0f : 1f;
            var end = appear ? 1f : 0f;

            return DOTween.Sequence()
                .Insert(0, target.DOFade(end, duration).ChangeStartValue(start))
                .Insert(0, target.transform.DOScale(end, duration).ChangeStartValue(start))
                .SetLink(target.gameObject)
                .SetAutoKill(false);
        }

        public static Sequence AddAppearAnimation(this CanvasGroup target, Vector3 start, Vector3 finish, float duration = .5f, Ease ease = Ease.OutCubic, bool playImmediately = true)
        {
            var seq = target.GetScaleFadeAnim(start, finish, 0, 1, duration).SetEase(ease);

            if (playImmediately) seq.Play();
            return seq;
        }

        public static Sequence AddAppearAnimation(this CanvasGroup target, float start, float finish,
            float duration = .5f, Ease ease = Ease.OutCubic, bool playImmediately = true)
            => target.AddAppearAnimation(new Vector3(start, start, start), new Vector3(finish, finish, finish), duration, ease, playImmediately);

        private static Sequence GetScaleFadeAnim(this CanvasGroup target, Vector3 startScale, Vector3 finishScale, float startAlpha, float finishAlpha, float duration)
        {
            return DOTween.Sequence()
                .Insert(0, target.DOFade(finishAlpha, duration).ChangeStartValue(startAlpha))
                .Insert(0, target.transform.DOScale(finishScale, duration).ChangeStartValue(startScale))
                .SetLink(target.gameObject);
        }

        public static Sequence GetScaleFadeAnim(this CanvasGroup target, float finish, float duration, float? changeStartValue = null, Ease ease = Ease.OutCubic)
        {
            if (changeStartValue != null)
                return DOTween.Sequence()
                    .Insert(0, target.DOFade(finish, duration).ChangeStartValue(changeStartValue.Value))
                    .Insert(0, target.transform.DOScale(finish, duration).ChangeStartValue(changeStartValue.Value))
                    .SetAutoKill(false)
                    .SetEase(ease)
                    .SetLink(target.gameObject);

            return DOTween.Sequence()
                .Insert(0, target.DOFade(finish, duration))
                .Insert(0, target.transform.DOScale(finish, duration))
                .SetAutoKill(false)
                .SetEase(ease)
                .SetLink(target.gameObject);
        }
        public static Sequence OnSelectAnimation(this GameObject go)
        {
            return OnSelectAnimation(go.GetComponent<RectTransform>());
        }

        public static Sequence OnSelectAnimation(this RectTransform rectTransform)
        {
            var baseScale = rectTransform.localScale;
            return DOTween.Sequence()
           .Append(rectTransform.DOScale(new Vector3(baseScale.x * 0.95f, baseScale.y * 1.01f, baseScale.z), 0.05f))
           .Append(rectTransform.DOScale(new Vector3(baseScale.x * 1.05f, baseScale.y * 0.9f, baseScale.z), 0.05f))
           .Append(rectTransform.DOScale(new Vector3(baseScale.x * 0.95f, baseScale.y * 1.01f, baseScale.z), 0.05f))
           .Append(rectTransform.DOScale(baseScale, 0.05f))
           .SetLink(rectTransform.gameObject)
           .Play();
        }

        public static IPromise SetAlphaTween(this MonoBehaviour m, float alpha, float dur = .25f)
        {
            if (!m) return Promise.Resolved();

            return SetAlphaTween(m.gameObject, alpha, dur);
        }

        public static IPromise SetAlphaTween(this GameObject o, float alpha, float dur = .25f)
        {
            if (!o) return Promise.Resolved();

            var imgs = o.GetComponentsInChildren<Image>();
            var txts = o.GetComponentsInChildren<TMP_Text>();
            var sprites = o.GetComponentsInChildren<SpriteRenderer>();

            if (imgs.Length == 0 && txts.Length == 0 && sprites.Length == 0)
            {
                o.SetActive(alpha > 0);

                return Promise.Resolved();
            }

            foreach (var image in imgs)
                image.DOFade(alpha, dur).SetLink(o);

            foreach (var txt in txts)
                txt.DOFade(alpha, dur).SetLink(o);

            foreach (var s in sprites)
                s.DOFade(alpha, dur).SetLink(o);

            var p = new Promise();
            DOVirtual.DelayedCall(dur, () =>
            {
                p.Resolve();
            });
            return p;
        }

        public static Sequence BounceSequence(this Transform t, float duration, float magnitude = .05f)
        {
            var seq = DOTween.Sequence();
            var durPart = duration / 3;

            var min = 1 - magnitude;
            var max = 1 + magnitude;

            var scaleMin = new Vector3(t.localScale.x * max, t.localScale.y * min, t.localScale.z);
            var scaleMax = new Vector3(t.localScale.x * min, t.localScale.y * max, t.localScale.z);
            var scaleInitial = t.localScale;

            seq.Append(t.DOScale(scaleMin, durPart).SetEase(Ease.OutCubic));
            seq.Append(t.DOScale(scaleMax, durPart).SetEase(Ease.InCubic));
            seq.Append(t.DOScale(scaleInitial, durPart).SetEase(Ease.OutCubic));
            seq.SetLink(t.gameObject);

            return seq;
        }

        public static Tween FlashImageTween(this Image img, float duration, bool instantFlash = false, Ease ease = Ease.Linear)
        {
            const string FLASH = "_Flash";
            var mat = img.material;

            if (!mat.HasFloat(FLASH))
            {
                throw new Exception("Can not flash this image, choose right material");
            }

            var uiMat = img.GetComponent<UiMaterialCloner>();
            if (!uiMat)
            {
                uiMat = img.gameObject.AddComponent<UiMaterialCloner>();
            }
            uiMat.Init();


            Tween SetFlash(float end, float dur) =>
                DOTween.To(() => mat.GetFloat(FLASH), x => mat.SetFloat(FLASH, x),
                    end, dur);

            var seq = DOTween.Sequence();

            if (instantFlash)
            {
                seq.Append(SetFlash(1, 0));
                seq.Append(DOVirtual.DelayedCall(duration / 2, null));
            }
            else
                seq.Append(SetFlash(1, duration / 2));

            seq.Append(SetFlash(0, duration / 2));
            seq.SetLink(img.gameObject);
            seq.SetEase(ease);
            seq.Play();
            return seq;
        }

        /// <summary>
        /// Текущая позиция берется как стартовая
        /// Трансформ двигается по кривой безье 2го порядка
        /// </summary>
        public static Tween Bezier2Tween(this Transform target, Vector3 control, Vector3 posEnd, float duration, bool global = false, Ease ease = Ease.Linear) =>
            target.Bezier2Tween(global ? target.position : target.localPosition, control, posEnd, duration, global);

        /// <summary>
        /// Трансформ двигается по кривой безье 2го порядка
        /// </summary>
        public static Tween Bezier2Tween(this Transform target, Vector3 startPos, Vector3 control, Vector3 posEnd, float duration, bool global = false, Ease ease = Ease.Linear)
        {
            float t = 0;
            void Setter(float x)
            {
                t = x;
                if (global)
                    target.position = MathUtils.Bezier2(startPos, control, posEnd, x);
                else
                    target.localPosition = MathUtils.Bezier2(startPos, control, posEnd, x);
            }
            return DOTween.To(() => t, Setter, 1f, duration).SetEase(ease).SetLink(target.gameObject);
        }

        /// <summary>
        /// Текущая позиция берется как стартовая
        /// Трансформ двигается по кривой безье 3го порядка
        /// </summary>
        public static Tween Bezier3Tween(this Transform target, Vector3 control1, Vector3 control2, Vector3 posEnd, float duration, bool global = false, Ease ease = Ease.Linear) =>
            target.Bezier3Tween(global ? target.position : target.localPosition, control1, control2, posEnd, duration, global, ease);

        /// <summary>
        /// Трансформ двигается по кривой безье 3го порядка
        /// </summary>
        public static Tween Bezier3Tween(this Transform target, Vector3 startPos, Vector3 control1, Vector3 control2, Vector3 posEnd, float duration, bool global = false, Ease ease = Ease.Linear)
        {
            float t = 0;
            void Setter(float x)
            {
                t = x;
                if (global)
                    target.position = MathUtils.Bezier3(startPos, control1, control2, posEnd, x);
                else
                    target.localPosition = MathUtils.Bezier3(startPos, control1, control2, posEnd, x);
            }
            return DOTween.To(() => t, Setter, 1f, duration).SetEase(ease).SetLink(target.gameObject);
        }
    }
}