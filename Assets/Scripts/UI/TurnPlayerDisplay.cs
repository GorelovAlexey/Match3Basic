using System;
using Assets.Scripts.Engine;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class TurnPlayerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI playerLabel;
        [SerializeField] private Image playerBack;

        private IDisposable currentPlayerSubscription;

        public void Setup(Match3Game g)
        {
            currentPlayerSubscription?.Dispose();
            currentPlayerSubscription = g.PlayersManager.CurrentPlayerReactive.StartWith(g.PlayersManager.CurrentPlayer)
                .Subscribe(x =>
                {
                    playerLabel.text = x.Name;

                }).AddTo(gameObject);
        }
    }
}