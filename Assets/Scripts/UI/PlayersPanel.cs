using System;
using System.Collections.Generic;
using Assets.Scripts.Engine;
using Assets.Scripts.Engine.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [Serializable]
    public class PlayerPanelSlotTuple
    {
        public Transform Position;
        public Vector2 Pivot;
    }

    /// <summary>
    /// Создает панели ресурсов для игроков
    /// </summary>
    public class PlayersPanel : MonoBehaviour
    {
        [SerializeField] private PlayerPanel playerPanelPrefab;
        [SerializeField] private PlayerPanelSlotTuple[] playerPanelSlots;

        private Dictionary<Match3Player, PlayerPanel> playerPanelsInstances = new Dictionary<Match3Player, PlayerPanel>();

        public void SetupPlayers(IEnumerable<Match3Player> players)
        {
            ResetPlayers();

            var i = 0;
            playerPanelPrefab.gameObject.SetActive(false);
            foreach (var p in players)
            {
                var inst = Instantiate(playerPanelPrefab, playerPanelSlots[i].Position);
                inst.GetComponent<RectTransform>().pivot = playerPanelSlots[i].Pivot;
                inst.gameObject.SetActive(true);
                inst.SetPlayer(p);
                if (playerPanelsInstances.ContainsKey(p))
                    playerPanelsInstances[p].Delete();

                playerPanelsInstances[p] = inst;

                i++;
                if (playerPanelSlots.Length <= i)
                    i = 0;
            }
        }

        private void ResetPlayers()
        {
            foreach (var pair in playerPanelsInstances)
                pair.Value.Delete();

            playerPanelsInstances.Clear();
        }

        // Для всяких летящих в панель штук
        public Vector3 GetPlayerPanelPosition(Match3Player p)
        {
            return playerPanelsInstances[p].GetPanelPosition();
        }

        // Для всяких летящих в панель штук
        public Vector3 GetPlayerResourcePosition(Match3Player p, Match3Token t)
        {
            return playerPanelsInstances[p].GetResourcePosition(t);
        }
    }
}