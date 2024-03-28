using System;
using Assets.Scripts.Engine;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class TurnCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI counterText;

        private IDisposable sub;
        private int turn = 0;
        public void Setup(Match3Game g)
        {
            turn = 0;
            sub?.Dispose();
            sub = g.PlayersManager.CurrentPlayerReactive.StartWith(g.PlayersManager.CurrentPlayer).Subscribe(x =>
            {
                turn++;
                counterText.text = turn.ToString();

            }).AddTo(gameObject);
        }
    }
}