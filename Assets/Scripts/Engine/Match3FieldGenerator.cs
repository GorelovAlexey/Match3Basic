using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Engine
{
    public class Match3FieldGenerator
    {
        private Match3TokenGenerator gen;
        private Random rnd;
        private int MinMoves => 3;
        private Match3Matcher matcher;

        public Match3FieldGenerator(Match3Matcher matcher, int seed)
        {
            gen = new Match3TokenGenerator(seed);
            rnd = new Random(seed);
            SetMatcher(matcher);
        }

        public void SetMatcher(Match3Matcher matcher)
        {
            this.matcher = matcher;
        }

        public Match3Token GetToken()
        {
            return gen.GetToken();
        }

        public Match3Token[,] GetField(int w, int h)
        {
            var possible = gen.GetGenerated;

            var res = new Match3Token[w, h];

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var p = GetPossible(x, y);

                    if (p.Count == 0) 
                        throw new Exception("Generation error = not enough tokenTypes, need a better generation algorithm");

                    res[x, y] = p[rnd.Next(p.Count)];
                }
            }

            List<Match3Token> GetPossible(int x, int y)
            {
                var need = matcher.MatchMin - 1;
                if (need <= 0)
                    return possible;

                var foundTokens = new HashSet<Match3Token>();

                // получем все токены в ряду которые можно сматчить
                // если токенов не достаточно для ряда (например нужно 5, а в ряду обнаружено 3) - пустое множество
                // на вход - шаги, как получить из шага X,Y
                HashSet<Match3Token> GetMatchedTokens(int steps, Func<int, (int, int)> stepToCoordinates)
                {
                    HashSet<Match3Token> foundTokens = new HashSet<Match3Token>();
                    Match3Token? last = null;
                    for (var i = 0; i < steps; i++)
                    {
                        var (x, y) = stepToCoordinates(i);

                        if (x < 0 || y < 0)
                            return new HashSet<Match3Token>();

                        if (last.HasValue && !matcher.CanMatch(last.Value, res[x, y]))
                            return new HashSet<Match3Token>();

                        last = res[x, y];
                        if (!foundTokens.Contains(res[x, y]))
                            foundTokens.Add(res[x, y]);
                    }

                    return foundTokens;
                }


                (int, int) HorizontalStep(int s) => (x - s - 1, y);
                (int, int) VerticalStep(int s) => (x , y - s - 1);

                if (y >= need)
                    foundTokens.UnionWith(GetMatchedTokens(need, VerticalStep));

                if (x >= need)
                    foundTokens.UnionWith(GetMatchedTokens(need, HorizontalStep));

                if (foundTokens.Count == 0)
                    return possible;

                return possible.Where(t => !foundTokens.Any(avoid => matcher.CanMatch(t, avoid))).ToList();
            }

            var moves = matcher.FindMoves(res);

            if (moves.Count < MinMoves)
                res = AddMoves(MinMoves - moves.Count);

            Match3Token[,] GetFieldCopy()
            {
                var ret = new Match3Token[res.GetLength(0), res.GetLength(1)];
                for (int x = 0; x < res.GetLength(0); x++)
                for (int y = 0; y < res.GetLength(1); y++)
                    ret[x, y] = res[x, y];
                return ret;
            }

            // Тупой перебор
            // если добалвение токена не составляет Х в ряд и если на соседней клетке появляется возможный ход
            // помещаем токен в клетку создавая доп ход
            Match3Token[,] AddMoves(int needMoves)
            {
                //return res;
                if (needMoves <= 0)
                    return res;

                var res2 = GetFieldCopy();
                var possibleTokens = gen.GetGenerated;

                // Чтобы новые ходы появлялись не с одного края - перебираем X Y в рандомном порядке
                var xPos = new List<int>();
                for (int x = 0; x < res2.GetLength(0); x++)
                    xPos.Add(x);
                var yPos = new List<int>();
                for (int y = 0; y < res2.GetLength(1); y++)
                    yPos.Add(y);

                foreach (var x in xPos.OrderBy(_ => rnd.Next()))
                {
                    foreach (var y in yPos.OrderBy(_ => rnd.Next()))
                    {
                        bool CloseValues(int value, params int[] values)
                        {
                            foreach (var v in values)
                            {
                                var dist = value - v;
                                if (dist > 1 || dist < -1)
                                    return false;
                            }
                            return true;
                        }

                        if (moves.Any(m => CloseValues(x, m.x, m.x1) || CloseValues(y, m.y, m.y1)))
                            continue;

                        foreach (var token in possibleTokens)
                        {
                            if (matcher.MatchExists(res2, x, y, token) || token == res2[x, y])
                                continue;

                            if (matcher.MatchExists(res2, x - 1, y, token) ||
                                matcher.MatchExists(res2, x + 1, y, token) ||
                                matcher.MatchExists(res2, x, y - 1, token) ||
                                matcher.MatchExists(res2, x, y + 1, token))
                            {
                                res2[x, y] = token;
                                needMoves--;
                                if (needMoves == 0)
                                    return res2;
                            }
                        }
                    }
                }

                return res2;
            }
            return res;
        }

        // **x
        // xx*
    }
}