using System;
using System.Collections.Generic;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.UI.WindowsManagerSystem;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows
{
    [Serializable]
    public class StyleDictionary : SerializableDictionaryBase<ModalWindow.ButtonStyle, Color> {}

    public class ModalWindow : Window
    {
        private WindowsTweenAnimationCreatorInstant<Window> animCreator;
        protected override WindowsTweenAnimationCreator<Window> AnimationCreator => animCreator ??= new WindowsTweenAnimationCreatorInstant<Window>(this);

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private ButtonText buttonText_prefab;
        [SerializeField] private Transform horizontalLayout;
        [SerializeField] private StyleDictionary styles;

        public void Init(string title, string message, params (string buttonText, Action OnSelect, ButtonStyle style)[] buttonOptions)
        {
            this.title.text = title;
            this.message.text = message;

            foreach (var buttonOption in buttonOptions)
            {
                var buttonText = Instantiate(buttonText_prefab, horizontalLayout);
                // epmty string => space, so autosize would work correctly
                buttonText.Text = string.IsNullOrEmpty(buttonOption.buttonText) ? " " : buttonOption.buttonText; 
                buttonText.onClick.AddListener(() =>
                {
                    buttonOption.OnSelect?.Invoke();
                    Close();
                });
                SetButtonStyle(buttonText, buttonOption.style);

                buttonText.gameObject.SetActive(true);
            }

            buttonText_prefab.gameObject.SetActive(false);
        }

        private void SetButtonStyle(ButtonText b, ButtonStyle s)
        {
            var img = b.GetComponent<Image>();
            img.color = styles[s];
        }

        /// <summary>
        /// Displays modal window with specified options
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="buttonOptions"> button wich displays text and color </param>
        public static ModalWindow Show(string title, string message,
            params (string buttonText, Action OnSelect, ButtonStyle style)[] buttonOptions)
        {
            return WindowsManager.Instance.OpenWindow<ModalWindow>(x => x.Init(title, message, buttonOptions));
        }

        public enum ButtonStyle
        {
            Default, Red, Green
        }

        public static ModalWindow ShowError(string msg, Action onOk = null )
        {
            return Show("Error", msg, ("ok", onOk, ButtonStyle.Red));
        }
    }
}