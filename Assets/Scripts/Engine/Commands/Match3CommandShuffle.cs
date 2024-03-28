using Assets.Scripts.UI.Utils;
using Assets.Scripts.Visualizer.VisualCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Engine.Commands
{
    public class Match3CommandShuffle : Match3GameCommand, ISavableCommand
    {
        public new const string TYPE_NAME = "shuffle";
        
        [JsonProperty]
        private Match3Token[,] generated;


        public Match3CommandShuffle(Match3FieldGenerator generator, int width, int height)
        {
            // для десериализации
            if (generator != null)
                generated = generator.GetField(width, height);
        }

        public override void Apply(Match3Game game)
        {
            game.Field.field = generated.Clone() as Match3Token[,];
        }

        public override FieldVisualCommand GetVisuals(Match3Game game)
        {
            var width = generated.GetLength(0);
            var height = generated.GetLength(1);

            var mask = new bool[width, height].SetEach((_, __, ___) => true);

            var hide = new FieldVisualHide(mask);
            var spawn = new FieldVisualSpawn(generated, game.TokensSpawnDirection);
            var pause = new FieldVisualPause(.25f); // TODO CHANGE

            var visuals = new FieldVisualCommandSequence(hide, pause, spawn);
            return visuals;
        }

        public string GetTypeName => TYPE_NAME;
        public JToken Serialize()
        {
            var jObj = JObject.FromObject(this);
            jObj[TYPE_TOKEN_NAME] = TYPE_NAME;

            return jObj;
        }
        public void LoadData(JToken obj)
        {
            var shuffle = obj.ToObject<Match3CommandShuffle>();
            this.generated = shuffle.generated;
        }
    }
}