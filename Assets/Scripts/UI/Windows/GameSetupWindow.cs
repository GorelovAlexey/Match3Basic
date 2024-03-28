using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Assets.Scripts.Engine;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.UI.Windows.GameSetup;
using Assets.Scripts.UI.WindowsManagerSystem;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.AI;
using Direction = Assets.Scripts.Engine.Direction;

namespace Assets.Scripts.UI.Windows
{
    public class GameSetupWindow : Window
    {
        [SerializeField] private PlayerSetupWindowAddPlayersPanel playerSetupPanel;
        [Space]
        [SerializeField] private TextMeshProUGUI label_dimensions;
        [SerializeField] private TextMeshProUGUI label_seed;
        [SerializeField] private TextMeshProUGUI label_spawn_direction;
        [Space]
        [SerializeField] private TMP_InputField input_dimensions_width;
        [SerializeField] private TMP_InputField input_dimensions_height;
        [SerializeField] private TMP_InputField input_seed;
        [Space]
        [SerializeField] private TMP_Dropdown dropdown_fall_direction;
        [Space]
        [SerializeField] private ButtonText btnStart;

        protected override void OnAppear()
        {
            CanClose.Value = false;
            base.OnAppear();
            Init();
        }

        protected Dictionary<string, Direction> directionsDictionary;


        protected void Init()
        {
            input_seed.onValueChanged.AddListener(InputSeedOnValueChanged);

            directionsDictionary = new Dictionary<string, Direction>();
            foreach (var dir in new[] {Direction.Top, Direction.Right, Direction.Bot, Direction.Left})
                directionsDictionary.Add(dir.ToString(), dir);

            dropdown_fall_direction.ClearOptions();
            var options = directionsDictionary.Select(x => x.Key).ToList();
            dropdown_fall_direction.AddOptions(options);

            // Setting default values
            var settings = Match3GameSettings.CreateDefault();
            input_seed.text = settings.seed.ToString();
            input_dimensions_height.text = settings.height.ToString();
            input_dimensions_width.text = settings.width.ToString();
            dropdown_fall_direction.value = 0;

            btnStart.onClick.AddListener(OnStartClick);
        }

        protected void OnStartClick()
        {
            var (settings, players, errors) = GetGameSettings();
            if (errors.Count > 0)
            {
                ModalWindow.ShowError(errors.Aggregate((x, y) => x + "\n" + y ), () => {});
                return;
            }

            Game.Instance.StartGame(settings, players.Select(x => x.Create()).ToArray());
            CanClose.Value = true;
            Manager.CloseAllWindows(3F);
        }



        protected void InputSeedOnValueChanged(string value)
        {
            if (long.TryParse(value, out var seed))
            {
                input_seed.text = ((int) seed).ToString();
            }
        }

        private const int MIN_DIMENSION = 4;
        protected (Match3GameSettings, PlayerSetupItem[], List<string> errors) GetGameSettings()
        {
            var errors = new List<string>();
            var setupItems = playerSetupPanel.playerItemsTransform.GetComponentsInChildren<PlayerSetupItem>();
            if (setupItems.Length <= 0)
                errors.Add("Insufficient amount of players");

            var settings = Match3GameSettings.CreateDefault();

            if (int.TryParse(input_dimensions_height.text, out var height))
                settings.height = height;
            else
                errors.Add($"Invalid parameter: height");

            if (int.TryParse(input_dimensions_width.text, out var width))
                settings.width = width;
            else
                errors.Add($"Invalid parameter: width");

            if (height < MIN_DIMENSION)
                errors.Add($"Height needs to be at least {MIN_DIMENSION}");
            if (width < MIN_DIMENSION )
                errors.Add($"Width needs to be at least {MIN_DIMENSION}");

            if (int.TryParse(input_seed.text, out var seed))
                settings.seed = seed;

            var directionText = dropdown_fall_direction.itemText.text;
            if (directionsDictionary?.ContainsKey(directionText) == true)
                settings.tokensSpawnDirection = directionsDictionary[directionText];

            return (settings, setupItems, errors);
        }
    }
}