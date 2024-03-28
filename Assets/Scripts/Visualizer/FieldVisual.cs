using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Assets.Scripts.Controlls;
using Assets.Scripts.Engine;
using Assets.Scripts.UI;
using DG.Tweening;
using RSG;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

namespace Assets.Scripts.Visualizer
{
    public class FieldVisual : MonoBehaviour
    {
        //TODO
        public Match3Game game;
        public PlayersPanel playersPanel;

        public Match3VisualToken tokenPrefab;
        public Image fieldBlocker;
        public Transform tokensContainer;

        [SerializeField] private Transform TokensFlyLayer;

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

        private Match3VisualToken[,] tokens;

        public int rows;
        public int columns;

        private float width;
        private float height;

        private float minX;
        private float minY;

        private float cellWidth; 
        private float cellHeight;

        public event Action<Match3VisualToken> VisualTokenSpawned = token => {};

        public Vector3 GetCellPosition(int x, int y)
        {
            var offsetX = x * cellWidth + cellWidth / 2f;
            var offsetY = y * cellHeight + cellHeight / 2f;
            return new Vector3(minX + offsetX, minY + height - offsetY);
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

        void Start()
        {
            CalculateField();
            IsInputBlocked = false;
        }

        void CalculateField()
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
            var f = field.GetField;
            columns = field.Width;
            rows = field.Height;
            CalculateField();

            tokens= new Match3VisualToken[columns, rows];
            for (int x = 0; x < field.Width; x++)
            {
                for (int y = 0; y < field.Height; y++)
                {
                    var p = InstantiateVisualToken(f[x, y], x, y);
                    tokens[x, y] = p;
                }
            }
        }

        public void SetupController(BoardController c)
        {
            foreach (var t in tokens)
                c.AddTokenObserver(t);
        }

        public (int x, int y) GetMatch3TokenPosition(Match3VisualToken t)
        {
            for (var x = 0;  x< tokens.GetLength(0); x++)
            {
                for (var y = 0; y < tokens.GetLength(1); y++)
                {
                    if (t == tokens[x, y])
                        return (x, y);
                }
            }

            throw new Exception("Token not found");
        }

        public IPromise RemoveTokens(bool [,] mask, float animSpeedup = 1f)
        {
            if (mask.GetLength(0) > tokens.GetLength(0) || mask.GetLength(1) > tokens.GetLength(1))
                throw new Exception("Wrong mask size");

            var promises = new List<IPromise>();
            for (var x = 0; x < mask.GetLength(0); x++)
                for (var y = 0; y < mask.GetLength(1); y++)
                    if (mask[x, y] == true)
                    {
                        var p = tokens[x, y].Remove(animSpeedup);
                        promises.Add(p);
                        tokens[x, y] = null;
                    }

            return Promise.All(promises);
        }

        public Match3VisualToken InstantiateVisualToken(Match3Token tokenType, int x, int y)
        {
            var pos = GetCellPosition(x, y);
            var result = GameObject.Instantiate(tokenPrefab, tokensContainer);
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

            resToken.transform.SetParent(GetFlyTokensLayer);

            return resToken.transform;
        }

        public Transform GetFlyTokensLayer => TokensFlyLayer;

        public IPromise MoveVisualToken(Match3VisualToken token, int x, int y, float totalTime)
        {
            var p = new Promise();
            token.transform.DOLocalMove(GetCellPosition(x, y), totalTime)
                .OnComplete(p.Resolve)
                .SetLink(token.gameObject);
            return p;
        }

        public IPromise AnimateMove(Match3Field.Move move)
        {
            var t0 = tokens[move.x, move.y];
            var t1 = tokens[move.x1, move.y1];
            // Swap animation
            var p0 = MoveVisualToken(t0, move.x1, move.y1, AnimationTimings.TOKEN_MOVE_TIME_PER_SQUARE);
            var p1 = MoveVisualToken(t1, move.x, move.y, AnimationTimings.TOKEN_MOVE_TIME_PER_SQUARE);
            // Swap logic
            tokens[move.x, move.y] = t1;
            tokens[move.x1, move.y1] = t0;

            return Promise.All(p0, p1);
        }

