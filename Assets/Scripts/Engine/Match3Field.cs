using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using Assets.Scripts.Utils;
using Assets.Scripts.Visualizer;
using Assets.Scripts.Visualizer.VisualCommands;
using UnityEngine;

namespace Assets.Scripts.Engine
{
    public class Match3Field
    {
        /// <summary>
        /// 00 10 20 30 X
        /// 01 11    31
        /// 02    22 32
        /// 03 13 23 33
        /// Y
        /// </summary>
        public Match3Token[,] field { get; set; }
        public Match3Token[,] GetField => field;
        public Match3Token[,] GetFieldClone => field.Clone() as Match3Token[,];
        public int Width => field.GetLength(0);
        public int Height => field.GetLength(1);
        private bool WithinWidth(int x) => x >= 0 && x < Width;
        private bool WithinHeight(int y) => y >= 0 && y < Height;

        private Match3Matcher matcher;
        public Match3Field(int width, int height, Match3FieldGenerator gen, Match3Matcher matcher)
        {
            field = new Match3Token[width, height];
            this.matcher = matcher;
            GenerateField(gen);
        }

        public Match3Field(Match3Token[,] field, Match3Matcher matcher)
        {
            this.field = field;
            this.matcher = matcher;
        }

        public void EmptyCells(List<(int, int)> cells)
        {
            foreach (var (x, y) in cells)
            {
                field[x, y] = Match3Token.Empty;
            }
        }

        public void FallDown(Direction dir, out (int, int)[,] moveMatrix)
        {
            field = FallDown(field, dir, out moveMatrix);
        }

        public void SpawnNewTokens(Match3TokenGenerator gen, out Match3Token[,] tokensMask)
        {
            field = SpawnNewTokens(field, gen, out tokensMask);
        }

        public (Dictionary<Match3Token, float>, int[]) ClearAfterMove(out (Match3Token, float)[,] valueField)
        {
            return ClearAfterMove(field, matcher, out valueField);
        }

        #region Static

        

        #endregion
        public static Match3Token[,] SpawnNewTokens(Match3Token[,] field, Match3TokenGenerator generator, out Match3Token[,] tokensMask)
        {
            tokensMask = new Match3Token[field.GetLength(0), field.GetLength(1)];

            var needTokens = 0;
            for (var x = 0; x < field.GetLength(0); x++)
                for (var y = 0; y < field.GetLength(1); y++)
                    if (field[x, y] == Match3Token.Empty)
                        needTokens++;

            var tokens = generator.GetTokens(needTokens);

            for (var x = 0; x < field.GetLength(0); x++)
                for (var y = 0; y < field.GetLength(1); y++) {
                    if (field[x, y] != Match3Token.Empty || needTokens <= 0) continue;
                    needTokens--;
                    field[x, y] = tokens[needTokens];
                    tokensMask[x, y] = tokens[needTokens];
                }

            return field;
        }

        public (bool possible, bool hasMatches) IsMoveValid(Match3Move move)
        {
            return move.IsValidAndPossible(matcher, field);
        }

        public static List<Match> FindMatches(Match3Token[,] field, Move? move, Match3Matcher matcher)
        {
            if (move.HasValue)
                ApplyMove(field, move.Value);

            var matches = matcher.FullCheck(field);
            return matches;
        }

        public static (Match3Token, float)[,] CreateValueField(Match3Token[,] field, Match3Matcher matcher)
        {
            var result = new (Match3Token, float)[field.GetLength(0), field.GetLength(1)];
            var matches = matcher.FullCheck(field);

            foreach (var match in matches)
                fillMatch(match);

            void fillMatch(Match m)
            {
                var x = m.x;
                var y = m.y;

                for (var i = 0; i < m.right; i++)
                {
                    var x0 = x + i;
                    var y0 = y;
                    result[x0, y0] = (field[x0, y0], 1f);
                }

                for (var i = 0; i < m.bottom; i++)
                {
                    var x0 = x;
                    var y0 = y + i;
                    result[x0, y0] = (field[x0, y0], 1f);
                }
            }

            return result;
        }

