using System;
using Assets.Scripts.Engine;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ResourcePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI amount;
        [SerializeField] private Image image;

        private IDisposable updateSub;

        public void SetPlayer(Match3Player player, Match3Token t)
        {
            updateSub?.Dispose();

            image.sprite = DataHolder.TokenIconDatabase.GetSpriteForToken(t);

            UpdateCount(player.GetDisplayCount(t).Value, false);
            updateSub = player.GetDisplayCount(t).Subscribe(x => UpdateCount(x)).AddTo(gameObject);
        }

        private int currentDisplayCount = 0;
        private Tween amountAnimation;
        private Tween iconBounceAnim;
        private void UpdateCount(float x, bool animate = true)
        {
            var displayCount = Mathf.FloorToInt(x);

            if (!animate || currentDisplayCount == displayCount)
            {
                SetDisplayCount(displayCount);
                return;
            }

            if (iconBounceAnim == null)
            {
                var seq = image.transform.BounceSequence(.25f).SetAutoKill(false);

                seq.Play();
                iconBounceAnim = seq;
            }
            else
            {
                iconBounceAnim.Rewind();
                iconBounceAnim.Play();
            }

            amountAnimation?.Kill();
            amountAnimation = DOTween.To(() => currentDisplayCount, SetDisplayCount, displayCount, .25f).SetLink(amount.gameObject);
        }

        private void SetDisplayCount(int x)
        {
            currentDisplayCount = x;
            amount.text = x.ToString();
        }

        public Vector3 GetResourcePosition()
        {
            return image.transform.position;
        }
    }
}