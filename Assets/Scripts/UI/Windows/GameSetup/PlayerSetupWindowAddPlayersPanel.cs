using Assets.Scripts.UI.Controls;
using UnityEngine;

namespace Assets.Scripts.UI.Windows.GameSetup
{
    public class PlayerSetupWindowAddPlayersPanel : MonoBehaviour
    {
        [SerializeField] private PlayerSetupItem prefab;
        [SerializeField] public Transform playerItemsTransform;
        [SerializeField] private ButtonBasic addPlayer;

        void Awake()
        {
            prefab.gameObject.SetActive(false);
            addPlayer.onClick.AddListener(OnAddPlayerClick);
        }

        void OnAddPlayerClick()
        {
            var inst = Instantiate(prefab, playerItemsTransform);
            inst.gameObject.SetActive(true);
            inst.SetName($"Player{playerItemsTransform.childCount}");
        }
    }
}