        private static Dictionary<Match3Token, Color32> debugColorDict = new Dictionary<Match3Token, Color32>
        {
            {Match3Token.Blue, Color.blue},
            {Match3Token.Red, Color.red},
            {Match3Token.Green, Color.green},
            {Match3Token.Yellow, Color.yellow},
            {Match3Token.Empty, Color.black }
        };

        public static void DebugShowField(StringBuilder sb, Match3Token[,] field, Move? move = null)
        {
            sb.Append('\n');
            for (var y = 0; y < field.GetLength(1); y++)
            {
                for (var x = 0; x < field.GetLength(0); x++)
                {
                    if (!move.HasValue)
                        sb.Append("▮".CreateDebugString(debugColorDict[field[x, y]]));
                    else if (move.Value.x == x && move.Value.y == y || move.Value.x1 == x && move.Value.y1 == y)
                        sb.Append("▯".CreateDebugString(debugColorDict[field[x, y]]));
                    else
                        sb.Append("▮".CreateDebugString(debugColorDict[field[x, y]]));
                }
                sb.Append('\n');
            }
        }


        public static (Dictionary<Match3Token, float>, int[]) ClearAfterMove(Match3Token[,] field, Match3Matcher matcher, out (Match3Token, float)[,] valueField)
        {
            //var debugString = new StringBuilder();
            //DebugShowField(debugString, field, move);

           // DebugShowField(debugString, field, move);

            //Debug.Log(debugString);
            valueField = CreateValueField(field, matcher);
            var numbers = matcher.FullCheck(field).Select(x => x.Count).OrderByDescending(x => x).ToArray();

            var itemCounts = new Dictionary<Match3Token, float>();
            foreach (var valueTuple in valueField)
                AddToken(valueTuple.Item1, valueTuple.Item2);

            void AddToken(Match3Token t, float v)
            {
                if (t == Match3Token.Empty) return;
                
                if (itemCounts.ContainsKey(t))
                    itemCounts[t] += v;
                else 
                    itemCounts[t] = v;
            }

            for (var x = 0; x < field.GetLength(0); x++)
            {
                for (var y = 0; y < field.GetLength(1); y++)
                {
                    if (valueField[x, y].Item2 > 0 && valueField[x, y].Item1 != Match3Token.Empty)
                        field[x, y] = Match3Token.Empty;
                }
            }

            return (itemCounts, numbers);
        }

