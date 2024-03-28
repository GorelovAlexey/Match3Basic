using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Engine.Player
{
    public class Match3PlayerData
    {
        public string Name { get; set; }
        public string MoveMaker { get; set; }

        [JsonIgnore] public static List<Type> moveMakers = new List<Type> {typeof(HumanMoveMaker), typeof(BasicAIMoveMaker)};
        [JsonIgnore] private static Dictionary<string, Type> moveMakersDictionary;

        [JsonIgnore]
        private static Dictionary<string, Type> MoveMakersDictionary
        {
            get
            {
                if (moveMakersDictionary == null)
                {
                    moveMakersDictionary = new Dictionary<string, Type>();
                    foreach (var moveMaker in moveMakers)
                        moveMakersDictionary.Add(moveMaker.Name, moveMaker);
                }

                return moveMakersDictionary;
            }
        }

        public static Match3PlayerMoveMaker CreateMoveMaker(Match3PlayerData data)
        {
            var type = MoveMakersDictionary[data.MoveMaker];
            return (Match3PlayerMoveMaker)Activator.CreateInstance(type);
        }

        public static Match3Player Create(Match3PlayerData data)
        {
            return new Match3Player(data.Name, CreateMoveMaker(data));
        }

        public Match3Player Create()
        {
            return Create(this);
        }

        public static Match3PlayerData Save(Match3Player p)
        {
            return new Match3PlayerData()
            {
                Name = p.Name,
                MoveMaker = p.MoveMaker.GetType().Name
            };
        }
    }
}