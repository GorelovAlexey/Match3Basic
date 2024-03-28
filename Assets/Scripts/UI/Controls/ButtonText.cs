using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Controls
{
    public class ButtonText : ButtonBasic
    {
        [SerializeField] private TextMeshProUGUI text;

        public string Text
        {
            get => text.text;
            set => text.text = value;
        }
    }
}