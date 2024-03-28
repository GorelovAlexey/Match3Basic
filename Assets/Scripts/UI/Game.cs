using System.IO;
using System.Linq;
using Assets.Scripts.Controlls;
using Assets.Scripts.Engine;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.UI.WindowsManagerSystem;
using Assets.Scripts.Visualizer;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class Game : MonoBehaviour
    {
        public const string SAVE_REPLAY_PATH_KEY = "SAVE_REPLAY_PATH";

        [Header("prefabs")] 
        [SerializeField] private GameObject fieldHolder;

        [Header("Other")] 
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private PlayersPanel hudPlayersPanel;
        [SerializeField] private HUD hud;

        private static Game _instance;
        public static Game Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<Game>();

                return _instance;
            }
        }
        public static HUD HUD => Instance.hud;

        public Match3Game ActiveGame { get; private set; }

        public static bool IsFieldExists => Instance.fieldVisualUi;
        public static BoardController BoardController => Instance.boardController;

        private BoardController boardController;
        private FieldVisualUI fieldVisualUi;

        public void CreateField()
        {
            if (!IsFieldExists)
            {
                var inst = Instantiate(fieldHolder, mainCanvas.transform);
                inst.transform.SetAsFirstSibling();
                boardController = inst.GetComponentInChildren<BoardController>();
                fieldVisualUi = inst.GetComponentInChildren<FieldVisualUI>();
            }

            hud.SetBoardController(x => boardController.SetBtn(x));
            hud.SetAutoToggle(x => boardController.SetToggle(x));
            hud.SetFieldVisualUi((x, y) => fieldVisualUi.SetupPrefab(x, y));
        }

        public void SetBoardVisibility(bool visibility)
        {
            if (fieldVisualUi)
                fieldVisualUi.gameObject.SetActive(visibility);
        }

        public void SetSceneMainMenu() 
        {
            ActiveGame?.SetActive(false);

            SetBoardVisibility(false);
            HUD.SetHudInGame(false);
            WindowsManager.Instance.OpenWindow<MainMenuWindow>();
        }

        private void SetSceneGame()
        {
            HUD.SetupGame(ActiveGame);
            SetBoardVisibility(true);
            ActiveGame?.SetActive(true);
        }

        public void StartGame(Match3GameSettings settings, Match3Player[] players)
        {
            if (!IsFieldExists)
                CreateField();

            ActiveGame = new Match3Game(settings);
            ActiveGame.SetupPlayers(players);

            ActiveGame.SetupVisuals(fieldVisualUi);
            BoardController.SetGame(ActiveGame);

            SetSceneGame();
        }

        public void PlayReplay(Match3Replay replay)
        {
            ActiveGame = new Match3Game(replay);

            ActiveGame.SetupVisuals(fieldVisualUi);
            BoardController.SetGame(ActiveGame);

            SetSceneGame();
        }

        private void Initialize()
        {
            DontDestroyOnLoad(gameObject);

            CreateField();
            SetSceneMainMenu();

            WindowsManager.Instance.CurrentWindow.Subscribe(w =>
            { 
                //ActiveGame?.SetActive(!w);
                Time.timeScale = w ? 0f : 1f;

            }).AddTo(BoardController);
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (ActiveGame != null)
            {
                var game = ActiveGame;
                if (game.MoveProvider.IsTurnInProgress == false && game.MoveProvider is Match3MoveProviderBasic 
                                                                && game.PlayersManager.CurrentPlayer.MoveMaker is HumanMoveMaker)
                    game.MoveProvider.ProgressTurn();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OpenMenu();
            }
        }

        public bool CanOpenMenu()
        {
            if (ActiveGame?.IsActive != true) return false;

            if (WindowsManager.Instance.CurrentWindow.Value is MenuWindow w && w.IsClosing)
                return true;

            if (WindowsManager.Instance.CurrentWindow.Value != null)
                return false;

            return true;
        }
        public void OpenMenu()
        {
            if (CanOpenMenu())
            {
                WindowsManager.Instance.OpenWindow<MenuWindow>();
            }
        }

        public static string GetSavePath()
        {
            if (!PlayerPrefs.HasKey(SAVE_REPLAY_PATH_KEY))
            {
                var path = Path.Combine(Application.dataPath, "save", "replay.txt");
                PlayerPrefs.SetString(SAVE_REPLAY_PATH_KEY, path);
                return path;
            }

            return PlayerPrefs.GetString(SAVE_REPLAY_PATH_KEY);
        }
    }
}