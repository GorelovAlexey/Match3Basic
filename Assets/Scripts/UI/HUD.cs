using System;
using Assets.Scripts.Engine;
using Assets.Scripts.UI.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Button btnNext;
        [SerializeField] private Toggle btnAuto;
        [SerializeField] private PlayersPanel playersPanel;
        [SerializeField] private Transform tokensFlyLayer;
        [SerializeField] private TurnCounter turnCounter;
        [SerializeField] private TurnPlayerDisplay turnPlayerDisplay;
        [SerializeField] private ButtonText menuButton;

        private GameObject[] inGameActiveObjects;
        private GameObject[] GetHudObjects => inGameActiveObjects ??= new[]
        {
            btnNext.gameObject,
            btnAuto.gameObject,
            playersPanel.gameObject,
            tokensFlyLayer.gameObject,
            turnCounter.gameObject,
            turnPlayerDisplay.gameObject,
            menuButton.gameObject
        };

        protected void Awake()
        {
            menuButton.onClick.AddListener(() =>
            {
                Game.Instance.OpenMenu();
            });
        }

        public void SetupGame(Match3Game g)
        {
            SetHudInGame(true);

            turnPlayerDisplay.Setup(g);
            turnCounter.Setup(g);
            playersPanel.SetupPlayers(g.PlayersManager.players);
        }

        public void SetBoardController(Action<Button> set)
        {
            set.Invoke(btnNext);
        }

        public void SetAutoToggle(Action<Toggle> set)
        {
            set.Invoke(btnAuto);
        }

        public void SetFieldVisualUi(Action<PlayersPanel, Transform> set)
        {
            set.Invoke(playersPanel, tokensFlyLayer);
        }

        public void SetHudInGame(bool inGame)
        {
            foreach (var go in GetHudObjects)
            {
                go.SetActive(inGame);
            }
        }
    }
}