using System.Collections.Generic;
using Assets.Scripts.Engine;
using Assets.Scripts.Engine.Player;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class PlayerPanel : MonoBehaviour
    {
        [SerializeField] private ResourcePanel resourcePrefab;
        [SerializeField] private Transform resourcesHolder;
        [SerializeField] private Color colorInactive;
        [SerializeField] private Color colorActive;
        [Space] 
        [SerializeField] private TextMeshProUGUI playerName;

        // TODO
        private readonly List<Match3Token> tokensToDisplay = new List<Match3Token> {Match3Token.Blue, Match3Token.Red, Match3Token.Green, Match3Token.Yellow};

        private Dictionary<Match3Token, ResourcePanel> resourcePanelsInstances;
        private Image backgroundImg;
        public Match3Player Player { get; private set; }

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void SetPlayer(Match3Player player)
        {
            this.Player = player;
            backgroundImg = GetComponent<Image>();

            DeleteOldResourcePanels();
            SpawnResourcePanels();

            playerName.text = player.Name;

            Game.Instance.ActiveGame.PlayersManager.CurrentPlayerReactive.Subscribe(p =>
            {
                backgroundImg.color = p == Player ? colorActive : colorInactive;

            }).AddTo(gameObject);
        }

        private void SpawnResourcePanels()
        {
            resourcePanelsInstances = new Dictionary<Match3Token, ResourcePanel>();
            resourcePrefab.gameObject.SetActive(false);
            foreach (var t in tokensToDisplay)
            {
                var inst = Instantiate(resourcePrefab, resourcesHolder);
                inst.gameObject.SetActive(true);
                inst.SetPlayer(Player, t);
                resourcePanelsInstances.Add(t, inst);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(resourcesHolder.GetComponent<RectTransform>());
        }

        private void DeleteOldResourcePanels()
        {
            var destroy = new List<GameObject>();
            for (var i = 0; i < resourcesHolder.childCount; i++)
                destroy.Add(resourcesHolder.GetChild(i).gameObject);

            foreach (var o in destroy)
            {
                if (o == resourcePrefab.gameObject)
                    continue;
                Destroy(o);
            }
        }

        public Vector3 GetResourcePosition(Match3Token t)
        {
            if (resourcePanelsInstances.ContainsKey(t))
                return resourcePanelsInstances[t].GetResourcePosition();

            return transform.position;
        }

        public Vector3 GetPanelPosition()
        {
            var rect = GetComponent<RectTransform>();
            return transform.TransformPoint(rect.rect.center);
        }
    }
}