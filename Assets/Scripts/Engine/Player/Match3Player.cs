using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using RSG;
using UniRx;

namespace Assets.Scripts.Engine.Player
{
    public class Match3Player
    {
        // Ёто данные дл€ возможного сохранени€ в процессе. 
        private Dictionary<Match3Token, float> collected = new Dictionary<Match3Token, float>();

        // ƒанные дл€ отображени€ в анимации
        private Dictionary<Match3Token, FloatReactiveProperty> visualCount =
            new Dictionary<Match3Token, FloatReactiveProperty>();

        public FloatReactiveProperty GetDisplayCount(Match3Token t) => visualCount.ContainsKey(t) ? 
            visualCount[t] : visualCount[t] = new FloatReactiveProperty(0);

        public Match3PlayerMoveMaker MoveMaker { get; set; }


        public float GetCount(Match3Token t) => collected.ContainsKey(t) ? collected[t] : 0;

        private void Add(Match3Token t, float real, float visual)
        {
            if (!collected.ContainsKey(t))
                collected[t] = real;
            else 
                collected[t] += real;

            GetDisplayCount(t).Value += visual;
        }

        public void Collect(Match3Token token, float amount, float visual)
        {
            Add(token, amount, visual);
        }

        public void Collect(IEnumerable<(Match3Token, float)> collectedTokens, bool real, bool visual)
        {
            foreach (var (token, amount) in collectedTokens)
                Add(token, real ? amount : 0, visual ? amount : 0);
        }

        public string Name { get; private set; }

        public Match3Player(string name, Match3PlayerMoveMaker moveMaker)
        {
            Name = name;
            MoveMaker = moveMaker;
        }

        public IPromise<Match3CommandMove> GetAMove(Match3Game game)
        {
            return MoveMaker.GetMovePromise(game);
        }

        public bool CanMakeAMove => MoveMaker.CanMakeMove;
    }
}