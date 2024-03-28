using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.Engine
{
    public class Match3Matcher
    {
        public virtual bool CanMatch(Match3Token t1, Match3Token t2)
        {
            return false;
        }

        public virtual int MatchMin => 3;

        public bool MatchExists(Match3Token[,] field, int x, int y, Match3Token? fakeToken = null)
        {
            Func<int, int, (int, int)> leftMove = (x, y) => (x - 1, y);
            Func<int, int, (int, int)> rightMove = (x, y) => (x + 1, y);
            Func<int, int, (int, int)> topMove = (x, y) => (x, y + 1);
            Func<int, int, (int, int)> botMove = (x, y) => (x, y - 1);

            var width = field.GetLength(0);
            var height = field.GetLength(1);

            if (x < 0 || x >= width || y < 0 || y > height) return false;

            var targetToken = fakeToken ?? field[x, y];

            bool WithinWidth(int v) => v >= 0 && v < width;
            bool WithinHeight(int v) => v >= 0 && v < height;

            int Count(Func<int, int, (int, int)> cordChange, int x, int y)
            {
                var count = 0;
                while (true)
                {
                    (x, y) = cordChange(x, y);
                    if (!WithinWidth(x) || !WithinHeight(y))
                        break;
                    if (!CanMatch(field[x, y], targetToken))
                        break;
                    count++;
                }
                return count;
            }

            var hor = Count(leftMove, x, y) + 1 + Count(rightMove, x, y);
            var vert = Count(topMove, x, y) + 1 + Count(botMove, x, y);

            return hor >= MatchMin || vert >= MatchMin;
        }

        public bool MatchesExists(Match3Token[,] field, params (int x, int y, Match3Token token)[] replaceTokens)
        {
            var clone = field.Clone() as Match3Token[,];
            foreach (var t in replaceTokens)
                clone[t.x, t.y] = t.token;

            if (replaceTokens.Length >= 4)
                return FullCheck(clone).Count > 0;

            return replaceTokens.Any(r => MatchExists(clone, r.x, r.y));
        }

        public bool MatchesExistsNoClone(Match3Token[,] field, params (int x, int y, Match3Token token)[] replaceTokens)
        {
            var replaceBack = new List<(int x, int y, Match3Token t)>();
            foreach (var t in replaceTokens.Reverse())
            {
                replaceBack.Add((t.x, t.y, field[t.x, t.y]));
            }

            void Replace(Match3Token[,] field, IEnumerable<(int x, int y, Match3Token token)> replace)
            {
                foreach (var t in replace)
                    field[t.x, t.y] = t.token;
            }

            Replace(field, replaceTokens);

            var hasMatches = replaceTokens.Length >= 4
                ? FullCheck(field).Count > 0
                : replaceTokens.Any(r => MatchExists(field, r.x, r.y));

            Replace(field, replaceBack);

            return hasMatches;
        }

        public List<Match3Field.Move> FindMoves(Match3Token[,] field)
        {
            var moves = new List<Match3Field.Move>();
            var width = field.GetLength(0);
            var height = field.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var token = field[x, y];

                    var x1 = x + 1;
                    var y1 = y + 1;

                    if (x1 < width)
                    {
                        var right = field[x1, y];
                        if (MatchesExistsNoClone(field, (x, y, right), (x1, y, token)))
                            moves.Add(new Match3Field.Move(x, y, x1, y));
                    }

                    if (y1 < height)
                    {
                        var bot = field[x, y1];
                        if (MatchesExistsNoClone(field, (x, y, bot), (x, y1, token)))
                            moves.Add(new Match3Field.Move(x, y, x, y1));

                    }
                }
            }

            return moves;
        }

        private List<Match3Field.Match> CheckMatches(Match3Token[,] field, int xStart, int yStart, int xEnd, int yEnd)
        {
            Func<int, int, (int, int)> leftMove = (x, y) => (x - 1, y);
            Func<int, int, (int, int)> rightMove = (x, y) => (x + 1, y);
            Func<int, int, (int, int)> topMove = (x, y) => (x, y + 1);
            Func<int, int, (int, int)> botMove = (x, y) => (x, y - 1);

            var width = field.GetLength(0);
            var height = field.GetLength(1);

            int Count(Func<int, int, (int, int)> cordChange, int x, int y)
            {
                var count = 0;
                var start = field[x, y];
                while (true)
                {
                    (x, y) = cordChange(x, y);
                    if (x < 0 || y < 0 || x >= width || y >= height)
                        break;
                    if (!CanMatch(field[x, y], start))
                        break;
                    count++;
                }
                return count;
            }

            var matchesHor = new int[width, height];
            var matchesVert = new int[width, height];

            void Check(int x, int y, Func<int, int, (int, int)> cordChange1, Func<int, int, (int, int)> cordChange2, int[,] matches)
            {
                var oneWay = Count(cordChange1, x, y);
                var otherWay = Count(cordChange2, x, y);
                var total = 1 + oneWay + otherWay;

                var affected = new List<(int, int)> { (x, y) };
                var last = (x, y);
                for (var i = oneWay; i > 0; i--)
                {
                    last = cordChange1(last.x, last.y);
                    affected.Add(last);
                }

                last = (x, y);
                for (var i = otherWay; i > 0; i--)
                {
                    last = cordChange1(last.x, last.y);
                    affected.Add(last);
                }

                foreach (var loc in affected)
                    matches[loc.Item1, loc.Item2] = total;
            }

            for (var x = xStart; x < xEnd; x++)
            {
                for (var y = yStart; y < yEnd; y++)
                {
                    // check Horizontal
                    if (matchesHor[x, y] == 0)
                        Check(x, y, leftMove, rightMove, matchesHor);

                    // check vertical
                    if (matchesVert[x, y] == 0)
                        Check(x, y, topMove, botMove, matchesVert);
                }
            }

            var matches = new List<Match3Field.Match>();
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var hor = matchesHor[x, y];
                    var vert = matchesVert[x, y];

                    if (hor > 0)
                    {
                        for (var x0 = x; x0 < x + hor; x0++)
                            matchesHor[x0, y] = 0;
                        if (hor >= MatchMin)
                            matches.Add(new Match3Field.Match { x = x, y = y, right = hor });
                    }

                    if (vert > 0)
                    {
                        for (int y0 = y; y0 < y + vert; y0++)
                            matchesVert[x, y0] = 0;

                        if (vert >= MatchMin)
                            matches.Add(new Match3Field.Match { x = x, y = y, bottom = vert });
                    }
                }
            }
            return matches;
        }

        public List<Match3Field.Match> FullCheck(Match3Token[,] field)
        {
            var horizontal = new int[field.GetLength(0), field.GetLength(1)];
            var vertical = new int[field.GetLength(0), field.GetLength(1)];

            Func<int, int, (int, int)> vertFunc = (x, y) => (x, y - 1);
            Func<int, int, (int, int)> horizFunc = (x, y) => (x - 1, y);

            int[,] CheckField(int[,] countsField, Func<int, int, (int, int)> func)
            {
                for (var x = field.GetLength(0) - 1; x >= 0; x--)
                {
                    for (var y = field.GetLength(1) - 1; y >= 0; y--)
                    {
                        var (prevX, prevY) = func(x, y);
                        if (prevX < 0 || prevY < 0)
                            continue;

                        var curr = field[x, y];
                        var prev = field[prevX, prevY];

                        if (CanMatch(curr, prev))
                            countsField[prevX, prevY] = countsField[x, y] + 1;
                    }
                }

                return countsField;
            }

            horizontal = CheckField(horizontal, horizFunc);
            vertical = CheckField(vertical, vertFunc);

            var matches = new List<Match3Field.Match>();
            for (var y = 0; y < field.GetLength(1); y++)
            {
                for (var x = 0; x < field.GetLength(0); x++)
                {
                    var h = horizontal[x, y] + 1;
                    if (h >= MatchMin)
                        matches.Add(new Match3Field.Match()
                        {
                            x = x,
                            y = y,
                            right = h,
                        });
                    x += h - 1;
                }
            }

            for (var x = 0; x < field.GetLength(0); x++)
            {
                for (var y = 0; y < field.GetLength(1); y++)
                {
                    var v = vertical[x, y] + 1;
                    if (v >= MatchMin)
                        matches.Add(new Match3Field.Match()
                        {
                            x = x,
                            y = y,
                            bottom = v,
                        });
                    y += v - 1;
                }
            }
            return matches;
        }
    }

    public class BasicMatcher : Match3Matcher
    {
        public override bool CanMatch(Match3Token t1, Match3Token t2)
        {
            if (t1 == Match3Token.Empty || t2 == Match3Token.Empty)
                return false;
            if (t1 == t2) 
                return true;
            return false;
        }
    }
}