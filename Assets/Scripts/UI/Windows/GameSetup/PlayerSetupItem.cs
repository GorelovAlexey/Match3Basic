using Assets.Scripts.Engine.Player;
using Assets.Scripts.UI.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.GameSetup
{
    public class PlayerSetupItem : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Toggle aiToggle;
        [SerializeField] private ButtonBasic removeBtn;

        void Awake()
        {
            removeBtn.onClick.AddListener(OnRemoveClick);
        }

        void OnRemoveClick()
        {
            Destroy(gameObject);
        }

        public Match3Player Create()
        {
            Match3PlayerMoveMaker moveMaker;
            if (aiToggle.isOn) moveMaker =  new BasicAIMoveMaker();
                else moveMaker = new HumanMoveMaker();
            return new Match3Player(nameField.text, moveMaker);
        }

        public void SetName(string name)
        {
            nameField.text = name;
        }
    }
}