        public static bool[,] CreateHideMask((Match3Token, float)[,] valueField)
        {
            var result = new bool[valueField.GetLength(0), valueField.GetLength(1)];
            for (int x = 0; x < valueField.GetLength(0); x++)
            {
                for (int y = 0; y < valueField.GetLength(1); y++)
                {
                    if (valueField[x, y].Item1 != Match3Token.Empty && valueField[x, y].Item2 > 0f)
                        result[x, y] = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Applies move.
        /// Changes original array, does not makes a new one to generate less garbage for GC
        /// </summary>
        public static Match3Token[,] ApplyMove(Match3Token[,] field, Move move)
        {
            var t1 = field[move.x, move.y];
            field[move.x, move.y] = field[move.x1, move.y1];
            field[move.x1, move.y1] = t1;
            return field;
        }

        public (int, int)[,] GetMoveMaskFromMove(Move move)
        {
            Vector2Int p0 = new Vector2Int(move.x, move.y);
            Vector2Int p1 = new Vector2Int(move.x1, move.y1);

            var vec01 = p1 - p0;
            var vec10 = p0 - p1;

            var res = new (int, int)[Width, Height];
            res[p0.x, p0.y] = (vec01.x, vec01.y);
            res[p1.x, p1.y] = (vec10.x, vec10.y);
            return res;
        }

        public static Match3Token[,] FallDown(Match3Token[,] field, Direction dir, out (int, int)[,] moveMatrix)
        {
            var width = field.GetLength(0);
            var height = field.GetLength(1);

            moveMatrix = new (int, int)[width, height];

            var outter = 0;
            var verticalFall = dir == Direction.Bot || dir == Direction.Top;
            var maxOutter = verticalFall ? width : height;

            Func<int, (int x, int y)> coordsGetter;
            if (verticalFall) coordsGetter = v => (outter, v);
            else coordsGetter = v => (v, outter);

            Func<int, int> moveInnerUpstream; 
            if (dir == Direction.Top || dir == Direction.Left) moveInnerUpstream = v => v - 1;
            else moveInnerUpstream = v => v + 1;

            var fallFrom = 0;
            var fallTo = 0;
            var fallLast = 0;
            void ResetFrom()
            {
                switch (dir)
                {
                    case Direction.Top:
                        fallLast = -1;
                        fallTo = height - 1;
                        break;
                    case Direction.Left:
                        fallLast = -1;
                        fallTo = width - 1;
                        break;
                    case Direction.Right:
                        fallLast = width;
                        fallTo = 0;
                        break;
                    case Direction.Bot:
                        fallLast = height;
                        fallTo = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
                }
                fallFrom = moveInnerUpstream.Invoke(fallTo);
            }

            for (outter = 0; outter < maxOutter; outter++)
            {
                ResetFrom();
                while (fallFrom != fallLast)
                {
                    var (x, y) = coordsGetter.Invoke(fallFrom);
                    var from = field[x, y];

                    var (x0, y0) = coordsGetter.Invoke(fallTo);
                    var to = field[x0, y0];

                    if (to != Match3Token.Empty)
                    {
                        fallTo = moveInnerUpstream.Invoke(fallTo);
                    }
                    else if (to == Match3Token.Empty && from != Match3Token.Empty)
                    {
                        field[x0, y0] = from;
                        field[x, y] = Match3Token.Empty;

                        moveMatrix[x, y] = (x0 - x, y0 - y);

                        fallTo = moveInnerUpstream.Invoke(fallTo);
                    }

                    fallFrom = moveInnerUpstream.Invoke(fallFrom);
                }
            }

            return field;
        }

        public static Match3Token[,] FallDown(Match3Token[,] field)
        {
            var width = field.GetLength(0);
            var height = field.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                var y = height - 1;
                var y0 = y - 1;

                while (y0 > 0)
                {
                    if (field[x, y] != Match3Token.Empty)
                    {
                        y--;
                        y0--;
                        continue;
                    }

                    if (field[x, y0] != Match3Token.Empty)
                    {
                        field[x, y] = field[x, y0];
                        field[x, y0] = Match3Token.Empty;
                        y--;
                        y0--;
                    }
                    else
                    {
                        y0--;
                    }
                }
            }

            return field;
        }

        public void GenerateField(Match3FieldGenerator gen)
        {
            field = gen.GetField(Width, Height);
        }

        public List<Match3CommandMove> FindBasicMoves()
        {
            var moves = new List<Match3CommandMove>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var token = field[x, y];

                    var x1 = x + 1;
                    var y1 = y + 1;

                    if (x1 < Width)
                    {
                        var right = field[x1, y];
                        if (matcher.MatchesExistsNoClone(field, (x, y, right), (x1, y, token)))
                            moves.Add(new Match3CommandMoveSwap(x, y, x1, y));
                    }

                    if (y1 < Height)
                    {
                        var bot = field[x, y1];
                        if (matcher.MatchesExistsNoClone(field, (x, y, bot), (x, y1, token)))
                            moves.Add(new Match3CommandMoveSwap(x, y, x, y1));
                    }
                }
            }

            return moves;
        }

        public bool HasAnyMoves()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var token = field[x, y];

                    var x1 = x + 1;
                    var y1 = y + 1;

                    if (x1 < Width)
                    {
                        var right = field[x1, y];
                        if (matcher.MatchesExistsNoClone(field, (x, y, right), (x1, y, token)))
                            return true;
                    }

                    if (y1 < Height)
                    {
                        var bot = field[x, y1];
                        if (matcher.MatchesExistsNoClone(field, (x, y, bot), (x, y1, token)))
                            return true;
                    }
                }
            }

            return false;
        }

        #region Structs
        public struct Match
        {
            public int x;
            public int y;

            public int right;
            public int bottom;

            public int Count => right > bottom ? right : bottom;
        }
        public struct Move
        {
            public int x;
            public int y;

            public int x1;
            public int y1;

            public Move(int x, int y, int x1, int y1)
            {
                this.x = x;
                this.y = y;

                this.x1 = x1;
                this.y1 = y1;
            }
        }

        

        #endregion
    }
}