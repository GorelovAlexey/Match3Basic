using System;
using Assets.Scripts.Engine;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.UI.WindowsManagerSystem;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
    public class MainMenuWindow : Window
    {
        [SerializeField] private ButtonText setupGame;
        [SerializeField] private ButtonText loadReplay;
        [SerializeField] private ButtonText exit;

        [SerializeField] private GameObject LoadReplayHidden;
        [SerializeField] private ButtonText loadConfirmBtn;
        [SerializeField] private TMP_InputField loadInputField;

        protected override void OnAppear()
        {
            base.OnAppear();

            setupGame.Text = "Setup Game";
            setupGame.onClick.AddListener(OnGameSetup);

            exit.Text = "Exit game";
            exit.onClick.AddListener(OnExit);

            loadReplay.Text = "Load Replay";
            loadReplay.onClick.AddListener(() =>
            {
                loadReplay.gameObject.SetActive(false);
                LoadReplayHidden.SetActive(true);
            });

            loadConfirmBtn.onClick.AddListener(OnReplay);
            loadInputField.onSubmit.AddListener(_ => OnReplay());
            loadInputField.text = Game.GetSavePath();
        }

        private void OnGameSetup()
        {
            Manager.OpenWindow<GameSetupWindow>();
        }

        private void OnExit()
        {
            Application.Quit();
        }

        void OnReplay()
        {
            try
            {
                var replay = Match3Replay.Load(loadInputField.text);
                Game.Instance.PlayReplay(replay);
                Manager.CloseAllWindows(3F);
            }
            catch (Exception e)
            {
                ModalWindow.ShowError(e.Message);
                throw;
            }
        }
    }
}
