using System;
using Assets.Scripts.Controlls;
using Assets.Scripts.Engine;
using Assets.Scripts.UI;
using Assets.Scripts.Utils;
using DG.Tweening;
using RSG;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Visualizer
{
    public class FieldVisualUI : MonoBehaviour, IFieldVisualizer
    {
        public PlayersPanel playersPanel;

        public Match3VisualToken tokenPrefab;
        public Image fieldBlocker;
        public Transform tokensContainer;

        private Match3VisualToken[,] tokens;

        [SerializeField] private Transform TokensFlyLayer;

        public int rows;
        public int columns;

        private float width;
        private float height;

        private float minX;
        private float minY;

        private float cellWidth;
        private float cellHeight;

        public event Action<Match3VisualToken> VisualTokenSpawned = token => { };

        private bool _isInputBlocked;
        public bool IsInputBlocked
        {
            get => _isInputBlocked;
            set
            {
                _isInputBlocked = value;
                fieldBlocker.gameObject.SetActive(value);
            }
        }

        public CommandsRegistry CommandsRegistry { get; } = new CommandsRegistry();

        public Vector3 GetCellPosition(float x, float y)
        {
            var offsetX = x * cellWidth + cellWidth / 2f;
            var offsetY = y * cellHeight + cellHeight / 2f;
            return new Vector3(minX + offsetX, minY + height - offsetY);
        }

        public void SetupPrefab(PlayersPanel playersPanel, Transform tokensLayer)
        {
            this.playersPanel = playersPanel;
            TokensFlyLayer = tokensLayer;
        }

        void Start()
        {
            RecalculateField();
            IsInputBlocked = false;
        }

        private RectTransform _selfRect;
        private RectTransform SelfRect
        {
            get
            {
                if (!_selfRect) _selfRect = GetComponent<RectTransform>();
                return _selfRect;
            }
        }

        public void RecalculateField()
        {
            var corners = new Vector3[4];
            SelfRect.GetLocalCorners(corners);

            var min = corners[0];
            var max = corners[2];

            width = max.x - min.x;
            height = max.y - min.y;

            minX = min.x;
            minY = min.y;

            cellWidth = width / columns;
            cellHeight = height / rows;
        }

        public void SetField(Match3Field field)
        {
            ClearOldTokens();

            var f = field.GetField;
            columns = field.Width;
            rows = field.Height;
            RecalculateField();

            tokens = new Match3VisualToken[columns, rows];
            for (int x = 0; x < field.Width; x++)
            {
                for (int y = 0; y < field.Height; y++)
                {
                    var p = SpawnToken(f[x, y], x, y) as Match3VisualToken;
                    SetToken(p, x, y);
                    tokens[x, y] = p;
                }
            }
        }

        void ClearOldTokens()
        {
            CommandsRegistry.CancelAllCommands();
            tokensContainer.ForEachChild(x => Destroy(x.gameObject));
        }

        public IPromise RemoveTokenAnimated(int x, int y, float animSpeed = 1)
        {
            if (!tokens[x, y].GameObject)
                return Promise.Resolved();

            var p = tokens[x, y].Remove(animSpeed);
            tokens[x, y] = null;
            return p;
        }

        public FieldTokenVisual SpawnToken(Match3Token tokenType, float x, float y)
        {
            var pos = GetCellPosition(x, y);
            var result = Instantiate(tokenPrefab, tokensContainer);
            result.gameObject.SetActive(true);
            result.Token = tokenType;
            result.transform.localPosition = pos;
            result.SetSize(cellWidth - 2, cellHeight - 2);

            VisualTokenSpawned.Invoke(result);

            return result;
        }

        public Transform SpawnResourceToken(Match3Token t, int x, int y)
        {
            var resToken = new GameObject($"ResToken: {t}");
            resToken.transform.SetParent(tokensContainer);
            resToken.transform.localPosition = GetCellPosition(x, y);
            var img = resToken.AddComponent<Image>();
            img.sprite = DataHolder.TokenIconDatabase.GetSpriteForToken(t);

            resToken.transform.SetParent(GetTokensLayer());

            return resToken.transform;
        }

        public Transform GetTokensLayer() => TokensFlyLayer;

        public IPromise MoveVisualToken(FieldTokenVisual token, int x, int y, float totalTime)
        {
            if (!token.GameObject)
                return Promise.Resolved();

            var p = new Promise();
            var go = token.GameObject;
            go.transform.DOLocalMove(GetCellPosition(x, y), totalTime)
                .OnComplete(p.Resolve)
                .SetLink(go);
            return p;
        }

        public FieldTokenVisual GetToken(int x, int y)
        {
            return tokens[x, y];
        }

        public FieldTokenVisual SetToken(FieldTokenVisual token, int x, int y)
        {
            tokens[x, y] = token as Match3VisualToken;
            return token;
        }

        public PlayersPanel PlayerPanel => playersPanel;

        public (int x, int y) GetMatch3TokenPosition(FieldTokenVisual t)
        {
            for (var x = 0; x < tokens.GetLength(0); x++)
            {
                for (var y = 0; y < tokens.GetLength(1); y++)
                {
                    if ((Match3VisualToken)t == tokens[x, y])
                        return (x, y);
                }
            }

            throw new Exception("Token not found");
        }

        public void SetupController(BoardController c)
        {
            if (tokens != null)
                foreach (var t in tokens)
                    c.AddTokenObserver(t);
        }
    }
}