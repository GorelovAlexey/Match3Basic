using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using Assets.Scripts.Engine.Player;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniRx;

namespace Assets.Scripts.Engine
{
    [Serializable]
    public class Match3Replay
    {
        public Match3Token[,] initialField;
        public Match3GameSettings settings;
        public Match3PlayerData[] players;

        // Для данных ниже нужно сохранять тип класса чтобы можно было правильно загрузить
        [JsonProperty("Turns")] 
        private JToken[] turnsJTokens;
        [JsonIgnore]
        public List<Match3ReplayTurn> turns = new List<Match3ReplayTurn>();
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Match3Replay Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Match3Replay>(json);
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            turnsJTokens = turns.Select(x => x.Serialize()).ToArray();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (turnsJTokens == null)
                return;

            turns = new List<Match3ReplayTurn>();
            foreach (var turnsJToken in turnsJTokens)
            {
                var turn = new Match3ReplayTurn();
                turn.LoadData(turnsJToken);
                turns.Add(turn);
            }
        }

        public void Save(string path)
        {
            File.WriteAllText(path, Serialize());
        }

        public static Match3Replay Load(string path)
        {
            var json = File.ReadAllText(path);
            return Match3Replay.Deserialize(json);
        }
    }
}