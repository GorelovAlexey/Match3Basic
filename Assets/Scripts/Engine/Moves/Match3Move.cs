using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.Visualizer;
using Assets.Scripts.Visualizer.VisualCommands;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

namespace Assets.Scripts.Engine.Moves
{
    public abstract class Match3Move
    {
        public abstract Match3Token[,] ApplyMove(Match3Token[,] field, out FieldVisualCommand command);
        public abstract Match3Token[,] RevertMove(Match3Token[,] field, out FieldVisualCommand command);
        public abstract (bool valid, bool possible) IsValidAndPossible(Match3Matcher matcher, Match3Token[,] field);

        public static string Serialize(List<Match3Move> moves)
        {
            //return JsonConvert.SerializeObject(moves, new Match3MovesJsonConverter());
            return JsonConvert.SerializeObject(moves, serializerSettings);
        }

        public static  List<Match3Move> Deserialize(string json)
        {
            //var converter = new Match3MovesJsonConverter();

            return JsonConvert.DeserializeObject<List<Match3Move>>(json, serializerSettings);
        }

        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings 
            { TypeNameHandling = TypeNameHandling.Auto, TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple };

        public static Dictionary<string, Type> Dictionary = new Dictionary<string, Type>()
        {
            {nameof(Match3MoveBasicSwap), typeof(Match3MoveBasicSwap) },
            {nameof(Match3MoveShuffle), typeof(Match3MoveShuffle)}
        };
    }


    public class Match3MovesJsonConverter : JsonConverter <Match3Move>
    {
        public override void WriteJson(JsonWriter writer, Match3Move? value, JsonSerializer serializer)
        {
            var token = JToken.FromObject(value);
            var ser = JsonConvert.SerializeObject(value);
            var obj = JObject.FromObject(value);
            var type = value.GetType();
            obj.Add("_type", type.Name);
            var str = obj.ToString();
            writer.WriteValue(obj.ToString());
        }

        public override Match3Move? ReadJson(JsonReader reader, Type objectType, Match3Move? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeName = obj["_type"].ToString();

            var type = Match3Move.Dictionary[typeName];

            var move = (Match3Move)JsonConvert.DeserializeObject(reader.ToString(), type);

            return move;
        }
    }
}