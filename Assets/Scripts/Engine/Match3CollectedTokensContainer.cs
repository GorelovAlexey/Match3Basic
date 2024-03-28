using System.Collections.Generic;
using Assets.Scripts.Engine.Player;

namespace Assets.Scripts.Engine
{
    public class Match3CollectedTokensContainer
    {
        public Dictionary<Match3Token, float> CollectedCount = new Dictionary<Match3Token, float>();
        public List<int> MatchesCount = new List<int>();

        public void AddValue((Dictionary<Match3Token, float>, int[]) value)
        {
            foreach (var c in value.Item1)
            {
                if (!CollectedCount.ContainsKey(c.Key))
                    CollectedCount.Add(c.Key, c.Value);
                else
                    CollectedCount[c.Key] += c.Value;
            }

            foreach (var c in value.Item2)
                MatchesCount.Add(c);
        }
    }
}