using System;
using System.Collections.Generic;
using Assets.Scripts.Engine;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Visualizer;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controlls
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] private Dictionary<Match3VisualToken, IDisposable> tokens = new Dictionary<Match3VisualToken, IDisposable>();
        [SerializeField] private FieldVisualUI field;
        [SerializeField] private Button btn;
        [SerializeField] private Toggle btnAuto;

        public Match3Game Game { get; private set; }

        public void SetGame(Match3Game g)
        {
            Game = g;
        }

        public void SetBtn(Button b)
        {
            btn = b;
        }

        public void SetToggle(Toggle t)
        {
            btnAuto = t;
        }


        public void Start()
        {
            field.VisualTokenSpawned += AddTokenObserver;
            field.SetupController(this);
            btn.onClick.AddListener(OnClick);
        }

        public void AddTokensObserver(List<Match3VisualToken> tokens)
        {
            foreach (var t in tokens)
                AddTokenObserver(t);
        }

        public void AddTokenObserver(Match3VisualToken t)
        {
            if (tokens.ContainsKey(t))
            {
                tokens[t].Dispose();
            }

            var disp = t.GetComponent<DragController>().DragDistanceRelative.Subscribe(x =>
            {
                if (x.x >.5f || x.x < -.5f || x.y > .5f || x.y < -.5f)
                    TryToMove(t, x);

            }).AddTo(t.gameObject);
            tokens[t] = disp;
        }

        public void TryToMove(Match3VisualToken token, Vector2 direction)
        {
            if (field.IsInputBlocked)
                return;

            var dirX = MathF.Abs(direction.x);
            var dirY = MathF.Abs(direction.y);

            var isHorizontalDrag = dirX > dirY;

            // Y   y=x  ==  y/x = 1
            // |  /
            // | /
            // |/
            // +-------X

            var diagonalDragCoef = Mathf.Approximately(dirX, 0) || Mathf.Approximately(dirY, 0) ? 0f : MathF.Min(dirX / dirY, dirY / dirX);
            var isDiagonalDrag = diagonalDragCoef >= .35f;

            if (isDiagonalDrag)
                return;

            try
            {
                var (x, y) = field.GetMatch3TokenPosition(token);

                var x1 = x;
                var y1 = y;

                if (isHorizontalDrag)
                {
                    x1 = direction.x > 0 ? x + 1 : x - 1;
                }
                else
                {
                    y1 = direction.y > 0 ? y - 1 : y + 1;
                }

                var move = new Match3CommandMoveSwap(x, y, x1, y1);
                Game.PlayerMoveInput(move);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        void Update()
        {
           if (btnAuto && btnAuto.isOn && Game?.MoveProvider != null && !Game.MoveProvider.IsTurnInProgress)
               Game.MoveProvider.ProgressTurn();
        }

        void OnClick()
        {
            Game.MoveProvider.ProgressTurn();
        }
    }
}