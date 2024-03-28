using System;
using Assets.Scripts.Engine;
using DG.Tweening;
using RotaryHeart.Lib.SerializableDictionary;
using RSG;
using UnityEngine;

namespace Assets.Scripts.Visualizer
{
    [Serializable]
    public class Match3TokenVisuals : SerializableDictionaryBase<Match3Token, GameObject> { }

    public class Match3VisualToken : MonoBehaviour, FieldTokenVisual
    {
        public GameObject GameObject => gameObject;
        private Match3Token _token;
        public Match3Token Token
        {
            get => _token;
            set
            {
                _token = value;
                foreach (var key in visuals.Keys)
                {
                    var val = visuals[key];
                    val.SetActive(key == _token);
                }
            }
        }

        public void SetSize(float width, float height)
        {
            var rect = GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public Match3TokenVisuals visuals;

        public void Start()
        {
            Token = _token;
        }

        Tween scaleTween;
        public IPromise Remove(float animTimeSpeed = 1f)
        {
            var promise = new Promise();
            scaleTween?.Kill();
            scaleTween = transform.DOScale(0, AnimationTimings.TOKEN_HIDE_TIME * animTimeSpeed)
                .SetEase(Ease.InCubic)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    Destroy(gameObject);
                    promise.Resolve();
                });
            return promise;
        }
    }
}