        public IPromise MoveTokens((int, int)[,] maskMove, float animSpeedup = 1f)
        {
            if (maskMove.GetLength(0) > tokens.GetLength(0) || maskMove.GetLength(1) > tokens.GetLength(1))
                throw new Exception("Wrong mask size");

            var tokensMapNew = new Match3VisualToken[tokens.GetLength(0), tokens.GetLength(1)];
            var tokensRewritten = new bool[tokens.GetLength(0), tokens.GetLength(1)];
            var tokensFullList = new HashSet<Match3VisualToken>();

            for (var x = 0; x < maskMove.GetLength(0); x++)
                for (var y = 0; y < maskMove.GetLength(1); y++)
                {
                    tokensMapNew[x, y] = tokens[x, y];
                    tokensFullList.Add(tokens[x, y]);
                }


            var promises = new List<IPromise>();

            for (var x = 0; x < maskMove.GetLength(0); x++)
                for (var y = 0; y < maskMove.GetLength(1); y++)
                {
                    var (xMove, yMove) = maskMove[x, y];
                    if (xMove == 0 && yMove == 0) continue;

                    var yNew = y + yMove;
                    var xNew = x + xMove;

                    var moveToken = tokens[x, y];
                    tokensMapNew[xNew, yNew] = moveToken;
                    tokensRewritten[xNew, yNew] = true;

                    if (!tokensRewritten[x, y])
                        tokensMapNew[x, y] = null;

                    var cellDistance = 0f;
                    if (xMove == 0) cellDistance = MathF.Abs(yMove);
                    else if (yMove == 0) cellDistance = MathF.Abs(xMove);
                    else cellDistance = MathF.Sqrt(xMove * xMove + yMove * yMove);

                    var p = new Promise();
                    var moveTime = cellDistance * AnimationTimings.TOKEN_MOVE_TIME_PER_SQUARE * animSpeedup;
                    promises.Add(MoveVisualToken(moveToken, xNew, yNew, moveTime));
                }

            tokens = tokensMapNew;
            foreach (var t in tokensMapNew)
            {
                if (tokensFullList.Contains(t))
                    tokensFullList.Remove(t);
            }

            var lostTokens = tokensFullList.ToList();
            var finishPromise = new Promise();
            Promise.All(promises).Then(() =>
            {
                Console.Write("!LOST_TOKENS: " + lostTokens.Count);
                finishPromise.Resolve();
                foreach (var match3VisualToken in lostTokens)
                {
                    GameObject.Destroy(match3VisualToken.gameObject);
                }
            });

            return finishPromise;
        }

        public IPromise SpawnAndFall(Match3Token[,] mask, Direction direction, float animSpeedup = 1f)
        {
            var width = mask.GetLength(0);
            var height = mask.GetLength(1);

            var horizontal = direction == Direction.Top || direction == Direction.Bot;

            var offsets = new int[horizontal ? width : height];

            var xStart = 0;
            var yStart = 0;
            var xChange = 1;
            var yChange = 1;
            var xMax = width - 1;
            var yMax = height - 1;

            if (direction == Direction.Left)
            {
                xStart = xMax;
                xMax = 0;
                xChange = -1;
            }

            if (direction == Direction.Top)
            {
                yStart = yMax;
                yMax = 0;
                yChange = -1;
            }

            var promises = new List<IPromise>();
            for (var x = xStart; x != xMax + xChange; x += xChange)
            {
                for (var y = yStart; y != yMax + yChange; y += yChange)
                {
                    if (mask[x,y] == Match3Token.Empty) continue;

                    var xSpawn = x;
                    var ySpawn = y;
                    var dist = 0;

                    switch (direction)
                    {
                        case Direction.Top:
                            ySpawn = -1 - offsets[x];
                            dist = y - ySpawn;
                            break;
                        case Direction.Right:
                            xSpawn = width + offsets[y];
                            dist = x - xSpawn;
                            break;
                        case Direction.Bot:
                            ySpawn = height + offsets[x];
                            dist = y - ySpawn;
                            break;
                        case Direction.Left:
                            xSpawn = -1 - offsets[y];
                            dist = x - xSpawn;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }

                    dist = Math.Abs(dist);

                    var tokenSpawn = InstantiateVisualToken(mask[x, y], xSpawn, ySpawn);
                    tokens[x, y] = tokenSpawn;

                    var p = MoveVisualToken(tokenSpawn, x, y, dist * AnimationTimings.TOKEN_MOVE_TIME_PER_SQUARE * animSpeedup);
                    promises.Add(p);

                    if (horizontal) offsets[x] += 1;
                    else offsets[y] += 1;
                }
            }

            return Promise.All(promises);
        }
    }

}
