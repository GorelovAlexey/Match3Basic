using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Engine
{
    public class Match3TokenGenerator
    {
        private Random rnd;
        public Match3TokenGenerator(int seed)
        {
            rnd = new Random(seed);
        }

        private List<Match3Token> generated = new List<Match3Token> { Match3Token.Red, Match3Token.Blue, Match3Token.Green, Match3Token.Yellow };
        public List<Match3Token> GetGenerated => generated.ToList();
        public Match3Token GetToken()
        {
            var i = rnd.Next(generated.Count);
            return generated[i];
        }

        public Match3Token[] GetTokens(int count)
        {
            var res = new Match3Token[count];
            for (int j = 0; j < count; j++)
            {
                var i = rnd.Next(generated.Count);
                res[j] = generated[i];
            }

            return res;
        }
    }
}