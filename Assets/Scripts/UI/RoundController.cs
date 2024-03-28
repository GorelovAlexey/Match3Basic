using System;
using Assets.Scripts.Engine;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.UI.WindowsManagerSystem;
using Assets.Scripts.Visualizer;
using DG.Tweening;
using RSG;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class RoundController : MonoBehaviour
    {
        public FieldVisualUI field;
        public PlayersPanel playersPanel;
        private Match3Game game;

        void Start()
        {
            return;
            game = new Match3Game(new Match3GameSettings()
            {
                width = 8,
                height = 8,
                seed = 100,
            });
            game.SetupPlayers(
                new Match3Player("Alex", new HumanMoveMaker()), 
                new Match3Player("AI", new BasicAIMoveMaker()), 
                new Match3Player("AI2", new BasicAIMoveMaker()), 
                new Match3Player("AI3", new BasicAIMoveMaker()));

            game.SetupVisuals(field);

            playersPanel.SetupPlayers(game.PlayersManager.players);
        }

        void Update()
        {
            return;
            if (game.MoveProvider.IsTurnInProgress == false && game.MoveProvider is Match3MoveProviderBasic && game.PlayersManager.CurrentPlayer.MoveMaker is HumanMoveMaker) 
                game.MoveProvider.ProgressTurn();

            if (Input.GetKeyDown(KeyCode.P))
            {
                var load = Match3Replay.Load(Match3Game.replayPath);
                game = new Match3Game(load);
                game.SetupVisuals(field);
                playersPanel.SetupPlayers(game.PlayersManager.players);
            }
        }

        void Test()
        {
            IPromise WaitPromise(float time)
            {
                var p = new Promise();
                DOVirtual.DelayedCall(time, p.Resolve);
                return p;
            }

            IPromise Say(string t)
            {
                Debug.Log(t); 
                return Promise.Resolved();
            }

            Promise.Sequence(() => WaitPromise(1), () => Say("1 sec"), () => WaitPromise(1), () => Say("1 sec"),
                () => WaitPromise(1), () => Say("1 sec")).Then(() => Say("done"));
        }
    }
}
