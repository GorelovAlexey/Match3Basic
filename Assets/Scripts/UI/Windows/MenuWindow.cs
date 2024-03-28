using System;
using System.IO;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.UI.WindowsManagerSystem;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
    public class MenuWindow : Window
    {
        public override bool IsMinimizeOthers => false;

        [SerializeField] private ButtonText backButton;
        [SerializeField] private ButtonText closeGame;


        [SerializeField] private ButtonText goToMainMenu;


        [SerializeField] private ButtonText saveReplay;
        [SerializeField] private GameObject saveHiddenOption;
        [SerializeField] private ButtonText saveReplayHidden;
        [SerializeField] private TMP_InputField savePathInput;

        protected override void OnAwake()
        {
            base.OnAwake();

            backButton.onClick.AddListener(Close);
            closeGame.onClick.AddListener(Application.Quit);
            goToMainMenu.onClick.AddListener(() =>
            {
                Close();
                Game.Instance.SetSceneMainMenu();
            });

            saveReplay.onClick.AddListener(() =>
            {
                saveHiddenOption.SetActive(true);
                saveReplay.gameObject.SetActive(false);
            });

            saveReplayHidden.onClick.AddListener(OnSaveClick);
            savePathInput.onSubmit.AddListener(_ => OnSaveClick());

            savePathInput.text = Game.GetSavePath();
        }

        public const string SAVE_REPLAY_PATH_KEY = "SAVE_REPLAY_PATH";

        private void OnSaveClick()
        {
            Save(savePathInput.text);
        }

        private void Save(string path)
        {
            try
            {
                var file = new FileInfo(path);
                if (file.Directory?.Exists == false)
                {
                    file.Directory.Create();
                }

                Game.Instance.ActiveGame?.Save(path);
                PlayerPrefs.SetString(SAVE_REPLAY_PATH_KEY, path);
                PlayerPrefs.Save();
                Close();
            }
            catch (Exception e)
            {
                ModalWindow.ShowError(e.Message, null);
            }
        }
    }
}